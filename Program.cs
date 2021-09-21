using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace q2_dm_parser
{
    class Program
    {
        //log file to be read by the parser (standard openffa/opentdm log file)
        const string logFile = @"d:\temp\openffa1.log";

        //location where all the reports will be saved
        const string reportsFolder = @"d:\temp\reports\";

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

        /// <summary>Builds a simple list of all players found in the log file</summary>
        static void WriteAllPlayers(List<Match> matches)
        {
            string reportFile = string.Format("{0}{1}-{2}.txt", reportsFolder, "all-players", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
            File.WriteAllLines(reportFile, matches.SelectMany(a => a.Players).Select(b => b.Nick).Distinct().OrderBy(c => c));
        }

        /// <summary>Returns a list of all matches found in the logfile. CAVEAT: the standard opentdm/openffa logs don't have a clear indication on when a match begins. This method is looking for the manual "gamemap" command, which is what we use via rcon when running our tournaments in Brazil</summary>
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

        /// <summary>Returns the individual list of players for a map, based on all the frags identified in the log</summary>
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

        /// <summary>Builds a csv report with the timeline of the match and count of frags per player broken down by minute of gameplay</summary>
        static void BuildFragTimeline(List<Match> matches)
        {
            foreach (var match in matches)
            {
                Console.WriteLine(string.Format("Creating frag timeline from file {0}", match.Map));
                string reportFile = string.Format("{0}{1}-{2}-{3}.csv", reportsFolder, "timeline-report", match.Map, DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
                using StreamWriter report = new(reportFile, append: true);

                DateTime[] timeline = match.Players.SelectMany(a => a.Frags).Select(b => b.Timestamp).Distinct().OrderBy(c => c).ToArray();

                report.WriteLine(string.Concat("Player,", string.Join(',', timeline)));

                foreach (var player in match.Players.OrderByDescending(a => a.FragCount))
                {
                    Dictionary<DateTime, int> tl = new Dictionary<DateTime, int>();
                    int fragCount = 0;

                    foreach (DateTime timestamp in timeline)
                    {
                        var kills = player.Frags.Where(a => a.Killer == player.Nick && a.Timestamp == timestamp && !a.isSuicide).Count();
                        var suicides = player.Frags.Where(a => a.Killer == player.Nick && a.Timestamp == timestamp && a.isSuicide).Count();

                        fragCount += kills - suicides;

                        tl.Add(timestamp, fragCount);
                    }

                    report.WriteLine(string.Concat(player.Nick, ",", string.Join(',', tl.Values)));
                }
            }
        }

        /// <summary>Parses a line of text in the log and returns a frag (with killer, killed, if it's a suicide and weapon used)</summary>
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
                    frag.Killed = string.Empty;
                    frag.Weapon = item.Value;
                    frag.isSuicide = true;

                    break;
                }
            }

            return frag;
        }

        /// <summary>Generates the final score for each match from the list provided</summary>
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

        /// <summary>Generates the player stats (count of frags/suicides by weapon) for each match in the list provided</summary>
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
                        Dictionary<string, int> weapon = player.Frags.Where(z => !z.isSuicide && z.Killer == player.Nick)
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
                        weapon = player.Frags.Where(z => z.isSuicide && z.Killer == player.Nick)
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

        /// <summary>Generates the killer vs killed stats for each match from the list provided</summary>
        static void GenerateOpponentStats(List<Match> matches)
        {
            var frags = matches.SelectMany(a => a.Players).SelectMany(b => b.Frags).ToList();
            var players = matches.SelectMany(a => a.Players).ToList();

            Console.WriteLine("Creating opponent report");
            string reportFile = string.Format("{0}{1}-{2}.csv", reportsFolder, "opponent-report", DateTime.Now.ToString("yyyy-MM-dd-hhmmssffff"));
            using StreamWriter report = new(reportFile, append: true);

            //jogando header na lista pra ser index0 (sem precisar ajustar o resto)
            List<string> killed = new List<string>(){ "Player/opponent" };
            killed.AddRange(frags.Where(a => !string.IsNullOrEmpty(a.Killed)).Select(b => b.Killed).Distinct().OrderBy(b => b).ToList());
            report.WriteLine(string.Join(',', killed));
            
            foreach (var player in frags.Select(a => a.Killer).Distinct().OrderBy(b => b))
            {
                Dictionary<string, int> opponents = players.Where(a => a.Nick == player).SelectMany(b => b.Frags).Where(c => c.Killed != player)
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
