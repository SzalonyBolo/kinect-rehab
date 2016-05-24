using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace kinectRehab
{
    /// <summary>
    /// Interaction logic for LevelCreator.xaml
    /// </summary>
    //
    public partial class LevelCreator : Window
    {
        IEnumerable<string> files;
        protected bool isDragging;
        private Point clickPosition;
        ElementOnBoard selected;
        int actualStep = 0;
        List<LevelStep> steps = new List<LevelStep>();
        int difficulty;
        string game;
        db.DbConnector dbConnector;

        public LevelCreator(string folder, int diff, db.DbConnector connector)
        {
            this.dbConnector = connector;
            this.difficulty = diff;
            this.game = folder;
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            int index = baseDir.IndexOf("kinectRehab");
            string dataDir = baseDir.Substring(0, index) + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            dataDir += "\\kinectRehab\\images\\" + folder;
            this.files = Directory.EnumerateFiles(dataDir)
             .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg"));
            var first = files.First();
            InitializeComponent();
            //makeTooth();
            makeComboList(files);
            cmbStep.Items.Add("1");
            steps.Add(new LevelStep());
            cmbStep.SelectedIndex = 0;
            levelCanvas.Background = null;
        }

        private void makeComboList(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                int schar = file.LastIndexOf('\\');
                comboBox.Items.Add(file.Substring(schar + 1));
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnChangeBackground_Click(object sender, RoutedEventArgs e)
        {
            string item = comboBox.SelectedItem.ToString();
            if (item != "")
            {
                foreach (string file in files)
                {
                    if (file.Contains(item))
                    {
                        ImageBrush imageBrush = new ImageBrush();
                        imageBrush.ImageSource = new BitmapImage(new Uri(file, UriKind.Absolute));
                        levelCanvas.Background = imageBrush;
                        steps[actualStep].backgroundFile = item;
                        return;
                    }
                }
            }
        }

        private void btnAddToCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedIndex == -1)
                return;
            string item = comboBox.SelectedItem.ToString();
            if (item != "")
            {
                item = "\\" + item;
                foreach (string file in files)
                {
                    if (file.Contains(item))
                    {
                        System.Drawing.Image newImage = System.Drawing.Image.FromFile(file);
                        float x = newImage.Width;
                        float y = newImage.Height;
                        Rectangle rec = new Rectangle();
                        rec.Height = y;
                        rec.Width = x;
                        ImageBrush imageBrush = new ImageBrush();
                        imageBrush.ImageSource = new BitmapImage(new Uri(file, UriKind.Absolute));
                        rec.Fill = imageBrush;
                        rec.MouseLeftButtonDown += dragRectangle_MouseLeftButtonDown;
                        rec.MouseLeftButtonUp += dragRectangle_MouseLeftButtonUp;
                        rec.MouseMove += dragRectangle_MouseMove;
                        int l = item.IndexOf('.');
                        rec.Name = item.Substring(1, l - 1);
                        ElementOnBoard element = new ElementOnBoard();
                        element.name = item.Substring(1, l - 1);
                        element.withName = item.Substring(1);
                        element.element = rec;
                        levelCanvas.Children.Add(element.element);
                        steps[actualStep].objects.Add(element);
                        cmbGrabableItem.Items.Add(element.name);
                        element.position[0] = Canvas.GetTop(element.element);
                        element.position[1] = Canvas.GetLeft(element.element);
                        element.isNew = true;
                        break;
                    }
                }
            }
        }

        private void dragRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var draggableControl = sender as Rectangle;
            lblSelected.Content = draggableControl.Name.ToString();
            clickPosition = e.GetPosition(this);
            draggableControl.CaptureMouse();
        }

        private void dragRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var draggable = sender as Rectangle;
            draggable.ReleaseMouseCapture();
            foreach (ElementOnBoard element in steps[actualStep].objects)
            {
                if (element.name.Equals(draggable.Name))
                {
                    element.element = draggable;
                    selected = element;
                    element.position[0] = Canvas.GetTop(draggable);
                    element.position[1] = Canvas.GetLeft(draggable);
                    break;
                }
            }
        }

        private void dragRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            var draggableControl = sender as Rectangle;

            if (isDragging && draggableControl != null)
            {
                Point currentPosition = e.GetPosition(this.Parent as UIElement);

                Canvas.SetTop(draggableControl, currentPosition.Y);
                Canvas.SetLeft(draggableControl, currentPosition.X);
            }   
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            List<string> orders = new List<string>();
            int istep = 0;
            orders.Add(game);
            orders.Add(difficulty.ToString());
            
            string order;
            foreach (LevelStep step in this.steps)
            {
                orders.Add("incrementstep " + istep);
               if (step.backgroundFile != "")
                {
                    orders.Add("chbg " + step.backgroundFile);
                }
               foreach (ElementOnBoard obj in step.objects)
                {
                    order = "";
                    if (obj.isNew)
                    {
                        order = "addtoelements " + obj.name + difficulty + " " + obj.element.Width + " " + obj.element.Height + " " + obj.withName;
                        orders.Add(order);
                        order = "addElement " + obj.name + difficulty + " " + obj.position[0] + " " + obj.position[1];
                        orders.Add(order);
                    }
                    if (obj.endStepState != ElementEndStepState.None && obj.endStepState != ElementEndStepState.ToGrabable)
                    {
                        order = "endStep " + obj.name + difficulty;
                        if (obj.endStepState == ElementEndStepState.Grab)
                            order += " grab";
                        if (obj.endStepState == ElementEndStepState.Push)
                            order += " push";
                        if (obj.endStepState == ElementEndStepState.Grabable)
                        {
                            foreach (ElementOnBoard o in step.objects)
                            {
                                if (o.endStepState == ElementEndStepState.ToGrabable)
                                {
                                    order += " grabable " + o.name + difficulty;
                                }
                            }
                        }
                    }
                    if (obj.invisible)
                        order += " inv noglow";

                    if (!order.Equals(""))
                        orders.Add(order);
                }
            }
            dbConnector.makeGame(orders);
            this.Close();
        }

        private void btnAddStep_Click(object sender, RoutedEventArgs e)
        {
            int lastStep = cmbStep.Items.Count - 1;
            steps.Add(new LevelStep(steps[lastStep]));
            cmbStep.Items.Add((lastStep + 2).ToString());
            cmbStep.SelectedIndex = lastStep + 1;
            actualStep = lastStep + 1;

        }

        private void btnDeleteStep_Click(object sender, RoutedEventArgs e)
        {
            int deletingIndex = cmbStep.SelectedIndex;
            if (deletingIndex > 0)
            {
                cmbStep.Items.RemoveAt(deletingIndex);
                cmbStep.SelectedIndex = 0;
                steps.RemoveAt(deletingIndex);
                actualStep = deletingIndex - 1;
            }
        }

        private void cmbStep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            actualStep = cmb.SelectedIndex;
            makeStepOnCanvas();
        }

        private void makeStepOnCanvas()
        {
            LevelStep stepToMake = steps[actualStep];
            if (stepToMake.backgroundFile != null)
            {
                foreach (string file in files)
                {
                    if (file.Contains(stepToMake.backgroundFile))
                    {
                        ImageBrush imageBrush = new ImageBrush();
                        imageBrush.ImageSource = new BitmapImage(new Uri(file, UriKind.Absolute));
                        levelCanvas.Background = imageBrush;
                        break;
                    }
                }
            }
            levelCanvas.Children.Clear();
            foreach (ElementOnBoard element in stepToMake.objects)
            {
                levelCanvas.Children.Add(element.element);
                Canvas.SetTop(element.element, element.position[0]);
                Canvas.SetLeft(element.element, element.position[1]);
                switch (element.endStepState)
                {
                    case ElementEndStepState.None:
                        break;

                    case ElementEndStepState.Grab:
                        lblEndStepSelected.Content = element.element.Name + " - Chwyt";
                        break;

                    case ElementEndStepState.Push:
                        lblEndStepSelected.Content = element.element.Name + " - Pchnięcie";
                        break;

                    case ElementEndStepState.Grabable:
                        lblEndStepSelected.Content = element.element.Name + " - Przeciągnięcie";
                        break;
                }
            }
        }

        public class LevelStep
        {
            public List<ElementOnBoard> objects = new List<ElementOnBoard>();
            public string backgroundFile = "";

            public LevelStep()
            {

            }

            public LevelStep(LevelStep levelStep)
            {
                this.backgroundFile = levelStep.backgroundFile;
                this.objects = new List<ElementOnBoard>();
                foreach (ElementOnBoard obj in levelStep.objects)
                {
                    ElementOnBoard copy = new ElementOnBoard();
                    copy.invisible = obj.invisible;
                    copy.invisibleToGrab = obj.invisibleToGrab;
                    copy.name = obj.name;
                    copy.position[0] = obj.position[0];
                    copy.position[1] = obj.position[1];
                    copy.endStepState = ElementEndStepState.None;
                    copy.element = obj.element;
                    copy.isNew = false;
                    this.objects.Add(copy);
                }
            }
        }

        public class ElementOnBoard
        {
            public Rectangle element;

            public bool invisible = false;
            public bool invisibleToGrab = false;
            public string name;
            public string withName;
            public double[] position = new double[2];
            public bool isNew = false;
            public bool isMoved = false;
            public ElementEndStepState endStepState = ElementEndStepState.None;
        }

        public enum ElementEndStepState
        {
            None,
            Push,
            Grab,
            Grabable,
            ToGrabable
        };

        private void cbxInvTo_Checked(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {
                foreach (ElementOnBoard element in steps[actualStep].objects)
                {
                    if (element.name.Equals(selected.name))
                    {
                        element.invisible = true;
                        break;
                    }
                }
            }
        }

        private void cbxInvTo_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {
                foreach (ElementOnBoard element in steps[actualStep].objects)
                {
                    if (element.name.Equals(selected.name))
                    {
                        element.invisible = false;
                        break;
                    }
                }
            }
        }

        private void cbxInvToGrab_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {
                foreach (ElementOnBoard element in steps[actualStep].objects)
                {
                    if (element.name.Equals(selected.name))
                    {
                        element.invisibleToGrab = false;
                        break;
                    }
                }
            }
        }

        private void cbxInvToGrab_Checked(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {
                foreach (ElementOnBoard element in steps[actualStep].objects)
                {
                    if (element.name.Equals(selected.name))
                    {
                        element.invisibleToGrab = true;
                        break;
                    }
                }
            }
        }

        private void btnGrab_Click(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {
                foreach (ElementOnBoard element in steps[actualStep].objects)
                {
                    if (element.name.Equals(selected.name))
                    {
                        element.endStepState = ElementEndStepState.Grab;
                        break;
                    }
                }
            }
        }

        private void btnPush_Click(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {
                foreach (ElementOnBoard element in steps[actualStep].objects)
                {
                    if (element.name.Equals(selected.name))
                    {
                        element.endStepState = ElementEndStepState.Push;
                        break;
                    }
                }
            }

        }

        private void btnGrabable_Click(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {
                if (cmbGrabableItem.SelectedIndex != -1)
                {
                    foreach (ElementOnBoard element in steps[actualStep].objects)
                    {
                        if (element.name.Equals(selected.name))
                        {
                            element.endStepState = ElementEndStepState.Grabable;
                            break;
                        }
                    }
                    foreach (ElementOnBoard element in steps[actualStep].objects)
                    {
                        if (element.name.Equals(cmbGrabableItem.SelectedItem))
                        {
                            element.endStepState = ElementEndStepState.ToGrabable;
                            break;
                        }
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (ElementOnBoard element in steps[actualStep].objects)
            {
                if (element.name.Equals(selected.name))
                {
                    levelCanvas.Children.Remove(element.element);
                    steps[actualStep].objects.Remove(element);
                    break;
                }
            }
        }
    }
}
