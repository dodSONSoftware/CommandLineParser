//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//namespace MirrorAndArchiveTool
//{
//    /// <summary>
//    /// Interaction logic for ReportViewerUserControl.xaml
//    /// </summary>
//    public partial class ReportViewerUserControl : UserControl
//    {
//        #region Ctor
//        public ReportViewerUserControl()
//        {
//            InitializeComponent();
//            DataContext = this;
//        }
//        public ReportViewerUserControl(JobBase job,
//                                       IEnumerable<dodSON.BuildingBlocks.FileStorage.ICompareResult> report)
//            : this()
//        {
//            _RawView = false;
//            if (job == null) { throw new ArgumentNullException("job"); }
//            if (report == null) { throw new ArgumentNullException("report"); }
//            _Job = job;
//            _Report = report;
//            AnalyzeReport();
//            InitializeButtons();
//            PopulateDataGrid();
//        }
//        public ReportViewerUserControl(IEnumerable<string> contentLines)
//            : this()
//        {
//            _RawView = true;
//            if (contentLines == null) { throw new ArgumentNullException("contentLines"); }
//            _ContentLines = contentLines;
//            InitializeButtons();
//            PopulateDataGrid();
//            UpdateStatusBar();
//        }
//        #endregion
//        #region Private Fields
//        private bool _RawView = true;
//        private JobBase _Job = null;
//        private IEnumerable<dodSON.BuildingBlocks.FileStorage.ICompareResult> _Report = null;
//        // ----
//        private IEnumerable<string> _ContentLines = null;
//        // ----
//        private Helper.ReportFileStatistics _JobStats = new Helper.ReportFileStatistics();
//        private Helper.ReportFileStatistics _ReportFileStatistics = null;
//        #endregion
//        #region Public Methods
//        public Helper.ReportFileStatistics ReportFileStatistics
//        {
//            get { return _ReportFileStatistics; }
//        }
//        #endregion
//        #region Window Events
//        private void toggleButtonOk_Click(object sender, RoutedEventArgs e)
//        {
//            PopulateDataGrid();
//        }
//        private void toggleButtonNew_Click(object sender, RoutedEventArgs e)
//        {
//            PopulateDataGrid();
//        }
//        private void toggleButtonUpdate_Click(object sender, RoutedEventArgs e)
//        {
//            PopulateDataGrid();
//        }
//        private void toggleButtonRemove_Click(object sender, RoutedEventArgs e)
//        {
//            PopulateDataGrid();
//        }
//        private void toggleButtonFilenameFormat_Click(object sender, RoutedEventArgs e)
//        {
//            if (_RawView) { throw new Exception("Cannot Export Report when in RawView Mode"); }
//            UpdateFilenameToggleButtonText();
//            PopulateDataGrid();
//        }
//        private void buttonExport_Click(object sender, RoutedEventArgs e)
//        {
//            if (_RawView) { throw new Exception("Cannot Export Report when in RawView Mode"); }
//            var wind = new ExportReportWindow(_Job,
//                                              _Report,
//                                              _JobStats,
//                                              (toggleButtonOk.IsChecked ?? false),
//                                              (toggleButtonNew.IsChecked ?? false),
//                                              (toggleButtonUpdate.IsChecked ?? false),
//                                              (toggleButtonRemove.IsChecked ?? false),
//                                              (toggleButtonFilenameFormat.IsChecked ?? false));
//            wind.Owner = Window.GetWindow(this);
//            wind.ShowDialog();
//        }
//        private void buttonDefaultSort_Click(object sender, RoutedEventArgs e)
//        {
//            PopulateDataGrid();
//        }
//        #endregion
//        #region Private Methods
//        private void AnalyzeReport()
//        {
//            foreach (var item in _Report)
//            {
//                if (item.ItemType == dodSON.BuildingBlocks.FileStorage.CompareType.File)
//                {
//                    switch (item.Action)
//                    {
//                        case dodSON.BuildingBlocks.FileStorage.CompareAction.New:
//                            ++_JobStats.NewCount;
//                            break;
//                        case dodSON.BuildingBlocks.FileStorage.CompareAction.Ok:
//                            ++_JobStats.OkCount;
//                            break;
//                        case dodSON.BuildingBlocks.FileStorage.CompareAction.Remove:
//                            ++_JobStats.RemoveCount;
//                            break;
//                        case dodSON.BuildingBlocks.FileStorage.CompareAction.Update:
//                            ++_JobStats.UpdateCount;
//                            break;
//                        default:
//                            break;
//                    }
//                }
//            }
//            _JobStats.Total = _JobStats.Ok + _JobStats.New + _JobStats.Update + _JobStats.Remove;
//            // ----
//            _JobStats.SourceTotal = _JobStats.Ok + _JobStats.New + _JobStats.Update;
//            _JobStats.DestinationTotal = _JobStats.Ok + _JobStats.Update + _JobStats.Remove;
//            _JobStats.TotalChanged = _JobStats.New + _JobStats.Update + _JobStats.Remove;
//        }
//        private void InitializeButtons()
//        {
//            toggleButtonOk.IsChecked = false;
//            if (_JobStats.TotalChanged == 0) { toggleButtonOk.IsChecked = true; }
//            toggleButtonNew.IsChecked = true;
//            toggleButtonUpdate.IsChecked = true;
//            toggleButtonRemove.IsChecked = true;
//            toggleButtonFilenameFormat.IsChecked = false;
//            UpdateFilenameToggleButtonText();
//            if (_RawView)
//            {
//                toggleButtonFilenameFormat.Visibility = System.Windows.Visibility.Collapsed;
//                buttonExport.Visibility = System.Windows.Visibility.Collapsed;
//            }
//            UpdateStatusBar();
//        }
//        private void UpdateStatusBar()
//        {
//            if (_RawView)
//            {
//                if (_ReportFileStatistics != null)
//                {
//                    textBlockTotalReportItemsCount.Text = string.Format("{0} total", _ReportFileStatistics.ReportRows);
//                    textBlockTotalReportOkCount.Text = string.Format("{0} ok file{2} ({1:N2}%)", _ReportFileStatistics.Ok, (((double)_ReportFileStatistics.OkDouble / (double)_ReportFileStatistics.TotalDouble) * 100.0), ((_ReportFileStatistics.Ok == "1") ? "" : "s"));
//                    textBlockTotalReportNewCount.Text = string.Format("{0} new file{2} ({1:N2}%)", _ReportFileStatistics.New, (((double)_ReportFileStatistics.NewDouble / (double)_ReportFileStatistics.TotalDouble) * 100.0), ((_ReportFileStatistics.New == "1") ? "" : "s"));
//                    textBlockTotalReportUpdateCount.Text = string.Format("{0} update file{2} ({1:N2}%)", _ReportFileStatistics.Update, (((double)_ReportFileStatistics.UpdateDouble / (double)_ReportFileStatistics.TotalDouble) * 100.0), ((_ReportFileStatistics.Update == "1") ? "" : "s"));
//                    textBlockTotalReportRemoveCount.Text = string.Format("{0} remove file{2} ({1:N2}%)", _ReportFileStatistics.Remove, (((double)_ReportFileStatistics.RemoveDouble / (double)_ReportFileStatistics.TotalDouble) * 100.0), ((_ReportFileStatistics.Remove == "1") ? "" : "s"));
//                }
//            }
//            else
//            {
//                textBlockTotalReportItemsCount.Text = string.Format("{0:N0} total", _JobStats.Total);
//                textBlockTotalReportOkCount.Text = string.Format("{0:N0} ok file{2} ({1:N2}%)", _JobStats.Ok, (((double)_JobStats.Ok / (double)_JobStats.Total) * 100.0), ((_JobStats.Ok == 1) ? "" : "s"));
//                textBlockTotalReportNewCount.Text = string.Format("{0:N0} new file{2} ({1:N2}%)", _JobStats.New, (((double)_JobStats.New / (double)_JobStats.Total) * 100.0), ((_JobStats.New == 1) ? "" : "s"));
//                textBlockTotalReportUpdateCount.Text = string.Format("{0:N0} update file{2} ({1:N2}%)", _JobStats.Update, (((double)_JobStats.Update / (double)_JobStats.Total) * 100.0), ((_JobStats.Update == 1) ? "" : "s"));
//                textBlockTotalReportRemoveCount.Text = string.Format("{0:N0} remove file{2} ({1:N2}%)", _JobStats.Remove, (((double)_JobStats.Remove / (double)_JobStats.Total) * 100.0), ((_JobStats.Remove == 1) ? "" : "s"));
//            }
//        }
//        private void UpdateFilenameToggleButtonText()
//        {
//            textBlockFilenameFormat.Text = (toggleButtonFilenameFormat.IsChecked ?? false) ? "Full Filenames" : "Root Filenames";
//        }
//        private void PopulateDataGrid()
//        {
//            var includeNew = (toggleButtonNew.IsChecked ?? false);
//            var includeOk = (toggleButtonOk.IsChecked ?? false);
//            var includeUpdate = (toggleButtonUpdate.IsChecked ?? false);
//            var includeRemove = (toggleButtonRemove.IsChecked ?? false);
//            var fullFilenameFormat = (toggleButtonFilenameFormat.IsChecked ?? false);
//            System.Threading.Tasks.Task.Factory.StartNew(
//                () =>
//                {
//                    this.Dispatcher.Invoke(new Action(
//                        () =>
//                        {
//                            // update UI
//                            dataGridFilesDetails.Visibility = System.Windows.Visibility.Hidden;
//                        }));
//                    // ----
//                    var masterList = new List<object>();
//                    if (_RawView)
//                    {
//                        // parse content string
//                        masterList = ParseContentString(includeOk, includeNew, includeUpdate, includeRemove);
//                    }
//                    else
//                    {
//                        SortedList<string, IEnumerable<object>> listedData = Helper.CreateReport(includeOk,
//                                                                                                 includeNew,
//                                                                                                 includeUpdate,
//                                                                                                 includeRemove,
//                                                                                                 fullFilenameFormat,
//                                                                                                 _Job,
//                                                                                                 _Report);
//                        // consolidate lists
//                        foreach (var item in listedData) { masterList.AddRange(item.Value); }
//                    }
//                    this.Dispatcher.Invoke(new Action(
//                        () =>
//                        {
//                            // update UI
//                            dataGridFilesDetails.ItemsSource = masterList;
//                            dataGridFilesDetails.Visibility = System.Windows.Visibility.Visible;
//                        }));
//                });
//        }
//        private List<object> ParseContentString(bool includeOk,
//                                                bool includeNew,
//                                                bool includeUpdate,
//                                                bool includeRemove)
//        {
//            var list = new List<object>();
//            // read header
//            // create dodSON.BuildingBlocks.DelimiterSeperatedValues.DsvTable
//            // update list
//            var iniPart = new List<string>();
//            var reportPart = new System.Text.StringBuilder(2048);
//            foreach (var line in _ContentLines)
//            {
//                if (line.StartsWith("#")) { iniPart.Add(line.Substring(1).Trim()); }
//                else { reportPart.AppendLine(line); }
//            }
//            var report = new dodSON.BuildingBlocks.DelimiterSeperatedValues.DsvTable();
//            report.ReadString(reportPart.ToString());
//            // update statistics UI
//            ProcessIni(iniPart);
//            // process report
//            foreach (var item in report.Rows)
//            {
//                object drow = null;
//                dodSON.BuildingBlocks.FileStorage.CompareAction action = (dodSON.BuildingBlocks.FileStorage.CompareAction)Enum.Parse(typeof(dodSON.BuildingBlocks.FileStorage.CompareAction), (item["Action"] ?? "Unknown").ToString(), true);
//                switch (action)
//                {
//                    case dodSON.BuildingBlocks.FileStorage.CompareAction.New:
//                        if (includeNew) { drow = CreateDataRow(report, item); }
//                        break;
//                    case dodSON.BuildingBlocks.FileStorage.CompareAction.Ok:
//                        if (includeOk) { drow = CreateDataRow(report, item); }
//                        break;
//                    case dodSON.BuildingBlocks.FileStorage.CompareAction.Remove:
//                        if (includeUpdate) { drow = CreateDataRow(report, item); }
//                        break;
//                    case dodSON.BuildingBlocks.FileStorage.CompareAction.Update:
//                        if (includeRemove) { drow = CreateDataRow(report, item); }
//                        break;
//                    default:
//                        break;
//                }
//                // add data row (to value)
//                if (drow != null) { list.Add(drow); }
//            }
//            // 
//            return list;
//        }
//        private object CreateDataRow(dodSON.BuildingBlocks.DelimiterSeperatedValues.DsvTable report,
//                                     dodSON.BuildingBlocks.DelimiterSeperatedValues.DsvRow item)
//        {
//            // create data row
//            object drow = null;
//            if (report.Columns.Contains("ArchiveFile"))
//            {
//                drow = new Helper.ArchiveFilesDetailsRow()
//                {
//                    DestinationArchive = (report.Columns.Contains("ArchiveFile")) ? (item["ArchiveFile"] ?? "").ToString() : "",
//                    Action = (item["Action"] ?? "").ToString(),
//                    SourceFile = (item["SourceFile"] ?? "").ToString(),
//                    SourceDate = (item["SourceFileDate"] ?? "").ToString(),
//                    DestinationFile = (item["DestinationFile"] ?? "").ToString(),
//                    DestinationDate = (item["DestinationFileDate"] ?? "").ToString(),
//                    DateDifference = (item["DateDifference"] ?? "").ToString()
//                };
//            }
//            else
//            {
//                drow = new Helper.MirrorFilesDetailsRow()
//                {
//                    Action = (item["Action"] ?? "").ToString(),
//                    SourceFile = (item["SourceFile"] ?? "").ToString(),
//                    SourceDate = (item["SourceFileDate"] ?? "").ToString(),
//                    DestinationFile = (item["DestinationFile"] ?? "").ToString(),
//                    DestinationDate = (item["DestinationFileDate"] ?? "").ToString(),
//                    DateDifference = (item["DateDifference"] ?? "").ToString()
//                };
//            }
//            return drow;
//        }
//        private void ProcessIni(List<string> iniPart)
//        {
//            _ReportFileStatistics = new Helper.ReportFileStatistics();
//            foreach (var line in iniPart)
//            {
//                if (line.Contains("="))
//                {
//                    var parts = line.Split('=');
//                    if (parts.Length > 1)
//                    {
//                        switch (parts[0].Trim())
//                        {
//                            case "Report Date":
//                                _ReportFileStatistics.ReportDate = parts[1].Trim();
//                                break;
//                            case "Report Rows":
//                                _ReportFileStatistics.ReportRows = parts[1].Trim();
//                                break;
//                            case "Name":
//                                _ReportFileStatistics.JobName = parts[1].Trim();
//                                break;
//                            case "Type":
//                                _ReportFileStatistics.JobType = parts[1].Trim();
//                                break;
//                            case "Date Create":
//                                _ReportFileStatistics.JobDateCreated = parts[1].Trim();
//                                break;
//                            case "Archive Remove Files":
//                                _ReportFileStatistics.JobArchiveRemoveFiles = parts[1].Trim();
//                                break;
//                            case "Ok":
//                                _ReportFileStatistics.Ok = parts[1].Trim();
//                                break;
//                            case "New":
//                                _ReportFileStatistics.New = parts[1].Trim();
//                                break;
//                            case "Update":
//                                _ReportFileStatistics.Update = parts[1].Trim();
//                                break;
//                            case "Remove":
//                                _ReportFileStatistics.Remove = parts[1].Trim();
//                                break;
//                            case "Total":
//                                _ReportFileStatistics.Total = parts[1].Trim();
//                                break;
//                            case "Total Changed":
//                                _ReportFileStatistics.TotalChanged = parts[1].Trim();
//                                break;
//                            case "Total Source":
//                                _ReportFileStatistics.SourceTotal = parts[1].Trim();
//                                break;
//                            case "Total Destination":
//                                _ReportFileStatistics.DestinationTotal = parts[1].Trim();
//                                break;
//                            case "Export Ok Files":
//                                _ReportFileStatistics.ExportOK = parts[1].Trim();
//                                break;
//                            case "Export New Files":
//                                _ReportFileStatistics.ExportNew = parts[1].Trim();
//                                break;
//                            case "Export Update Files":
//                                _ReportFileStatistics.ExportUpdate = parts[1].Trim();
//                                break;
//                            case "Export Remove Files":
//                                _ReportFileStatistics.ExportRemove = parts[1].Trim();
//                                break;
//                            case "Filename Format":
//                                _ReportFileStatistics.ExportFilenameFormat = parts[1].Trim();
//                                break;
//                            default:
//                                break;
//                        }
//                    }
//                }
//            }
//        }
//        #endregion
//    }
//    //#region Public Classes: ArchiveFilesDetailsRow, MirrorFilesDetailsRow
//    //public class ArchiveFilesDetailsRow
//    //{
//    //    public string Action { get; set; }
//    //    public string SourceFile { get; set; }
//    //    public string SourceDate { get; set; }
//    //    public string DestinationArchive { get; set; }
//    //    public string DestinationFile { get; set; }
//    //    public string DestinationDate { get; set; }
//    //    public string DateDifference { get; set; }
//    //}
//    //public class MirrorFilesDetailsRow
//    //{
//    //    public string Action { get; set; }
//    //    public string SourceFile { get; set; }
//    //    public string SourceDate { get; set; }
//    //    public string DestinationFile { get; set; }
//    //    public string DestinationDate { get; set; }
//    //    public string DateDifference { get; set; }
//    //}
//    //public class ReportStatistics
//    //{
//    //    public int Total { get; set; }
//    //    public int Ok { get; set; }
//    //    public int New { get; set; }
//    //    public int Update { get; set; }
//    //    public int Remove { get; set; }
//    //    public int SourceTotal { get; set; }
//    //    public int DestinationTotal { get; set; }
//    //    public int TotalChanged { get; set; }
//    //}
//    //#endregion
//}
