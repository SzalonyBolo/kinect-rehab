using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace kinectRehab
{
    /// <summary>
    /// Interaction logic for FolderChoose.xaml
    /// </summary>
    public partial class FolderChoose : Window
    {

        db.DbConnector dbConnector;
        int diffLvl = 0;
        int diffMax = -1;
        string game = "";
        public FolderChoose(db.DbConnector connector)
        {
            InitializeComponent();
            this.dbConnector = connector;
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            int index = baseDir.IndexOf("kinectRehab");
            string dataDir = baseDir.Substring(0, index) + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            dataDir += "\\kinectRehab\\images\\";
            var folders = Directory.EnumerateDirectories(dataDir); //, SearchOption.AllDirectories
            foreach (string folder in folders)
            {
                int lastSlash = folder.LastIndexOf('\\');
                string gameName = folder.Substring(lastSlash + 1);
                int gameDifCount = dbConnector.getDifficultyCount(gameName);
                lbxFolderNames.Items.Add(gameName);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (lbxFolderNames.SelectedIndex == -1)
                return;
            string gameName = lbxFolderNames.SelectedItem.ToString();
            LevelCreator levelCreator = new LevelCreator(gameName, diffLvl, dbConnector);
            levelCreator.ShowDialog();
            this.Close();
        }

        private void lbxFolderNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lstbx = (ListBox)sender;
            diffLvl = 0;
            lblDifficultyLevel.Content = "0";
            game = (string)lstbx.SelectedItem;
            diffMax = dbConnector.getDifficultyCount(game);
        }

        private void DiffInc_Click(object sender, RoutedEventArgs e)
        {
            if (diffLvl <= diffMax - 1)
            {
                diffLvl++;
                lblDifficultyLevel.Content = diffLvl.ToString();
            }
        }

        private void DiffDec_Click(object sender, RoutedEventArgs e)
        {
            if (diffLvl - 1 >= 0)
            {
                diffLvl--;
                lblDifficultyLevel.Content = diffLvl.ToString();
            }
        }
    }
}
