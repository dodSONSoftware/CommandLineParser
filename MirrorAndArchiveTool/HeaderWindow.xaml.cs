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
    /// Interaction logic for HeaderWindow.xaml
    /// </summary>
    public partial class HeaderWindow : Window
    {
        #region Ctor
        public HeaderWindow()
        {
            InitializeComponent();
            DataContext = this;
            // ----
            Title = Helper.FormatTitle("Report Header");
        }
        public HeaderWindow(HeaderSubControl2 ctrl)
            : this()
        {
            if (ctrl == null) { throw new ArgumentNullException(nameof(ctrl)); }
            gridContentHolder.Children.Add(ctrl);
            if (!ctrl.IsArchiveJob)
            {
                Height = 330;
            }
        }
        #endregion
        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
