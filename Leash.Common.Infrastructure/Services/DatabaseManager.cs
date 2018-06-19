using Leash.Common.Infrastructure.Extensions;
using Leash.Common.Infrastructure.Models;
using Leash.Common.Infrastructure.Services.Interfaces;
using LiteDB;
using Prism.Logging;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leash.Common.Infrastructure.Services
{
    public class DatabaseManager : BindableBase, IDatabaseManager
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        #endregion

        #region Bindables
        public ObservableCollection<Contestant> Contestants { get; private set; } = new ObservableCollection<Contestant>();
        #endregion

        #region Internals
        LiteDatabase Database { get; } = new LiteDatabase(Core.DATABASE_PATH);
        #endregion

        #endregion

        #region Constructors
        public DatabaseManager(ILoggerFacade logger)
        {
            Logger = logger;

            LoadContestants();
        }
        #endregion

        #region Methods

        #region IDatabase Implementation
        public void SaveContestant(Contestant contestant)
        {
            var collection = Database.GetCollection<Contestant>(Core.CONTESTANTS_DOC_NAME);

            if (collection.FindOne(c => c.Name.ToLower() == contestant.Name.ToLower()) == null)
                collection.Insert(contestant);
            else collection.Update(contestant);

            if (contestant.Partner != null)
            {
                var partner = contestant.Partner;
                if (collection.FindOne(c => c.Name.ToLower() == partner.Name.ToLower()) == null)
                    collection.Insert(partner);
                else collection.Update(partner);
            }

            LoadContestants();
        }

        public Contestant GetContestant(string name)
        {
            var collection = Database.GetCollection<Contestant>(Core.CONTESTANTS_DOC_NAME);

            return collection.FindOne(c => c.Name.ToLower() == name.ToLower());
        }

        public void ClearContestants()
        {
            try
            {
                Database.DropCollection(Core.CONTESTANTS_DOC_NAME);
                LoadContestants();
            }
            catch (Exception e)
            { Logger.Error(e); }
        }
        #endregion

        #region Load Engines
        void LoadContestants()
        {
            try
            {
                var collection = Database.GetCollection<Contestant>(Core.CONTESTANTS_DOC_NAME);

                if (collection == null) return;

                Contestants = new ObservableCollection<Contestant>(collection.FindAll());

                RaisePropertyChanged("Contestants");
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        #endregion

        #endregion
    }
}
