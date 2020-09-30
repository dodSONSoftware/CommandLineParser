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
    public partial class RunWindow : Window
    {
        #region Ctor
        private RunWindow()
        {
            InitializeComponent();
            DataContext = this;
            Title = Helper.FormatTitle("Run");
        }
        public RunWindow(GlobalSettings settings,
                         bool autoShutdown,
                         bool minimized,
                         IEnumerable<Helper.RunPack> packs,
                         Action onShutdown)
            : this()
        {
            if (packs == null) { throw new ArgumentNullException("packs"); }
            _Settings = settings ?? throw new ArgumentException("settings");
            _OnShutdown = onShutdown ?? throw new ArgumentNullException("onShutdown");
            if (autoShutdown) { checkBoxShutdownAppWhenComplete.IsChecked = true; }
            if (minimized) { WindowState = WindowState.Minimized; }
            ProcessPacks(settings, packs);
        }
        #endregion
        #region Private Fields
        private readonly string _BUG_SEARCH_LOGID_ = "BUGHUNTER_1";
        private readonly DateTime _TimeStamp = DateTime.Now;
        private System.Threading.CancellationTokenSource _CancelTokenSource = new System.Threading.CancellationTokenSource();
        private readonly GlobalSettings _Settings = null;
        private readonly Action _OnShutdown = null;
        //
        private bool _AllowKeyboardAccess = false;
        #endregion
        #region Dependency Properties
        public string ProcessingJobTaskCompleteString
        {
            get { return (string)GetValue(ProcessingJobTaskCompleteStringProperty); }
            set { SetValue(ProcessingJobTaskCompleteStringProperty, value); }
        }
        public static readonly DependencyProperty ProcessingJobTaskCompleteStringProperty =
            DependencyProperty.Register("ProcessingJobTaskCompleteString", typeof(string), typeof(RunWindow), new UIPropertyMetadata("Processing Job"));
        public string ProcessingJobCountOfTotal
        {
            get { return (string)GetValue(ProcessingJobCountOfTotalProperty); }
            set { SetValue(ProcessingJobCountOfTotalProperty, value); }
        }
        public static readonly DependencyProperty ProcessingJobCountOfTotalProperty =
            DependencyProperty.Register("ProcessingJobCountOfTotal", typeof(string), typeof(RunWindow), new UIPropertyMetadata("0 of 0"));
        // ----
        public string ProcessDetail
        {
            get { return (string)GetValue(ProcessDetailProperty); }
            set { SetValue(ProcessDetailProperty, value); }
        }
        public static readonly DependencyProperty ProcessDetailProperty =
            DependencyProperty.Register("ProcessDetail", typeof(string), typeof(RunWindow), new UIPropertyMetadata(""));
        // ----
        public string JobName
        {
            get { return (string)GetValue(JobNameProperty); }
            set { SetValue(JobNameProperty, value); }
        }
        public static readonly DependencyProperty JobNameProperty =
            DependencyProperty.Register("JobName", typeof(string), typeof(RunWindow), new UIPropertyMetadata(""));
        // ----
        public string JobCountOfTotalString
        {
            get { return (string)GetValue(JobCountOfTotalStringProperty); }
            set { SetValue(JobCountOfTotalStringProperty, value); }
        }
        public static readonly DependencyProperty JobCountOfTotalStringProperty =
            DependencyProperty.Register("JobCountOfTotalString", typeof(string), typeof(RunWindow), new UIPropertyMetadata(""));
        // ----
        public string CancelButtonText
        {
            get { return (string)GetValue(CancelButtonTextProperty); }
            set { SetValue(CancelButtonTextProperty, value); }
        }
        public static readonly DependencyProperty CancelButtonTextProperty =
            DependencyProperty.Register("CancelButtonText", typeof(string), typeof(RunWindow), new UIPropertyMetadata("Cancel"));
        // ----
        public bool CancelButtonIsEnabled
        {
            get { return (bool)GetValue(CancelButtonIsEnabledProperty); }
            set { SetValue(CancelButtonIsEnabledProperty, value); }
        }
        public static readonly DependencyProperty CancelButtonIsEnabledProperty =
            DependencyProperty.Register("CancelButtonIsEnabled", typeof(bool), typeof(RunWindow), new UIPropertyMetadata(true));
        // ----
        public bool CheckBoxShutdownIsEnabled
        {
            get { return (bool)GetValue(CheckBoxShutdownIsEnabledProperty); }
            set { SetValue(CheckBoxShutdownIsEnabledProperty, value); }
        }
        public static readonly DependencyProperty CheckBoxShutdownIsEnabledProperty =
            DependencyProperty.Register("CheckBoxShutdownIsEnabled", typeof(bool), typeof(RunWindow), new UIPropertyMetadata(true));
        // ----
        public double ProgressBarTotalValue
        {
            get { return (double)GetValue(ProgressBarTotalValueProperty); }
            set { SetValue(ProgressBarTotalValueProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarTotalValueProperty =
            DependencyProperty.Register("ProgressBarTotalValue", typeof(double), typeof(RunWindow), new UIPropertyMetadata(0.0));
        // ----
        public bool ProgressBarTotalIsIndeterminate
        {
            get { return (bool)GetValue(ProgressBarTotalIsIndeterminateProperty); }
            set { SetValue(ProgressBarTotalIsIndeterminateProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarTotalIsIndeterminateProperty =
            DependencyProperty.Register("ProgressBarTotalIsIndeterminate", typeof(bool), typeof(RunWindow), new UIPropertyMetadata(true));
        // ----
        public Visibility ProgressBarTotalVisibility
        {
            get { return (Visibility)GetValue(ProgressBarTotalVisibilityProperty); }
            set { SetValue(ProgressBarTotalVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarTotalVisibilityProperty =
            DependencyProperty.Register("ProgressBarTotalVisibility", typeof(Visibility), typeof(RunWindow), new UIPropertyMetadata(Visibility.Visible));
        // ----
        public double ProgressBarValue
        {
            get { return (double)GetValue(ProgressBarValueProperty); }
            set { SetValue(ProgressBarValueProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarValueProperty =
            DependencyProperty.Register("ProgressBarValue", typeof(double), typeof(RunWindow), new UIPropertyMetadata(0.0));
        // ----
        public bool ProgressBarIsIndeterminate
        {
            get { return (bool)GetValue(ProgressBarIsIndeterminateProperty); }
            set { SetValue(ProgressBarIsIndeterminateProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarIsIndeterminateProperty =
            DependencyProperty.Register("ProgressBarIsIndeterminate", typeof(bool), typeof(RunWindow), new UIPropertyMetadata(true));
        // ----
        public Visibility ProgressBarVisibility
        {
            get { return (Visibility)GetValue(ProgressBarVisibilityProperty); }
            set { SetValue(ProgressBarVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ProgressBarVisibilityProperty =
            DependencyProperty.Register("ProgressBarVisibility", typeof(Visibility), typeof(RunWindow), new UIPropertyMetadata(Visibility.Visible));
        // ----
        public string ProcessingJobProgressText1
        {
            get { return (string)GetValue(ProcessingJobProgressText1Property); }
            set { SetValue(ProcessingJobProgressText1Property, value); }
        }
        public static readonly DependencyProperty ProcessingJobProgressText1Property =
            DependencyProperty.Register("ProcessingJobProgressText1", typeof(string), typeof(RunWindow), new UIPropertyMetadata(""));
        // ----
        public string ProcessingJobProgressText2
        {
            get { return (string)GetValue(ProcessingJobProgressText2Property); }
            set { SetValue(ProcessingJobProgressText2Property, value); }
        }
        public static readonly DependencyProperty ProcessingJobProgressText2Property =
            DependencyProperty.Register("ProcessingJobProgressText2", typeof(string), typeof(RunWindow), new UIPropertyMetadata(""));
        #endregion
        #region Public Methods
        public bool IsShuttingDown { get; private set; } = false;
        public int JobsProcessedCount { get; private set; }
        #endregion
        #region Windows Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_CancelTokenSource != null)
            {
                if (MessageBox.Show(Window.GetWindow(this), string.Format("Cancel Job Run{0}{0}Are you sure?", Environment.NewLine), "Cancel Job Run", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (_CancelTokenSource != null) { _CancelTokenSource.Cancel(); }
                    Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.None);
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.None);
            }
        }
        private void checkBoxShutdownAppWhenComplete_Checked(object sender, RoutedEventArgs e)
        {
            _Settings.ProcessingShutDownWhenComplete = true;
        }
        private void checkBoxShutdownAppWhenComplete_Unchecked(object sender, RoutedEventArgs e)
        {
            _Settings.ProcessingShutDownWhenComplete = false;
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if ((_AllowKeyboardAccess) && (e.Key == Key.Escape))
            {
                e.Handled = true;
                Close();
            }
        }
        #endregion
        #region Private Methods
        private void ProcessPacks(GlobalSettings settings, IEnumerable<Helper.RunPack> packs)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: Starting [---task---]");

                _AllowKeyboardAccess = false;
                var innerCancellationToken_ClockTask = new System.Threading.CancellationTokenSource();
                var stWatch = System.Diagnostics.Stopwatch.StartNew();
                // start the on-form clock update task
                var innerTask = System.Threading.Tasks.Task.Run(() =>
                {
                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: Starting [---innertask---] (clock)");

                    while (!innerCancellationToken_ClockTask.IsCancellationRequested)
                    {
                        UpdateUI(() => { ProcessingJobProgressText2 = stWatch.Elapsed.ToString("mm\\:ss\\.f"); });
                        dodSON.Core.Threading.ThreadingHelper.Sleep(50);
                    }

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: Stopping [---innertask---] (clock)");

                });
                UpdateUI(() =>
                {
                    Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.Normal);
                    ProgressBarTotalIsIndeterminate = false;
                    ProgressBarTotalValue = 0;
                    ProgressBarIsIndeterminate = false;
                    ProgressBarValue = 0;
                });
                var count = 0;
                JobsProcessedCount = 0;
                var jobsTotal = packs.Count();
                var jobsCounter = 0;
                //
                try
                {
                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: Starting foreach (var pack in packs) loop");

                    // process all packs
                    foreach (var pack in packs)
                    {
                        UpdateUI(() =>
                        {
                            JobCountOfTotalString = string.Format("{0} of {1}", ++count, packs.Count());
                            JobName = pack.Job.Name;
                            ProcessingJobProgressText1 = "";
                            ProgressBarValue = 0;
                        });
                        if (ProcessReport(pack))
                        {

                            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: Starting ProcessReport");

                            ++JobsProcessedCount;
                            ProcessRemovedFiles(settings, pack);
                            ProcessJob(pack);
                            if ((settings.EnableArchiveZipFileSnapshots) && (pack.Job is ArchiveJob))
                            {
                                UpdateUI(() =>
                                {
                                    ProcessDetail = "Processing Snapshots";
                                    ProcessingJobProgressText1 = "";
                                });
                                var dudeTokenSource = new System.Threading.CancellationTokenSource();

                                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"Helper.ProcessSnapshots: Starting. Name={pack.Job.Name}, Id={pack.Job.ID}");

                                Helper.ProcessSnapshots((ArchiveJob)pack.Job, _Settings, _TimeStamp, false, dudeTokenSource.Token, (percent_, text_) =>
                                {
                                    // feedback
                                    UpdateProgressBar(percent_, text_);
                                });

                                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"Helper.ProcessSnapshots: Completed. Name={pack.Job.Name}, Id={pack.Job.ID}");

                            }
                            OutputReport(settings, pack);

                            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: ProcessReport Completed");

                        }
                        // update top progress bar
                        UpdateUI(() =>
                        {
                            ProgressBarTotalValue = (double)++jobsCounter / jobsTotal;
                            Helper.MainWindow.SetGlobalPercent(ProgressBarTotalValue, System.Windows.Shell.TaskbarItemProgressState.Normal);
                        });
                        if ((_CancelTokenSource != null) && (_CancelTokenSource.Token.IsCancellationRequested)) { break; }
                    }

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: foreach (var pack in packs) loop Completed");

                    // process overflows
                    if (JobsProcessedCount > 0) { ProcessOverflows(settings); }
                    UpdateProgressBar(1, "");
                    // ----

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: Starting final UI update");

                    System.Windows.Shell.TaskbarItemProgressState completedState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                    if ((_CancelTokenSource != null) && (!_CancelTokenSource.Token.IsCancellationRequested))
                    {
                        UpdateUI(() =>
                        {

                            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: [task complete]");

                            ProcessingJobTaskCompleteString = "";
                            JobCountOfTotalString = "";
                            JobName = "Task Complete";
                            ProcessDetail = "";
                            Helper.MainWindow.SetGlobalPercent(1, completedState);
                            ProgressBarTotalValue = 1;
                            ProgressBarValue = 1;
                            ProcessingJobProgressText1 = "";
                            CancelButtonText = "Close";
                            CancelButtonIsEnabled = true;
                            CheckBoxShutdownIsEnabled = true;
                            // 
                            if (checkBoxShutdownAppWhenComplete.IsChecked ?? false)
                            {

                                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: [shutting down...]");

                                ProcessDetail = "Shutting down...";
                                Helper.MainWindow.SetGlobalPercent(1, completedState);
                                ProgressBarTotalValue = 1;
                                ProgressBarValue = 1;
                                ProcessingJobProgressText1 = "";
                                CancelButtonIsEnabled = false;
                                CheckBoxShutdownIsEnabled = false;
                                IsShuttingDown = true;
                                _OnShutdown();
                            }
                        });
                    }
                    else
                    {
                        UpdateUI(() =>
                        {

                            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: [task canceled]");

                            ProcessingJobTaskCompleteString = "";
                            JobCountOfTotalString = "";
                            JobName = "Task Canceled";
                            ProcessDetail = "";
                            Helper.MainWindow.SetGlobalPercent(1, completedState);
                            ProgressBarTotalValue = 1;
                            ProgressBarValue = 1;
                            ProcessingJobProgressText1 = "";
                            CancelButtonText = "Close";
                            CancelButtonIsEnabled = true;
                            CheckBoxShutdownIsEnabled = true;
                        });
                    }
                }
                finally
                {

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: [---finally---]");

                    _CancelTokenSource.Cancel();
                    _CancelTokenSource.Dispose();
                    _CancelTokenSource = null;
                    // ----
                    innerCancellationToken_ClockTask.Cancel();
                    innerTask.Wait(1000);
                    innerCancellationToken_ClockTask.Dispose();
                    innerCancellationToken_ClockTask = null;
                    // ----
                    dodSON.Core.Threading.ThreadingHelper.Sleep(TimeSpan.FromSeconds(1));
                }
            }).ContinueWith((task) =>
            {

                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessPacks: [---continuewith---]");

                _AllowKeyboardAccess = true;
            });
        }
        private void UpdateProgressBar(double percentage, string text1)
        {
            if (percentage == -2)
            {
                // do nothing
            }
            else if (percentage == -1)
            {
                // indeterminate
                UpdateUI(() =>
                {
                    ProgressBarIsIndeterminate = true;
                    ProgressBarValue = 0;
                });
            }
            else
            {
                // calculate percentage
                UpdateUI(() =>
                {
                    ProgressBarIsIndeterminate = false;
                    ProgressBarValue = percentage;
                });
            }
            // ----
            UpdateUI(() => { ProcessingJobProgressText1 = text1; });
        }
        private bool ProcessReport(Helper.RunPack pack)
        {
            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessReport: Started. Name={pack.Job.Name}, Id={pack.Job.ID}");

            if ((_CancelTokenSource != null) && (!_CancelTokenSource.Token.IsCancellationRequested))
            {
                if (pack.ComparisonReport == null)
                {
                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessReport: Starting Analyzing Job");

                    UpdateUI(() =>
                    {
                        ProcessDetail = "Analyzing Job";
                        ProcessingJobProgressText1 = "";
                    });
                    pack.ComparisonReport = Helper.GenerateReport(pack.Job, _CancelTokenSource.Token);

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessReport: Analyzing Job Completed");
                }
                var dude = CanProcess();

                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessReport: Competed. Result={dude}, Name={pack.Job.Name}, Id={pack.Job.ID}");

                return dude;
            }

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessReport: Competed. Result=false, Name={pack.Job.Name}, Id={pack.Job.ID}");

            return false;

            // ########## internal function ##########
            bool CanProcess() => pack.ComparisonReport.FirstOrDefault((result) => { return result.Action != dodSON.Core.FileStorage.CompareAction.Ok; }) != null;
        }
        private void ProcessRemovedFiles(GlobalSettings settings, Helper.RunPack pack)
        {
            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessRemovedFiles: Starting. Name={pack.Job.Name}, Id={pack.Job.ID}");

            if ((_CancelTokenSource != null) && (!_CancelTokenSource.Token.IsCancellationRequested))
            {
                if ((settings.EnableRemovedFilesArchive) && (pack.Job.ArchiveRemoveFiles))
                {
                    UpdateUI(() =>
                    {
                        ProcessDetail = "Archiving Removed Files";
                        ProcessingJobProgressText1 = "";
                    });
                    ArchiveRemovedFiles(settings, pack);
                }
            }

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessRemovedFiles: Completed. Name={pack.Job.Name}, Id={pack.Job.ID}");

        }
        private void ProcessJob(Helper.RunPack pack)
        {
            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessJob: Starting. Name={pack.Job.Name}, Id={pack.Job.ID}");

            if ((_CancelTokenSource != null) && (!_CancelTokenSource.Token.IsCancellationRequested))
            {
                UpdateUI(() =>
                {
                    ProcessDetail = "Processing Job";
                    ProcessingJobProgressText1 = "";
                });
                ExecuteJob(pack);
            }
            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessJob: Completed. Name={pack.Job.Name}, Id={pack.Job.ID}");

        }
        private void OutputReport(GlobalSettings settings, Helper.RunPack pack)
        {
            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"OutputReport: Starting. Name={pack.Job.Name}, Id={pack.Job.ID}");

            if ((_CancelTokenSource != null) && (!_CancelTokenSource.Token.IsCancellationRequested) &&
                (_Settings.EnableAutomaticReporting))
            {
                UpdateUI(() =>
                {
                    ProcessDetail = "Creating Report";
                    ProcessingJobProgressText1 = "";
                });
                UpdateProgressBar(1, "");
                CreateAndSaveReport(settings, pack);
            }
            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"OutputReport: Completed. Name={pack.Job.Name}, Id={pack.Job.ID}");

        }
        private void ProcessOverflows(GlobalSettings settings)
        {
            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Starting");

            UpdateUI(() =>
            {
                ProcessDetail = "Cleaning up...";
                ProcessingJobProgressText1 = "";
            });
            UpdateProgressBar(-1, "Initializing...");

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Initializing");

            // ######## process reports (AGE)
            if (settings.EnableAutomaticReportsDeletionByAge)
            {
                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: EnableAutomaticReportsDeletionByAge");

                UpdateProgressBar(-1, "Processing Reports");
                var toBeDeleted = new List<dodSON.Core.FileStorage.IFileStoreItem>();
                var storeFilename = System.IO.Path.Combine(settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var store = new dodSON.Core.FileStorage.MSdotNETZip.FileStore(storeFilename, System.IO.Path.GetTempPath(), false);

                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Starting store.ForEach(...) loop");

                store.ForEach((storeItem) =>
                {
                    if ((_CancelTokenSource != null) && (_CancelTokenSource.Token.IsCancellationRequested)) { return; }
                    if (storeItem.IsRootFileAvailable)
                    {
                        if (DateTime.UtcNow > (storeItem.RootFileLastModifiedTimeUtc + settings.RemovedAutomaticReportsTimeLimit))
                        {
                            toBeDeleted.Add(storeItem);
                        }
                    }
                });

                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: store.ForEach(...) loop Completed");

                if (toBeDeleted.Count > 0)
                {

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Starting foreach (var item in toBeDeleted) loop");

                    foreach (var item in toBeDeleted)
                    {
                        store.Delete(item.RootFilename);
                    }

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: foreach (var item in toBeDeleted) loop Completed");

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Starting store.Save(true)");

                    try { store.Save(true); }
                    catch (Exception ex)
                    {
                        App.WriteErrorLog(_BUG_SEARCH_LOGID_, ex.Message + Environment.NewLine + ex.StackTrace);
                    }

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: store.Save(true) Completed");

                }
            }


            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Starting Processing Removed Files Archive");

            // ######## process removed files archive (AGE/SIZE)
            UpdateProgressBar(-1, "Processing Removed Files Archive");
            // ---- get data
            long combinedByteSize = 0;
            var list = new List<dynamic>();

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Starting foreach (var item in System.IO.Directory.GetFiles(settings.RemovedFilesArchiveRootPath, ...) loop");

            foreach (var item in System.IO.Directory.GetFiles(settings.RemovedFilesArchiveRootPath, "*.RemovedFiles.zip", System.IO.SearchOption.TopDirectoryOnly))
            {
                if ((_CancelTokenSource != null) && (_CancelTokenSource.Token.IsCancellationRequested)) { return; }
                var fInfo = new System.IO.FileInfo(item);
                combinedByteSize += fInfo.Length;
                list.Add(new { Filename = item, fInfo.CreationTime, fInfo.Length });
            }

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: foreach (var item in System.IO.Directory.GetFiles(settings.RemovedFilesArchiveRootPath, ...) loop Completed");

            // ---- process remove archives based on AGE
            if (settings.EnableAutoDeleteRemovedFilesArchiveByAge)
            {

                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Starting EnableAutoDeleteRemovedFilesArchiveByAge process loop");

                foreach (var item in list)
                {
                    if ((_CancelTokenSource != null) && (_CancelTokenSource.Token.IsCancellationRequested)) { return; }
                    if (DateTime.Now > ((DateTime)item.CreationTime + settings.RemovedFileArchiveAutoDeleteTimeLimit))
                    {
                        Helper.DeleteFile((string)item.Filename);
                    }
                }

                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: EnableAutoDeleteRemovedFilesArchiveByAge process loop Completed");

            }
            // ---- process remove archives based on SIZE
            if ((settings.EnableAutoDeleteRemovedFilesArchiveBySize) && (combinedByteSize > settings.RemovedFileArchiveAutoDeleteByteSizeLimit))
            {

                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Starting remove archives based on SIZE process loop");

                long countingByteSize = combinedByteSize;

                // sort by age ascending
                var enumerator = (from x in list
                                  orderby (DateTime)x.CreationTime ascending
                                  select x).GetEnumerator();
                if (enumerator.MoveNext())
                {
                    while (countingByteSize > settings.RemovedFileArchiveAutoDeleteByteSizeLimit)
                    {
                        if ((_CancelTokenSource != null) && (_CancelTokenSource.Token.IsCancellationRequested)) { return; }
                        var filename = (string)enumerator.Current.Filename;
                        var length = (long)enumerator.Current.Length;
                        //
                        Helper.DeleteFile(filename);
                        countingByteSize -= length;
                        // move to next item in list; terminate while...loop if at end of list
                        if (!enumerator.MoveNext()) { break; }
                    }
                }

                App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: remove archives based on SIZE process loop Completed");

            }

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ProcessOverflows: Completed");

        }
        // ----------------
        private void UpdateUI(Action worker)
        {
            if (worker == null) { throw new ArgumentNullException("worker"); }
            this.Dispatcher.Invoke(worker);
        }
        private void ArchiveRemovedFiles(GlobalSettings settings, Helper.RunPack pack)
        {
            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ArchiveRemovedFiles: Starting. Name={pack.Job.Name}, Id={pack.Job.ID}");

            if ((_CancelTokenSource != null) && (!_CancelTokenSource.Token.IsCancellationRequested))
            {
                //// get total number of files to UPDATE & REMOVE
                //// TODO: QUESTION: which is more efficient, this way or a simple loop and a ++counter
                var total = (from x in pack.ComparisonReport
                             where ((x.ItemType == dodSON.Core.FileStorage.CompareType.File) &&
                                    (x.Action == dodSON.Core.FileStorage.CompareAction.Update || x.Action == dodSON.Core.FileStorage.CompareAction.Remove))
                             select x).Count();
                //var total = 0;
                //foreach (var item in pack.ComparisonReport)
                //{
                //    if ((item.ItemType == dodSON.Core.FileStorage.CompareType.File) &&
                //        (item.Action == dodSON.Core.FileStorage.CompareAction.Remove) ||
                //        (item.Action == dodSON.Core.FileStorage.CompareAction.Update))
                //    {
                //        ++total;
                //    }
                //}
                if (total > 0)
                {
                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ArchiveRemovedFiles: Starting archive process");

                    UpdateProgressBar(0, "Initializing Archive Remove Files...");
                    double count = 0;
                    // create a dynamic variable
                    var cleanupFiles = new List<dynamic>();
                    // create working directory
                    var tempRootPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dodSON" + ((int)DateTime.Now.TimeOfDay.TotalMilliseconds).ToString());
                    if (!System.IO.Directory.Exists(tempRootPath)) { System.IO.Directory.CreateDirectory(tempRootPath); }
                    // create removed files archive
                    var removedFilesArchiveRootFilename = string.Format("{0}.{1}.RemovedFiles.zip", Helper.StripBadCharacters(pack.Job.Name), _TimeStamp.ToString("yyyyMMdd^HHmmss"));
                    var removedFilesArchiveFullFilename = System.IO.Path.Combine(settings.RemovedFilesArchiveRootPath, removedFilesArchiveRootFilename);
                    var tempRemovedFilesArchiveFullFilename = System.IO.Path.Combine(tempRootPath, removedFilesArchiveRootFilename);
                    var removedFilesStore = Helper.GetZipStore(tempRemovedFilesArchiveFullFilename, tempRootPath, true, Helper.GetExtensionsToStore(_Settings));
                    // process all files to UPDATE & REMOVE

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ArchiveRemovedFiles: Starting foreach (var item in pack.ComparisonReport) loop");

                    foreach (var item in pack.ComparisonReport)
                    {
                        App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ArchiveRemovedFiles: processing pack, ");

                        if ((item.ItemType == dodSON.Core.FileStorage.CompareType.File) &&
                            (item.Action == dodSON.Core.FileStorage.CompareAction.Remove) ||
                            (item.Action == dodSON.Core.FileStorage.CompareAction.Update))
                        {
                            if ((_CancelTokenSource != null) && (!_CancelTokenSource.Token.IsCancellationRequested))
                            {
                                if (pack.Job is ArchiveJob)
                                {
                                    // #### archive job
                                    // create temp extraction path
                                    var extractionRootPath = System.IO.Path.Combine(tempRootPath, item.DestinationRootPath.Split('.')[1]);
                                    if (!System.IO.Directory.Exists(extractionRootPath)) { System.IO.Directory.CreateDirectory(extractionRootPath); }
                                    // open source archive store
                                    var sourceArchiveFilename = System.IO.Path.Combine((pack.Job as ArchiveJob).ArchiveRootPath, item.DestinationRootPath);
                                    var sourceStore = Helper.GetZipStore(sourceArchiveFilename, extractionRootPath, true, null);
                                    if (sourceStore.Contains(item.DestinationRootFilename))
                                    {
                                        // create removed files archive root path name
                                        var archiveRootPathPrefix = System.IO.Path.GetFileNameWithoutExtension(sourceArchiveFilename).Split('.')[1];
                                        // get desired file item
                                        var sourceItem = sourceStore.Get(item.DestinationRootFilename);
                                        var extractedFilename = sourceStore.Extract(extractionRootPath, sourceItem, true);
                                        // populate the dynamic list
                                        cleanupFiles.Add(new
                                        {
                                            ExtractedFilename = extractedFilename,
                                            sourceItem.OriginalFilename,
                                            StoreItem = removedFilesStore.Add(archiveRootPathPrefix + "\\" + sourceItem.RootFilename,
                                                                              extractedFilename,
                                                                              sourceItem.RootFileLastModifiedTimeUtc,
                                                                              sourceItem.FileSize)
                                        });
                                    }
                                }
                                else
                                {
                                    // #### mirror job
                                    removedFilesStore.Add(item.DestinationRootFilename,
                                                          item.DestinationFullPath,
                                                          item.DestinationLastModifiedTimeUtc,
                                                          item.SourceFileSizeInBytes);
                                }
                                // update UI
                                UpdateUI(() => { UpdateProgressBar(count++ / (double)total, $"archiving {item.DestinationRootFilename}"); });
                            }
                        }
                    }

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ArchiveRemovedFiles: foreach (var item in pack.ComparisonReport) loop Completed");

                    // update UI
                    UpdateUI(() => { UpdateProgressBar(-2, $"saving archive {removedFilesArchiveRootFilename}"); });
                    // clean up
                    bool isArchiveJob = pack.Job is ArchiveJob;
                    // save removed files archive (will load all files into the ZIP file)
                    removedFilesStore.Save(false);
                    // 

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ArchiveRemovedFiles: Starting foreach (var item in cleanupFiles) loop");

                    foreach (var item in cleanupFiles)
                    {
                        // restore the 'proper' original file names
                        if (isArchiveJob)
                        {
                            ((dodSON.Core.FileStorage.IFileStoreItemAdvanced)item.StoreItem).SetOriginalFilename((string)item.OriginalFilename);
                        }
                        // cleanup temp files
                        Helper.DeleteFile((string)item.ExtractedFilename);
                    }

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ArchiveRemovedFiles: foreach (var item in cleanupFiles) loop Completed");

                    // save to update the 'original filenames' file
                    removedFilesStore.Save(false);
                    // move to its final destination
                    System.IO.File.Move(tempRemovedFilesArchiveFullFilename, removedFilesArchiveFullFilename);
                    // delete temp path
                    Helper.DeleteDirectory(tempRootPath);
                }
            }

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ArchiveRemovedFiles: Completed. Name={pack.Job.Name}, Id={pack.Job.ID}");

        }
        private void ExecuteJob(Helper.RunPack pack)
        {

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ExecuteJob: Starting. Name={pack.Job.Name}, Id={pack.Job.ID}");

            if ((_CancelTokenSource != null) && (!_CancelTokenSource.Token.IsCancellationRequested))
            {
                var itemSeparator = ";";
                var header = "  - ";
                var sWatch = System.Diagnostics.Stopwatch.StartNew();
                UpdateProgressBar(0, "Initializing Process Job...");
                //
                if (pack.Job is ArchiveJob)
                {

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ExecuteJob: Starting Archive Job. Name={pack.Job.Name}, Id={pack.Job.ID}, SourcePaths={((ArchiveJob)pack.Job).SourcePaths.Count:N0}");

                    // ######## ARCHIVE JOB
                    // process job
                    foreach (var item in ((ArchiveJob)pack.Job).SourcePaths)
                    {
                        if (!System.IO.Directory.Exists(item))
                        {
                            // TODO: throw error || log error and continue
                        }
                        // get file store
                        var filename = string.Format("{0}.{1}.Archive.zip", Helper.StripBadCharacters(pack.Job.Name), Helper.FixPathString(item));
                        var fullPath = ((ArchiveJob)pack.Job).ArchiveRootPath;
                        if (!System.IO.Directory.Exists(fullPath))
                        {
                            System.IO.Directory.CreateDirectory(fullPath);
                        }
                        var fullFilename = System.IO.Path.Combine(fullPath, filename);
                        var store = Helper.GetZipStore(fullFilename, System.IO.Path.GetTempPath(), true, Helper.GetExtensionsToStore(_Settings));
                        // execute it
                        dodSON.Core.FileStorage.FileStorageHelper.MirrorSourceToDestination(FilterReport(pack, filename),
                                                                                            item,
                                                                                            store,
                                                                                            (compareItem, percentDone) =>
                                                                                            {
                                                                                                // feedback
                                                                                                UpdateUI(() =>
                                                                                                {
                                                                                                    if (compareItem == null)
                                                                                                    {
                                                                                                        UpdateProgressBar(-1, "finalizing...");
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        UpdateProgressBar(percentDone, $"archiving {(!string.IsNullOrWhiteSpace(compareItem.DestinationRootFilename) ? compareItem.DestinationRootFilename : compareItem.SourceRootFilename)}");
                                                                                                    }
                                                                                                });
                                                                                            });
                    }

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ExecuteJob: Competed Archive Job. Name={pack.Job.Name}, Id={pack.Job.ID}, SourcePaths={((ArchiveJob)pack.Job).SourcePaths.Count:N0}");

                    // log it
                    WriteLog(App.LogCategory.Archive, $"{header}{pack.Job.Name} executed{itemSeparator} Elapsed Time={sWatch.Elapsed}{itemSeparator} Job Type=Archive{itemSeparator} Recommended Action={pack.Job.RecommendedActionText}{itemSeparator} Report={GetReportFilename(pack)}{itemSeparator} Archive Removed File={pack.Job.ArchiveRemoveFiles}{itemSeparator} Last Ran={dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(DateTime.Now - pack.Job.DateLastRan)} ({pack.Job.DateLastRan})");
                }
                else
                {

                    App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ExecuteJob: Starting Mirror Job. Name={pack.Job.Name}, Id={pack.Job.ID}");

                    // ######## MIRROR JOB
                    // execute it
                    dodSON.Core.FileStorage.FileStorageHelper.MirrorSourceToDestination(pack.ComparisonReport,
                                                                                        ((MirrorJob)pack.Job).SourcePath,
                                                                                        ((MirrorJob)pack.Job).MirrorPath,
                                                                                        (compareItem, percentDone) =>
                                                                                        {
                                                                                            // feedback
                                                                                            UpdateUI(() =>
                                                                                            {
                                                                                                if (compareItem == null)
                                                                                                {
                                                                                                    UpdateProgressBar(percentDone, "finalizing...");
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    UpdateProgressBar(percentDone, $"processing {(!string.IsNullOrWhiteSpace(compareItem.DestinationRootFilename) ? compareItem.DestinationRootFilename : compareItem.SourceRootFilename)}");
                                                                                                }
                                                                                            });
                                                                                        });
                    // log it
                    WriteLog(App.LogCategory.Mirror, $"{header}{pack.Job.Name} executed{itemSeparator} Elapsed Time={sWatch.Elapsed}{itemSeparator} Job Type=Mirror{itemSeparator} Recommended Action={pack.Job.RecommendedActionText}{itemSeparator} Report={GetReportFilename(pack)}{itemSeparator} Archive Removed File={pack.Job.ArchiveRemoveFiles}{itemSeparator} Last Ran={dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(DateTime.Now - pack.Job.DateLastRan)} ({pack.Job.DateLastRan})");
                }
                pack.Job.DateLastRan = pack.TimeStamp;
            }

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"ExecuteJob: Completed. Name={pack.Job.Name}, Id={pack.Job.ID}");



            // ################ INTERNAL FUNCTIONS

            IEnumerable<dodSON.Core.FileStorage.ICompareResult> FilterReport(Helper.RunPack pack_, string archiveName_)
            {
                return from x in pack_.ComparisonReport
                       where x.DestinationRootPath.Equals(archiveName_, StringComparison.InvariantCultureIgnoreCase)
                       select x;
            }

            void WriteLog(App.LogCategory category, string text)
            {
                // TODO: add number of removed files and their total size

                App.WriteLog(nameof(App), category, text, _TimeStamp);
            }
        }

        private void CreateAndSaveReport(GlobalSettings settings, Helper.RunPack pack)
        {

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"CreateAndSaveReport: Starting. Name={pack.Job.Name}, Id={pack.Job.ID}");

            if ((_CancelTokenSource != null) && (!_CancelTokenSource.Token.IsCancellationRequested))
            {
                string filename = GetReportFilename(pack);
                var fullFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, filename);
                var stats = Helper.AnalyzeReport(pack.Job, pack.ComparisonReport, _TimeStamp);
                stats.ReportDate = _TimeStamp;
                Helper.OutputReport(fullFilename, stats, pack.ComparisonReport, settings);
            }

            App.WriteDebugLog(_BUG_SEARCH_LOGID_, $"CreateAndSaveReport: Completed. Name={pack.Job.Name}, Id={pack.Job.ID}");

        }

        private string GetReportFilename(Helper.RunPack pack)
        {
            return $"{Helper.StripBadCharacters(pack.Job.Name)}.{_TimeStamp.ToString("yyyyMMdd^HHmmss")}.JobReport.txt";
        }
        #endregion
    }
}
