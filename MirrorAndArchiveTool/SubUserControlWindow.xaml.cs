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
    /// Interaction logic for SubUserControlWindow.xaml
    /// </summary>
    public partial class SubUserControlWindow : Window
    {
        public SubUserControlWindow(ISubUserControl uc)
        {
            InitializeComponent();
            Title = Helper.FormatTitle(uc.Title);
            gridContentControl.Children.Add(uc as UIElement);
            App.WriteDebugLog($"SubControl_{uc.Key}", $"Opening {uc.Title} Editor Window");
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
