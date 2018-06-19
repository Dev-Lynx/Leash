using Leash.Common.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leash.Common.Infrastructure.Services.Interfaces
{
    public interface ICoordinator
    {
        ContestantInfo CompileInfo(Contestant c);
        ObservableCollection<string> Roles { get; }
        ContestInfo Info { get; }
        ContestantInfo CurrentContestantInfo { get; }
        void AddContestant(Contestant contestant);
        bool AdvanceToNextRound();
        void NavigateToPreviousRound();
        bool GenerateCSVTable(string path, out string message);
        bool GenerateTable(string path, out string message);
        bool ExportContestantInfo(ContestantInfo info, ExportTypes type, string path, out string message);
        void Reset();
    }
}
