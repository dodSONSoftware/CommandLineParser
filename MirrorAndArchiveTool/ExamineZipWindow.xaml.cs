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
    /// Interaction logic for ExamineZipWindow.xaml
    /// </summary>
    public partial class ExamineZipWindow : Window
    {
        #region Ctor
        public ExamineZipWindow()
        {
            InitializeComponent();
            DataContext = this;
            ChartGrid.Visibility = System.Windows.Visibility.Hidden;
            ZipFileStatisticsPanel.Visibility = System.Windows.Visibility.Hidden;
            Title = Helper.FormatTitle("Examine Zip File");
            App.WriteDebugLog(nameof(ExamineZipWindow), $"Opening Examine Zip File Window");
        }
        public ExamineZipWindow(Action<string> onAddToSettings)
            : this()
        {
            _OnAddToSettings = onAddToSettings ?? throw new ArgumentNullException("onAddToSettings");
        }
        #endregion
        #region Private Fields
        private Action<string> _OnAddToSettings = null;
        private IEnumerable<ExtensionData> _CurrentExaminationResults = null;
        #endregion
        #region Dependency Properties
        public string ZipFilename
        {
            get { return (string)GetValue(ZipFilenameProperty); }
            set { SetValue(ZipFilenameProperty, value); }
        }
        public static readonly DependencyProperty ZipFilenameProperty =
            DependencyProperty.Register("ZipFilename", typeof(string), typeof(ExamineZipWindow), new UIPropertyMetadata(""));
        // ----
        public double CompressionRatioSliderValue
        {
            get { return (double)GetValue(CompressionRatioSliderValueProperty); }
            set { SetValue(CompressionRatioSliderValueProperty, value); }
        }
        public static readonly DependencyProperty CompressionRatioSliderValueProperty =
            DependencyProperty.Register("CompressionRatioSliderValue", typeof(double), typeof(ExamineZipWindow), new UIPropertyMetadata(0.35));
        // ----
        public string ExtensionsToStore
        {
            get { return (string)GetValue(ExtensionsToStoreProperty); }
            set { SetValue(ExtensionsToStoreProperty, value); }
        }
        public static readonly DependencyProperty ExtensionsToStoreProperty =
            DependencyProperty.Register("ExtensionsToStore", typeof(string), typeof(ExamineZipWindow), new UIPropertyMetadata(""));
        // ----
        public System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>> ProgressRatios
        {
            get { return (System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>)GetValue(ProgressRatiosProperty); }
            set { SetValue(ProgressRatiosProperty, value); }
        }
        public static readonly DependencyProperty ProgressRatiosProperty =
            DependencyProperty.Register("ProgressRatios", typeof(System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>), typeof(ExamineZipWindow), new UIPropertyMetadata(null));
        public System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>> CompressedSizes
        {
            get { return (System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>)GetValue(CompressedSizesProperty); }
            set { SetValue(CompressedSizesProperty, value); }
        }
        public static readonly DependencyProperty CompressedSizesProperty = DependencyProperty.Register("CompressedSizes", typeof(System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>), typeof(ExamineZipWindow), new PropertyMetadata(null));
        public System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>> UncompressedSizes
        {
            get { return (System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>)GetValue(UncompressedSizesProperty); }
            set { SetValue(UncompressedSizesProperty, value); }
        }
        public static readonly DependencyProperty UncompressedSizesProperty = DependencyProperty.Register("UncompressedSizes", typeof(System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>), typeof(ExamineZipWindow), new PropertyMetadata(null));
        public System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, long>> Count
        {
            get { return (System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, long>>)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, long>>), typeof(ExamineZipWindow), new PropertyMetadata(null));
        // ----
        public string CompressionRationSliderValueTextBox
        {
            get { return (string)GetValue(CompressionRationSliderValueTextBoxProperty); }
            set { SetValue(CompressionRationSliderValueTextBoxProperty, value); }
        }
        public static readonly DependencyProperty CompressionRationSliderValueTextBoxProperty =
            DependencyProperty.Register("CompressionRationSliderValueTextBox", typeof(string), typeof(ExamineZipWindow), new UIPropertyMetadata(""));
        // ----
        public Visibility TextBoxOpenZipFileStatementVisibility
        {
            get { return (Visibility)GetValue(TextBoxOpenZipFileStatementVisibilityProperty); }
            set { SetValue(TextBoxOpenZipFileStatementVisibilityProperty, value); }
        }
        public static readonly DependencyProperty TextBoxOpenZipFileStatementVisibilityProperty =
            DependencyProperty.Register("TextBoxOpenZipFileStatementVisibility", typeof(Visibility), typeof(ExamineZipWindow), new UIPropertyMetadata(Visibility.Visible));
        // ---- 
        public string OpenReportFileStatement
        {
            get { return (string)GetValue(OpenReportFileStatementProperty); }
            set { SetValue(OpenReportFileStatementProperty, value); }
        }
        public static readonly DependencyProperty OpenReportFileStatementProperty =
            DependencyProperty.Register("OpenReportFileStatement", typeof(string), typeof(ExamineZipWindow), new UIPropertyMetadata("Open zip file to view it's file extension statistics"));
        // ----
        public Visibility ImageWorkingIconVisiblity
        {
            get { return (Visibility)GetValue(ImageWorkingIconVisiblityProperty); }
            set { SetValue(ImageWorkingIconVisiblityProperty, value); }
        }
        public static readonly DependencyProperty ImageWorkingIconVisiblityProperty =
            DependencyProperty.Register("ImageWorkingIconVisiblity", typeof(Visibility), typeof(ExamineZipWindow), new UIPropertyMetadata(Visibility.Hidden));
        // ----
        public string ButtonExplainationText
        {
            get { return (string)GetValue(ButtonExplainationTextProperty); }
            set { SetValue(ButtonExplainationTextProperty, value); }
        }
        public static readonly DependencyProperty ButtonExplainationTextProperty = DependencyProperty.Register("ButtonExplainationText", typeof(string), typeof(ExamineZipWindow), new PropertyMetadata("Displays the space used, in kilobytes, by file extension"));
        #endregion
        #region Window Events
        private void buttonOpenZipFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog()
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "*.zip",
                DereferenceLinks = true,
                Title = "Open",
                Filter = "Zip Files|*.zip|All Files|*.*",
                FilterIndex = 0
            };
            if (dialog.ShowDialog(this) ?? false)
            {
                // Open Button Statement
                ZipFilename = "";
                OpenReportFileStatement = "Loading...";
                // show working icon
                ImageWorkingIconVisiblity = System.Windows.Visibility.Visible;
                // hide/clear UI elements
                ZipFileStatisticsPanel.Visibility = System.Windows.Visibility.Hidden;
                ChartGrid.Visibility = System.Windows.Visibility.Hidden;
                ProgressRatios = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>();
                CompressedSizes = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>();
                UncompressedSizes = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>();
                ExtensionsToStore = "";
                // start thread
                System.Threading.Tasks.Task.Run(
                    () =>
                    {
                        // examine zip file
                        ZipFileStatistics stats = null;
                        _CurrentExaminationResults = ExamineZipFile(dialog.FileName, out stats);
                        if (_CurrentExaminationResults != null)
                        {
                            // update UI
                            this.Dispatcher.Invoke(
                                new Action(() =>
                                {
                                    // Open Button Statement
                                    ZipFilename = dialog.FileName;
                                    OpenReportFileStatement = "";
                                    // hide working icon
                                    ImageWorkingIconVisiblity = System.Windows.Visibility.Hidden;
                                    // show/update UI elements
                                    ZipFileStatisticsPanel.Visibility = System.Windows.Visibility.Visible;
                                    ChartGrid.Visibility = System.Windows.Visibility.Visible;
                                    textBlockZipFileStatsTotalExtensions.Text = stats.TotalExtensions.ToString("N0");
                                    textBlockZipFileStatsTotalFiles.Text = stats.TotalFiles.ToString("N0");
                                    textBlockZipFileStatsCompressedSize.Text = stats.CompressedSizeString + "   " + stats.CompressedSize.ToString("N0");
                                    textBlockZipFileStatsUncompressedSize.Text = stats.UncompressedSizeString + "   " + stats.UncompressedSize.ToString("N0");
                                    textBlockZipFileStatsCompressionPercentage.Text = stats.CompressedPercentageString;
                                    DisplayResults(_CurrentExaminationResults);
                                    IntializeBarChart();
                                }));
                        }
                        else
                        {
                            this.Dispatcher.Invoke(
                                   new Action(() =>
                                   {
                                       OpenReportFileStatement = "Open zip file to view it's file extension statistics";
                                       // hide working icon
                                       ImageWorkingIconVisiblity = System.Windows.Visibility.Hidden;
                                   }));
                        }
                    });
            }
        }
        private void buttonAddToSettings(object sender, RoutedEventArgs e)
        {
            _OnAddToSettings(ExtensionsToStore);
        }
        private void sliderCompressionRatio_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DisplayResults(_CurrentExaminationResults);
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
        #region Commands
        private DelegateCommand _ShowKilobytesCommand;
        public ICommand ShowKilobytes
        {
            get
            {
                // create about window command the first time is it requested
                if (_ShowKilobytesCommand == null)
                {
                    _ShowKilobytesCommand = new DelegateCommand(x =>
                    {
                        // #### execute 
                        // set buttons
                        SetAllToggleButtonsState(false);
                        toggleButtonKilobytes.IsChecked = true;
                        Chart_Count.Visibility = Visibility.Hidden;
                        Chart_Kilobytes.Visibility = Visibility.Visible;
                        ButtonExplainationText = "Displays the space used, in kilobytes, by file extension";
                    });
                }
                // 
                return _ShowKilobytesCommand;
            }
        }
        private ICommand _ShowCountCommand;
        public ICommand ShowCount
        {
            get
            {
                // create about window command the first time is it requested
                if (_ShowCountCommand == null)
                {
                    _ShowCountCommand = new DelegateCommand(x =>
                    {
                        // #### execute 
                        // set buttons
                        SetAllToggleButtonsState(false);
                        toggleButtonCount.IsChecked = true;
                        Chart_Count.Visibility = Visibility.Visible;
                        Chart_Kilobytes.Visibility = Visibility.Hidden;
                        ButtonExplainationText = "Displays the number of files by file extension";
                    });
                }
                // 
                return _ShowCountCommand;
            }
        }
        private void SetAllToggleButtonsState(bool value)
        {
            foreach (var item in ToggleButtonContainer.Children)
            {
                if (item is System.Windows.Controls.Primitives.ToggleButton)
                {
                    (item as System.Windows.Controls.Primitives.ToggleButton).IsChecked = value;
                }
            }
        }
        #endregion
        #region Private Methods
        private IEnumerable<ExtensionData> ExamineZipFile(string zipFilename,
                                                          out ZipFileStatistics stats)
        {
            dodSON.Core.FileStorage.ICompressedFileStore store = null;
            var result = new SortedList<string, ExtensionData>();
            try
            {
                // open as ICompressedFileStore
                store = Helper.GetZipStore(zipFilename, System.IO.Path.GetTempPath(), false, null);
            }
            catch (Exception)
            {
                this.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        MessageBox.Show(Window.GetWindow(this),
                                        "Not a Zip File.",
                                         "Invalid File Format",
                                         MessageBoxButton.OK,
                                         MessageBoxImage.Error);
                    }));
                stats = null;
                //bSizes = null;
                return null;
            }
            // process all items
            //bSizes = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, long>>();
            foreach (dodSON.Core.FileStorage.ICompressedFileStoreItem item in store.GetAll())
            {
                var extension = System.IO.Path.GetExtension(item.RootFilename).ToLower();
                if (extension.StartsWith(".")) { extension = extension.Substring(1); }
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    if (result.ContainsKey(extension))
                    {
                        // update data
                        result[extension].Count++;
                        result[extension].CompressedSize += item.CompressedFileSize;
                        result[extension].UncompressedSize += item.FileSize;
                    }
                    else
                    {
                        // add data
                        var data = new ExtensionData()
                        {
                            Count = 1,
                            Extension = extension,
                            CompressionStrategy = item.CompressionStrategy,
                            CompressedSize = item.CompressedFileSize,
                            UncompressedSize = item.FileSize
                        };
                        result.Add(data.Extension, data);
                    }
                }
            }
            // update zip file stats
            stats = new ZipFileStatistics()
            {
                TotalExtensions = result.Count,
                TotalFiles = store.Count,
                CompressedSize = store.CompressedSize,
                UncompressedSize = store.UncompressedSize
            };
            // ----
            return result.Values;
        }
        private void IntializeBarChart()
        {
            // build  (invert ratio to get compression %)
            var bars = new List<KeyValuePair<string, int>>();
            var count = new List<KeyValuePair<string, long>>();
            var graph = new List<KeyValuePair<string, decimal>>();
            var graph2 = new List<KeyValuePair<string, decimal>>();
            foreach (var item in _CurrentExaminationResults)
            {
                var value = (int)Math.Round(((1.0 - item.CompressionRatio) * (double)100), 0);
                bars.Add(new KeyValuePair<string, int>(item.Extension, value));
                //
                var byte1 = dodSON.Core.Common.ByteCountHelper.ToKilobytes(item.CompressedSize);
                graph.Add(new KeyValuePair<string, decimal>(item.Extension, byte1));
                //
                var byte2 = dodSON.Core.Common.ByteCountHelper.ToKilobytes(item.UncompressedSize);
                graph2.Add(new KeyValuePair<string, decimal>(item.Extension, byte2));
                //
                var count1 = item.Count;
                count.Add(new KeyValuePair<string, long>(item.Extension, count1));
            }
            ProgressRatios = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>(bars);
            CompressedSizes = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>(graph);
            UncompressedSizes = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>(graph2);
            Count = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, long>>(count);



            //// build  (invert values)
            //var bars = new List<KeyValuePair<string, int>>();
            //var graph = new List<KeyValuePair<string, decimal>>();
            //var graph2 = new List<KeyValuePair<string, decimal>>();
            //foreach (var item in _CurrentExaminationResults)
            //{
            //    var value = (int)Math.Round(((1.0 - item.CompressionRatio) * (double)100), 0);
            //    bars.Add(new KeyValuePair<string, int>(item.Extension, value));
            //    //
            //    var byte1 = (long)dodSON.Core.Common.ByteCountHelper.ToKilobytes(item.CompressedSize);
            //    graph.Add(new KeyValuePair<string, decimal>(item.Extension, byte1));
            //    var byte2 = (long)dodSON.Core.Common.ByteCountHelper.ToKilobytes(item.UncompressedSize);
            //    graph2.Add(new KeyValuePair<string, decimal>(item.Extension, byte2));
            //}
            //ProgressRatios = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>(bars);
            //CompressedSizes = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>(graph);
            //UncompressedSizes = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, decimal>>(graph2);
        }
        private void DisplayResults(IEnumerable<ExtensionData> list)
        {
            if (_CurrentExaminationResults == null)
            {
                ExtensionsToStore = "";
            }
            else
            {
                // build comma-separated list
                var sortedList = new SortedList<string, ExtensionData>();
                foreach (var item in list)
                {
                    if ((1 - item.CompressionRatio) <= CompressionRatioSliderValue)
                    {
                        if (!sortedList.ContainsKey(item.Extension))
                        {
                            sortedList.Add(item.Extension, item);
                        }
                    }
                }
                var value = "";
                if (sortedList.Count > 0)
                {
                    var builder = new System.Text.StringBuilder(1024);
                    foreach (var item in sortedList.Values) { builder.Append(item.Extension + ","); }
                    builder.Length--;
                    value = builder.ToString();
                }
                // update UI
                ExtensionsToStore = value;
                CompressionRationSliderValueTextBox = string.Format("{0}%", CompressionRatioSliderValue * 100);
            }
        }
        #endregion
        #region Private Classes: ExtensionData, ZipFileStatistics
        private class ExtensionData
        {
            public long Count { get; set; }
            public string Extension { get; set; }
            public dodSON.Core.FileStorage.CompressionStorageStrategy CompressionStrategy { get; set; }
            public long UncompressedSize { get; set; }
            public long CompressedSize { get; set; }
            public double CompressionRatio
            {
                get { return (UncompressedSize != 0) ? ((double)CompressedSize / (double)UncompressedSize) : 0.0; }
            }
        }
        // ----
        public class ZipFileStatistics
        {
            public long CompressedSize { get; set; }
            public string CompressedSizeString
            {
                get { return string.Format("{0}", dodSON.Core.Common.ByteCountHelper.ToString(CompressedSize)); }
            }
            public long UncompressedSize { get; set; }
            public string UncompressedSizeString
            {
                get { return string.Format("{0}", dodSON.Core.Common.ByteCountHelper.ToString(UncompressedSize)); }
            }
            public long TotalExtensions { get; set; }
            public int TotalFiles { get; set; }
            public double CompressionRatio
            {
                get { return (UncompressedSize != 0) ? ((double)CompressedSize / (double)UncompressedSize) : 0.0; }
            }
            public string CompressedPercentageString
            {
                get { return string.Format("{0:N0}%", Math.Round(((1.0 - CompressionRatio) * (double)100), 0)); }
            }
        }
        #endregion
    }
}
