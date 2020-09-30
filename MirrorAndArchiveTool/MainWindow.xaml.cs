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
    public partial class MainWindow : Window
    {
        #region Internal Static Methods
        // TODO: add a new parameter   (Window owner, ...
        //internal static void DisplayErrorMessageBox(Window owner, string caption, string message)
        internal static void DisplayErrorMessageBox(string caption, string message)
        {
            //MessageBox.Show(owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
        #region Ctor
        public MainWindow(bool minimized, bool runAll, bool shutdown)
        {
            _Minimized = minimized;
            _RunAll = runAll;
            _Shutdown = shutdown;

            Helper.MainWindow = this;
            InitializeComponent();
            this.DataContext = this;
            Jobs = new System.Collections.ObjectModel.ObservableCollection<JobBase>();
            Title = Helper.FormatTitle("");
            //
            LoadGlobalSettings();
            LoadUserSettings();
            LoadJobs();
            SortJobsInJobsList();
            //
            App.WriteDebugLog(nameof(MainWindow), $"Opening Main Window");
        }
        #endregion
        #region Private Fields
        private JobDetailsUserControl _CurrentJob = null;
        private GlobalSettings _GlobalSettings = new GlobalSettings();
        private readonly bool _Minimized = false;
        private readonly bool _RunAll = false;
        private readonly bool _Shutdown = false;
        #endregion
        #region Dependency Properties
        public double GlobalPercentValue
        {
            get { return (double)GetValue(GlobalPercentValueProperty); }
            set { SetValue(GlobalPercentValueProperty, value); }
        }
        public static readonly DependencyProperty GlobalPercentValueProperty =
            DependencyProperty.Register("GlobalPercentValue", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));
        public System.Windows.Shell.TaskbarItemProgressState GlobalPercentState
        {
            get { return (System.Windows.Shell.TaskbarItemProgressState)GetValue(GlobalPercentStateProperty); }
            set { SetValue(GlobalPercentStateProperty, value); }
        }
        public static readonly DependencyProperty GlobalPercentStateProperty =
            DependencyProperty.Register("GlobalPercentState", typeof(System.Windows.Shell.TaskbarItemProgressState), typeof(MainWindow), new PropertyMetadata(System.Windows.Shell.TaskbarItemProgressState.None));
        public System.Collections.ObjectModel.ObservableCollection<JobBase> Jobs
        {
            get { return (System.Collections.ObjectModel.ObservableCollection<JobBase>)GetValue(JobsProperty); }
            set { SetValue(JobsProperty, value); }
        }
        public static readonly DependencyProperty JobsProperty =
            DependencyProperty.Register("Jobs", typeof(System.Collections.ObjectModel.ObservableCollection<JobBase>), typeof(MainWindow));
        // ----
        public string CreateOrSelectJobText
        {
            get { return (string)GetValue(CreateOrSelectJobTextProperty); }
            set { SetValue(CreateOrSelectJobTextProperty, value); }
        }
        public static readonly DependencyProperty CreateOrSelectJobTextProperty =
            DependencyProperty.Register("CreateOrSelectJobText", typeof(string), typeof(MainWindow), new UIPropertyMetadata("Working..."));
        // ----
        public bool ButtonRunAllEnabledJob_IsEnabled
        {
            get { return (bool)GetValue(ButtonRunAllEnabledJob_IsEnabledProperty); }
            set { SetValue(ButtonRunAllEnabledJob_IsEnabledProperty, value); }
        }
        public static readonly DependencyProperty ButtonRunAllEnabledJob_IsEnabledProperty =
            DependencyProperty.Register("ButtonRunAllEnabledJob_IsEnabled", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false));
        // ----
        public double ButtonRunAllEnabledJob_Opacity
        {
            get { return (double)GetValue(ButtonRunAllEnabledJob_OpacityProperty); }
            set { SetValue(ButtonRunAllEnabledJob_OpacityProperty, value); }
        }
        public static readonly DependencyProperty ButtonRunAllEnabledJob_OpacityProperty =
            DependencyProperty.Register("ButtonRunAllEnabledJob_Opacity", typeof(double), typeof(MainWindow), new UIPropertyMetadata(0.2));
        // ----
        public bool ButtonRefreshAll_IsEnabled
        {
            get { return (bool)GetValue(ButtonRefreshAll_IsEnabledProperty); }
            set { SetValue(ButtonRefreshAll_IsEnabledProperty, value); }
        }
        public static readonly DependencyProperty ButtonRefreshAll_IsEnabledProperty =
            DependencyProperty.Register("ButtonRefreshAll_IsEnabled", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false));
        // ----
        public double ButtonRefreshAll_Opacity
        {
            get { return (double)GetValue(ButtonRefreshAll_OpacityProperty); }
            set { SetValue(ButtonRefreshAll_OpacityProperty, value); }
        }
        public static readonly DependencyProperty ButtonRefreshAll_OpacityProperty =
            DependencyProperty.Register("ButtonRefreshAll_Opacity", typeof(double), typeof(MainWindow), new UIPropertyMetadata(0.2));

        public int TotalFilesCount
        {
            get { return (int)GetValue(TotalFilesCountProperty); }
            set { SetValue(TotalFilesCountProperty, value); }
        }
        public static readonly DependencyProperty TotalFilesCountProperty =
            DependencyProperty.Register("TotalFilesCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));
        public int EnabledFilesCount
        {
            get { return (int)GetValue(EnabledFilesCountProperty); }
            set { SetValue(EnabledFilesCountProperty, value); }
        }
        public static readonly DependencyProperty EnabledFilesCountProperty =
            DependencyProperty.Register("EnabledFilesCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));
        // ----
        public int TotalJobsCount
        {
            get { return (int)GetValue(TotalJobsCountProperty); }
            set { SetValue(TotalJobsCountProperty, value); }
        }
        public static readonly DependencyProperty TotalJobsCountProperty =
            DependencyProperty.Register("TotalJobsCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));
        public int EnabledJobsCount
        {
            get { return (int)GetValue(EnabledJobsCountProperty); }
            set { SetValue(EnabledJobsCountProperty, value); }
        }
        public static readonly DependencyProperty EnabledJobsCountProperty =
            DependencyProperty.Register("EnabledJobsCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));
        public string ReadyToRunJobsCount
        {
            get { return (string)GetValue(ReadyToRunJobsCountProperty); }
            set { SetValue(ReadyToRunJobsCountProperty, value); }
        }
        public static readonly DependencyProperty ReadyToRunJobsCountProperty =
            DependencyProperty.Register("ReadyToRunJobsCount", typeof(string), typeof(MainWindow), new PropertyMetadata("---"));
        public string DisabledReadyToRunJobsCount
        {
            get { return (string)GetValue(DisabledReadyToRunJobsCountProperty); }
            set { SetValue(DisabledReadyToRunJobsCountProperty, value); }
        }
        public static readonly DependencyProperty DisabledReadyToRunJobsCountProperty =
            DependencyProperty.Register("DisabledReadyToRunJobsCount", typeof(string), typeof(MainWindow), new PropertyMetadata("---"));
        public string JobInformation
        {
            get { return (string)GetValue(JobInformationProperty); }
            set { SetValue(JobInformationProperty, value); }
        }
        public static readonly DependencyProperty JobInformationProperty = DependencyProperty.Register("JobInformation", typeof(string), typeof(MainWindow), new PropertyMetadata());
        public Visibility AltKey_Visibility
        {
            get { return (Visibility)GetValue(AltKey_VisibilityProperty); }
            set { SetValue(AltKey_VisibilityProperty, value); }
        }
        public static readonly DependencyProperty AltKey_VisibilityProperty = DependencyProperty.Register("AltKey_Visibility", typeof(Visibility), typeof(MainWindow), new PropertyMetadata(Visibility.Hidden));
        public string AppId
        {
            get { return (string)GetValue(AppIdProperty); }
            set { SetValue(AppIdProperty, value); }
        }
        public static readonly DependencyProperty AppIdProperty = DependencyProperty.Register("AppId", typeof(string), typeof(MainWindow), new PropertyMetadata(""));
        #endregion
        #region Public Methods
        public IEnumerable<string> AllJobNamesButThisOne(string currentJobName)
        {
            return from x in Jobs
                   where x.Name != currentJobName
                   select x.Name;
        }
        public void SetGlobalPercent(double value, System.Windows.Shell.TaskbarItemProgressState state)
        {
            GlobalPercentValue = value;
            GlobalPercentState = state;
        }
        #endregion
        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppId = App.AppId;
            ApplyUserSettings();
            var task = UpdateAllJobsRecommendedActionIcons();

            if (!_Minimized)
            {
                // show splash screen
                var wind = new AboutWindow(true);
                wind.Owner = Window.GetWindow(this);
                wind.ShowDialog();
            }
            if (_RunAll)
            {
                task.ContinueWith((tt) =>
                {
                    // run all jobs
                    this.Dispatcher.Invoke(() => RunAll(_Shutdown, _Minimized));
                });
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveGlobalSettings();
            SaveUserSettings();
            SaveJobs();
            // ----
            canvasJobDetails.Children.Clear();
            if (_CurrentJob != null)
            {
                _CurrentJob.Shutdown();
                _CurrentJob = null;
            }
            //
            Helper.CleanUpLogger();   // backup for the App.Application_Exit(...) method
        }
        private void listBoxJobs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // logging
            string oldJobName = _CurrentJob?.Job.Name;
            string newJobName = "";
            if ((e.AddedItems.Count > 0) && (e.AddedItems[0] is JobBase)) { newJobName = (e.AddedItems[0] as JobBase).Name; }
            App.WriteDebugLog(nameof(MainWindow), $"Job Selected=({newJobName}), Previous Job=({oldJobName})");
            // 
            SetCreateOrSelectJobTextProperty();
            canvasJobDetails.Children.Clear();
            if (_CurrentJob != null)
            {
                oldJobName = _CurrentJob.Job.Name;
                _CurrentJob.Shutdown();
                _CurrentJob = null;
            }
            if ((e.AddedItems.Count > 0) && (e.AddedItems[0] is JobBase))
            {
                _CurrentJob = new JobDetailsUserControl(e.AddedItems[0] as JobBase,
                                                        _GlobalSettings,
                                                        AllJobNamesButThisOne((e.AddedItems[0] as JobBase).Name),
                                                        new Action<JobBase>(job =>
                                                        {
                                                            // #### editAction
                                                            // this action is performed when the OK button is clicked (edit job)

                                                            var index = Jobs.IndexOf(e.AddedItems[0] as JobBase);
                                                            if (index >= 0)
                                                            {
                                                                App.WriteDebugLog(nameof(MainWindow), $"Editing Job, Job Name= {job.Name}");
                                                                Jobs[index].RecommendedActionIcon = "";
                                                                Jobs[index].RecommendedActionText = "";
                                                                //
                                                                System.Threading.Tasks.Task.Run(() =>
                                                                {
                                                                    JobBase originalJob = null;
                                                                    JobBase newJob = null;
                                                                    Dispatcher.Invoke(() =>
                                                                    {
                                                                        originalJob = Jobs[index].Clone();
                                                                        newJob = job.Clone();
                                                                    });
                                                                    FixReportsAndArchiveNames(originalJob, newJob);
                                                                    //
                                                                    Dispatcher.Invoke(() =>
                                                                    {
                                                                        Jobs[index] = job;
                                                                        var idHolder = Jobs[index].ID;
                                                                        SortJobsInJobsList();
                                                                        SelectJobById(idHolder);
                                                                    });
                                                                    UpdateRecommendedActionIcon(job);
                                                                    Dispatcher.Invoke(() => { UpdateUI(); });
                                                                });
                                                            }
                                                        }),
                                                        new Action<JobBase>(job =>
                                                        {
                                                            // #### deleteAction
                                                            // this action is performed when the DELETE button is clicked (delete job)

                                                            var index = Jobs.IndexOf(e.AddedItems[0] as JobBase);
                                                            if (index >= 0)
                                                            {
                                                                App.WriteDebugLog(nameof(MainWindow), $"Deleting Job {job.Name}");
                                                                Jobs.RemoveAt(index);
                                                                UpdateToolbarButtons();
                                                                RemoveAllJobArtifacts(job);
                                                                UpdateUI();
                                                                App.WriteLog(nameof(App), ((job is MirrorJob) ? App.LogCategory.Mirror : App.LogCategory.Archive), $"{((job is MirrorJob) ? "Mirror" : "Archive")} Job Deleted; " +
                                                                    $"Job Name={job.Name}; Date Created={dodSON.Core.Common.DateTimeHelper.FormatTimeSpan((DateTime.Now - job.DateCreate))} ({job.DateCreate})");
                                                            }
                                                        }),
                                                        new Action<JobBase>(job =>
                                                        {
                                                            // #### enableAction
                                                            // this action is performed when the ENABLED button is clicked (enable/disable job)

                                                            var enableDisable = job.IsEnabled ? "Enabling" : "Disabling";
                                                            App.WriteDebugLog(nameof(MainWindow), $"{enableDisable} Job, Job Name={job.Name}");
                                                            UpdateRecommendedActionIcon(job);
                                                            UpdateUI();
                                                        }),
                                                        new Action<JobBase>(job =>
                                                        {
                                                            // #### pre-refreshAction
                                                            // this action is performed before the REFRESH is serviced

                                                            App.WriteDebugLog(nameof(MainWindow), $"Pre-Refreshing Job, Job Name={job.Name}");
                                                            job.RecommendedActionIcon = "";
                                                            job.RecommendedActionText = "";
                                                        }),
                                                        new Action<JobBase>(job =>
                                                        {
                                                            // #### refreshAction
                                                            // this action is performed when the REFRESH button is clicked 

                                                            App.WriteDebugLog(nameof(MainWindow), $"Refreshing Job, Job Name={job.Name}");
                                                            System.Threading.Tasks.Task.Run(() =>
                                                            {
                                                                UpdateRecommendedActionIcon(job);
                                                                Dispatcher.Invoke(() => { UpdateUI(); });
                                                            });
                                                        }),
                                                        new Action<JobBase>(job =>
                                                        {
                                                            // #### AfterRun
                                                            // this action is performed after the job has been run

                                                            App.WriteDebugLog(nameof(MainWindow), $"Running Job, Job Name= {job.Name}");
                                                            System.Threading.Tasks.Task.Run(() =>
                                                            {
                                                                UpdateRecommendedActionIcon(job);
                                                                Dispatcher.Invoke(() => { UpdateUI(); });
                                                            });
                                                        }),
                                                        () =>
                                                        {
                                                            // #### OnShutdown
                                                            // this action is performed when the job is complete and the shutdown when complete check box has been checked

                                                            App.WriteDebugLog(nameof(MainWindow), $"Run Window Shutdown Requested");
                                                            dodSON.Core.Threading.ThreadingHelper.Sleep(TimeSpan.FromSeconds(3));
                                                            this.Dispatcher.Invoke(new Action(() =>
                                                            {
                                                                if (_CurrentJob != null)
                                                                {
                                                                    _CurrentJob.Shutdown();
                                                                    _CurrentJob = null;
                                                                }
                                                                canvasJobDetails.Children.Clear();
                                                                this.Close();
                                                            }));
                                                        });
                // add job details user control to canvas
                canvasJobDetails.Children.Add(_CurrentJob);
                //
                UpdateJobStatusBarInfo(_CurrentJob?.Job);
            }
            else
            {
                JobInformation = "";
            }
        }
        private void listBoxJobs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _CurrentJob?.EditJob();
            UpdateUI();
        }
        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var converter = new GridLengthConverter();
            Helper.GlobalUserSettings.MainWindowJobButtonsColumnsWidths[0] = converter.ConvertToString(column0.Width);
            Helper.GlobalUserSettings.MainWindowJobButtonsColumnsWidths[1] = converter.ConvertToString(column1.Width);
            Helper.GlobalUserSettings.MainWindowJobButtonsColumnsWidths[2] = converter.ConvertToString(column2.Width);
        }
        //
        private bool IsIn_PKDThread = false;
        private DateTimeOffset IsIn_PKDThread_StartTime;
        private int IsIn_PKDThread_SecondsToWait = 5;
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                // show keyboard shortcuts
                e.Handled = true;
                IsIn_PKDThread_StartTime = DateTimeOffset.Now;
                if (!IsIn_PKDThread)
                {
                    IsIn_PKDThread = true;
                    IsIn_PKDThread_StartTime = DateTimeOffset.Now;
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        // update UI
                        Dispatcher.Invoke(() => { AltKey_Visibility = Visibility.Visible; });
                        while (true)
                        {
                            if (DateTimeOffset.Now > IsIn_PKDThread_StartTime.AddSeconds(IsIn_PKDThread_SecondsToWait))
                            {
                                // timer elapsed
                                break;
                            }
                            dodSON.Core.Threading.ThreadingHelper.Sleep(TimeSpan.FromSeconds(1));
                        }
                        // update UI
                        Dispatcher.Invoke(() => { AltKey_Visibility = Visibility.Hidden; });
                        IsIn_PKDThread = false;
                    });
                }
            }
        }
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1: // #### ABOUT
                    e.Handled = true;
                    var AboutWind = new AboutWindow(false);
                    AboutWind.Owner = Window.GetWindow(this);
                    AboutWind.ShowDialog();
                    break;
                case Key.F2: // #### JOB INFORMATION
                    e.Handled = true;
                    if (_CurrentJob != null)
                    {
                        var jobInfoWind = new JobInformationWindow(_CurrentJob.Job, _GlobalSettings);
                        jobInfoWind.Owner = Window.GetWindow(this);
                        jobInfoWind.ShowDialog();
                    }
                    break;
                case Key.F4: // #### SETTINGS
                    e.Handled = true;
                    ShowSettingsWindow();
                    break;
                case Key.F5:
                    e.Handled = true;
                    if (Keyboard.Modifiers == ModifierKeys.None)
                    {
                        App.WriteDebugLog(nameof(MainWindow), $"Refresh All, Jobs={listBoxJobs.Items.Count:N0}");
                        App.WriteLog(nameof(MainWindow), App.LogCategory.App, $"Refresh All, Jobs={listBoxJobs.Items.Count:N0}");
                        RefreshAll();
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        //// #### RUN ALL
                        //RunAll();

                        // RUN CURRENT
                        RunCurrent();
                    }
                    break;
                case Key.D1: // #### Next Job
                    e.Handled = true;
                    SelectNextJob("all");
                    break;
                case Key.D2: // #### Next Enabled Job
                    e.Handled = true;
                    SelectNextJob("enabled");
                    break;
                case Key.D3: // #### Next Ready To Run Job
                    e.Handled = true;
                    SelectNextJob("ready");
                    break;
                case Key.D4: // #### Next Disabled, Ready To Run
                    e.Handled = true;
                    SelectNextJob("disabledready");
                    break;
                default:
                    break;
            }
        }
        private void SelectNextJob_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (listBoxJobs.Items.Count > 0)
            {
                // initialize
                var tag = (sender as FrameworkElement).Tag.ToString().ToLower();
                SelectNextJob(tag);
            }
        }
        private void SelectNextJob(string tag)
        {
            var newIndex = -1;
            var originalIndex = listBoxJobs.SelectedIndex;
            var workingOriginalIndex = originalIndex;
            var searchIndex = workingOriginalIndex;
            //
            if (tag == "all")
            {
                newIndex = ++searchIndex;
                if (newIndex == listBoxJobs.Items.Count) { newIndex = 0; }
            }
            else
            {
                // search for next TAG by type
                var loopCount = 0;
                do
                {
                    // increment index
                    if (++searchIndex == listBoxJobs.Items.Count) { searchIndex = 0; }
                    // 
                    if (workingOriginalIndex == -1)
                    {
                        // no original job selected
                        if (++loopCount > listBoxJobs.Items.Count)
                        {
                            // done
                            break;
                        }
                    }
                    else
                    {
                        // original job selected
                        if (searchIndex == workingOriginalIndex)
                        {
                            // done, check original job
                            if ((newIndex == -1) && (originalIndex != -1))
                            {
                                if (IsValid(originalIndex, tag, out int textNewIndex))
                                {
                                    newIndex = originalIndex;
                                }
                            }
                            break;
                        }
                    }
                    // test for TAG type
                    if (IsValid(searchIndex, tag, out int candidateNewIndex))
                    {
                        // done, found a match
                        newIndex = candidateNewIndex;
                        break;
                    }
                } while (true);
            }
            // update UI
            listBoxJobs.SelectedIndex = newIndex;


            // ################ INTERNAL FUNCTIONS
            bool IsValid(int searchIndex_, string tag_, out int newIndex_)
            {
                newIndex_ = searchIndex_;
                var isEnabled = (listBoxJobs.Items[searchIndex_] as JobBase).IsEnabled;
                var isReady = (listBoxJobs.Items[searchIndex_] as JobBase).RecommendedActionText.StartsWith("Run", StringComparison.InvariantCultureIgnoreCase) ||
                              (listBoxJobs.Items[searchIndex_] as JobBase).RecommendedActionText.StartsWith("Consider", StringComparison.InvariantCultureIgnoreCase);
                if (tag_ == "ready")
                {
                    if (isEnabled && isReady) { return true; }
                }
                else if (tag_ == "enabled")
                {
                    if (isEnabled) { return true; }
                }
                else if (tag_ == "disabledready")
                {
                    if (!isEnabled && isReady) { return true; }
                }
                newIndex_ = -1;
                return false;
            }
        }
        private void OpenJobInformation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((_CurrentJob != null) && (_CurrentJob.Job != null))
            {
                var wind = new JobInformationWindow(_CurrentJob.Job, _GlobalSettings);
                wind.Owner = Window.GetWindow(this);
                wind.ShowDialog();
            }
        }
        #endregion
        #region Toolbar Button Events
        private void toolBarButtonRefreshAll_Click(object sender, RoutedEventArgs e)
        {
            App.WriteDebugLog(nameof(MainWindow), $"Refresh All, Jobs={listBoxJobs.Items.Count:N0}");
            App.WriteLog(nameof(MainWindow), App.LogCategory.App, $"Refresh All, Jobs={listBoxJobs.Items.Count:N0}");
            RefreshAll();
        }
        private void RefreshAll()
        {
            if (!_IsRefreshing)
            {
                CreateOrSelectJobText = "Working...";
                var dude = listBoxJobs.SelectedIndex;
                listBoxJobs.SelectedIndex = -1;
                UpdateAllJobsRecommendedActionIcons();
                listBoxJobs.SelectedIndex = dude;
            }
        }
        private void toolBarButtonRunAllEnabledJobs_Click(object sender, RoutedEventArgs e)
        {
            RunAll(false, false);
        }
        private void toolBarButtonNewMirrorJob_Click(object sender, RoutedEventArgs e)
        {
            var job = new MirrorJob(string.Format("Job {0}", DateTime.Now.ToString("yyyyMMdd-HHmmss")), "", "");
            var wind = new EditorWindow(job, true, AllJobNamesButThisOne(job.Name));
            wind.Owner = Window.GetWindow(this);
            if (wind.ShowDialog() ?? false)
            {
                Jobs.Add(wind.Job);
                SortJobsInJobsList();
                SelectJobById(wind.Job.ID);
                UpdateRecommendedActionIcon(wind.Job);
                UpdateUI();
                App.WriteLog(nameof(App), 0, $"Mirror Job Created; Name={wind.Job.Name}");
            }
        }
        private void toolBarButtonNewArchiveJob_Click(object sender, RoutedEventArgs e)
        {
            var job = new ArchiveJob(string.Format("Job {0}", DateTime.Now.ToString("yyyyMMdd-HHmmss")), "", null);
            var wind = new EditorWindow(job, true, AllJobNamesButThisOne(job.Name));
            wind.Owner = Window.GetWindow(this);
            if (wind.ShowDialog() ?? false)
            {
                Jobs.Add(wind.Job);
                SortJobsInJobsList();
                SelectJobById(wind.Job.ID);
                UpdateRecommendedActionIcon(wind.Job);
                UpdateUI();
                App.WriteLog(nameof(App), App.LogCategory.Archive, $"Archive Job Created; Name={wind.Job.Name}");
            }
        }
        private void toolBarButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsWindow();
        }
        private void toolBarButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AboutWindow(false);
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
        }
        #endregion
        #region Private Methods

        // GLOBAL SETTINGS

        private void LoadGlobalSettings()
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(DataFilesLoadPath, "settings.dat")))
            {
                // load settings
                App.WriteDebugLog(nameof(MainWindow), $"Loading Global Settings");
                var dude = new dodSON.Core.Converters.BinaryFileTypeSerializer();
                _GlobalSettings = dude.Load<GlobalSettings>(System.IO.Path.Combine(DataFilesLoadPath, "settings.dat"));
                if (string.IsNullOrWhiteSpace(_GlobalSettings.ArchiveZipFileSnapshotsRootPath)) { _GlobalSettings.ArchiveZipFileSnapshotsRootPath = DataFilesLoadPath; }
                if (string.IsNullOrWhiteSpace(_GlobalSettings.AutomaticReportingRootPath)) { _GlobalSettings.AutomaticReportingRootPath = DataFilesLoadPath; }
                if (string.IsNullOrWhiteSpace(_GlobalSettings.RemovedFilesArchiveRootPath)) { _GlobalSettings.RemovedFilesArchiveRootPath = DataFilesLoadPath; }
            }
            else
            {
                // set default settings
                App.WriteDebugLog(nameof(MainWindow), $"Creating Default Global Settings");
                _GlobalSettings.ArchiveZipFileSnapshotsRootPath = System.IO.Path.Combine(DataFilesLoadPath, "Snapshots");
                _GlobalSettings.AutomaticReportingRootPath = DataFilesLoadPath;
                _GlobalSettings.RemovedFilesArchiveRootPath = System.IO.Path.Combine(DataFilesLoadPath, "RemovedArchive");
            }
            // ensure folder exist
            if (!System.IO.Directory.Exists(_GlobalSettings.ArchiveZipFileSnapshotsRootPath)) { System.IO.Directory.CreateDirectory(_GlobalSettings.ArchiveZipFileSnapshotsRootPath); }
            if (!System.IO.Directory.Exists(_GlobalSettings.AutomaticReportingRootPath)) { System.IO.Directory.CreateDirectory(_GlobalSettings.AutomaticReportingRootPath); }
            if (!System.IO.Directory.Exists(_GlobalSettings.RemovedFilesArchiveRootPath)) { System.IO.Directory.CreateDirectory(_GlobalSettings.RemovedFilesArchiveRootPath); }
        }
        private void SaveGlobalSettings()
        {
            // TODO: check for IsDirty flag

            App.WriteDebugLog(nameof(MainWindow), $"Saving Global Settings");
            if (_GlobalSettings.ArchiveZipFileSnapshotsRootPath == DataFilesLoadPath) { _GlobalSettings.ArchiveZipFileSnapshotsRootPath = ""; }
            if (_GlobalSettings.AutomaticReportingRootPath == DataFilesLoadPath) { _GlobalSettings.AutomaticReportingRootPath = ""; }
            if (_GlobalSettings.RemovedFilesArchiveRootPath == DataFilesLoadPath) { _GlobalSettings.RemovedFilesArchiveRootPath = ""; }
            var dude = new dodSON.Core.Converters.BinaryFileTypeSerializer();
            dude.Save<GlobalSettings>(_GlobalSettings, System.IO.Path.Combine(DataFilesLoadPath, "settings.dat"), true);
        }

        // USER SETTINGS

        private void SaveUserSettings()
        {
            // TODO: check for IsDirty flag

            // ---- set mainwindow usersettings
            App.WriteDebugLog(nameof(MainWindow), $"Saving User Settings");
            Helper.GlobalUserSettings.MainWindowLocation = new Pair<double, double>(this.Top, this.Left);
            Helper.GlobalUserSettings.MainWindowSize = new Pair<double, double>(this.Width, this.Height);
            // ----
            var dude = new dodSON.Core.Converters.BinaryFileTypeSerializer();
            dude.Save<UserSettings>(Helper.GlobalUserSettings, System.IO.Path.Combine(DataFilesLoadPath, "usersettings.dat"), true);
        }
        private void LoadUserSettings()
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(DataFilesLoadPath, "usersettings.dat")))
            {
                // load settings
                App.WriteDebugLog(nameof(MainWindow), $"Loading User Settings");
                var dude = new dodSON.Core.Converters.BinaryFileTypeSerializer();
                Helper.GlobalUserSettings = dude.Load<UserSettings>(System.IO.Path.Combine(DataFilesLoadPath, "usersettings.dat"));
            }
            else
            {
                // set mainwindow default settings
                App.WriteDebugLog(nameof(MainWindow), $"Creating Default User Settings");
                Helper.GlobalUserSettings.MainWindowLocation = new Pair<double, double>(this.Top, this.Left);
                Helper.GlobalUserSettings.MainWindowSize = new Pair<double, double>(this.Width, this.Height);
                Helper.GlobalUserSettings.MainWindowJobButtonsColumnsWidths[0] = "300";
                Helper.GlobalUserSettings.MainWindowJobButtonsColumnsWidths[1] = "5";
                Helper.GlobalUserSettings.MainWindowJobButtonsColumnsWidths[2] = "*";
            }
        }
        private void ApplyUserSettings()
        {
            // set the MainWindow position and size
            this.Top = Helper.GlobalUserSettings.MainWindowLocation.Alpha;
            this.Left = Helper.GlobalUserSettings.MainWindowLocation.Beta;
            this.Width = Helper.GlobalUserSettings.MainWindowSize.Alpha;
            this.Height = Helper.GlobalUserSettings.MainWindowSize.Beta;

            // set the 
            var converter = new GridLengthConverter();
            this.column0.Width = (GridLength)converter.ConvertFromString(Helper.GlobalUserSettings.MainWindowJobButtonsColumnsWidths[0]);
            this.column1.Width = (GridLength)converter.ConvertFromString(Helper.GlobalUserSettings.MainWindowJobButtonsColumnsWidths[1]);
            this.column2.Width = (GridLength)converter.ConvertFromString(Helper.GlobalUserSettings.MainWindowJobButtonsColumnsWidths[2]);
        }

        // UTILITY

        private void RemoveAllJobArtifacts(JobBase job)
        {
            // delete all REMOVED FILES ARCHIVES
            foreach (var item in System.IO.Directory.GetFiles(_GlobalSettings.RemovedFilesArchiveRootPath, string.Format("{0}.*", job.Name), System.IO.SearchOption.TopDirectoryOnly))
            {
                System.IO.File.Delete(item);
            }
            // delete all REPORTS
            var store = Helper.GetZipStore(System.IO.Path.Combine(_GlobalSettings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip"), System.IO.Path.GetTempPath(), true, null);
            var items = (from x in store.GetAll()
                         where x.RootFilename.StartsWith(job.Name)
                         select x).ToList();
            foreach (var item in items)
            {
                store.Delete(item.RootFilename);
            }
            store.Save(false);
            // delete all SNAPSHOTS
            if (job is ArchiveJob)
            {
                foreach (var item in System.IO.Directory.GetFiles(_GlobalSettings.ArchiveZipFileSnapshotsRootPath, string.Format("{0}.*", job.Name), System.IO.SearchOption.TopDirectoryOnly))
                {
                    System.IO.File.Delete(item);
                }
            }
        }
        private string DataFilesLoadPath => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private IEnumerable<Helper.RunPack> GetJobPacks
        {
            get
            {
                var packs = new List<Helper.RunPack>();
                foreach (var item in from x in Jobs
                                     where x.IsEnabled
                                     select x)
                {
                    packs.Add(new Helper.RunPack()
                    {
                        ComparisonReport = null,
                        Job = item
                    });
                }
                return packs;
            }
        }
        // this function will rename all reports and archives to match the new job name.
        // job name is the only way to track reports and archives to the proper job.
        private void FixReportsAndArchiveNames(JobBase originalJob, JobBase newJob)
        {
            if (originalJob.Name != newJob.Name)
            {
                // ######## RENAME JOB ########

                // process auto-reports
                var zipFilename = System.IO.Path.Combine(_GlobalSettings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                var list = new List<string>();
                foreach (var item in zip.GetAll().ToArray())
                {
                    var parts = item.RootFilename.Split('.');
                    if ((parts.Length > 1) &&
                        (parts[0] == originalJob.Name) &&
                        (parts[2].Equals("JobReport", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        // extract file
                        var originalFilename = zip.Extract(item, true);
                        list.Add(originalFilename);
                        // delete file
                        zip.Delete(item.RootFilename);
                        // import file
                        var newFilename = newJob.Name;
                        for (int i = 1; i < parts.Length; i++) { newFilename += "." + parts[i]; }
                        zip.Add(newFilename, originalFilename, item.RootFileLastModifiedTimeUtc, item.FileSize);
                    }
                }
                zip.Save(true);
                // clean up
                foreach (var item in list)
                {
                    Helper.DeleteFile(item);
                }
                // process removed files archive
                foreach (var item in System.IO.Directory.GetFiles(_GlobalSettings.RemovedFilesArchiveRootPath, "*.RemovedFiles.zip", System.IO.SearchOption.TopDirectoryOnly))
                {
                    var parts = System.IO.Path.GetFileName(item).Split('.');
                    if ((parts.Count() > 1) && (parts[0] == originalJob.Name))
                    {
                        var newFilename = newJob.Name;
                        for (int i = 1; i < parts.Length; i++) { newFilename += "." + parts[i]; }
                        System.IO.File.Move(item, System.IO.Path.Combine(_GlobalSettings.RemovedFilesArchiveRootPath, newFilename));
                    }
                }
                //
                if (originalJob is ArchiveJob)
                {
                    // process archives
                    foreach (var item in System.IO.Directory.GetFiles((originalJob as ArchiveJob).ArchiveRootPath, string.Format("{0}.*.Archive.zip", originalJob.Name), System.IO.SearchOption.TopDirectoryOnly))
                    {
                        var parts = System.IO.Path.GetFileName(item).Split('.');
                        if ((parts.Count() > 1) && (parts[0] == originalJob.Name))
                        {
                            var newFilename = newJob.Name;
                            for (int i = 1; i < parts.Length; i++) { newFilename += "." + parts[i]; }
                            System.IO.File.Move(item, System.IO.Path.Combine((originalJob as ArchiveJob).ArchiveRootPath, newFilename));
                        }
                    }
                    // process snapshots
                    foreach (var item in System.IO.Directory.GetFiles(_GlobalSettings.ArchiveZipFileSnapshotsRootPath, string.Format("{0}.*.Snapshot.zip", originalJob.Name), System.IO.SearchOption.TopDirectoryOnly))
                    {
                        var parts = System.IO.Path.GetFileName(item).Split('.');
                        if ((parts.Count() > 1) && (parts[0] == originalJob.Name))
                        {
                            var newFilename = newJob.Name;
                            for (int i = 1; i < parts.Length; i++) { newFilename += "." + parts[i]; }
                            System.IO.File.Move(item, System.IO.Path.Combine(_GlobalSettings.ArchiveZipFileSnapshotsRootPath, newFilename));
                        }
                    }
                }
            }
        }
        private void ShowSettingsWindow()
        {
            var wind = new SettingsWindow(_GlobalSettings);
            wind.Owner = Window.GetWindow(this);
            if (wind.ShowDialog() ?? false)
            {
                _GlobalSettings = wind.Settings;
                if (_CurrentJob != null) { SelectJobById(_CurrentJob.Job.ID); }
            }
        }

        // LOAD/SAVE JOBS

        private void SaveJobs()
        {
            // TODO: check for IsDirty flag

            var dudes = (from x in Jobs
                         where x.StateChangeTracker.IsDirty
                         select x);
            var dude = new dodSON.Core.Converters.BinaryFileTypeSerializer();
            if (dudes.Count() > 0)
            {
                App.WriteDebugLog(nameof(MainWindow), $"Saving Jobs");
                dude.Save<System.Collections.ObjectModel.ObservableCollection<JobBase>>(Jobs, System.IO.Path.Combine(DataFilesLoadPath, "jobs.dat"), true);
            }
        }
        private void LoadJobs()
        {
            var dude = new dodSON.Core.Converters.BinaryFileTypeSerializer();
            if (System.IO.File.Exists(System.IO.Path.Combine(DataFilesLoadPath, "jobs.dat")))
            {
                App.WriteDebugLog(nameof(MainWindow), $"Loading Jobs");
                Jobs = dude.Load<System.Collections.ObjectModel.ObservableCollection<JobBase>>(System.IO.Path.Combine(DataFilesLoadPath, "jobs.dat"));
            }
        }

        // UPDATE UI

        private void UpdateUI()
        {
            SetCreateOrSelectJobTextProperty();
            UpdateToolbarButtons();
            UpdateJobsListCounterBar(null, null);
            //
            UpdateJobStatusBarInfo(_CurrentJob?.Job);
        }
        private bool _IsRefreshing = false;
        private System.Threading.Tasks.Task UpdateAllJobsRecommendedActionIcons()
        {
            if (!_IsRefreshing)
            {
                _IsRefreshing = true;
                ButtonRefreshAll_IsEnabled = false;
                ButtonRefreshAll_Opacity = 0.2;
                ButtonRunAllEnabledJob_IsEnabled = false;
                ButtonRunAllEnabledJob_Opacity = 0.2;
                UpdateJobsListCounterBar("---", "---");
                Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.Indeterminate);
                var workList = new List<JobBase>();
                foreach (var job in Jobs)
                {
                    job.RecommendedActionText = "";
                    job.RecommendedActionIcon_WaitIconVisibility = Visibility.Visible;
                    job.RecommendedActionIcon = "";
                    workList.Add(job);
                }
                return System.Threading.Tasks.Task.Run(() =>
                {
                    System.Threading.Tasks.Parallel.ForEach(workList, x => UpdateRecommendedActionIcon(x));
                }).ContinueWith((task) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        UpdateUI();
                        Helper.MainWindow.SetGlobalPercent(0, System.Windows.Shell.TaskbarItemProgressState.None);
                    });
                    //
                    _IsRefreshing = false;
                });
            }
            return null;
        }
        private void UpdateRecommendedActionIcon(JobBase job_)
        {
            var cancelTokenSource = new System.Threading.CancellationTokenSource();
            var report = Helper.GenerateReport(job_, cancelTokenSource.Token);
            //
            if ((!cancelTokenSource.Token.IsCancellationRequested) &&
                (report != null))
            {
                int okCount = 0;
                int newCount = 0;
                int updateCount = 0;
                int removeCount = 0;
                int total = 0;
                int totalChanges = 0;
                okCount = (from x in report
                           where (x.Action == dodSON.Core.FileStorage.CompareAction.Ok)
                           select x).Count();
                newCount = (from x in report
                            where (x.Action == dodSON.Core.FileStorage.CompareAction.New)
                            select x).Count();
                updateCount = (from x in report
                               where (x.Action == dodSON.Core.FileStorage.CompareAction.Update)
                               select x).Count();
                removeCount = (from x in report
                               where (x.Action == dodSON.Core.FileStorage.CompareAction.Remove)
                               select x).Count();
                total = okCount + updateCount + removeCount;
                totalChanges = newCount + updateCount + removeCount;
                //
                job_.RecommendedActionIcon_WaitIconVisibility = Visibility.Hidden;
                job_.RecommendedActionIcon_WaitIconInvertVisibility = Visibility.Visible;
                //
                if (totalChanges == 0)
                {
                    job_.RecommendedActionIcon = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-security-high.ico";
                    job_.RecommendedActionText = "No Action";
                }
                else
                {
                    var timeAgo = (DateTime.Now - job_.DateLastRan);
                    if (job_.DateLastRan == DateTime.MinValue) { timeAgo = (DateTime.Now - job_.DateCreate); }
                    if (timeAgo > _GlobalSettings.RecommendationActionAgeLimit)
                    {
                        job_.RecommendedActionIcon = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-security-low.ico";
                        job_.RecommendedActionText = "Run";
                    }
                    else
                    {
                        job_.RecommendedActionIcon = "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-security-medium.ico";
                        job_.RecommendedActionText = "Consider Running";
                    }
                }
            }
        }

        //----
        private void SetCreateOrSelectJobTextProperty()
        {
            if (listBoxJobs.Items.Count == 0) { CreateOrSelectJobText = "Create a New Job"; }
            else { CreateOrSelectJobText = "Create or Select a Job"; }
        }
        private void UpdateToolbarButtons()
        {
            // refresh all button
            ButtonRefreshAll_IsEnabled = false;
            ButtonRefreshAll_Opacity = 0.2;
            if (Jobs.Count > 0)
            {
                ButtonRefreshAll_IsEnabled = true;
                ButtonRefreshAll_Opacity = 1;
            }
            // run all button
            ButtonRunAllEnabledJob_IsEnabled = false;
            ButtonRunAllEnabledJob_Opacity = 0.2;
            foreach (var item in Jobs)
            {
                if (item.IsEnabled && (item.RecommendedActionText.StartsWith("Run", StringComparison.InvariantCultureIgnoreCase) ||
                                       item.RecommendedActionText.StartsWith("Consider", StringComparison.InvariantCultureIgnoreCase)))
                {
                    ButtonRunAllEnabledJob_IsEnabled = true;
                    ButtonRunAllEnabledJob_Opacity = 1;
                    break;
                }
            }
        }
        private void UpdateJobsListCounterBar(string readyToRunText, string couldBeRunText)
        {
            TotalJobsCount = Jobs.Count;
            EnabledJobsCount = (from x in Jobs
                                where x.IsEnabled
                                select x).Count();
            // READY TO RUN
            if (!string.IsNullOrWhiteSpace(readyToRunText))
            {
                ReadyToRunJobsCount = readyToRunText;
            }
            else
            {

                // FIX: the just added job has it's "RecommenedActionText" set to NULL

                var count = (from x in Jobs
                             where x.IsEnabled &&
                                   (x.RecommendedActionText.StartsWith("Run", StringComparison.InvariantCultureIgnoreCase) ||
                                    x.RecommendedActionText.StartsWith("Consider", StringComparison.InvariantCultureIgnoreCase))
                             select x).Count();
                ReadyToRunJobsCount = $"{count}";
            }
            // DISABLED READY TO RUN
            if (!string.IsNullOrWhiteSpace(couldBeRunText))
            {
                DisabledReadyToRunJobsCount = couldBeRunText;
            }
            else
            {
                var count = (from x in Jobs
                             where (!x.IsEnabled) &&
                                   (x.RecommendedActionText.StartsWith("Run", StringComparison.InvariantCultureIgnoreCase) ||
                                    x.RecommendedActionText.StartsWith("Consider", StringComparison.InvariantCultureIgnoreCase))
                             select x).Count();
                DisabledReadyToRunJobsCount = $"{count}";
            }
        }
        private System.Threading.Tasks.Task _UpdateStatusBarTask;
        private System.Threading.CancellationTokenSource _UpdateStatusBarCancelTokenSource = new System.Threading.CancellationTokenSource();
        private void UpdateJobStatusBarInfo(JobBase job)
        {
            if (_UpdateStatusBarTask != null)
            {
                if (!_UpdateStatusBarTask.IsCompleted)
                {
                    _UpdateStatusBarCancelTokenSource.Cancel();
                    _UpdateStatusBarCancelTokenSource = new System.Threading.CancellationTokenSource();
                }
            }
            //
            var token = _UpdateStatusBarCancelTokenSource.Token;
            _UpdateStatusBarTask = System.Threading.Tasks.Task.Run(() =>
            {
                Dispatcher.Invoke(() => { JobInformation = ""; });
                if (job != null)
                {
                    string result;
                    string path;
                    string prefix;
                    string spaces = "        ";
                    //
                    long totalBytes = 0;
                    long totalFiles = 0;
                    long totalDirectories = 0;
                    if (job is MirrorJob)
                    {
                        // #### MIRROR
                        path = (job as MirrorJob).MirrorPath;
                        prefix = $"{job.Name}:{spaces}Mirrored";
                        //
                        if (System.IO.Directory.Exists(path))
                        {
                            // count directories
                            foreach (var pathName in System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories))
                            {
                                if (token.IsCancellationRequested) { return; }
                                ++totalDirectories;
                            }
                            // count files
                            foreach (var filename in System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories))
                            {
                                if (token.IsCancellationRequested) { return; }
                                ++totalFiles;
                                totalBytes += (new System.IO.FileInfo(filename)).Length;
                            }
                            result = $"{prefix} {totalFiles:N0} files in {totalDirectories:N0} directories, {dodSON.Core.Common.ByteCountHelper.ToString(totalBytes)}   ({totalBytes:N0} bytes)";
                        }
                        else
                        {
                            result = $"{prefix}   (no directories or files)";
                        }
                    }
                    else
                    {
                        // #### ARCHIVE
                        path = (job as ArchiveJob).ArchiveRootPath;
                        prefix = $"{job.Name}:{spaces}Archived";
                        //
                        if (System.IO.Directory.Exists(path))
                        {
                            foreach (var filename in System.IO.Directory.GetFiles(path, "*.Archive.zip", System.IO.SearchOption.TopDirectoryOnly))
                            {
                                if (token.IsCancellationRequested) { return; }
                                ++totalFiles;
                                totalBytes += (new System.IO.FileInfo(filename)).Length;
                            }
                            result = $"{prefix} {totalFiles:N0} archives, {dodSON.Core.Common.ByteCountHelper.ToString(totalBytes)}   ({totalBytes:N0} bytes)";
                        }
                        else
                        {
                            result = $"{prefix}   (no archives)";
                        }
                    }
                    // update UI
                    if (token.IsCancellationRequested) { return; }
                    Dispatcher.Invoke(() => { JobInformation = result; });
                }
            }, token);
        }

        // JOBS LIST-RELATED

        private void SelectJobById(string id)
        {
            listBoxJobs.SelectedIndex = -1;
            for (int i = 0; i < listBoxJobs.Items.Count; i++)
            {
                if (listBoxJobs.Items[i] is JobBase item)
                {
                    if (item.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        listBoxJobs.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
        private void SortJobsInJobsList()
        {
            var archives = new SortedList<string, JobBase>();
            var mirrors = new SortedList<string, JobBase>();
            for (int i = 0; i < Jobs.Count; i++)
            {
                if (Jobs[i] is ArchiveJob) { archives.Add(((ArchiveJob)Jobs[i]).Name, Jobs[i]); }
                else if (Jobs[i] is MirrorJob) { mirrors.Add(((MirrorJob)Jobs[i]).Name, Jobs[i]); }
            }
            var result = new System.Collections.ObjectModel.ObservableCollection<JobBase>();
            foreach (var item in archives) { result.Add(item.Value); }
            foreach (var item in mirrors) { result.Add(item.Value); }
            Jobs = result;
        }
        private void RunAll(bool autoShutdown, bool minimized)
        {
            var sWatch = System.Diagnostics.Stopwatch.StartNew();
            App.WriteLog(nameof(App), App.LogCategory.App, $"Run All Started; Jobs Ready To Run={ReadyToRunJobsCount}; Disabled Jobs Ready To Run={DisabledReadyToRunJobsCount}; Jobs Total={TotalJobsCount}");
            //
            UpdateJobsListCounterBar("---", "---");
            var idHolder = (_CurrentJob != null) ? _CurrentJob.JobPack.Job.ID : "";
            var wind = new RunWindow(_GlobalSettings,
                                     autoShutdown,
                                     minimized,
                                     GetJobPacks,
                                     () =>
                                     {
                                         // this action is performed when the All Job have been ran AND the check box (Shutdown when complete) is checked
                                         // this is a pass-through from the Run window
                                         dodSON.Core.Threading.ThreadingHelper.Sleep(TimeSpan.FromSeconds(3));
                                         this.Dispatcher.Invoke(new Action(() => { this.Close(); }));
                                     });
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
            if (!wind.IsShuttingDown)
            {
                var dude = listBoxJobs.SelectedIndex;
                listBoxJobs.SelectedIndex = -1;
                UpdateAllJobsRecommendedActionIcons();
                listBoxJobs.SelectedIndex = dude;
                //
                sWatch.Stop();
                App.WriteLog(nameof(App), App.LogCategory.App, $"Run All Completed; Jobs Processed={wind.JobsProcessedCount}; Elapsed Time={dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(sWatch.Elapsed)}");
            }
        }
        private void RunCurrent()
        {
            if (_CurrentJob == null) { return; }
            //
            var sWatch = System.Diagnostics.Stopwatch.StartNew();
            var jobName = _CurrentJob.Job.Name;
            App.WriteLog(nameof(App), App.LogCategory.App, $"Run Job Started; Job={jobName}; Jobs Ready To Run={ReadyToRunJobsCount}; Disabled Jobs Ready To Run={DisabledReadyToRunJobsCount}; Jobs Total={TotalJobsCount}");
            //
            UpdateJobsListCounterBar("---", "---");
            var idHolder = (_CurrentJob != null) ? _CurrentJob.JobPack.Job.ID : "";
            var wind = new RunWindow(_GlobalSettings,
                                     false,
                                     false,
                                     new Helper.RunPack[]
                                     {
                                         new Helper.RunPack() { ComparisonReport = null, Job = _CurrentJob.Job }
                                     },
                                     () =>
                                     {
                                         // this action is performed when the Job have been ran AND the check box (Shutdown when complete) is checked
                                         // this is a pass-through from the Run window
                                         dodSON.Core.Threading.ThreadingHelper.Sleep(TimeSpan.FromSeconds(3));
                                         this.Dispatcher.Invoke(new Action(() => { this.Close(); }));
                                     });
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
            if (!wind.IsShuttingDown)
            {
                var dude = listBoxJobs.SelectedIndex;
                listBoxJobs.SelectedIndex = -1;
                UpdateAllJobsRecommendedActionIcons();
                listBoxJobs.SelectedIndex = dude;
                //
                sWatch.Stop();
                App.WriteLog(nameof(App), App.LogCategory.App, $"Run Job Completed; Job={jobName}; Elapsed Time={dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(sWatch.Elapsed)}");
            }
        }
        #endregion

        #region Unused Code
        //private IEnumerable<dodSON.Core.FileStorage.ICompareResult> GenerateReport(JobBase workerJob, System.Threading.CancellationToken token)
        //{
        //    if (workerJob is MirrorJob)
        //    {
        //        // **** MIRROR JOB
        //        return dodSON.Core.FileStorage.FileStorageHelper.Compare(((MirrorJob)workerJob).SourcePath,
        //                                                                         ((MirrorJob)workerJob).MirrorPath,
        //                                                                         token,
        //                                                                         (x) => { });
        //    }
        //    // **** ARCHIVE JOB
        //    var job = (ArchiveJob)workerJob;
        //    var results = new List<dodSON.Core.FileStorage.ICompareResult>();
        //    double total = job.SourcePaths.Count;
        //    double count = 0;
        //    foreach (var sourcePath in job.SourcePaths)
        //    {
        //        if (token.IsCancellationRequested) { return null; }
        //        var filename = Helper.ConvertPathToFilename(sourcePath);
        //        var archiveStoreFilename = System.IO.Path.Combine(job.ArchiveRootPath, filename);
        //        if (System.IO.Directory.Exists(sourcePath))
        //        {
        //            var archiveStore = Helper.GetZipStore(archiveStoreFilename, System.IO.Path.GetTempPath(), false, null);
        //            var list = dodSON.Core.FileStorage.FileStorageHelper.Compare(sourcePath,
        //                                                                                 archiveStore,
        //                                                                                 token,
        //                                                                                 (x) =>
        //                                                                                 { });
        //            if (list != null) { results.AddRange(list); }
        //        }
        //        if (token.IsCancellationRequested) { return null; }
        //        count++;
        //    }
        //    return results;
        //}
        #endregion
    }
}
