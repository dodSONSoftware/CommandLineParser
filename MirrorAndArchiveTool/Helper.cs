using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MirrorAndArchiveTool
{
    public class Helper
    {
        #region Private Static Fields
        private static readonly string CommentChar = "#";
        private static readonly string ValidCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890$()_- ";
        #endregion
        #region public Static Fields
        public static UserSettings GlobalUserSettings = new UserSettings();
        public static MainWindow MainWindow { get; set; }
        #endregion
        #region public Static Methods
        public static string FormatTitle(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return $"Archive & Mirror Tool - dodSON Software";
            }
            return $"{text} - Archive & Mirror Tool - dodSON Software";
        }
        // ----------------
        public static string GetApplicationFormalTitle => $"{Helper.GetApplicationTitle} - {Helper.GetApplicationVersion} - dodSON Software";
        public static string GetApplicationFilename => System.Reflection.Assembly.GetExecutingAssembly().Location;
        public static string GetDebugLogFilename => System.Reflection.Assembly.GetExecutingAssembly().Location + ".debug.log";
        public static string GetApplicationLogFilename => System.Reflection.Assembly.GetExecutingAssembly().Location + ".app.log";
        public static string GetApplicationConfigurationFilename => System.Reflection.Assembly.GetExecutingAssembly().Location + ".config.xml";
        public static string GetApplicationTitle => (string)App.Current.Resources["AppTitle"];
        public static string GetApplicationVersion => (string)App.Current.Resources["AppVersion"];
        public static string GetApplicationCopyright => (string)App.Current.Resources["AppCopyright"];
        public static string GetApplicationEmail => (string)App.Current.Resources["AppEmail"];
        public static string GetApplicationCopyrightNotice => (string)App.Current.Resources["AppNotice"];
        public static string GetApplicationCommandLineInfo => (string)App.Current.Resources["AppCommandLineInfo"];
        public static string GetReportTitleLine => $"{GetApplicationTitle}";
        // ----------------
        public static string GetDestinationDirectoryName(JobBase job)
        {
            if (job is MirrorAndArchiveTool.MirrorJob)
            {
                return (job as MirrorAndArchiveTool.MirrorJob).MirrorPath;
            }
            else if (job is MirrorAndArchiveTool.ArchiveJob)
            {
                return (job as MirrorAndArchiveTool.ArchiveJob).ArchiveRootPath;
            }
            //
            return "";
        }
        // ----------------
        public static string GetArchiveFilename(dodSON.Core.FileStorage.ICompareResult item)
        {
            return item.DestinationRootPath;
        }
        public static string ConvertPathToFilename(string sourcePath)
        {
            return string.Format("{0}{1}", FixPathString(sourcePath), ".zip");
        }
        public static string ConvertFilenameToPath(string filename)
        {
            return UnfixPathString(System.IO.Path.GetFileNameWithoutExtension(filename));
        }
        public static string FixPathString(string str)
        {
            return str.Replace(":", "^").Replace("\\", "^").Replace("^^", "#").Replace(".", "~");
        }
        public static string UnfixPathString(string str)
        {
            return str.Replace("#", ":").Replace("^", "\\").Replace("~", ".");
        }
        public static string StripBadCharacters(string source)
        {
            var worker = new System.Text.StringBuilder(1024);
            foreach (var ch in source) { if (ValidCharacters.Contains(ch)) { worker.Append(ch); } }
            return worker.ToString();
        }

        // ---------------- SNAPSHOTS ----------------

        public static Dictionary<string, SnapshotInformation> GetSnapshotData(GlobalSettings settings,
                                                                              Action refreshUI = null,
                                                                              string searchJobName = "*")
        {
            var results = new Dictionary<string, SnapshotInformation>();
            foreach (var filename in System.IO.Directory.GetFiles(settings.ArchiveZipFileSnapshotsRootPath,
                                                                  $"{searchJobName}.*.Snapshot.zip",
                                                                  System.IO.SearchOption.TopDirectoryOnly))
            {
                var fInfo = new System.IO.FileInfo(filename);
                ProcessFile(filename,
                            out string jobName,
                            out string snapshotSession,
                            out int filesCount,
                            out long compressedSize,
                            out long uncompressedSize,
                            out double compressionPercent,
                            out double compressionRatio);
                if (results.ContainsKey(jobName))
                {
                    if (results[jobName].Sessions.ContainsKey(snapshotSession))
                    {
                        results[jobName].Sessions[snapshotSession].Files.Add(new SnapshotFileInfo()
                        {
                            FileInfo = fInfo,
                            FilesCount = filesCount,
                            CompressedBytes = compressedSize,
                            UncompressedBytes = uncompressedSize,
                            CompressionPercentage = compressionPercent,
                            CompressionRatio = compressionRatio
                        });
                    }
                    else
                    {
                        var ssSession = new SnapshotSession()
                        {
                            Name = snapshotSession,
                            RefreshUI = refreshUI
                        };
                        ssSession.Files.Add(new SnapshotFileInfo()
                        {
                            FileInfo = fInfo,
                            FilesCount = filesCount,
                            CompressedBytes = compressedSize,
                            UncompressedBytes = uncompressedSize,
                            CompressionPercentage = compressionPercent,
                            CompressionRatio = compressionRatio
                        });
                        results[jobName].Sessions.Add(snapshotSession, ssSession);
                    }
                }
                else
                {
                    var ssInfo = new SnapshotInformation();
                    ssInfo.JobName = jobName;
                    var ssSession = new SnapshotSession()
                    {
                        Name = snapshotSession,
                        RefreshUI = refreshUI
                    };
                    ssSession.Files.Add(new SnapshotFileInfo()
                    {
                        FileInfo = fInfo,
                        FilesCount = filesCount,
                        CompressedBytes = compressedSize,
                        UncompressedBytes = uncompressedSize,
                        CompressionPercentage = compressionPercent,
                        CompressionRatio = compressionRatio
                    });
                    ssInfo.Sessions.Add(snapshotSession, ssSession);
                    //
                    results.Add(jobName, ssInfo);
                }
            }
            //
            return results;


            // ########-######## INTERNAL FUNCTIONS

            void ProcessFile(string filename_,
                             out string jobName_,
                             out string snapshotDate_,
                             out int filesCount_,
                             out long compressedSize_,
                             out long uncompressedSize_,
                             out double compressionPercent_,
                             out double compressionRatio_)
            {
                jobName_ = "";
                snapshotDate_ = "";
                filesCount_ = 0;
                compressedSize_ = 0;
                uncompressedSize_ = 0;
                compressionPercent_ = 0;
                compressionRatio_ = 0;
                var name_ = System.IO.Path.GetFileNameWithoutExtension(filename_);
                var parts = name_.Split('.');
                if (parts.Length > 0) { jobName_ = parts[0]; }
                if (parts.Length > 2) { snapshotDate_ = parts[2]; }
                //
                if (System.IO.File.Exists(filename_))
                {
                    var dude = Helper.GetZipStore(filename_, System.IO.Path.GetTempPath(), false, null);
                    filesCount_ = dude.Count;
                    compressedSize_ = dude.CompressedSize;
                    uncompressedSize_ = dude.UncompressedSize;
                    compressionPercent_ = dude.CompressionPercentage;
                    compressionRatio_ = dude.CompressionRatio;
                }
            }
        }
        public class SnapshotInformation
        {
            public string JobName { get; set; }
            public Dictionary<string, SnapshotSession> Sessions { get; } = new Dictionary<string, SnapshotSession>();
            public int TotalSessions => Sessions.Count;
            public string TotalSessionsString => $"{TotalSessions:N0} Sessions, {TotalAcrhives:N0} archives";
            public int TotalAcrhives
            {
                get
                {
                    var count = 0;
                    foreach (var session in Sessions?.Values)
                    {
                        count += session.Files.Count;
                    }
                    return count;
                }
            }
            public string TotalArchivesString => $"{TotalAcrhives:N0} Archives";
            public long UncompressedBytes
            {
                get
                {
                    long result = 0;
                    foreach (var item in Sessions.Values) { result += item.UncompressedBytes; }
                    return result;
                }
            }
            public string UncompressedBytesString => dodSON.Core.Common.ByteCountHelper.ToString(UncompressedBytes);
            public long CompressedBytes
            {
                get
                {
                    long result = 0;
                    foreach (var item in Sessions.Values) { result += item.CompressedBytes; }
                    return result;
                }
            }
            public string CompressedBytesString => dodSON.Core.Common.ByteCountHelper.ToString(CompressedBytes);
            public double CompressedPercentage => 1 - ((double)CompressedBytes / UncompressedBytes);
            public string CompressedPercentageString => $"{CompressedPercentage * 100.0:N1}%";
            public DateTimeOffset OldestSessionDate
            {
                get
                {
                    var oldest = DateTimeOffset.MinValue;
                    foreach (var item in Sessions)
                    {
                        if (item.Value.CreatedDate >= oldest)
                        {
                            oldest = item.Value.CreatedDate;
                        }
                    }
                    return oldest;
                }
            }
            public DateTimeOffset NewestSessionDate
            {
                get
                {
                    var newest = DateTimeOffset.MaxValue;
                    foreach (var item in Sessions)
                    {
                        if (item.Value.CreatedDate <= newest)
                        {
                            newest = item.Value.CreatedDate;
                        }
                    }
                    return newest;
                }
            }
            public TimeSpan Age => OldestSessionDate - NewestSessionDate;
            public string AgeString => dodSON.Core.Common.DateTimeHelper.FormatTimeSpanVerbose(Age);
        }
        public class SnapshotSession
        {
            public string Name { get; set; }
            public string NameAsDate => ConvertJobNameToDate(Name);
            public DateTimeOffset CreatedDate => Files[0].FileInfo.CreationTime;
            public string ElapsedTimeFromCreation => dodSON.Core.Common.DateTimeHelper.FormatTimeSpanVerbose(DateTime.Now - CreatedDate)+" ago";
            public List<SnapshotFileInfo> Files { get; } = new List<SnapshotFileInfo>();
            public string ArchivesCountString => $"{Files.Count:N0} archives";
            public long UncompressedBytes
            {
                get
                {
                    long result = 0;
                    foreach (var item in Files) { result += item.UncompressedBytes; }
                    return result;
                }
            }
            public string UncompressedBytesString => dodSON.Core.Common.ByteCountHelper.ToString(UncompressedBytes);
            public long CompressedBytes
            {
                get
                {
                    long result = 0;
                    foreach (var item in Files) { result += item.CompressedBytes; }
                    return result;
                }
            }
            public string CompressedBytesString => dodSON.Core.Common.ByteCountHelper.ToString(CompressedBytes);
            public double CompressionPercentage => 1 - ((double)CompressedBytes / UncompressedBytes);
            public string CompressionPercentageString => $"{CompressionPercentage * 100.0:N1}%";
            public Action RefreshUI { get; set; }
            // ---- COMMANDS ----
            #region Commands
            public Helper.SnapshotFileInfo CurrentlySelected { get; set; }

            private DelegateCommand _DeleteSessionCommand = null;
            public DelegateCommand DeleteSessionCommand
            {
                get
                {
                    if (_DeleteSessionCommand == null)
                    {
                        _DeleteSessionCommand = new DelegateCommand((x) =>
                        {
                            var dude = x as SnapshotSession;
                            if (System.Windows.MessageBox.Show($"Delete Snapshot Session {dude.Name}?{Environment.NewLine}{Environment.NewLine}Are You Sure?",
                                                               "Delete Snapshot Session",
                                                               System.Windows.MessageBoxButton.YesNo,
                                                               System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                            {
                                App.WriteLog(nameof(DeleteJobWindow), App.LogCategory.App, $"Deleted Snapshot Session, Job Name={dude.Name}");
                                App.WriteDebugLog(nameof(DeleteJobWindow), $"Deleted Snapshot Session, Job Name={dude.Name}");



                                // TODO: add window with indeterminate progress bar



                                foreach (var fInfo in dude.Files)
                                {
                                    Helper.DeleteFile(fInfo.FileInfo.FullName);
                                }
                                RefreshUI.Invoke();
                            }
                        });
                    }
                    return _DeleteSessionCommand;
                }
            }

            private DelegateCommand _SaveSessionCommand = null;
            public DelegateCommand SaveSessionCommand
            {
                get
                {
                    if (_SaveSessionCommand == null)
                    {
                        _SaveSessionCommand = new DelegateCommand((x) =>
                        {
                            var dude = x as SnapshotSession;
                            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                var extractPath = folderDialog.SelectedPath;
                                var iterator = from r in dude.Files
                                               select new FileCopyInfo() { SourceFilename = r.FileInfo.FullName, DestinationFilename = System.IO.Path.Combine(extractPath, r.FileInfo.Name) };
                                var wind = new CopyFiles("Save Session", $"Saving Session {dude.Name}", $"Session {dude.Name} Saved", iterator);
                                wind.ShowDialog();
                            }
                        });
                    }
                    return _SaveSessionCommand;
                }
            }
            public ICommand OpenArchiveCommand => new DelegateCommand(x =>
            {
                // execute
                try { System.Diagnostics.Process.Start(CurrentlySelected.FileInfo.FullName); }
                catch { }
            },
            x =>
            {
                // can execute
                return ((CurrentlySelected != null) && (CurrentlySelected.FileInfo.Exists));
            });
            public ICommand SaveArchiveCommand => new DelegateCommand(x =>
            {
                // execute
                var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var extractPath = folderDialog.SelectedPath;
                    var newFileName = System.IO.Path.Combine(extractPath, CurrentlySelected.FileInfo.Name);
                    System.IO.File.Copy(CurrentlySelected.FileInfo.FullName, newFileName, true);
                }
            },
            x =>
            {
                // can execute
                return ((CurrentlySelected != null) && (CurrentlySelected.FileInfo.Exists));
            });
            public ICommand DeleteArchiveCommand => new DelegateCommand(x =>
            {
                // execute
                if (MessageBox.Show($"Delete Archive {CurrentlySelected.FileInfo.Name}" + Environment.NewLine + Environment.NewLine + "Are you sure?",
                                    "Delete Archive",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Helper.DeleteFile(CurrentlySelected.FileInfo.FullName);
                    RefreshUI.Invoke();
                }
            },
            x =>
            {
                // can execute
                return ((CurrentlySelected != null) && (CurrentlySelected.FileInfo.Exists));
            });
            public ICommand CopyToClipboardCommand => new DelegateCommand(x =>
            {
                // execute
                Clipboard.SetText(CurrentlySelected.FileInfo.DirectoryName);
            },
            x =>
            {
                // can execute
                return (CurrentlySelected != null);
            });
            public ICommand CopyFilenameToClipboardCommand => new DelegateCommand(x =>
            {
                // execute
                Clipboard.SetText(CurrentlySelected.FileInfo.FullName);
            },
            x =>
            {
                // can execute
                return (CurrentlySelected != null);
            });
            #endregion
            // --------
            private string ConvertJobNameToDate(string name)
            {
                return new DateTime(int.Parse(name.Substring(0, 4)),
                                    int.Parse(name.Substring(4, 2)),
                                    int.Parse(name.Substring(6, 2)),
                                    int.Parse(name.Substring(9, 2)),
                                    int.Parse(name.Substring(11, 2)),
                                    int.Parse(name.Substring(13, 2))).ToString();
            }
        }
        public class SnapshotFileInfo
        {
            public System.IO.FileInfo FileInfo { get; set; }
            public int FilesCount { get; set; }
            public long UncompressedBytes { get; set; }
            public string UncompressedBytesString => dodSON.Core.Common.ByteCountHelper.ToString(UncompressedBytes);
            public long CompressedBytes { get; set; }
            public string CompressedBytesString => dodSON.Core.Common.ByteCountHelper.ToString(CompressedBytes);
            public double CompressionPercentage { get; set; }
            public string CompressionPercentageString => $"{CompressionPercentage * 100.0:N1}%";
            public double CompressionRatio { get; set; }
            public string CompressionRatioString => $"{CompressionRatio:N3}";
        }
        public static void ProcessSnapshots(ArchiveJob job,
                                            GlobalSettings settings,
                                            DateTime timeStamp,
                                            bool forceCreation,
                                            System.Threading.CancellationToken cancellationToken,
                                            Action<double, string> feedBackAction)
        {
            feedBackAction(0, "Initializing Snapshot Process...");

            // ######## determine if snapshots should be created
            var doIt = true;
            if (!forceCreation)
            {
                GetSnapshotsInformation(out int count, out DateTime newestDate);
                if (cancellationToken.IsCancellationRequested) { return; }
                if (count > 0)
                {
                    doIt = (newestDate.Add(settings.ArchiveZipFileSnapshotsMaturityTimeLimit) <= DateTime.Now);
                }
            }
            if (doIt)
            {
                var message = $"Creating Snapshot Set. Job={job.Name}, Time Stamp={timeStamp}";
                App.WriteLog(nameof(Helper), App.LogCategory.App, message);
                App.WriteDebugLog(nameof(Helper), message);

                // ######## create snapshots

                if (!System.IO.Directory.Exists(settings.ArchiveZipFileSnapshotsRootPath))
                {
                    System.IO.Directory.CreateDirectory(settings.ArchiveZipFileSnapshotsRootPath);
                }
                var counter = 0;
                foreach (var sourcePath in job.SourcePaths)
                {
                    if (cancellationToken.IsCancellationRequested) { return; }
                    var filename = string.Format("{0}.{1}.Archive.zip", job.Name, Helper.FixPathString(sourcePath));
                    var snapshotFilename = string.Format("{0}.{1}.{2}.Snapshot.zip", job.Name, Helper.FixPathString(sourcePath), timeStamp.ToString("yyyyMMdd^HHmmss"));
                    var fullFilename = System.IO.Path.Combine(job.ArchiveRootPath, filename);
                    if (System.IO.File.Exists(fullFilename))
                    {
                        var fInfo = new System.IO.FileInfo(fullFilename);
                        feedBackAction(++counter / (double)job.SourcePaths.Count, $"{snapshotFilename}, {dodSON.Core.Common.ByteCountHelper.ToString(fInfo.Length)}   ({fInfo.Length:N0})");

                        // get snapshot's full filename and ensure that the snapshot directory exists
                        var snapshotFullFilename = System.IO.Path.Combine(settings.ArchiveZipFileSnapshotsRootPath, snapshotFilename);

                        // copy file to create snapshot
                        System.IO.File.Copy(fullFilename, snapshotFullFilename, true);

                        // update snapshot file's dates to restart the time-based check
                        System.IO.File.SetCreationTime(snapshotFullFilename, timeStamp);
                        System.IO.File.SetLastAccessTime(snapshotFullFilename, timeStamp);
                        System.IO.File.SetLastWriteTime(snapshotFullFilename, timeStamp);

                        dodSON.Core.Threading.ThreadingHelper.Sleep(1);
                    }
                }

                // ######## manage snapshots for single job

                feedBackAction(0, $"Cleaning Up Snapshots...");

                // get all snapshot files for this job
                var candidates = System.IO.Directory.GetFiles(settings.ArchiveZipFileSnapshotsRootPath, $"{job.Name}*.Snapshot.zip", System.IO.SearchOption.TopDirectoryOnly);
                // get a list of all group names
                var uniqueNames = new List<string>();
                foreach (var filename in candidates)
                {
                    if (cancellationToken.IsCancellationRequested) { return; }
                    var parts = (System.IO.Path.GetFileName(filename)).Split('.');
                    var name = parts[2];
                    if (!uniqueNames.Contains(name)) { uniqueNames.Add(name); }
                }
                // test if there are more than allowed
                if (uniqueNames.Count > settings.ArchiveZipFileSnapshotsMaximum)
                {
                    // gather information from the candidate list
                    var dude = from x in candidates
                               let y = new System.IO.FileInfo(x)
                               let s = y.Name.Split('.')
                               orderby y.CreationTime ascending
                               select new { Fullname = x, Filename = y.Name, Job = s[0], Group = s[2], y.CreationTime };
                    // get (a member of) the oldest group
                    var groupDude = dude.FirstOrDefault();
                    // get all of the files in the snapshots folder belonging to the Job and the Group
                    var files = from x in System.IO.Directory.GetFiles(settings.ArchiveZipFileSnapshotsRootPath, $"{groupDude.Job}.*.{groupDude.Group}.Snapshot.zip", System.IO.SearchOption.TopDirectoryOnly)
                                select x;
                    // delete the selected files
                    foreach (var filename in files)
                    {
                        Helper.DeleteFile(filename);
                    }
                }
            }

            // ################ INTERNAL FUNCTIONS
            void GetSnapshotsInformation(out int count_, out DateTime newestDate_)
            {
                count_ = 0;
                newestDate_ = DateTime.MinValue;
                // get newest
                foreach (var filename in System.IO.Directory.GetFiles(settings.ArchiveZipFileSnapshotsRootPath,
                                                                      $"{job.Name}.*.Snapshot.zip",
                                                                      System.IO.SearchOption.TopDirectoryOnly))
                {
                    if (cancellationToken.IsCancellationRequested) { return; }
                    ++count_;
                    var fileTime = (new System.IO.FileInfo(filename)).CreationTime;
                    if (fileTime > newestDate_) { newestDate_ = fileTime; }
                }
            }
        }

        // ---------------- LOGGER ----------------

        public static TimeSpan LogFlushTimeLimit { get; set; } = TimeSpan.FromSeconds(10);
        public static int LogFlushMaximumLogs { get; set; } = 10;
        //
        private static readonly System.Threading.CancellationTokenSource _CachedLogCancellationSource = new System.Threading.CancellationTokenSource();
        private static dodSON.Core.Logging.ICachedLog _InternalCachedLog = null;
        private static readonly long Log_MaxLogSizeBytes = dodSON.Core.Common.ByteCountHelper.FromMegabytes(15);
        private static readonly int Log_LogToRetain = 10000;
        private static readonly bool Log_WriteLogsUsingLocalTime = true;
        //
        public static dodSON.Core.Logging.ILog GetLogger
        {
            get
            {
                // check cached log
                if (_InternalCachedLog == null)
                {
                    // ######## -------- create default logger --------
                    // ######## create log splitter
                    var logFilters = new List<dodSON.Core.Logging.LogFilter>();
                    // filter 1: Debug
                    logFilters.Add(new dodSON.Core.Logging.LogFilter(CreateDebugValidator(), CreateDebugLog()));
                    // filter 1: !Debug
                    logFilters.Add(new dodSON.Core.Logging.LogFilter(CreateAppValidator(), CreateAppLog()));
                    // create log splitter logger
                    var logActual = new dodSON.Core.Logging.LogSplitter(logFilters);
                    // ######## create internal cached logger
                    CreateInternalCachedLog(logActual);
                }
                // return cached log
                return _InternalCachedLog;


                // ######## INTERNAL FUNCTIONS

                dodSON.Core.Logging.ILoggable CreateDebugValidator() { return new DebugValidator(); }
                dodSON.Core.Logging.ILog CreateDebugLog() { return new dodSON.Core.Logging.FileEventLog.Log(GetDebugLogFilename, Log_WriteLogsUsingLocalTime, true, Log_MaxLogSizeBytes, Log_LogToRetain); }

                dodSON.Core.Logging.ILoggable CreateAppValidator() { return new AppValidator(); }
                dodSON.Core.Logging.ILog CreateAppLog() { return new dodSON.Core.Logging.FileEventLog.Log(GetApplicationLogFilename, Log_WriteLogsUsingLocalTime, true, Log_MaxLogSizeBytes, Log_LogToRetain); }
            }
            set
            {
                if (value == null) { throw new ArgumentNullException(nameof(value)); }
                // close internal cached logger, if not null
                _InternalCachedLog?.Close();
                // create internal cached logger
                CreateInternalCachedLog(value);
            }
        }

        public static bool IsAnalyzingJobs { get; internal set; }

        public static void FlushLog() => _InternalCachedLog?.FlushLogs();
        // --------------------------------
        private static System.Threading.Tasks.Task _InternalCachedLogTask = null;
        private static void CreateInternalCachedLog(dodSON.Core.Logging.ILog log_)
        {
            _InternalCachedLog = new dodSON.Core.Logging.CachedLog(log_, false, LogFlushMaximumLogs, LogFlushTimeLimit);
            _InternalCachedLog.Open();
            // create cached log thread worker
            _InternalCachedLogTask = Helper.StartBackgroundTask(() =>
            {
                App.WriteDebugLog(nameof(Helper), $"Starting Background Task for {nameof(Helper)}:{{Internal Cache}}");
                while (true)
                {
                    if (_CachedLogCancellationSource.IsCancellationRequested)
                    {
                        App.WriteDebugLog(nameof(Helper), $"Stopping Background Task for {nameof(Helper)}:{{Internal Cache}}");
                        break;
                    }
                    if (_InternalCachedLog.IsFlushable)
                    {
                        _InternalCachedLog.FlushLogs();
                    }
                    dodSON.Core.Threading.ThreadingHelper.Sleep(1000);
                }
            });
        }
        private static bool CleanUpLoggerIsWorking = false;
        public static void CleanUpLogger()
        {
            if (!CleanUpLoggerIsWorking)
            {
                CleanUpLoggerIsWorking = true;
                try
                {
                    // cancel worker thread
                    if (!_CachedLogCancellationSource.IsCancellationRequested)
                    {
                        _CachedLogCancellationSource.Cancel();
                        _InternalCachedLogTask.Wait(1200);
                    }

                    // flush and close log
                    if ((_InternalCachedLog != null) && (_InternalCachedLog.IsOpen))
                    {
                        // write final log entries
                        var message = $"Application Stopped, {Helper.GetApplicationFormalTitle}, RunTime={dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(App.RunTime)}";
                        App.WriteDebugLog(nameof(App), message);
                        App.WriteLog(nameof(App), App.LogCategory.App, message);

                        // flush and close log
                        _InternalCachedLog.FlushLogs();
                        _InternalCachedLog.Close();
                        _InternalCachedLog = null;
                    }
                }
                finally
                {
                    CleanUpLoggerIsWorking = false;
                }
            }
        }

        public static System.Threading.Tasks.Task StartBackgroundTask(Action action)
        {
            if (action == null) { throw new ArgumentNullException(nameof(action)); }
            return System.Threading.Tasks.Task.Factory.StartNew(action, System.Threading.Tasks.TaskCreationOptions.LongRunning);
        }

        // ---- LOG VALIDATORS ----

        public class DebugValidator
            : dodSON.Core.Logging.ILoggable
        {
            public bool IsValid(dodSON.Core.Logging.ILogEntry logEntry) { return logEntry.EntryType == dodSON.Core.Logging.LogEntryType.Debug; }
        }
        public class AppValidator
            : dodSON.Core.Logging.ILoggable
        {
            public bool IsValid(dodSON.Core.Logging.ILogEntry logEntry) { return logEntry.EntryType != dodSON.Core.Logging.LogEntryType.Debug; }
        }

        // ---------------- UTILITY ----------------

        public static bool DeleteDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                return true;
            }
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.Delete(path, true);
                        return true;
                    }
                }
                catch
                {
                    dodSON.Core.Threading.ThreadingHelper.Sleep(50);
                }
            }
            return false;
        }
        public static bool DeleteFile(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                return true;
            }
            //
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                        return true;
                    }
                }
                catch
                {
                    dodSON.Core.Threading.ThreadingHelper.Sleep(50);
                }
            }
            //
            return false;
        }

        // ----------------

        public static Helper.ReportFileStatistics AnalyzeReport(JobBase job,
                                                                        IEnumerable<dodSON.Core.FileStorage.ICompareResult> report,
                                                                        DateTime timeStamp)
        {
            var stats = new Helper.ReportFileStatistics
            {
                ReportDate = timeStamp,
                JobName = job.Name
            };
            if (job is ArchiveJob)
            {
                stats.JobType = "Archive";
                stats.JobArchiveStoragePath = ((ArchiveJob)job).ArchiveRootPath;
                foreach (var item in ((ArchiveJob)job).SourcePaths) { stats.JobArchiveSourcePaths.Add(item); }
            }
            else
            {
                stats.JobType = "Mirror";
                stats.JobMirrorSourcePath = ((MirrorJob)job).SourcePath;
                stats.JobMirrorDestinationPath = ((MirrorJob)job).MirrorPath;
            }
            stats.JobDateCreated = job.DateCreate;
            stats.JobArchiveRemoveFiles = job.ArchiveRemoveFiles;
            foreach (var item in report)
            {
                switch (item.Action)
                {
                    case dodSON.Core.FileStorage.CompareAction.New:
                        ++stats.NewCount;
                        break;
                    case dodSON.Core.FileStorage.CompareAction.Ok:
                        ++stats.OkCount;
                        break;
                    case dodSON.Core.FileStorage.CompareAction.Remove:
                        ++stats.RemoveCount;
                        break;
                    case dodSON.Core.FileStorage.CompareAction.Update:
                        ++stats.UpdateCount;
                        break;
                    default:
                        break;
                }
            }
            stats.TotalCount = stats.OkCount + stats.NewCount + stats.UpdateCount + stats.RemoveCount;
            stats.SourceTotalCount = stats.OkCount + stats.NewCount + stats.UpdateCount;
            stats.DestinationTotalCount = stats.OkCount + stats.UpdateCount + stats.RemoveCount;
            stats.TotalChangedCount = stats.NewCount + stats.UpdateCount + stats.RemoveCount;
            stats.ExportOK = true;
            stats.ExportNew = true;
            stats.ExportUpdate = true;
            stats.ExportRemove = true;
            return stats;
        }


        // TODO: starting with this method, and including, at least, the following two (2) methods, consider how to make this activity more efficient...

        public static void OutputReport(string filename,
                                        Helper.ReportFileStatistics reportStats,
                                        IEnumerable<dodSON.Core.FileStorage.ICompareResult> comparisonReport,
                                        GlobalSettings settings)
        {
            // create report header
            var header = new System.Text.StringBuilder(2048);
            header.AppendFormat("{0} {2}{1}", CommentChar, Environment.NewLine, GetReportTitleLine);
            header.AppendFormat("{0} {2}{1}", CommentChar, Environment.NewLine, GetApplicationCopyright);
            header.AppendFormat("{0} Report Date= {2}{1}", CommentChar, Environment.NewLine, reportStats.ReportDate);
            header.AppendFormat("{0} Report Rows= {2:N0}{1}", CommentChar, Environment.NewLine, reportStats.TotalCount);
            header.AppendFormat("{0} -------- Job Information --------{1}", CommentChar, Environment.NewLine);
            header.AppendFormat("{0} Name= {2}{1}", CommentChar, Environment.NewLine, reportStats.JobName);
            header.AppendFormat("{0} Type= {2}{1}", CommentChar, Environment.NewLine, reportStats.JobType);
            header.AppendFormat("{0} Date Create= {2}{1}", CommentChar, Environment.NewLine, reportStats.JobDateCreated);
            header.AppendFormat("{0} Archive Remove Files= {2}{1}", CommentChar, Environment.NewLine, reportStats.JobArchiveRemoveFiles);
            if (reportStats.JobType == "Archive")
            {
                header.AppendFormat("{0} Archive Storage Path= {2}{1}", CommentChar, Environment.NewLine, reportStats.JobArchiveStoragePath);
                foreach (var item in reportStats.JobArchiveSourcePaths)
                {
                    header.AppendFormat("{0} Archive Source Path= {2}{1}", CommentChar, Environment.NewLine, item);
                }
            }
            else
            {
                header.AppendFormat("{0} Mirror Source Path= {2}{1}", CommentChar, Environment.NewLine, reportStats.JobMirrorSourcePath);
                header.AppendFormat("{0} Mirror Destination Path= {2}{1}", CommentChar, Environment.NewLine, reportStats.JobMirrorDestinationPath);
            }
            header.AppendFormat("{0} -------- File Counts --------{1}", CommentChar, Environment.NewLine);
            header.AppendFormat("{0} Ok= {2:N0}{1}", CommentChar, Environment.NewLine, reportStats.OkCount);
            header.AppendFormat("{0} New= {2:N0}{1}", CommentChar, Environment.NewLine, reportStats.NewCount);
            header.AppendFormat("{0} Update= {2:N0}{1}", CommentChar, Environment.NewLine, reportStats.UpdateCount);
            header.AppendFormat("{0} Remove= {2:N0}{1}", CommentChar, Environment.NewLine, reportStats.RemoveCount);
            header.AppendFormat("{0} Total= {2:N0}{1}", CommentChar, Environment.NewLine, reportStats.TotalCount);
            header.AppendFormat("{0} Total Changed= {2:N0}{1}", CommentChar, Environment.NewLine, reportStats.TotalChangedCount);
            header.AppendFormat("{0} Total Source= {2:N0}{1}", CommentChar, Environment.NewLine, reportStats.SourceTotalCount);
            header.AppendFormat("{0} Total Destination= {2:N0}{1}", CommentChar, Environment.NewLine, reportStats.DestinationTotalCount);
            header.AppendFormat("{0} -------- Export Settings --------{1}", CommentChar, Environment.NewLine);
            header.AppendFormat("{0} Export Ok Files= {2}{1}", CommentChar, Environment.NewLine, "True");
            header.AppendFormat("{0} Export New Files= {2}{1}", CommentChar, Environment.NewLine, "True");
            header.AppendFormat("{0} Export Update Files= {2}{1}", CommentChar, Environment.NewLine, "True");
            header.AppendFormat("{0} Export Remove Files= {2}{1}", CommentChar, Environment.NewLine, "True");
            header.AppendFormat("{0} -----------------------------{1}", CommentChar, Environment.NewLine);
            // create temp report file
            var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(filename));
            using (var sw = new System.IO.StreamWriter(tempFile, false))
            {
                sw.Write(header.ToString());
                if (reportStats.JobType == "Archive") { sw.WriteLine(GenerateArchiveCSV(comparisonReport)); }
                else { sw.WriteLine(GenerateMirrorCSV(comparisonReport)); }
                sw.Flush();
                sw.Close();
            }
            // add temp report file to archive
            var zipFilename = System.IO.Path.Combine(settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
            var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
            zip.Add(System.IO.Path.GetFileName(filename), tempFile, DateTime.UtcNow, (new System.IO.FileInfo(tempFile)).Length);
            zip.Save(false);
            // delete temp report file
            try { System.IO.File.Delete(tempFile); }
            catch { }
        }
        public static string GenerateArchiveCSV(IEnumerable<dodSON.Core.FileStorage.ICompareResult> comparisonReport)
        {
            // create table
            var csvReport = new dodSON.Core.DelimiterSeparatedValues.DsvTable(
                                new dodSON.Core.DelimiterSeparatedValues.DsvSettings(true, false, true, true, dodSON.Core.DelimiterSeparatedValues.ColumnEnclosingRuleEnum.EncloseMinimal));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "Action", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "SourceFile", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "SourceFileDate", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "ArchiveFile", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "DestinationFile", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "DestinationFileDate", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "DateDifference", false));
            // populate table
            foreach (var item in comparisonReport)
            {
                var timeDiff = (item.SourceLastModifiedTimeUtc - item.DestinationLastModifiedTimeUtc);
                csvReport.Rows.Add(csvReport.NewRow(item.Action,
                                                    item.SourceFullPath,
                                                    (item.SourceLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.SourceLastModifiedTimeUtc.ToString(),
                                                    item.DestinationRootPath,
                                                    item.DestinationRootFilename,
                                                    (item.DestinationLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.DestinationLastModifiedTimeUtc.ToString(),
                                                    (item.Action == dodSON.Core.FileStorage.CompareAction.Update) ? timeDiff.ToString() : ""));
            }
            // output report
            return csvReport.WriteString();
        }

        public static dodSON.Core.FileStorage.ICompressedFileStore GetZipStore(string zipFilename, string tempPath, bool saveOriginalFilenames, IEnumerable<string> extensionsToStore)
        {
            // return new dodSON.Core.FileStorage.MSdotNETZip.FileStore(zipFilename, tempPath, saveOriginalFilenames, extensionsToStore);

            return new dodSON.Core.FileStorage.IonicZip.FileStore(zipFilename, tempPath, saveOriginalFilenames, extensionsToStore);
        }

        public static string GenerateMirrorCSV(IEnumerable<dodSON.Core.FileStorage.ICompareResult> comparisonReport)
        {
            // create table
            var csvReport = new dodSON.Core.DelimiterSeparatedValues.DsvTable(
                                new dodSON.Core.DelimiterSeparatedValues.DsvSettings(true, false, true, true, dodSON.Core.DelimiterSeparatedValues.ColumnEnclosingRuleEnum.EncloseMinimal));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "Action", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "ItemType", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "SourceFile", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "SourceFileDate", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "DestinationFile", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "DestinationFileDate", false));
            csvReport.Columns.Add(new dodSON.Core.DelimiterSeparatedValues.DsvColumn(csvReport, "DateDifference", false));
            // populate table
            foreach (var item in comparisonReport)
            {
                var timeDiff = (item.SourceLastModifiedTimeUtc - item.DestinationLastModifiedTimeUtc);
                csvReport.Rows.Add(csvReport.NewRow(item.Action,
                                                    item.ItemType,
                                                    item.SourceFullPath,
                                                    (item.SourceLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.SourceLastModifiedTimeUtc.ToString(),
                                                    (item.Action != dodSON.Core.FileStorage.CompareAction.New) ? item.DestinationRootFilename : "",
                                                    (item.DestinationLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.DestinationLastModifiedTimeUtc.ToString(),
                                                    ((item.ItemType == dodSON.Core.FileStorage.CompareType.File) &&
                                                     (item.Action == dodSON.Core.FileStorage.CompareAction.Update)) ? timeDiff.ToString() : "")
                                  );


                //Action = item.Action.ToString(),
                //    ItemType = item.ItemType.ToString(),
                //    SourceFile = item.SourceFullPath,
                //    SourceDate = (item.SourceLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.SourceLastModifiedTimeUtc.ToString(),
                //    DestinationFile = (item.Action != dodSON.Core.FileStorage.CompareAction.New) ? item.DestinationFullPath : "",
                //    DestinationDate = (item.DestinationLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.DestinationLastModifiedTimeUtc.ToString(),
                //    DateDifference = ((item.ItemType == dodSON.Core.FileStorage.CompareType.File) &&
                //                      (item.Action == dodSON.Core.FileStorage.CompareAction.Update))
                //                        ? (item.SourceLastModifiedTimeUtc - item.DestinationLastModifiedTimeUtc).ToString()
                //                        : ""



            }
            // output report
            return csvReport.WriteString();
        }

        // ----------------

        // TODO: make this more efficient; write header to file, for each line, create one line, write to file. repeat...

        public static IEnumerable<dodSON.Core.FileStorage.ICompareResult> GenerateReport(JobBase job,
                                                                                         System.Threading.CancellationToken token)
        {
            if (job is MirrorJob)
            {
                // **** MIRROR JOB
                var results = dodSON.Core.FileStorage.FileStorageHelper.Compare(((MirrorJob)job).SourcePath,
                                                                                ((MirrorJob)job).MirrorPath,
                                                                                token,
                                                                                null);   /*   (x) => { DispatchProgressBarState(x, ((MirrorJob)job).SourcePath, ""); }   */
                if (!token.IsCancellationRequested)
                {
                    return from x in results
                           where ((x.ItemType == dodSON.Core.FileStorage.CompareType.File) || (x.ItemType == dodSON.Core.FileStorage.CompareType.Directory)) &&
                                 (x.Action == dodSON.Core.FileStorage.CompareAction.Ok ||
                                  x.Action == dodSON.Core.FileStorage.CompareAction.New ||
                                  x.Action == dodSON.Core.FileStorage.CompareAction.Update ||
                                  x.Action == dodSON.Core.FileStorage.CompareAction.Remove)
                           select x;
                }
            }
            else
            {
                // **** ARCHIVE JOB
                double total = ((ArchiveJob)job).SourcePaths.Count;
                double count = 0;
                var results = new List<dodSON.Core.FileStorage.ICompareResult>();
                foreach (var sourcePath in ((ArchiveJob)job).SourcePaths)
                {
                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }
                    var filename = string.Format("{0}.{1}.Archive.zip", Helper.StripBadCharacters(((ArchiveJob)job).Name), Helper.FixPathString(sourcePath));
                    var archiveStoreFilename = System.IO.Path.Combine(((ArchiveJob)job).ArchiveRootPath, filename);
                    if (System.IO.Directory.Exists(sourcePath))
                    {
                        try
                        {
                            var archiveStore = Helper.GetZipStore(archiveStoreFilename, System.IO.Path.GetTempPath(), false, null);
                            var list = dodSON.Core.FileStorage.FileStorageHelper.Compare(sourcePath,
                                                                                         archiveStore,
                                                                                         token,
                                                                                         null);   /*   (x) => { DispatchProgressBarState((count / total) + (x / total), sourcePath, ""); });   */
                            if (list != null)
                            {
                                foreach (var item in from x in list
                                                     where (x.ItemType == dodSON.Core.FileStorage.CompareType.File) &&
                                                           (x.Action == dodSON.Core.FileStorage.CompareAction.Ok ||
                                                            x.Action == dodSON.Core.FileStorage.CompareAction.New ||
                                                            x.Action == dodSON.Core.FileStorage.CompareAction.Update ||
                                                            x.Action == dodSON.Core.FileStorage.CompareAction.Remove)
                                                     select x)
                                {
                                    results.Add(new ComparisonResults2(item, filename));
                                }
                            }
                        }
                        catch { }
                    }
                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }
                    count++;
                }
                return results;
            }
            return null;
        }
        //public static void DispatchProgressBarState(double value, string text1, string text2)
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

        // ----------------
        public static IEnumerable<string> ReadContentLines(string filename, System.Threading.CancellationTokenSource tokenSource)
        {
            var result = new List<string>();
            using (var sr = new System.IO.StreamReader(filename))
            {
                if (tokenSource.Token.IsCancellationRequested) { return null; }
                // ----
                var line = "";
                while ((line = sr.ReadLine()) != null) { result.Add(line); }
                sr.Close();
            }
            return result;
        }
        public static void ProcessLines(JobBase job,
                         IEnumerable<string> lines,
                         System.Threading.CancellationTokenSource tokenSource,
                         out IEnumerable<object> okData,
                         out IEnumerable<object> newData,
                         out IEnumerable<object> updateData,
                         out IEnumerable<object> removeData,
                         out Helper.ReportFileStatistics stats)
        {
            okData = new List<Helper.DetailsRowBase>();
            newData = new List<Helper.DetailsRowBase>();
            updateData = new List<Helper.DetailsRowBase>();
            removeData = new List<Helper.DetailsRowBase>();
            stats = new Helper.ReportFileStatistics();
            // ----
            if (tokenSource.Token.IsCancellationRequested) { return; }
            // ----
            var iniPart = new List<string>();
            var reportPart = new System.Text.StringBuilder(2048);
            var validCheck = false;
            foreach (var line in lines)
            {
                if (tokenSource.Token.IsCancellationRequested) { return; }
                // ----
                if (!validCheck)
                {
                    validCheck = true;
                    if (!line.StartsWith($"# {Helper.GetReportTitleLine.Substring(0, Helper.GetApplicationTitle.Length)}")) { throw new Exception("Invalid File Format: not a dodSON.Software Archive & Mirror Tool Report File."); }
                }
                if (line.StartsWith("#")) { iniPart.Add(line.Substring(1).Trim()); }
                else { reportPart.AppendLine(line); }
            }
            // ----
            // create report stats
            stats = ProcessIni(iniPart);
            // create report
            var report = new dodSON.Core.DelimiterSeparatedValues.DsvTable();
            report.ReadString(reportPart.ToString());
            // process report
            var worker_okData = new SortedList<string, Helper.DetailsRowBase>();
            var worker_newData = new SortedList<string, Helper.DetailsRowBase>();
            var worker_updateData = new SortedList<string, Helper.DetailsRowBase>();
            var worker_removeData = new SortedList<string, Helper.DetailsRowBase>();
            foreach (var item in report.Rows)
            {
                if (tokenSource.Token.IsCancellationRequested) { return; }
                // ----
                Helper.DetailsRowBase drow = CreateDataRow(item, out bool isArchiveJob);
                dodSON.Core.FileStorage.CompareAction action = (dodSON.Core.FileStorage.CompareAction)Enum.Parse(typeof(dodSON.Core.FileStorage.CompareAction), (item["Action"] ?? "Unknown").ToString(), true);
                dodSON.Core.FileStorage.CompareType compType = dodSON.Core.FileStorage.CompareType.File;
                if (!isArchiveJob) { compType = (dodSON.Core.FileStorage.CompareType)Enum.Parse(typeof(dodSON.Core.FileStorage.CompareType), (item["ItemType"] ?? "Unknown").ToString(), true); }
                var sortableName = "";
                if (isArchiveJob)
                {
                    sortableName = string.Format("{0}{1}{2}{3}",
                                                (int)action,
                                                (int)compType,
                                                ((Helper.ArchiveFilesDetailsRow)drow).DestinationArchive,
                                                ((!string.IsNullOrWhiteSpace(((Helper.ArchiveFilesDetailsRow)drow).SourceFile)) ? ((Helper.ArchiveFilesDetailsRow)drow).SourceFile : ((Helper.ArchiveFilesDetailsRow)drow).DestinationFile));
                }
                else
                {
                    sortableName = string.Format("{0}{1}{2}",
                                                (int)action,
                                                (int)compType,
                                                ((!string.IsNullOrWhiteSpace(((Helper.MirrorFilesDetailsRow)drow).SourceFile)) ? ((Helper.MirrorFilesDetailsRow)drow).SourceFile : ((Helper.MirrorFilesDetailsRow)drow).DestinationFile));
                }
                switch (action)
                {
                    case dodSON.Core.FileStorage.CompareAction.Ok:
                        worker_okData.Add(sortableName, CreateDataRow(item, out isArchiveJob));
                        break;
                    case dodSON.Core.FileStorage.CompareAction.New:
                        worker_newData.Add(sortableName, CreateDataRow(item, out isArchiveJob));
                        break;
                    case dodSON.Core.FileStorage.CompareAction.Update:
                        worker_updateData.Add(sortableName, CreateDataRow(item, out isArchiveJob));
                        break;
                    case dodSON.Core.FileStorage.CompareAction.Remove:
                        worker_removeData.Add(sortableName, CreateDataRow(item, out isArchiveJob));
                        break;
                    default:
                        break;
                }
            }
            // ----
            if (tokenSource.Token.IsCancellationRequested) { return; }
            // ----
            okData = new List<Helper.DetailsRowBase>(worker_okData.Values);
            newData = new List<Helper.DetailsRowBase>(worker_newData.Values);
            updateData = new List<Helper.DetailsRowBase>(worker_updateData.Values);
            removeData = new List<Helper.DetailsRowBase>(worker_removeData.Values);
            stats.TotalCount = okData.Count() + newData.Count() + updateData.Count() + removeData.Count();
        }
        public static Helper.DetailsRowBase CreateDataRow(dodSON.Core.DelimiterSeparatedValues.DsvRow item, out bool isArchiveJob)
        {
            // create data row
            Helper.DetailsRowBase drow = null;
            if (item.Parent.Columns.Contains("ArchiveFile"))
            {
                isArchiveJob = true;
                drow = new Helper.ArchiveFilesDetailsRow()
                {
                    DestinationArchive = (item.Parent.Columns.Contains("ArchiveFile")) ? (item["ArchiveFile"] ?? "").ToString() : "",
                    Action = (item["Action"] ?? "").ToString(),
                    SourceFile = (item["SourceFile"] ?? "").ToString(),
                    SourceDate = (item["SourceFileDate"] ?? "").ToString(),
                    DestinationFile = (item["DestinationFile"] ?? "").ToString(),
                    DestinationDate = (item["DestinationFileDate"] ?? "").ToString(),
                    DateDifference = (item["DateDifference"] ?? "").ToString()
                };
            }
            else
            {
                isArchiveJob = false;
                drow = new Helper.MirrorFilesDetailsRow()
                {
                    Action = (item["Action"] ?? "").ToString(),
                    ItemType = (item["ItemType"] ?? "").ToString(),
                    SourceFile = (item["SourceFile"] ?? "").ToString(),
                    SourceDate = (item["SourceFileDate"] ?? "").ToString(),
                    DestinationFile = (item["DestinationFile"] ?? "").ToString(),
                    DestinationDate = (item["DestinationFileDate"] ?? "").ToString(),
                    DateDifference = (item["DateDifference"] ?? "").ToString()
                };
            }
            return drow;
        }
        public static Helper.ReportFileStatistics ProcessIni(List<string> iniPart)
        {
            var stats = new Helper.ReportFileStatistics();
            foreach (var line in iniPart)
            {
                if (line.Contains("="))
                {
                    var parts = line.Split('=');
                    if (parts.Length > 1)
                    {
                        switch (parts[0].Trim())
                        {
                            case "Report Date":
                                stats.ReportDate = ConvertToDate(parts[1].Trim());
                                break;
                            case "Name":
                                stats.JobName = parts[1].Trim();
                                break;
                            case "Type":
                                stats.JobType = parts[1].Trim();
                                break;
                            case "Date Create":
                                stats.JobDateCreated = ConvertToDate(parts[1].Trim());
                                break;
                            case "Archive Remove Files":
                                stats.JobArchiveRemoveFiles = ConvertToBoolean(parts[1].Trim());
                                break;
                            case "Ok":
                                stats.OkCount = ConvertToInteger(parts[1].Trim());
                                break;
                            case "New":
                                stats.NewCount = ConvertToInteger(parts[1].Trim());
                                break;
                            case "Update":
                                stats.UpdateCount = ConvertToInteger(parts[1].Trim());
                                break;
                            case "Remove":
                                stats.RemoveCount = ConvertToInteger(parts[1].Trim());
                                break;
                            case "Total":
                                stats.TotalCount = ConvertToInteger(parts[1].Trim());
                                break;
                            case "Total Changed":
                                stats.TotalChangedCount = ConvertToInteger(parts[1].Trim());
                                break;
                            case "Total Source":
                                stats.SourceTotalCount = ConvertToInteger(parts[1].Trim());
                                break;
                            case "Total Destination":
                                stats.DestinationTotalCount = ConvertToInteger(parts[1].Trim());
                                break;
                            case "Export Ok Files":
                                stats.ExportOK = ConvertToBoolean(parts[1].Trim());
                                break;
                            case "Export New Files":
                                stats.ExportNew = ConvertToBoolean(parts[1].Trim());
                                break;
                            case "Export Update Files":
                                stats.ExportUpdate = ConvertToBoolean(parts[1].Trim());
                                break;
                            case "Export Remove Files":
                                stats.ExportRemove = ConvertToBoolean(parts[1].Trim());
                                break;
                            case "Archive Storage Path":
                                stats.JobArchiveStoragePath = parts[1].Trim();
                                break;
                            case "Archive Source Path":
                                stats.JobArchiveSourcePaths.Add(parts[1].Trim());
                                break;
                            case "Mirror Source Path":
                                stats.JobMirrorSourcePath = parts[1].Trim();
                                break;
                            case "Mirror Destination Path":
                                stats.JobMirrorDestinationPath = parts[1].Trim();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return stats;
        }
        public static DateTime ConvertToDate(string str)
        {
            var result = DateTime.MinValue;
            if (!DateTime.TryParse(str, out result)) { result = DateTime.MinValue; }
            return result;
        }
        public static int ConvertToInteger(string str)
        {
            var result = int.MinValue;
            str = str.Replace(",", "");
            if (!int.TryParse(str, out result)) { result = int.MinValue; }
            return result;
        }
        public static bool ConvertToBoolean(string str)
        {
            if (!bool.TryParse(str, out bool result)) { result = false; }
            return result;
        }
        public static bool ConvertExportFilenameFormatToBoolean(string str)
        {
            if (!bool.TryParse(str, out bool result)) { result = false; }
            return result;
        }
        public static void ExtractLists(JobBase job,
                                          IEnumerable<dodSON.Core.FileStorage.ICompareResult> report,
                                          out IEnumerable<object> okData,
                                          out IEnumerable<object> newData,
                                          out IEnumerable<object> updateData,
                                          out IEnumerable<object> removeData)
        {
            var worker_okData = new SortedList<string, Helper.DetailsRowBase>();
            var worker_newData = new SortedList<string, Helper.DetailsRowBase>();
            var worker_updateData = new SortedList<string, Helper.DetailsRowBase>();
            var worker_removeData = new SortedList<string, Helper.DetailsRowBase>();
            foreach (var item in report)
            {
                Helper.DetailsRowBase worker_drow = JobFactory(job, item);
                string sortableName = "";
                if (job is ArchiveJob)
                {
                    sortableName = string.Format("{0}{1}{2}{3}",
                                                 (int)item.Action,
                                                 (int)item.ItemType,
                                                 item.DestinationRootPath,
                                                 ((!string.IsNullOrWhiteSpace(item.SourceFullPath)) ? item.SourceFullPath : item.DestinationFullPath));
                }
                else
                {
                    sortableName = string.Format("{0}{1}{2}",
                                                 (int)item.Action,
                                                 (int)item.ItemType,
                                                 ((!string.IsNullOrWhiteSpace(item.SourceFullPath)) ? item.SourceFullPath : item.DestinationFullPath));
                }
                switch (item.Action)
                {
                    case dodSON.Core.FileStorage.CompareAction.Ok:
                        worker_okData.Add(sortableName, worker_drow);
                        break;
                    case dodSON.Core.FileStorage.CompareAction.New:
                        worker_newData.Add(sortableName, worker_drow);
                        break;
                    case dodSON.Core.FileStorage.CompareAction.Update:
                        worker_updateData.Add(sortableName, worker_drow);
                        break;
                    case dodSON.Core.FileStorage.CompareAction.Remove:
                        worker_removeData.Add(sortableName, worker_drow);
                        break;
                    default:
                        break;
                }
            }
            // ----
            okData = new List<Helper.DetailsRowBase>(worker_okData.Values);
            newData = new List<Helper.DetailsRowBase>(worker_newData.Values);
            updateData = new List<Helper.DetailsRowBase>(worker_updateData.Values);
            removeData = new List<Helper.DetailsRowBase>(worker_removeData.Values);
        }
        public static Helper.DetailsRowBase JobFactory(JobBase job, dodSON.Core.FileStorage.ICompareResult item)
        {
            if (job is ArchiveJob)
            {
                return new Helper.ArchiveFilesDetailsRow()
                {
                    DestinationArchive = Helper.GetArchiveFilename(item),
                    Action = item.Action.ToString(),
                    SourceFile = item.SourceFullPath,
                    SourceDate = (item.SourceLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.SourceLastModifiedTimeUtc.ToString(),
                    DestinationFile = item.DestinationRootFilename,
                    DestinationDate = (item.DestinationLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.DestinationLastModifiedTimeUtc.ToString(),
                    DateDifference = (item.Action == dodSON.Core.FileStorage.CompareAction.Update)
                                        ? dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(item.SourceLastModifiedTimeUtc - item.DestinationLastModifiedTimeUtc)
                                        : ""
                };
            }
            else if (job is MirrorJob)
            {
                return new Helper.MirrorFilesDetailsRow()
                {
                    Action = item.Action.ToString(),
                    ItemType = item.ItemType.ToString(),
                    SourceFile = item.SourceFullPath,
                    SourceDate = (item.SourceLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.SourceLastModifiedTimeUtc.ToString(),
                    DestinationFile = (item.Action != dodSON.Core.FileStorage.CompareAction.New) ? item.DestinationFullPath : "",
                    DestinationDate = (item.DestinationLastModifiedTimeUtc == DateTime.MinValue) ? "" : item.DestinationLastModifiedTimeUtc.ToString(),
                    DateDifference = ((item.ItemType == dodSON.Core.FileStorage.CompareType.File) &&
                                      (item.Action == dodSON.Core.FileStorage.CompareAction.Update))
                                        ? (item.SourceLastModifiedTimeUtc - item.DestinationLastModifiedTimeUtc).ToString()
                                        : ""
                };
            }
            throw new Exception("ReportWindow.ProcessCompareItem() ArchiveJob or MirrorJob, thats all there is; there isn't any more.");
        }
        public static IEnumerable<string> GetExtensionsToStore(GlobalSettings settings)
        {
            if (settings.EnableFileExtensionsToStore)
            {
                var dude = from x in settings.FileExtensionsToStore.Split(',')
                           select '.' + x;
                return dude;
            }
            return null;
        }
        #endregion
        #region public Classes
        public abstract class DetailsRowBase { }
        public class ArchiveFilesDetailsRow
            : DetailsRowBase
        {
            public string Action { get; set; }
            public string SourceFile { get; set; }
            public string SourceDate { get; set; }
            public string DestinationArchive { get; set; }
            public string DestinationFile { get; set; }
            public string DestinationDate { get; set; }
            public string DateDifference { get; set; }
        }
        public class MirrorFilesDetailsRow
            : DetailsRowBase
        {
            public string Action { get; set; }
            public string ItemType { get; set; }
            public string SourceFile { get; set; }
            public string SourceDate { get; set; }
            public string DestinationFile { get; set; }
            public string DestinationDate { get; set; }
            public string DateDifference { get; set; }
        }
        public class ReportFileStatistics
        {
            public string JobName { get; set; }
            public string JobType { get; set; }
            public DateTime ReportDate { get; set; }
            public string ReportAge
            {
                get
                {
                    var age = DateTime.Now - ReportDate;
                    if (age < TimeSpan.FromMinutes(1.5))
                    {
                        return "Just Now!";
                    }
                    return dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(age);
                }
            }
            public DateTime LastRunDate { get; set; }
            public string LastRunAge
            {
                get
                {
                    var age = DateTime.Now - LastRunDate;
                    if (LastRunDate == DateTime.MinValue)
                    {
                        return "Never Ran";
                    }
                    else if (age < TimeSpan.FromMinutes(1.5))
                    {
                        return "Just Now!";
                    }
                    return dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(age);
                }
            }
            public DateTime JobDateCreated { get; set; }
            public string JobAge
            {
                get
                {
                    var age = DateTime.Now - JobDateCreated;
                    if (age < TimeSpan.FromMinutes(1.5))
                    {
                        return "Just Now!";
                    }
                    return dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(age);
                }
            }
            public bool JobArchiveRemoveFiles { get; set; }
            //
            public int TotalCount { get; set; }
            public int OkCount { get; set; }
            public int NewCount { get; set; }
            public int UpdateCount { get; set; }
            public int RemoveCount { get; set; }
            public int SourceTotalCount { get; set; }
            public int DestinationTotalCount { get; set; }
            public int TotalChangedCount { get; set; }
            public bool ExportOK { get; set; }
            public bool ExportNew { get; set; }
            public bool ExportUpdate { get; set; }
            public bool ExportRemove { get; set; }
            public double OkPercentChanged => ((double)OkCount / (double)TotalCount) * 100.0;
            public double NewPercentChanged => ((double)NewCount / (double)TotalCount) * 100.0;
            public double UpdatePercentChanged => ((double)UpdateCount / (double)TotalCount) * 100.0;
            public double RemovePercentChanged => ((double)RemoveCount / (double)TotalCount) * 100.0;
            public double TotalPercentChanged => ((double)TotalChangedCount / (double)TotalCount) * 100.0;
            public string JobArchiveStoragePath { get; set; }
            private readonly List<string> _JobArchiveSourcePaths = new List<string>();
            public List<string> JobArchiveSourcePaths
            {
                get { return _JobArchiveSourcePaths; }
            }
            public object JobMirrorSourcePath { get; set; }
            public object JobMirrorDestinationPath { get; set; }
        }
        public class FilesDetailsRow
        {
            public string Action { get; set; }
            public string Files { get; set; }
            public string Percentage { get; set; }
        }
        public class RunPack
        {
            public DateTime TimeStamp { get; } = DateTime.Now;
            public JobBase Job { get; set; }
            public IEnumerable<dodSON.Core.FileStorage.ICompareResult> ComparisonReport { get; set; }
        }
        // ----
        public class ComparisonResults2
            : dodSON.Core.FileStorage.ICompareResult
        {
            #region Ctor
            private ComparisonResults2()
            { }
            public ComparisonResults2(dodSON.Core.FileStorage.ICompareResult value,
                                      string destinationZipFileDestinationRootPath)
                : this()
            {
                _Holder = value;
                DestinationRootPath = destinationZipFileDestinationRootPath;
            }
            #endregion
            #region Private Fields
            private readonly dodSON.Core.FileStorage.ICompareResult _Holder = null;
            #endregion
            #region dodSON.Core.FileStorage.ICompareResult Methods
            public dodSON.Core.FileStorage.CompareType ItemType
            {
                get { return _Holder.ItemType; }
            }
            public dodSON.Core.FileStorage.CompareAction Action
            {
                get { return _Holder.Action; }
            }
            public string SourceRootPath
            {
                get { return _Holder.SourceRootPath; }
            }
            public string SourceRootFilename
            {
                get { return _Holder.SourceRootFilename; }
            }
            public string SourceFullPath
            {
                get { return _Holder.SourceFullPath; }
            }
            public DateTime SourceLastModifiedTimeUtc
            {
                get { return _Holder.SourceLastModifiedTimeUtc; }
            }
            public string DestinationRootPath { get; } = "";
            public string DestinationRootFilename
            {
                get { return _Holder.DestinationRootFilename; }
            }
            public string DestinationFullPath
            {
                get { return _Holder.DestinationFullPath; }
            }
            public DateTime DestinationLastModifiedTimeUtc
            {
                get { return _Holder.DestinationLastModifiedTimeUtc; }
            }
            public long SourceFileSizeInBytes
            {
                get { return _Holder.SourceFileSizeInBytes; }
            }
            #endregion
        }
        #endregion
    }
}
