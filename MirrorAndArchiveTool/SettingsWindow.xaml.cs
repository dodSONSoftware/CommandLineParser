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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region Ctor
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = this;
            Title = Helper.FormatTitle("Settings");
            App.WriteDebugLog(nameof(SettingsWindow), $"Opening Settings Window");
        }
        public SettingsWindow(GlobalSettings settings)
            : this()
        {
            if (settings == null) { throw new ArgumentNullException("settings"); }
            Settings = settings.Clone();
            PopulateForm();
        }
        #endregion
        #region Private Fields
        #endregion
        #region Dependency Properties
        public GlobalSettings Settings
        {
            get { return (GlobalSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(GlobalSettings), typeof(SettingsWindow), new UIPropertyMetadata(null));
        // ----
        public string DeleteByAgeExplaination
        {
            get { return (string)GetValue(DeleteByAgeExplainationProperty); }
            set { SetValue(DeleteByAgeExplainationProperty, value); }
        }
        public static readonly DependencyProperty DeleteByAgeExplainationProperty =
            DependencyProperty.Register("DeleteByAgeExplaination", typeof(string), typeof(SettingsWindow), new UIPropertyMetadata(""));
        // ----
        public string DeleteBySizeExplaination
        {
            get { return (string)GetValue(DeleteBySizeExplainationProperty); }
            set { SetValue(DeleteBySizeExplainationProperty, value); }
        }
        public static readonly DependencyProperty DeleteBySizeExplainationProperty =
            DependencyProperty.Register("DeleteBySizeExplaination", typeof(string), typeof(SettingsWindow), new UIPropertyMetadata(""));
        // ----
        public string RecommendedActionTimeLimitExplaination
        {
            get { return (string)GetValue(RecommendedActionTimeLimitExplainationProperty); }
            set { SetValue(RecommendedActionTimeLimitExplainationProperty, value); }
        }
        public static readonly DependencyProperty RecommendedActionTimeLimitExplainationProperty =
            DependencyProperty.Register("RecommendedActionTimeLimitExplaination", typeof(string), typeof(SettingsWindow), new UIPropertyMetadata(""));
        // ----
        public string DeleteAutomaticReportsByAgeExplaination
        {
            get { return (string)GetValue(DeleteAutomaticReportsByAgeExplainationProperty); }
            set { SetValue(DeleteAutomaticReportsByAgeExplainationProperty, value); }
        }
        public static readonly DependencyProperty DeleteAutomaticReportsByAgeExplainationProperty =
            DependencyProperty.Register("DeleteAutomaticReportsByAgeExplaination", typeof(string), typeof(SettingsWindow), new UIPropertyMetadata(""));
        // ----
        public string SnapshotTimeLimitExplaination
        {
            get { return (string)GetValue(SnapshotTimeLimitExplainationProperty); }
            set { SetValue(SnapshotTimeLimitExplainationProperty, value); }
        }
        public static readonly DependencyProperty SnapshotTimeLimitExplainationProperty =
            DependencyProperty.Register("SnapshotTimeLimitExplaination", typeof(string), typeof(SettingsWindow), new UIPropertyMetadata(""));
        // ----
        public string SnapshotMaximumExplaination
        {
            get { return (string)GetValue(SnapshotMaximumExplainationProperty); }
            set { SetValue(SnapshotMaximumExplainationProperty, value); }
        }
        public static readonly DependencyProperty SnapshotMaximumExplainationProperty =
            DependencyProperty.Register("SnapshotMaximumExplaination", typeof(string), typeof(SettingsWindow), new UIPropertyMetadata(""));
        #endregion
        #region Windows Events
        // recommended action
        private void ComboBoxRecommendedActionTimeLimit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var x = ((ComboBox)sender).SelectedItem as ComboBoxItem;
            if (x != null)
            {
                Settings.RecommendationActionAgeLimit = TimeSpan.FromMinutes(int.Parse(x.Tag.ToString()));
                RecommendedActionTimeLimitExplaination = string.Format("Will recommend that jobs, with changes, older than {0} should be run", x.Content.ToString().Trim().ToLower());
            }
        }
        // extensions to store
        private void buttonStoreExtensionClear(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("Clear all file extensions{0}{0}Are you sure?", Environment.NewLine),
                                "Clear",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                Settings.FileExtensionsToStore = "";
            }
        }
        private void buttonStoreExtensionExamineZip(object sender, RoutedEventArgs e)
        {
            var wind = new ExamineZipWindow(
                (x) =>
                {
                    Settings.FileExtensionsToStore += "," + x;
                    NormailizeSettingsExtensionsToStore();
                });
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
        }
        private void buttonStoreExtensionImport(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                DefaultExt = ".txt",
                DereferenceLinks = true,
                Title = "Import File",
                Filter = "Text|*.txt|All Files|*.*",
                FilterIndex = 0
            };
            if (dialog.ShowDialog(this) ?? false)
            {
                using (var sr = new System.IO.StreamReader(dialog.FileName))
                {
                    Settings.FileExtensionsToStore = sr.ReadToEnd();
                }
                NormailizeSettingsExtensionsToStore();
            }
        }
        private void buttonStoreExtensionExport(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog()
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "*.txt",
                DereferenceLinks = true,
                OverwritePrompt = true,
                Title = "Export File",
                Filter = "Text|*.txt|All Files|*.*",
                FilterIndex = 0
            };
            if (dialog.ShowDialog(this) ?? false)
            {
                NormailizeSettingsExtensionsToStore();
                using (var sw = new System.IO.StreamWriter(dialog.FileName, false))
                {
                    sw.Write(Settings.FileExtensionsToStore);
                    sw.Flush();
                    sw.Close();
                }
            }
        }
        // automatic reporting
        private void buttonAutomaticReportingRootPathOpen(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(Settings.AutomaticReportingRootPath))
            {
                System.Diagnostics.Process.Start(Settings.AutomaticReportingRootPath);
            }
            else
            {
                MainWindow.DisplayErrorMessageBox("Folder not Found", string.Format("Can not find folder{0}\"{1}\"", Environment.NewLine, Settings.AutomaticReportingRootPath));
            }
        }
        private void buttonAutomaticReportingRootPathBrowse(object sender, RoutedEventArgs e)
        {
            string path = Settings.AutomaticReportingRootPath;
            if (TryBrowseForPath(ref path))
            {
                Settings.AutomaticReportingRootPath = path;
            }
        }
        private void ComboBoxDeleteAutomaticReportsByAge_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.RemovedAutomaticReportsTimeLimit = ComboBoxComboBoxDeleteAutomaticReportsByAgeIndexToTimeSpan();
            if (e.AddedItems.Count > 0)
            {
                var timeStr = ((ComboBoxItem)e.AddedItems[0]).Content.ToString().ToLower();
                DeleteAutomaticReportsByAgeExplaination = string.Format("Will delete reports older than {0}", timeStr);
            }
        }
        private void buttonReportViewer_Click(object sender, RoutedEventArgs e)
        {
            var wind = new ReportViewerWindow(Settings);
            wind.Owner = Window.GetWindow(this);
            wind.ShowDialog();
        }
        // removed files archiving
        private void buttonRemovedFilesArchiveRootPathOpen(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(Settings.RemovedFilesArchiveRootPath))
            {
                System.Diagnostics.Process.Start(Settings.RemovedFilesArchiveRootPath);
            }
            else
            {
                MainWindow.DisplayErrorMessageBox("Folder not Found", string.Format("Can not find folder{0}\"{1}\"", Environment.NewLine, Settings.RemovedFilesArchiveRootPath));
            }
        }
        private void buttonRemovedFilesArchiveRootPathBrowse(object sender, RoutedEventArgs e)
        {
            string path = Settings.RemovedFilesArchiveRootPath;
            if (TryBrowseForPath(ref path))
            {
                Settings.RemovedFilesArchiveRootPath = path;
            }
        }
        private void ComboBoxDeleteByAge_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.RemovedFileArchiveAutoDeleteTimeLimit = ComboBoxDeleteByAgeIndexToTimeSpan();
            if (e.AddedItems.Count > 0)
            {
                var timeStr = ((ComboBoxItem)e.AddedItems[0]).Content.ToString().ToLower();
                DeleteByAgeExplaination = string.Format("Will delete archives older than {0}", timeStr);
            }
        }
        private void ComboBoxDeleteBySize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDeleteBySizeSettingsFromUI();
            UpdateDeleteBySizeExplaination();
        }
        private void TextBoxDeleteBySize_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDeleteBySizeSettingsFromUI();
            UpdateDeleteBySizeExplaination();
        }
        // snapshots        
        private void buttonSnapshotsStoragePathBrowse(object sender, RoutedEventArgs e)
        {
            string path = Settings.ArchiveZipFileSnapshotsRootPath;
            if (TryBrowseForPath(ref path))
            {
                Settings.ArchiveZipFileSnapshotsRootPath = path;
            }
        }
        private void buttonSnapshotsStorageRootPathOpen(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(Settings.ArchiveZipFileSnapshotsRootPath))
            {
                System.Diagnostics.Process.Start(Settings.ArchiveZipFileSnapshotsRootPath);
            }
            else
            {
                MainWindow.DisplayErrorMessageBox("Folder not Found", string.Format("Can not find folder{0}\"{1}\"", Environment.NewLine, Settings.ArchiveZipFileSnapshotsRootPath));
            }
        }
        private void ComboBoxSnapshotTimeLimit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var x = ((ComboBox)sender).SelectedItem as ComboBoxItem;
            if (x != null)
            {
                Settings.ArchiveZipFileSnapshotsMaturityTimeLimit = TimeSpan.FromDays(int.Parse(x.Tag.ToString()));
                SnapshotTimeLimitExplaination = string.Format("Will keep the current archive for {0} before converting it to a snapshot", x.Content.ToString().Trim().ToLower());
            }
        }
        private void ComboBoxSnapshotSnapshotMaximum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var x = ((ComboBox)sender).SelectedItem as ComboBoxItem;
            if (x != null)
            {
                Settings.ArchiveZipFileSnapshotsMaximum = int.Parse(x.Tag.ToString());
                SnapshotMaximumExplaination = string.Format("Will maintain a maximum of {0}", x.Content.ToString().Trim().ToLower());
            }
        }
        // OK & CANCEL & others
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!((Helper.GlobalUserSettings.SettingsWindowLocation.Alpha == -1) &&
                 (Helper.GlobalUserSettings.SettingsWindowLocation.Beta == -1) &&
                 (Helper.GlobalUserSettings.SettingsWindowSize.Alpha == -1) &&
                 (Helper.GlobalUserSettings.SettingsWindowSize.Beta == -1)))
            {
                this.Top = Helper.GlobalUserSettings.SettingsWindowLocation.Alpha;
                this.Left = Helper.GlobalUserSettings.SettingsWindowLocation.Beta;
                this.Width = Helper.GlobalUserSettings.SettingsWindowSize.Alpha;
                this.Height = Helper.GlobalUserSettings.SettingsWindowSize.Beta;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Helper.GlobalUserSettings.SettingsWindowLocation = new Pair<double, double>(this.Top, this.Left);
            Helper.GlobalUserSettings.SettingsWindowSize = new Pair<double, double>(this.Width, this.Height);
        }
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            NormailizeSettingsExtensionsToStore();
            this.DialogResult = true;
            this.Close();
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        //private void buttonAdvanced_Click(object sender, RoutedEventArgs e)
        //{
        //    var wind = new AdvancedSettingsWindow(Settings);
        //    wind.Owner = Window.GetWindow(this);
        //    if (wind.ShowDialog() ?? false)
        //    {
        //        Settings = wind.Settings;
        //    }
        //}
        // keyboard & mouse events
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                this.DialogResult = false;
                this.Close();
            }
        }
        #endregion
        #region Private Methods
        private void PopulateForm()
        {
            comboBoxDeleteByAge.SelectedIndex = TimeSpanToComboBoxDeleteByAgeIndex(Settings.RemovedFileArchiveAutoDeleteTimeLimit);
            UpdateDeleteBySizeControls(Settings.RemovedFileArchiveAutoDeleteByteSizeLimit);
            comboBoxRecommendedActionTimeLimit.SelectedIndex = TimeSpanToComboBoxRecommendedActionTimeLimitIndex(Settings.RecommendationActionAgeLimit);
            comboBoxDeleteAutomaticReportsByAge.SelectedIndex = TimeSpanToComboBoxDeleteAutomaticReportsByAgeIndex(Settings.RemovedAutomaticReportsTimeLimit);
            checkBoxEnableArchiveZipFileSnapshots.IsChecked = Settings.EnableArchiveZipFileSnapshots;
            comboBoxSnapshotTimeLimit.SelectedIndex = TimeSpanToComboBoxSnapshotTimeLimitIndex(Settings.ArchiveZipFileSnapshotsMaturityTimeLimit);
            comboBoxSnapshotMaximum.SelectedIndex = IntegerToComboBoxSnapshotMaximumIndex(Settings.ArchiveZipFileSnapshotsMaximum);
        }
        private void UpdateDeleteBySizeSettingsFromUI()
        {
            // get typed value
            long typedNumber = 0;
            if (!long.TryParse(textBoxDeleteBySize.Text.Trim(), out typedNumber)) { typedNumber = 0; }
            // get selected multiplier
            long multiplier = 1;
            if (!long.TryParse((comboBoxDeleteBySize.SelectedItem as ComboBoxItem).Tag.ToString(), out multiplier)) { multiplier = 1; }
            // set settings
            Settings.RemovedFileArchiveAutoDeleteByteSizeLimit = typedNumber * multiplier;
        }
        private void UpdateDeleteBySizeExplaination()
        {
            DeleteBySizeExplaination = string.Format("When the combined sizes of all archives is larger than {0}, then some archives will be deleted; the oldest archives will be deleted first",
                                                      dodSON.Core.Common.ByteCountHelper.ToString(Settings.RemovedFileArchiveAutoDeleteByteSizeLimit));
        }
        private void UpdateDeleteBySizeControls(long sizeValue)
        {
            // set combobox index
            comboBoxDeleteBySize.SelectedIndex = IntegerToComboBoxDeleteBySizeIndex(sizeValue);
            // set textbox value
            long divisor = 1;
            if (!long.TryParse((comboBoxDeleteBySize.SelectedItem as ComboBoxItem).Tag.ToString(), out divisor)) { divisor = 1; }
            textBoxDeleteBySize.Text = (sizeValue / divisor).ToString();
        }
        private int IntegerToComboBoxDeleteBySizeIndex(long value)
        {
            for (int index = 0; index < comboBoxDeleteBySize.Items.Count; index++)
            {
                var itemSizeInBytes = long.Parse(((ComboBoxItem)comboBoxDeleteBySize.Items[index]).Tag.ToString());
                if (value <= itemSizeInBytes)
                {
                    return (index > 0) ? index - 1 : 0;
                }
            }
            return 3;
        }
        private int TimeSpanToComboBoxDeleteByAgeIndex(TimeSpan timeSpan)
        {
            for (int index = 0; index < comboBoxDeleteByAge.Items.Count; index++)
            {
                var itemDays = int.Parse(((ComboBoxItem)comboBoxDeleteByAge.Items[index]).Tag.ToString());
                if (timeSpan.TotalDays <= itemDays)
                {
                    return index;
                }
            }
            return 0;
        }
        private int TimeSpanToComboBoxRecommendedActionTimeLimitIndex(TimeSpan timeSpan)
        {
            for (int index = 0; index < comboBoxRecommendedActionTimeLimit.Items.Count; index++)
            {
                var itemMinutes = int.Parse(((ComboBoxItem)comboBoxRecommendedActionTimeLimit.Items[index]).Tag.ToString());
                if (timeSpan.TotalMinutes <= itemMinutes)
                {
                    return index;
                }
            }
            return 0;
        }
        private int TimeSpanToComboBoxDeleteAutomaticReportsByAgeIndex(TimeSpan timeSpan)
        {
            for (int index = 0; index < comboBoxDeleteAutomaticReportsByAge.Items.Count; index++)
            {
                var itemDays = int.Parse(((ComboBoxItem)comboBoxDeleteAutomaticReportsByAge.Items[index]).Tag.ToString());
                if (timeSpan.TotalDays <= itemDays)
                {
                    return index;
                }
            }
            return 0;
        }
        private TimeSpan ComboBoxDeleteByAgeIndexToTimeSpan()
        {
            var comboBoxDude = comboBoxDeleteByAge.SelectedItem as ComboBoxItem;
            if (comboBoxDude != null)
            {
                return TimeSpan.FromDays(int.Parse(comboBoxDude.Tag.ToString()));
            }
            return TimeSpan.FromDays(1);
        }
        private TimeSpan ComboBoxComboBoxDeleteAutomaticReportsByAgeIndexToTimeSpan()
        {
            var comboBoxDude = comboBoxDeleteAutomaticReportsByAge.SelectedItem as ComboBoxItem;
            if (comboBoxDude != null)
            {
                return TimeSpan.FromDays(int.Parse(comboBoxDude.Tag.ToString()));
            }
            return TimeSpan.FromDays(1);
        }
        private void NormailizeSettingsExtensionsToStore()
        {
            if (!string.IsNullOrWhiteSpace(Settings.FileExtensionsToStore))
            {
                // normalize Extensions to store list
                var list = new SortedList<string, string>();
                foreach (var item in Settings.FileExtensionsToStore.Split(','))
                {
                    var worker = item.Trim().ToLower();
                    if (!string.IsNullOrWhiteSpace(worker))
                    {
                        while (worker.StartsWith(".")) { worker = worker.Substring(1); }
                        if (!list.ContainsKey(worker)) { list.Add(worker, worker); }
                    }
                }
                var builder = new System.Text.StringBuilder(1024);
                foreach (var kvPair in list) { builder.Append(kvPair.Key + ","); }
                builder.Length--;
                Settings.FileExtensionsToStore = builder.ToString();
            }
        }
        private int TimeSpanToComboBoxSnapshotTimeLimitIndex(TimeSpan timeSpan)
        {
            for (int index = 0; index < comboBoxSnapshotTimeLimit.Items.Count; index++)
            {
                var itemDays = int.Parse(((ComboBoxItem)comboBoxSnapshotTimeLimit.Items[index]).Tag.ToString());
                if (timeSpan.TotalDays <= itemDays)
                {
                    return index;
                }
            }
            return 0;
        }
        private int IntegerToComboBoxSnapshotMaximumIndex(long value)
        {
            for (int index = 0; index < comboBoxSnapshotMaximum.Items.Count; index++)
            {
                var itemSizeInBytes = long.Parse(((ComboBoxItem)comboBoxSnapshotMaximum.Items[index]).Tag.ToString());
                if (value <= itemSizeInBytes)
                {
                    return (index > 0) ? index : 0;
                }
            }
            return 3;
        }
        private bool TryBrowseForPath(ref string path)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = path;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                    return true;
                }
            }
            path = "";
            return false;
        }
        #endregion
    }
}
