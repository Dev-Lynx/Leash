using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leash.Common.Infrastructure.Models
{
    public class ContestInfo
    {
        #region Properties
        public DateTime Date { get; } = DateTime.Now;
        public int Picks { get; set; } = 1;
        public int Draws { get; set; } = 1;
        public int RoundsPerEntries => Picks + Draws;
        public int TotalRounds => (RoundsPerEntries) *Entries;
        public string System => string.Format("Pick {0} - Draw {1}", Picks, Draws);
        public int ContestPosition { get; set; } = 0;
        public int MaxAppearances { get; set; } = 1;
        public int Entries { get; set; } = 1;
        public int EntryPosition
        {
            get
            {
                double position = (double)ContestPosition / (double)RoundsPerEntries;
                return (int)Math.Ceiling(position);
            }
        }
        public bool AtEntryStart => ContestPosition % RoundsPerEntries == 1;
        public string OrdinalEntry => NumberToOrdinal(EntryPosition);

        /*
        
            Round Per Entry = 4
            Current Round = 10
            Entries = 4
            

        */

        public ObservableCollection<Contestant> OriginalContestants { get; set; } = new ObservableCollection<Contestant>();
        public ObservableCollection<Contestant> Contestants { get; set; } = new ObservableCollection<Contestant>();
        public ObservableCollection<Team> OriginalTeamEntries { get; set; } = new ObservableCollection<Team>();
        public ObservableCollection<Contestant> Headers { get; set; } = new ObservableCollection<Contestant>();
        public ObservableCollection<Contestant> Heelers { get; set; } = new ObservableCollection<Contestant>();
        public ObservableCollection<Contestant> OriginalHeaders { get; set; } = new ObservableCollection<Contestant>();
        public ObservableCollection<Contestant> OriginalHeelers { get; set; } = new ObservableCollection<Contestant>();
        public ObservableCollection<Team> Teams { get; set; } = new ObservableCollection<Team>();
        public List<List<Team>> TeamEntries { get; set; } = new List<List<Team>>();

        public ObservableCollection<int> DrawTypes { get; } = new ObservableCollection<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
        

        public bool ContestActive { get; set; }
        public bool CanRetract => ContestPosition > 1;
        public bool CanAdvance => ContestPosition < TotalRounds;
        public bool ContestEnd => ContestPosition == TotalRounds;
        public bool ContestComplete { get; set; }

        public int HeaderCount { get; private set; }
        public int HeelerCount { get; private set; }
        public bool BalancedContest => HeaderCount == HeelerCount && (HeaderCount + HeelerCount) > 1;
        #endregion

        #region Constructors
        public ContestInfo() { }
        #endregion

        #region Methods

        public void AddTeam(Team team)
        {
            OriginalTeamEntries.Add(team);

            OriginalHeaders.Add(team.Header);
            OriginalHeelers.Add(team.Heeler);

            if (team.Header != null)
            {
                HeaderCount++;
                Headers.Add(team.Header);
            }

            if (team.Heeler != null)
            {
                HeelerCount++;
                Heelers.Add(team.Heeler);
            }
        }

        string NumberToOrdinal(int number)
        {
            var work = number.ToString();
            if ((number % 100) == 11 || (number % 100) == 12 || (number % 100) == 13)
                return work + "th";
            switch (number % 10)
            {
                case 1: work += "st"; break;
                case 2: work += "nd"; break;
                case 3: work += "rd"; break;
                default: work += "th"; break;
            }
            return work;
        }
        #endregion
    }
}
