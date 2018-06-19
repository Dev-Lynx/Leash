using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leash.Common.Infrastructure.Models
{
    public class Contestant
    {
        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public Roles Role { get; set; }

        [LiteDB.BsonIgnore]
        public Contestant Partner { get; set; }
        [LiteDB.BsonIgnore]
        public Contestant OriginalPartner { get; set; }

        [LiteDB.BsonIgnore]
        public bool IsSelected { get; set; } = true;
        #endregion

        #region Constructors
        public Contestant() { }
        public Contestant(string name, Contestant partner)
        {
            Name = name;
            Role = Roles.Heeler;
            Partner = partner;
        }

        public Contestant(Contestant c)
        {
            Id = c.Id;
            Name = c.Name;
            Role = c.Role;
            Partner = c.Partner;
        }
        #endregion

        #region Methods

        #region Equity and Comparison
        static bool CompareContestants(Contestant c1, Contestant c2)
        {
            bool nullOne = object.ReferenceEquals(c1, null);
            bool nullTwo = object.ReferenceEquals(c2, null);

            if (nullOne && nullTwo) return true;
            else if (nullOne || nullTwo) return false;

            return c1.Name.ToLower() == c2.Name.ToLower();// && c1.Role == c2.Role;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Contestant)) return false;
            return CompareContestants(this, (Contestant)obj);
        }

        public static bool operator ==(Contestant c1, Contestant c2) => CompareContestants(c1, c2);
        public static bool operator !=(Contestant c1, Contestant c2) => !CompareContestants(c1, c2);

        public override int GetHashCode()
        {
            object[] properties = new object[]
            {
                Name, Role
            };

            int hash = 17;
            unchecked
            {
                for (int i = 0; i < properties.Length; i++)
                    hash += 23 * properties[i].GetHashCode(); 
            }
            return hash;
        }

        public override string ToString()
        {
            return Name;//string.Format("{0} - {1}", Name, Role.ToString());
        }
        #endregion

        #endregion
    }
}
