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
    /// Interaction logic for EditorCommonControl1.xaml
    /// </summary>
    public partial class EditorCommonControl1
        : UserControl,
          IValidEditorControl
    {
        #region Ctor
        public EditorCommonControl1()
        {
            InitializeComponent();
            DataContext = this;
        }
        public EditorCommonControl1(JobBase job,
                                    Action<bool> onValueChanged,
                                    IEnumerable<string> jobNames)
            : this()
        {
            if (job == null) { throw new ArgumentNullException("job"); }
            if (onValueChanged == null) { throw new ArgumentNullException("onValueChanged"); }
            Job = job;
            _OnValueChanged = onValueChanged;
            _JobNames = jobNames;
        }
        #endregion
        #region Private Fields
        private Action<bool> _OnValueChanged = null;
        private IEnumerable<string> _JobNames = null;
        #endregion
        #region Public Methods
        public bool IsFormDataValid
        {
            get { return ((!string.IsNullOrWhiteSpace(Job.Name)) && (!(_JobNames.Contains(Job.Name)))); }
        }
        #endregion
        #region Dependency Properties
        public JobBase Job
        {
            get { return (JobBase)GetValue(JobProperty); }
            set { SetValue(JobProperty, value); }
        }
        public static readonly DependencyProperty JobProperty =
            DependencyProperty.Register("Job", typeof(JobBase), typeof(EditorCommonControl1), new UIPropertyMetadata(null));
        #endregion
        #region User Control Events
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: this keyboard thing is odd

            var pos = ((TextBox)sender).SelectionStart;
            Job.Name = ((TextBox)sender).Text;
            ((TextBox)sender).SelectionStart = pos;
            _OnValueChanged(IsFormDataValid);
        }
        #endregion
    }
}
