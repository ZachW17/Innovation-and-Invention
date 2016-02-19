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

namespace ImpTest
{
    // the base class for all things animating
    public class AnimationObject
    {
        // time stuff
        public int durationMS;
        public int elapsedMS;

        // containing information for the lines this object uses
        public List<Line> lines;
        public List<Line> reserveLines;

        // a reference to the grid
        public Canvas canvas;

        // THUDERCATS ARE GOOOOOOOO
        public AnimationObject(Canvas canvas)
        {
            elapsedMS = 0;
            lines = new List<Line>();
            reserveLines = new List<Line>();

            this.canvas = canvas;
        }

        public virtual void Update(int dt) { }
    }

    // whose line is it anyway
    public class Cursor : AnimationObject
    {
        // look at all these nice variables
        public int lpc; // lines per circle
        public int numTools; // number of partitions
        public float radius; // the radius of the cursor
        public float futureRadius; // used for scaling the radius smoothly
        public float interiorOffset; // the offset of the inner circle from the outer
        private Point center; // the center to draw from
        public double circleAlpha; // the alpha of the circle (for idling) (from 0-1)

        public int idleMS;
        public int idleTimeOut;

        public double fadeCap;
        public double fadeRate;
        public bool shouldFade;
        public bool canWakeUp; //Tim: Added so that i can keep it transparent while being moved
        //canSleep DOESNT WORK
        public bool canSleep;  //Tim: Added so that I can take my time with the cursor and not have it go to sleep

        public bool hasFan;
        public int fanPanels;
        public int fanIndex;

        public List<Point> iconSpot;
        public List<Point> fanIcons;
        public Brush[] drawColor;

        public bool toolSelected;
        public int indexToolSelected;
        public List<Line> selectLines;

        public enum State { Opening, Scaling, Idle }
        public State state;

        public Action<double, double> fadeFunction { get; set; }

        public Point Center
        {
            get { return center; }
            set
            {
                center = value;
                wakeUp();
            }
        }

        // initialize with all these nice params
        public Cursor(int lpc, int numTools, float radius, Point center, Canvas canvas, int durationMS)
            : base(canvas)
        {
            // vars blah blah blabvalhlhalhlalfglhgl
            this.lpc = lpc;
            this.numTools = numTools;
            this.radius = radius;
            this.center = center;
            this.durationMS = durationMS;

            // init stuff
            iconSpot = new List<Point>();
            updateIconSpots(numTools);
            circleAlpha = 1;
            interiorOffset = 10;
            idleMS = 0;
            idleTimeOut = 1500;
            canWakeUp = true;

            fanIcons = new List<Point>();

            selectLines = new List<Line>();

            for (int i = 0; i < (float)lpc / numTools * 2 + 2; i++)
            {
                Line myLine = new Line();

                // visibility stuff
                myLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 150, 255, 255));
                myLine.StrokeThickness = 2;

                // init
                myLine.X1 = myLine.Y1 = myLine.X2 = myLine.Y2 = 0;
                selectLines.Add(myLine);

                canvas.Children.Add(myLine);
            }

            drawColor = new Brush[4];

            for (int i = 0; i < 4; i++)
            {
                drawColor[i] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                canSleep = true;
            }

            // populado el bablado rodrigues
            for (int i = 0; i < lpc * 2 + 10; i++)
            {
                Line myLine = new Line();

                // visibility stuff
                myLine.Stroke = new SolidColorBrush(Color.FromArgb((byte)(circleAlpha * 255), 0, 0, 0));
                myLine.StrokeThickness = 2;

                // init
                myLine.X1 = myLine.Y1 = myLine.X2 = myLine.Y2 = 0;
                lines.Add(myLine);

                canvas.Children.Add(myLine);
            }

            for (int i = 0; i < 50; i++)
            {
                Line myLine = new Line();

                // visibility stuff
                myLine.Stroke = new SolidColorBrush(Color.FromArgb((byte)(circleAlpha * 255), 0, 0, 0));
                myLine.StrokeThickness = 2;

                // init
                myLine.X1 = myLine.Y1 = myLine.X2 = myLine.Y2 = 0;
                reserveLines.Add(myLine);

                canvas.Children.Add(myLine);
            }

            futureRadius = 0;

