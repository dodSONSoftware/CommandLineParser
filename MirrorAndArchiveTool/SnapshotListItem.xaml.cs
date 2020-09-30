using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for SnapshotListItem.xaml
    /// </summary>
    public partial class SnapshotListItem : UserControl
    {
        #region Ctor
        public SnapshotListItem()
        {
            InitializeComponent();
            DataContext = this;
        }
        public SnapshotListItem(Helper.SnapshotSession session)
            : this()
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            DataContext = Session;
        }
        #endregion
        #region Public Properties
        public Helper.SnapshotSession Session { get; private set; }
        public void Expand()
        {
            TheExpander.IsExpanded = true;
        }
        public void Collapse()
        {
            TheExpander.IsExpanded = false;
        }
        #endregion
        #region UserControl Events
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var dude = sender as System.Windows.Controls.ContextMenu;
            var dudeHold = dude.DataContext;
            dude.DataContext = null;
            dude.DataContext = dudeHold;
        }
        private void DataGridData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) { Session.CurrentlySelected = e.AddedItems[0] as Helper.SnapshotFileInfo; ; }
        }
        #endregion
    }
}
