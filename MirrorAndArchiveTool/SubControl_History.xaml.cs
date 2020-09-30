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
    /// Interaction logic for SubControl_History.xaml
    /// </summary>
    public partial class SubControl_History
        : UserControl,
          ISubUserControl
    {
        #region Ctor
        public SubControl_History()
        {
            InitializeComponent();
            DataContext = this;
        }
        public SubControl_History(JobBase job,
                                  IEnumerable<dodSON.Core.FileStorage.ICompareResult> report,
                                  GlobalSettings settings)
            : this()
        {
            _Job = job ?? throw new ArgumentNullException(nameof(job));
            _Report = report;
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            // ----
            UpdateUI();
            App.WriteDebugLog(nameof(SubControl_History), $"Viewing Sub Control: HISTORY, Job={_Job.Name}");
        }
        #endregion
        #region Private Fields
        private readonly JobBase _Job = null;
        private IEnumerable<dodSON.Core.FileStorage.ICompareResult> _Report = null;
        private readonly GlobalSettings _Settings = null;
        // ----
        private ReportInfo _CurrentReportInfo = null;
        private System.Threading.CancellationTokenSource _CancelTokenSource = new System.Threading.CancellationTokenSource();
        //
        private DateTimeOffset _FirstDateTime;
        private DateTimeOffset _LastDateTime;
        private TimeSpan _DateTimeRange;
        #endregion
        #region Dependency Properties
        public IEnumerable<ReportInfo> Reports
        {
            get { return (IEnumerable<ReportInfo>)GetValue(ReportsProperty); }
            set { SetValue(ReportsProperty, value); }
        }
        public static readonly DependencyProperty ReportsProperty =
            DependencyProperty.Register("Reports", typeof(IEnumerable<ReportInfo>), typeof(SubControl_History), new UIPropertyMetadata(null));
        public string DateRangeText
        {
            get { return (string)GetValue(DateRangeTextProperty); }
            set { SetValue(DateRangeTextProperty, value); }
        }
        public static readonly DependencyProperty DateRangeTextProperty = DependencyProperty.Register("DateRangeText", typeof(string), typeof(SubControl_History), new PropertyMetadata(""));
        #endregion
        #region User Control Events
        private void buttonDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Window.GetWindow(this),
                                "Delete All Reports" + Environment.NewLine + Environment.NewLine + "Are you sure?",
                                "Delete All Reports",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                foreach (ReportInfo item in dataGridData.Items)
                {
                    if (zip.Contains(item.Filename)) { zip.Delete(item.Filename); }
                }
                zip.Save(true);
                UpdateUI();
            }
        }
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteReports();
        }
        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            ExtractAndShowReport();
        }
        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            SaveReports();
        }
        // ----
        private void dataGridData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var dude = e.AddedItems[0] as ReportInfo;
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
        #endregion
        #region Commands
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var dude = sender as System.Windows.Controls.ContextMenu;
            var dudeHold = dude.DataContext;
            dude.DataContext = null;
            dude.DataContext = dudeHold;
        }
        public ICommand OpenReportsCommand => new DelegateCommand(x =>
        {
            // execute
            ExtractAndShowReport();
        },
        x =>
        {
            // can execute
            return (_CurrentReportInfo != null);
        });
        public ICommand SaveReportsCommand => new DelegateCommand(x =>
        {
            // execute
            SaveReports();
        },
        x =>
        {
            // can execute
            return (_CurrentReportInfo != null);
        });
        public ICommand DeleteReportsCommand => new DelegateCommand(x =>
        {
            // execute
            DeleteReports();
        },
        x =>
        {
            // can execute
            return (_CurrentReportInfo != null);
        });
        #endregion
        #region ISubUserControl Methods
        public string Title
        {
            get { return "History"; }
        }
        public string Key
        {
            get { return "HISTORY"; }
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
            // process extra information in parallel (in a task)
            System.Threading.Tasks.Task.Run(() =>
            {
                _FirstDateTime = DateTimeOffset.MaxValue;
                _LastDateTime = DateTimeOffset.MinValue;
                _DateTimeRange = TimeSpan.Zero;
                object zipLock = new object();

                // collect all reports for this job
                var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                var list = new List<ReportInfo>();
                zip.ForEach((item) =>
                {
                    if (item.RootFilename.StartsWith(_Job.Name + "."))
                    {
                        list.Add(new ReportInfo()
                        {
                            Name = ParseFilenameTitle(item.RootFilename),
                            DateCreated = ParseFilenameDate(item.RootFilename),
                            SortableDateCreated = ParseSortableFilenameDate(item.RootFilename),
                            Filename = item.RootFilename,
                            ReportName = string.Format($"{ParseFilenameTitle(item.RootFilename)}.JobReport.txt")
                        });
                    }
                });

                // build reports
                this.Dispatcher.Invoke(new Action(() =>
                {
                    Reports = from item in list
                              orderby item.SortableDateCreated descending
                              select item;
                }));
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
                        }
                        finally
                        {
                            // attempt to delete extracted file
                            Helper.DeleteFile(filename);
                            //try { System.IO.File.Delete(filename); }
                            //catch { }
                        }
                        if (stats != null)
                        {
                            // update UI
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                var dude = Reports.FirstOrDefault((info) => { return info.Filename == x.Filename; });
                                if (dude != null)
                                {
                                    dude.TotalRows = stats.TotalCount.ToString("N0");
                                    dude.PercentChanged = (((double)stats.TotalChangedCount / (double)stats.TotalCount) * 100.0).ToString("N2") + "%";
                                }
                            }));
                        }
                    }
                });
                // 
                if (!_CancelTokenSource.Token.IsCancellationRequested)
                {
                    // update UI
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Collapsed;
                        if (_DateTimeRange == TimeSpan.Zero) { DateRangeText = ""; }
                        else { DateRangeText = $"{dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(_DateTimeRange)}   ({_FirstDateTime:d} to {_LastDateTime:d})"; }
                    }));
                }
            });
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
                // process date information
                if (dude.ReportDate < _FirstDateTime) { _FirstDateTime = dude.ReportDate; }
                if (dude.ReportDate > _LastDateTime) { _LastDateTime = dude.ReportDate; }
                _DateTimeRange = _LastDateTime - _FirstDateTime;
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
        private void ExtractAndShowReport()
        {
            if (_CurrentReportInfo != null)
            {
                var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                var rootFilename = _CurrentReportInfo.Filename;
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
        private void DeleteReports()
        {
            if (MessageBox.Show(Window.GetWindow(this),
                                   "Delete Selected Reports" + Environment.NewLine + Environment.NewLine + "Are you sure?",
                                   "Delete Selected Reports",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                foreach (ReportInfo item in dataGridData.SelectedItems)
                {
                    if (zip.Contains(item.Filename)) { zip.Delete(item.Filename); }
                }
                zip.Save(true);
                UpdateUI();
            }
        }
        private void SaveReports()
        {
            // #### select path to save reports to
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog() { Description = "DESCRIPTION", ShowNewFolderButton = true })
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var extractPath = dialog.SelectedPath;
                    var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                    var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                    var count = 0;
                    foreach (ReportInfo item in dataGridData.SelectedItems)
                    {
                        if (zip.Contains(item.Filename))
                        {
                            try
                            {
                                var dude = zip.Get(item.Filename);
                                dude?.Extract(extractPath, true);
                                ++count;
                            }
                            catch { }
                        }
                    }
                    //
                    MessageBox.Show(Window.GetWindow(this),
                                    $"{count:N0} Reports Extracted.{Environment.NewLine}{Environment.NewLine}Extraction Path:{Environment.NewLine}{extractPath}",
                                    "Reports Extracted",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                // #### save reports
                // ---- ----
                //UpdateUI();
            }
        }
        #endregion
    }
    #region Public Class: ReportInfo
    public class ReportInfo
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
        private string _TotalRows = "";
        public string TotalRows
        {
            get { return _TotalRows; }
            set
            {
                _TotalRows = value;
                RaisePropertyChangedEvent("TotalRows");
            }
        }
        private string _PercentChanged = "";
        public string PercentChanged
        {
            get { return _PercentChanged; }
            set
            {
                _PercentChanged = value;
                RaisePropertyChangedEvent("PercentChanged");
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
    }
    #endregion
}
