using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace kinectRehab
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        Game game;
        int stepCount = 0;
        MainWindow menu = null;
        Rectangle end;
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        public GameWindow(Game game, MainWindow menu)
        {
            this.game = game;
            this.menu = menu;

            this.kinectSensor = KinectSensor.GetDefault();
            this.InitializeComponent();

            KinectRegion.SetKinectRegion(this, kinectRegion);

            App app = ((App)Application.Current);
            app.KinectRegion = kinectRegion;

            // Use the default sensor
            this.kinectRegion.KinectSensor = kinectSensor;

            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.Closed += GameWindow_Closed;
            game.setWindow(this);
            generateStep();

            //Grabable grb = new Grabable();
            //Polygon grbPol = new Polygon();
            //PointCollection polPoints = new PointCollection();
            //polPoints.Add(new Point(50, 100));
            //polPoints.Add(new Point(200, 100));
            //polPoints.Add(new Point(200, 200));
            //polPoints.Add(new Point(300, 30));
            //grbPol.Points = polPoints;
            //grbPol.Fill = new SolidColorBrush(System.Windows.Media.Colors.Green);
            //grb.Child = grbPol;
            //this.gameRegion.Children.Add(grb);
        }

        private void GameWindow_Closed(object sender, EventArgs e)
        {
            end.Visibility = Visibility.Visible;
            gameCanvas.UpdateLayout();
            menu.endedGameToDB(this.game.gameStat);
            menu.Visibility = Visibility.Visible;
            this.Close();
        }

        public void generateStep()
        {
            if (game.steps.Length <= stepCount)
            {
                end.Visibility = Visibility.Visible;
                gameCanvas.UpdateLayout();
                endGame();
            }                
            else
            {
                GameStep actualStep = game.steps[stepCount];
                if (!(String.IsNullOrEmpty(actualStep.img)))
                {
                    this.Background = new ImageBrush(new BitmapImage(new Uri("../../../images/" + game.game + "/" + actualStep.img, UriKind.Relative)));
                }

                game.clearControllerOptions();

                if (actualStep.polygons.Count > 0)
                {
                    foreach (Grabable grab in actualStep.polygons)
                    {
                        game.addGameShape(grab);
                        gameCanvas.Children.Add(grab);

                        //DropShadowBitmapEffect myDropShadowEffect = new DropShadowBitmapEffect();

                        //Color myShadowColor = new Color();
                        //myShadowColor.ScR = 255;
                        //myShadowColor.ScG = 215;
                        //myShadowColor.ScB = 0;
                        //myShadowColor.ScA = 1;
                        //myDropShadowEffect.Color = myShadowColor;

                        //myDropShadowEffect.Direction = 320;
                        //myDropShadowEffect.ShadowDepth = 25;
                        //myDropShadowEffect.Softness = 1;
                        //myDropShadowEffect.Opacity = 0.5;

                        //grab.BitmapEffect = myDropShadowEffect;
                    }
                }

                if (actualStep.polygonsToChange.Count > 0)
                {
                    foreach (KeyValuePair<string, List<string>> element in actualStep.polygonsToChange)
                    {
                        Grabable grab = game.getGameShape(element.Key);

                        string[] order = element.Value.ToArray();
                        for (int i = 0; i < order.Count(); i++)
                        {
                            switch (order[i])
                            {
                                case "chbg":
                                    i++;
                                    string img = order[i];
                                    string url = "../../../images/" + game.game + "/" + img;
                                    grab.imgurl = url;
                                    var child = grab.Child as Rectangle;
                                    child.Fill = new ImageBrush(new BitmapImage(new Uri(url, UriKind.Relative)));
                                    break;

                                case "grabable":
                                    grab.controllerOptions.Add(order[i]); //grabable
                                    i++;
                                    grab.controllerOptions.Add(order[i]); //toGrabable
                                    break;

                                case "elchsize":
                                    i++;
                                    float x = float.Parse(order[i]);
                                    i++;
                                    float y = float.Parse(order[i]);
                                    var childsize = grab.Child as Rectangle;
                                    childsize.Width = x;
                                    childsize.Height = y;
                                    break;

                                case "del":
                                    gameCanvas.Children.Remove(grab);
                                    break;
                            }
                        }

                    }
                }
                actualStep.addEndStepOrders(actualStep.endStepOptions);
                stepCount++;
            }
        }

        private void endGame()
        {
            end.Visibility = Visibility.Visible;
            gameCanvas.UpdateLayout();
            bool b = menu.endedGameToDB(this.game.gameStat);
            if (b)
            {
                Thread.Sleep(5000);
                menu.Visibility = Visibility.Visible;
                this.Close();
            }

        }

        private void BtnEndStep_Click(object sender, EventArgs e)
        {
            Button btnEndStep = (Button)sender;
            gameCanvas.Children.Remove(btnEndStep);
            generateStep();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
            end = new Rectangle();
            end.VerticalAlignment = VerticalAlignment.Center;
            end.HorizontalAlignment = HorizontalAlignment.Left;
            end.Fill = new ImageBrush(new BitmapImage(new Uri("../../../images/koniec.png", UriKind.Relative)));
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double x = screenWidth / 2 - 100;
            end.Margin = new Thickness(x, 200, 0, 0);
            end.Width = 295;
            end.Height = 40;
            end.Visibility = Visibility.Hidden;
            gameCanvas.Children.Add(end);
        }

        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                foreach (Body body in this.bodies)
                {
                    if (body.IsTracked)
                    {
                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                        //lblGesture.Content = body.HandLeftState + " " + body.HandRightState;
                    }
                }
            }
        }
    }
}
