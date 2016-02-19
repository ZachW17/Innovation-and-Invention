using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PleaseWorkUserControl
{
    //potentiall make tool extend button
    //for now i think its better to abstract tool away from button
    /*10/16/2013
     * make a class called base tool that extends this and adds default values
     */
    public class Tool
    {
        public int Index;
        protected ExpandedDisk homeDisk;
        public Tool(ExpandedDisk Owner)
        {
            homeDisk = Owner;
            Pen_Unique = new Pen();
        }

        public virtual void Click()
        {
        }
        public  virtual void Deactivate()
        {
        }

        public virtual string IconUri { get { return "Default.png"; } }
        public virtual string Name { get { return "NULL"; } }
        public AttemptedLineDrawera Render { get { return homeDisk.MainDisk.mainWindow.ald; } }
        public Pen Pen_Unique;
    }
    public class MinimizeTool : Tool
    {
        public MinimizeTool(ExpandedDisk Owner)
            : base(Owner)
        {
        }

        public override void Click()
        {
            homeDisk.MainDisk.Collapse();
        }

        public override string Name
        {
            get
            {
                return "Minimize";
            }
        }

        public override string IconUri
        {
            get
            {
                return "BasicToolImages/Shrink_icon.png";
            }
        }
    }
    public class EraserTool : Tool
    {
        public EraserTool(ExpandedDisk Owner)
            :base(Owner)
        {
        }

        public override void Click()
        {
            Pen_Unique.Brush = Brushes.White;
            Pen_Unique.StartLineCap = PenLineCap.Round;
            Pen_Unique.EndLineCap = PenLineCap.Round;
            Pen_Unique.Thickness = 50;
        }

        public override string Name
        {
            get { return "Eraser"; }
        }

        public override string IconUri
        {
            get
            {
                return "BasicToolImages/Eraser_icon.png";
            }
        }
    }

    public class PenTool : Tool
    {
        public PenTool(ExpandedDisk Owner)
            : base(Owner)
        {
        }

        public override void Click()
        {
            Pen_Unique.Brush = Brushes.Black;
            Pen_Unique.StartLineCap = PenLineCap.Round;
            Pen_Unique.EndLineCap = PenLineCap.Round;
            Pen_Unique.Thickness = 3;
        }

        public override string Name
        {
            get { return "Pen"; }
        }

        public override string IconUri
        {
            get
            {
                return "BasicToolImages/pencil_icon[2].png";
            }
        }
    }

    public class BrushTool : Tool
    {
        public BrushTool(ExpandedDisk Owner)
            : base(Owner)
        {
            Pen_Unique.Brush = Brushes.Orange;
            Pen_Unique.StartLineCap = PenLineCap.Round;
            Pen_Unique.EndLineCap = PenLineCap.Round;
            Pen_Unique.Thickness = 30;
        }

        public override void Click()
        {
        }

        public override string Name
        {
            get { return "Brush"; }
        }

        public override string IconUri
        {
            get
            {
                return "BasicToolImages/brush_icon.png";
            }
        }
    }

    public class ColorTool : Tool
    {
        private int colorIndex = 0;

        public ColorTool(ExpandedDisk Owner)
            : base(Owner)
        {
        }

        public override void Click()
        {
            //we will sync this pen with the brush pen
            Pen_Unique = homeDisk.FindTool("Brush").Pen_Unique;
            //homeDisk.MainDisk.mainWindow.ald.ForceUpdate();

            //only change colors if we were already in brush mode
            //if (homeDisk.currentTool == this) colorIndex=(colorIndex+1)%8;
            colorIndex = (colorIndex + 1) % 8;

            switch(colorIndex)
            {
                
                case 0:
                    Pen_Unique.Brush = Brushes.CornflowerBlue;
                    break;
                case 1:
                    Pen_Unique.Brush = Brushes.Green;
                    break;
                case 2:
                    Pen_Unique.Brush = Brushes.Gold;
                    break;
                case 3:
                    Pen_Unique.Brush = Brushes.HotPink;
                    break;
                case 4:
                    Pen_Unique.Brush = Brushes.Red;
                    break;
                case 5:
                    Pen_Unique.Brush = Brushes.Orange;
                    break;
                case 6:
                    Pen_Unique.Brush = Brushes.DarkGray;
                    break;
                case 7:
                    Pen_Unique.Brush = Brushes.Black;
                    break;
            }

            (homeDisk.circle.Stroke as GradientBrush).GradientStops[0].Color = (Pen_Unique.Brush as SolidColorBrush).Color;
            Color col = (Pen_Unique.Brush as SolidColorBrush).Color;
            col.A = (homeDisk.circle.Stroke as GradientBrush).GradientStops[1].Color.A;
            (homeDisk.circle.Stroke as GradientBrush).GradientStops[1].Color = col;
            homeDisk.MainDisk.InnerCircle.Fill = Pen_Unique.Brush;
#region Getting an opposing color for the outline
            float h,s,l;
            Color color = (Pen_Unique.Brush as SolidColorBrush).Color;
            float r = (float)color.R / 255, g = (float)color.G / 255, b = (float)color.B / 255;
            //thank System.Drawing.Color for getHue
            System.Drawing.Color c = System.Drawing.Color.FromArgb(255,color.R, color.G,color.B);
            h = c.GetHue();
            s = c.GetSaturation();
            l = c.GetBrightness();
            h = (h+180)%360; //spin to win

            //find opposing color 
            Color oppositeColor = FromHSLA(h/360, l, s, 1.0);
            homeDisk.MainDisk.cursor.changeRingColor(oppositeColor.R, oppositeColor.G, oppositeColor.B);
            //bahhhhh forget it
            if(l > .5)
                homeDisk.MainDisk.cursor.changeRingColor(0, 0, 0);
            else
                homeDisk.MainDisk.cursor.changeRingColor(255, 255, 255);
#endregion
            
            Pen_Unique.StartLineCap = PenLineCap.Round;
            Pen_Unique.EndLineCap = PenLineCap.Round;
            Pen_Unique.Thickness = 30;
        }

#region http://stackoverflow.com/questions/8847760/convertation-rgb-bytes-to-hsl-and-back ColinE

        // Given H,S,L,A in range of 0-1
        // Returns a Color (RGB struct) in range of 0-255
        public static Color FromHSLA(double H, double S, double L, double A)
        {
            double v;
            double r, g, b;
            if (A > 1.0)
                A = 1.0;

            r = L;   // default to gray
            g = L;
            b = L;
            v = (L <= 0.5) ? (L * (1.0 + S)) : (L + S - L * S);
            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;

                m = L + L - v;
                sv = (v - m) / v;
                H *= 6.0;
                sextant = (int)H;
                fract = H - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            Color rgb = new Color();
            rgb.R = Convert.ToByte(r * 255.0f);
            rgb.G = Convert.ToByte(g * 255.0f);
            rgb.B = Convert.ToByte(b * 255.0f);
            rgb.A = Convert.ToByte(A * 255.0f);
            return rgb;
        }

#endregion

        public override string Name
        {
            get { return "Color"; }
        }

        //Need to get a different icon depending on the color picked
        public override string IconUri
        {
            get
            {
                return "BasicToolImages/Palette_icon.png";
            }
        }
    }
}
