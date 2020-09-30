using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool
{
    [Serializable()]
    public class UserSettings
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
        public UserSettings()
        { }
        #endregion
        #region Private Fields
        private Pair<double, double> _MainWindowLocation = new Pair<double, double>(-1, -1);
        private Pair<double, double> _MainWindowSize = new Pair<double, double>(-1, -1);
        // ----
        private Pair<double, double> _SettingsWindowLocation = new Pair<double, double>(-1, -1);
        private Pair<double, double> _SettingsWindowSize = new Pair<double, double>(-1, -1);
        // ----
        private Pair<double, double> _AdvancedSettingsWindowLocation = new Pair<double, double>(-1, -1);
        private Pair<double, double> _AdvancedSettingsWindowSize = new Pair<double, double>(-1, -1);
        // ----
        private string[] _MainWindowJobButtonsColumnsWidths = new string[3] { "300", "5", "*" };
        #endregion
        #region Public Methods
        public Pair<double, double> MainWindowLocation
        {
            get { return _MainWindowLocation; }
            set
            {
                if (!_MainWindowLocation.Equals(value))
                {
                    _MainWindowLocation = value;
                    RaisePropertyChangedEvent("MainWindowLocation");
                }
            }
        }
        public Pair<double, double> MainWindowSize
        {
            get { return _MainWindowSize; }
            set
            {
                if (!_MainWindowSize.Equals(value))
                {
                    _MainWindowSize = value;
                    RaisePropertyChangedEvent("MainWindowSize");
                }
            }
        }
        // ----
        public Pair<double, double> SettingsWindowLocation
        {
            get { return _SettingsWindowLocation; }
            set
            {
                if (!_SettingsWindowLocation.Equals(value))
                {
                    _SettingsWindowLocation = value;
                    RaisePropertyChangedEvent("SettingsWindowLocation");
                }
            }
        }
        public Pair<double, double> SettingsWindowSize
        {
            get { return _SettingsWindowSize; }
            set
            {
                if (!_SettingsWindowSize.Equals(value))
                {
                    _SettingsWindowSize = value;
                    RaisePropertyChangedEvent("SettingsWindowSize");
                }
            }
        }
        // ----
        public Pair<double, double> AdvancedSettingsWindowLocation
        {
            get { return _AdvancedSettingsWindowLocation; }
            set
            {
                if (!_AdvancedSettingsWindowLocation.Equals(value))
                {
                    _AdvancedSettingsWindowLocation = value;
                    RaisePropertyChangedEvent("AdvancedSettingsWindowLocation");
                }
            }
        }
        public Pair<double, double> AdvancedSettingsWindowSize
        {
            get { return _AdvancedSettingsWindowSize; }
            set
            {
                if (!_AdvancedSettingsWindowSize.Equals(value))
                {
                    _AdvancedSettingsWindowSize = value;
                    RaisePropertyChangedEvent("AdvancedSettingsWindowSize");
                }
            }
        }
        // ----
        public string[] MainWindowJobButtonsColumnsWidths
        {
            get {
                if (_MainWindowJobButtonsColumnsWidths == null) { _MainWindowJobButtonsColumnsWidths = new string[3] { "300", "5", "*" }; }
                return _MainWindowJobButtonsColumnsWidths; }
            set
            {
                if (!_MainWindowJobButtonsColumnsWidths.Equals(value))
                {
                    _MainWindowJobButtonsColumnsWidths = value;
                    RaisePropertyChangedEvent("MainWindowJobButtonsColumnsWidths");
                }
            }
        }
        #endregion
    }
}
