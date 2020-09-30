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
    /// Interaction logic for MirrorJobDetails.xaml
    /// </summary>
    public partial class MirrorJobDetails
        : UserControl
    {
        #region Ctor
        public MirrorJobDetails()
        {
            InitializeComponent();
            DataContext = this;
        }
        public MirrorJobDetails(JobBase job)
            : this()
        {
            if (job == null) { throw new ArgumentNullException("job"); }
            Job = job;
        }
        #endregion
        #region User Control Events
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.LocalPath);
        }
        #endregion
        #region Dependency Properties
        public JobBase Job
        {
            get { return (JobBase)GetValue(JobProperty); }
            set { SetValue(JobProperty, value); }
        }
        public static readonly DependencyProperty JobProperty =
            DependencyProperty.Register("Job", typeof(JobBase), typeof(MirrorJobDetails), new UIPropertyMetadata(null));
        #endregion
    }
}
