//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace MirrorAndArchiveTool
//{
//    public class JobsView
//    {
//        #region Ctor
//        private JobsView()
//        { }
//        public JobsView(IEnumerable<JobBase> jobs)
//            : this()
//        {
//            if (jobs == null) { throw new ArgumentNullException("jobs"); }
//            _Jobs = new System.Collections.ObjectModel.ObservableCollection<JobBase>(jobs);
//        }
//        #endregion
//        #region Private Fields
//        private System.Collections.ObjectModel.ObservableCollection<JobBase> _Jobs = null;
//        #endregion
//        #region Public Methods
//        public System.Collections.ObjectModel.ObservableCollection<JobBase> Jobs
//        {
//            get { return _Jobs; }
//        }
//        #endregion
//    }
//}
