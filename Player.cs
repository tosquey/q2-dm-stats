using System;
using System.Collections.Generic;
using System.Linq;

namespace q2_dm_parser
{
    public class Player
    {
        public string Nick { get; set; }

        public List<Match> Matches { get; set; }
        public List<Frag> Frags { get; set; }
        public int Suicides
        {
            get
            {
                return this.Matches.SelectMany(a => a.Frags.Where(b => b.isSuicide)).Count(); //.All().Frags.Where(a => a.isSuicide).Count();
            }
        }

        public int FragCount
        {
            get
            {
                return this.Matches.SelectMany(a => a.Frags.Where(b => !b.isSuicide)).Count() - Suicides;
            }
        }

        public int Deaths
        {
            get
            {
                return this.Matches.SelectMany(a => a.Frags.Where(b => b.Killed == this.Nick)).Count();
            }
        }
    }
}