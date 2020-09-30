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
    /// Interaction logic for AdvancedSettingsWindow.xaml
    /// </summary>
    public partial class AdvancedSettingsWindow : Window
    {
        #region Ctor
        public AdvancedSettingsWindow()
        {
            InitializeComponent();
            DataContext = this;
            Title = "Advanced Settings" + Helper.TitleBarSuffix;
        }
        public AdvancedSettingsWindow(GlobalSettings settings)
            : this()
        {
            if (settings == null) { throw new ArgumentNullException("settings"); }
            Settings = settings.Clone();
            PopulateForm();
        }
        #endregion
        #region Dependency Properties
        public GlobalSettings Settings
        {
            get { return (GlobalSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(GlobalSettings), typeof(AdvancedSettingsWindow), new UIPropertyMetadata(null));
        // ----
        public string  SnapshotTimeLimitExplaination
        {
            get { return (string )GetValue(SnapshotTimeLimitExplainationProperty); }
            set { SetValue(SnapshotTimeLimitExplainationProperty, value); }
        }
        public static readonly DependencyProperty SnapshotTimeLimitExplainationProperty =
            DependencyProperty.Register("SnapshotTimeLimitExplaination", typeof(string ), typeof(AdvancedSettingsWindow), new UIPropertyMetadata(""));
        // ----
        public string SnapshotMaximumExplaination
        {
            get { return (string)GetValue(SnapshotMaximumExplainationProperty); }
            set { SetValue(SnapshotMaximumExplainationProperty, value); }
        }
        public static readonly DependencyProperty SnapshotMaximumExplainationProperty =
            DependencyProperty.Register("SnapshotMaximumExplaination", typeof(string), typeof(AdvancedSettingsWindow), new UIPropertyMetadata(""));
        #endregion
        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = Helper.GlobalUserSettings.AdvancedSettingsWindowLocation.Alpha;
            this.Left = Helper.GlobalUserSettings.AdvancedSettingsWindowLocation.Beta;
            this.Width = Helper.GlobalUserSettings.AdvancedSettingsWindowSize.Alpha;
            this.Height = Helper.GlobalUserSettings.AdvancedSettingsWindowSize.Beta;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Helper.GlobalUserSettings.AdvancedSettingsWindowLocation = new Pair<double, double>(this.Top, this.Left);
            Helper.GlobalUserSettings.AdvancedSettingsWindowSize = new Pair<double, double>(this.Width, this.Height);
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

        }
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
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
                SnapshotTimeLimitExplaination = string.Format("Will keep the current snapshot for {0}", x.Content.ToString().Trim().ToLower());
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
        #endregion
        #region Private Methods
        private void PopulateForm()
        {
            checkBoxEnableArchiveZipFileSnapshots.IsChecked = Settings.EnableArchiveZipFileSnapshots;
            comboBoxSnapshotTimeLimit.SelectedIndex = TimeSpanToComboBoxSnapshotTimeLimitIndex(Settings.ArchiveZipFileSnapshotsMaturityTimeLimit);
            comboBoxSnapshotMaximum.SelectedIndex = IntegerToComboBoxSnapshotMaximumIndex(Settings.ArchiveZipFileSnapshotsMaximum);
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
                    return (index > 0) ? index  : 0;
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
