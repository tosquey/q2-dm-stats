using System;
using System.Collections.Generic;
using System.Linq;

namespace q2_dm_parser
{
    public class Match
    {
        public Match()
        {
            Players = new List<Player>();
        }
        
        public string Map { get; set; }
        public List<Player> Players { get; set; }
    }
}