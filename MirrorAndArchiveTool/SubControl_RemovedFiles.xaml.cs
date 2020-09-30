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
    /// Interaction logic for SubControl_RemovedFiles.xaml
    /// </summary>
    public partial class SubControl_RemovedFiles
        : System.Windows.Controls.UserControl,
          ISubUserControl
    {
        #region Ctor
        public SubControl_RemovedFiles()
        {
            InitializeComponent();
            DataContext = this;
        }
        public SubControl_RemovedFiles(JobBase job,
                                       IEnumerable<dodSON.Core.FileStorage.ICompareResult> report,
                                       GlobalSettings settings)
            : this()
        {
            _Job = job ?? throw new ArgumentNullException("job");
            _Report = report;
            _Settings = settings;
            // ----
            UpdateUI();
            App.WriteDebugLog(nameof(SubControl_RemovedFiles), $"Viewing Sub Control: REMOVED FILES, Job={_Job.Name}");
        }
        #endregion
        #region Private Fields
        private readonly JobBase _Job = null;
        private IEnumerable<dodSON.Core.FileStorage.ICompareResult> _Report = null;
        private GlobalSettings _Settings = null;
        // ----
        private ZipFilesReportInfo _CurrentReportInfo = null;
        private System.Threading.CancellationTokenSource _CancelTokenSource = new System.Threading.CancellationTokenSource();
        //
        private DateTimeOffset _FirstDateTime;
        private DateTimeOffset _LastDateTime;
        private TimeSpan _DateTimeRange;
        #endregion
        #region Dependency Properties
        public IEnumerable<ZipFilesReportInfo> Reports
        {
            get { return (IEnumerable<ZipFilesReportInfo>)GetValue(ReportsProperty); }
            set { SetValue(ReportsProperty, value); }
        }
        public static readonly DependencyProperty ReportsProperty =
            DependencyProperty.Register("Reports", typeof(IEnumerable<ZipFilesReportInfo>), typeof(SubControl_RemovedFiles), new UIPropertyMetadata(null));
        // ----
        public string TotalSize
        {
            get { return (string)GetValue(TotalSizeProperty); }
            set { SetValue(TotalSizeProperty, value); }
        }
        public static readonly DependencyProperty TotalSizeProperty =
            DependencyProperty.Register("TotalSize", typeof(string), typeof(SubControl_RemovedFiles), new UIPropertyMetadata(""));
        // ----
        public string TotalCompression
        {
            get { return (string)GetValue(TotalCompressionProperty); }
            set { SetValue(TotalCompressionProperty, value); }
        }
        public static readonly DependencyProperty TotalCompressionProperty =
            DependencyProperty.Register("TotalCompression", typeof(string), typeof(SubControl_RemovedFiles), new UIPropertyMetadata(""));
        // ----
        public string DateRangeText
        {
            get { return (string)GetValue(DateRangeTextProperty); }
            set { SetValue(DateRangeTextProperty, value); }
        }
        public static readonly DependencyProperty DateRangeTextProperty = DependencyProperty.Register("DateRangeText", typeof(string), typeof(SubControl_RemovedFiles), new PropertyMetadata(""));
        #endregion
        #region User Control Events
        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {

            OpenArchives();
        }
        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveArchiveToFolder();
        }
        private void buttonDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show(Window.GetWindow(this),
                                "Delete All Removed Files Archives" + Environment.NewLine + Environment.NewLine + "Are you sure?",
                                "Delete All Archives",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                foreach (ZipFilesReportInfo item in dataGridData.Items)
                {
                    if (System.IO.File.Exists(item.Filename))
                    {
                        try { System.IO.File.Delete(item.Filename); }
                        catch { }
                    }
                }
                UpdateUI();
            }
        }
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            DeletedSelectedArchives();
        }
        // ----
        private void dataGridData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var dude = e.AddedItems[0] as ZipFilesReportInfo;
                if (dude != null) { _CurrentReportInfo = dude; }
                else { _CurrentReportInfo = null; }
            }
        }
        private void dataGridData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ExtractAndShowReport();
        }
        private void OnHyperLinkClick(object o, RoutedEventArgs e)
        {
            ExtractAndShowReport();
        }
        private void ExtractAndShowReport()
        {
            if (_CurrentReportInfo != null)
            {
                var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                var parts = System.IO.Path.GetFileName(_CurrentReportInfo.Filename).Split('.');
                var rootFilename = string.Format($"{parts[0]}.{parts[1]}.JobReport.txt");
                var filename = "";
                try
                {
                    try
                    {
                        filename = zip.Extract(zip.Get(rootFilename), true);
                        //
                        if (System.IO.File.Exists(filename))
                        {
                            try
                            {
                                var wind = new ProcessingReportWindow(_Job, filename, _CancelTokenSource);
                                wind.Owner = Window.GetWindow(this);
                                if ((wind.ShowDialog() ?? false))
                                {
                                    var dude = new SubControl_Report(false, wind.Stats, wind.OkData, wind.NewData, wind.UpdateData, wind.RemoveData, _Settings);
                                    var wind2 = new SubUserControlWindow(dude);
                                    wind2.Owner = Window.GetWindow(this);
                                    wind2.ShowDialog();
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            MainWindow.DisplayErrorMessageBox("File not Found", string.Format($"Cannot find report file{Environment.NewLine}{filename}"));
                        }
                    }
                    catch { }
                }
                finally
                {
                    try { System.IO.File.Delete(filename); }
                    catch { }
                }
            }
        }
        //private string ConvertDateStringToReportFilename(string filename)
        //{
        //    var dt = DateTime.Parse(filename);
        //    return string.Format("{0}", _CurrentReportInfo.ReportName);
        //}
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var dude = sender as System.Windows.Controls.ContextMenu;
            var dudeHold = dude.DataContext;
            dude.DataContext = null;
            dude.DataContext = dudeHold;
        }
        #endregion
        #region Commands
        public ICommand OpenArchiveCommand => new DelegateCommand(x =>
        {
            // execute
            OpenArchives();
        },
        x =>
        {
            // can execute
            return (_CurrentReportInfo != null);
        });
        public ICommand SaveArchiveCommand => new DelegateCommand(x =>
        {
            // execute
            if (_CurrentReportInfo != null)
            {
                SaveArchiveToFolder();
            }
        },
        x =>
        {
            // can execute
            return (_CurrentReportInfo != null);
        });
        public ICommand DeleteArchiveCommand => new DelegateCommand(x =>
        {
            // execute
            if (_CurrentReportInfo != null)
            {
                DeletedSelectedArchives();
            }
        },
        x =>
        {
            // can execute
            if (_CurrentReportInfo != null)
            {
                return System.IO.File.Exists(_CurrentReportInfo.Filename);
            }
            return false;
        });
        public ICommand CopyToClipboardCommand => new DelegateCommand(x =>
        {
            // execute
            if (_CurrentReportInfo != null)
            {
                Clipboard.SetText(System.IO.Path.GetDirectoryName(_CurrentReportInfo.Filename));
            }
        },
        x =>
        {
            // can execute            
            return (_CurrentReportInfo != null);
        });
        public ICommand CopyFilenameToClipboardCommand => new DelegateCommand(x =>
        {
            // execute
            if (_CurrentReportInfo != null)
            {
                Clipboard.SetText(_CurrentReportInfo.Filename);
            }
        },
        x =>
        {
            // can execute            
            return (_CurrentReportInfo != null);
        });
        //public ICommand ShowInHistoryCommand => new RelayCommand(x =>
        //{
        //    // execute
        //    if (_CurrentReportInfo != null)
        //    {

        //    }
        //},
        //x =>
        //{
        //    // can execute            
        //    return (_CurrentReportInfo != null);
        //});
        #endregion
        #region ISubUserControl Methods
        public string Title
        {
            get { return "Removed Files Archives"; }
        }
        public string Key
        {
            get { return "REMOVEDFILES"; }
        }
        public void Refresh(IEnumerable<dodSON.Core.FileStorage.ICompareResult> report)
        {
            _Report = report;
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
            System.Threading.Tasks.Task.Run(() =>
            {
                _FirstDateTime = DateTimeOffset.MaxValue;
                _LastDateTime = DateTimeOffset.MinValue;
                _DateTimeRange = TimeSpan.Zero;
                // process all files in target directory into the Report
                var list = new List<ZipFilesReportInfo>();
                foreach (var filename in System.IO.Directory.GetFiles(_Settings.RemovedFilesArchiveRootPath, string.Format($"{_Job.Name}.*.RemovedFiles.zip"), System.IO.SearchOption.TopDirectoryOnly))
                {
                    var fileDate = ParseFilenameDate(filename);
                    if (DateTimeOffset.TryParse(fileDate, out DateTimeOffset fileDateActual))
                    {
                        // process date information
                        if (fileDateActual < _FirstDateTime) { _FirstDateTime = fileDateActual; }
                        if (fileDateActual > _LastDateTime) { _LastDateTime = fileDateActual; }
                        _DateTimeRange = _LastDateTime - _FirstDateTime;
                    }
                    list.Add(new ZipFilesReportInfo()
                    {
                        Name = ParseFilenameTitle(filename),
                        DateCreated = fileDate,
                        SortableDateCreated = ParseSortableFilenameDate(filename),
                        Filename = filename,
                        ReportName = "",
                        ReportName2 = string.Format("{0}.{1}.JobReport.txt", System.IO.Path.GetFileName(filename.Split('.')[0]), System.IO.Path.GetFileName(filename.Split('.')[1]))
                    });
                }
                // sort the Report
                this.Dispatcher.Invoke(new Action(() =>
                {
                    Reports = from item in list
                              orderby item.SortableDateCreated descending
                              select item;
                }));
                // process extra information in parallel (in a task)
                long totalCompressedSize = 0;
                long totalUncompressedSize = 0;
                var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                System.Threading.Tasks.Parallel.ForEach<ZipFilesReportInfo>(list, (x) =>
                {
                    if (!_CancelTokenSource.Token.IsCancellationRequested)
                    {
                        if (System.IO.File.Exists(x.Filename))
                        {
                            if (zip.Contains(x.ReportName2))
                            {
                                x.ReportName = x.ReportName2;
                            }
                            var dude = Helper.GetZipStore(x.Filename,
                                                          System.IO.Path.GetTempPath(),
                                                          false,
                                                          null);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                x.TotalFiles = dude.Count.ToString("N0");
                                x.CompressionRatio = (dude.CompressionPercentage * 100.0).ToString("N0") + "%";
                                x.CompressedSize = dodSON.Core.Common.ByteCountHelper.ToString(dude.CompressedSize);
                                x.UncompressedSize = dodSON.Core.Common.ByteCountHelper.ToString(dude.UncompressedSize);
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
                        if (_DateTimeRange == TimeSpan.Zero) { DateRangeText = ""; }
                        else { DateRangeText = $"{dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(_DateTimeRange)}   ({_FirstDateTime:d} to {_LastDateTime:d})"; }
                    }));
                }
            }, _CancelTokenSource.Token);
        }
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
        private void OpenArchives()
        {
            foreach (ZipFilesReportInfo item in dataGridData.SelectedItems)
            {
                if (System.IO.File.Exists(item.Filename))
                {
                    try { System.Diagnostics.Process.Start(item.Filename); }
                    catch { }
                }
            }
        }
        private void SaveArchiveToFolder()
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var extractPath = folderDialog.SelectedPath;
                var count = 0;
                var list = new List<FileCopyInfo>();
                //
                foreach (ZipFilesReportInfo item in dataGridData.SelectedItems)
                {
                    if (System.IO.File.Exists(item.Filename))
                    {
                        ++count;
                        list.Add(new FileCopyInfo()
                        {
                            SourceFilename = item.Filename,
                            DestinationFilename = System.IO.Path.Combine(folderDialog.SelectedPath, System.IO.Path.GetFileName(item.Filename))
                        });
                    }
                }
                // 
                var wind = new CopyFiles("Save Archives", $"Saving Archives", $"Archives Saved", list);
                wind.ShowDialog();
                //
                MessageBox.Show(Window.GetWindow(this),
                        $"{count:N0} Archives Saved.{Environment.NewLine}{Environment.NewLine}Path:{Environment.NewLine}{extractPath}",
                        "Archives Saved",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }
        }
        private void DeletedSelectedArchives()
        {
            if (MessageBox.Show(Window.GetWindow(this),
                                "Delete Selected Removed Files Archives" + Environment.NewLine + Environment.NewLine + "Are you sure?",
                                "Delete Selected Archives",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                foreach (ZipFilesReportInfo item in dataGridData.SelectedItems)
                {
                    if (System.IO.File.Exists(item.Filename))
                    {
                        try { System.IO.File.Delete(item.Filename); }
                        catch { }
                    }
                }
                UpdateUI();
            }
        }
        #endregion
    }
    #region Public Class: ReportInfo
    public class ZipFilesReportInfo
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
        public string DateCreated { get; set; }
        public string SortableDateCreated { get; set; }
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
        private string _ReportName = "";
        public string ReportName
        {
            get { return _ReportName; }
            set
            {
                _ReportName = value;
                RaisePropertyChangedEvent("ReportName");
            }
        }
        public string ReportName2 { get; set; }
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
        private string _UncompressedSize = "";
        public string UncompressedSize
        {
            get { return _UncompressedSize; }
            set
            {
                _UncompressedSize = value;
                RaisePropertyChangedEvent("UncompressedSize");
            }
        }
    }
    #endregion
}
