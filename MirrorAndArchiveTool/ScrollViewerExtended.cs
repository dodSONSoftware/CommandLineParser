using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool
{
    public class ScrollViewerExtended : System.Windows.Controls.ScrollViewer
    {
        private System.Windows.Controls.Primitives.ScrollBar verticalScrollbar;

        public override void OnApplyTemplate()
        {
            // Call base class
            base.OnApplyTemplate();

            // Obtain the vertical scrollbar
            this.verticalScrollbar = this.GetTemplateChild("PART_VerticalScrollBar") as System.Windows.Controls.Primitives.ScrollBar;
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            // Only handle this message if the vertical scrollbar is in use
            if ((this.verticalScrollbar != null) && 
                (this.verticalScrollbar.Visibility ==System.Windows.Visibility.Visible) && 
                this.verticalScrollbar.IsEnabled)
            {
                // Perform default handling
                base.OnMouseWheel(e);
            }
        }
    }
}
