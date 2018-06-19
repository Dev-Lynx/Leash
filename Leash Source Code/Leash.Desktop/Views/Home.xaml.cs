using Leash.Common.Infrastructure;
using Leash.Common.Infrastructure.Extensions;
using Prism.Logging;
using Prism.Regions;
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

namespace Leash.Desktop.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        #endregion

        #endregion

        public Home(ILoggerFacade logger, IRegionManager regionManager)
        {
            InitializeComponent();
            
            
            Logger = logger;
            RegionManager = regionManager;

            this.Loaded += (s, e) =>
            {
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3);
                timer.Tick += async (x, xe) =>
                {
                    timer.Stop();

                    await WelcomeBox.FadeOut();
                    RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, Core.START_VIEW);
                    WelcomeBox.Opacity = 1;
                };
                timer.Start();
            };
            
        }
    }
}
