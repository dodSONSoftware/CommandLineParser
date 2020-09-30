using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool
{
    [Serializable()]
    public class Pair<T1, T2>
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
        public Pair(T1 alpha, T2 beta)
        {
            _Alpha = alpha;
            _Beta = beta;
        }
        #endregion
        #region Private Fields
        private T1 _Alpha = default(T1);
        private T2 _Beta = default(T2);
        #endregion
        #region Public Methods
        public T1 Alpha
        {
            get { return _Alpha; }
            set
            {
                if (!_Alpha.Equals(value))
                {
                    _Alpha = value;
                    RaisePropertyChangedEvent("Alpha");
                }
            }
        }
        public T2 Beta
        {
            get { return _Beta; }
            set
            {
                if (!_Beta.Equals(value))
                {
                    _Beta = value;
                    RaisePropertyChangedEvent("Beta");
                }
            }
        }
        #endregion
    }
}
