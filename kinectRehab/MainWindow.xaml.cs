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
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using Microsoft.Kinect.Input;

namespace kinectRehab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        db.DbConnector dbConnector;
        string gameSelected;

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

        public MainWindow()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.InitializeComponent();

            // Use the default sensor
            this.kinectRegion.KinectSensor = kinectSensor;

            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            
            dbConnector = new db.DbConnector();
            setupGamesMenu();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            int diffCount = dbConnector.getDifficultyCount((string)btn.Content);
            lblPlayer.Visibility = Visibility.Visible;
            txtPlayer.Visibility = Visibility.Visible;
            lblDifficulty.Visibility = Visibility.Visible;
            btnStart.Visibility= Visibility.Visible;
            sldDifficulty.Minimum = 0;
            sldDifficulty.Maximum = diffCount - 1;
            sldDifficulty.Visibility = Visibility.Visible;
            txtAge.Visibility = Visibility.Visible;
            gameSelected = (string)btn.Content;
        }

        private void setupGamesMenu()
        {
            List<string[]> gamesIndex = dbConnector.getGamesCount();
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            int gameCount = gamesIndex.Count;
            double step = screenWidth / gameCount;
            double mv = screenWidth % gameCount;
            mv /= 2;
            double x = (step / 2) + mv - 100;
            double y = 0;
            foreach (var gameIndex in gamesIndex)
            {
                Rectangle rec = new Rectangle();
                string bgpath = "images\\" + gameIndex[1] + "\\button.png";
                rec.Fill = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), bgpath)));
                rec.VerticalAlignment = VerticalAlignment.Center;
                rec.HorizontalAlignment = HorizontalAlignment.Left;
                rec.Margin = new Thickness(x, y, 0, 0);
                rec.Height = 200;
                rec.Width = 200;
                this.kinectRegionGrid.Children.Add(rec);

                Button btn = new Button();
                btn.Height = 200;
                btn.Width = 200;
                btn.VerticalAlignment = VerticalAlignment.Center;
                btn.HorizontalAlignment = HorizontalAlignment.Left;
                btn.Margin = new Thickness(x, y, 0, 0);
                btn.Click += Button_Click;
                btn.Content = gameIndex[1];
                btn.Opacity = 0.01;
                this.kinectRegionGrid.Children.Add(btn);

                Label lbl = new Label();
                lbl.Content = gameIndex[0];
                lbl.Margin = new Thickness(x, y + 300, 0, 0);
                lbl.VerticalAlignment = VerticalAlignment.Center;
                lbl.HorizontalAlignment = HorizontalAlignment.Left;
                this.kinectRegionGrid.Children.Add(lbl);

                x += step;
            }
        }

        internal bool endedGameToDB(GameStat gameStat)
        {
            return dbConnector.endedGameToDB(gameStat);
            
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            string player = txtPlayer.Text;
            int difficulty = (int)sldDifficulty.Value;
            int age;
            if (!int.TryParse(txtAge.Text,out age))
                return;
            Game game = new Game(gameSelected, player, age, difficulty, dbConnector);
            GameWindow gameWindow = new GameWindow(game, this);
            gameWindow.Show();
            this.Visibility = Visibility.Collapsed;
        }
        
        private void KinectRehab_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
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
                      
                    }
                }
            }
        }
        
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnLevelCreator_Click(object sender, RoutedEventArgs e)
        {
            FolderChoose folderChoose = new FolderChoose(this.dbConnector);
            folderChoose.ShowDialog();
        }
    }
}
