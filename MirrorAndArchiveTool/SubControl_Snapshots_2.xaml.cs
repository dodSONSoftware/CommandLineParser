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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MirrorAndArchiveTool
{
    /// <summary>
    /// Interaction logic for SubControl_Snapshots_2.xaml
    /// </summary>
    public partial class SubControl_Snapshots_2
        : UserControl,
          ISubUserControl
    {
        #region Ctor
        public SubControl_Snapshots_2()
        {
            InitializeComponent();
            DataContext = this;
        }
        public SubControl_Snapshots_2(JobBase job,
                                      GlobalSettings settings)
            : this()
        {
            _Job = job ?? throw new ArgumentNullException(nameof(job));
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            // ----
            App.WriteDebugLog(nameof(SubControl_Snapshots_2), $"Viewing Sub Control: SNAPSHOTS, Job={_Job.Name}");
            UpdateUI();
            var token = _CancellationTokenSource.Token;
            Helper.StartBackgroundTask(() =>
            {
                App.WriteDebugLog(nameof(SubControl_Snapshots_2), $"Starting Background Task for {nameof(SubControl_Snapshots_2)}:{{Snapshot Countdown Clock}}");
                while (!_UIUpdated_FirstTime)
                {
                    dodSON.Core.Threading.ThreadingHelper.Sleep(100);
                }
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        App.WriteDebugLog(nameof(SubControl_Snapshots_2), $"Stopping Background Task for {nameof(SubControl_Snapshots_2)}:{{Snapshot Countdown Clock}}");
                        break;
                    }
                    //
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                if (_NewestSnapshotSessionTime == DateTimeOffset.MinValue)
                                {
                                    var timeDiff3 = DateTimeOffset.Now - job.DateCreate;
                                    TimeDiffStr = $"Snapshot overdue by {dodSON.Core.Common.DateTimeHelper.FormatTimeSpanVerbose(timeDiff3)}";
                                }
                                else
                                {
                                    var timeDiff = _Settings.ArchiveZipFileSnapshotsMaturityTimeLimit - (DateTimeOffset.Now - _NewestSnapshotSessionTime);
                                    if (timeDiff >= TimeSpan.Zero)
                                    {
                                        TimeDiffStr = $"Next Snapshot in {dodSON.Core.Common.DateTimeHelper.FormatTimeSpanVerbose(timeDiff)}";
                                    }
                                    else
                                    {
                                        var timeDiff2 = TimeSpan.FromMilliseconds(Math.Abs(timeDiff.TotalMilliseconds));
                                        TimeDiffStr = $"Snapshot overdue by {dodSON.Core.Common.DateTimeHelper.FormatTimeSpanVerbose(timeDiff2)}";
                                    }
                                }
                            }
                            catch { }
                        });
                    }
                    catch { }
                    //
                    dodSON.Core.Threading.ThreadingHelper.Sleep(1000);
                }
            });
        }
        #endregion
        #region Private Fields
        private readonly JobBase _Job = null;
        private readonly GlobalSettings _Settings = null;
        //
        private Helper.SnapshotInformation _SnapshotInfo;
        private readonly System.Threading.CancellationTokenSource _CancellationTokenSource = new System.Threading.CancellationTokenSource();
        private DateTimeOffset _NewestSnapshotSessionTime = DateTimeOffset.MinValue;
        private bool _UIUpdated_FirstTime = false;
        #endregion
        #region Dependency Properties
        public string TotalCount
        {
            get { return (string)GetValue(TotalCountProperty); }
            set { SetValue(TotalCountProperty, value); }
        }
        public static readonly DependencyProperty TotalCountProperty = DependencyProperty.Register("TotalCount", typeof(string), typeof(SubControl_Snapshots_2), new PropertyMetadata(""));
        public string TotalSize
        {
            get { return (string)GetValue(TotalSizeProperty); }
            set { SetValue(TotalSizeProperty, value); }
        }
        public static readonly DependencyProperty TotalSizeProperty = DependencyProperty.Register("TotalSize", typeof(string), typeof(SubControl_Snapshots_2), new UIPropertyMetadata(""));
        public string TotalCompression
        {
            get { return (string)GetValue(TotalCompressionProperty); }
            set { SetValue(TotalCompressionProperty, value); }
        }
        public static readonly DependencyProperty TotalCompressionProperty = DependencyProperty.Register("TotalCompression", typeof(string), typeof(SubControl_Snapshots_2), new UIPropertyMetadata(""));
        public string AgeString
        {
            get { return (string)GetValue(AgeStringProperty); }
            set { SetValue(AgeStringProperty, value); }
        }
        public static readonly DependencyProperty AgeStringProperty = DependencyProperty.Register("AgeString", typeof(string), typeof(SubControl_Snapshots_2), new PropertyMetadata(""));
        // ----
        public double DeleteAllSnapshotsOpacity
        {
            get { return (double)GetValue(DeleteAllSnapshotsOpacityProperty); }
            set { SetValue(DeleteAllSnapshotsOpacityProperty, value); }
        }
        public static readonly DependencyProperty DeleteAllSnapshotsOpacityProperty = DependencyProperty.Register("DeleteAllSnapshotsOpacity", typeof(double), typeof(SubControl_Snapshots_2), new PropertyMetadata(1.0));
        public double CreateNewSnapshotsOpacity
        {
            get { return (double)GetValue(CreateNewSnapshotsOpacityProperty); }
            set { SetValue(CreateNewSnapshotsOpacityProperty, value); }
        }
        public static readonly DependencyProperty CreateNewSnapshotsOpacityProperty = DependencyProperty.Register("CreateNewSnapshotsOpacity", typeof(double), typeof(SubControl_Snapshots_2), new PropertyMetadata(1.0));
        // ----
        public string TimeDiffStr
        {
            get { return (string)GetValue(TimeDiffStrProperty); }
            set { SetValue(TimeDiffStrProperty, value); }
        }
        public static readonly DependencyProperty TimeDiffStrProperty =
            DependencyProperty.Register("TimeDiffStr", typeof(string), typeof(SubControl_Snapshots_2), new PropertyMetadata(""));
        #endregion
        #region Commands
        private DelegateCommand _DeleteAllSnapshotsCommand;
        public DelegateCommand DeleteAllSnapshotsCommand
        {
            get
            {
                if (_DeleteAllSnapshotsCommand == null)
                {
                    _DeleteAllSnapshotsCommand = new DelegateCommand(
                    (x) =>
                    {
                        // ######## execute
                        if (MessageBox.Show(Window.GetWindow(this),
                                  "Delete All Snapshots" + Environment.NewLine + Environment.NewLine + "Are you sure?",
                                  "Delete All Snapshots",
                                  MessageBoxButton.YesNo,
                                  MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            var message = $"Deleting All Snapshots for {_Job.Name}, {TotalCount}, {dodSON.Core.Common.ByteCountHelper.ToString(_SnapshotInfo.CompressedBytes)}   ({_SnapshotInfo.CompressedBytes:N0} bytes)";
                            App.WriteDebugLog(nameof(SubControl_Snapshots_2), message);
                            App.WriteLog(nameof(SubControl_Snapshots_2), App.LogCategory.Archive, message);
                            foreach (var session in _SnapshotInfo.Sessions.Values)
                            {
                                foreach (var fInfo in session.Files)
                                {
                                    fInfo.FileInfo.Delete();
                                }
                            }
                            //
                            UpdateUI();
                        }
                    },
                    (x) =>
                    {
                        // ######## query
                        var dude = ((_SnapshotInfo != null) && (_SnapshotInfo.Sessions.Count > 0));
                        if (dude) { DeleteAllSnapshotsOpacity = 1.0; }
                        else { DeleteAllSnapshotsOpacity = 0.2; }
                        return dude;
                    });
                }
                //
                return _DeleteAllSnapshotsCommand;
            }
        }
        // ================
        private DelegateCommand _CreateNewSnapshotsCommand;
        public DelegateCommand CreateNewSnapshotsCommand
        {
            get
            {
                if (_CreateNewSnapshotsCommand == null)
                {
                    _CreateNewSnapshotsCommand = new DelegateCommand(
                    (x) =>
                    {
                        // ######## execute
                        if (MessageBox.Show(Window.GetWindow(this),
                                  "Create New Snapshots Now" + Environment.NewLine + Environment.NewLine + "Are you sure?",
                                  "Create New Snapshots Now",
                                  MessageBoxButton.YesNo,
                                  MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {

                            var wind = new SnapshotProcessingWindow((ArchiveJob)_Job, _Settings, DateTime.Now, true);
                            wind.Owner = Window.GetWindow(this);
                            wind.ShowDialog();
                            UpdateUI();
                        }
                    },
                    (x) =>
                    {
                        // ######## query
                        var dude = (System.IO.Directory.Exists((_Job as ArchiveJob).ArchiveRootPath)) &&
                                   (System.IO.Directory.GetFiles((_Job as ArchiveJob).ArchiveRootPath, $"{_Job.Name}.*.Archive.zip", System.IO.SearchOption.TopDirectoryOnly).Length > 0);
                        if (dude) { CreateNewSnapshotsOpacity = 1.0; }
                        else { CreateNewSnapshotsOpacity = 0.2; }
                        return dude;
                    });
                }
                //
                return _CreateNewSnapshotsCommand;
            }
        }
        // ================
        private DelegateCommand _OpenSnapshotsDirectoryCommand;
        public DelegateCommand OpenSnapshotsDirectoryCommand
        {
            get
            {
                if (_OpenSnapshotsDirectoryCommand == null)
                {
                    _OpenSnapshotsDirectoryCommand = new DelegateCommand((x) =>
                    {
                        // execute
                        if (System.IO.Directory.Exists(_Settings.ArchiveZipFileSnapshotsRootPath))
                        {
                            try { System.Diagnostics.Process.Start(_Settings.ArchiveZipFileSnapshotsRootPath); }
                            catch { }
                        }
                    },
                    (x) =>
                    {
                        // query
                        return true;
                    });
                }
                return _OpenSnapshotsDirectoryCommand;
            }
        }
        // ================ ExpandAllSnapshotsCommand
        private DelegateCommand _ExpandAllSnapshotsCommand;
        public DelegateCommand ExpandAllSnapshotsCommand
        {
            get
            {
                if (_ExpandAllSnapshotsCommand == null)
                {
                    _ExpandAllSnapshotsCommand = new DelegateCommand(
                        (x) =>
                        {
                            foreach (var item in SnapshotSets_StackPanel.Children)
                            {
                                (item as SnapshotListItem).Expand();
                            }
                        },
                        (x) =>
                        {
                            // ######## query
                            var dude = ((_SnapshotInfo != null) && (_SnapshotInfo.Sessions.Count > 0));
                            return dude;
                        });
                }
                return _ExpandAllSnapshotsCommand;
            }
        }
        // ================ CollapseAllSnapshotsCommand
        private DelegateCommand _CollapseAllSnapshotsCommand;
        public DelegateCommand CollapseAllSnapshotsCommand
        {
            get
            {
                if (_CollapseAllSnapshotsCommand == null)
                {
                    _CollapseAllSnapshotsCommand = new DelegateCommand(
                        (x) =>
                        {
                            foreach (var item in SnapshotSets_StackPanel.Children)
                            {
                                (item as SnapshotListItem).Collapse();
                            }
                        },
                        (x) =>
                        {
                            // ######## query
                            var dude = ((_SnapshotInfo != null) && (_SnapshotInfo.Sessions.Count > 0));
                            return dude;
                        });
                }
                return _CollapseAllSnapshotsCommand;
            }
        }
        // ================
        #endregion
        #region ISubUserControl Methods
        public string Title
        {
            get { return "Snapshots"; }
        }
        public string Key
        {
            get { return "SNAPSHOTS"; }
        }
        public void Refresh(IEnumerable<dodSON.Core.FileStorage.ICompareResult> report)
        {
            UpdateUI();
        }
        public void Shutdown()
        {
            _CancellationTokenSource.Cancel();
        }
        #endregion
        #region Private Methods
        private void UpdateUI()
        {
            Task.Run(() =>
            {
                // ######## display wait icon
                Dispatcher.Invoke(() =>
                {
                    imageRecommendationWaitIcon.Visibility = Visibility.Visible;
                    imageRecommendationWaitIcon2.Visibility = Visibility.Visible;
                    SnapshotSets_StackPanel.Children.Clear();
                });
                // ######## refresh snapshot information
                UpdateSnapshotData();
                _NewestSnapshotSessionTime = DateTimeOffset.MinValue;
                // ######## update snapshot sessions list
                if ((_SnapshotInfo == null) || (_SnapshotInfo.Sessions.Count == 0))
                {
                    // no snapshots
                    Dispatcher.Invoke(() => { TimeDiffStr = ""; });
                }
                else
                {
                    // snapshots found
                    foreach (var session in from x in _SnapshotInfo.Sessions.Values
                                            orderby x.CreatedDate descending
                                            select x)
                    {
                        if (session.CreatedDate >= _NewestSnapshotSessionTime) { _NewestSnapshotSessionTime = session.CreatedDate; }
                        Dispatcher.Invoke(() => { SnapshotSets_StackPanel.Children.Add(new SnapshotListItem(session)); });
                    }
                }
                // ######## update status bar
                Dispatcher.Invoke(() =>
                {
                    UpdateStatusBar();
                    imageRecommendationWaitIcon.Visibility = Visibility.Collapsed;
                    imageRecommendationWaitIcon2.Visibility = Visibility.Collapsed;
                    // ######## WPF Refresh
                    DataContext = null;
                    DataContext = this;
                });
                //
                _UIUpdated_FirstTime = true;
            });
        }
        private void UpdateSnapshotData()
        {
            // get snapshot data
            var dude = Helper.GetSnapshotData(_Settings, () => { UpdateUI(); }, _Job.Name);
            if (dude.Count > 0) { _SnapshotInfo = dude.FirstOrDefault().Value; }
            else { _SnapshotInfo = null; }
        }
        private void UpdateStatusBar()
        {
            imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Collapsed;
            imageRecommendationWaitIcon2.Visibility = System.Windows.Visibility.Collapsed;
            //
            if (_SnapshotInfo == null)
            {
                TotalCount = "0 Sessions";
                TotalSize = "0";
                TotalCompression = "0.0%";
                AgeString = "";
            }
            else
            {
                TotalCount = _SnapshotInfo.TotalSessionsString;
                TotalSize = _SnapshotInfo.CompressedBytesString;
                TotalCompression = _SnapshotInfo.CompressedPercentageString;
                if (_SnapshotInfo.Age == TimeSpan.Zero) { AgeString = ""; }
                else { AgeString = $"{_SnapshotInfo.AgeString}   ({_SnapshotInfo.NewestSessionDate:d} to {_SnapshotInfo.OldestSessionDate:d})"; }
            }
        }
        // --------
        #endregion



        #region Unused Code
        //private string ParseFilenameTitle(string filename)
        //{
        //    filename = System.IO.Path.GetFileNameWithoutExtension(filename);
        //    var parts = filename.Split('.');
        //    if (parts.Length > 1)
        //    {
        //        return parts[1];
        //    }
        //    return "";
        //}
        //private string ParseFilenameDate(string filename)
        //{
        //    filename = System.IO.Path.GetFileNameWithoutExtension(filename);
        //    var parts = filename.Split('.');
        //    if (parts.Length > 1)
        //    {
        //        return new DateTime(int.Parse(parts[2].Substring(0, 4)),
        //                            int.Parse(parts[2].Substring(4, 2)),
        //                            int.Parse(parts[2].Substring(6, 2)),
        //                            int.Parse(parts[2].Substring(9, 2)),
        //                            int.Parse(parts[2].Substring(11, 2)),
        //                            int.Parse(parts[2].Substring(13, 2))).ToString();
        //    }
        //    return "";
        //}
        //private string ParseSortableFilenameDate(string filename)
        //{
        //    filename = System.IO.Path.GetFileNameWithoutExtension(filename);
        //    var parts = filename.Split('.');
        //    if (parts.Length > 1)
        //    {
        //        return parts[2];
        //    }
        //    return "";
        //}
        #endregion
    }
}
