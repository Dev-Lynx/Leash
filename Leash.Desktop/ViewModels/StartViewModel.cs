using Leash.Common.Infrastructure;
using Leash.Common.Infrastructure.Extensions;
using Leash.Common.Infrastructure.Services.Interfaces;
using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Leash.Desktop.ViewModels
{
    public class StartViewModel : BindableBase
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public ICoordinator Coordinator { get; }
        #endregion

        #region Commands
        public ICommand NextCommand { get; }
        #endregion

        #endregion

        #region Constructors
        public StartViewModel(ILoggerFacade logger, IRegionManager regionManager, ICoordinator coordinator)
        {
            Logger = logger;
            RegionManager = regionManager;
            Coordinator = coordinator;

            NextCommand = new DelegateCommand(OnNext);
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnNext()
        {
            RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, Core.REGISTER_VIEW);
        }
        #endregion

        #endregion
    }
}
