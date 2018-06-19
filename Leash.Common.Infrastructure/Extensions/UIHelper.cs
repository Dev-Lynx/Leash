using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Leash.Common.Infrastructure.Extensions
{
    public static class UIHelper
    {
        public static void RequestNavigateToUserControl(this IRegionManager manager, string region, string view)
        {
            try
            {
                var control = (UserControl)manager.Regions[region].GetView(view);
                if (control == null)
                    throw new NullReferenceException("The region manager could not locate the specified region or view.");

                manager.RequestNavigate(region, control.GetType().Name);
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex);
            }
        }

        public static async Task FadeOut(this FrameworkElement element, double speed=1, bool reappear=false)
        {
            if (speed == 0) speed = 1;
            element.Opacity = 1;

            // time = distance / speed
            var time = (double)1 / speed;
            
            var animation = new DoubleAnimation
            {
                From = 1, To = 0, FillBehavior = FillBehavior.Stop,
                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(time))
            };

            EventHandler fadeHandler = null;
            bool completed = false;

            animation.Completed += fadeHandler = (s, e) =>
            {
                if (reappear) element.Opacity = 1;
                else element.Opacity = 0;
                completed = true;    
            };

            element.BeginAnimation(FrameworkElement.OpacityProperty, animation);

            while (!completed)
                await Task.Delay(10);
        }

        public static async Task FadeIn(this FrameworkElement element, double speed = 1, bool disappear = false)
        {
            if (speed == 0) speed = 1;
            element.Opacity = 0;

            // time = distance / speed
            var time = (double)1 / speed;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                FillBehavior = FillBehavior.Stop,
                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(time))
            };

            EventHandler fadeHandler = null;
            bool completed = false;

            animation.Completed += fadeHandler = (s, e) =>
            {
                if (disappear) element.Opacity = 0;
                else element.Opacity = 1;
                completed = true;
            };

            element.BeginAnimation(FrameworkElement.OpacityProperty, animation);

            while (!completed)
                await Task.Delay(10);
        }
    }
}
