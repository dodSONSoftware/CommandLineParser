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
    /// Interaction logic for SubControl_Snapshots.xaml
    /// </summary>
    public partial class SubControl_Snapshots
        : UserControl,
          ISubUserControl
    {
        #region Ctor
        public SubControl_Snapshots()
        {
            InitializeComponent();
            DataContext = this;
        }
        public SubControl_Snapshots(JobBase job,
                                    GlobalSettings settings)
            : this()
        {
            _Job = job ?? throw new ArgumentNullException(nameof(job));
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            // ----
            UpdateUI();
            App.WriteDebugLog(nameof(SubControl_Snapshots), $"Viewing Sub Control: SNAPSHOTS, Job={_Job.Name}");
        }
        #endregion
        #region Private Fields
        private readonly JobBase _Job = null;
        private readonly GlobalSettings _Settings = null;
        // ----
        private SnapshotFilesReportInfo _CurrentReportInfo = null;
        private System.Threading.CancellationTokenSource _CancelTokenSource = new System.Threading.CancellationTokenSource();
        #endregion
        #region Dependency Properties
        public IEnumerable<SnapshotFilesReportInfo> Reports
        {
            get { return (IEnumerable<SnapshotFilesReportInfo>)GetValue(ReportsProperty); }
            set { SetValue(ReportsProperty, value); }
        }
        public static readonly DependencyProperty ReportsProperty =
            DependencyProperty.Register("Reports", typeof(IEnumerable<SnapshotFilesReportInfo>), typeof(SubControl_Snapshots), new UIPropertyMetadata(null));
        // ----
        public string TotalSize
        {
            get { return (string)GetValue(TotalSizeProperty); }
            set { SetValue(TotalSizeProperty, value); }
        }
        public static readonly DependencyProperty TotalSizeProperty =
            DependencyProperty.Register("TotalSize", typeof(string), typeof(SubControl_Snapshots), new UIPropertyMetadata(""));
        // ----
        public string TotalCompression
        {
            get { return (string)GetValue(TotalCompressionProperty); }
            set { SetValue(TotalCompressionProperty, value); }
        }
        public static readonly DependencyProperty TotalCompressionProperty =
            DependencyProperty.Register("TotalCompression", typeof(string), typeof(SubControl_Snapshots), new UIPropertyMetadata(""));
        #endregion
        #region User Control Events
        private void buttonDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Window.GetWindow(this),
                                "Delete All Snapshots" + Environment.NewLine + "Are you sure?",
                                "Delete All Snapshots",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                //foreach (SnapshotFilesReportInfo item in dataGridData.Items)
                //{
                //    if (System.IO.File.Exists(item.Filename))
                //    {
                //        try { System.IO.File.Delete(item.Filename); }
                //        catch { }
                //    }
                //}
                //UpdateUI();
            }
        }
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Window.GetWindow(this),
                                   "Delete Selected Snapshots" + Environment.NewLine + "Are you sure?",
                                   "Delete Selected Snapshots",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                //foreach (SnapshotFilesReportInfo item in dataGridData.SelectedItems)
                //{
                //    if (System.IO.File.Exists(item.Filename))
                //    {
                //        try { System.IO.File.Delete(item.Filename); }
                //        catch { }
                //    }
                //}
                //UpdateUI();
            }
        }
        private void buttonCreateSnapshotsNow_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Window.GetWindow(this),
                                   "Create New Snapshots Now" + Environment.NewLine + "Are you sure?",
                                   "Create New Snapshots Now",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {

                var wind = new SnapshotProcessingWindow((ArchiveJob)_Job, _Settings, DateTime.Now, true);
                wind.Owner = Window.GetWindow(this);
                wind.ShowDialog();
                UpdateUI();
            }
        }
        // ----
        private void dataGridData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var dude = e.AddedItems[0] as SnapshotFilesReportInfo;
                if (dude != null) { _CurrentReportInfo = dude; }
                else { _CurrentReportInfo = null; }
            }
        }
        private void dataGridData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_CurrentReportInfo != null)
            {
                if (System.IO.File.Exists(_CurrentReportInfo.Filename))
                {
                }
                else
                {
                }
            }
        }
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
            _CancelTokenSource.Cancel();
        }
        #endregion
        #region Private Methods
        private void UpdateUI()
        {
            // process all files in target directory
            var list = new List<SnapshotFilesReportInfo>();
            foreach (var filename in System.IO.Directory.GetFiles(_Settings.ArchiveZipFileSnapshotsRootPath, string.Format("{0}.*.Snapshot.zip", _Job.Name), System.IO.SearchOption.TopDirectoryOnly))
            {
                list.Add(new SnapshotFilesReportInfo()
                {
                    Name = ParseFilenameTitle(filename),
                    DateCreatedString = ParseFilenameDate(filename),
                    DateCreated = ParseSortableFilenameDate(filename),
                    Filename = filename,
                });
            }
            // build reports
            Reports = from item in list
                      orderby item.DateCreated descending
                      select item;
            // process extra information in parallel (in a task)
            long totalCompressedSize = 0;
            long totalUncompressedSize = 0;
            System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Tasks.Parallel.ForEach<SnapshotFilesReportInfo>(list, (x) =>
                {
                    if (!_CancelTokenSource.Token.IsCancellationRequested)
                    {
                        if (System.IO.File.Exists(x.Filename))
                        {
                            var dude = Helper.GetZipStore(x.Filename,
                                                          System.IO.Path.GetTempPath(),
                                                          false,
                                                          null);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                x.TotalFiles = dude.Count.ToString("N0");
                                x.CompressionRatio = ((1.0 - dude.CompressionRatio) * 100.0).ToString("N0") + "%";
                                x.CompressedSize = dodSON.Core.Common.ByteCountHelper.ToString(dude.CompressedSize);
                                totalCompressedSize += dude.CompressedSize;
                                totalUncompressedSize += dude.UncompressedSize;
                            }));
                        }
                    }
                });
                if (!_CancelTokenSource.Token.IsCancellationRequested)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Collapsed;
                        TotalSize = dodSON.Core.Common.ByteCountHelper.ToString(totalCompressedSize);
                        var value = ((1.0 - ((double)((double)totalCompressedSize / (double)totalUncompressedSize))) * 100.0);
                        TotalCompression = (double.IsNaN(value)) ? "0%" : value.ToString("N0") + "%";
                    }));
                }
            }, _CancelTokenSource.Token);
        }
        private string ParseFilenameTitle(string filename)
        {
            filename = System.IO.Path.GetFileNameWithoutExtension(filename);
            var parts = filename.Split('.');
            if (parts.Length > 1)
            {
                return parts[1];
            }
            return "";
        }
        private string ParseFilenameDate(string filename)
        {
            filename = System.IO.Path.GetFileNameWithoutExtension(filename);
            var parts = filename.Split('.');
            if (parts.Length > 1)
            {
                return new DateTime(int.Parse(parts[2].Substring(0, 4)),
                                    int.Parse(parts[2].Substring(4, 2)),
                                    int.Parse(parts[2].Substring(6, 2)),
                                    int.Parse(parts[2].Substring(9, 2)),
                                    int.Parse(parts[2].Substring(11, 2)),
                                    int.Parse(parts[2].Substring(13, 2))).ToString();
            }
            return "";
        }
        private DateTime ParseSortableFilenameDate(string filename)
        {
            return DateTime.Parse(ParseFilenameDate(filename));
        }
        #endregion
    }
    #region Public Class: ReportInfo
    public class SnapshotFilesReportInfo
          : System.ComponentModel.INotifyPropertyChanged
    {
        #region System.ComponentModel.INotifyPropertyChanged Methods
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Will raise the property changed events with the provided property name.
        /// </summary>
        /// <param name="propertyName">The name of the property which has changed.</param>
        protected void RaisePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName)); }
        }
        #endregion
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public string DateCreatedString { get; set; }
        private string _Filename = "";
        public string Filename
        {
            get { return _Filename; }
            set
            {
                _Filename = value;
                RaisePropertyChangedEvent("Filename");
            }
        }
        private string _TotalFiles = "";
        public string TotalFiles
        {
            get { return _TotalFiles; }
            set
            {
                _TotalFiles = value;
                RaisePropertyChangedEvent("TotalFiles");
            }
        }
        private string _CompressionRatio = "";
        public string CompressionRatio
        {
            get { return _CompressionRatio; }
            set
            {
                _CompressionRatio = value;
                RaisePropertyChangedEvent("CompressionRatio");
            }
        }
        private string _CompressedSize = "";
        public string CompressedSize
        {
            get { return _CompressedSize; }
            set
            {
                _CompressedSize = value;
                RaisePropertyChangedEvent("CompressedSize");
            }
        }
    }
    #endregion
}