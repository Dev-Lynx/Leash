using Leash.Common.Infrastructure;
using Leash.Common.Infrastructure.Extensions;
using Leash.Common.Infrastructure.Models;
using Leash.Common.Infrastructure.Services.Interfaces;
using Prism.Commands;
using Prism.Logging;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Leash.Desktop.ViewModels
{
    public class ContestantViewModel
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public ICoordinator Coordinator { get; }
        #endregion

        #region Commands
        public ICommand BackCommand { get; }
        public ICommand ExportCommand { get; }
        #endregion

        #endregion

        #region Constructors
        public ContestantViewModel(ILoggerFacade logger, IRegionManager regionManager, ICoordinator coordinator)
        {
            Logger = logger;
            RegionManager = regionManager;
            Coordinator = coordinator;

            BackCommand = new DelegateCommand(OnBack);
            ExportCommand = new DelegateCommand(OnExport);
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnBack()
        {
            RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, Core.CONTEST_VIEW);
        }

        void OnExport()
        {
            string path = "";

            if (DialogHelper.SaveFile(out path, Core.SUPPORTED_EXPORTS))
            {
                var ext = Path.GetExtension(path);
                string message = "";

                var type = ext == ".xlsx" ? ExportTypes.Excel : ExportTypes.CSV;

                Coordinator.ExportContestantInfo(Coordinator.CurrentContestantInfo, type, path, out message);
            }
            
        }
        #endregion

        #endregion
    }
}
