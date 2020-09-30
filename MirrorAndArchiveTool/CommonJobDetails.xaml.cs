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
    /// Interaction logic for CommonJobDetails.xaml
    /// </summary>
    public partial class CommonJobDetails
        : UserControl
    {
        #region Ctor
        public CommonJobDetails()
        {
            InitializeComponent();
            DataContext = this;
        }
        public CommonJobDetails(JobBase job)
            : this()
        {
            if (job == null) { throw new ArgumentNullException("job"); }
            Job = job;
        }
        #endregion
        #region Dependency Properties
        public JobBase Job
        {
            get { return (JobBase)GetValue(JobProperty); }
            set { SetValue(JobProperty, value); }
        }
        public static readonly DependencyProperty JobProperty =
            DependencyProperty.Register("Job", typeof(JobBase), typeof(CommonJobDetails), new UIPropertyMetadata(null));
        #endregion
    }
}
