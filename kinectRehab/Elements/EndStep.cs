using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect.Input;
using System.Windows;
using System.Globalization;
using System.Windows.Media.Effects;

namespace kinectRehab.Elements
{
    public class EndStep
    {
        public Dictionary<Grabable, string> elementsToChange;
        public List<Grabable> prevElements;
        public List<string> controllerOptions;
        Game game;


        public EndStep(List<string> options, Game game, Grabable grab, GameStep gameStep)
        {
            this.game = game;
            this.elementsToChange = new Dictionary<Grabable, string>();
            this.prevElements = new List<Grabable>();
            this.controllerOptions = new List<string>();

            string switchEndStepOrder = options.ElementAt(0);
            options.RemoveAt(0); //if not null, options to casses
            switch (switchEndStepOrder)
            {
                case "grabable": //make endstep grabbable to next object
                    string toGrabableName = options.ElementAt(0);
                    options.RemoveAt(0); //removing name element, next - options to grabbable

                    this.controllerOptions.Add("grabable");
                    this.controllerOptions.Add(toGrabableName);
                    if (options.Count() > 0)
                    {
                        for (int i = 0; i < options.Count; i++)
                        {
                            switch (options[i])
                            {
                                case "inv":
                                    grab.Child.Opacity = 0.01;
                                    this.controllerOptions.Add(options[i]);
                                    break;

                                case "elchbg":
                                    string orderNameChbg = options[i];
                                    this.controllerOptions.Add(orderNameChbg);
                                    i++;

                                    string chbgElementName = options[i];
                                    i++;

                                    string newBackground = options[i];
                                    string url = "../../../images/" + this.game.game + "/" + newBackground;

                                    string orderChbg = orderNameChbg + " " + chbgElementName + " " + url;
                                    Grabable toGrabable = gameStep.getElement(chbgElementName);
                                    if (elementsToChange.ContainsKey(toGrabable))
                                    {
                                        elementsToChange[toGrabable] += " ";
                                        elementsToChange[toGrabable] += orderChbg;
                                    }
                                    else
                                    {
                                        elementsToChange.Add(toGrabable, orderChbg);
                                    }
                                    break;

                                case "elchsize":
                                    string orderNameChsize = options[i];
                                    this.controllerOptions.Add(orderNameChsize);
                                    i++;

                                    string chsizeElementName = options[i];
                                    i++;

                                    string nWidth = options[i];
                                    i++;

                                    string nHeight = options[i];
                                    string orderChsize = orderNameChsize + " " + chsizeElementName + " " + nWidth + " " + nHeight;
                                    Grabable grabableChsize = gameStep.getElement(chsizeElementName);
                                    if (elementsToChange.ContainsKey(grabableChsize))
                                    {
                                        elementsToChange[grabableChsize] += " ";
                                        elementsToChange[grabableChsize] += orderChsize;
                                    }
                                    else
                                    {
                                        elementsToChange.Add(grabableChsize, orderChsize);
                                    }
                                    break;

                                case "elmove":
                                    string orderNameMove = options[i];
                                    this.controllerOptions.Add(orderNameMove);
                                    i++;

                                    string moveElementName = options[i];
                                    i++;

                                    string moveMode = options[i];
                                    i++;

                                    string xVec = options[i];
                                    i++;

                                    string yVec = options[i];
                                    string orderElmove = orderNameMove + " " + moveMode + " " + xVec + " " + yVec;
                                    Grabable grabableMove = gameStep.getElement(moveElementName);
                                    if (elementsToChange.ContainsKey(grabableMove))
                                    {
                                        elementsToChange[grabableMove] += " ";
                                        elementsToChange[grabableMove] += orderElmove;
                                    }
                                    else
                                    {
                                        elementsToChange.Add(grabableMove, orderElmove);
                                    }
                                    break;
                            }
                        }
                    }
                    break;

                case "grab":
                    this.controllerOptions.Add("grab");
                    break;

                case "push":
                    //grab.grab = false;
                    this.controllerOptions.Add("grab");
                    break;
            }
        }

        public void elemetsChange()
        {
            foreach (KeyValuePair<Grabable, string> element in this.elementsToChange)
            {
                Grabable grab = element.Key;
                Rectangle child = grab.Child as Rectangle;
                string[] orders = element.Value.Split(' ');
                for (int i = 0; i < orders.Count(); i++)
                {
                    switch (orders[i])
                    {
                        case "elchbg":
                            i++;
                            //string name = orders[i];
                            //Rectangle ChbgShape = grab.Child as Rectangle;
                            i++;
                            string url = orders[i];
                            child.Fill = new ImageBrush(new BitmapImage(new Uri(url, UriKind.Relative)));
                            break;

                        case "elchsize":
                            i++;
                            //string name = orders[i];
                            //Rectangle ChsizeShape = grab.Child as Rectangle;
                            i++;
                            double nWidth = double.Parse(orders[i]);
                            i++;

                            double nHeight = double.Parse(orders[i]);
                            //string url = orders[i];
                            child.Width = nWidth;
                            child.Height = nHeight;
                            break;

                        case "elmove":
                            i++;

                            string moveMode = orders[i];
                            i++;

                            double mWidth = double.Parse(orders[i], NumberStyles.AllowLeadingSign);
                            i++;

                            double mHeight = double.Parse(orders[i], NumberStyles.AllowLeadingSign);
                            i++;

                            

                            if (moveMode.Equals("abs"))
                            {
                                //Canvas.SetLeft(child, mWidth);
                                //Canvas.SetTop(child, mHeight);
                            }
                            else
                            {
                                var aWidth = Canvas.GetLeft(grab);
                                var aHeight = Canvas.GetTop(grab);
                                if (double.IsNaN(aWidth)) aWidth = 0;
                                if (double.IsNaN(aHeight)) aHeight = 0;
                                Canvas.SetLeft(grab, aWidth + mWidth);
                                Canvas.SetTop(grab, aHeight + mHeight);
                                grab.prevMargin[0] = mWidth;
                                grab.prevMargin[1] = mHeight;
                            }
                            Console.WriteLine("prev in elmove: " + grab.prevMargin[1]);
                            break;
                    }
                }
            }
        }

        internal void toGrabbableStart(Grabable obj)
        {
            prevElements = new List<Grabable>();
            foreach (KeyValuePair<Grabable, string> element in this.elementsToChange)
            {
                string optionsElement = element.Value;
                var grab = element.Key as Grabable;
                var prevGrab = grab.clone();
                var prevGrabChild = grab.CloneShape();
                prevGrab.Child = prevGrabChild;
                prevElements.Add(prevGrab);
            }
            var thisClone = obj.clone();
            var thisCloneChild = obj.CloneShape();
            thisClone.Child = thisCloneChild;
            prevElements.Add(thisClone);


        }
    }
}
