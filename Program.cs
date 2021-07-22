using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace q2_dm_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            string reportFile = string.Format("{0}-{1}.txt", @"/Users/lmatto1/temp/report", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
            using StreamWriter report = new(reportFile, append: true);

            List<Frag> overallFrags = new List<Frag>();

            foreach (var file in Directory.GetFiles(@"/Users/lmatto1/temp/q2"))
            {
                
                report.WriteLine("=========");
                report.WriteLine(file.Split('/').Last());
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

                GenerateWeaponStats(query.ToList(), report);

                report.WriteLine();

                //BuildFragTimeline(frags);

                overallFrags.AddRange(frags);
            }

            GenerateOpponentStats(overallFrags);
        }

        static Frag GetFrag(string line)
        {
            Frag frag = new Frag();

            //rocket
            Regex regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) almost dodged (\S+.*)'s");
            var match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Rocket";

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) ate (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Rocket";

                return frag;
            }


            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) couldn't hide from (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "BFG";

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) saw the pretty lights from (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "BFG";

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was disintegrated by (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "BFG";

                return frag;
            }

            //SSG
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was blown away by (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "SSG";

                return frag;
            }

            //hyperblaster
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was melted by (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Hyperblaster";

                return frag;
            }

            //blaster
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was blasted by (\S+.*)");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Blaster";

                return frag;
            }

            //machinegun
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was machinegunned by (\S+.*)");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Machinegun";

                return frag;
            }

            //rail
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was railed by (\S+.*)");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Railgun";

                return frag;
            }

            //shotgun
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was gunned down by (\S+.*)");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Shotgun";

                return frag;
            }

            //grenade launcher
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was popped by (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Grenade launcher";

                return frag;
            }

            //chain
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was cut in half by (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Chaingun";

                return frag;
            }
            
            //grenade
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was shredded by (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Grenade";

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) caught (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Grenade";

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) didn't see (\S+.*)'s");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Grenade";

                return frag;
            }

            //telefrag
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) tried to invade (\S+.*)'s personal space");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killed = match.Groups[2].Value;
                frag.Killer = match.Groups[3].Value;
                frag.Weapon = "Telefrag";

                return frag;
            }

            //suicides
            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) blew .* up.");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Rocket";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) tripped on");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Grenade";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) cratered");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Queda";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) melted");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Acido";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) sank like a rock");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Afogado";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) should have used a smaller gun.");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "BFG";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) does a back flip into the lava.");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "LARVA";
                frag.isSuicide = true;

                return frag;
            }

            regex = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was squished");
            match = regex.Match(line);

            if (match.Success)
            {
                frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                frag.Killer = match.Groups[2].Value;
                frag.Weapon = "Amassado";
                frag.isSuicide = true;

                return frag;
            }

            return null;
        }

        static void GenerateWeaponStats(List<Player> players, StreamWriter report)
        {
            {
                report.WriteLine("==================");
                report.WriteLine("Weapon frag stats:");
                report.WriteLine("==================");
                report.WriteLine();

                foreach (var player in players)
                {
                    report.WriteLine("==================");
                    report.WriteLine(player.Nick);
                    report.WriteLine("==================");

                    Dictionary<string, int> weapon = player.Frags
                        .GroupBy(a => a.Weapon)
                        .ToDictionary(b => b.Key, b => b.Count());
                    
                    foreach (var item in weapon)
                    {
                        report.WriteLine(string.Format("{0}: {1}", item.Key, item.Value));
                    }

                    report.WriteLine();
                }
            }
        }

        static void GenerateOpponentStats(List<Frag> frags)
        {
            string reportFile = string.Format("{0}-{1}.csv", @"/Users/lmatto1/temp/opponent-report", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
            using StreamWriter report = new(reportFile, append: true);

            //jogando header na lista pra ser index0 (sem precisar ajustar o resto)
            List<string> killed = new List<string>(){ "Player/opponent" };
            killed.AddRange(frags.Where(a => !string.IsNullOrEmpty(a.Killed)).Select(b => b.Killed).Distinct().OrderBy(b => b).ToList());
            report.WriteLine(string.Join(',', killed));
            
            foreach (var player in frags.Select(a => a.Killer).Distinct())
            {
                Dictionary<string, int> opponents = frags.Where(a => a.Killer == player && !string.IsNullOrEmpty(a.Killed))
                    .GroupBy(b => b.Killed)
                    .ToDictionary(c => c.Key, c => c.Count());
                
                int[] oppIndex = new int[killed.Count];
                
                foreach (var item in opponents)
                {
                    var index = killed.FindIndex(1, killed.Count -1, a => a == item.Key);
                    if (index > 0)
                        oppIndex[index] = item.Value;
                }

                report.WriteLine(string.Concat(player, ",", string.Join(',', oppIndex.Skip(1))));
            }
        }

        static void BuildFragTimeline(List<Frag> frags)
        {
            List<Timeline> timeline = new List<Timeline>();
            List<PlayerTimeline> playerTimeline = frags.Select(a => new PlayerTimeline() {
                Nick = a.Killer,
                Frags = 0
            }).Distinct().ToList();

            var query = from a in frags
                group a by a.Timestamp into g
                select new Timeline {
                    Timestamp = g.Key,
                    Frags = frags.Where(a => a.Timestamp == g.Key).ToList()
                };
            
            var list = query.ToList().OrderBy(a => a.Timestamp);

            List<TimelineFrag> timelineFrags = new List<TimelineFrag>();

            foreach (var timestamp in list)
            {
                foreach (var frag in timestamp.Frags)
                {
                    foreach (var player in playerTimeline)
                    {
                        if (frag.Killer == player.Nick)
                        {
                            player.Frags += frag.isSuicide ? -1 : 1;
                        }

                        var tlFrag = timelineFrags.Where(a => a.Nick == player.Nick).SingleOrDefault();

                        if (tlFrag == null)
                        {
                            var tl = new TimelineFrag();
                            tl.Nick = player.Nick;
                            tl.TimestampFrags.Add(timestamp.Timestamp, player.Frags);   
                            timelineFrags.Add(tl);
                        }
                        else
                        {
                            tlFrag.TimestampFrags.Add(timestamp.Timestamp, player.Frags);
                        }
                    }
                }
            }

            string reportFile = string.Format("{0}-{1}.csv", @"/Users/lmatto1/temp/timeline-report", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
            using StreamWriter report = new(reportFile, append: true);

            report.WriteLine(string.Join(',', list.Select(a => a.Timestamp)));

            foreach (var line in timelineFrags.OrderBy(a => a.Nick))
            {
                
            }
        }
    }

    class Frag
    {
        public string Killer { get; set; }
        public string Killed { get; set; }
        public string Weapon { get; set; }
        public DateTime Timestamp { get; set; }

        public bool isSuicide { get; set; }
    }

    class Player
    {
        public string Nick { get; set; }
        public List<Frag> Frags { get; set; }

        public int Suicides { get; set; }
    }

    class TimelineFrag
    {
        public TimelineFrag()
        {
            TimestampFrags = new Dictionary<DateTime, int>();
        }
        public string Nick { get; set; }
        public IDictionary<DateTime, int> TimestampFrags { get; private set; }
    }

    class PlayerTimeline : IEquatable<PlayerTimeline>
    {
        public bool Equals(PlayerTimeline other)
        {
            return this.Nick == other.Nick;
        }

        public override int GetHashCode()
        {
            return Nick.GetHashCode();
        }
        public DateTime Timestamp { get; set; }

        public string Nick { get; set; }
        public int Frags { get; set; }
    }

    class Timeline
    {
        public DateTime Timestamp { get; set; }
        public List<Frag> Frags { get; set; }
    }
}
