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
    /// Interaction logic for ExportReportWindow.xaml
    /// </summary>
    public partial class ExportReportWindow : Window
    {
        #region Readonly Fields
        private readonly string CommentChar = "#";
        #endregion
        #region Ctor
        public ExportReportWindow()
        {
            InitializeComponent();
            DataContext = this;
            Title = Helper.FormatTitle("Export Report");
            App.WriteDebugLog(nameof(ExportReportWindow), $"Opening Export Report Window");
        }
        public ExportReportWindow(Helper.ReportFileStatistics header,
                                  IEnumerable<object> okData,
                                  IEnumerable<object> newData,
                                  IEnumerable<object> updateData,
                                  IEnumerable<object> removeData,
                                  bool showOk, bool exportOk,
                                  bool showNew, bool exportNew,
                                  bool showUpdate, bool exportUpdate,
                                  bool showRemove, bool exportRemove)
            : this()
        {
            if (header == null) { throw new ArgumentNullException("header"); }
            if (okData == null) { throw new ArgumentNullException("okData"); }
            if (newData == null) { throw new ArgumentNullException("newData"); }
            if (updateData == null) { throw new ArgumentNullException("updateData"); }
            if (removeData == null) { throw new ArgumentNullException("removeData"); }
            _ReportStats = header;
            _OkData = okData;
            _NewData = newData;
            _UpdateData = updateData;
            _RemoveData = removeData;
            // populate window's initial state
            checkBoxOk.Visibility = (showOk) ? Visibility.Visible : Visibility.Collapsed;
            checkBoxOk.IsChecked = (showOk && exportOk);

            checkBoxNew.Visibility = (showNew) ? Visibility.Visible : Visibility.Collapsed;
            checkBoxNew.IsChecked = showNew && exportNew;

            checkBoxUpdate.Visibility = (showUpdate) ? Visibility.Visible : Visibility.Collapsed;
            checkBoxUpdate.IsChecked = showUpdate && exportUpdate;

            checkBoxRemove.Visibility = (showRemove) ? Visibility.Visible : Visibility.Collapsed;
            checkBoxRemove.IsChecked = showRemove && exportRemove;
        }
        #endregion
        #region Private Fields
        private Helper.ReportFileStatistics _ReportStats = null;
        private IEnumerable<object> _OkData = null;
        private IEnumerable<object> _NewData = null;
        private IEnumerable<object> _UpdateData = null;
        private IEnumerable<object> _RemoveData = null;
        #endregion
        #region Window Events
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            // get filename
            Microsoft.Win32.SaveFileDialog openFileWindow = new Microsoft.Win32.SaveFileDialog()
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "*.txt",
                DereferenceLinks = true,
                OverwritePrompt = true,
                Title = "Export to File",
                Filter = "Text|*.txt|Comma-separated Values|*.csv|All Files|*.*",
                FilterIndex = 0
            };
            if (openFileWindow.ShowDialog(this) ?? false)
            {
                // create report header
                var header = new System.Text.StringBuilder(1024);
                header.AppendFormat("{0} {2}{1}", CommentChar, Environment.NewLine, "Archive & Mirror Tool");
                header.AppendFormat("{0} {2}{1}", CommentChar, Environment.NewLine, "Copyright (c) dodSON Software 2015");
                header.AppendFormat("{0} Report Date= {2}{1}", CommentChar, Environment.NewLine, DateTime.Now);
                header.AppendFormat("{0} Report Rows= {2:N0}{1}", CommentChar, Environment.NewLine, CountReportRows());
                header.AppendFormat("{0} -------- Job Information --------{1}", CommentChar, Environment.NewLine);
                header.AppendFormat("{0} Name= {2}{1}", CommentChar, Environment.NewLine, _ReportStats.JobName);
                header.AppendFormat("{0} Type= {2}{1}", CommentChar, Environment.NewLine, _ReportStats.JobType);
                header.AppendFormat("{0} Date Create= {2}{1}", CommentChar, Environment.NewLine, _ReportStats.JobDateCreated);
                header.AppendFormat("{0} Archive Remove Files= {2}{1}", CommentChar, Environment.NewLine, _ReportStats.JobArchiveRemoveFiles);
                if (_ReportStats.JobType == "Archive")
                {
                    header.AppendFormat("{0} Archive Storage Path= {2}{1}", CommentChar, Environment.NewLine, _ReportStats.JobArchiveStoragePath);
                    foreach (var item in _ReportStats.JobArchiveSourcePaths)
                    {
                        header.AppendFormat("{0} Archive Source Path= {2}{1}", CommentChar, Environment.NewLine, item);
                    }
                }
                else
                {
                    header.AppendFormat("{0} Mirror Source Path= {2}{1}", CommentChar, Environment.NewLine, _ReportStats.JobMirrorSourcePath);
                    header.AppendFormat("{0} Mirror Destination Path= {2}{1}", CommentChar, Environment.NewLine, _ReportStats.JobMirrorDestinationPath);
                }
                header.AppendFormat("{0} -------- File Counts --------{1}", CommentChar, Environment.NewLine);
                header.AppendFormat("{0} Ok= {2:N0}{1}", CommentChar, Environment.NewLine, _ReportStats.OkCount);
                header.AppendFormat("{0} New= {2:N0}{1}", CommentChar, Environment.NewLine, _ReportStats.NewCount);
                header.AppendFormat("{0} Update= {2:N0}{1}", CommentChar, Environment.NewLine, _ReportStats.UpdateCount);
                header.AppendFormat("{0} Remove= {2:N0}{1}", CommentChar, Environment.NewLine, _ReportStats.RemoveCount);
                header.AppendFormat("{0} Total= {2:N0}{1}", CommentChar, Environment.NewLine, _ReportStats.TotalCount);
                header.AppendFormat("{0} Total Changed= {2:N0}{1}", CommentChar, Environment.NewLine, _ReportStats.TotalChangedCount);
                header.AppendFormat("{0} Total Source= {2:N0}{1}", CommentChar, Environment.NewLine, _ReportStats.SourceTotalCount);
                header.AppendFormat("{0} Total Destination= {2:N0}{1}", CommentChar, Environment.NewLine, _ReportStats.DestinationTotalCount);
                header.AppendFormat("{0} -------- Export Settings --------{1}", CommentChar, Environment.NewLine);
                header.AppendFormat("{0} Export Ok Files= {2}{1}", CommentChar, Environment.NewLine, (checkBoxOk.IsChecked ?? false));
                header.AppendFormat("{0} Export New Files= {2}{1}", CommentChar, Environment.NewLine, (checkBoxNew.IsChecked ?? false));
                header.AppendFormat("{0} Export Update Files= {2}{1}", CommentChar, Environment.NewLine, (checkBoxUpdate.IsChecked ?? false));
                header.AppendFormat("{0} Export Remove Files= {2}{1}", CommentChar, Environment.NewLine, (checkBoxRemove.IsChecked ?? false));
                header.AppendFormat("{0} -----------------------------{1}", CommentChar, Environment.NewLine);
                // output report
                using (var sw = new System.IO.StreamWriter(openFileWindow.FileName.Trim(), false))
                {
                    sw.Write(header.ToString());
                    if (_ReportStats.JobType == "Archive") { sw.WriteLine(GenerateArchiveCSV()); }
                    else { sw.WriteLine(GenerateMirrorCSV()); }
                    sw.Flush();
                    sw.Close();
                }
                this.DialogResult = true;
                this.Close();
            }
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        #endregion
        #region Private Methods
        private int CountReportRows()
        {
            var count = 0;
            if (checkBoxOk.IsChecked ?? false) { count += _ReportStats.OkCount; }
            if (checkBoxNew.IsChecked ?? false) { count += _ReportStats.NewCount; }
            if (checkBoxUpdate.IsChecked ?? false) { count += _ReportStats.UpdateCount; }
            if (checkBoxRemove.IsChecked ?? false) { count += _ReportStats.RemoveCount; }
            return count;
        }
        private IEnumerable<object> GetExportableDataRows()
        {
            var masterList = new List<object>();
            if (checkBoxOk.IsChecked ?? false) { masterList.AddRange(_OkData); }
            if (checkBoxNew.IsChecked ?? false) { masterList.AddRange(_NewData); }
            if (checkBoxUpdate.IsChecked ?? false) { masterList.AddRange(_UpdateData); }
            if (checkBoxRemove.IsChecked ?? false) { masterList.AddRange(_RemoveData); }
            return masterList;
        }
        private string GenerateArchiveCSV()
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
            foreach (Helper.ArchiveFilesDetailsRow item in GetExportableDataRows())
            {
                csvReport.Rows.Add(csvReport.NewRow(item.Action,
                                                    item.SourceFile,
                                                    item.SourceDate,
                                                    item.DestinationArchive,
                                                    item.DestinationFile,
                                                    item.DestinationDate,
                                                    item.DateDifference));
            }
            // output report
            return csvReport.WriteString();
        }
        private string GenerateMirrorCSV()
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
            foreach (Helper.MirrorFilesDetailsRow item in GetExportableDataRows())
            {
                csvReport.Rows.Add(csvReport.NewRow(item.Action,
                                                    item.ItemType,
                                                    item.SourceFile,
                                                    item.SourceDate,
                                                    item.DestinationFile,
                                                    item.DestinationDate,
                                                    item.DateDifference));
            }
            // output report
            return csvReport.WriteString();
        }
        #endregion
    }
}
