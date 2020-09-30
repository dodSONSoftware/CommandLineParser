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
    /// Interaction logic for SubControl_Editor.xaml
    /// </summary>
    public partial class SubControl_Editor 
        : UserControl,
          ISubUserControl
    {
        #region Ctor
        public SubControl_Editor()
        {
            InitializeComponent();
            DataContext = this;
        }

        #endregion
        #region Private Fields
        
        #endregion
        #region ISubUserControl Methods
        public string Title
        {
            get { return "Editor"; }
        }
        public string Key
        {
            get { return "EDITOR"; }
        }
        public void Shutdown()
        {

        }
        public void Refresh(IEnumerable<dodSON.Core.FileStorage.ICompareResult> report)
        {

        }
        #endregion
        #region Dependency Properties

        #endregion
        #region UserControl Events

        #endregion
    }
}
