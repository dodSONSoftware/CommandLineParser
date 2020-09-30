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
    /// Interaction logic for EditorMirrorControl.xaml
    /// </summary>
    public partial class EditorMirrorControl
        : UserControl,
        MirrorAndArchiveTool.IValidEditorControl
    {
        #region Ctor
        public EditorMirrorControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        public EditorMirrorControl(JobBase job, Action<bool> onValueChanged)
            : this()
        {
            if (job == null) { throw new ArgumentNullException("job"); }
            if ((job as MirrorJob) == null) { throw new ArgumentNullException("(job as MirrorJob)"); }
            if (onValueChanged == null) { throw new ArgumentNullException("onValueChanged"); }
            Job = (job as MirrorJob);
            _OnValueChanged = onValueChanged;
        }
        #endregion
        #region Private Fields
        private Action<bool> _OnValueChanged = null;
        #endregion
        #region Dependency Properties
        public MirrorJob Job
        {
            get { return (MirrorJob)GetValue(JobProperty); }
            set { SetValue(JobProperty, value); }
        }
        public static readonly DependencyProperty JobProperty =
            DependencyProperty.Register("Job", typeof(MirrorJob), typeof(EditorMirrorControl), new UIPropertyMetadata(null));
        #endregion
        #region Public Methods
        public bool IsFormDataValid
        {
            get
            {
                // test Job.SourcePath
                // test Job.MirrorPath
                var sourceOK = false;
                var mirrorOK = false;
                if (!string.IsNullOrWhiteSpace(Job.SourcePath) && (System.IO.Directory.Exists(Job.SourcePath))) { sourceOK = true; }
                if (!string.IsNullOrWhiteSpace(Job.MirrorPath) && (System.IO.Directory.Exists(Job.MirrorPath))) { mirrorOK = true; }
                return sourceOK && mirrorOK;
            }
        }
        #endregion
        #region User Control Methods
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Tag.ToString() == "Source") { Job.SourcePath = ((TextBox)sender).Text.Trim(); }
            else { Job.MirrorPath = ((TextBox)sender).Text.Trim(); }
            _OnValueChanged(IsFormDataValid);
        }
        // ----
        private void buttonSourcePathOpen(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(Job.SourcePath))
            {
                System.Diagnostics.Process.Start(Job.SourcePath);
            }
            else
            {
                MainWindow.DisplayErrorMessageBox("Folder not Found", string.Format("Can not find folder{0}\"{1}\"", Environment.NewLine, Job.SourcePath));
            }
        }
        private void buttonSourcePathBrowse(object sender, RoutedEventArgs e)
        {
            string path = Job.SourcePath;
            if (TryBrowseForPath(ref path))
            {
                Job.SourcePath = path;
            }
        }
        // ----
        private void buttonDestinationPathOpen(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(Job.MirrorPath))
            {
                System.Diagnostics.Process.Start(Job.MirrorPath);
            }
            else
            {
                MainWindow.DisplayErrorMessageBox("Folder not Found", string.Format("Can not find folder{0}\"{1}\"", Environment.NewLine, Job.MirrorPath));
            }
        }
        private void buttonDestinationPathBrowse(object sender, RoutedEventArgs e)
        {
            string path = Job.MirrorPath;
            if (TryBrowseForPath(ref path))
            {
                Job.MirrorPath = path;
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
        #endregion

    }
}
