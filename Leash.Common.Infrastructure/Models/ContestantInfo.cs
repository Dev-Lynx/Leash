using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leash.Common.Infrastructure.Models
{
    public class ContestantInfo
    {
        #region Properties
        public Contestant Contestant { get; set; }
        public List<Contestant> HeaderEntries { get; } = new List<Contestant>();
        public List<Contestant> HeelerEntries { get; } = new List<Contestant>();
        public bool IsHeader { get; set; }
        public bool IsHeeler { get; set; }
        public ContestInfo ContestInfo { get; set; }

        public ObservableCollection<EntryInfo> _entries = null;
        public ObservableCollection<EntryInfo> Entries
        {
            get
            {
                if (_entries == null)
                {
                    _entries = new ObservableCollection<EntryInfo>();

                    int position = 0;
                    for (int i = 1; i <= ContestInfo.Entries; i++)
                    {
                        var info = new EntryInfo();
                        info.Entry = i;
                        for (int j = 1; j <= ContestInfo.RoundsPerEntries; j++)
                        {
                            string origin = j == 1 ? "(PICK)" : "(DRAW)";

                            info.Headers.Add(new ContestantWrapper(position+1, HeaderEntries[position], origin));
                            info.Heelers.Add(new ContestantWrapper(position+1, HeelerEntries[position], origin));

                            position++;
                        }

                        _entries.Add(info);
                    }
                }
                return _entries;
            }
        }
        #endregion

        #region Constructors
        public ContestantInfo() { }
        #endregion

        #region Methods
        #endregion
    }

    public class EntryInfo
    {

        #region Properties
        public ObservableCollection<ContestantWrapper> Headers { get; } = new ObservableCollection<ContestantWrapper>();
        public ObservableCollection<ContestantWrapper> Heelers { get; } = new ObservableCollection<ContestantWrapper>();
        public int Entry { get; set; }
        public string Title => $"{NumberToOrdinal(Entry)} ENTRY";
        #endregion

        #region Methods
        string NumberToOrdinal(int number)
        {
            var work = number.ToString();
            if ((number % 100) == 11 || (number % 100) == 12 || (number % 100) == 13)
                return work + "TH";
            switch (number % 10)
            {
                case 1: work += "ST"; break;
                case 2: work += "ND"; break;
                case 3: work += "RD"; break;
                default: work += "TH"; break;
            }
            return work;
        }
        #endregion

    }

    public class ContestantWrapper
    {
        #region Properties
        
        public int Round { get; set; }
        public Contestant Contestant { get; set; }
        public string Origin { get; set; }

        #endregion

        #region Constructors
        public ContestantWrapper() { }
        public ContestantWrapper(int round, Contestant contestant, string origin = "(DRAW)")
        {
            Round = round;
            Contestant = contestant;
            Origin = origin;

            if (contestant == null) Origin = "";
        }
        #endregion
    }
}
