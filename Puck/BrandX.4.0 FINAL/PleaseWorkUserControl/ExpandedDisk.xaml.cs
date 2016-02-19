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
    /// Interaction logic for ExpandedDisk.xaml
    /// </summary>
    public partial class ExpandedDisk : UserControl
    {
        public List<Tool> Tools; //a list to initialize tools into
        public List<Button> ToolButtons; //a list to keep track of all tool buttons so we can operate on them freely

        #region Properties

        public Tool currentTool;
        double Radius { get { return circle.Width / 2; } }
        public Disk MainDisk { get { return ((grid_Main.Parent as ExpandedDisk).Parent as Grid).Parent as Disk; } }

        #endregion

        public ExpandedDisk()
        {
            InitializeComponent();
            InitializeTools();
        }

        //this is where all the tools are added to the ring/disk
        #region Initialize Tools

        //this is where u add tools
        void InitializeTools()
        {
            Tools = new List<Tool>();
            Tools.Add(new MinimizeTool(this));
            Tools.Add(new PenTool(this));
            Tools.Add(new BrushTool(this));
            Tools.Add(new EraserTool(this));
            Tools.Add(new ColorTool(this));
            ButtonizeTools();
            currentTool = null;// Tools[1];
        }
        //this function handles making wpf buttons for the tools
        void ButtonizeTools()
        {
            ToolButtons = new List<Button>();
            double angleChange = (Math.PI * 2) / Tools.Count; //The amount we rotate before placing a new button
            int index = -1;//start at negative one so we can increment INSTANTLY to get 0, just m3akes it more readable to have the index increment up front IMO
            foreach(Tool tool in Tools)
            {
                index++;
                tool.Index = index;
                //center then push away by cos and sin
                double newX = Math.Cos(angleChange*index) * (Radius - 30);
                double newY = Math.Sin(angleChange*index) * (Radius - 30);
                Button newButton = new Button();
                //newButton.Content = tool.Name;
                newButton.Background = new ImageBrush(new BitmapImage(new Uri(tool.IconUri, UriKind.Relative)));
                newButton.Width = 50;
                newButton.Height = 50;
                newButton.Name = "tool" + index;
                newButton.Visibility = System.Windows.Visibility.Collapsed;
                //newButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                //newButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                newButton.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                newButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                newButton.BorderThickness = new Thickness(1, 4, 1, 4);
                newButton.BorderBrush = Brushes.Transparent;
                newButton.TouchDown += (object sender, TouchEventArgs e) =>
                {
                    tool.Click();
                    currentTool = tool;
                    if (tool is MinimizeTool) MainDisk.cursor.selectTool(-1);
                    else MainDisk.cursor.selectTool(tool.Index);
                    FocusManager.SetFocusedElement(this, null);
                };
                ToolButtons.Add(newButton);
                grid_Main.Children.Add(newButton);
                /*MainDisk.mainWindow.eitime.CustomizableUpdate += new EventHandler((object o, EventArgs e) => {

                });*/
            }
        }

        #endregion

        //Miscellaneous methods, generally they are to make life easier when using the class
        #region Helper Functions

        /// <summary>
        /// Find the first tool with a matching name.
        /// </summary>
        /// <param name="Name">Name</param>
        /// <returns>The first match</returns>
        public Tool FindTool(string Name)
        {
            // loop through all the tools
            for (int i = 0; i < Tools.Count; i++)
            {
                //find a matching name?
                if (Tools[i].Name == Name)
                    return Tools[i]; //take the first one we find and get the hell out of this function
            }
            //just return null, it will throw an error and this should never be the outcome
            return null;
        }

        #endregion
    }
}
