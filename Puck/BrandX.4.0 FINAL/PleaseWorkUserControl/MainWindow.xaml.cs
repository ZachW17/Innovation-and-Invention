using ImpTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const float MillisecondsItTakesToSpawn = 1000;

        public bool doLockCursor = false;
        public AttemptedLineDrawera ald;
        public System.Timers.Timer spawnTimer; //triggers spawn event
        Point spawnLocation; //location to spawn new Disk
        public AnimationManager aman;
        public EITime eitime;
        Canvas animationCanvas;
        int colorIndex = 1;

        public MainWindow()
        {
            //add animation and cursor technology before initalize component makes the Disks
            aman = new AnimationManager();
            eitime = new EITime(aman.batchUpdate, animationCanvas);

            //Initializes all components on the stage or to be on the stage at startup
            InitializeComponent();

            animationCanvas = new Canvas();
            grid_Main.Children.Add(animationCanvas);
            ald = new AttemptedLineDrawera(grid_Main, eitime);
            spawnTimer = new System.Timers.Timer(MillisecondsItTakesToSpawn);
            spawnTimer.AutoReset = false;
            spawnTimer.Elapsed += spawnTimer_Elapsed_SpawnNewDisk;
            //grid_Main.Children.Add(b);
            animationCanvas.RenderTransform = new MatrixTransform();
            animationCanvas.LayoutTransform = new MatrixTransform();
            animationCanvas.Margin = new Thickness();

            color_btn.Background = ald.myPen.Brush;

            #region "initialize all animation disks, should be moved someplace more appropriate"
            foreach (UIElement ui in grid_Main.Children)
            {
                Disk disk = ui as Disk;
                if (disk != null)
                {
                    disk.CreateCursor(animationCanvas);
                    Matrix m = Matrix.Identity;
                    m.Translate(Canvas.GetLeft(disk), Canvas.GetTop(disk));
                    disk.RenderTransform = new MatrixTransform(m);
                    Canvas.SetLeft(disk, 0);
                    Canvas.SetTop(disk, 0);
                }
            }
            #endregion
        }

        private void Window_ManipulationStarting_1(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
        }

        private void Window_ManipulationDelta_1(object sender, ManipulationDeltaEventArgs e)
        {
            var element = e.OriginalSource as UIElement;
            if (doLockCursor && element is Disk)
            {
                if ((element as Disk).touchedInnerCanvas)
                    element = null;
            }
            if (element != null)
            {
                var transformation = element.RenderTransform
                                                        as MatrixTransform;
                var matrix = transformation == null ? Matrix.Identity :
                                                transformation.Matrix;

                //dont scale if fingers are on the inner part
                if (element is Disk && !(element as Disk).touchedInnerCanvas)
                    matrix.ScaleAt(e.DeltaManipulation.Scale.X,
                                    e.DeltaManipulation.Scale.Y,
                                    e.ManipulationOrigin.X,
                                    e.ManipulationOrigin.Y);
                /*
                matrix.RotateAt(e.DeltaManipulation.Rotation,
                                e.ManipulationOrigin.X,
                                e.ManipulationOrigin.Y);
                    */

                matrix.Translate(e.DeltaManipulation.Translation.X,
                                    e.DeltaManipulation.Translation.Y);

                //we will do special magic to Disk so that it will be unnaffected by these transforms, we only want disk to be affected
                element.RenderTransform = new MatrixTransform(matrix);

                if (element is Disk) (element as Disk).NotifyMovement();
            }
            e.Handled = true;
        }

        private void Window_TouchDown_1(object sender, TouchEventArgs e)
        {
            #region User created Disk Tim 10/30/13
            if (!(e.Source is Disk))
            {
                spawnLocation = e.GetTouchPoint(this).Position;
                spawnTimer.Start();
            }
            #endregion
        }

        [STAThread]
        void spawnTimer_Elapsed_SpawnNewDisk(object sender, ElapsedEventArgs e)
        {
            //this.Dispatcher.Invoke(() =>
            //{
            //    Disk newDisk = new Disk();
            //    newDisk.Width = newDisk.Height = 300;
            //    //lets us move it through manipulationdelta
            //    newDisk.IsManipulationEnabled = true;
            //    //init default matrix
            //    var matrix = Matrix.Identity;
            //    //move it to where the touch initiated
            //    matrix.Translate(spawnLocation.X-newDisk.Width/2, spawnLocation.Y-newDisk.Height/2);
            //    //apply our new matrix by changing the rendertransform to a new transform.
            //    newDisk.RenderTransform = new MatrixTransform(matrix);
            //    newDisk.Margin = new Thickness(0);
            //    newDisk.LayoutTransform = new MatrixTransform();
            //    //insert it into the grid, index matters so it isnt under anything blocking input, and it cant be above animation canvas for now
            //    grid_Main.Children.Insert(1,newDisk);
            //    newDisk.CreateCursor(animationCanvas);
            //});
        }

        private void Window_TouchUp_1(object sender, TouchEventArgs e)
        {
            spawnTimer.Stop();
        }

        private void Window_TouchMove_1(object sender, TouchEventArgs e)
        {
            //doesnt work as intented
            //spawnTimer.Stop();
            //alternative
            Point p = e.GetTouchPoint(this).Position;
            float distance = (float)(p - spawnLocation).Length;
            if (distance > 50)
                spawnTimer.Stop();
        }

        private void brush_btn_Click(object sender, RoutedEventArgs e)
        {
            ald.myPen.Thickness = 10;
            switch (colorIndex)
            {

                case 0:
                    ald.myPen.Brush = Brushes.CornflowerBlue;
                    break;
                case 1:
                    ald.myPen.Brush = Brushes.Black;
                    break;
                case 2:
                    ald.myPen.Brush = Brushes.DarkGray;
                    break;
                case 3:
                    ald.myPen.Brush = Brushes.Green;
                    break;
                case 4:
                    ald.myPen.Brush = Brushes.Gold;
                    break;
                case 5:
                    ald.myPen.Brush = Brushes.HotPink;
                    break;
                case 6:
                    ald.myPen.Brush = Brushes.Red;
                    break;
                case 7:
                    ald.myPen.Brush = Brushes.Orange;
                    break;
            }
        }

        private void erase_btn_Click(object sender, RoutedEventArgs e)
        {
            ald.myPen.Brush = Brushes.White;
            ald.myPen.Thickness = 50;
        }

        private void color_btn_Click(object sender, RoutedEventArgs e)
        {
            colorIndex++;

            if (colorIndex > 7)
                colorIndex = 0;

            switch (colorIndex)
            {

                case 0:
                    ald.myPen.Brush = Brushes.CornflowerBlue;
                    break;
                case 1:
                    ald.myPen.Brush = Brushes.Black;
                    break;
                case 2:
                    ald.myPen.Brush = Brushes.DarkGray;
                    break;
                case 3:
                    ald.myPen.Brush = Brushes.Green;
                    break;
                case 4:
                    ald.myPen.Brush = Brushes.Gold;
                    break;
                case 5:
                    ald.myPen.Brush = Brushes.HotPink;
                    break;
                case 6:
                    ald.myPen.Brush = Brushes.Red;
                    break;
                case 7:
                    ald.myPen.Brush = Brushes.Orange;
                    break;
            }

            color_btn.Background = ald.myPen.Brush;

            backRec.Fill = ald.myPen.Brush;
        }
    }
}
