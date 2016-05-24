using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.Wpf.Controls;
using System.Windows.Controls;
using Microsoft.Kinect.Toolkit.Input;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using kinectRehab.Elements;
using Microsoft.Kinect.Input;

namespace kinectRehab
{
    public class Grabable : Decorator, IKinectControl
    {
        public string imgurl = null;
        public string toGrabableName = null;
        public List<string> controllerOptions;
        public Game game;
        public double[] prevMargin = new double[2];
        public EndStep endStep = null;
        public bool grab = true;

        bool IKinectControl.IsManipulatable
        {
            get
            {
                if (grab)
                    return true;
                else
                    return false;
            }
        }

        bool IKinectControl.IsPressable
        {
            get
            {
                if (grab)
                    return false;
                else
                    return true;
            }
        }

        IKinectController IKinectControl.CreateController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            if (endStep != null)
            {
                return new GrabableController(inputModel, kinectRegion, this.endStep.controllerOptions);
            }
            if (controllerOptions == null)
            {
                return new GrabableController(inputModel, kinectRegion);
            }
            else
            {
                return new GrabableController(inputModel, kinectRegion, controllerOptions);
            }
        }

        public Grabable clone()
        {
            Grabable clone = new Grabable();
            clone.Margin = this.Margin;
            clone.Child = this.CloneShape();
            clone.game = this.game;
            clone.imgurl = this.imgurl;
            clone.toGrabableName = this.toGrabableName;
            //clone.controllerOptions = this.controllerOptions;
            clone.Name = this.Name;
            clone.Height = this.Height;
            clone.Width = this.Width;
            clone.prevMargin[0] = this.prevMargin[0];
            clone.prevMargin[1] = this.prevMargin[1];
            return clone;
        }

        public bool checkIfIntersect(Grabable toGrabable)
        {
            Vector v1 = VisualTreeHelper.GetOffset(this);
            Rect r1 = new Rect(v1.X, v1.Y, this.ActualWidth, this.ActualHeight);

            Vector v2 = VisualTreeHelper.GetOffset(toGrabable);
            Rect r2 = new Rect(v2.X, v2.Y, toGrabable.ActualWidth, toGrabable.ActualHeight);

            if (r1.IntersectsWith(r2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Rectangle CloneShape()
        {
            Rectangle clone = new Rectangle();
            Rectangle child = this.Child as Rectangle;
            clone.Name = (string)child.Name.Clone();
            clone.Margin = new Thickness(child.Margin.Left, child.Margin.Top, child.Margin.Right, child.Margin.Bottom);
            clone.Height = child.Height;
            clone.Width = child.Width;
            if (this.imgurl != null)
                clone.Fill = new ImageBrush(new BitmapImage(new Uri(this.imgurl, UriKind.Relative)));
            clone.Opacity = child.Opacity;
            return clone;
        }

        internal void toGrabableEnd()
        {   
            if (this.endStep == null)
            {

            }
            else
            {
                int igb = this.endStep.controllerOptions.IndexOf("grabable");
                string toGrabableName = this.endStep.controllerOptions.ElementAt(igb + 1);
                var toGrabable = game.getGameShape(toGrabableName);
                if (this.checkIfIntersect(toGrabable))
                {
                    game.endStepCompleted();
                }
                else
                {
                    undoEndStep();
                }
            }
        }

        internal void endThisStep()
        {
            game.endStepCompleted();
        }

        internal void undoEndStep()
        {
            foreach (Grabable element in this.endStep.prevElements)
            {
                var gameElement = game.getGameShape(element.Name);
                gameElement.Margin = element.Margin;
                gameElement.Child = element.CloneShape();

                if (element.Name == this.Name)
                {
                    this.Margin = element.Margin;
                    this.Child = element.CloneShape();
                    Canvas.SetLeft(this, -prevMargin[0]);
                    Canvas.SetTop(this, -prevMargin[1]);
                }
                else
                {
                    Canvas.SetLeft(gameElement, -gameElement.prevMargin[0]);
                    Canvas.SetTop(gameElement, -gameElement.prevMargin[1]);
                    Console.WriteLine("prev in grabable: " + gameElement.prevMargin[1]);
                }
            }
        }

        internal void toGrabableStart()
        {
            prevMargin[0] = Canvas.GetLeft(this);
            prevMargin[1] = Canvas.GetTop(this);
        }
    }
}
