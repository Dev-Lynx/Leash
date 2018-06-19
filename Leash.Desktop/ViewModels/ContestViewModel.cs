using Leash.Common.Infrastructure;
using Leash.Common.Infrastructure.Extensions;
using Leash.Common.Infrastructure.Models;
using Leash.Common.Infrastructure.Services.Interfaces;
using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Leash.Desktop.ViewModels
{
    public class ContestViewModel : BindableBase
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public ICoordinator Coordinator { get; }
        public IDatabaseManager DatabaseManager { get; }
        #endregion

        #region Command Handlers
        public ICommand RetractCommand { get; }
        public ICommand AdvanceCommand { get; }
        public ICommand GenerateTableCommand { get; }
        public ICommand DoneCommand { get; }
        public ICommand ViewContestantCommand { get; }
        public ICommand GenerateContestants { get; }
        #endregion

        #region Bindables

        #endregion

        #endregion

        #region Constructors
        public ContestViewModel(ILoggerFacade logger, IRegionManager regionManager, IDatabaseManager databaseManager, ICoordinator coordinator)
        {
            Logger = logger;
            DatabaseManager = databaseManager;
            Coordinator = coordinator;
            RegionManager = regionManager; 


            RetractCommand = new DelegateCommand(OnRetractToPreviousRound);
            AdvanceCommand = new DelegateCommand(OnAdvanceToNextRound);
            GenerateTableCommand = new DelegateCommand(OnGenerateTable);
            DoneCommand = new DelegateCommand(OnReset);
            ViewContestantCommand = new DelegateCommand<object>(OnViewContestant);
            GenerateContestants = new DelegateCommand(OnGenerateContestants);
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnAdvanceToNextRound()
        {
            Coordinator.AdvanceToNextRound();  
        }

        void OnRetractToPreviousRound()
        {
            Coordinator.NavigateToPreviousRound();
        }

        void OnGenerateTable()
        {
            string savePath = "";
            if (DialogHelper.SaveFile(out savePath, Core.SUPPORTED_EXPORTS))
            {
                string message = "";
                string ext = Path.GetExtension(savePath);

                if (ext.ToLower() == ".csv") Coordinator.GenerateCSVTable(savePath, out message);
                else Coordinator.GenerateTable(savePath, out message);
            }
        }

        void OnReset()
        {
            Coordinator.Reset();

            RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, Core.START_VIEW);
        }

        void OnViewContestant(object obj)
        {
            Logger.Debug("{0}", obj);
            var contestant = (Contestant)obj;

            if (contestant == null) return; 

            var info = Coordinator.CompileInfo(contestant);

            Logger.Debug("");
            for (int i = 0; i < info.HeaderEntries.Count; i++)
                Logger.Debug("({0})", info.HeaderEntries[i]);
            Logger.Debug("");
            Logger.Debug("");
            Logger.Debug("");
            for (int i = 0; i < info.HeelerEntries.Count; i++)
                Logger.Debug("({0})", info.HeelerEntries[i]);

            RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, Core.CONTESTANT_VIEW);
        }

        async void OnGenerateContestants()
        {
            var view = (FrameworkElement)RegionManager.Regions[Core.MAIN_REGION].GetView(Core.CONTEST_VIEW);
            await view.FadeOut(3);
            RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, Core.EXPORT_CONTESTANTS_VIEW);
            view.Opacity = 1;
        }
        #endregion

        #endregion
    }
}
