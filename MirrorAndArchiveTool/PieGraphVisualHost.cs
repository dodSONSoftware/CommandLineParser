using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MirrorAndArchiveTool
{
    public class PieGraphVisualHost
        : FrameworkElement
    {
        #region Dependency Properties
        public System.Windows.Media.Pen VisualHostPen
        {
            get { return (System.Windows.Media.Pen)GetValue(VisualHostPenProperty); }
            set { SetValue(VisualHostPenProperty, value); }
        }
        public static readonly DependencyProperty VisualHostPenProperty = DependencyProperty.Register("VisualHostPen", typeof(System.Windows.Media.Pen), typeof(VisualHost));
        // --------
        public System.Windows.Media.Pen VisualHostAltPen
        {
            get { return (System.Windows.Media.Pen)GetValue(VisualHostAltPenProperty); }
            set { SetValue(VisualHostAltPenProperty, value); }
        }
        public static readonly DependencyProperty VisualHostAltPenProperty = DependencyProperty.Register("VisualHostAltPen", typeof(System.Windows.Media.Pen), typeof(VisualHost));
        #endregion
        #region Ctor
        public PieGraphVisualHost()
            : base()
        {
            // initialize
            DataContext = this;
            _Children = new System.Windows.Media.VisualCollection(this);
            MouseEnter += new System.Windows.Input.MouseEventHandler(VisualHost_MouseEnter);
            MouseLeave += new System.Windows.Input.MouseEventHandler(VisualHost_MouseLeave);
            this.Loaded += new RoutedEventHandler(VisualHost_Loaded);
            // setup default dependency properties
            var penBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            VisualHostPen = new System.Windows.Media.Pen(penBrush, 1);
            //var geoStr = "M0,0 L100,0";
            //_Geometry = System.Windows.Media.Geometry.Parse(geoStr.ToString());
            //SetWidthAndHeight(geoStr);
        }
        public PieGraphVisualHost(string title,
                                  string geoStr,
                                  System.Windows.Media.Pen pen,
                                  System.Windows.Media.Pen altPen)
            : this()
        {
            if (string.IsNullOrWhiteSpace(title)) { throw new ArgumentNullException("title"); }
            if (string.IsNullOrWhiteSpace(geoStr)) { throw new ArgumentNullException("geoStr"); }
            if (pen == null) { throw new ArgumentNullException("pen"); }
            if (altPen == null) { throw new ArgumentNullException("altPen"); }
            //_Title = title;
            //_GeometryString = geoStr;
            //_Geometry = System.Windows.Media.Geometry.Parse(geoStr.ToString());
            //SetWidthAndHeight(geoStr);
            VisualHostPen = pen;
            VisualHostAltPen = altPen;
        }
        #endregion
        #region Private Fields
        private System.Windows.Media.VisualCollection _Children;
        private System.Windows.Media.Geometry _Geometry = null;
        //private string _GeometryString = "";
        //private string _Title = "";
        //private double _Minimum = 0;
        //private double _Maximum = 0;
        #endregion
        #region Public Methods
        //public string Title
        //{
        //    get { return _Title; }
        //}
        //public string GeometryString
        //{
        //    get { return _GeometryString; }
        //}
        //public double Minumum
        //{
        //    get { return _Minimum; }
        //}
        //public double Maximum
        //{
        //    get { return _Maximum; }
        //}
        #endregion
        #region FrameworkElement Events
        void VisualHost_Loaded(object sender, RoutedEventArgs e)
        {
            _Children.Clear();
            _Children.Add(CreateDrawing(VisualHostPen, null));
        }
        void VisualHost_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _Children.Clear();
            _Children.Add(CreateDrawing(VisualHostAltPen, null));
        }
        void VisualHost_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _Children.Clear();
            _Children.Add(CreateDrawing(VisualHostPen, null));
        }
        #endregion
        #region Overrides
        protected override int VisualChildrenCount
        {
            get { return _Children.Count; }
        }
        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            if ((index < 0) || (index >= _Children.Count)) { throw new ArgumentOutOfRangeException("index"); }
            return _Children[index];
        }
        #endregion
        #region Private Methods
        private System.Windows.Media.Visual CreateDrawing(System.Windows.Media.Pen pen,
                                                          System.Windows.Media.SolidColorBrush brush)
        {
            var drawingVisual = new System.Windows.Media.DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                var angleShare = 45.0;
                
                var centerPoint = new Point(this.Width / 2, this.Height / 2);
                var startAngle = 


                ArcSegment arcSegment = new ArcSegment(endPointofArc, pieSize, angleShare, angleShare > 180, SweepDirection.Clockwise, false);
            }
            return drawingVisual;
        }
        //private void SetWidthAndHeight(string geoStr)
        //{
        //    var working = new StringBuilder(1024);
        //    foreach (var ch in geoStr)
        //    {
        //        if ((char.IsDigit(ch)) || (ch == ' ') || (ch == ',') || (ch == '-')) { working.Append(ch); }
        //    }
        //    var pairs = working.ToString().Split(' ');
        //    var smallestX = Int32.MaxValue;
        //    var largestX = Int32.MinValue;
        //    var smallestY = Int32.MaxValue;
        //    var largestY = Int32.MinValue;
        //    foreach (var pair in pairs)
        //    {
        //        var xy = pair.Split(',');
        //        var x = Convert.ToInt32(xy[0].ToString());
        //        var y = Convert.ToInt32(xy[1].ToString());
        //        if (x < smallestX) { smallestX = x; }
        //        if (x > largestX) { largestX = x; }
        //        if (y < smallestY) { smallestY = y; }
        //        if (y > largestY) { largestY = y; }
        //    }
        //    _Minimum = smallestY;
        //    _Maximum = largestY;
        //    this.Width = largestX - smallestX;
        //    this.Height = largestY - smallestY;
        //}
        #endregion
    }
}
