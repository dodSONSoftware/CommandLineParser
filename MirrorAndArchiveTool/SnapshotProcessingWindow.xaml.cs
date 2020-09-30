using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MirrorAndArchiveTool
{
    /// <summary>
    /// Interaction logic for SnapshotProcessingWindow.xaml
    /// </summary>
    public partial class SnapshotProcessingWindow
        : Window
    {
        #region Ctor
        public SnapshotProcessingWindow()
        {
            InitializeComponent();
            DataContext = this;
            Title = Helper.FormatTitle("Processing Snapshots");
        }
        public SnapshotProcessingWindow(ArchiveJob job,
                                        GlobalSettings settings,
                                        DateTime timeStamp,
                                        bool forceCreation)
            : this()
        {
            _Job = job ?? throw new ArgumentNullException("job");
            _Settings = settings ?? throw new ArgumentNullException("settings");
            _TimeStamp = timeStamp;
            _ForceCreation = forceCreation;
        }
        #endregion
        #region Private Fields
        private readonly ArchiveJob _Job = null;
        private readonly GlobalSettings _Settings = null;
        private readonly DateTime _TimeStamp = DateTime.MinValue;
        private readonly bool _ForceCreation = true;
        //
        private readonly System.Threading.CancellationTokenSource _CancellationToken_ProcessTask = new System.Threading.CancellationTokenSource();
        private System.Threading.Tasks.Task _ProcessTask = null;
        #endregion
        #region Dependency Properties
        public double ProgressBarValue
        {
            get { return (double)GetValue(ProgressBarValueProperty); }
            set { SetValue(ProgressBarValueProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarValueProperty =
            DependencyProperty.Register("ProgressBarValue", typeof(double), typeof(SnapshotProcessingWindow), new UIPropertyMetadata(0.0));
        // ----
        public bool ProgressBarIsIndeterminate
        {
            get { return (bool)GetValue(ProgressBarIsIndeterminateProperty); }
            set { SetValue(ProgressBarIsIndeterminateProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarIsIndeterminateProperty =
            DependencyProperty.Register("ProgressBarIsIndeterminate", typeof(bool), typeof(SnapshotProcessingWindow), new UIPropertyMetadata(false));
        // ----
        public Visibility ProgressBarVisibility
        {
            get { return (Visibility)GetValue(ProgressBarVisibilityProperty); }
            set { SetValue(ProgressBarVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarVisibilityProperty =
            DependencyProperty.Register("ProgressBarVisibility", typeof(Visibility), typeof(SnapshotProcessingWindow), new UIPropertyMetadata(Visibility.Visible));
        // ----
        public string ProcessingJobProgressText1
        {
            get { return (string)GetValue(ProcessingJobProgressText1Property); }
            set { SetValue(ProcessingJobProgressText1Property, value); }
        }
        public static readonly DependencyProperty ProcessingJobProgressText1Property =
            DependencyProperty.Register("ProcessingJobProgressText1", typeof(string), typeof(SnapshotProcessingWindow), new UIPropertyMetadata(""));
        // ----
        public string ProcessingJobProgressText2
        {
            get { return (string)GetValue(ProcessingJobProgressText2Property); }
            set { SetValue(ProcessingJobProgressText2Property, value); }
        }
        public static readonly DependencyProperty ProcessingJobProgressText2Property =
            DependencyProperty.Register("ProcessingJobProgressText2", typeof(string), typeof(SnapshotProcessingWindow), new UIPropertyMetadata(""));
        public bool IsProcessing
        {
            get { return (bool)GetValue(IsProcessingProperty); }
            set { SetValue(IsProcessingProperty, value); }
        }
        public static readonly DependencyProperty IsProcessingProperty =
            DependencyProperty.Register("IsProcessing", typeof(bool), typeof(SnapshotProcessingWindow), new PropertyMetadata(true));
        public string CloseButtonText
        {
            get { return (string)GetValue(CloseButtonTextProperty); }
            set { SetValue(CloseButtonTextProperty, value); }
        }
        public static readonly DependencyProperty CloseButtonTextProperty =
            DependencyProperty.Register("CloseButtonText", typeof(string), typeof(SnapshotProcessingWindow), new PropertyMetadata("Cancel"));
        #endregion
        #region Windows Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBarIsIndeterminate = false;
            ProgressBarValue = 0;
            Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.Normal);
            ProcessingJobProgressText1 = "";

            // start the process task
            _ProcessTask = System.Threading.Tasks.Task.Run(() =>
            {
                Dispatcher.Invoke(() => { IsProcessing = true; });

                // start the on-form clock update task
                var innerTaskCancellationTokenSource = new System.Threading.CancellationTokenSource();
                var innerTask = System.Threading.Tasks.Task.Run(() =>
                                                                {
                                                                    var stWatch = System.Diagnostics.Stopwatch.StartNew();
                                                                    while (!innerTaskCancellationTokenSource.IsCancellationRequested)
                                                                    {
                                                                        Dispatcher.Invoke(() => { ProcessingJobProgressText2 = stWatch.Elapsed.ToString("mm\\:ss\\.f"); });
                                                                        dodSON.Core.Threading.ThreadingHelper.Sleep(50);
                                                                    }
                                                                });
                // process snapshots
                Helper.ProcessSnapshots(_Job, _Settings, _TimeStamp, _ForceCreation,
                                        _CancellationToken_ProcessTask.Token,
                                        (x, y) =>
                                        {
                                            // update UI
                                            Dispatcher.Invoke(() =>
                                                              {
                                                                  ProgressBarValue = x;
                                                                  ProcessingJobProgressText1 = y;
                                                                  Helper.MainWindow.SetGlobalPercent(ProgressBarValue, System.Windows.Shell.TaskbarItemProgressState.Normal);
                                                              });
                                        });
                // update UI
                Dispatcher.Invoke(() =>
                {
                    ProgressBarValue = 1.0;
                    ProcessingJobProgressText1 = "Task Complete";
                    if (_CancellationToken_ProcessTask.IsCancellationRequested) { Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.None); }
                    else { Helper.MainWindow.SetGlobalPercent(1, System.Windows.Shell.TaskbarItemProgressState.Paused); }
                });

                // cancel inner task
                innerTaskCancellationTokenSource.Cancel();
                innerTask.Wait(100);

                // update UI
                Dispatcher.Invoke(() =>
                {
                    IsProcessing = false;
                    CloseButtonText = "Close";
                });
            });
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsProcessing)
            {
                _CancellationToken_ProcessTask.Cancel();
            }
            Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.None);
        }
        #endregion
        #region Commands
        private DelegateCommand _CloseCommand = null;
        public DelegateCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                {
                    _CloseCommand = new DelegateCommand((x) => { Close(); });
                }
                return _CloseCommand;
            }
        }
        #endregion
    }
}
