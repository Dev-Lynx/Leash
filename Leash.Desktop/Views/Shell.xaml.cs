using Leash.Common.Infrastructure;
using Leash.Common.Infrastructure.Extensions;
using Microsoft.Practices.Unity;
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
    public interface IMainWindow
    {
        WindowState WindowState { get; set; }
        void DragMove();
        void Close();
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Shell : Window, IMainWindow
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        IUnityContainer Container { get; }
        #endregion

        #endregion

        #region Constructors
        public Shell(ILoggerFacade logger, IRegionManager regionManager, IUnityContainer container)
        {
            Logger = logger;
            RegionManager = regionManager;
            Container = container;

            var vm = Container.Resolve<ViewModels.ShellViewModel>();
            InitializeComponent();

            vm.MainWindow = this;
            this.DataContext = vm;

            this.Loaded += (s, e) =>
            {
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.Home>(), Core.HOME_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.StartView>(), Core.START_VIEW);                
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.RegisterView>(), Core.REGISTER_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.ContestView>(), Core.CONTEST_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.AboutView>(), Core.ABOUT_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.ContestantView>(), Core.CONTESTANT_VIEW);
                RegionManager.Regions[Core.MAIN_REGION].Add(Container.Resolve<Views.ExportContestantsView>(), Core.EXPORT_CONTESTANTS_VIEW);
            };
        }
        #endregion

        #region Methods
        #endregion
    }
}
