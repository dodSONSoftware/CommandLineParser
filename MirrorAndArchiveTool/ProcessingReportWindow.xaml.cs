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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ProcessingReportWindow : Window
    {
        public ProcessingReportWindow(JobBase job,
                                      string filename,
                                      System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            InitializeComponent();
            Title = Helper.FormatTitle("Reading Report");
            _Job = job;
            _Filename = filename;
            _CancelTokenSource = cancellationTokenSource;
        }
        // ----
        private JobBase _Job = null;
        // ----
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Threading.Tasks.Task.Run(
                () =>
                {
                    var lines = ReadContentLines(_Filename, _LocalCancelTokenSource);
                    if ((!_CancelTokenSource.IsCancellationRequested) && (!_LocalCancelTokenSource.IsCancellationRequested))
                    {
                        Helper.ProcessLines(_Job, lines, _CancelTokenSource, out okData, out newData, out updateData, out removeData, out stats);
                        if ((!_CancelTokenSource.IsCancellationRequested) && (!_LocalCancelTokenSource.IsCancellationRequested))
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                DialogResult = true;
                                this.Close();
                            }));
                        }
                        else
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.Close();
                            }));
                        }
                    }
                }, _LocalCancelTokenSource.Token);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _LocalCancelTokenSource.Cancel();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _LocalCancelTokenSource.Cancel();
            DialogResult = false;
            this.Close();
        }
        // ----
        private string _Filename = "";
        private System.Threading.CancellationTokenSource _LocalCancelTokenSource = new System.Threading.CancellationTokenSource();
        private System.Threading.CancellationTokenSource _CancelTokenSource = null;
        private IEnumerable<object> okData = null;
        private IEnumerable<object> newData = null;
        private IEnumerable<object> updateData = null;
        private IEnumerable<object> removeData = null;
        private Helper.ReportFileStatistics stats = null;
        // ----
        public IEnumerable<object> OkData
        {
            get { return okData; }
            set { okData = value; }
        }
        public IEnumerable<object> NewData
        {
            get { return newData; }
            set { newData = value; }
        }
        public IEnumerable<object> UpdateData
        {
            get { return updateData; }
            set { updateData = value; }
        }
        public IEnumerable<object> RemoveData
        {
            get { return removeData; }
            set { removeData = value; }
        }
        public Helper.ReportFileStatistics Stats
        {
            get { return stats; }
            set { stats = value; }
        }
        // ----
        private IEnumerable<string> ReadContentLines(string filename, System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            var result = new List<string>();
            using (var sr = new System.IO.StreamReader(filename))
            {
                if ((_CancelTokenSource.IsCancellationRequested) || (cancellationTokenSource.IsCancellationRequested)) { return null; }
                // ----
                var line = "";
                while ((line = sr.ReadLine()) != null) { result.Add(line); }
                sr.Close();
            }
            return result;
        }

    }
}
