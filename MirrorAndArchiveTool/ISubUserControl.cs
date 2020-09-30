using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool
{
    public interface ISubUserControl
    {
        string Title { get; }
        string Key { get; }
        void Refresh(IEnumerable<dodSON.Core.FileStorage.ICompareResult> report);
        void Shutdown();
    }
}
