using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool
{
    [Serializable()]
    public class ArchiveJob
        : JobBase
    {
        #region Ctor
        private ArchiveJob()
            : base()
        { }
        public ArchiveJob(string name, string archiveRootPath, IEnumerable<string> sourcePaths)
            : base(name)
        {
            //if (string.IsNullOrWhiteSpace(archiveRootPath)) { throw new ArgumentNullException("archiveRootPath"); }
            //if (sourcePaths == null) { throw new ArgumentNullException("sourcePaths"); }
            _ArchiveRootPath = archiveRootPath;
            if (sourcePaths != null) { _SourcePaths = new System.Collections.ObjectModel.ObservableCollection<string>(sourcePaths); }
        }
        #endregion
        #region Private Fields
        private string _ArchiveRootPath = "";
        private System.Collections.ObjectModel.ObservableCollection<string> _SourcePaths = new System.Collections.ObjectModel.ObservableCollection<string>();
        [field: NonSerialized()]
        private System.Threading.CancellationTokenSource _CancelTokenSource = new System.Threading.CancellationTokenSource();
        #endregion
        #region Public Methods
        public string ArchiveRootPath
        {
            get { return _ArchiveRootPath; }
            set
            {
                if (_ArchiveRootPath != value)
                {
                    _ArchiveRootPath = value;
                    RaisePropertyChangedEvent("ArchiveRootPath");
                }
            }
        }
        public System.Collections.ObjectModel.ObservableCollection<string> SourcePaths
        {
            get { return _SourcePaths; }
        }
        public void AddSourcePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new ArgumentNullException("path"); }
            if (!_SourcePaths.Contains(path))
            {
                _SourcePaths.Add(path);
                //            RaisePropertyChangedEvent("SourcePaths");
            }
        }
        public void RemoveSourcePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { throw new ArgumentNullException("path"); }
            if (_SourcePaths.Contains(path))
            {
                _SourcePaths.Remove(path);
                //            RaisePropertyChangedEvent("SourcePaths");
            }
        }
        public void ClearAllSourcePaths()
        {
            _SourcePaths.Clear();
            //            RaisePropertyChangedEvent("SourcePaths");
        }
        #endregion
    }
}
