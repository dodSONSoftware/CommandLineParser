using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool
{
    [Serializable()]
    public abstract class JobBase
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
            if (_StateChangeTracker == null) { _StateChangeTracker = new dodSON.Core.Common.StateChangeTracking(); }
            _StateChangeTracker.MarkDirty();
            if (PropertyChanged != null) { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName)); }
        }
        #endregion
        #region Ctor
        protected JobBase()
        {
            _StateChangeTracker = new dodSON.Core.Common.StateChangeTracking();
        }
        protected JobBase(string name)
            : this()
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentNullException("name"); }
            _Name = name;
        }
        #endregion
        #region Private Fields
        [NonSerialized()]
        private dodSON.Core.Common.StateChangeTracking _StateChangeTracker = new dodSON.Core.Common.StateChangeTracking();
        private string _ID = Guid.NewGuid().ToString();
        private string _Name = "";
        private bool _IsEnabled = true;
        private DateTime _DateCreate = DateTime.Now;
        private DateTime _DateLastRan = DateTime.MinValue;
        private bool _ArchiveRemoveFiles = false;
        [NonSerialized()]
        private string _RecommendedActionIcon = "";
        private System.Windows.Visibility _RecommendedActionIcon_WaitIconVisibility = System.Windows.Visibility.Hidden;
        private System.Windows.Visibility _RecommendedActionIcon_WaitIconInvertVisibility = System.Windows.Visibility.Hidden;
        private string _RecommenedActionText = "";
        private string _RecommendedActionReason = "";
        #endregion
        #region Public Methods
        public dodSON.Core.Common.StateChangeTracking StateChangeTracker
        {
            get
            {
                if (_StateChangeTracker == null) { _StateChangeTracker = new dodSON.Core.Common.StateChangeTracking(); }
                return _StateChangeTracker;
            }
        }
        public string ID
        {
            get { return _ID; }
            protected set
            {
                if (string.IsNullOrWhiteSpace(value)) { throw new ArgumentNullException("value"); }
                if (_ID != value)
                {
                    _ID = value;
                    RaisePropertyChangedEvent("ID");
                }
            }
        }
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = Helper.StripBadCharacters(value);
                    RaisePropertyChangedEvent("Name");
                }
            }
        }
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    RaisePropertyChangedEvent("IsEnabled");
                }
            }
        }
        public DateTime DateCreate
        {
            get { return _DateCreate; }
            internal set
            {
                if (_DateCreate != value)
                {
                    _DateCreate = value;
                    RaisePropertyChangedEvent("DateCreate");
                }
            }
        }
        public string Age
        {
            get
            {
                var age = DateTime.Now - DateCreate;
                if (age < TimeSpan.FromMinutes(1.5))
                {
                    return "(Just Now!)";
                }
                return $"({dodSON.Core.Common.DateTimeHelper.FormatTimeSpan(age)})";
            }
        }
        public DateTime DateLastRan
        {
            get { return _DateLastRan; }
            set
            {
                if (_DateLastRan != value)
                {
                    _DateLastRan = value;
                    RaisePropertyChangedEvent("DateLastRan");
                }
            }
        }
        public bool ArchiveRemoveFiles
        {
            get { return _ArchiveRemoveFiles; }
            set
            {
                if (_ArchiveRemoveFiles != value)
                {
                    _ArchiveRemoveFiles = value;
                    RaisePropertyChangedEvent("ArchiveRemoveFiles");
                }
            }
        }
        public string RecommendedActionIcon
        {
            get { return _RecommendedActionIcon; }
            set
            {
                if (_RecommendedActionIcon != value)
                {
                    _RecommendedActionIcon = value;
                    RaisePropertyChangedEvent(nameof(RecommendedActionIcon));
                }
            }
        }
        public System.Windows.Visibility RecommendedActionIcon_WaitIconVisibility
        {
            get { return _RecommendedActionIcon_WaitIconVisibility; }
            set
            {
                if (_RecommendedActionIcon_WaitIconVisibility != value)
                {
                    _RecommendedActionIcon_WaitIconVisibility = value;
                    RaisePropertyChangedEvent(nameof(RecommendedActionIcon_WaitIconVisibility));
                }
            }
        }
        public System.Windows.Visibility RecommendedActionIcon_WaitIconInvertVisibility
        {
            get { return _RecommendedActionIcon_WaitIconInvertVisibility; }
            set
            {
                if (_RecommendedActionIcon_WaitIconInvertVisibility != value)
                {
                    _RecommendedActionIcon_WaitIconInvertVisibility = value;
                    RaisePropertyChangedEvent(nameof(RecommendedActionIcon_WaitIconInvertVisibility));
                }
            }
        }
        public string RecommendedActionText
        {
            get { return _RecommenedActionText; }
            set
            {
                if (_RecommenedActionText != value)
                {
                    _RecommenedActionText = value;
                    RaisePropertyChangedEvent(nameof(RecommendedActionText));
                }
            }
        }
        public string RecommendedActionReason
        {
            get { return _RecommendedActionReason; }
            set
            {
                if (_RecommendedActionReason != value)
                {
                    _RecommendedActionReason = value;
                    RaisePropertyChangedEvent(nameof(RecommendedActionReason));
                }
            }
        }
        public JobBase Clone()
        {
            return dodSON.Core.Converters.ConvertersHelper.Clone(this);
        }
        #endregion
    }
}
