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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Leash.Desktop.ViewModels
{
    public class RegisterViewModel : BindableBase
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public IDatabaseManager DatabaseManager { get; }
        public ICoordinator Coordinator { get; }
        #endregion

        #region Bindables

        Contestant _selectedContestant;
        public Contestant SelectedContestant
        {
            get => _selectedContestant;
            set
            {
                _selectedContestant = value;

                /*
                if (value != null)
                    ContestantRole = (int)value.Role;
                */
                RaisePropertyChanged("SelectedContestant");
            }
        }


        string _contestantName = "";
        public string ContestantName
        {
            get => _contestantName;
            set
            {
                _contestantName = value;
                RaisePropertyChanged("ContestantName");
                RaisePropertyChanged("CanAdd");
            }
        }

        int _contestantRole = 0;
        public int ContestantRole
        {
            get => _contestantRole;
            set
            {
                _contestantRole = value;
                RaisePropertyChanged("PartnerRole");
            }
        }
        public string ContestantPartner { get; set; }
        public int PartnerRole
        {
            get
            {
                if ((Roles)ContestantRole == Roles.Header) return (int)Roles.Heeler;
                else return (int)Roles.Header;
            }
        }

        public string ErrorMessage { get; set; }
        #endregion

        #region Commands
        public bool CanAdd => !string.IsNullOrWhiteSpace(ContestantName);
        public ICommand AddCommand { get; }
        public ICommand CanAddChangedCommand { get; }
        public ICommand StartCommand { get; }
        #endregion

        #region Internals
        public FrameworkElement View { get; set; }
        
        #endregion

        #endregion

        #region Constructors
        public RegisterViewModel(ILoggerFacade logger, ICoordinator coordinator, IDatabaseManager databaseManager, IRegionManager regionManager)
        {
            Logger = logger;
            DatabaseManager = databaseManager;
            Coordinator = coordinator;
            RegionManager = regionManager;

            AddCommand = new DelegateCommand(OnAdd);
            StartCommand = new DelegateCommand(OnStart);

            CanAddChangedCommand = new DelegateCommand(()
                => {
                    RaisePropertyChanged("CanAdd"); });
            RaisePropertyChanged("PartnerRole");
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnAdd()
        {
            Contestant contestant = new Contestant()
            {
                Name = ContestantName, Role = (Roles)ContestantRole
            };

            if (!string.IsNullOrWhiteSpace(ContestantPartner))
            {
                contestant.Partner = new Contestant()
                {
                    Name = ContestantPartner, Role = (Roles)PartnerRole
                };
            }

            Coordinator.AddContestant(contestant);
            DatabaseManager.SaveContestant(contestant);

            ContestantName = "";
            ContestantPartner = "";
            RaisePropertyChanged("ContestantPartner");
        }

        async void OnStart()
        {
            /*
            if (!Coordinator.Info.BalancedContest)
            {
                ErrorMessage = "The amount of Headers and Heelers must be balanced";

                Timer t = new Timer((s) =>
                {
                    ErrorMessage = "";
                    RaisePropertyChanged("ErrorMessage");
                }, null, 3000, -1);

                
                
                RaisePropertyChanged("ErrorMessage");
                
                return;
            }
            */
            await View.FadeOut(3);
            RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, Core.CONTEST_VIEW);
            View.Opacity = 1;
        }
        #endregion

        #endregion
    }
}