            state = State.Opening;
        }

        // do your stuff mang
        public override void Update(int dt)
        {
            return;
            if (shouldFade) { fade(); }

            // dynamic alpha
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].Stroke.Opacity = circleAlpha;
            }
            for (int i = 0; i < reserveLines.Count; i++)
            {
                reserveLines[i].Stroke.Opacity = circleAlpha;
            }
            for (int i = 0; i < selectLines.Count; i++)
            {
                selectLines[i].Stroke.Opacity = circleAlpha;
            }

            // elapsed time in milliseconds
            if (elapsedMS < durationMS)
            {
                elapsedMS += dt;
            }
            else
            {
                elapsedMS = durationMS;
            }

            // time coefficient
            float timeCoef, lbc1, lbc2 = 0;

            timeCoef = (float)elapsedMS / durationMS;

            if (timeCoef >= 1)
            {
                timeCoef = 1;

                if (state == State.Scaling) radius = futureRadius;

                state = State.Idle;
            }

            // put that ALU to good use
            switch (state)
            {
                #region open
                case State.Opening:

                    int i;

                    for (i = 0; i < lpc; i++)
                    {
                        lbc1 = (float)i / lpc;
                        lbc2 = (float)(i + 1) / lpc;

                        lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 * timeCoef) * radius * timeCoef * (lbc1 + (1 - lbc1) * timeCoef);
                        lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 * timeCoef) * radius * timeCoef * (lbc1 + (1 - lbc1) * timeCoef);

                        lines[i].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2 * timeCoef) * radius * timeCoef * (lbc2 + (1 - lbc2) * timeCoef);
                        lines[i].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2 * timeCoef) * radius * timeCoef * (lbc2 + (1 - lbc2) * timeCoef);

                        //canvas.Children.Add(lines[i]);
                    }

                    for (i = lpc; i < lpc * 2; i++)
                    {
                        lbc1 = (float)(i - lpc) / lpc;
                        lbc2 = (float)(i + 1 - lpc) / lpc;

                        lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 * timeCoef) * (radius - interiorOffset) * timeCoef * (lbc1 + (1 - lbc1) * timeCoef);
                        lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 * timeCoef) * (radius - interiorOffset) * timeCoef * (lbc1 + (1 - lbc1) * timeCoef);

                        lines[i].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2 * timeCoef) * (radius - interiorOffset) * timeCoef * (lbc2 + (1 - lbc2) * timeCoef);
                        lines[i].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2 * timeCoef) * (radius - interiorOffset) * timeCoef * (lbc2 + (1 - lbc2) * timeCoef);

                        //canvas.Children.Add(lines[i]);
                    }

                    for (i = 2 * lpc; i < 2 * lpc + numTools; i++)
                    {
                        lbc1 = (float)(i + 1 - 2 * lpc) / numTools;

                        lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 * timeCoef) * (radius - interiorOffset) * timeCoef * (lbc1 + (1 - lbc1) * timeCoef);
                        lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 * timeCoef) * (radius - interiorOffset) * timeCoef * (lbc1 + (1 - lbc1) * timeCoef);

                        lines[i].X2 = center.X + Math.Cos(lbc1 * Math.PI * 2 * timeCoef) * radius * timeCoef * (lbc1 + (1 - lbc1) * timeCoef);
                        lines[i].Y2 = center.Y + Math.Sin(lbc1 * Math.PI * 2 * timeCoef) * radius * timeCoef * (lbc1 + (1 - lbc1) * timeCoef);

                        //canvas.Children.Add(lines[i]);
                    }
                    break;
                #endregion
                #region scale
                case State.Scaling:
                    float cubeCoef = timeCoef * timeCoef * timeCoef;

                    for (i = 0; i < lpc; i++)
                    {
                        lbc1 = (float)i / lpc;
                        lbc2 = (float)(i + 1) / lpc;

                        lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2) * (radius * (1 - cubeCoef) + futureRadius * cubeCoef);
                        lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2) * (radius * (1 - cubeCoef) + futureRadius * cubeCoef);

                        lines[i].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2) * (radius * (1 - cubeCoef) + futureRadius * cubeCoef);
                        lines[i].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2) * (radius * (1 - cubeCoef) + futureRadius * cubeCoef);

                        //canvas.Children.Add(lines[i]);
                    }

                    for (i = lpc; i < lpc * 2; i++)
                    {
                        lbc1 = (float)(i - lpc) / lpc;
                        lbc2 = (float)(i + 1 - lpc) / lpc;

                        lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2) * ((radius) * (1 - cubeCoef) + futureRadius * cubeCoef - interiorOffset);
                        lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2) * ((radius) * (1 - cubeCoef) + futureRadius * cubeCoef - interiorOffset);

                        lines[i].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2) * ((radius) * (1 - cubeCoef) + futureRadius * cubeCoef - interiorOffset);
                        lines[i].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2) * ((radius) * (1 - cubeCoef) + futureRadius * cubeCoef - interiorOffset);

                        //canvas.Children.Add(lines[i]);
                    }

                    for (i = 2 * lpc; i < 2 * lpc + numTools; i++)
                    {
                        lbc1 = (float)(i + 1 - 2 * lpc) / numTools;

                        lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 + Math.PI * 2 * cubeCoef * (2 / (float)numTools)) * ((radius) * (1 - cubeCoef) + futureRadius * cubeCoef - interiorOffset);
                        lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 + Math.PI * 2 * cubeCoef * (2 / (float)numTools)) * ((radius) * (1 - cubeCoef) + futureRadius * cubeCoef - interiorOffset);

                        lines[i].X2 = center.X + Math.Cos(lbc1 * Math.PI * 2 + Math.PI * 2 * cubeCoef * (2 / (float)numTools)) * (radius * (1 - cubeCoef) + futureRadius * cubeCoef);
                        lines[i].Y2 = center.Y + Math.Sin(lbc1 * Math.PI * 2 + Math.PI * 2 * cubeCoef * (2 / (float)numTools)) * (radius * (1 - cubeCoef) + futureRadius * cubeCoef);

                        //canvas.Children.Add(lines[i]);
                    }
                    break;
                #endregion
                #region idle
                case State.Idle:
                    if (!hasFan)
                    {
                        idleMS += dt;
                    }

                    if (idleMS > idleTimeOut)
                    {
                        if (canSleep) //accounting for this boolean
                            beginFade(.1, .08);
                    }

                    for (i = 0; i < lpc; i++)
                    {
                        lbc1 = (float)i / lpc;
                        lbc2 = (float)(i + 1) / lpc;

                        lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2) * radius;
                        lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2) * radius;

                        lines[i].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2) * radius;
                        lines[i].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2) * radius;

                        //canvas.Children.Add(lines[i]);
                    }

                    for (i = lpc; i < lpc * 2; i++)
                    {
                        lbc1 = (float)(i - lpc) / lpc;
                        lbc2 = (float)(i + 1 - lpc) / lpc;

                        lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2) * (radius - interiorOffset);
                        lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2) * (radius - interiorOffset);

                        lines[i].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2) * (radius - interiorOffset);
                        lines[i].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2) * (radius - interiorOffset);

                        //canvas.Children.Add(lines[i]);
                    }

                    for (i = 2 * lpc; i < 2 * lpc + numTools; i++)
                    {
                        lbc1 = (float)(i + 1 - 2 * lpc) / numTools;

                        //ready
                        if (idleMS < idleTimeOut)
                        {
                            lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2) * (radius - interiorOffset);
                            lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2) * (radius - interiorOffset);

                            lines[i].X2 = center.X + Math.Cos(lbc1 * Math.PI * 2) * radius;
                            lines[i].Y2 = center.Y + Math.Sin(lbc1 * Math.PI * 2) * radius;
                        }
                        //idle
                        else
                        {
                            lines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset);
                            lines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset);

                            lines[i].X2 = center.X + Math.Cos(lbc1 * Math.PI * 2 + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * radius;
                            lines[i].Y2 = center.Y + Math.Sin(lbc1 * Math.PI * 2 + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * radius;
                        }

                        //canvas.Children.Add(lines[i]);
                    }
                    #region selection
                    for (i = 0; i < selectLines.Count / 2; i++)
                    {
                        if (toolSelected)
                        {
                            lbc1 = (float)(i) / (selectLines.Count / 2 - 1);
                            lbc2 = (float)(i + 1) / (selectLines.Count / 2 - 1);

                            if (idleMS < idleTimeOut)
                            {
                                if (i < selectLines.Count / 2 - 1)
                                {
                                    selectLines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - 2);
                                    selectLines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - 2);

                                    selectLines[i].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - 2);
                                    selectLines[i].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - 2);

                                    //canvas.Children.Add(selectLines[i]);

                                    selectLines[i + selectLines.Count / 2].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - interiorOffset + 2);
                                    selectLines[i + selectLines.Count / 2].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - interiorOffset + 2);

                                    selectLines[i + selectLines.Count / 2].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - interiorOffset + 2);
                                    selectLines[i + selectLines.Count / 2].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - interiorOffset + 2);

                                    //canvas.Children.Add(selectLines[i + selectLines.Count / 2]);
                                }
                                else
                                {
                                    selectLines[i].X1 = center.X + Math.Cos(0.01 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - 2);
                                    selectLines[i].Y1 = center.Y + Math.Sin(0.01 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - 2);

                                    selectLines[i].X2 = center.X + Math.Cos(0.02 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - interiorOffset + 2);
                                    selectLines[i].Y2 = center.Y + Math.Sin(0.02 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - interiorOffset + 2);

                                    //canvas.Children.Add(selectLines[i]);

                                    selectLines[i + selectLines.Count / 2].X1 = center.X + Math.Cos(0.99 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - 2);
                                    selectLines[i + selectLines.Count / 2].Y1 = center.Y + Math.Sin(0.99 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - 2);

                                    selectLines[i + selectLines.Count / 2].X2 = center.X + Math.Cos(0.98 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - interiorOffset + 2);
                                    selectLines[i + selectLines.Count / 2].Y2 = center.Y + Math.Sin(0.98 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools) * (radius - interiorOffset + 2);

                                    //canvas.Children.Add(selectLines[i + selectLines.Count / 2]);
                                }
                            }
                            else
                            {
                                if (i < selectLines.Count / 2 - 1)
                                {
                                    selectLines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - 2);
                                    selectLines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - 2);

                                    selectLines[i].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - 2);
                                    selectLines[i].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - 2);

                                    //canvas.Children.Add(selectLines[i]);

                                    selectLines[i + selectLines.Count / 2].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset + 2);
                                    selectLines[i + selectLines.Count / 2].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset + 2);

                                    selectLines[i + selectLines.Count / 2].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset + 2);
                                    selectLines[i + selectLines.Count / 2].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset + 2);

                                    //canvas.Children.Add(selectLines[i + selectLines.Count / 2]);
                                }
                                else
                                {
                                    selectLines[i].X1 = center.X + Math.Cos(0.01 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - 2);
                                    selectLines[i].Y1 = center.Y + Math.Sin(0.01 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - 2);

                                    selectLines[i].X2 = center.X + Math.Cos(0.02 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset + 2);
                                    selectLines[i].Y2 = center.Y + Math.Sin(0.02 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset + 2);

                                    //canvas.Children.Add(selectLines[i]);

                                    selectLines[i + selectLines.Count / 2].X1 = center.X + Math.Cos(0.99 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - 2);
                                    selectLines[i + selectLines.Count / 2].Y1 = center.Y + Math.Sin(0.99 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - 2);

                                    selectLines[i + selectLines.Count / 2].X2 = center.X + Math.Cos(0.98 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset + 2);
                                    selectLines[i + selectLines.Count / 2].Y2 = center.Y + Math.Sin(0.98 * Math.PI * 2 / numTools + Math.PI * 2 * indexToolSelected / numTools + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset + 2);

                                    //canvas.Children.Add(selectLines[i + selectLines.Count / 2]);
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                #endregion
            }

            #region fan
            // FANNING OUT =)
            if (hasFan)
            {
                // initial extension of line from disk
                if (elapsedMS < durationMS * .3333)
                {
                    reserveLines[0].X1 = center.X + Math.Cos(Math.PI * 2 * fanIndex / numTools) * radius;
                    reserveLines[0].Y1 = center.Y + Math.Sin(Math.PI * 2 * fanIndex / numTools) * radius;

                    reserveLines[0].X2 = center.X + Math.Cos(Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset * timeCoef * 3);
                    reserveLines[0].Y2 = center.Y + Math.Sin(Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset * timeCoef * 3);

                    //canvas.Children.Add(reserveLines[0]);
                }
                // errthang else
                else
                {
                    double timeCoef2 = (timeCoef - .3333) / .6667;

                    // dont get rid ofthat first line
                    reserveLines[0].X1 = center.X + Math.Cos(Math.PI * 2 * fanIndex / numTools) * radius;
                    reserveLines[0].Y1 = center.Y + Math.Sin(Math.PI * 2 * fanIndex / numTools) * radius;

                    reserveLines[0].X2 = center.X + Math.Cos(Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset);
                    reserveLines[0].Y2 = center.Y + Math.Sin(Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset);

                    // the second sliding line
                    reserveLines[1].X1 = center.X + Math.Cos(Math.PI * 2 * (fanIndex + 1 * timeCoef2) / numTools) * radius;
                    reserveLines[1].Y1 = center.Y + Math.Sin(Math.PI * 2 * (fanIndex + 1 * timeCoef2) / numTools) * radius;

                    reserveLines[1].X2 = center.X + Math.Cos(Math.PI * 2 * (fanIndex + 1 * timeCoef2) / numTools) * (radius + interiorOffset);
                    reserveLines[1].Y2 = center.Y + Math.Sin(Math.PI * 2 * (fanIndex + 1 * timeCoef2) / numTools) * (radius + interiorOffset);

                    // the lines that separate the options
                    for (int i = 2; i < 1 + fanPanels; i++)
                    {
                        if (timeCoef2 >= (i - 1f) / fanPanels)
                        {
                            reserveLines[i].X1 = center.X + Math.Cos(Math.PI * 2 * (fanIndex + 1f * (i - 1) / fanPanels) / numTools) * radius;
                            reserveLines[i].Y1 = center.Y + Math.Sin(Math.PI * 2 * (fanIndex + 1f * (i - 1) / fanPanels) / numTools) * radius;

                            reserveLines[i].X2 = center.X + Math.Cos(Math.PI * 2 * (fanIndex + 1f * (i - 1) / fanPanels) / numTools) * (radius + interiorOffset);
                            reserveLines[i].Y2 = center.Y + Math.Sin(Math.PI * 2 * (fanIndex + 1f * (i - 1) / fanPanels) / numTools) * (radius + interiorOffset);

                            //canvas.Children.Add(reserveLines[i]);
                        }
                    }

                    // the outer ring 
                    for (int i = 1 + fanPanels; i < 1 + fanPanels + 10; i++)
                    {
                        lbc1 = (float)(i - 1 - fanPanels) / 10;
                        lbc2 = (float)(i + 1 - 1 - fanPanels) / 10;

                        reserveLines[i].X1 = center.X + Math.Cos(lbc1 * Math.PI * 2 / numTools * timeCoef2 + Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset);
                        reserveLines[i].Y1 = center.Y + Math.Sin(lbc1 * Math.PI * 2 / numTools * timeCoef2 + Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset);

                        reserveLines[i].X2 = center.X + Math.Cos(lbc2 * Math.PI * 2 / numTools * timeCoef2 + Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset);
                        reserveLines[i].Y2 = center.Y + Math.Sin(lbc2 * Math.PI * 2 / numTools * timeCoef2 + Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset);

                        //canvas.Children.Add(reserveLines[i]);
                    }

                    //canvas.Children.Add(reserveLines[0]);
                    if (fanPanels > 0)
                    {
                        //canvas.Children.Add(reserveLines[1]);
                    }
                }
            }
            #endregion

            updateIconSpots(numTools);
        }

        public void smoothScale(float futureRadius, int duration = 750)
        {
            if (!hasFan)
            {
                this.futureRadius = futureRadius;
                state = State.Scaling;

                wakeUp();
                elapsedMS = 0;
                durationMS = duration;
            }
        }

        public void changeRingColor(byte r, byte g, byte b)
        {
            SolidColorBrush colorTo = new SolidColorBrush(Color.FromArgb((byte)(circleAlpha * 255), r, g, b));

            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].Stroke = colorTo;
            }
            for (int i = 0; i < reserveLines.Count; i++)
            {
                reserveLines[i].Stroke = colorTo;
            }
        }

        public void addFan(int indexOfFan, int numPanels, int duration = 1200)
        {
            if (numPanels > 0)
            {
                if (numPanels > 38) numPanels = 38;
                hasFan = true;
                fanPanels = numPanels;
                fanIndex = indexOfFan;

                wakeUp();
                elapsedMS = 0;
                durationMS = duration;

                fanIcons.Clear();

                for (int i = 0; i < numPanels; i++)
                {
                    double lbc1 = (i + .5) / numPanels;
                    Point iconPosition = new Point();

                    iconPosition.X = center.X + Math.Cos(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset / 2);
                    iconPosition.Y = center.Y + Math.Sin(lbc1 * Math.PI * 2 / numTools + Math.PI * 2 * fanIndex / numTools) * (radius + interiorOffset / 2);

                    fanIcons.Add(iconPosition);
                }
            }
        }

        public void removeFan()
        {
            hasFan = false;
        }
        public void fade()
        {
            if (fadeRate > 0)
            {
                if (circleAlpha > fadeCap)
                {
                    circleAlpha -= fadeRate;
                }
                if (circleAlpha < fadeCap)
                {
                    circleAlpha = fadeCap;
                    shouldFade = false;
                }
            }
            else
            {
                if (circleAlpha < fadeCap)
                {
                    circleAlpha -= fadeRate;
                }
                if (circleAlpha > fadeCap)
                {
                    circleAlpha = fadeCap;
                    shouldFade = false;
                }
            }
        }

        public void beginFade(double cap, double rate)
        {
            fadeCap = cap;
            fadeRate = rate;
            shouldFade = true;
        }

        public void wakeUp()
        {
            if (canWakeUp)
            {
                idleMS = 0;
                beginFade(1.0, -.05);
            }
        }

        // for updating the iconspots list with the locations for where the icons should go.
        // you can use this method to change the number of tools as well
        // TODO: animate this
        public void updateIconSpots(int numTools)
        {
            iconSpot.Clear();

            this.numTools = numTools;

            for (float i = 0; i < numTools; i++)
            {
                Point p;

                if (idleMS < idleTimeOut)
                {
                    p = new Point(
                    center.X + Math.Cos((i + .5) / numTools * 2 * Math.PI) * (radius - interiorOffset / 2f),
                    center.Y + Math.Sin((i + .5) / numTools * 2 * Math.PI) * (radius - interiorOffset / 2f));
                }
                //idle
                else
                {
                    p = new Point(
                    center.X + Math.Cos((i + .5) / numTools * 2 * Math.PI + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset / 2f),
                    center.Y + Math.Sin((i + .5) / numTools * 2 * Math.PI + ((idleMS - idleTimeOut) / 3000f) % (2 * Math.PI)) * (radius - interiorOffset / 2f));
                }
                iconSpot.Add(p);
            }
        }

        public void selectTool(int index)
        {
            /*
            if (index == -1)
            {
                toolSelected = false;
            }
            else
            {
             * */
                toolSelected = true;

                indexToolSelected = index;
            //}
        }
    }

    // special time class containing our special sauce
    public class EITime
    {
        public int lastMS; // variable to make sure deltaTime calculates the way we expect it to
        public System.Windows.Threading.DispatcherTimer time; // the C# thing that makes it all work

        public int deltaTime; // time since the last update

        public Canvas canvas;

        public Action<int> updateMethod { get; set; } // sexy C# delegates yum yum (good luck ive never used these)

        //eventhandler has the += operator so people can add stuff onto update openly
        //maybe we can use the event args to make the useless parameters 'func(Object Sender, EventArgs E)' useful
        //Object sender is already potentially useful
        public EventHandler CustomizableUpdate;

        // TRICKY
        // param1: you need to pass in the update method for this EITime instance to call when its timer ticks.
        // param2: pass in the rough ms delay for the updates to occur (default 25)
        public EITime(Action<int> update, Canvas canvas, int msTick = 25)
        {
            // get the reference for the canvas to clear
            this.canvas = canvas;

            // set up the TICKER
            time = new System.Windows.Threading.DispatcherTimer();
            time.Tick += new EventHandler(dispatcherTimer_Tick);
            time.Interval = new TimeSpan(0, 0, 0, 0, msTick);
            time.Start();

            // what we made this class for (pretty much)
            // update times,
            lastMS = 0;
            deltaTime = 0;

            // and allocating the update method.
            updateMethod = update;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ForceUpdate();
        }
        public void ForceUpdate()
        {
            // every good boy deserves fudge
            //canvas.Children.Clear();

            // DateTime.Now.Millisecond goes from 0-999. so, we need to calculate
            // how much we missed when it overflow..ed
            if (lastMS > DateTime.Now.Millisecond)
            {
                // dt = the remaining portion of the last second and portion of
                // the current second
                deltaTime = 1000 - lastMS + DateTime.Now.Millisecond;
            }
            else
            {
                deltaTime = DateTime.Now.Millisecond - lastMS;
            }

            // tells whatever method you gave it to do it.
            updateMethod(deltaTime);

            //I believe += will make CustomizableUpdate not null
            //so although we never construct it, this if statement covers us
            if (CustomizableUpdate != null)
                CustomizableUpdate(this, null); //this is the sender, could be useful.
            //Event Args is null for now, change at leisure.

            // for next time
            lastMS = DateTime.Now.Millisecond;

            // Forcing the CommandManager to raise the RequerySuggested event
            // the only remaining line i copy pasted.. copyright????
            CommandManager.InvalidateRequerySuggested();
        }
    }

    // holds all the movey things
    public class AnimationManager
    {
        public List<AnimationObject> objects; // MY BABIESSS
        public int numCursors; // the number of cursors allowed

        public AnimationManager()
        {
            objects = new List<AnimationObject>();
        }

        // updates all the animation objects. this was designed to be passed to EITime
        public void batchUpdate(int dt)
        {
            if (objects.Count > 0)
            {
                // loop through updates for each animating object
                for (int i = 0; i < objects.Count; i++)
                {
                    objects[i].Update(dt);
                }
            }
        }

        // shortcut
        public Cursor addCursor(int lpc, int numTools, float radius, Point center, Canvas canvas, int durationMS)
        {
            Cursor newCursor = new Cursor(lpc, numTools, radius, center, canvas, durationMS);

            objects.Add(newCursor);
            return newCursor;
        }

        // #LAST if something goes rogue its probably this ?
        public void removeCursor(int indexOfCursor)
        {
            (objects[indexOfCursor] as Cursor).canvas.Children.Clear();

            objects.Remove(objects[indexOfCursor]);

            for (int i = 0; i < objects.Count; i++)
            {
                int j;

                for (j = 0; j < objects[i].lines.Count; j++)
                {
                    objects[i].canvas.Children.Add(objects[i].lines[j]);
                }

                for (j = 0; j < objects[i].reserveLines.Count; j++)
                {
                    objects[i].canvas.Children.Add(objects[i].reserveLines[j]);
                }

                for (j = 0; j < (objects[i] as Cursor).selectLines.Count; j++)
                {
                    objects[i].canvas.Children.Add((objects[i] as Cursor).selectLines[j]);
                }
            }
        }
    }
    /*commented out
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AnimationManager aMan;
        public EITime timer;

        public MainWindow()
        {
            InitializeComponent();

            aMan = new AnimationManager();
            timer = new EITime(aMan.batchUpdate, myCanvas);
        }

        public void MakeCursor(Object sender, EventArgs e)
        {
            aMan.addCursor(40, 4, 50, Point.Parse("100,100"), myCanvas, 800);
            debug.Content = (aMan.objects[0] as Cursor).state;
        }

        public void ScaleCursor(Object sender, EventArgs e)
        {
            (aMan.objects[0] as Cursor).smoothScale((aMan.objects[0] as Cursor).radius + 10);
        }

        public void ScaleCursor2(Object sender, EventArgs e)
        {
            (aMan.objects[0] as Cursor).smoothScale((aMan.objects[0] as Cursor).radius - 10);
        }

        public void UpdateDebug(Object sender, EventArgs e)
        {
            debug.Content = (aMan.objects[0] as Cursor).state;
        }

        public void marginUp(Object sender, EventArgs e)
        {
            (aMan.objects[0] as Cursor).interiorOffset+=5;
        }

        public void marginDown(Object sender, EventArgs e)
        {
            (aMan.objects[0] as Cursor).interiorOffset-=5;
        }

        public void AddFan0(Object sender, EventArgs e)
        {
            (aMan.objects[0] as Cursor).addFan(3,2);
            debug.Content = (aMan.objects[0] as Cursor).fanIcons[0].X;
        }

        public void RemoveFan0(Object sender, EventArgs e)
        {
            (aMan.objects[0] as Cursor).removeFan();
        }
    }
     */
}
