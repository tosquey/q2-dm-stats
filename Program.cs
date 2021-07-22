using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace q2_dm_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            using StreamWriter report = new(@"C:\Users\luizm\Desktop\report.txt", append: true);

            foreach (var file in Directory.GetFiles(@"d:\temp\"))
            {
                
                report.WriteLine("=========");
                report.WriteLine(file.Split('\\').Last());
                report.WriteLine("=========");

                List<Frag> frags = new List<Frag>();

                foreach (string line in File.ReadAllLines(file))
                {
                    var frag = GetFrag(line);
                    if (frag != null)
                    {
                        frags.Add(frag);
                    }
                }

                var query = from frag in frags
                    group frag by frag.Killer into g
                    select new Player
                    {
                        Nick = g.Key,
                        Frags = frags.Where(a => a.Killer == g.Key && !a.isSuicide).ToList(),
                        Suicides = frags.Where(a => a.Killer == g.Key && a.isSuicide).Count()
                    };
                    

                foreach (var player in query.OrderByDescending(a => a.Frags.Count - a.Suicides))
                {
                    report.WriteLine(string.Format("{0}: {1}", player.Nick, player.Frags.Count - player.Suicides));
                }

                report.WriteLine();
            }
        }

        static Frag GetFrag(string line)
        {
            Frag frag = new Frag();

            //rocket
            Regex regex = new Regex(@"(\S+) almost dodged (\S+)'s");
            var match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Rocket";

                return frag;
            }

            regex = new Regex(@"(\S+) ate (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Rocket";

                return frag;
            }


            regex = new Regex(@"(\S+) couldn't hide from (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "BFG";

                return frag;
            }

            regex = new Regex(@"(\S+) saw the pretty lights from (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "BFG";

                return frag;
            }

            regex = new Regex(@"(\S+) was disintegrated by (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "BFG";

                return frag;
            }

            //SSG
            regex = new Regex(@"(\S+) was blown away by (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "SSG";

                return frag;
            }

            //hyperblaster
            regex = new Regex(@"(\S+) was melted by (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Hyperblaster";

                return frag;
            }

            //blaster
            regex = new Regex(@"(\S+) was blasted by (\S+)");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Blaster";

                return frag;
            }

            //machinegun
            regex = new Regex(@"(\S+) was machinegunned by (\S+)");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Machinegun";

                return frag;
            }

            //rail
            regex = new Regex(@"(\S+) was railed by (\S+)");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Railgun";

                return frag;
            }

            //shotgun
            regex = new Regex(@"(\S+) was gunned down by (\S+)");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Shotgun";

                return frag;
            }

            //grenade launcher
            regex = new Regex(@"(\S+) was popped by (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Grenade launcher";

                return frag;
            }

            //chain
            regex = new Regex(@"(\S+) was cut in half by (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Chaingun";

                return frag;
            }
            
            //grenade
            regex = new Regex(@"(\S+) was shredded by (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Grenade";

                return frag;
            }

            regex = new Regex(@"(\S+) caught (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Grenade";

                return frag;
            }

            regex = new Regex(@"(\S+) didn't see (\S+)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Grenade";

                return frag;
            }

            //telefrag
            regex = new Regex(@"(\S+) tried to invade (\S+)'s personal space");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killed = match.Groups[1].Value;
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Telefrag";

                return frag;
            }

            //suicides
            regex = new Regex(@"(\S+) blew .* up.");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killer = match.Groups[1].Value;
                frag.Weapon = "Suicide";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"(\S+) tripped on");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killer = match.Groups[1].Value;
                frag.Weapon = "Suicide";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"(\S+) cratered");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killer = match.Groups[1].Value;
                frag.Weapon = "Suicide";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"(\S+) melted");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killer = match.Groups[1].Value;
                frag.Weapon = "Suicide";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"(\S+) sank like a rock");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killer = match.Groups[1].Value;
                frag.Weapon = "Suicide";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"(\S+) should have used a smaller gun.");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killer = match.Groups[1].Value;
                frag.Weapon = "Suicide";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"(\S+) does a back flip into the lava.");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killer = match.Groups[1].Value;
                frag.Weapon = "Suicide";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"(\S+) was squished");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Killer = match.Groups[1].Value;
                frag.Weapon = "Suicide";
                frag.isSuicide = true;

                return frag;
            }

            return null;
        }
    }

    class Frag
    {
        public string Killer { get; set; }
        public string Killed { get; set; }
        public string Weapon { get; set; }

        public bool isSuicide { get; set; }
    }

    class Player
    {
        public string Nick { get; set; }
        public List<Frag> Frags { get; set; }

        public int Suicides { get; set; }
    }
}
