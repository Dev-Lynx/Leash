using Leash.Common.Infrastructure;
using Leash.Common.Infrastructure.Services.Interfaces;
using Prism.Commands;
using Prism.Logging;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Leash.Desktop.ViewModels
{
    public class AboutViewModel
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IDatabaseManager DatabaseManager { get; }
        #endregion

        #region Commands
        public ICommand ClearNamesCommand { get; }
        public ICommand VisitMeCommand { get; }
        #endregion

        #endregion

        #region Constructors
        public AboutViewModel(ILoggerFacade logger, IDatabaseManager databaseManager)
        {
            Logger = logger;
            DatabaseManager = databaseManager;

            ClearNamesCommand = new DelegateCommand(OnClearNames);
            VisitMeCommand = new DelegateCommand(OnVisitMe);
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnClearNames()
        {
            var result = MessageBox.Show("A lot of legends may be forgotten if you do this...", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Hand);

            if (result == MessageBoxResult.Yes)
                DatabaseManager.ClearContestants();
        }

        void OnVisitMe() => Process.Start(new ProcessStartInfo(Core.COMPANY_PAGE));
        #endregion

        #endregion
    }
}
