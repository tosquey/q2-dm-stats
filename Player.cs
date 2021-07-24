using System;
using System.Collections.Generic;
using System.Linq;

namespace q2_dm_parser
{
    public class Player
    {
        public string Nick { get; set; }

                public List<Frag> Frags { get; set; }
        public int Suicides
        {
            get
            {
                return this.Frags.Where(b => b.Killer == this.Nick && b.isSuicide).Count();
            }
        }

        public int FragCount
        {
            get
            {
                return this.Frags.Where(b => b.Killer == this.Nick && !b.isSuicide).Count() - this.Suicides;
            }
        }

        public int Deaths
        {
            get
            {
                return this.Frags.Where(a => a.Killed == this.Nick).Count() + this.Suicides;
            }
        }

        public double Efficiency
        {
            get
            {
                return (double)((double)this.FragCount / (double)(this.FragCount + this.Deaths));
            }
        }
    }
}