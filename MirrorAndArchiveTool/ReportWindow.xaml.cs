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
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        #region Ctor
        public ReportWindow()
        {
            InitializeComponent();
            DataContext = this;
            Title = "Report" + Helper.TitleBarSuffix;
        }
        public ReportWindow(JobBase job,
                            IEnumerable<dodSON.BuildingBlocks.FileStorage.ICompareResult> report)
            : this()
        {
            var stats = Helper.AnalyzeReport(job, report);
            IEnumerable<object> okData = null;
            IEnumerable<object> newData = null;
            IEnumerable<object> updateData = null;
            IEnumerable<object> removeData = null;
           Helper.ExtractLists(job, report, out okData, out newData, out updateData, out removeData);
            // display user control
            gridContentHolder.Children.Add(new ReportViewerUserControl2(true, (job is ArchiveJob), stats, okData, newData, updateData, removeData));
        }
        #endregion
    }
}
