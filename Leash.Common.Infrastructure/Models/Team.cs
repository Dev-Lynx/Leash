using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leash.Common.Infrastructure.Models
{
    public class Team
    {
        #region Properties
        [FieldFixedLength(50)]
        public Contestant Header { get; set; }
        [FieldFixedLength(50)]
        public Contestant Heeler { get; set; }
        #endregion

        #region Constructors
        public Team() { }
        public Team(Contestant header, Contestant heeler)
        {
            Header = header;
            Heeler = heeler;
        }
        #endregion

        #region Methods

        #region Equity
        static bool CompareTeams(Team one, Team two)
        {
            bool nullOne = object.ReferenceEquals(one, null);
            bool nullTwo = object.ReferenceEquals(two, null);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return one.Header == two.Header && one.Heeler == two.Heeler;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Team)) return false;
            return CompareTeams(this, (Team)obj);
        }

        public static bool operator ==(Team one, Team two) => CompareTeams(one, two);
        public static bool operator !=(Team one, Team two) => !CompareTeams(one, two);

        public override int GetHashCode()
        {
            object[] properties = new object[]
            {
                Header, Heeler
            };

            int hash = 17;
            unchecked
            {
                for (int i = 0; i < properties.Length; i++)
                    if (properties[i] != null)
                        hash += 23 * properties[i].GetHashCode();
            }
            return hash;
        }
        #endregion

        #endregion
    }
}
