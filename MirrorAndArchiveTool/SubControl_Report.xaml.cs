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
    /// Interaction logic for SubControl_Report.xaml
    /// </summary>
    public partial class SubControl_Report
        : UserControl,
          ISubUserControl
    {
        #region Ctor
        private SubControl_Report()
            : base()
        {
            InitializeComponent();
            DataContext = this;
        }
        public SubControl_Report(JobBase job,
                                 IEnumerable<dodSON.Core.FileStorage.ICompareResult> report,
                                 GlobalSettings settings)
            : this()
        {
            _Job = job;
            _Report = report;
            _Settings = settings;
            UpdateUI(job, report);
            App.WriteDebugLog(nameof(SubControl_Report), $"Viewing Sub Control: REPORT, Job={_Job.Name}");
        }
        public SubControl_Report(bool showExportButton,
                                 Helper.ReportFileStatistics header,
                                 IEnumerable<object> okData,
                                 IEnumerable<object> newData,
                                 IEnumerable<object> updateData,
                                 IEnumerable<object> removeData,
                                 GlobalSettings settings)
            : this()
        {
            _Settings = settings;
            var dude = new ReportViewerUserControl2(showExportButton, Helper.GetDestinationDirectoryName(_Job), header, okData, newData, updateData, removeData, settings);
            gridContentHolder.Children.Add(dude);
            App.WriteDebugLog(nameof(SubControl_Report), $"Viewing Sub Control: REPORT, Job={header.JobName}");
        }
        #endregion
        #region Private Fields
        private JobBase _Job = null;
        private IEnumerable<dodSON.Core.FileStorage.ICompareResult> _Report = null;
        private GlobalSettings _Settings = null;
        #endregion
        #region ISubUserControl Methods
        public string Title
        {
            get { return "Report"; }
        }
        public string Key
        {
            get { return "REPORT"; }
        }
        public void Refresh(IEnumerable<dodSON.Core.FileStorage.ICompareResult> report)
        {
            _Report = report;
            UpdateUI(_Job, _Report);
        }
        public void Shutdown()
        {

        }
        #endregion
        #region Private Methods
        private void UpdateUI(JobBase job, IEnumerable<dodSON.Core.FileStorage.ICompareResult> report)
        {
            gridContentHolder.Children.Clear();
            System.Threading.Tasks.Task.Run(() =>
            {
                var stats = Helper.AnalyzeReport(job, report, DateTime.Now);
                IEnumerable<object> okData = null;
                IEnumerable<object> newData = null;
                IEnumerable<object> updateData = null;
                IEnumerable<object> removeData = null;
                Helper.ExtractLists(job, report, out okData, out newData, out updateData, out removeData);
                // display user control
                Dispatcher.Invoke(() => { gridContentHolder.Children.Add(new ReportViewerUserControl2(true, Helper.GetDestinationDirectoryName(job), stats, okData, newData, updateData, removeData, _Settings)); });
            });





            //if (report == null)
            //{
            //    gridContentHolder.Children.Clear();
            //}
            //else
            //{
            //    var stats = Helper.AnalyzeReport(job, report, DateTime.Now);
            //    IEnumerable<object> okData = null;
            //    IEnumerable<object> newData = null;
            //    IEnumerable<object> updateData = null;
            //    IEnumerable<object> removeData = null;
            //    Helper.ExtractLists(job, report, out okData, out newData, out updateData, out removeData);
            //    // display user control

            //    gridContentHolder.Children.Add(new ReportViewerUserControl2(true, Helper.GetDestinationDirectoryName(job), stats, okData, newData, updateData, removeData, _Settings));
            //}
        }
        #endregion
    }
}
