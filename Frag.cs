using System;

namespace q2_dm_parser
{
    public class Frag
    {
        public string Killer { get; set; }
        public string Killed { get; set; }
        public string Weapon { get; set; }
        public DateTime Timestamp { get; set; }
        public bool isSuicide { get; set; }
    }
}