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
    public partial class AboutWindow : Window
    {
        #region Ctor
        public AboutWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        public AboutWindow(bool isSplashScreen)
            : this()
        {
            _IsSplashScreen = isSplashScreen;
            if (isSplashScreen)
            {
                System.Threading.Tasks.Task.Run(() =>
                    {
                        dodSON.Core.Threading.ThreadingHelper.Sleep(TimeSpan.FromMilliseconds(1234));
                        this.Dispatcher.BeginInvoke(new Action(() => { this.Close(); }));
                    });
            }
        }
        #endregion
        #region Private Fields
        private bool _IsSplashScreen = false;
        #endregion
        #region Window Events
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            CheckKeyboardEvent(e);
        }
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckKeyboardEvent(e);
        }
        #endregion
        #region Private Methods
        private void CheckKeyboardEvent(InputEventArgs e)
        {
            if (!_IsSplashScreen)
            {
                e.Handled = true;
                Close();
            }
        }
        #endregion

    }
}
