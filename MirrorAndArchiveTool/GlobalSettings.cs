using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool
{
    [Serializable()]
    public class GlobalSettings
        : System.ComponentModel.INotifyPropertyChanged
    {
        #region System.ComponentModel.INotifyPropertyChanged Methods
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized()]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Will raise the property changed events with the provided property name.
        /// </summary>
        /// <param name="propertyName">The name of the property which has changed.</param>
        protected void RaisePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName)); }
        }
        #endregion
        #region Ctor
        public GlobalSettings()
        { }
        #endregion
        #region Private Fields
        // ---- removed files archive
        private bool _EnableRemovedFilesArchive = false;
        private string _RemovedFilesArchiveRootPath = "";
        private bool _EnableAutoDeleteRemovedFilesArchiveByDate = false;
        private TimeSpan _RemovedFileArchiveAutoDeleteTimeLimit = TimeSpan.FromDays(30);
        private bool _EnableAutoDeleteRemovedFilesArchiveBySize = false;
        private long _RemovedFileArchiveAutoDeleteByteSizeLimit = dodSON.Core.Common.ByteCountHelper.Gigabyte;
        // ---- file extensions
        private bool _EnableFileExtensionsToStore = false;
        private string _FileExtensionsToStore = "";
        // ---- recommended action
        private TimeSpan _RecommendationActionAgeLimit = TimeSpan.FromHours(24);
        // ---- automatic reporting
        private bool _EnableAutomaticReporting = false;
        private string _AutomaticReportingRootPath = "";
        private bool _EnableAutomaticReportsDeletionByAge = true;
        private TimeSpan _RemovedAutomaticReportsTimeLimit = TimeSpan.FromDays(90);
        // ---- 
        private bool _ProcessingShutDownWhenComplete = false;
        private bool _JobRunUseCachedReportEnableCache = true;
        private TimeSpan _JobRunUseCachedReportTimeLimit = TimeSpan.FromMinutes(3);
        // ---- snap shots
        private bool _EnableArchiveZipFileSnapshots = false;
        private TimeSpan _ArchiveZipFileSnapshotsMaturityTimeLimit = TimeSpan.FromDays(7);
        private long _ArchiveZipFileSnapshotsMaximum = 5;
        private string _ArchiveZipFileSnapshotsRootPath = "";
        #endregion
        #region Public Methods
        // ---- removed files archive
        public bool EnableRemovedFilesArchive
        {
            get { return _EnableRemovedFilesArchive; }
            set
            {
                if (_EnableRemovedFilesArchive != value)
                {
                    _EnableRemovedFilesArchive = value;
                    RaisePropertyChangedEvent("EnableRemovedFilesArchive");
                }
            }
        }
        public string RemovedFilesArchiveRootPath
        {
            get { return _RemovedFilesArchiveRootPath; }
            set
            {
                if (_RemovedFilesArchiveRootPath != value)
                {
                    _RemovedFilesArchiveRootPath = value;
                    RaisePropertyChangedEvent("RemovedFilesArchiveRootPath");
                }
            }
        }
        public bool EnableAutoDeleteRemovedFilesArchiveByAge
        {
            get { return _EnableAutoDeleteRemovedFilesArchiveByDate; }
            set
            {
                if (_EnableAutoDeleteRemovedFilesArchiveByDate != value)
                {
                    _EnableAutoDeleteRemovedFilesArchiveByDate = value;
                    RaisePropertyChangedEvent("EnableAutoDeleteRemovedFilesArchiveByDate");
                }
            }
        }
        public TimeSpan RemovedFileArchiveAutoDeleteTimeLimit
        {
            get { return _RemovedFileArchiveAutoDeleteTimeLimit; }
            set
            {
                if (_RemovedFileArchiveAutoDeleteTimeLimit != value)
                {
                    _RemovedFileArchiveAutoDeleteTimeLimit = value;
                    RaisePropertyChangedEvent("RemovedFileArchiveAutoDeleteTimeLimit");
                }
            }
        }
        public bool EnableAutoDeleteRemovedFilesArchiveBySize
        {
            get { return _EnableAutoDeleteRemovedFilesArchiveBySize; }
            set
            {
                if (_EnableAutoDeleteRemovedFilesArchiveBySize != value)
                {
                    _EnableAutoDeleteRemovedFilesArchiveBySize = value;
                    RaisePropertyChangedEvent("EnableAutoDeleteRemovedFilesArchiveBySize");
                }
            }
        }
        public long RemovedFileArchiveAutoDeleteByteSizeLimit
        {
            get { return _RemovedFileArchiveAutoDeleteByteSizeLimit; }
            set
            {
                if (_RemovedFileArchiveAutoDeleteByteSizeLimit != value)
                {
                    _RemovedFileArchiveAutoDeleteByteSizeLimit = value;
                    RaisePropertyChangedEvent("RemovedFileArchiveAutoDeleteByteSizeLimit");
                }
            }
        }
        // ---- file extensions
        public string FileExtensionsToStore
        {
            get { return _FileExtensionsToStore; }
            set
            {
                if (_FileExtensionsToStore != value)
                {
                    _FileExtensionsToStore = value;
                    RaisePropertyChangedEvent("FileExtensionsToStore");
                }
            }
        }
        public bool EnableFileExtensionsToStore
        {
            get { return _EnableFileExtensionsToStore; }
            set
            {
                if (_EnableFileExtensionsToStore != value)
                {
                    _EnableFileExtensionsToStore = value;
                    RaisePropertyChangedEvent("EnableFileExtensionsToStore");
                }
            }
        }
        // ---- recommended action
        public TimeSpan RecommendationActionAgeLimit
        {
            get { return _RecommendationActionAgeLimit; }
            set
            {
                if (_RecommendationActionAgeLimit != value)
                {
                    _RecommendationActionAgeLimit = value;
                    RaisePropertyChangedEvent("RecommendationActionAgeLimit");
                }
            }
        }
        // ---- automatic reporting
        public bool EnableAutomaticReporting
        {
            get { return _EnableAutomaticReporting; }
            set
            {
                if (_EnableAutomaticReporting != value)
                {
                    _EnableAutomaticReporting = value;
                    RaisePropertyChangedEvent("EnableAutomaticReporting");
                }
            }
        }
        public string AutomaticReportingRootPath
        {
            get { return _AutomaticReportingRootPath; }
            set
            {
                if (_AutomaticReportingRootPath != value)
                {
                    _AutomaticReportingRootPath = value;
                    RaisePropertyChangedEvent("AutomaticReportingRootPath");
                }
            }
        }
        public bool EnableAutomaticReportsDeletionByAge
        {
            get { return _EnableAutomaticReportsDeletionByAge; }
            set
            {
                if (_EnableAutomaticReportsDeletionByAge != value)
                {
                    _EnableAutomaticReportsDeletionByAge = value;
                    RaisePropertyChangedEvent("EnableAutomaticReportsDeletionByAge");
                }
            }
        }
        public TimeSpan RemovedAutomaticReportsTimeLimit
        {
            get { return _RemovedAutomaticReportsTimeLimit; }
            set
            {
                if (_RemovedAutomaticReportsTimeLimit != value)
                {
                    _RemovedAutomaticReportsTimeLimit = value;
                    RaisePropertyChangedEvent("RemovedAutomaticReportsTimeLimit");
                }
            }
        }
        // ---- processing...
        public bool ProcessingShutDownWhenComplete
        {
            get { return _ProcessingShutDownWhenComplete; }
            set
            {
                if (_ProcessingShutDownWhenComplete != value)
                {
                    _ProcessingShutDownWhenComplete = value;
                    RaisePropertyChangedEvent("ProcessingShutDownWhenComplete");
                }
            }
        }
        public bool JobRunUseCachedReportEnableCache
        {
            get { return _JobRunUseCachedReportEnableCache; }
            set
            {
                if (_JobRunUseCachedReportEnableCache != value)
                {
                    _JobRunUseCachedReportEnableCache = value;
                    RaisePropertyChangedEvent("JobRunUseCachedReportEnableCache");
                }
            }
        }
        public TimeSpan JobRunUseCachedReportTimeLimit
        {
            get { return _JobRunUseCachedReportTimeLimit; }
            set
            {
                if (_JobRunUseCachedReportTimeLimit != value)
                {
                    _JobRunUseCachedReportTimeLimit = value;
                    RaisePropertyChangedEvent("JobRunUseCachedReportTimeLimit");
                }
            }
        }
        // ---- snap shots
        public bool EnableArchiveZipFileSnapshots
        {
            get { return _EnableArchiveZipFileSnapshots; }
            set
            {
                if (_EnableArchiveZipFileSnapshots != value)
                {
                    _EnableArchiveZipFileSnapshots = value;
                    RaisePropertyChangedEvent("EnableArchiveZipFileSnapshots");
                }
            }
        }
        public TimeSpan ArchiveZipFileSnapshotsMaturityTimeLimit
        {
            get { return _ArchiveZipFileSnapshotsMaturityTimeLimit; }
            set
            {
                if (_ArchiveZipFileSnapshotsMaturityTimeLimit != value)
                {
                    _ArchiveZipFileSnapshotsMaturityTimeLimit = value;
                    RaisePropertyChangedEvent("ArchiveZipFileSnapshotsMaturityTimeLimit");
                }
            }
        }
        public long ArchiveZipFileSnapshotsMaximum
        {
            get { return _ArchiveZipFileSnapshotsMaximum; }
            set
            {
                if (_ArchiveZipFileSnapshotsMaximum != value)
                {
                    _ArchiveZipFileSnapshotsMaximum = value;
                    RaisePropertyChangedEvent("ArchiveZipFileSnapshotsMaximum");
                }
            }
        }
        public string ArchiveZipFileSnapshotsRootPath
        {
            get { return _ArchiveZipFileSnapshotsRootPath; }
            set
            {
                if (_ArchiveZipFileSnapshotsRootPath != value)
                {
                    _ArchiveZipFileSnapshotsRootPath = value;
                    RaisePropertyChangedEvent("ArchiveZipFileSnapshotsRootPath");
                }
            }
        }
        // ---- functions
        public GlobalSettings Clone()
        {
            var worker = new dodSON.Core.Converters.TypeSerializer<GlobalSettings>();
            return worker.FromByteArray(worker.ToByteArray(this));
        }
        #endregion
    }
}
