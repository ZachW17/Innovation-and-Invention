using ImpTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PleaseWorkUserControl
{
    /// <summary>
    /// Interaction logic for UserControl2.xaml
    /// </summary>
    public partial class Disk : UserControl
    {
        public float ringSize = 75;
        public int TouchID;
        public MainWindow mainWindow { get { 
            Canvas c = Parent as Canvas;
            if (c != null)
            {
                MainWindow m = c.Parent as MainWindow;
                return m;
            }
            return null;
        } }
        public ImpTest.Cursor cursor;
        //center of disk
        public double CenterX { get { return RenderTransform.Value.OffsetX + Radius; } }
        public double CenterY { get { return RenderTransform.Value.OffsetY + Radius; } }
        public double Radius
        {
            get
            {
                Matrix m = RenderTransform.Value;
                //undo translation since we only wanna apply scaling (rotation to but it wont affect radius)
                m.OffsetX = 0;
                m.OffsetY = 0;
                return Length(m.Transform(new Point(Width / 2, 0)));
            }
        }  //radius of disk
        public double Scale
        {
            get
            {
                return Radius / (OuterCircle.circle.RadiusX/2);
            }
        }   //scale of entire disk (different from radius)
        public double InvScale
        {
            get { return 1 / Scale; }
        }//1 / scale

        public Disk()
        {
            InitializeComponent();
            TouchMove += THIS_TouchMove_1;
            TouchDown += Disk_TouchDown;
            TouchUp+=Disk_TouchUp;
        }
        #region random helper functions
        double Length(Point p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }
        #endregion
        //contructs the new cursor in the center of the ring, also stores the handle for it
        public void CreateCursor(Canvas c)
        {
            cursor = mainWindow.aman.addCursor(36, 5, 50, new Point(CenterX, CenterY), c, 750);
            //add our update once
            mainWindow.eitime.CustomizableUpdate += update;
        }
        public void update(object sender, EventArgs e)
        {
            //keep our icons up to date
            Opacity = cursor.circleAlpha;
            UpdateToolSpots();
            //switch (cursor.state)
            //{
            //    case ImpTest.Cursor.State.Idle:
                    OuterCircle.circle.Opacity = 1;
            //        break;
            //}
            OuterCircle.circle.StrokeThickness = ringSize * InvScale;
        }

        /// <summary>
        /// if the Zone has been moved then it needs to be notified to cancel zone collapsing functions
        /// </summary>
        public void NotifyMovement()
        {
            touchedCircle = false;
            mainWindow.eitime.ForceUpdate();
            //place buttons on appropriate points on Mikes Ring    UpdateToolSpots();
            //UpdateToolSpots();
            SynchronizeCursor();
            if (CenterX < 0 || CenterX > mainWindow.Width ||
                CenterY < 0 || CenterY > mainWindow.Height)
            {
                //remove cursor
                mainWindow.aman.removeCursor(mainWindow.aman.objects.IndexOf(cursor));
                mainWindow.grid_Main.Children.Remove(this);
            }
        }
        public void UpdateToolSpots()
        {
            float angle = 360 / OuterCircle.ToolButtons.Count;
            for (int i = 0; i < OuterCircle.ToolButtons.Count; i++)
            {
                Tool tool = OuterCircle.Tools[i];
                Button but = OuterCircle.ToolButtons[i];
                //translate by the icon spot at this index
                Matrix translatedMatrix = Matrix.Identity;
                //translatedMatrix.Scale(InvScale, InvScale);
                //very weird compensations for icons being attached to things at once
                translatedMatrix.Rotate(i * -angle);
                translatedMatrix.Translate(Radius-ringSize/2, 0);
                translatedMatrix.Rotate(i * angle);
                translatedMatrix.Scale(InvScale, InvScale);
                //translatedMatrix.Translate(Radius, Radius);
                //10
                //01
                but.RenderTransform = new MatrixTransform(translatedMatrix);

                // OuterCircle.ToolButtons[i].

                but.Visibility = System.Windows.Visibility.Visible;
                if (OuterCircle.currentTool == tool)
                {
                    but.BorderBrush = new SolidColorBrush(Color.FromArgb(100,0,0,0));
                }
                else
                {
                    but.BorderBrush = Brushes.Transparent;
                }

                //switch (cursor.state)
                //{
                //    case ImpTest.Cursor.State.Scaling:
                //        OuterCircle.ToolButtons[i].Visibility = System.Windows.Visibility.Collapsed;
                //        break;
                //    case ImpTest.Cursor.State.Opening:
                //        OuterCircle.ToolButtons[i].Visibility = System.Windows.Visibility.Collapsed;
                //        break;
                //    case ImpTest.Cursor.State.Idle:
                //        OuterCircle.ToolButtons[i].Visibility = System.Windows.Visibility.Visible;
                //        break;
                //}
            }
        }
        public void SynchronizeCursor()
        {
            if (cursor != null)
            {
                //offset is top left, add width/2 and height/2 to push it to the center
                cursor.Center = new Point(CenterX, CenterY);
                if (OuterCircle.Visibility == System.Windows.Visibility.Visible)
                {
                    //cursor.smoothScale((float)Radius);
                    cursor.radius = (float)Radius;
                }
            }
        }

        bool touchedCircle = false;
        private void Circle_TouchDown_1(object sender, TouchEventArgs e)
        {
            touchedCircle = true;
        }

        private void UserControl_TouchUp_1(object sender, TouchEventArgs e)
        {
            if (touchedCircle)
            {
                if (InnerCircle.IsMouseOver) Expand();
                //else if (OuterCircle.IsMouseOver) Collapse();
            }
            touchedCircle = false;
        }
        public void Expand()
        {
            InnerCircle.Visibility = System.Windows.Visibility.Collapsed;
            OuterCircle.Visibility = System.Windows.Visibility.Visible;
            cursor.smoothScale((float)Radius);
            OuterCircle.circle.Opacity = 0;
            //cursor.radius = (float)Radius;
        }
        public void Collapse()
        {
            OuterCircle.Visibility = System.Windows.Visibility.Collapsed;
            InnerCircle.Visibility = System.Windows.Visibility.Visible;
            cursor.smoothScale(50);
            //cursor.radius = 50;
        }

        private void UserControl_TouchLeave_1(object sender, TouchEventArgs e)
        {
            touchedCircle = false;
        }

        #region Handling Inner Canvas Pen Input

        public bool touchedInnerCanvas = false;
        private void THIS_TouchMove_1(object sender, TouchEventArgs e)
        {
            //after the disk gets deleted its tends to crash here because it is deleted during movement and still comes here
            //its not deleted just removed from stage, which should be as good as deleted but isnt...
            if (mainWindow == null) return;
            if(touchedInnerCanvas)
            {
                mainWindow.ald.DrawAgain = true;
                TouchID = e.TouchDevice.Id;
            }
        }

        private void Disk_TouchDown(object sender, TouchEventArgs e)
        {
            //get the touch location relative to 'this'
            Point loc = e.GetTouchPoint(mainWindow).Position;
            Point p = loc;
            p.X -= CenterX;
            p.Y -= CenterY;
            if (OuterCircle.Visibility == System.Windows.Visibility.Visible &&
                Math.Sqrt(p.X*p.X+p.Y*p.Y) < Radius - ringSize)
            {
                touchedInnerCanvas = true;

                //transparency
                cursor.canWakeUp = false;
                cursor.shouldFade = true;
                cursor.beginFade(.1, .1);

            #region centering code Tim 10/30/13
                //matrix is a struct so any changes are lost unless we make a copy (it seems)
                //we cast (as) Matrix Transform because it allows us access to '.Matrix'
                var matrix = RenderTransform.Value;
                //the loc is relative to the top left corner, so to get it relative to the center we have to subtract the distance to the center
                //radiusX and radiusY are actually diameters so we halve them
                matrix.OffsetX = matrix.OffsetY = 0;
                matrix.Translate(loc.X - Radius, loc.Y - Radius);
                this.RenderTransform = new MatrixTransform(matrix);
                NotifyMovement();
#endregion
            }
            ReOrderDisks();
        }

        /// <summary>
        /// When a user touches a disk it will bring it in front of all other disks
        /// </summary>
        private void ReOrderDisks()
        {
            int lastDiskPosition = -1;
            int thisDiskPosition = mainWindow.grid_Main.Children.IndexOf(this);
            for (int i = 0; i < mainWindow.grid_Main.Children.Count; i++)
            {
                if (mainWindow.grid_Main.Children[i] is Disk && mainWindow.grid_Main.Children[i] != this) 
                    lastDiskPosition = i;
            }
            //check if we found another disk and if it is after ours
            if (lastDiskPosition != -1 && lastDiskPosition > thisDiskPosition)
            {
                //we are going to remove this from the canvas but when we do Parent will be null
                //so lets store it
                MainWindow holdOn = mainWindow;
                holdOn.grid_Main.Children.RemoveAt(thisDiskPosition);
                holdOn.grid_Main.Children.Insert(lastDiskPosition, this);
            }
        }

        private void Disk_TouchUp(object sender, TouchEventArgs e)
        {
            touchedInnerCanvas = false;
            //touchID = -1 happens in AttemptedLineDrawer
            //transparency
            cursor.canWakeUp = true;
            cursor.wakeUp();
        }

        #endregion
    }
}
