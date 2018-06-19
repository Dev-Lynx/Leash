using Leash.Common.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leash.Common.Infrastructure.Services.Interfaces
{
    public interface IDatabaseManager
    {
        ObservableCollection<Contestant> Contestants { get; }
        void SaveContestant(Contestant contestant);
        Contestant GetContestant(string name);
        void ClearContestants();
    }
}
