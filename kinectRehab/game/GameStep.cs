using kinectRehab.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace kinectRehab
{
    public class GameStep
    {
        public Game game;
        public string img;
        public List<Grabable> polygons;
        public List<string> endStepOptions;
        public Dictionary<string, List<string>> polygonsToChange;

        public GameStep(Game game)
        {
            this.game = game;
            polygons = new List<Grabable>();
            polygonsToChange = new Dictionary<string, List<string>>();
        }

        internal void addEndStepOrders(List<string> endStepOptions)
        {
            string endStepElementName = endStepOptions.ElementAt(0);
            endStepOptions.RemoveAt(0); //Element Name + endstep mode and options to it
            var grab = getElement(endStepElementName);
            grab.endStep = new EndStep(endStepOptions, game, grab, this);
        }

        public Grabable getElement(string name)
        {
            Grabable grabable = game.getGameShape(name);
            if (grabable == null)
            {
                foreach (Grabable grab in polygons)
                {
                    if (grab.Name.Equals(name))
                    {
                        grabable = grab;
                        break;
                    }
                }
            }
            return grabable;
        }

        private bool deleteElement(string name)
        {
            bool del = game.delGameShape(name);
            if (!del)
            {
                foreach (Grabable grab in polygons)
                {
                    if (grab.Name.Equals(name))
                    {
                        polygons.Remove(grab);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        internal void chgElement(string chgElementName, List<string> chgElementOptions)
        {
            Grabable chgElement = getElement(chgElementName);
            string chgElementSwitch = chgElementOptions.ElementAt(0);
            chgElementOptions.RemoveAt(0);
            switch (chgElementSwitch)
            {
                case "grabable":
                    string toGrabableName = chgElementOptions.ElementAt(0);
                    chgElementOptions.RemoveAt(0);
                    chgElement.toGrabableName = toGrabableName;
                    break;
            }
        }
    }
}