using System;

namespace q2_dm_parser
{
    public class Frag : IEquatable<Frag>
    {
        public bool Equals(Frag other)
        {
            return (this.Killed == other.Killed)
                && (this.Killer == other.Killer)
                && (this.Timestamp == other.Timestamp)
                && (this.Weapon == other.Weapon);
        }

        public override int GetHashCode()
        {
            return this.Killed.GetHashCode() + this.Killer.GetHashCode() + this.Timestamp.GetHashCode() + this.Weapon.GetHashCode();
        }

        public string Killer { get; set; }
        public string Killed { get; set; }
        public string Weapon { get; set; }
        public DateTime Timestamp { get; set; }
        public bool isSuicide { get; set; }
    }
}