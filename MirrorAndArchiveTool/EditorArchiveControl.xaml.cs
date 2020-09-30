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
    /// Interaction logic for EditorArchiveControl.xaml
    /// </summary>
    public partial class EditorArchiveControl
        : UserControl,
        MirrorAndArchiveTool.IValidEditorControl
    {
        #region Ctor
        public EditorArchiveControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        public EditorArchiveControl(JobBase job, Action<bool> onValueChanged)
            : this()
        {
            if (job == null) { throw new ArgumentNullException("job"); }
            if ((job as ArchiveJob) == null) { throw new ArgumentNullException("(job as ArchiveJob)"); }
            if (onValueChanged == null) { throw new ArgumentNullException("onValueChanged"); }
            Job = (job as ArchiveJob);
            _OnValueChanged = onValueChanged;
        }
        #endregion
        #region Private Fields
        private Action<bool> _OnValueChanged = null;
        #endregion
        #region Dependency Properties
        public ArchiveJob Job
        {
            get { return (ArchiveJob)GetValue(JobProperty); }
            set { SetValue(JobProperty, value); }
        }
        public static readonly DependencyProperty JobProperty =
            DependencyProperty.Register("Job", typeof(ArchiveJob), typeof(EditorArchiveControl), new UIPropertyMetadata(null));
        #endregion
        #region Public Methods
        public bool IsFormDataValid
        {
            get
            {
                // test Job.ArchiveRootPath
                // test Job.SourcePaths
                var archiveRootPathOk = false;
                var sourcePathsOk = true;
                if (!string.IsNullOrWhiteSpace(Job.ArchiveRootPath) && (System.IO.Directory.Exists(Job.ArchiveRootPath))) { archiveRootPathOk = true; }
                foreach (var item in Job.SourcePaths)
                {
                    if (string.IsNullOrWhiteSpace(item) && (!System.IO.Directory.Exists(item))) { sourcePathsOk = false; }
                }
                return archiveRootPathOk && sourcePathsOk && (Job.SourcePaths.Count > 0);
            }
        }
        #endregion
        #region User Control Events
        private void buttonText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Job.ArchiveRootPath = ((TextBox)sender).Text.Trim();
            _OnValueChanged(IsFormDataValid);
        }
        private void buttonArchiveStoragePathBrowseOpen(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(Job.ArchiveRootPath))
            {
                System.Diagnostics.Process.Start(Job.ArchiveRootPath);
            }
            else
            {
                MainWindow.DisplayErrorMessageBox("Folder not Found", string.Format("Can not find folder{0}\"{1}\"", Environment.NewLine, Job.ArchiveRootPath));
            }
        }
        private void buttonArchiveStoragePathBrowse(object sender, RoutedEventArgs e)
        {
            string path = Job.ArchiveRootPath;
            if (TryBrowseForPath(ref path))
            {
                Job.ArchiveRootPath = path;
            }
        }
        // ----
        private void listBoxSourcePaths_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((System.Windows.Controls.ListBox)(sender)).SelectedItems.Count > 0)
            {
                var dirPath = ((System.Windows.Controls.ListBox)(sender)).SelectedItems[0].ToString();
                try { System.Diagnostics.Process.Start(dirPath); }
                catch
                {
                    var msg = $"The selected path does not exist.{Environment.NewLine}{Environment.NewLine}Delete entry from List?";
                    if (MessageBox.Show(msg, "Invalid Path", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        Job.RemoveSourcePath(dirPath);
                        _OnValueChanged(IsFormDataValid);
                    }
                }
            }
        }
        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            // ((TextBlock)sender).Foreground = new SolidColorBrush(Colors.Blue);
            ((TextBlock)sender).TextDecorations = System.Windows.TextDecorations.Underline;
        }
        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            //((TextBlock)sender).Foreground = new SolidColorBrush(Colors.Black);
            ((TextBlock)sender).ClearValue(TextBlock.ForegroundProperty);
            ((TextBlock)sender).TextDecorations = null;
        }
        // ----
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            var path = "";
            if (Job.SourcePaths.Count > 0)
            {
                path = Job.SourcePaths[Job.SourcePaths.Count - 1];
            }
            if (TryBrowseForPath(ref path))
            {
                Job.AddSourcePath(path);
                SortSourcePaths();
                _OnValueChanged(IsFormDataValid);
            }
        }
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxSourcePaths.SelectedItem != null)
            {
                if (MessageBox.Show(string.Format("Remove \"{1}\"{0}{0}Are you sure?", Environment.NewLine, (string)listBoxSourcePaths.SelectedItem),
                                    string.Format("Remove Selected Folder"),
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Hand) == MessageBoxResult.Yes)
                {
                    Job.RemoveSourcePath((string)listBoxSourcePaths.SelectedItem);
                    _OnValueChanged(IsFormDataValid);
                }
            }
        }
        private void buttonClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("Remove All Folders{0}{0}Are you sure?", Environment.NewLine),
                                string.Format("Remove All Folders"),
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Hand) == MessageBoxResult.Yes)
            {
                Job.ClearAllSourcePaths();
                _OnValueChanged(IsFormDataValid);
            }
        }
        #endregion
        #region Private Methods
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
        private void SortSourcePaths()
        {
            var list = new SortedList<string, string>();
            foreach (var item in Job.SourcePaths) { list.Add(item, item); }
            Job.ClearAllSourcePaths();
            foreach (var item in list.Values) { Job.AddSourcePath(item); }
        }
        #endregion
    }
}
