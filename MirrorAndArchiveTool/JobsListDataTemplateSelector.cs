using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MirrorAndArchiveTool
{
    public class JobsListDataTemplateSelector
        : DataTemplateSelector
    {
        public DataTemplate ArchiveTemplate { get; set; }
        public DataTemplate MirrorTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item is MirrorJob) { return MirrorTemplate; }
            else if (item is ArchiveJob) { return ArchiveTemplate; }
            return null;
        }
    }
}
