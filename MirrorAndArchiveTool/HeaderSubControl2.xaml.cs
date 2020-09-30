using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for HeaderSubControl2.xaml
    /// </summary>
    public partial class HeaderSubControl2 : UserControl
    {
        #region Ctor
        public HeaderSubControl2()
        {
            InitializeComponent();
            DataContext = this;
        }
        public HeaderSubControl2(Helper.ReportFileStatistics stats)
            : this()
        {
            if (stats == null) { throw new ArgumentNullException("stats"); }
            if (stats.JobType == "Archive")
            {
                IsArchiveJob = true;
                ArchivePathsVisibility = System.Windows.Visibility.Visible;
            }
            else
            {
                IsArchiveJob = false;
                MirrorPathsVisibility = System.Windows.Visibility.Visible;
            }
            ReportFileStats = stats;
        }
        #endregion
        #region Public Property
        public bool IsArchiveJob { get; private set; }
        #endregion
        #region Dependency Properties
        public Helper.ReportFileStatistics ReportFileStats
        {
            get { return (Helper.ReportFileStatistics)GetValue(ReportFileStatsProperty); }
            set { SetValue(ReportFileStatsProperty, value); }
        }
        public static readonly DependencyProperty ReportFileStatsProperty =
            DependencyProperty.Register("ReportFileStats", typeof(Helper.ReportFileStatistics), typeof(HeaderSubControl2), new UIPropertyMetadata(null));
        // ----
        public Visibility MirrorPathsVisibility
        {
            get { return (Visibility)GetValue(MirrorPathsVisibilityProperty); }
            set { SetValue(MirrorPathsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty MirrorPathsVisibilityProperty =
            DependencyProperty.Register("MirrorPathsVisibility", typeof(Visibility), typeof(HeaderSubControl2), new UIPropertyMetadata(Visibility.Hidden));
        // ----
        public Visibility ArchivePathsVisibility
        {
            get { return (Visibility)GetValue(ArchivePathsVisibilityProperty); }
            set { SetValue(ArchivePathsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ArchivePathsVisibilityProperty =
            DependencyProperty.Register("ArchivePathsVisibility", typeof(Visibility), typeof(HeaderSubControl2), new UIPropertyMetadata(Visibility.Hidden));
        #endregion
    }
}
