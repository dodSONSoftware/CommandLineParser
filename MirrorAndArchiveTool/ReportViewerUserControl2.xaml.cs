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
    /// Interaction logic for ReportViewerUserControl2.xaml
    /// </summary>
    public partial class ReportViewerUserControl2 : UserControl
    {
        #region Ctor
        public ReportViewerUserControl2()
        {
            InitializeComponent();
            DataContext = this;
        }
        public ReportViewerUserControl2(bool showExportButton,
                                        string destPath,
                                        Helper.ReportFileStatistics header,
                                        IEnumerable<object> okData,
                                        IEnumerable<object> newData,
                                        IEnumerable<object> updateData,
                                        IEnumerable<object> removeData,
                                        GlobalSettings settings)
            : this()
        {
            _ShowExportButton = showExportButton;
            _DestPath = (string.IsNullOrWhiteSpace(destPath)) ? "" : $"{destPath}\\";
            _Header = header ?? throw new ArgumentNullException(nameof(header));
            _OkData = okData;
            _NewData = newData;
            _UpdateData = updateData;
            _RemoveData = removeData;
            _Settings = settings;
            InitializeButtons();
            UpdateStatusBar();
            PopulateDataGrid();
        }
        #endregion
        #region Private Fields
        private readonly bool _ShowExportButton = true;
        private readonly string _DestPath = "";
        private readonly Helper.ReportFileStatistics _Header = null;
        private readonly IEnumerable<object> _OkData = null;
        private readonly IEnumerable<object> _NewData = null;
        private readonly IEnumerable<object> _UpdateData = null;
        private readonly IEnumerable<object> _RemoveData = null;
        private readonly GlobalSettings _Settings = null;
        #endregion
        #region Window Events
        private void toggleButtonOk_Click(object sender, RoutedEventArgs e)
        {
            PopulateDataGrid();
        }
        private void toggleButtonNew_Click(object sender, RoutedEventArgs e)
        {
            PopulateDataGrid();
        }
        private void toggleButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            PopulateDataGrid();
        }
        private void toggleButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            PopulateDataGrid();
        }
        private void buttonHeaderInfo_Click(object sender, RoutedEventArgs e)
        {
            //_Header.ReportDate = DateTime.Now;
            var wind = new HeaderWindow(new HeaderSubControl2(_Header));
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
        }
        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            var wind = new ExportReportWindow(_Header,
                                              _OkData,
                                              _NewData,
                                              _UpdateData,
                                              _RemoveData,
                                              (toggleButtonOk.Visibility == System.Windows.Visibility.Visible), (toggleButtonOk.IsChecked ?? false),
                                              (toggleButtonNew.Visibility == System.Windows.Visibility.Visible), (toggleButtonNew.IsChecked ?? false),
                                              (toggleButtonUpdate.Visibility == System.Windows.Visibility.Visible), (toggleButtonUpdate.IsChecked ?? false),
                                              (toggleButtonRemove.Visibility == System.Windows.Visibility.Visible), (toggleButtonRemove.IsChecked ?? false));
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
        }
        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
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
            if (openFileWindow.ShowDialog() ?? false)
            {
                var zipFilename = System.IO.Path.Combine(_Settings.AutomaticReportingRootPath, "AutomaticReports.ReportsArchive.zip");
                var zip = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
                var filename = _Header.JobName + "." + _Header.ReportDate.ToString("yyyyMMdd^HHmmss") + "." + "JobReport.txt";
                if (zip.Contains(System.IO.Path.GetFileName(filename)))
                {
                    zip.Extract(new dodSON.Core.FileStorage.IFileStoreItem[] { zip.Get(filename) }, true);
                    filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), filename);
                    if (System.IO.File.Exists(filename))
                    {
                        if (System.IO.File.Exists(openFileWindow.FileName))
                        {
                            var fInfo = new System.IO.FileInfo(openFileWindow.FileName);
                            fInfo.IsReadOnly = false;
                            System.IO.File.Delete(openFileWindow.FileName);
                        }
                        System.IO.File.Move(filename, openFileWindow.FileName);
                    }
                }
                else
                {
                    MainWindow.DisplayErrorMessageBox("File not Found",
                                                     $"Cannot find report file{Environment.NewLine}{((System.Windows.Documents.Hyperlink)e.Source).NavigateUri}");
                }
            }
        }
        private void buttonDefaultSort_Click(object sender, RoutedEventArgs e)
        {
            PopulateDataGrid();
        }
        #region Commands
        // ---------- source path
        private bool TryGetSourcePath(out string filePath)
        {
            filePath = "";
            if (dataGridFilesDetails.SelectedItem is Helper.MirrorFilesDetailsRow)
            {
                var dude = (dataGridFilesDetails.SelectedItem as Helper.MirrorFilesDetailsRow);
                if (dude.ItemType == "Directory")
                {
                    try { filePath = dude.SourceFile; }
                    catch { }
                }
                else if (dude.ItemType == "File")
                {
                    try { filePath = System.IO.Path.GetDirectoryName(dude.SourceFile); }
                    catch { }
                }
            }
            else if (dataGridFilesDetails.SelectedItem is Helper.ArchiveFilesDetailsRow)
            {
                var dude = (dataGridFilesDetails.SelectedItem as Helper.ArchiveFilesDetailsRow);
                try { filePath = System.IO.Path.GetDirectoryName(dude.SourceFile); }
                catch { }
            }
            //
            return !string.IsNullOrWhiteSpace(filePath);
        }
        private bool TryGetSourceFilePath(out string filePath)
        {
            filePath = "";
            if (dataGridFilesDetails.SelectedItem is Helper.MirrorFilesDetailsRow)
            {
                filePath = (dataGridFilesDetails.SelectedItem as Helper.MirrorFilesDetailsRow).SourceFile;
            }
            else if (dataGridFilesDetails.SelectedItem is Helper.ArchiveFilesDetailsRow)
            {
                try { filePath = (dataGridFilesDetails.SelectedItem as Helper.ArchiveFilesDetailsRow).SourceFile; }
                catch { }
            }
            //
            return !string.IsNullOrWhiteSpace(filePath);
        }
        public ICommand OpenSourcePathCommand => new DelegateCommand(x =>
        {
            // execute
            if (TryGetSourcePath(out string source))
            {
                try { System.Diagnostics.Process.Start(source); }
                catch
                {
                    MainWindow.DisplayErrorMessageBox("Path not Found",
                                                      $"Cannot find Source Path:{Environment.NewLine}{source}");
                }
            }
        },
        x =>
        {
            // can execute
            return TryGetSourcePath(out string _);
        });
        public ICommand CopyToClipboardSourcePathCommand => new DelegateCommand(x =>
        {
            // execute 
            TryGetSourcePath(out string source);
            Clipboard.SetText(source);
        },
        x =>
        {
            // can execute
            return TryGetSourcePath(out string _);
        });
        public ICommand CopyToClipboardSourceFilePathCommand =>
        new DelegateCommand(x =>
        {
            // execute 
            TryGetSourceFilePath(out string source);
            Clipboard.SetText(source);
        },
        x =>
        {
            // can execute
            return TryGetSourceFilePath(out string _);
        });
        // ---------- destination path
        private bool TryGetDestinationPath(out string filePath)
        {
            filePath = "";
            if (dataGridFilesDetails.SelectedItem is Helper.MirrorFilesDetailsRow)
            {
                try
                {
                    var dude = (dataGridFilesDetails.SelectedItem as Helper.MirrorFilesDetailsRow);
                    if (dude.ItemType.Equals("directory", StringComparison.InvariantCultureIgnoreCase)) { filePath = dude.DestinationFile; }
                    else { filePath = System.IO.Path.GetDirectoryName(dude.DestinationFile); }
                }
                catch { }
            }
            else if (dataGridFilesDetails.SelectedItem is Helper.ArchiveFilesDetailsRow)
            {
                try
                {
                    var dude = (dataGridFilesDetails.SelectedItem as Helper.ArchiveFilesDetailsRow);
                    filePath = $"{_DestPath}{dude.DestinationArchive}\\{System.IO.Path.GetDirectoryName(dude.DestinationFile)}";
                }
                catch { }
            }
            //
            return !string.IsNullOrWhiteSpace(filePath);
        }
        private bool TryGetDestinationFilePath(out string filePath)
        {
            filePath = "";
            if (dataGridFilesDetails.SelectedItem is Helper.MirrorFilesDetailsRow)
            {
                var dude = (dataGridFilesDetails.SelectedItem as Helper.MirrorFilesDetailsRow);
                if (dude.ItemType == "Directory")
                {
                    try { filePath = dude.DestinationFile; }
                    catch { }
                }
                else if (dude.ItemType == "File")
                {
                    try { filePath = dude.DestinationFile; }
                    catch { }
                }
            }
            else if (dataGridFilesDetails.SelectedItem is Helper.ArchiveFilesDetailsRow)
            {
                try
                {
                    var dude = (dataGridFilesDetails.SelectedItem as Helper.ArchiveFilesDetailsRow);
                    if (!string.IsNullOrWhiteSpace(dude.DestinationFile))
                    {
                        filePath = $"{_DestPath}{dude.DestinationArchive}\\{dude.DestinationFile}";
                    }
                }
                catch { }
            }
            //
            return !string.IsNullOrWhiteSpace(filePath);
        }
        public ICommand OpenDestinationPathCommand => new DelegateCommand(x =>
        {
            // execute
            if (TryGetDestinationPath(out string destination))
            {
                try { System.Diagnostics.Process.Start(destination); }
                catch
                {
                    MainWindow.DisplayErrorMessageBox("Path not Found",
                                                      $"Cannot find Destination Path:{Environment.NewLine}{destination}");
                }
            }
        },
        x =>
        {
            // can execute
            return TryGetDestinationPath(out string _);
        });
        public ICommand CopyToClipboardDestinationPathCommand => new DelegateCommand(x =>
        {
            // execute 
            TryGetDestinationPath(out string destination);
            Clipboard.SetText(destination);
        },
        x =>
        {
            // can execute
            return TryGetDestinationPath(out string _);
        });
        public ICommand CopyToClipboardDestinationFilePathCommand =>
        new DelegateCommand(x =>
        {
            // execute 
            TryGetDestinationFilePath(out string destination);
            Clipboard.SetText(destination);
        },
        x =>
        {
            // can execute
            return TryGetDestinationFilePath(out string _);
        });
        #endregion
        #endregion
        #region Private Methods
        private void InitializeButtons()
        {
            toggleButtonOk.IsChecked = false;
            if (_Header.TotalChangedCount == 0) { toggleButtonOk.IsChecked = true; }
            toggleButtonNew.IsChecked = true;
            toggleButtonUpdate.IsChecked = true;
            toggleButtonRemove.IsChecked = true;
            buttonExport.Visibility = (_ShowExportButton) ? Visibility.Visible : Visibility.Collapsed;
            buttonSave.Visibility = (_ShowExportButton) ? Visibility.Collapsed : Visibility.Visible;
            // ----
            if (_Header.ExportOK && (_OkData.Count() > 0)) { toggleButtonOk.Visibility = System.Windows.Visibility.Visible; }
            else { toggleButtonOk.Visibility = System.Windows.Visibility.Collapsed; }
            if (_Header.ExportNew && (_NewData.Count() > 0)) { toggleButtonNew.Visibility = System.Windows.Visibility.Visible; }
            else { toggleButtonNew.Visibility = System.Windows.Visibility.Collapsed; }
            if (_Header.ExportUpdate && (_UpdateData.Count() > 0)) { toggleButtonUpdate.Visibility = System.Windows.Visibility.Visible; }
            else { toggleButtonUpdate.Visibility = System.Windows.Visibility.Collapsed; }
            if (_Header.ExportRemove && (_RemoveData.Count() > 0)) { toggleButtonRemove.Visibility = System.Windows.Visibility.Visible; }
            else { toggleButtonRemove.Visibility = System.Windows.Visibility.Collapsed; }
        }
        private void UpdateStatusBar()
        {
            textBlockTotalReportItemsCount.Text = string.Format("{0:N0} total", _Header.TotalCount);

            if (_Header.ExportOK && (_OkData.Count() > 0)) { textBlockTotalReportOkCount.Text = string.Format("{0:N0} ok ({1:N2}%)", _Header.OkCount, (((double)_Header.OkCount / (double)_Header.TotalCount) * 100.0)); }
            else { textBlockTotalReportOkCount.Text = ""; }

            if (_Header.ExportNew && (_NewData.Count() > 0)) { textBlockTotalReportNewCount.Text = string.Format("{0:N0} new ({1:N2}%)", _Header.NewCount, (((double)_Header.NewCount / (double)_Header.TotalCount) * 100.0)); }
            else { textBlockTotalReportNewCount.Text = ""; }

            if (_Header.ExportUpdate && (_UpdateData.Count() > 0)) { textBlockTotalReportUpdateCount.Text = string.Format("{0:N0} update ({1:N2}%)", _Header.UpdateCount, (((double)_Header.UpdateCount / (double)_Header.TotalCount) * 100.0)); }
            else { textBlockTotalReportUpdateCount.Text = ""; }

            if (_Header.ExportRemove && (_RemoveData.Count() > 0)) { textBlockTotalReportRemoveCount.Text = string.Format("{0:N0} remove ({1:N2}%)", _Header.RemoveCount, (((double)_Header.RemoveCount / (double)_Header.TotalCount) * 100.0)); }
            else { textBlockTotalReportRemoveCount.Text = ""; }
        }
        private void PopulateDataGrid()
        {
            var masterList = new List<object>();
            if ((toggleButtonOk.IsChecked ?? false)) { masterList.AddRange(_OkData); }
            if ((toggleButtonNew.IsChecked ?? false)) { masterList.AddRange(_NewData); }
            if ((toggleButtonUpdate.IsChecked ?? false)) { masterList.AddRange(_UpdateData); }
            if ((toggleButtonRemove.IsChecked ?? false)) { masterList.AddRange(_RemoveData); }
            // update UI
            dataGridFilesDetails.ItemsSource = masterList;
        }
        #endregion

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var dude = sender as ContextMenu;
            var dudeHold = dude.DataContext;
            dude.DataContext = null;
            dude.DataContext = dudeHold;
        }
    }
}
