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
    /// <summary>
    /// Interaction logic for ReportViewerWindow.xaml
    /// </summary>
    public partial class ReportViewerWindow : Window
    {
        #region Ctor
        public ReportViewerWindow(GlobalSettings settings)
        {
            InitializeComponent();
            DataContext = this;
            ReportFileStatisticsPanel.Visibility = System.Windows.Visibility.Hidden;
            Title = Helper.FormatTitle("Report Viewer");
            _Settings = settings;
            App.WriteDebugLog(nameof(ReportViewerWindow), $"Opening Report Viewer Window");
        }
        #endregion
        #region Private Fields
        private GlobalSettings _Settings = null;
        private List<System.Threading.CancellationTokenSource> _TaskCancelTokens = new List<System.Threading.CancellationTokenSource>();
        #endregion
        #region Dependency Properties
        public string ReportFilename
        {
            get { return (string)GetValue(ReportFilenameProperty); }
            set { SetValue(ReportFilenameProperty, value); }
        }
        public static readonly DependencyProperty ReportFilenameProperty =
            DependencyProperty.Register("ReportFilename", typeof(string), typeof(ReportViewerWindow), new UIPropertyMetadata(""));
        // ----
        public Visibility TextBoxOpenReportFileStatementVisibility
        {
            get { return (Visibility)GetValue(TextBoxOpenReportFileStatementVisibilityProperty); }
            set { SetValue(TextBoxOpenReportFileStatementVisibilityProperty, value); }
        }
        public static readonly DependencyProperty TextBoxOpenReportFileStatementVisibilityProperty =
            DependencyProperty.Register("TextBoxOpenReportFileStatementVisibility", typeof(Visibility), typeof(ReportViewerWindow), new UIPropertyMetadata(Visibility.Visible));
        // ----
        public Helper.ReportFileStatistics ReportFileStats
        {
            get { return (Helper.ReportFileStatistics)GetValue(ReportFileStatsProperty); }
            set { SetValue(ReportFileStatsProperty, value); }
        }
        public static readonly DependencyProperty ReportFileStatsProperty =
            DependencyProperty.Register("ReportFileStats", typeof(Helper.ReportFileStatistics), typeof(ReportViewerWindow), new UIPropertyMetadata(null));
        // ----
        public Visibility ImageWorkingIcon
        {
            get { return (Visibility)GetValue(ImageWorkingIconProperty); }
            set { SetValue(ImageWorkingIconProperty, value); }
        }
        public static readonly DependencyProperty ImageWorkingIconProperty =
            DependencyProperty.Register("ImageWorkingIcon", typeof(Visibility), typeof(ReportViewerWindow), new UIPropertyMetadata(Visibility.Hidden));
        // ----
        public string OpenReportFileStatement
        {
            get { return (string)GetValue(OpenReportFileStatementProperty); }
            set { SetValue(OpenReportFileStatementProperty, value); }
        }
        public static readonly DependencyProperty OpenReportFileStatementProperty =
            DependencyProperty.Register("OpenReportFileStatement", typeof(string), typeof(ReportViewerWindow), new UIPropertyMetadata("Open report file to view it's contents"));
        // ----
        #endregion
        #region Window Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CancelAllRunningTasks();
        }
        private void CancelAllRunningTasks()
        {
            while (_TaskCancelTokens.Count > 0)
            {
                var cancelled = new List<System.Threading.CancellationTokenSource>();
                foreach (var item in _TaskCancelTokens)
                {
                    if (item.Token.CanBeCanceled && item.IsCancellationRequested)
                    {
                        item.Dispose();
                        cancelled.Add(item);
                    }
                    else
                    {
                        item.Cancel();
                    }
                }
                foreach (var item in cancelled) { _TaskCancelTokens.Remove(item); }
            }
        }
        private void buttonOpenReportFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog()
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "*.txt",
                DereferenceLinks = true,
                Title = "Open",
                Filter = "text Files|*.txt|All Files|*.*",
                FilterIndex = 0
            };
            if (dialog.ShowDialog(this) ?? false)
            {
                // cancel any previous tasks
                CancelAllRunningTasks();
                // add task
                System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource();
                _TaskCancelTokens.Add(tokenSource);
                // Open Button Statement
                ReportFilename = "";
                OpenReportFileStatement = "Loading...";
                TextBoxOpenReportFileStatementVisibility = System.Windows.Visibility.Visible;
                // show working icon
                ImageWorkingIcon = System.Windows.Visibility.Visible;
                // hide UI elements
                ReportFileStatisticsPanel.Visibility = System.Windows.Visibility.Hidden;
                // clear current data grid
                gridContentHolder.Children.Clear();
                // start thread
                System.Threading.Tasks.Task.Run(
                () =>
                {
                    // process report
                    IEnumerable<object> okData = null;
                    IEnumerable<object> newData = null;
                    IEnumerable<object> updateData = null;
                    IEnumerable<object> removeData = null;
                    Helper.ReportFileStatistics stats = null;
                    IEnumerable<string> lines = null;
                    try
                    {
                        lines = ReadContentLines(dialog.FileName, tokenSource);
                        ProcessLines(lines, tokenSource, out okData, out newData, out updateData, out removeData, out stats);
                        if (tokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                    catch (System.ObjectDisposedException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.StartsWith("Invalid File Format:"))
                        {
                            this.Dispatcher.Invoke(
                                new Action(() =>
                                {
                                    MessageBox.Show(Window.GetWindow(this),
                                                    "Not a Report File.",
                                                     "Invalid File Format",
                                                     MessageBoxButton.OK,
                                                     MessageBoxImage.Error);
                                    // hide working icon
                                    ImageWorkingIcon = System.Windows.Visibility.Hidden;
                                    OpenReportFileStatement = "Open report file to view it's contents";
                                }));
                            return;
                        }
                        else { throw; }
                    }
                    // update UI
                    this.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        // create user control
                        var dude = new ReportViewerUserControl2(false, Helper.GetDestinationDirectoryName(null), stats, okData, newData, updateData, removeData, _Settings); // false because it does not matter
                                                                                                                                                                             // Open Button Statement
                        TextBoxOpenReportFileStatementVisibility = System.Windows.Visibility.Hidden;
                        // update filename
                        ReportFilename = dialog.FileName;
                        // update stats
                        ReportFileStats = stats;
                        // hide working icon
                        ImageWorkingIcon = System.Windows.Visibility.Hidden;
                        // update grid
                        gridContentHolder.Children.Add(dude);
                        // show UI elements
                        ReportFileStatisticsPanel.Visibility = System.Windows.Visibility.Visible;
                    }));
                });
            }
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
        #region Private Methods
        private IEnumerable<string> ReadContentLines(string filename, System.Threading.CancellationTokenSource tokenSource)
        {
            var result = new List<string>();
            using (var sr = new System.IO.StreamReader(filename))
            {
                if (tokenSource.Token.IsCancellationRequested) { return null; }
                var line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (tokenSource.Token.IsCancellationRequested) { return null; }
                    result.Add(line);
                }
                sr.Close();
            }
            return result;
        }
        private void ProcessLines(IEnumerable<string> lines,
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
                    if (!line.Equals("# Archive & Mirror Tool")) { throw new Exception("Invalid File Format: not a dodSON.Software Archive & Mirror Tool Report File."); }
                }
                if (line.StartsWith("#")) { iniPart.Add(line.Substring(1).Trim()); }
                else { reportPart.AppendLine(line); }
            }
            // ----
            // create report stats
            stats = Helper.ProcessIni(iniPart);
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
                bool isArchiveJob = false;
                Helper.DetailsRowBase drow = CreateDataRow(item, out isArchiveJob);
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
        private Helper.DetailsRowBase CreateDataRow(dodSON.Core.DelimiterSeparatedValues.DsvRow item, out bool isArchiveJob)
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
        //private Helper.ReportFileStatistics ProcessIni(List<string> iniPart)
        //{
        //    var stats = new Helper.ReportFileStatistics();
        //    foreach (var line in iniPart)
        //    {
        //        if (line.Contains("="))
        //        {
        //            var parts = line.Split('=');
        //            if (parts.Length > 1)
        //            {
        //                switch (parts[0].Trim())
        //                {
        //                    case "Report Date":
        //                        stats.ReportDate = ConvertToDate(parts[1].Trim());
        //                        break;
        //                    case "Name":
        //                        stats.JobName = parts[1].Trim();
        //                        break;
        //                    case "Type":
        //                        stats.JobType = parts[1].Trim();
        //                        break;
        //                    case "Date Create":
        //                        stats.JobDateCreated = ConvertToDate(parts[1].Trim());
        //                        break;
        //                    case "Archive Remove Files":
        //                        stats.JobArchiveRemoveFiles = ConvertToBoolean(parts[1].Trim());
        //                        break;
        //                    case "Ok":
        //                        stats.OkCount = ConvertToInteger(parts[1].Trim());
        //                        break;
        //                    case "New":
        //                        stats.NewCount = ConvertToInteger(parts[1].Trim());
        //                        break;
        //                    case "Update":
        //                        stats.UpdateCount = ConvertToInteger(parts[1].Trim());
        //                        break;
        //                    case "Remove":
        //                        stats.RemoveCount = ConvertToInteger(parts[1].Trim());
        //                        break;
        //                    case "Total":
        //                        stats.TotalCount = ConvertToInteger(parts[1].Trim());
        //                        break;
        //                    case "Total Changed":
        //                        stats.TotalChangedCount = ConvertToInteger(parts[1].Trim());
        //                        break;
        //                    case "Total Source":
        //                        stats.SourceTotalCount = ConvertToInteger(parts[1].Trim());
        //                        break;
        //                    case "Total Destination":
        //                        stats.DestinationTotalCount = ConvertToInteger(parts[1].Trim());
        //                        break;
        //                    case "Export Ok Files":
        //                        stats.ExportOK = ConvertToBoolean(parts[1].Trim());
        //                        break;
        //                    case "Export New Files":
        //                        stats.ExportNew = ConvertToBoolean(parts[1].Trim());
        //                        break;
        //                    case "Export Update Files":
        //                        stats.ExportUpdate = ConvertToBoolean(parts[1].Trim());
        //                        break;
        //                    case "Export Remove Files":
        //                        stats.ExportRemove = ConvertToBoolean(parts[1].Trim());
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //    return stats;
        //}
        //private DateTime ConvertToDate(string str)
        //{
        //    var result = DateTime.MinValue;
        //    if (!DateTime.TryParse(str, out result)) { result = DateTime.MinValue; }
        //    return result;
        //}
        //private int ConvertToInteger(string str)
        //{
        //    var result = int.MinValue;
        //    str = str.Replace(",", "");
        //    if (!int.TryParse(str, out result)) { result = int.MinValue; }
        //    return result;
        //}
        //private bool ConvertToBoolean(string str)
        //{
        //    var result = false;
        //    if (!bool.TryParse(str, out result)) { result = false; }
        //    return result;
        //}
        //private bool ConvertExportFilenameFormatToBoolean(string str)
        //{
        //    var result = false;
        //    if (!bool.TryParse(str, out result)) { result = false; }
        //    return result;
        //}
        #endregion
    }
}
