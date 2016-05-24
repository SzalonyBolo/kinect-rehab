using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace kinectRehab
{
    public class GameStat
    {
        private int mistakes = 0;
        private Stopwatch sw = new Stopwatch();
        private string player;
        private int playerAge;
        private int difficulty;
        public string game;

        public GameStat(string pl, int age, int difficulty, string game)
        {
            this.player = pl;
            this.playerAge = age;
            this.difficulty = difficulty;
            this.game = game;
        }

        public int getDuration()
        {
            return sw.Elapsed.Seconds;
        }

        public int getMistakes()
        {
            return mistakes;
        }

        public int getDiff()
        {
            return difficulty;
        }

        public string getName()
        {
            return player;
        }

        public int getAge()
        {
            return playerAge;
        }

        public void incrementMistakes()
        {
            this.mistakes++;
        }

        public void startTimer()
        {
            sw.Start();
        }

        public void stopTimer()
        {
            sw.Stop();
        }
    }
}
