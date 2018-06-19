using Leash.Common.Infrastructure.Services;
using Leash.Common.Infrastructure.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Leash.Desktop
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override ILoggerFacade CreateLogger()
        {
            return new NLogger();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterType<IDatabaseManager, DatabaseManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICoordinator, Coordinator>(new ContainerControlledLifetimeManager());
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
            {
                var viewName = viewType.FullName;
                viewName = viewName.Replace(".Views.", ".ViewModels.");
                var viewAssemmbleName = viewType.Assembly.FullName;
                var suffix = viewName.EndsWith("View") ? "Model" : "ViewModel";
                var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", viewName, suffix);

                var assembly = viewType.Assembly;
                var type = assembly.GetType(viewModelName, true);

                return type;
            });
        }

        protected override DependencyObject CreateShell()
        {
            return ServiceLocator.Current.TryResolve<Views.Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }
    }
}
