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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MirrorAndArchiveTool
{
    /// <summary>
    /// Interaction logic for JobDetails.xaml
    /// </summary>
    public partial class JobDetails
        : UserControl
    {
        #region Ctor
        public JobDetails()
        {
            InitializeComponent();
            DataContext = this;
            FileChangesPercentages = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>();
            Job = null;
        }
        public JobDetails(JobBase job,
                          IEnumerable<string> jobNames,
                          Action<JobBase> okAction,
                          Action<JobBase> deleteAction,
                          Action<JobBase> enabledAction,
                          GlobalSettings settings,
                          Action onShutdown)
            : this()
        {
            if (job == null) { throw new ArgumentNullException("job"); }
            if (settings == null) { throw new ArgumentNullException("settings"); }
            if (onShutdown == null) { throw new ArgumentNullException("onShutdown"); }
            _Job = job;
            _JobNames = jobNames;
            _EditOkAction = okAction;
            _DeleteAction = deleteAction;
            _EnabledAction = enabledAction;
            _Settings = settings;
            Job = job;
            _OnShutdown = onShutdown;
            PopulateJobDates();
            PopulateJobDetails();
            InitializeRecommendationActionUI();
            UpdateToolbarButtons();
            _AutoUpdateUIThreadWorker = new AutoUpdateUIThreadWorker(this);
            StartProcessingPieChart();
        }
        #endregion
        #region Private Fields
        private DateTime _StartedUserControl = DateTime.Now;
        private JobBase _Job = null;
        private IEnumerable<string> _JobNames = null;
        private Action<JobBase> _EditOkAction = null;
        private Action<JobBase> _DeleteAction = null;
        private Action<JobBase> _EnabledAction = null;
        private GlobalSettings _Settings = null;
        private Action _OnShutdown = null;
        private IEnumerable<dodSON.BuildingBlocks.FileStorage.ICompareResult> _Report = null;
        private AutoUpdateUIThreadWorker _AutoUpdateUIThreadWorker = null;
        private bool __IsProcessingPieChart__ = true;
        private System.Threading.CancellationTokenSource _CancelTokenSource = new System.Threading.CancellationTokenSource();
        private List<System.Threading.CancellationTokenSource> _Tasks = new List<System.Threading.CancellationTokenSource>();
        private int _Total = -1;
        private int _TotalChanges = -1;
        private int _OkCount = -1;
        private int _NewCount = -1;
        private int _UpdateCount = -1;
        private int _RemoveCount = -1;
        #endregion
        #region Dependency Properties
        public System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>> FileChangesPercentages
        {
            get { return (System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>)GetValue(FileChangesPercentagesProperty); }
            set { SetValue(FileChangesPercentagesProperty, value); }
        }
        public static readonly DependencyProperty FileChangesPercentagesProperty =
            DependencyProperty.Register("FileChangesPercentages",
                                        typeof(System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>),
                                        typeof(JobDetails));
        // ----
        public JobBase Job
        {
            get { return (JobBase)GetValue(JobProperty); }
            set { SetValue(JobProperty, value); }
        }
        public static readonly DependencyProperty JobProperty =
            DependencyProperty.Register("JobBase", typeof(JobBase), typeof(JobDetails));
        // ----
        public string RecommendedActionImage
        {
            get { return (string)GetValue(RecommendedActionImageProperty); }
            set { SetValue(RecommendedActionImageProperty, value); }
        }
        public static readonly DependencyProperty RecommendedActionImageProperty =
            DependencyProperty.Register(
                    "RecommendedActionImage",
                    typeof(string), typeof(JobDetails));
        // ----
        public string RecommendedActionText
        {
            get { return (string)GetValue(RecommendedActionTextProperty); }
            set { SetValue(RecommendedActionTextProperty, value); }
        }
        public static readonly DependencyProperty RecommendedActionTextProperty =
            DependencyProperty.Register("RecommendedActionText", typeof(string), typeof(JobDetails), new UIPropertyMetadata(""));
        // ----
        public string RecommendedActionReasons
        {
            get { return (string)GetValue(RecommendedActionReasonsProperty); }
            set { SetValue(RecommendedActionReasonsProperty, value); }
        }
        public static readonly DependencyProperty RecommendedActionReasonsProperty =
            DependencyProperty.Register("RecommendedActionReasons", typeof(string), typeof(JobDetails), new UIPropertyMetadata(""));
        // ----
        public string IsEnabledButtonImage
        {
            get { return (string)GetValue(IsEnabledButtonImageProperty); }
            set { SetValue(IsEnabledButtonImageProperty, value); }
        }
        public static readonly DependencyProperty IsEnabledButtonImageProperty =
            DependencyProperty.Register("IsEnabledButtonImage", typeof(string), typeof(JobDetails), new UIPropertyMetadata(""));
        // ----
        public string RunJobButtonImage
        {
            get { return (string)GetValue(RunJobButtonImageProperty); }
            set { SetValue(RunJobButtonImageProperty, value); }
        }
        public static readonly DependencyProperty RunJobButtonImageProperty =
            DependencyProperty.Register("RunJobButtonImage", typeof(string), typeof(JobDetails), new UIPropertyMetadata(""));
        // ----
        public double ProgressBarPercentage
        {
            get { return (double)GetValue(ProgressBarPercentageProperty); }
            set { SetValue(ProgressBarPercentageProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarPercentageProperty =
            DependencyProperty.Register("ProgressBarPercentage", typeof(double), typeof(JobDetails), new UIPropertyMetadata(0.0));
        // ----
        public bool ProgressBarIsIndeterminate
        {
            get { return (bool)GetValue(ProgressBarIsIndeterminateProperty); }
            set { SetValue(ProgressBarIsIndeterminateProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarIsIndeterminateProperty =
            DependencyProperty.Register("ProgressBarIsIndeterminate", typeof(bool), typeof(JobDetails), new UIPropertyMetadata(true));
        // ----
        public string textBlockAnalyzingText
        {
            get { return (string)GetValue(textBlockAnalyzingTextProperty); }
            set { SetValue(textBlockAnalyzingTextProperty, value); }
        }
        public static readonly DependencyProperty textBlockAnalyzingTextProperty =
            DependencyProperty.Register("textBlockAnalyzingText", typeof(string), typeof(JobDetails), new UIPropertyMetadata(""));
        // ----
        public string textBlockAnalyzingText2
        {
            get { return (string)GetValue(textBlockAnalyzingText2Property); }
            set { SetValue(textBlockAnalyzingText2Property, value); }
        }
        public static readonly DependencyProperty textBlockAnalyzingText2Property =
            DependencyProperty.Register("textBlockAnalyzingText2", typeof(string), typeof(JobDetails), new UIPropertyMetadata(""));
        // ----
        public double RunJobButtonOpacity
        {
            get { return (double)GetValue(RunJobButtonOpacityProperty); }
            set { SetValue(RunJobButtonOpacityProperty, value); }
        }
        public static readonly DependencyProperty RunJobButtonOpacityProperty =
            DependencyProperty.Register("RunJobButtonOpacity", typeof(double), typeof(JobDetails), new UIPropertyMetadata(1.0));
        #endregion
        #region UserControl Events
        private void dataGridFilesDetails_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            //            dataGridFilesDetails.UnselectAll();
        }
        private void button_IsEnabled(object sender, RoutedEventArgs e)
        {
            _Job.IsEnabled = !_Job.IsEnabled;
            UpdateToolbarButtons();
            _EnabledAction(_Job);
        }
        private void buttonRunNow_Click(object sender, RoutedEventArgs e)
        {
            var wind = new RunWindow(_Settings, new List<Helper.RunPack>() { GetJobPack() }, false, _OnShutdown);
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
            if (_EditOkAction != null) { _EditOkAction(_Job); }
        }
        private void buttonReport_Click(object sender, RoutedEventArgs e)
        {
            var wind = new ReportWindow(_Job, _Report);
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
        }
        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            EditJob();
        }
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_DeleteAction != null)
            {
                if (MessageBox.Show(string.Format("Delete \"{1}\"{0}Are you sure?", Environment.NewLine, Job.Name),
                                    string.Format("Delete {0} Job", ((Job is ArchiveJob) ? "Archive" : "Mirror")),
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Hand) == MessageBoxResult.Yes)
                {
                    _DeleteAction(Job);
                }
            }
        }
        // ----
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Shutdown();
            // ----
            while (_Tasks.Count > 0)
            {
                var cancelled = new List<System.Threading.CancellationTokenSource>();
                foreach (var item in _Tasks)
                {
                    if (item.Token.CanBeCanceled && item.IsCancellationRequested)
                    {
                        item.Dispose();
                        cancelled.Add(item);
                    }
                    else
                    {
                        item.Cancel();
                    }
                }
                foreach (var item in cancelled) { _Tasks.Remove(item); }
            }
        }
        #endregion
        #region Public Methods
        public Helper.RunPack GetJobPack()
        {
            return new Helper.RunPack()
                    {
                        ComparisonReport = (_Settings.JobRunUseCachedReportEnableCache && ((DateTime.Now - _StartedUserControl) < _Settings.JobRunUseCachedReportTimeLimit))
                                                ? _Report
                                                : null,
                        Job = _Job
                    };
        }
        public void EditJob()
        {
            var wind = new EditorWindow(_Job, false, _JobNames);
            wind.Owner = Window.GetWindow(this);
            if (wind.ShowDialog() ?? false)
            {
                if (_EditOkAction != null) { _EditOkAction(wind.Job); }
            }
        }
        public void Shutdown()
        {
            if (_AutoUpdateUIThreadWorker != null)
            {
                try { _AutoUpdateUIThreadWorker.StopThread(TimeSpan.FromMilliseconds(333)); }
                catch { }
                _AutoUpdateUIThreadWorker = null;
            }
        }
        #endregion
        #region Private Methods
        private void UpdateToolbarButtons()
        {
            if (_Job.IsEnabled)
            {
                buttonIsEnabledText.Text = "Enabled";
                IsEnabledButtonImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Actions-dialog-ok-apply.ico";
            }
            else
            {
                buttonIsEnabledText.Text = "Disabled";
                IsEnabledButtonImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-dialog-error.ico";
            }
            // ----
            if (_Job is MirrorJob)
            {
                RunJobButtonImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/folders.ico";
            }
            else
            {
                RunJobButtonImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/decompress_48_h.ico";
            }
        }
        private void PopulateJobDates()
        {
            textBlockDateCreated.Text = string.Format("{0} {1}",
                                                        _Job.DateCreate,
                                                        "(" + dodSON.BuildingBlocks.Common.DateAndTime.FormatTimeSpan((DateTime.Now - _Job.DateCreate), true, " days ", true, true, false) + ")");
            if (_Job.DateLastRan == DateTime.MinValue)
            {
                textBlockDateLastRan.Text = "Job has never been run";
            }
            else
            {
                textBlockDateLastRan.Text = string.Format("{0} {1}",
                                                            _Job.DateLastRan,
                                                            "(" + dodSON.BuildingBlocks.Common.DateAndTime.FormatTimeSpan((DateTime.Now - _Job.DateLastRan), true, " days ", true, true, false) + ")");
            }
        }
        private void InitializeRecommendationActionUI()
        {
            RecommendedActionText = "Analyzing Job...";
            RecommendedActionImage = "";
            imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Visible;
        }
        private void PopulateRecommendedAction()
        {
            // **** update recommendation text and icon
            if (_TotalChanges == 0)
            {
                RecommendedActionText = "No action required";
                RecommendedActionImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-security-high.ico";
                imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                var timeAgo = (DateTime.Now - _Job.DateLastRan);
                if (_Job.DateLastRan == DateTime.MinValue) { timeAgo = (DateTime.Now - _Job.DateCreate); }
                if (timeAgo > _Settings.RecommendationActionAgeLimit)
                {
                    RecommendedActionText = "Run job";
                    RecommendedActionImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-security-low.ico";
                    imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    RecommendedActionText = "Consider running job";
                    RecommendedActionImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-security-medium.ico";
                    imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            // **** update recommendation reasons
            var recommendedReasonsText = new System.Text.StringBuilder(1024);
            // date reasons
            if (_Job.DateLastRan == DateTime.MinValue)
            {
                recommendedReasonsText.AppendLine("Job has never been run");
            }
            else
            {
                var agoString = dodSON.BuildingBlocks.Common.DateAndTime.FormatTimeSpan((DateTime.Now - _Job.DateLastRan), true, " days ", true, true, false);
                recommendedReasonsText.AppendLine(string.Format("Job has not been run for {0}", agoString));
            }
            // files changed reasons
            if (_TotalChanges > 0)
            {
                recommendedReasonsText.AppendLine(string.Format("{0}% changed", ((double)((double)_TotalChanges / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2")));
                //var percentChanged = (_Total == 0) ? 100.0 : changedCount / _Total;
            }
            RecommendedActionReasons = recommendedReasonsText.ToString();
        }
        private void PopulateJobDetails()
        {
            contentControlJobDetails.Content = null;
            contentControlJobCommonDetails.Content = null;
            if (_Job is MirrorJob)
            {
                contentControlJobDetails.Content = new MirrorJobDetails(_Job);
            }
            else if (_Job is ArchiveJob)
            {
                contentControlJobDetails.Content = new ArchiveJobDetails(_Job);
                contentControlJobCommonDetails.Content = new CommonJobDetails(_Job);
            }
        }
        private void StartProcessingPieChart()
        {
            //            System.Diagnostics.Debug.WriteLine("#################### StartProcessingPieChart()--> JOB ANALYSIS STARTED: Job Name=" + Job.Name);

            System.Threading.Tasks.Task.Factory.StartNew(
                () =>
                {
                    Dispatcher.Invoke(new Action(
                        () =>
                        {
                            dataGridFilesDetails.ItemsSource = null;
                            chart.Visibility = System.Windows.Visibility.Hidden;
                            borderAnalyzingJobActionStatus.Visibility = System.Windows.Visibility.Visible;
                            RunJobButtonOpacity = 0.2;
                            if (_AutoUpdateUIThreadWorker != null) { _AutoUpdateUIThreadWorker.StartThread(); }
                        }));

                    // calculate all statistics
                    System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource();
                    _Tasks.Add(tokenSource);
                    _Report = GenerateReport(tokenSource.Token);
                    if (_Report != null)
                    {
                        _OkCount = (from x in _Report
                                    where (x.ItemType == dodSON.BuildingBlocks.FileStorage.CompareType.File) &&
                                          (x.Action == dodSON.BuildingBlocks.FileStorage.CompareAction.Ok)
                                    select x).Count();
                        _NewCount = (from x in _Report
                                     where (x.ItemType == dodSON.BuildingBlocks.FileStorage.CompareType.File) &&
                                           (x.Action == dodSON.BuildingBlocks.FileStorage.CompareAction.New)
                                     select x).Count();
                        _UpdateCount = (from x in _Report
                                        where (x.ItemType == dodSON.BuildingBlocks.FileStorage.CompareType.File) &&
                                              (x.Action == dodSON.BuildingBlocks.FileStorage.CompareAction.Update)
                                        select x).Count();
                        _RemoveCount = (from x in _Report
                                        where (x.ItemType == dodSON.BuildingBlocks.FileStorage.CompareType.File) &&
                                              (x.Action == dodSON.BuildingBlocks.FileStorage.CompareAction.Remove)
                                        select x).Count();
                        _Total = _OkCount + _UpdateCount + _RemoveCount;
                        _TotalChanges = _NewCount + _UpdateCount + _RemoveCount;
                        // update UI
                        Dispatcher.Invoke(new Action(
                            () =>
                            {
                                System.Collections.ObjectModel.ObservableCollection<Helper.FilesDetailsRow> filesDetails = new System.Collections.ObjectModel.ObservableCollection<Helper.FilesDetailsRow>();
                                System.Collections.ObjectModel.Collection<ResourceDictionary> piePalette = new System.Collections.ObjectModel.Collection<ResourceDictionary>();
                                var slices = new List<KeyValuePair<string, int>>();
                                if (_Total > 0)
                                {
                                    if (_OkCount > 0)
                                    {
                                        slices.Add(new KeyValuePair<string, int>("Ok", (int)(_OkCount)));
                                        piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.SteelBlue));
                                        filesDetails.Add(new Helper.FilesDetailsRow() { Action = "Ok", Files = _OkCount.ToString("N0"), Percentage = ((double)((double)_OkCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                                    }
                                    if (_NewCount > 0)
                                    {
                                        slices.Add(new KeyValuePair<string, int>("New", (int)(_NewCount)));
                                        piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.ForestGreen));
                                        filesDetails.Add(new Helper.FilesDetailsRow() { Action = "New", Files = _NewCount.ToString("N0"), Percentage = ((double)((double)_NewCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                                    }
                                    if (_UpdateCount > 0)
                                    {
                                        slices.Add(new KeyValuePair<string, int>("Update", (int)(_UpdateCount)));
                                        piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.Gold));
                                        filesDetails.Add(new Helper.FilesDetailsRow() { Action = "Update", Files = _UpdateCount.ToString("N0"), Percentage = ((double)((double)_UpdateCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                                    }
                                    if (_RemoveCount > 0)
                                    {
                                        slices.Add(new KeyValuePair<string, int>("Remove", (int)(_RemoveCount)));
                                        piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.Crimson));
                                        filesDetails.Add(new Helper.FilesDetailsRow() { Action = "Remove", Files = _RemoveCount.ToString("N0"), Percentage = ((double)((double)_RemoveCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                                    }
                                }
                                else
                                {
                                    slices.Add(new KeyValuePair<string, int>("New", (int)(100)));
                                    piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.ForestGreen));
                                    filesDetails.Add(new Helper.FilesDetailsRow() { Action = "New", Files = _NewCount.ToString("N0"), Percentage = ((double)((double)_NewCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                                }
                                // update UI
                                filesDetails.Add(new Helper.FilesDetailsRow() { Action = string.Format("{0} Changes", ((_Job is MirrorJob) ? "Mirror" : "Archive")), Files = _TotalChanges.ToString("N0") + " files", Percentage = (((double)_TotalChanges / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "% changed" });
                                textBoxFilesTotal.Text = _Total.ToString("N0");
                                chart.Palette = piePalette;
                                FileChangesPercentages = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>(slices);
                                dataGridFilesDetails.ItemsSource = filesDetails;
                                PopulateJobDates();
                                PopulateRecommendedAction();
                                borderAnalyzingJobActionStatus.Visibility = System.Windows.Visibility.Hidden;
                                chart.Visibility = System.Windows.Visibility.Visible;
                                RunJobButtonOpacity = 1;

                                buttonRunNow.IsEnabled = true;
                                buttonReport.IsEnabled = true;
                            }));

                        //                        System.Diagnostics.Debug.WriteLine("#################### StartProcessingPieChart()--> JOB ANALYSIS COMPLETE: Job Name=" + _Job.Name);
                        __IsProcessingPieChart__ = false;
                    }
                });
        }
        private ResourceDictionary CreateResourceDictionaryForPieSliceColor(Color color)
        {
            var resourceDict = new ResourceDictionary();
            var style = new Style(typeof(Control));
            var brush = new SolidColorBrush(color);
            style.Setters.Add(new Setter() { Property = Control.BackgroundProperty, Value = brush });
            resourceDict.Add("DataPointStyle", style);
            return resourceDict;
        }
        //private void DisposeOfCancelToken(bool cancelToken)
        //{
        //    if (_CancelTokenSource != null)
        //    {
        //        if (cancelToken) { _CancelTokenSource.Cancel(); }
        //        _CancelTokenSource.Dispose();
        //        _CancelTokenSource = null;
        //    }
        //}
        private IEnumerable<dodSON.BuildingBlocks.FileStorage.ICompareResult> GenerateReport(System.Threading.CancellationToken token)
        {
            IEnumerable<dodSON.BuildingBlocks.FileStorage.ICompareResult> results = null;
            if (_Job is MirrorJob)
            {
                // **** MIRROR JOB
                results = dodSON.BuildingBlocks.FileStorage.FileStoreHelper.Compare(((MirrorJob)_Job).SourcePath,
                                                                                    ((MirrorJob)_Job).MirrorPath,
                                                                                    token,
                                                                                    (x) => { DispatchProgressBarState(x, ((MirrorJob)_Job).SourcePath, ""); });
            }
            else
            {
                results = new dodSON.BuildingBlocks.FileStorage.ICompareResult[0];
                // **** ARCHIVE JOB
                var job = (ArchiveJob)_Job;
                double total = job.SourcePaths.Count;
                double count = 0;
                var worker = new List<dodSON.BuildingBlocks.FileStorage.ICompareResult>();
                foreach (var sourcePath in job.SourcePaths)
                {
                    if (token.IsCancellationRequested) { return null; }
                    var filename = string.Format("{0}.{1}.Archive.zip", Helper.StripBadCharacters(job.Name), Helper.StripBadCharacters(Helper.FixPathString(sourcePath)));
                    var archiveStoreFilename = System.IO.Path.Combine((job).ArchiveRootPath, filename);
                    if (System.IO.Directory.Exists(sourcePath))
                    {
                        try
                        {
                            var archiveStore = new dodSON.BuildingBlocks.FileStorage.IonicZipStore.IonicZipFileStore(archiveStoreFilename, System.IO.Path.GetTempPath(), false);
                            var list = dodSON.BuildingBlocks.FileStorage.FileStoreHelper.Compare(sourcePath,
                                                                                                 archiveStore,
                                                                                                 token,
                                                                                                 (x) =>
                                                                                                 {
                                                                                                     DispatchProgressBarState((count / total) + (x / total), sourcePath, "");
                                                                                                 });
                            if (list != null) { worker.AddRange(list); }
                        }
                        catch { }
                    }
                    if (token.IsCancellationRequested) { return null; }
                    count++;
                }
                results = (IEnumerable<dodSON.BuildingBlocks.FileStorage.ICompareResult>)worker;
            }
            if (!token.IsCancellationRequested)
            {
                return from x in results
                       where x.ItemType == dodSON.BuildingBlocks.FileStorage.CompareType.File
                       select x;
            }
            return null;
        }
        private void DispatchProgressBarState(double value, string text1, string text2)
        {
            this.Dispatcher.Invoke(new Action(
                () =>
                {
                    textBlockAnalyzingText = text1;
                    textBlockAnalyzingText2 = text2;
                    if (value == -1)
                    {
                        ProgressBarIsIndeterminate = true;
                    }
                    else
                    {
                        ProgressBarIsIndeterminate = false;
                        ProgressBarPercentage = value;
                    }
                }));
        }
        #endregion
        #region Private Class: AutoUpdateUIThreadWorker
        private class AutoUpdateUIThreadWorker
            : dodSON.BuildingBlocks.Threading.ThreadBase
        {
            public AutoUpdateUIThreadWorker(JobDetails jobDetailsControl)
            {
                _JobDetailsControl = jobDetailsControl;
            }

            private JobDetails _JobDetailsControl = null;

            protected override TimeSpan ThreadWorkerExecutionInterval
            {
                get { return TimeSpan.FromSeconds(1); }
            }
            protected override void ThreadWorker(dodSON.BuildingBlocks.Threading.ThreadCancelToken cancelToken)
            {
                //                System.Diagnostics.Debug.WriteLine("#################### AutoUpdateUI+ThreadWorker()--> WORKING: " + _JobDetailsControl._Job.Name);
                if (!cancelToken.CancelThread)
                {
                    if (_JobDetailsControl != null)
                    {
                        if (_JobDetailsControl.Dispatcher != null)
                        {
                            _JobDetailsControl.Dispatcher.Invoke(new Action(() =>
                            {
                                _JobDetailsControl.PopulateJobDates();
                                if (!_JobDetailsControl.__IsProcessingPieChart__) { _JobDetailsControl.PopulateRecommendedAction(); }
                            }));
                        }
                    }
                }
            }
        }
        #endregion
    }
}
