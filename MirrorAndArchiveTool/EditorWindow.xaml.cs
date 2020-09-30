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
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow
        : Window
    {
        #region Ctor
        public EditorWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        public EditorWindow(JobBase job,
                            bool isNewJob,
                            IEnumerable<string> jobNames)
            : this()
        {
            if (job == null) { throw new ArgumentNullException("job"); }
            _Job = job.Clone();
            _JobNames = jobNames;
            Title = Helper.FormatTitle((_Job is ArchiveJob ? "Archive" : "Mirror") + " Editor");
            //Title = (_Job is ArchiveJob ? "Archive " : "Mirror ") + "Editor" + Helper.TitleBarSuffix;
            PopulateControls(isNewJob);
            App.WriteDebugLog(nameof(EditorWindow), $"Opening Editor Window, Job={Job.Name}");
        }
        #endregion
        #region Private Fields
        private JobBase _Job = null;
        private IValidEditorControl _CurrentDynamicUserControl = null;
        private IEnumerable<string> _JobNames = null;
        #endregion
        #region Dependency Properties
        public bool ButtonOkIsEnabled
        {
            get { return (bool)GetValue(ButtonOkIsEnabledProperty); }
            set { SetValue(ButtonOkIsEnabledProperty, value); }
        }
        public static readonly DependencyProperty ButtonOkIsEnabledProperty =
            DependencyProperty.Register("ButtonOkIsEnabled", typeof(bool), typeof(EditorWindow), new UIPropertyMetadata(false));
        #endregion
        #region Public Methods
        public JobBase Job
        {
            get { return _Job; }
        }
        #endregion
        #region Window Events
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
        private void PopulateControls(bool isNewJob)
        {
            // control display of ADD symbol
            if (isNewJob) { imageNewEditorImage.Visibility = System.Windows.Visibility.Visible; }
            else { imageNewEditorImage.Visibility = System.Windows.Visibility.Hidden; }
            // add common controls before specific controls
            var nameControlOK = false;
            var dynamicControlOK = false;
            stackPanelBeforeDynamicControls.Children.Add(new EditorCommonControl1(_Job,
                    (validData) =>
                    {
                        // execute on content change
                        nameControlOK = validData;
                        ButtonOkIsEnabled = (validData && dynamicControlOK);
                    }, _JobNames));
            // add job specific control
            _CurrentDynamicUserControl = null;
            if (_Job is MirrorJob)
            {
                imageEditorImage.Source = new BitmapImage(new Uri("Images\\folders.ico", UriKind.RelativeOrAbsolute));
                _CurrentDynamicUserControl = new EditorMirrorControl(_Job,
                    (validData) =>
                    {
                        // execute on content change
                        dynamicControlOK = validData;
                        ButtonOkIsEnabled = (validData && dynamicControlOK);
                    });
                gridDynamicControls.Children.Add((UserControl)_CurrentDynamicUserControl);
            }
            else if (_Job is ArchiveJob)
            {
                imageEditorImage.Source = new BitmapImage(new Uri("Images\\decompress_48_h.ico", UriKind.RelativeOrAbsolute));
                _CurrentDynamicUserControl = new EditorArchiveControl(_Job,
                    (validData) =>
                    {
                        // execute on content change
                        dynamicControlOK = validData;
                        ButtonOkIsEnabled = (validData && dynamicControlOK);
                    });
                gridDynamicControls.Children.Add((UserControl)_CurrentDynamicUserControl);
            }
            // add common controls after specific controls
            stackPanelAfterDynamicControls.Children.Add(new EditorCommonControls(_Job));
        }
        #endregion
    }
}
