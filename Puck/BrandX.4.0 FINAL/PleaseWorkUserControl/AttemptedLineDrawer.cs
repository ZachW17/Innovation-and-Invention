using ImpTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PleaseWorkUserControl
{
    public class AttemptedLineDrawera
    {
        public Canvas myCanvas;
        public bool DrawAgain = true;
        private bool drawingVisualUpdated = false;

        // testing bool
        private bool mouseTest = false;
        private bool doTemporaryLineDraw=false;

        private bool isMouseDown = false;

        // variables to hold mouse location
        private Point prevMouseLoc = new Point();
        private Point currMouseLoc = new Point();

        // Lists for touch collections
        //private TouchPoint tpCurr;
        //private TouchPoint tpPrev;

        private Dictionary<int, Point> prevTouches = new Dictionary<int,Point>();
        private Dictionary<int, Point> currTouches = new Dictionary<int,Point>();

        RenderTargetBitmap bmp;

        Image myImage = new Image();
        DispatcherTimer dt;
        DrawingVisual drawingVisual;
        DrawingContext drawingContext;

        public Pen myPen;
        int everyOther = 0;
        

        public AttemptedLineDrawera(Canvas MyCanvas, EITime ei)
        {
            myCanvas = MyCanvas;
            /*
            myCanvas.MouseDown +=myCanvas_MouseDown;
            myCanvas.MouseUp+=myCanvas_MouseUp;
            myCanvas.MouseMove+=myCanvas_MouseMove;
             */
            myCanvas.TouchDown+=myCanvas_TouchDown;
            myCanvas.TouchUp+=myCanvas_TouchUp;
            myCanvas.TouchMove += myCanvas_TouchMove;
            bmp = new RenderTargetBitmap(
                (int)(myCanvas.Parent as MainWindow).Width, (int)(myCanvas.Parent as MainWindow).Height, 96, 96, PixelFormats.Pbgra32);
            myImage.Source = bmp;
            myCanvas.Children.Insert(0,myImage);
            InputManager.Current.PostProcessInput += Current_PostProcessInput;
            //ei.CustomizableUpdate += Current_PostProcessInput;
            myPen = new Pen();
            myPen.Brush = Brushes.Black;
            myPen.Thickness = 3;
            drawingVisual = new DrawingVisual();
            dt= new DispatcherTimer();
            dt.Tick += update;
            dt.Interval = TimeSpan.FromMilliseconds(100);

            myPen = new Pen();
            myPen.Brush = Brushes.Black;
            myPen.Thickness = 3;
            myPen.StartLineCap = PenLineCap.Round;
            myPen.EndLineCap = PenLineCap.Round;
            //dt.Start();
        }
        //Just doesnt work
        public void ForceUpdate()
        {
            //just throw update, parameters dont matter for now (11/3/13)
            update(this, null);
            //none of this works
            myCanvas.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            myCanvas.InvalidateVisual();
            myCanvas.UpdateLayout();
        }
        //basically renders the next frame
        void update(object sender, EventArgs e)
        {
            if (drawingContext != null)
            {
                bmp.Render(drawingVisual);
                drawingContext.Close();
                drawingContext = null;
            }
        }
        //this should happen whenever it finishes updating input
        public void Current_PostProcessInput(object sender, EventArgs e)
        {
            everyOther = (everyOther + 1) % 100;
            if(everyOther == 0)
                update(sender, e);
        }

        

        private void draw(Pen pen, Point prevLoc, Point currLoc, int thick, int touchId)
        {
            /*// Create a new line and define color
            Line myLine = new Line();
            myLine.Stroke = color;

            // line's x coord
            myLine.X1 = prevLoc.X;
            myLine.X2 = currLoc.X;

            // line's y coord
            myLine.Y1 = prevLoc.Y;
            myLine.Y2 = currLoc.Y;

            // line's alignment
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Center;

            // thickness
            myLine.StrokeThickness = thick;

            // add line to the canvas
            myCanvas.Children.Insert(0,myLine);



            // Garbage Collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            */

                Line myLine = new Line();
                myLine.Stroke = Brushes.Black;
                // line's x coord
                myLine.X1 = prevLoc.X;
                myLine.X2 = currLoc.X;

                // line's y coord
                myLine.Y1 = prevLoc.Y;
                myLine.Y2 = currLoc.Y;

                // line's alignment
                myLine.HorizontalAlignment = HorizontalAlignment.Left;
                myLine.VerticalAlignment = VerticalAlignment.Center;

                // thickness
                myLine.StrokeThickness = 10;

                // add line to the canvas
                //myCanvas.Children.Add(myLine);


                if(drawingContext == null) drawingContext = drawingVisual.RenderOpen();
                            // Hide Disk, draw line
                            //d.OuterCircle.Visibility = System.Windows.Visibility.Hidden; //commented out because mikes code handles transparency
                            drawingContext.DrawLine(pen, new Point(myLine.X1, myLine.Y1), new Point(myLine.X2, myLine.Y2));

                //bmp.Render(drawingVisual);
                //drawingVisualUpdated = true;
        }

        #region mouse
        /*
        private void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            prevMouseLoc = e.GetPosition(myCanvas);
        }

        private void myCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        private void myCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                // Create a new line and define color
                //Line myLine = new Line();
                //myLine.Stroke = Brushes.Red;

                //currMouseLoc = e.GetPosition(this);

                //// line's x coord
                //myLine.X1 = prevMouseLoc.X;
                //myLine.X2 = currMouseLoc.X;

                //// line's y coord
                //myLine.Y1 = prevMouseLoc.Y;
                //myLine.Y2 = currMouseLoc.Y;

                // EDIT: currMouseLoc = e.GetPosition(this); //
                currMouseLoc = e.GetPosition(myCanvas);

                // use draw method
                if(mouseTest)  
                draw(Brushes.Red, prevMouseLoc, currMouseLoc, 3, -1);

                // store current mouse Location in prev mouse Location
                prevMouseLoc = currMouseLoc;

                //// line's alignment
                //myLine.HorizontalAlignment = HorizontalAlignment.Left;
                //myLine.VerticalAlignment = VerticalAlignment.Center;

                //// thickness
                //myLine.StrokeThickness = 2;

                //// add line to the canvas
                //myCanvas.Children.Add(myLine);
            }
        }
*/
        #endregion

        #region touch
        private void myCanvas_TouchMove(object sender, TouchEventArgs e)
        {
            if (DrawAgain)
            {
                DrawAgain = true;
                e.Handled = true;
                //tpCurr = e.GetTouchPoint(this);

                int tempId = e.TouchDevice.Id;
                //Disk d = null;
                //foreach (UIElement ui in myCanvas.Children)
                //{
                //    d = ui as Disk;
                //    if (d != null)
                //    {
                //        if (d.TouchID == tempId)
                //        {
                //            //we found a match
                //            //check if this disk is trying to draw(user touched inner canvas)
                //            if (!d.touchedInnerCanvas || d.OuterCircle.currentTool == null) return;
                //            d.OuterCircle.Opacity = .2;
                //            break;
                //        }
                //    }
                //}
                //if (d == null)
                //    return;
                //Point tempPoint = e.GetTouchPoint(this).Position;
                Point tempPoint = e.GetTouchPoint(myCanvas).Position;
                if (currTouches.ContainsKey(tempId))
                    currTouches[tempId] = tempPoint;
                else
                    currTouches.Add(tempId, tempPoint);

                draw(myPen, prevTouches[tempId], currTouches[tempId], 3, tempId);

                prevTouches[tempId] = currTouches[tempId];

                // set prev touch collect to current
                //tpPrev = tpCurr;
                #region TemporaryLine Code
                if (doTemporaryLineDraw)
                {
                    if (myCanvas.Children.Count > 300)
                    {
                        for (int i = myCanvas.Children.Count - 1; i >= 0; i--)
                        {
                            if (myCanvas.Children[i] is Line)
                            {
                                myCanvas.Children.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                #endregion
            }
        }        

        private void myCanvas_TouchDown(object sender, TouchEventArgs e)
        {
            //tpPrev = e.GetTouchPoint(this);

            // Store the touch id and the touch location
            int tempId = e.TouchDevice.Id;
            /* Edit: Point tempPoint = e.GetTouchPoint(this).Position; */
            Point tempPoint = e.GetTouchPoint(myCanvas).Position;

            if (!prevTouches.ContainsKey(tempId))
            {
                prevTouches.Add(tempId, tempPoint);
            }
        }

        private void myCanvas_TouchUp(object sender, TouchEventArgs e)
        {
            //TEMP to replace mikes transparency
            int tempId = e.TouchDevice.Id;
            Disk d=null;
            foreach (UIElement ui in myCanvas.Children)
            {
                d = ui as Disk;
                if (d != null)
                {
                    if (d.TouchID == tempId)
                    {
                        // Hide Disk, draw line
                        d.OuterCircle.Opacity = 1f;
                        d.TouchID = -1;
                        break;
                    }
                }
            }
#region WIP line finishing fix
            //dont return this time, because prevtouches and currtouches might actually have stuff to be removed
            //if (d != null)
            //    draw(d.OuterCircle.currentTool.Pen_Unique, prevTouches[tempId], currTouches[tempId], 3, tempId);
            //ForceUpdate();
#endregion
            prevTouches.Remove(e.TouchDevice.Id);
            currTouches.Remove(e.TouchDevice.Id);
        }
        #endregion
    }
}
