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
        const string logFile = @"/Users/lmatto1/temp/q2/openffa1.log";
        const string reportsFolder = @"/Users/lmatto1/temp/";

        static void Main(string[] args)
        {
            List<Match> matches = GetMatches(logFile);

            GenerateMapStats(matches);
            GenerateOpponentStats(matches);
            GenerateWeaponStats(matches);
            WriteAllPlayers(matches);
            BuildFragTimeline(matches);

            Console.WriteLine("Done");
        }


        static void WriteAllPlayers(List<Match> matches)
        {
            string reportFile = string.Format("{0}{1}-{2}.txt", reportsFolder, "all-players", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
            File.WriteAllLines(reportFile, matches.SelectMany(a => a.Players).Select(b => b.Nick).Distinct().OrderBy(c => c));
        }

        static List<Match> GetMatches(string logfile)
        {
            List<Match> matches = new List<Match>();
            List<Frag> frags = new List<Frag>();
            bool moveOn = false;
            Match match = new Match();

            Regex regexStart = new Regex(@"\[([0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9])\] gamemap (\S+.*)");
            Regex regexEnd = new Regex(@"\[[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9]\] Timelimit hit.");
            System.Text.RegularExpressions.Match regexStartMatch, regexEndMatch;

            foreach (string line in File.ReadAllLines(logfile))
            {
                if (!moveOn)
                {
                    regexStartMatch = regexStart.Match(line);

                    if (regexStartMatch.Success)
                    {
                        moveOn = true;
                        frags = new List<Frag>();
                        match = new Match();
                        match.Map = string.Join('_', regexStartMatch.Groups[2].Value, regexStartMatch.Groups[1].Value.Replace(' ', '-').Replace(":", string.Empty));
                    }
                }
                else
                {
                    regexEndMatch = regexEnd.Match(line);
                    if (regexEndMatch.Success)
                    {
                        moveOn = false;
                        match.Players.AddRange(GetPlayers(frags, match.Map));
                        matches.Add(match);
                    }
                }

                if (moveOn)
                {
                    var frag = GetFrag(line);
                    if (frag != null)
                    {
                        frags.Add(frag);
                    }
                }
            }

            return matches;
        }

        static List<Player> GetPlayers(List<Frag> frags, string mapName)
        {
            return (from frag in frags
                group frag by frag.Killer into g
                select new Player
                {
                    Nick = g.Key,
                    Frags = frags.Where(a => g.Key == a.Killer || g.Key == a.Killed).ToList()
                }).ToList();
        }

        static void BuildFragTimeline(List<Match> matches)
        {
            foreach (var match in matches)
            {
                Console.WriteLine(string.Format("Creating frag timeline from file {0}", match.Map));
                string reportFile = string.Format("{0}{1}-{2}-{3}.csv", reportsFolder, "timeline-report", match.Map, DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
                using StreamWriter report = new(reportFile, append: true);

                var frags = match.Players.SelectMany(a => a.Frags).ToList();

                DateTime[] timeline = frags.Select(b => b.Timestamp).Distinct().OrderBy(c => c).ToArray();

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
        }

        static Frag GetFrag(string line)
        {
            Frag frag = null;

            //frags
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

            //suicides
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

        static void GenerateMapStats(List<Match> matches)
        {
            string reportFile = string.Format("{0}{1}-{2}.txt", reportsFolder, "report", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));

            foreach (var match in matches)
            {
                List<Player> players = match.Players;
                
                Console.WriteLine(string.Format("Creating frag report for map {0}", match.Map));

                using StreamWriter report = new(reportFile, append: true);
                {
                    report.WriteLine("=========");
                    report.WriteLine(match.Map);
                    report.WriteLine("=========");
                    
                    foreach (var player in match.Players.OrderByDescending(a => a.FragCount))
                    {
                        report.WriteLine(string.Join("\t", player.Nick, player.FragCount, player.Deaths));
                    }

                    report.WriteLine();
                }
            }
        }

        static void GenerateWeaponStats(List<Match> matches)
        {
            string reportFile = string.Format("{0}{1}-{2}.txt", reportsFolder, "weapon-stats", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));

            foreach (var match in matches)
            {
                List<Player> players = match.Players;
                Console.WriteLine(string.Format("Creating weapon report for map {0}", match.Map));

                using StreamWriter report = new(reportFile, append: true);
                {
                    report.WriteLine("==================");
                    report.WriteLine("Weapon frag stats:");
                    report.WriteLine("==================");
                    report.WriteLine();

                    report.WriteLine("==================");
                    report.WriteLine(match.Map);
                    report.WriteLine("==================");
                    report.WriteLine();

                    foreach (var player in players)
                    {
                        report.WriteLine("==================");
                        report.WriteLine(player.Nick);
                        report.WriteLine("==================");

                        //frags
                        Dictionary<string, int> weapon = player.Frags.Where(z => !z.isSuicide)
                            .GroupBy(a => a.Weapon)
                            .ToDictionary(b => b.Key, b => b.Count());
                        
                        if (weapon.Count > 0)
                        {
                            report.WriteLine("Frags:");
                            foreach (var item in weapon)
                            {
                                report.WriteLine(string.Format("{0}: {1}", item.Key, item.Value));
                            }
                        }

                        //suicides
                        weapon = player.Frags.Where(z => z.isSuicide)
                            .GroupBy(a => a.Weapon)
                            .ToDictionary(b => b.Key, b => b.Count());
                        
                        if (weapon.Count > 0)
                        {
                            report.WriteLine();
                            report.WriteLine("Suicides:");
                            foreach (var item in weapon)
                            {
                                report.WriteLine(string.Format("{0}: {1}", item.Key, item.Value));
                            }
                        }

                        report.WriteLine();
                    }
                }
            }
        }

        static void GenerateOpponentStats(List<Match> matches)
        {
            var frags = matches.SelectMany(a => a.Players).SelectMany(b => b.Frags).ToList();

            Console.WriteLine("Creating opponent report");
            string reportFile = string.Format("{0}{1}-{2}.csv", reportsFolder, "opponent-report", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
            using StreamWriter report = new(reportFile, append: true);

            //jogando header na lista pra ser index0 (sem precisar ajustar o resto)
            List<string> killed = new List<string>(){ "Player/opponent" };
            killed.AddRange(frags.Where(a => !string.IsNullOrEmpty(a.Killed)).Select(b => b.Killed).Distinct().OrderBy(b => b).ToList());
            report.WriteLine(string.Join(',', killed));
            
            foreach (var player in frags.Select(a => a.Killer).Distinct().OrderBy(b => b))
            {
                Dictionary<string, int> opponents = frags.Where(a => a.Killer == player && !string.IsNullOrEmpty(a.Killed))
                    .GroupBy(b => b.Killed)
                    .ToDictionary(c => c.Key, c => c.Count());
                
                int[] oppIndex = new int[killed.Count];
                
                foreach (var item in opponents.OrderBy(a => a.Key))
                {
                    var index = killed.FindIndex(1, killed.Count -1, a => a == item.Key);
                    if (index > 0)
                        oppIndex[index] = item.Value;
                }

                report.WriteLine(string.Concat(player, ",", string.Join(',', oppIndex.Skip(1))));
            }
        }
    }
}
