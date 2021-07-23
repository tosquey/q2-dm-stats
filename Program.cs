﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace q2_dm_parser
{
    class Program
    {
        static string baseFolder = @"/Users/lmatto1/temp";
        static void Main(string[] args)
        {
            string reportFile = string.Format("{0}/{1}-{2}.txt", baseFolder, "report", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
            using StreamWriter report = new(reportFile, append: true);

            List<Frag> overallFrags = new List<Frag>();

            foreach (var file in Directory.GetFiles(string.Format("{0}/q2", baseFolder), "*.txt").OrderBy(a => a))
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

                BuildFragTimeline(frags, file.Split('/').Last().Replace(".txt", string.Empty));

                overallFrags.AddRange(frags);
            }

            GenerateOpponentStats(overallFrags);
        }

        static void BuildFragTimeline(List<Frag> frags, string mapFile)
        {
            string reportFile = string.Format("{0}{1}-{2}-{3}.csv", baseFolder, "/timeline-report", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"), mapFile);
            using StreamWriter report = new(reportFile, append: true);

            DateTime[] timeline = frags.Select(a => a.Timestamp).Distinct().OrderBy(b => b).ToArray();

            report.WriteLine(string.Concat("Player,", string.Join(',', timeline)));

            foreach (string player in frags.Select(a => a.Killer).Distinct().OrderBy(b => b))
            {
                Dictionary<DateTime, int> tl = new Dictionary<DateTime, int>();
                int fragCount = 0;

                foreach (DateTime timestamp in timeline)
                {
                    var kills = frags.Where(a => a.Killer == player && a.Timestamp == timestamp && !a.isSuicide).Count();
                    var suicides = frags.Where(a => a.Killer == player && a.Timestamp == timestamp && a.isSuicide).Count();

                    fragCount += kills - suicides;

                    tl.Add(timestamp, fragCount);
                }

                report.WriteLine(string.Concat(player, ",", string.Join(',', tl.Values)));
            }
        }

        static Frag GetFrag(string line)
        {
            Frag frag = null;

            Dictionary<string, string> regexFrags = new Dictionary<string, string>();
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) almost dodged (\S+.*)'s", "Rocket");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) ate (\S+.*)'s", "Rocket");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) couldn't hide from (\S+.*)'s", "BFG");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) saw the pretty lights from (\S+.*)'s", "BFG");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was disintegrated by (\S+.*)'s", "BFG");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was blown away by (\S+.*)'s", "SSG");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was melted by (\S+.*)'s", "Hyperblaster");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was blasted by (\S+.*)", "Blaster");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was machinegunned by (\S+.*)", "Machinegun");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was railed by (\S+.*)", "Railgun");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was gunned down by (\S+.*)", "Shotgun");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was popped by (\S+.*)'s", "Grenade launcher");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was cut in half by (\S+.*)'s", "Chaingun");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was shredded by (\S+.*)'s", "Grenade");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) caught (\S+.*)'s", "Grenade");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) didn't see (\S+.*)'s", "Grenade");
            regexFrags.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) tried to invade (\S+.*)'s personal space", "Telefrag");

            foreach (var item in regexFrags)
            {
                Regex regex = new Regex(item.Key);
                var match = regex.Match(line);

                if (match.Success)
                {
                    frag = new Frag();
                    frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                    frag.Killed = match.Groups[2].Value;
                    frag.Killer = match.Groups[3].Value;
                    frag.Weapon = item.Value;

                    break;
                }
            }

            Dictionary<string, string> regexSuicides = new Dictionary<string, string>();
            regexSuicides.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) blew .* up.", "Rocket");
            regexSuicides.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) tripped on", "Grenade");
            regexSuicides.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) cratered", "Queda");
            regexSuicides.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) melted\.", "Acido");
            regexSuicides.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) sank like a rock", "Afogado");
            regexSuicides.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) should have used a smaller gun.", "BFG");
            regexSuicides.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) does a back flip into the lava.", "LARVA!");
            regexSuicides.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) was squished", "Amassado");
            regexSuicides.Add(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] (\S+.*) saw the light", "Laser");

            foreach (var item in regexSuicides)
            {
                Regex regex = new Regex(item.Key);
                var match = regex.Match(line);

                if (match.Success)
                {
                    frag = new Frag();
                    frag.Timestamp = Convert.ToDateTime(match.Groups[1].Value);
                    frag.Killer = match.Groups[2].Value;
                    frag.Weapon = item.Value;
                    frag.isSuicide = true;

                    break;
                }
            }

            return frag;
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
            string reportFile = string.Format("{0}{1}-{2}.csv", baseFolder, "/opponent-report", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
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
}
