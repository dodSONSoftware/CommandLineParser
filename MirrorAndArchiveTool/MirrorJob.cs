using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool
{
    [Serializable()]
    public class MirrorJob
        : JobBase
    {
        #region Ctor
        private MirrorJob()
            : base()
        { }
        public MirrorJob(string name, string sourcePath, string mirrorPath)
            : base(name)
        {
            _SourcePath = sourcePath;
            _MirrorPath = mirrorPath;
        }
        #endregion
        #region Private Fields
        private string _SourcePath = "";
        private string _MirrorPath = "";
        [field: NonSerialized()]
        private System.Threading.CancellationTokenSource _CancelTokenSource = new System.Threading.CancellationTokenSource();
        #endregion
        #region Public Methods
        public string SourcePath
        {
            get { return _SourcePath; }
            set
            {
                if (_SourcePath != value)
                {
                    _SourcePath = value;
                    RaisePropertyChangedEvent("SourcePath");
                }
            }
        }
        public string MirrorPath
        {
            get { return _MirrorPath; }
            set
            {
                if (_MirrorPath != value)
                {
                    _MirrorPath = value;
                    RaisePropertyChangedEvent("MirrorPath");
                }
            }
        }
        #endregion
    }
}
