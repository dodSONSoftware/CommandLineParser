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
    /// Interaction logic for ReportSingleDataGridControl.xaml
    /// </summary>
    public partial class ReportSingleDataGridControl : UserControl
    {
        #region Ctor
        public ReportSingleDataGridControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        public ReportSingleDataGridControl(string name, System.Collections.IEnumerable data)
            : this()
        {
            Title = name;
            this.dataGridFilesDetails.ItemsSource = data;
        }
        #endregion
        #region Dependency Properites
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ReportSingleDataGridControl), new UIPropertyMetadata(""));
        #endregion
    }
}
