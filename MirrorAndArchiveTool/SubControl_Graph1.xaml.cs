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
    /// Interaction logic for SubControl_Graph1.xaml
    /// </summary>
    public partial class SubControl_Graph1
        : UserControl,
          ISubUserControl
    {
        #region Ctor
        public SubControl_Graph1()
        {
            InitializeComponent();
            DataContext = this;
        }
        public SubControl_Graph1(JobBase job,
                                 IEnumerable<dodSON.Core.FileStorage.ICompareResult> report,
                                 GlobalSettings settings)
            : this()
        {
            _Job = job ?? throw new ArgumentNullException(nameof(job));
            _Report = report ?? throw new ArgumentNullException(nameof(report));
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            // ----
            UpdateUI();
            App.WriteDebugLog(nameof(SubControl_Graph1), $"Viewing Sub Control: GRAPH, Job={_Job.Name}");
        }
        #endregion
        #region Private Fields
        private JobBase _Job = null;
        private IEnumerable<dodSON.Core.FileStorage.ICompareResult> _Report = null;
        private GlobalSettings _Settings = null;
        private int _Total = -1;
        private int _TotalChanges = -1;
        private int _OkCount = -1;
        private int _NewCount = -1;
        private int _UpdateCount = -1;
        private int _RemoveCount = -1;
        #endregion
        #region Dependency Properties
        public System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>> FileChangesPercentages
        {
            get { return (System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>)GetValue(FileChangesPercentagesProperty); }
            set { SetValue(FileChangesPercentagesProperty, value); }
        }
        public static readonly DependencyProperty FileChangesPercentagesProperty =
            DependencyProperty.Register("FileChangesPercentages",
                                        typeof(System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>),
                                        typeof(SubControl_Graph1));
        // ----        
        #endregion
        #region ISubUserControl Methods
        public string Title
        {
            get { return "Action Status"; }
        }
        public string Key
        {
            get { return "GRAPH1"; }
        }
        public void Refresh(IEnumerable<dodSON.Core.FileStorage.ICompareResult> report)
        {
            _Report = report;
            UpdateUI();
        }
        public void Shutdown()
        {
        }
        #endregion
        #region UserControl Events
        private void dataGridFilesDetails_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }
        #endregion
        #region Private Methods
        private void UpdateUI()
        {
            dataGridFilesDetails.ItemsSource = null;
            dataGridFilesDetails.Visibility = System.Windows.Visibility.Hidden;
            imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Visible;
            chart.Visibility = System.Windows.Visibility.Hidden;
            // 
            if (_Report != null)
            {
                _OkCount = (from x in _Report
                            where (x.Action == dodSON.Core.FileStorage.CompareAction.Ok)
                            select x).Count();
                _NewCount = (from x in _Report
                             where (x.Action == dodSON.Core.FileStorage.CompareAction.New)
                             select x).Count();
                _UpdateCount = (from x in _Report
                                where (x.Action == dodSON.Core.FileStorage.CompareAction.Update)
                                select x).Count();
                _RemoveCount = (from x in _Report
                                where (x.Action == dodSON.Core.FileStorage.CompareAction.Remove)
                                select x).Count();
                _Total = _OkCount + _UpdateCount + _RemoveCount;
                _TotalChanges = _NewCount + _UpdateCount + _RemoveCount;
                // update UI
                System.Collections.ObjectModel.ObservableCollection<Helper.FilesDetailsRow> filesDetails = new System.Collections.ObjectModel.ObservableCollection<Helper.FilesDetailsRow>();
                System.Collections.ObjectModel.Collection<ResourceDictionary> piePalette = new System.Collections.ObjectModel.Collection<ResourceDictionary>();
                var slices = new List<KeyValuePair<string, int>>();
                if (_Total > 0)
                {
                    if (_OkCount > 0)
                    {
                        slices.Add(new KeyValuePair<string, int>("Ok", (int)(_OkCount)));
                        piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.SteelBlue));
                        filesDetails.Add(new Helper.FilesDetailsRow() { Action = "Ok", Files = _OkCount.ToString("N0"), Percentage = ((double)((double)_OkCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                    }
                    if (_NewCount > 0)
                    {
                        slices.Add(new KeyValuePair<string, int>("New", (int)(_NewCount)));
                        piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.ForestGreen));
                        filesDetails.Add(new Helper.FilesDetailsRow() { Action = "New", Files = _NewCount.ToString("N0"), Percentage = ((double)((double)_NewCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                    }
                    if (_UpdateCount > 0)
                    {
                        slices.Add(new KeyValuePair<string, int>("Update", (int)(_UpdateCount)));
                        piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.Gold));
                        filesDetails.Add(new Helper.FilesDetailsRow() { Action = "Update", Files = _UpdateCount.ToString("N0"), Percentage = ((double)((double)_UpdateCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                    }
                    if (_RemoveCount > 0)
                    {
                        slices.Add(new KeyValuePair<string, int>("Remove", (int)(_RemoveCount)));
                        piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.Crimson));
                        filesDetails.Add(new Helper.FilesDetailsRow() { Action = "Remove", Files = _RemoveCount.ToString("N0"), Percentage = ((double)((double)_RemoveCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                    }
                }
                else
                {
                    slices.Add(new KeyValuePair<string, int>("New", (int)(100)));
                    piePalette.Add(CreateResourceDictionaryForPieSliceColor(Colors.ForestGreen));
                    filesDetails.Add(new Helper.FilesDetailsRow() { Action = "New", Files = _NewCount.ToString("N0"), Percentage = ((double)((double)_NewCount / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "%" });
                }
                // update UI
                filesDetails.Add(new Helper.FilesDetailsRow() { Action = string.Format("{0} Changes", ((_Job is MirrorJob) ? "Mirror" : "Archive")), Files = _TotalChanges.ToString("N0"), Percentage = (((double)_TotalChanges / ((double)_Total + (double)_NewCount)) * 100.0).ToString("N2") + "% changed" });
                chart.Palette = piePalette;
                FileChangesPercentages = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, int>>(slices);
                dataGridFilesDetails.ItemsSource = filesDetails;
                dataGridFilesDetails.Visibility = System.Windows.Visibility.Visible;
                imageRecommendationWaitIcon.Visibility = System.Windows.Visibility.Hidden;
                chart.Visibility = System.Windows.Visibility.Visible;
            }
        }
        private ResourceDictionary CreateResourceDictionaryForPieSliceColor(Color color)
        {
            var resourceDict = new ResourceDictionary();
            var style = new Style(typeof(Control));
            var brush = new SolidColorBrush(color);
            style.Setters.Add(new Setter() { Property = Control.BackgroundProperty, Value = brush });
            resourceDict.Add("DataPointStyle", style);
            return resourceDict;
        }
        #endregion
    }
}
