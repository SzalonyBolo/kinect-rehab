using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Kinect.Input;

namespace kinectRehab.Elements
{
    public class GrabableController : IKinectManipulatableController
    {
        protected ManipulatableModel inputModel;
        protected KinectRegion kinectRegion;
        protected Grabable dragDropElement;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        FrameworkElement IKinectController.Element
        {
            get
            {
                return this.inputModel.Element as FrameworkElement;
            }
        }

        ManipulatableModel IKinectManipulatableController.ManipulatableInputModel
        {
            get
            {
                return this.inputModel;
            }
        }

        public GrabableController()
        {

        }

        public GrabableController updateAndReturn(IInputModel inputModel, KinectRegion kinectRegion)
        {
            this.inputModel = inputModel as ManipulatableModel;
            this.kinectRegion = kinectRegion;
            this.dragDropElement = this.inputModel.Element as Grabable;
            return this;
        }

        public GrabableController(IInputModel inputModel, KinectRegion kinectRegion)
        {
            this.inputModel = inputModel as ManipulatableModel;
            this.kinectRegion = kinectRegion;
            this.dragDropElement = this.inputModel.Element as Grabable;



            this.inputModel.ManipulationStarted += Error_ManipulationStarted;
            //this.inputModel.ManipulationUpdated += InputModel_ManipulationUpdated;
            //this.inputModel.ManipulationCompleted += InputModel_ManipulationCompleted;
        }

        public GrabableController(IInputModel inputModel, KinectRegion kinectRegion, List<string> endStepOrder)
        {
            this.inputModel = inputModel as ManipulatableModel;
            this.kinectRegion = kinectRegion;
            this.dragDropElement = this.inputModel.Element as Grabable;

            this.inputModel.ManipulationStarted += endStep_Grab_ManipulationStarted;
            //this.inputModel.ManipulationCompleted += InputModel_ManipulationCompleted;
            //Grabable grabGlow = dragDropElement;
            bool isElementsToChange = false;
            foreach (string order in endStepOrder)
            {
                switch (order)
                {
                    case "grabable":
                        this.inputModel.ManipulationUpdated += InputModel_Move_ManipulationUpdated;
                        this.inputModel.ManipulationCompleted += grababble_ManipulationCompleted;
                        break;

                    case "grab":
                        this.inputModel.ManipulationStarted += endThisStep;
                        break;

                    case "push":
                        this.inputModel.ManipulationStarted += endThisStep;
                        break;

                    case "inv":
                        this.inputModel.ManipulationStarted += endStep_Visibility_ManipulationStarted;
                        break;

                    case "elchbg":
                        //this.inputModel.ManipulationStarted += endStep_Elchbg_ManipulationStarted;
                        isElementsToChange = true;
                        break;

                    case "elchsize":
                        //this.inputModel.ManipulationStarted += endStep_Elchbg_ManipulationStarted;
                        isElementsToChange = true;
                        break;

                    case "elmove":
                        //this.inputModel.ManipulationStarted += endStep_Elchbg_ManipulationStarted;
                        isElementsToChange = true;
                        break;

                    case "elementsToChange":
                        //this.inputModel.ManipulationStarted += endStep_Elchbg_ManipulationStarted;
                        isElementsToChange = true;
                        break;
                }
            }
            if (isElementsToChange)
            {
                this.inputModel.ManipulationStarted += endStep_Elchbg_ManipulationStarted;
            }
        }

        private void grababble_ManipulationCompleted(object sender, Microsoft.Kinect.Input.KinectManipulationCompletedEventArgs e)
        {
            var obj = sender as ManipulatableModel;
            var grab = obj.Element as Grabable;
            //if (grab.endStep != null)
            //{
            //    grab.endStep.toGrabbableEnd();
            //}
            //else
            //{
            //    grab.toGrabableEnd();
            //}
            grab.toGrabableEnd();
        }

        private void endThisStep(object sender, Microsoft.Kinect.Input.KinectManipulationStartedEventArgs e)
        {
            var obj = sender as ManipulatableModel;
            var grab = obj.Element as Grabable;
            grab.endThisStep();
        }

        private void endStep_Grab_ManipulationStarted(object sender, Microsoft.Kinect.Input.KinectManipulationStartedEventArgs e)
        {
            var obj = sender as ManipulatableModel;
            var grab = obj.Element as Grabable;
            if (grab.endStep != null)
            {
                grab.endStep.toGrabbableStart(grab);
            }
            else
            {
                grab.toGrabableStart();
            }
            //Console.WriteLine("TO TO");
        }

        private void endStep_Elchbg_ManipulationStarted(object sender, Microsoft.Kinect.Input.KinectManipulationStartedEventArgs e)
        {
            var obj = sender as ManipulatableModel;
            var grab = obj.Element as Grabable;
            if (grab.endStep != null)
                grab.endStep.elemetsChange();
        }

        private void Endstep_ManipulationCompleted(object sender, Microsoft.Kinect.Input.KinectManipulationCompletedEventArgs e)
        {
            //Console.WriteLine("NO DZIALA");
        }

        private void endStep_Visibility_ManipulationStarted(object sender, Microsoft.Kinect.Input.KinectManipulationStartedEventArgs e)
        {
            var obj = sender as ManipulatableModel;
            var grab = obj.Element as Grabable;
            grab.Opacity = 1;
            grab.Child.Opacity = 1;
        }


        private void Error_ManipulationStarted(object sender, Microsoft.Kinect.Input.KinectManipulationStartedEventArgs e)
        {
            var obj = sender as ManipulatableModel;
            var grab = obj.Element as Grabable;
            grab.game.addMistake();
        }

        private void InputModel_Move_ManipulationUpdated(object sender, Microsoft.Kinect.Input.KinectManipulationUpdatedEventArgs e)
        {
            var parentCanvas = dragDropElement.Parent as Canvas;
            if (parentCanvas != null)
            {
                var delta = e.Delta.Translation;
                var y = Canvas.GetTop(dragDropElement);
                var x = Canvas.GetLeft(dragDropElement);
                if (double.IsNaN(y)) y = 0;
                if (double.IsNaN(x)) x = 0;

                // delta values are 0.0 to 1.0, so we need to scale it to the number of pixels in the kinect region
                var yDelta = delta.Y * this.kinectRegion.ActualHeight;
                var xDelta = delta.X * this.kinectRegion.ActualWidth;

                Canvas.SetTop(dragDropElement, y + yDelta);
                Canvas.SetLeft(dragDropElement, x + xDelta);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).    
                }

                this.kinectRegion = null;
                this.inputModel = null;
                this.dragDropElement = null;

                this.inputModel.ManipulationStarted -= Error_ManipulationStarted;
                this.inputModel.ManipulationUpdated -= InputModel_Move_ManipulationUpdated;
                //this.inputModel.ManipulationCompleted -= endStep_Grabable_ManipulationCompleted;

               disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
   } 
}
