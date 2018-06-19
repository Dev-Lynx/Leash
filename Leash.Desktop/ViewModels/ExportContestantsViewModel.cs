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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Leash.Desktop.ViewModels
{
    public class ExportContestantsViewModel : BindableBase
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        public ICoordinator Coordinator { get; }
        #endregion

        #region Commands
        public ICommand BackCommand { get; }
        public ICommand BrowseCommand { get; }
        public ICommand GenerateCommand { get; }
        #endregion

        #region Bindables
        public ObservableCollection<string> ExportTypesList { get; } = new ObservableCollection<string>()
        {
            "Excel SpreadSheet (.xlsx)", "Comma Seperated Values (.csv)"
        };
        public string SelectedExportType { get; set; }
        public double Progress { get; private set; }
        public string ExportPath { get; private set; } = Core.DOCUMENT_BASE;
        public bool Exporting { get; private set; }
        public bool CanExport => Directory.Exists(ExportPath);
        #endregion

        #endregion

        #region Constructors
        public ExportContestantsViewModel(ILoggerFacade logger, IRegionManager regionManager, ICoordinator coordinator)
        {
            Logger = logger;
            RegionManager = regionManager;
            Coordinator = coordinator;

            BackCommand = new DelegateCommand(OnBack);
            GenerateCommand = new DelegateCommand(OnGenerate);
            BrowseCommand = new DelegateCommand(OnBrowse);
        }
        #endregion

        #region Methods

        #region Command Handlers
        async void OnBack()
        {
            var view = (FrameworkElement)RegionManager.Regions[Core.MAIN_REGION].GetView(Core.EXPORT_CONTESTANTS_VIEW);
            await view.FadeOut(3);
            RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, Core.CONTEST_VIEW);
            view.Opacity = 1;
        }

        void OnBrowse()
        {
            if (!DialogHelper.GetFolderDirectory(out string folder)) return;
            ExportPath = folder;
            RaisePropertyChanged("ExportPath");
            RaisePropertyChanged("CanExport");
        }

        void OnGenerate()
        {
            var folder = ExportPath;

            if (Directory.Exists(folder))
                try { Directory.CreateDirectory(folder); }
                catch (Exception e)
                {
                    Logger.Error(e);
                    MessageBox.Show("The provided directory does not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            Task.Run(() =>
            {
                Exporting = true;
                Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged("Exporting"));
                var count = Coordinator.Info.OriginalContestants.Count;
                for (int i = 0; i < count; i++)
                {
                    Progress = (((i + .5)) / (double)count) * 100;
                    Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged("Progress"));

                    var contestant = Coordinator.Info.OriginalContestants[i];

                    if (!contestant.IsSelected) continue;

                    var type = (ExportTypes)ExportTypesList.IndexOf(SelectedExportType);
                    var path = Path.Combine(folder, contestant.Name);

                    path = path + ((type == ExportTypes.Excel) ? ".xlsx" : ".csv");

                    Coordinator.ExportContestantInfo(Coordinator.CompileInfo(contestant), type, path, out string message);

                    Progress = ((i + 1) / (double)count) * 100;
                    Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged("Progress"));
                }

                Exporting = false;
                Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged("Exporting"));
            });
        }
        #endregion

        #endregion
    }
}
