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
    /// Interaction logic for JobDetailsUserControl.xaml
    /// </summary>
    public partial class JobDetailsUserControl : UserControl
    {
        #region Ctor
        private JobDetailsUserControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        public JobDetailsUserControl(JobBase job,
                                     GlobalSettings settings,
                                     IEnumerable<string> jobNames,
                                     Action<JobBase> editAction,
                                     Action<JobBase> deleteAction,
                                     Action<JobBase> enabledAction,
                                     Action<JobBase> prerefreshAction,
                                     Action<JobBase> refreshAction,
                                     Action<JobBase> afterRunAction,
                                     Action onShutdown)
            : this()
        {
            Job = job ?? throw new ArgumentNullException("job");
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _JobNames = jobNames ?? throw new ArgumentNullException(nameof(jobNames));
            _EditOkAction = editAction ?? throw new ArgumentNullException(nameof(editAction));
            _DeleteAction = deleteAction ?? throw new ArgumentNullException(nameof(deleteAction));
            _EnabledAction = enabledAction ?? throw new ArgumentNullException(nameof(enabledAction));
            _PrerefreshAction = prerefreshAction ?? throw new ArgumentNullException(nameof(prerefreshAction));
            _RefreshAction = refreshAction ?? throw new ArgumentNullException(nameof(refreshAction));
            _AfterRunAction = afterRunAction ?? throw new ArgumentNullException(nameof(afterRunAction));
            _OnShutdown = onShutdown ?? throw new ArgumentNullException(nameof(onShutdown));
            // ----
            UpdateToolbarButtons();
            //PopulateJobDates();
            InitializeRecommendationActionUI();
            // ----
            ProcessJob_BackgroundWorker(Job);
            // ---- start the thread worker
            ++App.ThreadWorkerCounter;
            var jobName = Job.Name;
            App.WriteDebugLog(nameof(JobDetailsUserControl), $"Starting Background Task for {nameof(JobDetailsUserControl)}:{jobName}");
            Helper.StartBackgroundTask(() =>
            {
                while (true)
                {
                    if (_ClockThreadCancelTokenSource.IsCancellationRequested)
                    {
                        App.WriteDebugLog(nameof(JobDetailsUserControl), $"Stopping Background Task for {nameof(JobDetailsUserControl)}:{jobName}");
                        break;
                    }
                    if (!_IsAnalyzingJob)
                    {
                        Dispatcher?.Invoke(new Action(() =>
                        {
                            PopulateRecommendedAction();
                        }));
                    }
                    dodSON.Core.Threading.ThreadingHelper.Sleep(1000);
                }
            });
            App.WriteDebugLog(nameof(JobDetailsUserControl), $"{Job.Name}: Job Badge ThreadWorker Started");
            string message;
            if (Job.DateLastRan == DateTime.MinValue)
            {
                message = $"{Job.Name} selected, Job has never been ran";
            }
            else
            {
                message = $"{Job.Name} selected, Date Last Ran={dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(DateTime.Now - Job.DateLastRan)} ({Job.DateLastRan:G})";
            }
            App.WriteLog(nameof(App), ((job is MirrorJob) ? App.LogCategory.Mirror : App.LogCategory.Archive), message);
        }
        #endregion
        #region Private Fields
        private IEnumerable<dodSON.Core.FileStorage.ICompareResult> _Report = null;
        private readonly DateTime _JobAnalyzedTimeStamp = DateTime.Now;
        private readonly IEnumerable<string> _JobNames = null;
        private readonly Action<JobBase> _EditOkAction = null;
        private readonly Action<JobBase> _DeleteAction = null;
        private readonly Action<JobBase> _EnabledAction = null;
        private readonly Action<JobBase> _PrerefreshAction = null;
        private readonly Action<JobBase> _RefreshAction = null;
        private readonly Action<JobBase> _AfterRunAction = null;
        private readonly GlobalSettings _Settings = null;
        private readonly Action _OnShutdown = null;
        // ----
        private bool _IsAnalyzingJob = false;
        private readonly System.Threading.CancellationTokenSource _CancelTokenSource = new System.Threading.CancellationTokenSource();
        private readonly System.Threading.CancellationTokenSource _ClockThreadCancelTokenSource = new System.Threading.CancellationTokenSource();
        // ----
        private int _Total = -1;
        private int _TotalChanges = -1;
        private int _OkCount = -1;
        private int _NewCount = -1;
        private int _UpdateCount = -1;
        private int _RemoveCount = -1;
        //
        //private int _UpdateCounter = 0;
        #endregion
        #region Dependency Properties
        public JobBase Job
        {
            get { return (JobBase)GetValue(JobProperty); }
            set { SetValue(JobProperty, value); }
        }
        public static readonly DependencyProperty JobProperty =
            DependencyProperty.Register("Job", typeof(JobBase), typeof(JobDetailsUserControl), new UIPropertyMetadata(null));
        // ----
        public string RecommendedActionImage
        {
            get { return (string)GetValue(RecommendedActionImageProperty); }
            set { SetValue(RecommendedActionImageProperty, value); }
        }
        public static readonly DependencyProperty RecommendedActionImageProperty =
            DependencyProperty.Register("RecommendedActionImage", typeof(string), typeof(JobDetailsUserControl));
        // ----
        public string RecommendedActionText
        {
            get { return (string)GetValue(RecommendedActionTextProperty); }
            set { SetValue(RecommendedActionTextProperty, value); }
        }
        public static readonly DependencyProperty RecommendedActionTextProperty =
            DependencyProperty.Register("RecommendedActionText", typeof(string), typeof(JobDetailsUserControl), new UIPropertyMetadata(""));
        // ----
        public string RecommendedActionReasons
        {
            get { return (string)GetValue(RecommendedActionReasonsProperty); }
            set { SetValue(RecommendedActionReasonsProperty, value); }
        }
        public static readonly DependencyProperty RecommendedActionReasonsProperty =
            DependencyProperty.Register("RecommendedActionReasons", typeof(string), typeof(JobDetailsUserControl), new UIPropertyMetadata(""));
        // ----
        public string IsEnabledButtonImage
        {
            get { return (string)GetValue(IsEnabledButtonImageProperty); }
            set { SetValue(IsEnabledButtonImageProperty, value); }
        }
        public static readonly DependencyProperty IsEnabledButtonImageProperty =
            DependencyProperty.Register("IsEnabledButtonImage", typeof(string), typeof(JobDetailsUserControl), new UIPropertyMetadata(""));
        // ----
        public string RunJobButtonImage
        {
            get { return (string)GetValue(RunJobButtonImageProperty); }
            set { SetValue(RunJobButtonImageProperty, value); }
        }
        public static readonly DependencyProperty RunJobButtonImageProperty =
            DependencyProperty.Register("RunJobButtonImage", typeof(string), typeof(JobDetailsUserControl), new UIPropertyMetadata(""));
        // ----
        public double GeneralEnabledDisableOpacity
        {
            get { return (double)GetValue(GeneralEnabledDisableOpacityProperty); }
            set { SetValue(GeneralEnabledDisableOpacityProperty, value); }
        }
        public static readonly DependencyProperty GeneralEnabledDisableOpacityProperty =
            DependencyProperty.Register("GeneralEnabledDisableOpacity", typeof(double), typeof(JobDetailsUserControl), new UIPropertyMetadata(1.0));
        // ----
        public Visibility SubControl_Visibility
        {
            get { return (Visibility)GetValue(SubControl_VisibilityProperty); }
            set { SetValue(SubControl_VisibilityProperty, value); }
        }
        public static readonly DependencyProperty SubControl_VisibilityProperty =
            DependencyProperty.Register("SubControl_Visibility", typeof(Visibility), typeof(JobDetailsUserControl), new UIPropertyMetadata(Visibility.Hidden));
        // ----
        public Visibility SubControlActionText_Visibility
        {
            get { return (Visibility)GetValue(SubControlActionText_VisibilityProperty); }
            set { SetValue(SubControlActionText_VisibilityProperty, value); }
        }
        public static readonly DependencyProperty SubControlActionText_VisibilityProperty =
            DependencyProperty.Register("SubControlActionText_Visibility", typeof(Visibility), typeof(JobDetailsUserControl), new UIPropertyMetadata(Visibility.Visible));
        // ----
        public Visibility showSnapshotsTab
        {
            get { return (Visibility)GetValue(showSnapshotsTabProperty); }
            set { SetValue(showSnapshotsTabProperty, value); }
        }
        public static readonly DependencyProperty showSnapshotsTabProperty =
            DependencyProperty.Register("showSnapshotsTab", typeof(Visibility), typeof(JobDetailsUserControl), new UIPropertyMetadata(Visibility.Collapsed));
        // ----
        public string JobAge
        {
            get { return (string)GetValue(JobAgeProperty); }
            set { SetValue(JobAgeProperty, value); }
        }
        public static readonly DependencyProperty JobAgeProperty = DependencyProperty.Register("JobAge", typeof(string), typeof(JobDetailsUserControl), new PropertyMetadata("---"));
        // ----
        #endregion
        #region User Control Events
        private void button_IsEnabled(object sender, RoutedEventArgs e)
        {
            Job.IsEnabled = !Job.IsEnabled;
            UpdateToolbarButtons();
            _EnabledAction(Job);
        }
        private void button_JobInformation(object sender, RoutedEventArgs e)
        {
            var wind = new JobInformationWindow(Job, _Settings);
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
        }
        private void buttonRunNow_Click(object sender, RoutedEventArgs e)
        {
            var sWatch = System.Diagnostics.Stopwatch.StartNew();
            App.WriteLog(nameof(App), ((Job is MirrorJob) ? App.LogCategory.Mirror : App.LogCategory.Archive), $"Starting Job; Name={Job.Name}; Date Last Ran={dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(DateTime.Now - Job.DateLastRan)} ({Job.DateLastRan:G})");
            SubControl_Visibility = Visibility.Hidden;
            SubControlActionText_Visibility = Visibility.Visible;
            //
            var wind = new RunWindow(_Settings, false, false, new List<Helper.RunPack>() { JobPack }, _OnShutdown);
            wind.Owner = Window.GetWindow(this);
            _IsAnalyzingJob = true;
            wind.ShowDialog();
            _IsAnalyzingJob = false;
            if (!wind.IsShuttingDown)
            {
                // select action status button
                UncheckAllSubControlToggleButtons();
                //
                System.Threading.Tasks.Task.Run(() =>
                {
                    JobBase job_ = null;
                    Dispatcher.Invoke(() => { job_ = Job.Clone(); });
                    ProcessJob(job_);
                    Dispatcher.Invoke(() =>
                    {
                        _AfterRunAction?.Invoke(Job);
                        PopulateRecommendedAction();
                        // select action status button
                        UncheckAllSubControlToggleButtons();
                        buttonActionStatus.IsChecked = true;
                        LoadSubUserControl(buttonActionStatus.Tag.ToString(), true);
                        //
                        SubControl_Visibility = Visibility.Visible;
                        SubControlActionText_Visibility = Visibility.Hidden;
                    });
                    //
                    sWatch.Stop();
                    App.WriteLog(nameof(App), ((job_ is MirrorJob) ? App.LogCategory.Mirror : App.LogCategory.Archive), $"Job Completed; Name={job_.Name}; Elapsed Time={dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(sWatch.Elapsed)}");
                });
            }
        }
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            _PrerefreshAction(Job);
            ProcessJob_BackgroundWorker(Job.Clone()).ContinueWith((task) =>
            {
                Dispatcher.Invoke(() => { _RefreshAction(Job); });
            });
        }
        private void buttonEditor_Click(object sender, RoutedEventArgs e)
        {
            EditJob();
        }
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var wind = new DeleteJobWindow(Job, _Settings);
            wind.Owner = Window.GetWindow(this);
            if (wind.ShowDialog().Value)
            {
                _CancelTokenSource.Cancel();
                _DeleteAction(Job);
            }
            if (wind.FilesDeleted)
            {
                _RefreshAction(Job);
                ProcessJob_BackgroundWorker(Job.Clone());
            }
        }
        // invoked when one of the sub-buttons are pressed (Action Status/Report/History/Removed Files/Snapshots)
        private void buttonSubUserControl_Click(object sender, RoutedEventArgs e)
        {
            if (_IsAnalyzingJob)
            {
                buttonActionStatus.IsChecked = true;
                LoadSubUserControl(buttonActionStatus.Tag.ToString(), true);
            }
            else
            {
                foreach (object item in toolBarSubControls.Items)
                {
                    var dude = item as System.Windows.Controls.Primitives.ToggleButton;
                    if (dude != null)
                    {
                        if (item == e.Source)
                        {
                            // process selected button
                            ((item as System.Windows.Controls.Primitives.ToggleButton)).IsChecked = true;
                            LoadSubUserControl(((item as System.Windows.Controls.Primitives.ToggleButton)).Tag.ToString(), false);
                        }
                        else
                        {
                            // process all other, non-selected, buttons
                            ((item as System.Windows.Controls.Primitives.ToggleButton)).IsChecked = false;
                        }
                    }
                }
            }
        }
        private void buttonSubUserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!_IsAnalyzingJob)
            {
                var dude = CreateSubUserControl(((System.Windows.FrameworkElement)(sender)).Tag.ToString());
                var wind = new SubUserControlWindow(dude);
                wind.Owner = Window.GetWindow(this);
                //if (userControlSubUserControl.Content != null) { (userControlSubUserControl.Content as ISubUserControl).Shutdown(); }
                //userControlSubUserControl.Content = null;
                wind.ShowDialog();
                //LoadSubUserControl(dude.Key, false);
            }
        }
        #endregion
        #region Public Methods
        public Helper.RunPack JobPack
        {
            get
            {
                return new Helper.RunPack()
                {
                    ComparisonReport = (_Settings.JobRunUseCachedReportEnableCache && ((DateTime.Now - _JobAnalyzedTimeStamp) < _Settings.JobRunUseCachedReportTimeLimit))
                                            ? _Report
                                            : null,
                    Job = Job
                };
            }
        }
        public void EditJob()
        {
            var wind = new EditorWindow(Job, false, _JobNames);
            wind.Owner = Window.GetWindow(this);
            if (wind.ShowDialog() ?? false)
            {
                _EditOkAction?.Invoke(wind.Job);
            }
            PopulateRecommendedAction();
        }
        public void Shutdown()
        {
            // ---- shutdown sub control
            if (userControlSubUserControl.Content != null)
            {
                (userControlSubUserControl.Content as ISubUserControl)?.Shutdown();
                userControlSubUserControl.Content = null;
            }
            // ---- stop the thread worker
            --App.ThreadWorkerCounter;
            App.WriteDebugLog(nameof(JobDetailsUserControl), $"{Job.Name}: Job Badge ThreadWorker Stopped");
            _ClockThreadCancelTokenSource.Cancel();
        }
        #endregion
        #region Private Methods
        private void PauseForDramaticEffect()
        {
            //dodSON.Core.Threading.ThreadingHelper.Sleep(TimeSpan.FromSeconds(2));
        }
        //private void UpdateUI()
        //{
        //    PopulateRecommendedAction();



        //    //    //
        //    //    UpdateToolbarButtons();
        //    //    InitializeRecommendationActionUI();

        //    //    PopulateRecommendedAction();
        //    //    UncheckAllSubControlToggleButtons();
        //    //    buttonActionStatus.IsChecked = true;
        //    //    LoadSubUserControl(buttonActionStatus.Tag.ToString(), true);
        //}
        private void UpdateToolbarButtons()
        {
            if (Job.IsEnabled)
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
            if (Job is MirrorJob)
            {
                RunJobButtonImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/folders.ico";
            }
            else
            {
                showSnapshotsTab = Visibility.Visible;
                RunJobButtonImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/decompress_48_h.ico";
            }
        }
        private void InitializeRecommendationActionUI()
        {
            RecommendedActionText = "Analyzing Job...";
            RecommendedActionImage = "";
            imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Visible;
            RecommendedActionReasons = "";
        }
        private void PopulateRecommendedAction()
        {
            // **** update created age
            JobAge = Job.Age;
            // **** update recommendation text and icon
            imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Hidden;
            if (_TotalChanges == 0)
            {
                RecommendedActionText = "No action required";
                RecommendedActionImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-security-high.ico";
            }
            else
            {
                var timeAgo = (DateTime.Now - Job.DateLastRan);
                if (Job.DateLastRan == DateTime.MinValue) { timeAgo = (DateTime.Now - Job.DateCreate); }
                if (timeAgo > _Settings.RecommendationActionAgeLimit)
                {
                    RecommendedActionText = "Run job";
                    RecommendedActionImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-security-low.ico";
                }
                else
                {
                    RecommendedActionText = "Consider running job";
                    RecommendedActionImage = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-security-medium.ico";
                }
            }
            Job.RecommendedActionText = RecommendedActionText;
            Job.RecommendedActionIcon = RecommendedActionImage;
            // **** update recommendation reasons
            var recommendedReasonsText = new System.Text.StringBuilder(1024);
            // date reasons
            if (Job.DateLastRan == DateTime.MinValue)
            {
                recommendedReasonsText.AppendLine("Job has never been run");
            }
            else
            {
                var agoString = dodSON.Core.Common.DateTimeHelper.FormatTimeSpan((DateTime.Now - Job.DateLastRan));
                recommendedReasonsText.AppendLine(string.Format("Job has not been run for {0}", agoString));
            }
            // files changed reasons
            if (_TotalChanges > 0)
            {
                recommendedReasonsText.AppendLine(string.Format("{0}% changed     ({1} changes/ {2} total)",
                                                        ((double)((double)_TotalChanges / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2"),
                                                        _TotalChanges.ToString("N0"),
                                                        _Total.ToString("N0")));
            }
            RecommendedActionReasons = recommendedReasonsText.ToString();
            Job.RecommendedActionReason = RecommendedActionReasons.Trim();
        }
        private void LoadSubUserControl(string userControlKey, bool forceUnload)
        {
            if (userControlSubUserControl.Content == null)
            {
                // load sub-control into empty slot
                userControlSubUserControl.Content = CreateSubUserControl(userControlKey);
            }
            else
            {
                // only if the system is forcing a reload OR the sub-control has changed
                if (forceUnload || ((userControlSubUserControl.Content as ISubUserControl).Key != userControlKey))
                {
                    // shutdown exiting sub-control and load new sub-control into, the now vacant slot.
                    (userControlSubUserControl.Content as ISubUserControl).Shutdown();
                    userControlSubUserControl.Content = CreateSubUserControl(userControlKey);
                }
            }
        }
        private ISubUserControl CreateSubUserControl(string userControlKey)
        {
            switch (userControlKey)
            {
                case "GRAPH1":
                    //ProcessJob(Job);
                    return new SubControl_Graph1(Job, _Report, _Settings);
                case "REPORT":
                    //ProcessJob(Job);
                    return new SubControl_Report(Job, _Report, _Settings);
                case "HISTORY":
                    return new SubControl_History(Job, _Report, _Settings);
                case "REMOVEDFILES":
                    return new SubControl_RemovedFiles(Job, _Report, _Settings);
                case "SNAPSHOTS":
                    return new SubControl_Snapshots_2(Job, _Settings);
                default:
                    break;
            }
            return null;
        }
        private System.Threading.Tasks.Task ProcessJob_BackgroundWorker(JobBase job)
        {
            if (!_IsAnalyzingJob)
            {
                _IsAnalyzingJob = true;
                //
                return System.Threading.Tasks.Task.Run(() =>
                {
                    // update UI
                    Dispatcher.Invoke(new Action(() =>
                          {
                              buttonRunNow.IsEnabled = false;
                              buttonRefresh.IsEnabled = false;
                              GeneralEnabledDisableOpacity = 0.2;
                              SubControl_Visibility = System.Windows.Visibility.Hidden;
                              SubControlActionText_Visibility = System.Windows.Visibility.Visible;
                              // 
                              InitializeRecommendationActionUI();

                              // TODO: lock UI controls
                          }));
                    //
                    PauseForDramaticEffect();
                    // process job into report
                    ProcessJob(job);
                    // update UI
                    Dispatcher.Invoke(new Action(() =>
                    {
                        buttonRunNow.IsEnabled = true;
                        buttonRefresh.IsEnabled = true;
                        GeneralEnabledDisableOpacity = 1.0;
                        SubControl_Visibility = System.Windows.Visibility.Visible;
                        SubControlActionText_Visibility = System.Windows.Visibility.Hidden;
                        //
                        PopulateRecommendedAction();
                        UncheckAllSubControlToggleButtons();
                        buttonActionStatus.IsChecked = true;
                        LoadSubUserControl(buttonActionStatus.Tag.ToString(), true);

                        // TODO: unlock UI controls
                    }));
                }, _CancelTokenSource.Token);
            }
            //
            return null;
        }
        private void ProcessJob(JobBase job)
        {
            _IsAnalyzingJob = true;
            // process report
            _Report = Helper.GenerateReport(job, _CancelTokenSource.Token);
            // gather statistics
            if (_Report != null)
            {
                _OkCount = (from x in _Report
                            where (x.Action == dodSON.Core.FileStorage.CompareAction.Ok)
                            select x).Count();
                _NewCount = (from x in _Report
                             where (x.Action == dodSON.Core.FileStorage.CompareAction.New)
                             select x).Count();
                _UpdateCount = (from x in _Report
                                where (x.Action == dodSON.Core.FileStorage.CompareAction.Update)
                                select x).Count();
                _RemoveCount = (from x in _Report
                                where (x.Action == dodSON.Core.FileStorage.CompareAction.Remove)
                                select x).Count();
                _Total = _OkCount + _UpdateCount + _RemoveCount;
                _TotalChanges = _NewCount + _UpdateCount + _RemoveCount;
            }
            _IsAnalyzingJob = false;
        }
        private void UncheckAllSubControlToggleButtons()
        {
            foreach (object item in toolBarSubControls.Items)
            {
                var dude = item as System.Windows.Controls.Primitives.ToggleButton;
                if (dude != null)
                {
                    dude.IsChecked = false;
                }
            }
        }
        //private IEnumerable<dodSON.Core.FileStorage.ICompareResult> GenerateReport(System.Threading.CancellationToken token)
        //{
        //    IEnumerable<dodSON.Core.FileStorage.ICompareResult> results = null;
        //    if (_Job is MirrorJob)
        //    {
        //        // **** MIRROR JOB
        //        results = dodSON.Core.FileStorage.FileStoreHelper.Compare(((MirrorJob)_Job).SourcePath,
        //                                                                            ((MirrorJob)_Job).MirrorPath,
        //                                                                            token,
        //                                                                            (x) => { DispatchProgressBarState(x, ((MirrorJob)_Job).SourcePath, ""); });
        //    }
        //    else
        //    {
        //        // **** ARCHIVE JOB
        //        double total = ((ArchiveJob)_Job).SourcePaths.Count;
        //        double count = 0;
        //        var worker = new List<dodSON.Core.FileStorage.ICompareResult>();
        //        foreach (var sourcePath in ((ArchiveJob)_Job).SourcePaths)
        //        {
        //            if (token.IsCancellationRequested) { return null; }
        //            var filename = string.Format("{0}.{1}.Archive.zip", Helper.StripBadCharacters(((ArchiveJob)_Job).Name), Helper.StripBadCharacters(Helper.FixPathString(sourcePath)));
        //            var archiveStoreFilename = System.IO.Path.Combine(((ArchiveJob)_Job).ArchiveRootPath, filename);
        //            if (System.IO.Directory.Exists(sourcePath))
        //            {
        //                try
        //                {
        //                    var archiveStore = new dodSON.Core.FileStorage.IonicZipStore.IonicZipFileStore(archiveStoreFilename, System.IO.Path.GetTempPath(), false);
        //                    var list = dodSON.Core.FileStorage.FileStoreHelper.Compare(sourcePath,
        //                                                                                         archiveStore,
        //                                                                                         token,
        //                                                                                         (x) =>
        //                                                                                         {
        //                                                                                             DispatchProgressBarState((count / total) + (x / total), sourcePath, "");
        //                                                                                         });
        //                    if (list != null) { worker.AddRange(list); }
        //                }
        //                catch { }
        //            }
        //            if (token.IsCancellationRequested) { return null; }
        //            count++;
        //        }
        //        results = (IEnumerable<dodSON.Core.FileStorage.ICompareResult>)worker;
        //    }
        //    if (!token.IsCancellationRequested)
        //    {
        //        return from x in results
        //               where x.ItemType == dodSON.Core.FileStorage.CompareType.File
        //               select x;
        //    }
        //    return null;
        //}
        //private void DispatchProgressBarState(double value, string text1, string text2)
        //{
        //    //this.Dispatcher.Invoke(new Action(
        //    //    () =>
        //    //    {
        //    //        textBlockAnalyzingText = text1;
        //    //        textBlockAnalyzingText2 = text2;
        //    //        if (value == -1)
        //    //        {
        //    //            ProgressBarIsIndeterminate = true;
        //    //        }
        //    //        else
        //    //        {
        //    //            ProgressBarIsIndeterminate = false;
        //    //            ProgressBarPercentage = value;
        //    //        }
        //    //    }));
        //}
        #endregion
    }
}
