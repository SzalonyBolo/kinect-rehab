using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kinectRehab.db
{
    public class DbConnector
    {
        private System.Data.SqlClient.SqlConnection conn;
        Game game;

        public DbConnector()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            int index = baseDir.IndexOf("kinectRehab");
            string dataDir = baseDir.Substring(0, index) + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            dataDir += "\\kinectRehab\\db\\GamesDatabase.mdf";
            conn = new System.Data.SqlClient.SqlConnection();
            conn.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB; AttachDbFilename=\"" + dataDir +"\"; Integrated Security=True";

            try { conn.Open(); }
            catch (Exception e)
            { System.Console.WriteLine(e.ToString()); }
            System.Console.WriteLine("Connection opened!");
        }

        public List<string[]> getGamesCount()
        {
            SqlCommand cmd;
            SqlDataReader rdr;
            cmd = new SqlCommand("SELECT GameName, GameId FROM GameIndex;", conn);
            rdr = cmd.ExecuteReader();
            string[] value = new string[2];
            List<string[]> gameIndex = new List<string[]>();
            while (rdr.Read())
            {
                value[0] = (string)rdr[0];
                value[1] = (string)rdr[1];
                string[] addObject = (string[]) value.Clone();
                gameIndex.Add(addObject);
            }
            rdr.Close();
            return gameIndex;
        }

        public int getDifficultyCount(string game)
        {
            SqlCommand cmd;
            SqlDataReader rdr;
            cmd = new SqlCommand("SELECT COUNT(DISTINCT Difficulty) FROM Games WHERE GameId='" + game + "';", conn);
            rdr = cmd.ExecuteReader();
            int count = 0;
            while (rdr.Read())
            {
                count = (int)rdr[0];
            }
            rdr.Close();
            return count;
        }

        public void setGame(Game game)
        {
            this.game = game;
        }

        public GameStep[] getSteps(int difficulty)
        {
            SqlCommand cmd;
            SqlDataReader rdr;
            cmd = new SqlCommand("SELECT COUNT(DISTINCT Step) FROM Games WHERE GameId='" + game.game + "' AND Difficulty=" + difficulty +";", conn);
            rdr = cmd.ExecuteReader();
            int count = 0;
            while (rdr.Read())
            {
                count = (int)rdr[0];
            }
            rdr.Close();
            GameStep[] gameSteps = new GameStep[count];

            for (int i = 0; i < count; i++)
            {
                cmd = new SqlCommand("select Value from Games Where GameId='" + game.game + "' AND Difficulty=" + difficulty + " AND Step=" + i + " order by Step ASC", conn);
                rdr = cmd.ExecuteReader();
                List<string> valueList = new List<string>();
                while (rdr.Read())
                {
                     valueList.Add((string)rdr[0]);
                }
                rdr.Close();
                gameSteps[i] = valueToGameStep(valueList, i);
            }
            return gameSteps;
        }

        private GameStep valueToGameStep(List<string> values, int step)
        {
            GameStep gameStep = new GameStep(game);
            foreach (string value in values)
            {
                string[] order = value.Split(' ');
                switch(order[0])
                {
                    case "chbg":
                        gameStep.img = order[1];
                        break;
                    case "addElement":
                        List<string> options = new List<string>();
                        if (order.Count() > 3)
                        {
                            for (int i = 4; i < order.Count(); i++)
                            {
                                options.Add(order[i]);
                            }
                        }
                        var shape = createElement(order[1], options);
                        float[] margin = new float[2];
                        margin[0] = Single.Parse(order[2]);
                        margin[1] = Single.Parse(order[3]);
                        shape.Margin = new Thickness(margin[0], margin[1], 0, 0);
                        
                        gameStep.polygons.Add(shape);
                        break;
                    case "endStep":
                        List<string> endStepOptions = order.ToList();
                        endStepOptions.RemoveAt(0); //remove "endstep" from order Options
                        gameStep.endStepOptions = endStepOptions;
                        break;

                    default: //change some element on begining of step
                        List<string> chgElementOptions = order.ToList();
                        string chgElementName = chgElementOptions.ElementAt(0);
                        chgElementOptions.RemoveAt(0);
                        gameStep.polygonsToChange.Add(chgElementName, chgElementOptions);
                        break;
                }
            }
            return gameStep;
        }

        internal bool endedGameToDB(GameStat gameStat)
        {
            SqlCommand cmd;
            SqlDataReader rdr;

            cmd = new SqlCommand("SELECT Id FROM Player WHERE Name= '" + gameStat.getName() + "'", conn);
            rdr = cmd.ExecuteReader();

            int pid = -1;
            rdr.Read();
            pid = (int)rdr[0];
            rdr.Close();

            if (pid == -1)
            {
                cmd = new SqlCommand("SELECT Id FROM Player ORDER BY Id DESC", conn);
                rdr = cmd.ExecuteReader();

                rdr.Read();
                pid = (int)rdr[0];
                rdr.Close();

                pid++;
                cmd = new SqlCommand("INSERT INTO Player(Name, Age) values('" + gameStat.getName() + "', '" + gameStat.getAge() + "')", conn);
                cmd.ExecuteNonQuery();
            }

            int gameid = 0;
            cmd = new SqlCommand("Select Id from GameIndex Where Convert(VARCHAR, GameId) = '" + gameStat.game + "'", conn);
            rdr = cmd.ExecuteReader();
            rdr.Read();
            gameid = (int)rdr[0];
            rdr.Close();


            cmd = new SqlCommand("SELECT Id FROM GameParam ORDER BY Id DESC", conn);
            rdr = cmd.ExecuteReader();
            int gameparamid = 0;
            rdr.Read();
            gameparamid = (int)rdr[0];
            rdr.Close();
            gameparamid++;
            cmd = new SqlCommand("INSERT INTO GameParam(Id, GameId, Difficulty) values('" + gameparamid + "', '" + gameid + "', '" + gameStat.getDiff() + "')", conn);
            cmd.ExecuteNonQuery();



            cmd = new SqlCommand("SELECT Id FROM GameResult ORDER BY Id DESC", conn);
            rdr = cmd.ExecuteReader();

            int gameresultid = 0;
            rdr.Read();
            gameresultid = (int)rdr[0];
            rdr.Close();
            gameresultid++;
            DateTime date = DateTime.Now;
            cmd = new SqlCommand("INSERT INTO GameResult(Id, Date, Duration, Mistakes) values('" + gameresultid +"', '" + date + "', '" + gameStat.getDuration() + "', '" + gameStat.getMistakes() + "')", conn);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand("SELECT Id FROM History ORDER BY Id DESC", conn);
            rdr = cmd.ExecuteReader();
            int historyid = 0;
            rdr.Read();
            historyid = (int)rdr[0];
            rdr.Close();
            historyid++;
            cmd = new SqlCommand("INSERT INTO History(Id, GameParamId, GameResultsId, PlayerId) values('" + historyid +"', '" + gameparamid + "', '" + gameresultid + "', '" + pid + "')", conn);
            cmd.ExecuteNonQuery();

            return true;
        }

        internal void makeGame(List<string> orders)
        {
            SqlCommand cmd;
            SqlDataReader rdr;

            string game = orders[0];
            orders.RemoveAt(0);
            int diff = Int32.Parse(orders[0]);
            orders.RemoveAt(0);

            int step = -1;
            foreach (string order in orders)
            {
                if (order.Contains("incrementstep"))
                {
                    step++;
                    continue;
                }
                if (order.Contains("addtoelements"))
                {
                    string[] element = order.Split(' ');
                    cmd = new SqlCommand("INSERT INTO Elements(ElementName, cords, image) values('" +  element[1] + "', '" + element[2] + " " + element[3] + "', '" + element[4] + "')", conn);
                    cmd.ExecuteNonQuery();
                    continue;
                }
                cmd = new SqlCommand("INSERT INTO Games(GameId, Difficulty, Step, Value) values('" + game + "', '" + diff + "', '" + step + "', '" + order + "')", conn);
                cmd.ExecuteNonQuery();
            }
        }

        private Grabable createElement(string name, List<string> options)
        {
            SqlCommand cmd;
            SqlDataReader rdr;

            //get cords
            cmd = new SqlCommand("SELECT cords from Elements Where ElementName= '" + name + "'", conn);
            rdr = cmd.ExecuteReader();
            string cords = null;
            while (rdr.Read())
            {
                cords = (String)rdr[0];
            }
            rdr.Close();

            //get image
            cmd = new SqlCommand("SELECT image from Elements Where ElementName= '" + name + "'", conn);
            rdr = cmd.ExecuteReader();
            string image = null;
            while (rdr.Read())
            {
                image = (String)rdr[0];
            }
            rdr.Close();
            Grabable grabable = game.createElement(cords, image, name, game.game, options);
            return grabable;
        }

        
    }
}
