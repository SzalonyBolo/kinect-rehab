using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace kinectRehab
{
    public class Game
    {
        public string game;
        
        int difficulty;
        db.DbConnector dbConnector;
        public GameStep[] steps;
        private Dictionary<string, Grabable> shapes = new Dictionary<string, Grabable>();
        public GameStat gameStat;
        private GameWindow gameWindow;

        public Game(string game, string player, int playerAge, int difficulty, db.DbConnector dbConnector)
        {
            this.game = game;
            this.gameStat = new GameStat(player, playerAge, difficulty, game);
            this.difficulty = difficulty;
            this.dbConnector = dbConnector;
            this.dbConnector.setGame(this);
            steps = this.dbConnector.getSteps(this.difficulty);
        }

        public void addMistake()
        {
            this.gameStat.incrementMistakes();
        }

        public void startTimer()
        {
            this.gameStat.startTimer();
        }

        public KinectRegion getKinectRegion()
        {
            return gameWindow.kinectRegion;
        }

        public void setWindow(GameWindow game)
        {
            this.gameWindow = game;
        }

        public Grabable getGameShape(string name)
        {
            Grabable grab = null;
            if (shapes.ContainsKey(name))
            {
                grab = shapes[name];
            }
            return grab;
        }

        public void endStepCompleted()
        {
            this.gameWindow.generateStep();
        }

        public void addGameShape(Grabable grab)
        {
            var shape = grab.Child as Shape;
            shapes.Add(grab.Name, grab);
        }

        public bool delGameShape(string name)
        {
            if (shapes.ContainsKey(name))
            {
                foreach (var grab in gameWindow.gameCanvas.Children.OfType<Grabable>())
                {
                    if (name.Equals(grab.Name))
                    {
                        gameWindow.gameCanvas.Children.Remove(grab);
                        break;
                    }
                }
                shapes.Remove(name);
                return true;
            }
            else
            {
                return false;
            }
        }

        public Grabable createElement(string cordsValue, string imageValue, string name, string game, List<string> options)
        {
            bool inv = false;
            if (options.Count > 0)
            {
                foreach (string option in options)
                {
                    switch (option)
                    {
                        case "inv":
                            inv = true;
                            break;
                    }
                }
            }
            Grabable grabable = new Grabable();
            string[] cords = cordsValue.Split(' ');
            Shape shape;
            if (cords.Count() == 2) //if there is only 2 cords it's rectangle - first is width second is height of control
            {
                shape = new Rectangle();
                Rectangle rec = shape as Rectangle;
                float width = float.Parse(cords[0], CultureInfo.InvariantCulture.NumberFormat);
                rec.Width = width;
                float height = float.Parse(cords[1], CultureInfo.InvariantCulture.NumberFormat);
                rec.Height = height;
                shape = rec;
            }
            else //else it's polygon and cords are pairs of next points
            {
                shape = new Polygon();
                Polygon asdf = shape as Polygon;
                //Polygon pol = shape as Polygon;
                PointCollection points = new PointCollection();
                float[] margin = new float[2];
                for (int i = 0; i < cords.Count(); i = i + 2)
                {
                    margin[0] = float.Parse(cords[i], CultureInfo.InvariantCulture.NumberFormat);
                    margin[1] = float.Parse(cords[i + 1], CultureInfo.InvariantCulture.NumberFormat);
                    Point point = new Point(margin[0], margin[1]);
                    points.Add(point);
                }
                asdf.Points = points;

            }
            shape.Name = name;
            shape.VerticalAlignment = VerticalAlignment.Top;
            shape.HorizontalAlignment = HorizontalAlignment.Left;

            if (!imageValue.Equals("nobg"))
                {
                grabable.imgurl = "../../../images/" + game + "/" + imageValue;
                shape.Fill = new ImageBrush(new BitmapImage(new Uri(grabable.imgurl, UriKind.Relative)));
                }
            else
                grabable.imgurl = null;
            
            if (inv)
            {
                shape.Opacity = 0.01;
            }
            grabable.Child = shape;
            grabable.Name = name;
            grabable.game = this;
            return grabable;
        }

        internal void clearControllerOptions()
        {
            foreach(KeyValuePair<string, Grabable> element in shapes)
            {
                element.Value.controllerOptions = new List<string>();
                element.Value.endStep = null;
            }
        }
    }
}
