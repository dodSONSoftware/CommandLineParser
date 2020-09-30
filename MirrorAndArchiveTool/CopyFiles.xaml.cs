using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for CopyFiles.xaml
    /// </summary>
    public partial class CopyFiles : Window
    {
        #region Ctor
        public CopyFiles()
        {
            InitializeComponent();
            DataContext = this;
        }
        public CopyFiles(string windowTitle,
                         string startMessage,
                         string endMessage,
                         IEnumerable<FileCopyInfo> copyData)
            : this()
        {
            Title = Helper.FormatTitle(windowTitle);
            _StartText = startMessage;
            _EndText = endMessage;
            HeaderText = startMessage;
            _CopyData = copyData ?? throw new ArgumentNullException(nameof(copyData));
        }
        #endregion
        #region Private Fields
        private readonly IEnumerable<FileCopyInfo> _CopyData;
        private bool _IsProcessing = true;
        private readonly string _StartText;
        private readonly string _EndText;
        //
        private readonly System.Threading.CancellationTokenSource _CancellationToken_ProcessTask = new System.Threading.CancellationTokenSource();
        private System.Threading.Tasks.Task _ProcessTask = null;
        #endregion
        #region Dependency Properties
        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(CopyFiles), new PropertyMetadata("HeaderText"));
        // ----
        public double ProgressBarValue
        {
            get { return (double)GetValue(ProgressBarValueProperty); }
            set { SetValue(ProgressBarValueProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarValueProperty =
            DependencyProperty.Register("ProgressBarValue", typeof(double), typeof(CopyFiles), new UIPropertyMetadata(0.0));
        public bool ProgressBarIsIndeterminate
        {
            get { return (bool)GetValue(ProgressBarIsIndeterminateProperty); }
            set { SetValue(ProgressBarIsIndeterminateProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarIsIndeterminateProperty =
            DependencyProperty.Register("ProgressBarIsIndeterminate", typeof(bool), typeof(CopyFiles), new UIPropertyMetadata(false));
        // ----
        public string ProcessingJobProgressText1
        {
            get { return (string)GetValue(ProcessingJobProgressText1Property); }
            set { SetValue(ProcessingJobProgressText1Property, value); }
        }
        public static readonly DependencyProperty ProcessingJobProgressText1Property =
            DependencyProperty.Register("ProcessingJobProgressText1", typeof(string), typeof(CopyFiles), new UIPropertyMetadata(""));
        public string ProcessingJobProgressText2
        {
            get { return (string)GetValue(ProcessingJobProgressText2Property); }
            set { SetValue(ProcessingJobProgressText2Property, value); }
        }
        public static readonly DependencyProperty ProcessingJobProgressText2Property =
            DependencyProperty.Register("ProcessingJobProgressText2", typeof(string), typeof(CopyFiles), new UIPropertyMetadata(""));
        public string CloseButtonText
        {
            get { return (string)GetValue(CloseButtonTextProperty); }
            set { SetValue(CloseButtonTextProperty, value); }
        }
        public static readonly DependencyProperty CloseButtonTextProperty =
            DependencyProperty.Register("CloseButtonText", typeof(string), typeof(CopyFiles), new PropertyMetadata("Cancel"));
        #endregion
        #region Windows Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBarIsIndeterminate = false;
            ProgressBarValue = 0;
            Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.Normal);
            ProcessingJobProgressText1 = "Initializing...";

            // start the process task
            _ProcessTask = System.Threading.Tasks.Task.Run(() =>
            {
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

                // process 
                var index = 1.0;
                var copyDataTotal = _CopyData.Count();
                foreach (var item in _CopyData)
                {
                    if (_CancellationToken_ProcessTask.IsCancellationRequested) { break; }
                    Dispatcher.Invoke(() =>
                    {
                        var fInfo = new System.IO.FileInfo(item.SourceFilename);
                        ProcessingJobProgressText1 = $"{System.IO.Path.GetFileName(item.SourceFilename)}, {dodSON.Core.Common.ByteCountHelper.ToString(fInfo.Length)}   ({fInfo.Length:N0})";
                        ProgressBarValue = (double)(index++ / (double)copyDataTotal);
                        Helper.MainWindow.SetGlobalPercent(ProgressBarValue, System.Windows.Shell.TaskbarItemProgressState.Normal);
                        dodSON.Core.Threading.ThreadingHelper.Sleep(10);
                    });
                    System.IO.File.Copy(item.SourceFilename, item.DestinationFilename, true);
                }

                // update UI
                Dispatcher.Invoke(() =>
                {
                    ProgressBarValue = 1.0;
                    if (_CancellationToken_ProcessTask.IsCancellationRequested) { Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.None); }
                    else { Helper.MainWindow.SetGlobalPercent(1, System.Windows.Shell.TaskbarItemProgressState.Paused); }
                    HeaderText = _EndText;
                    ProcessingJobProgressText1 = "Task Complete";
                });

                // cancel inner task
                innerTaskCancellationTokenSource.Cancel();
                innerTask.Wait(100);

                _IsProcessing = false;

                // update UI
                Dispatcher.Invoke(() => { CloseButtonText = "Close"; });
            });
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_IsProcessing)
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

    public class FileCopyInfo
    {
        public string SourceFilename { get; set; }
        public string DestinationFilename { get; set; }
    }
}
