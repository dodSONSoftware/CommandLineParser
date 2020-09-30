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
    /// Interaction logic for DeleteJobWindow.xaml
    /// </summary>
    public partial class DeleteJobWindow : Window
    {
        #region Ctor
        private DeleteJobWindow()
        {
            InitializeComponent();
            DataContext = this;
            Title = Helper.FormatTitle("Delete Job");
        }
        public DeleteJobWindow(JobBase job,
                               GlobalSettings settings)
            : this()
        {
            Job = job ?? throw new ArgumentNullException(nameof(job));
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            App.WriteDebugLog(nameof(DeleteJobWindow), $"Opening Delete Job Window, Job={Job.Name}");
        }
        #endregion
        #region Private Fields
        private readonly GlobalSettings _Settings = null;
        private readonly System.Threading.CancellationTokenSource _CancelTokenSource = new System.Threading.CancellationTokenSource();
        private readonly System.Threading.CancellationTokenSource _UIUpdateThreadCancelTokenSource = new System.Threading.CancellationTokenSource();
        #endregion
        #region Public Properties
        public bool FilesDeleted { get; private set; } = false;
        #endregion
        #region Dependency Properties
        public JobBase Job
        {
            get { return (JobBase)GetValue(JobProperty); }
            set { SetValue(JobProperty, value); }
        }
        public static readonly DependencyProperty JobProperty = DependencyProperty.Register("Job", typeof(JobBase), typeof(DeleteJobWindow), new PropertyMetadata(null));
        public DeletableItems<dynamic> Reports
        {
            get { return (DeletableItems<dynamic>)GetValue(ReportsProperty); }
            set { SetValue(ReportsProperty, value); }
        }
        public static readonly DependencyProperty ReportsProperty = DependencyProperty.Register("Reports", typeof(DeletableItems<dynamic>), typeof(DeleteJobWindow), new PropertyMetadata(null));
        public DeletableItems<dynamic> RemovedArchives
        {
            get { return (DeletableItems<dynamic>)GetValue(RemovedArchivesProperty); }
            set { SetValue(RemovedArchivesProperty, value); }
        }
        public static readonly DependencyProperty RemovedArchivesProperty = DependencyProperty.Register("RemovedArchives", typeof(DeletableItems<dynamic>), typeof(DeleteJobWindow), new PropertyMetadata(null));
        public DeletableItems<dynamic> Snapshots
        {
            get { return (DeletableItems<dynamic>)GetValue(SnapshotsProperty); }
            set { SetValue(SnapshotsProperty, value); }
        }
        public static readonly DependencyProperty SnapshotsProperty = DependencyProperty.Register("Snapshots", typeof(DeletableItems<dynamic>), typeof(DeleteJobWindow), new PropertyMetadata(null));
        public string JobFilesSizeStr
        {
            get { return (string)GetValue(JobFilesSizeStrProperty); }
            set { SetValue(JobFilesSizeStrProperty, value); }
        }
        public static readonly DependencyProperty JobFilesSizeStrProperty = DependencyProperty.Register("JobFilesSizeStr", typeof(string), typeof(DeleteJobWindow), new PropertyMetadata(""));
        public string JobTypeTitle
        {
            get { return (string)GetValue(JobTypeTitleProperty); }
            set { SetValue(JobTypeTitleProperty, value); }
        }
        public static readonly DependencyProperty JobTypeTitleProperty = DependencyProperty.Register("JobTypeTitle", typeof(string), typeof(DeleteJobWindow), new PropertyMetadata("Delete Title"));
        public Visibility HasSnapshotsVisibility
        {
            get { return (Visibility)GetValue(HasSnapshotsVisibilityProperty); }
            set { SetValue(HasSnapshotsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty HasSnapshotsVisibilityProperty =
            DependencyProperty.Register("HasSnapshotsVisibility", typeof(Visibility), typeof(DeleteJobWindow), new PropertyMetadata(Visibility.Visible));
        // ----
        public Visibility ImageWorkingIconVisiblity
        {
            get { return (Visibility)GetValue(ImageWorkingIconVisiblityProperty); }
            set { SetValue(ImageWorkingIconVisiblityProperty, value); }
        }
        public static readonly DependencyProperty ImageWorkingIconVisiblityProperty = DependencyProperty.Register("ImageWorkingIconVisiblity", typeof(Visibility), typeof(DeleteJobWindow), new PropertyMetadata(Visibility.Visible));
        public Visibility ImageWorkingIconReverseVisiblity
        {
            get { return (Visibility)GetValue(ImageWorkingIconReverseVisiblityProperty); }
            set { SetValue(ImageWorkingIconReverseVisiblityProperty, value); }
        }
        public static readonly DependencyProperty ImageWorkingIconReverseVisiblityProperty = DependencyProperty.Register("ImageWorkingIconReverseVisiblity", typeof(Visibility), typeof(DeleteJobWindow), new PropertyMetadata(Visibility.Hidden));
        // ----
        #endregion
        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // #### update UI
            // show wait icon
            ImageWorkingIconVisiblity = Visibility.Visible;
            ImageWorkingIconReverseVisiblity = Visibility.Hidden;
            //
            if (Job is MirrorJob)
            {
                JobTypeTitle = "Mirror";
                HasSnapshotsVisibility = Visibility.Collapsed;
                MinHeight = 450;
                Height = 450;
            }
            else
            {
                JobTypeTitle = "Archive";
                HasSnapshotsVisibility = Visibility.Visible;
            }
            // #### 
            var jobName = Job.Name;
            Task.Run(() =>
            {
                // gather information
                UpdateJobDeletables();
                if (!_CancelTokenSource.IsCancellationRequested)
                {
                    // update UI
                    Dispatcher.Invoke(() =>
                    {
                        // hide wait icon
                        ImageWorkingIconVisiblity = Visibility.Hidden;
                        ImageWorkingIconReverseVisiblity = Visibility.Visible;
                    });
                    // start thread worker to update UI
                    App.WriteDebugLog(nameof(DeleteJobWindow), $"Starting Background Task for {nameof(DeleteJobWindow)}:{jobName}");
                    Helper.StartBackgroundTask(() =>
                    {
                        while (true)
                        {
                            if (_UIUpdateThreadCancelTokenSource.IsCancellationRequested)
                            {
                                App.WriteDebugLog(nameof(DeleteJobWindow), $"Stopping Background Task for {nameof(DeleteJobWindow)}:{jobName}");
                                break;
                            }
                            Dispatcher.Invoke(() =>
                            {
                                var dude = Job;
                                Job = null;
                                Job = dude;
                            });
                            dodSON.Core.Threading.ThreadingHelper.Sleep(1000);
                        }
                    });
                }
            });
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            _CancelTokenSource.Cancel();
            _UIUpdateThreadCancelTokenSource.Cancel();
        }
        #endregion
        #region Commands
        public ICommand CloseCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                this.DialogResult = false;
                Close();
            });
        public ICommand BrowseReportsCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                var zipFilename = "";
                try
                {
                    zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                    System.Diagnostics.Process.Start(zipFilename);
                }
                catch
                {
                    MainWindow.DisplayErrorMessageBox("File not Found", $"Cannot find Destination File:{Environment.NewLine}{zipFilename}");
                }
            },
            (x) =>
            {
                // can execute
                var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                return System.IO.File.Exists(zipFilename);
            });
        public ICommand BrowseRemovedArchivesCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                try
                {
                    System.Diagnostics.Process.Start(_Settings.RemovedFilesArchiveRootPath);
                }
                catch
                {
                    MainWindow.DisplayErrorMessageBox("Path not Found", $"Cannot find Destination Path:{Environment.NewLine}{_Settings.AutomaticReportingRootPath}");
                }
            },
            (x) =>
            {
                // can execute
                return System.IO.Directory.Exists(_Settings.RemovedFilesArchiveRootPath);
            });
        public ICommand BrowseSnapshotsCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                try
                {
                    System.Diagnostics.Process.Start(_Settings.ArchiveZipFileSnapshotsRootPath);
                }
                catch
                {
                    MainWindow.DisplayErrorMessageBox("Path not Found", $"Cannot find Destination Path:{Environment.NewLine}{_Settings.ArchiveZipFileSnapshotsRootPath}");
                }
            },
            (x) =>
            {
                // can execute
                return System.IO.Directory.Exists(_Settings.ArchiveZipFileSnapshotsRootPath);
            });
        public ICommand BrowseJobInfoCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                var path = "";
                if (Job is MirrorJob) { path = (Job as MirrorJob).MirrorPath; }
                else { path = (Job as ArchiveJob).ArchiveRootPath; }
                try
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                }
                catch
                {
                    MainWindow.DisplayErrorMessageBox("Path not Found", $"Cannot find Destination Path:{Environment.NewLine}{path}");
                }
            },
            (x) =>
            {
                // can execute
                var path = "";
                if (Job is MirrorJob) { path = (Job as MirrorJob).MirrorPath; }
                else { path = (Job as ArchiveJob).ArchiveRootPath; }
                return System.IO.Directory.Exists(path);
            });
        // ----------
        public ICommand DeleteReportsCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                if (MessageBox.Show(GetWindow(this), $"Delete {Job.Name} Reports?{Environment.NewLine}{Environment.NewLine}Are You Sure?", "Delete Reports", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    App.WriteLog(nameof(DeleteJobWindow), App.LogCategory.App, $"Deleted Reports, Job Name={Job.Name}");
                    App.WriteDebugLog(nameof(DeleteJobWindow), $"Deleted Reports, Job Name={Job.Name}");
                    DeleteReports();
                    UpdateJobDeletables();
                }
            },
            (x) =>
            {
                // can execute
                return Reports?.ListCount > 0;
            });
        public ICommand DeleteRemovedArchivesCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                if (MessageBox.Show(GetWindow(this), $"Delete {Job.Name} Removed Archives?{Environment.NewLine}{Environment.NewLine}Are You Sure?", "Delete Removed Archives", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    App.WriteLog(nameof(DeleteJobWindow), App.LogCategory.App, $"Deleted Removed Archives, Job Name={Job.Name}");
                    App.WriteDebugLog(nameof(DeleteJobWindow), $"Deleted Removed Archives, Job Name={Job.Name}");
                    DeleteRemovedArchives();
                    UpdateJobDeletables();
                }
            },
            (x) =>
            {
                // can execute
                return RemovedArchives?.ListCount > 0;
            });
        public ICommand DeleteSnapshotsCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                if (MessageBox.Show(GetWindow(this), $"Delete {Job.Name} Snapshots?{Environment.NewLine}{Environment.NewLine}Are You Sure?", "Delete Snapshots", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    DeleteSnapshots();
                    UpdateJobDeletables();
                }
            },
            (x) =>
            {
                // can execute
                return Snapshots?.ListCount > 0;
            });
        public ICommand DeleteJobInfoCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                if (MessageBox.Show(GetWindow(this), $"Delete {Job.Name} Files?{Environment.NewLine}{Environment.NewLine}Are You Sure?", "Delete Job Files", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    App.WriteLog(nameof(DeleteJobWindow), App.LogCategory.App, $"Deleted Job Files, Job Name={Job.Name}");
                    App.WriteDebugLog(nameof(DeleteJobWindow), $"Deleted Job Files, Job Name={Job.Name}");
                    DeleteJobInfo();
                    UpdateJobDeletables();
                }
            },
            (x) =>
            {
                // can execute
                return CanExecuteDeleteJobInfo;
            });
        public ICommand DeleteAllCommand => new DelegateCommand(
            (x) =>
            {
                // execute
                var message = $"Delete {Job.Name}?{Environment.NewLine}" +
                              $"{Environment.NewLine}This will delete the Job and all Job Artifacts." +
                              $"{Environment.NewLine}" +
                              $"{Environment.NewLine}Including:" +
                              $"{Environment.NewLine}\t> Reports" +
                              $"{Environment.NewLine}\t> Removed Archives" +
                              $"{Environment.NewLine}\t> Snapshots" +
                              $"{Environment.NewLine}\t> All Job Files" +
                              $"{Environment.NewLine}" +
                              $"{Environment.NewLine}The Job will no longer exist." +
                              $"{Environment.NewLine}This action cannot be undone." +
                              $"{Environment.NewLine}{Environment.NewLine}Are You Sure?";
                if (MessageBox.Show(GetWindow(this), message, "Delete Job and Job Artifacts", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    App.WriteLog(nameof(DeleteJobWindow), App.LogCategory.App, $"Deleted Job, Job Name={Job.Name}");
                    App.WriteDebugLog(nameof(DeleteJobWindow), $"Deleted Job, Job Name={Job.Name}");
                    DeleteReports();
                    DeleteRemovedArchives();
                    DeleteSnapshots();
                    DeleteJobInfo();
                    this.DialogResult = true;
                    Close();
                }
            },
            (x) =>
            {
                // can execute
                return true;
            });
        #endregion
        #region Private Methods

        // ----------
        // ---------- see: JobInformationWindow
        // ----------

        private void UpdateJobDeletables()
        {
            // clone job 
            JobBase job = null;
            Dispatcher.Invoke(() => { job = Job.Clone(); });
            // gather information
            var reports = FindReports(job);
            var removedArchives = FindRemovedArchives(job);
            var snapshots = FindSnapshots(job);
            var jobFilesSizeStr = FindJobInfo(job);
            // update UI
            Dispatcher.Invoke(() =>
            {
                Reports = reports;
                RemovedArchives = removedArchives;
                Snapshots = snapshots;
                JobFilesSizeStr = jobFilesSizeStr;
                // FYI: this appears to cause the UI to refresh
                DataContext = null;
                DataContext = this;
            });
        }
        private dynamic FindReports(JobBase job)
        {
            var firstDateTime = DateTimeOffset.MaxValue;
            var lastDateTime = DateTimeOffset.MinValue;
            var list = new List<ReportInfo>();
            object zipLock = new object();
            // 
            if (System.IO.Directory.Exists(_Settings.AutomaticReportingRootPath))
            {
                // collect all reports for this job
                var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                zip.ForEach((item) =>
                {
                    if (item.RootFilename.StartsWith(job.Name + "."))
                    {
                        list.Add(new ReportInfo()
                        {
                            Name = ParseFilenameTitle(item.RootFilename),
                            DateCreated = ParseFilenameDate(item.RootFilename),
                            SortableDateCreated = ParseSortableFilenameDate(item.RootFilename),
                            Filename = item.RootFilename,
                            ReportName = string.Format($"{ParseFilenameTitle(item.RootFilename)}.JobReport.txt"),
                            TotalRows = "---",
                            PercentChanged = "---"
                        });
                    }
                });
                // 
                System.Threading.Tasks.Parallel.ForEach<ReportInfo>(list, (x) =>
                {
                    if (!_CancelTokenSource.Token.IsCancellationRequested)
                    {
                        string filename = "";
                        Helper.ReportFileStatistics stats = null;
                        try
                        {
                            // extract file
                            lock (zipLock)
                            {
                                zip.Extract(new dodSON.Core.FileStorage.IFileStoreItem[] { zip.Get(x.Filename) }, true);
                            }
                            // get a store extracted filename
                            filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), x.Filename);
                            // process report
                            stats = ProcessReportHeader(filename);
                            if (stats != null)
                            {
                                if (stats.ReportDate < firstDateTime) { firstDateTime = stats.ReportDate; }
                                if (stats.ReportDate > lastDateTime) { lastDateTime = stats.ReportDate; }
                            }
                        }
                        finally
                        {
                            // attempt to delete extracted file
                            Helper.DeleteFile(filename);
                        }
                    }
                });
            }
            //
            var dateRangeValue = "";
            if (list.Count == 1) { dateRangeValue = $"({firstDateTime.ToString("g")})"; }
            else if (list.Count > 1) { dateRangeValue = $"{dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(lastDateTime - firstDateTime)}   ({firstDateTime.ToString("d")} to {lastDateTime.ToString("d")})"; }
            return new DeletableItems<object>()
            {
                ItemType = DeletableItemsType.Reports,
                Name = job.Name,
                FirstDate = firstDateTime,
                LastDate = lastDateTime,
                DateRange = dateRangeValue,
                List = list
            };
        }
        private dynamic FindRemovedArchives(JobBase job)
        {
            var firstDateTime = DateTimeOffset.MaxValue;
            var lastDateTime = DateTimeOffset.MinValue;
            var list = new List<dynamic>();
            object totalLengthLock = new object();
            long totalLength = 0;
            // 
            if (System.IO.Directory.Exists(_Settings.RemovedFilesArchiveRootPath))
            {
                Parallel.ForEach(System.IO.Directory.GetFiles(_Settings.RemovedFilesArchiveRootPath,
                                                              $"{job.Name}.*.RemovedFiles.zip"),
                                                              (filename) =>
                                                              {
                                                                  var fInfo = new System.IO.FileInfo(filename);
                                                                  lock (totalLengthLock)
                                                                  {
                                                                      totalLength += fInfo.Length;
                                                                      if (fInfo.LastAccessTime < firstDateTime) { firstDateTime = fInfo.LastAccessTime; }
                                                                      if (fInfo.LastAccessTime > lastDateTime) { lastDateTime = fInfo.LastAccessTime; }
                                                                      //
                                                                      list.Add(new { filename, fInfo.Length });
                                                                  }
                                                              });
            }
            //
            var dateRangeValue = "";
            if (list.Count == 1) { dateRangeValue = $"({firstDateTime.ToString("g")})"; }
            else if (list.Count > 1) { dateRangeValue = $"{dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(lastDateTime - firstDateTime)}   ({firstDateTime.ToString("d")} to {lastDateTime.ToString("d")})"; }
            return new DeletableItems<dynamic>()
            {
                ItemType = DeletableItemsType.RemovedArchives,
                Name = job.Name,
                FirstDate = firstDateTime,
                LastDate = lastDateTime,
                DateRange = dateRangeValue,
                List = list,
                TotalLength = totalLength
            };
        }
        private dynamic FindSnapshots(JobBase job)
        {
            var dude = Helper.GetSnapshotData(_Settings, searchJobName: job.Name);
            if (dude.Count > 0)
            {
                var snapshot = dude.FirstOrDefault().Value;
                //
                var sessionCount = 0;
                var totalFilesCount = 0;
                foreach (var session in snapshot.Sessions)
                {
                    ++sessionCount;
                    foreach (var file in session.Value.Files)
                    {
                        ++totalFilesCount;
                    }
                }
                return new DeletableItems<dynamic>()
                {
                    ItemType = DeletableItemsType.Snapshots,
                    Name = job.Name,
                    FirstDate = snapshot.OldestSessionDate,
                    LastDate = snapshot.NewestSessionDate,
                    DateRange = (sessionCount == 0) ? "" :
                                (sessionCount == 1) ? $"({snapshot.Sessions.First().Value.CreatedDate:g})" :
                                $"{snapshot.AgeString}   ({snapshot.NewestSessionDate:d} to {snapshot.OldestSessionDate:d})",
                    List = snapshot.Sessions.Values,
                    SessionsCountString = $"{sessionCount} sessions, {totalFilesCount} archives",
                    TotalLength = snapshot.CompressedBytes
                };
            }
            return new DeletableItems<dynamic>()
            {
                ItemType = DeletableItemsType.Snapshots,
                Name = job.Name,
                FirstDate = DateTimeOffset.MinValue,
                LastDate = DateTimeOffset.MinValue,
                DateRange = "",
                List = null,
                SessionsCountString = $"0 sessions, 0 archives",
                TotalLength = 0
            };
        }

        //private dynamic FindSnapshots(JobBase job)
        //{
        //    var firstDateTime = DateTimeOffset.MaxValue;
        //    var lastDateTime = DateTimeOffset.MinValue;
        //    var list = new List<dynamic>();
        //    object totalLengthLock = new object();
        //    long totalLength = 0;
        //    // 
        //    if (System.IO.Directory.Exists(_Settings.ArchiveZipFileSnapshotsRootPath))
        //    {
        //        Parallel.ForEach(System.IO.Directory.GetFiles(_Settings.ArchiveZipFileSnapshotsRootPath,
        //                                                      $"{job.Name}.*.Snapshot.zip"),
        //                                                      (filename) =>
        //                                                      {
        //                                                          var fInfo = new System.IO.FileInfo(filename);
        //                                                          lock (totalLengthLock)
        //                                                          {
        //                                                              totalLength += fInfo.Length;
        //                                                              if (fInfo.LastAccessTime < firstDateTime) { firstDateTime = fInfo.LastAccessTime; }
        //                                                              if (fInfo.LastAccessTime > lastDateTime) { lastDateTime = fInfo.LastAccessTime; }
        //                                                              //
        //                                                              list.Add(new { filename, fInfo.Length });
        //                                                          }
        //                                                      });
        //    }
        //    //
        //    var dateRangeValue = "";
        //    if (list.Count == 1) { dateRangeValue = $"({firstDateTime.ToString("g")})"; }
        //    else if (list.Count > 1) { dateRangeValue = $"{dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(lastDateTime - firstDateTime)}   ({firstDateTime.ToString("d")} to {lastDateTime.ToString("d")})"; }
        //    return new DeletableItems<dynamic>()
        //    {
        //        ItemType = DeletableItemsType.Snapshots,
        //        Name = job.Name,
        //        FirstDate = firstDateTime,
        //        LastDate = lastDateTime,
        //        DateRange = dateRangeValue,
        //        List = list,
        //        TotalLength = totalLength
        //    };
        //}
        private string FindJobInfo(JobBase job)
        {
            string result = "";
            string path;
            if (job is MirrorJob) { path = (job as MirrorJob).MirrorPath; }
            else { path = (job as ArchiveJob).ArchiveRootPath; }
            //
            long totalBytes = 0;
            long totalFiles = 0;
            long totalDirectories = 0;
            if (System.IO.Directory.Exists(path))
            {
                if (job is MirrorJob)
                {
                    // count directories
                    System.Threading.Tasks.Parallel.ForEach(System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories), (filename) =>
                    {
                        System.Threading.Interlocked.Increment(ref totalDirectories);
                    });
                    // count files
                    System.Threading.Tasks.Parallel.ForEach(System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories), (filename) =>
                    {
                        System.Threading.Interlocked.Increment(ref totalFiles);
                        System.Threading.Interlocked.Add(ref totalBytes, (new System.IO.FileInfo(filename)).Length);
                    });
                    if ((totalDirectories == 0) && (totalFiles == 0))
                    {
                        result = $"no directories or files";
                    }
                    else if ((totalDirectories > 0) && (totalFiles == 0))
                    {
                        result = $"0 files in {totalDirectories:N0} directories";
                    }
                    else if ((totalDirectories == 0) && (totalFiles > 0))
                    {
                        result = $"{totalFiles:N0} files in 1 directory, {dodSON.Core.Common.ByteCountHelper.ToString(totalBytes)}   ({totalBytes:N0} bytes)";
                    }
                    else
                    {
                        result = $"{totalFiles:N0} files in {totalDirectories:N0} directories, {dodSON.Core.Common.ByteCountHelper.ToString(totalBytes)}   ({totalBytes:N0} bytes)";
                    }
                }
                else
                {
                    System.Threading.Tasks.Parallel.ForEach(System.IO.Directory.GetFiles(path, "*.Archive.zip", System.IO.SearchOption.TopDirectoryOnly), (filename) =>
                    {
                        System.Threading.Interlocked.Increment(ref totalFiles);
                        System.Threading.Interlocked.Add(ref totalBytes, (new System.IO.FileInfo(filename)).Length);
                    });
                    result = $"{totalFiles:N0} archives, {dodSON.Core.Common.ByteCountHelper.ToString(totalBytes)}   ({totalBytes:N0} bytes)";
                }
            }
            else
            {
                if (job is MirrorJob) { result = $"no directories or files"; }
                else { result = $"no archives"; }
            }
            //
            return result;
        }

        // ----------
        // ----------
        // ----------

        private void DeleteReports()
        {
            // get reports archive file store
            var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
            var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
            foreach (ReportInfo item in Reports.List)
            {
                if (zip.Contains(item.Filename))
                {
                    zip.Delete(item.Filename);
                }
            }
            //
            zip.Save(false);
            FilesDeleted = true;
        }
        private void DeleteRemovedArchives()
        {
            foreach (var item in RemovedArchives.List)
            {
                Helper.DeleteFile((string)item.filename);
            }
            FilesDeleted = true;
        }
        private void DeleteSnapshots()
        {
            if (Job is ArchiveJob)
            {
                var message = $"Deleting All Snapshots for {Job.Name}, {Snapshots.SessionsCountString}, {Snapshots.SizeStr}";
                App.WriteDebugLog(nameof(DeleteJobWindow), message);
                App.WriteLog(nameof(DeleteJobWindow), App.LogCategory.Archive, message);
                foreach (var item in Snapshots.List)
                {
                    foreach (Helper.SnapshotFileInfo fileInfo in item.Files)
                    {
                        Helper.DeleteFile(fileInfo.FileInfo.FullName);
                    }
                }
                //
                FilesDeleted = true;
            }
        }
        private bool CanExecuteDeleteJobInfo
        {
            get
            {
                string path;
                if (Job is MirrorJob) { path = (Job as MirrorJob).MirrorPath; }
                else { path = (Job as ArchiveJob).ArchiveRootPath; }
                return (System.IO.Directory.Exists(path)) && (System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories).Length > 0);
            }
        }
        private void DeleteJobInfo()
        {
            string path;
            if (Job is MirrorJob) { path = (Job as MirrorJob).MirrorPath; }
            else { path = (Job as ArchiveJob).ArchiveRootPath; }
            if (Helper.DeleteDirectory(path))
            {
                //dodSON.Core.Threading.ThreadingHelper.Sleep(250);
                //System.IO.Directory.CreateDirectory(path);
            }
            FilesDeleted = true;
        }
        // ----------
        private string ParseFilenameTitle(string filename)
        {
            filename = System.IO.Path.GetFileNameWithoutExtension(filename);
            var parts = filename.Split('.');
            if (parts.Length > 0)
            {
                return parts[0];
            }
            return "";
        }
        private string ParseFilenameDate(string filename)
        {
            filename = System.IO.Path.GetFileNameWithoutExtension(filename);
            var parts = filename.Split('.');
            if (parts.Length > 1)
            {
                return new DateTime(int.Parse(parts[1].Substring(0, 4)),
                                    int.Parse(parts[1].Substring(4, 2)),
                                    int.Parse(parts[1].Substring(6, 2)),
                                    int.Parse(parts[1].Substring(9, 2)),
                                    int.Parse(parts[1].Substring(11, 2)),
                                    int.Parse(parts[1].Substring(13, 2))).ToString();
            }
            return "";
        }
        private string ParseSortableFilenameDate(string filename)
        {
            filename = System.IO.Path.GetFileNameWithoutExtension(filename);
            var parts = filename.Split('.');
            if (parts.Length > 1)
            {
                return parts[1];
            }
            return "";
        }
        private Helper.ReportFileStatistics ProcessReportHeader(string filename)
        {
            var lines = ReadContentLines(filename, _CancelTokenSource);
            if (lines != null)
            {
                var iniPart = new List<string>();
                var validCheck = false;
                foreach (var line in lines)
                {
                    if (_CancelTokenSource.Token.IsCancellationRequested) { return null; }
                    if (!validCheck)
                    {
                        validCheck = true;
                        if (!line.StartsWith($"# {Helper.GetReportTitleLine.Substring(0, Helper.GetApplicationTitle.Length)}")) { throw new Exception("Invalid File Format: not a dodSON.Software Archive & Mirror Tool Report File."); }
                    }
                    iniPart.Add(line.Substring(1).Trim());
                }
                // create report stats
                var dude = Helper.ProcessIni(iniPart);
                // return report stats
                return dude;
            }
            else
            {
                return null;
            }
        }
        private IEnumerable<string> ReadContentLines(string filename, System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            var result = new List<string>();
            using (var sr = new System.IO.StreamReader(filename))
            {
                if ((_CancelTokenSource.IsCancellationRequested) || (cancellationTokenSource.IsCancellationRequested)) { return null; }
                // ----
                var line = "";
                while (((line = sr.ReadLine()) != null) && (line.StartsWith("#")))
                {
                    result.Add(line);
                }
                sr.Close();
            }
            return result;
        }
        #endregion
    }

    public class DeletableItems<T>
    {
        #region Ctor
        public DeletableItems() { }
        #endregion
        #region Public Properties
        public DeletableItemsType ItemType { get; set; }
        public string Name { get; set; }
        public DateTimeOffset FirstDate { get; set; }
        public DateTimeOffset LastDate { get; set; }
        public string DateRange { get; set; }
        public IEnumerable<T> List { get; set; }
        public int ListCount => (this.List == null) ? 0 : List.Count();
        public string SessionsCountString { get; set; }
        public long TotalLength { get; set; }
        public string SizeStr => $"{dodSON.Core.Common.ByteCountHelper.ToString(TotalLength)}   ({TotalLength:N0} bytes)";
        #endregion
        #region Overrides
        public override string ToString() => $"Type={ItemType}, DateRange={DateRange}, Count={ListCount}";
        #endregion
    }

    public enum DeletableItemsType
    {
        Reports = 0,
        RemovedArchives,
        Snapshots
    }
}
