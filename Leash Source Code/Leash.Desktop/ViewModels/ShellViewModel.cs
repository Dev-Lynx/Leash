using Leash.Common.Infrastructure;
using Leash.Common.Infrastructure.Extensions;
using Leash.Desktop.Views;
using Prism.Commands;
using Prism.Logging;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Leash.Desktop.ViewModels
{
    public class ShellViewModel
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManager { get; }
        #endregion

        #region Commands
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand MoveCommand { get; }
        public ICommand ImageDoubleCommand { get; }
        #endregion

        #region Internals
        public IMainWindow MainWindow { get; set; }
        public string PreviousView { get; set; }
        public bool CanNavigateToAbout { get; set; } = true;
        #endregion

        #endregion

        #region Constructors
        public ShellViewModel(ILoggerFacade logger, IRegionManager regionManager)
        {
            Logger = logger;
            RegionManager = regionManager;

            MoveCommand = new DelegateCommand(OnMoveWindow);
            MinimizeCommand = new DelegateCommand(OnMinimizeWindow);
            CloseCommand = new DelegateCommand(OnClose);
            ImageDoubleCommand = new DelegateCommand(OnNavigateToAbout);
        }
        #endregion

        #region Methods

        #region Command Handlers
        async void OnNavigateToAbout()
        {
            if (!CanNavigateToAbout) return;

            CanNavigateToAbout = false;
            string current = "";
            try
            {
                current = RegionManager.Regions[Core.MAIN_REGION].NavigationService.Journal.CurrentEntry.Uri.ToString();
            }
            catch (Exception e) { Logger.Error(e); CanNavigateToAbout = true; return; }

            string temp = "";
            string original = "";

            for (int i = 0; i< current.Length; i++)
            {
                if (char.IsUpper(current[i]) && i != 0) temp += " ";
                temp += current[i];
            }
            original = current;
            current = temp;

            string destination = Core.ABOUT_VIEW;

            
            if (!string.IsNullOrWhiteSpace(PreviousView))
                destination = PreviousView;

            var view = (FrameworkElement)RegionManager.Regions[Core.MAIN_REGION].GetView(destination);

            if (destination == Core.ABOUT_VIEW)
            {
                view.Opacity = 0;
                RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, destination);
                await view.FadeIn(1);
                view.Opacity = 1;
            }
            else
            {
                var about = (FrameworkElement)RegionManager.Regions[Core.MAIN_REGION].GetView(Core.ABOUT_VIEW);
                about.Opacity = 1;
                await about.FadeOut(3);
                about.Opacity = 0;
                RegionManager.RequestNavigateToUserControl(Core.MAIN_REGION, destination);
            }


            PreviousView = current;
            CanNavigateToAbout = true;
        }
        void OnMoveWindow()
        {
            try { MainWindow.DragMove(); }
            catch { }
        }
        void OnMinimizeWindow() => MainWindow.WindowState = System.Windows.WindowState.Minimized;
        void OnClose() => MainWindow.Close();
        #endregion

        #endregion
    }
}
