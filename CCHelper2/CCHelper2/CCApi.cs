﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CCHelper2
{
    class CCApi
    {
        private static List<string> alliances;

        public class Match
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public string DisplayName { get; set; }
            public DateTime Time { get; set; }
            public int ElimRound { get; set; }
            public int ElimGroup { get; set; }
            public int ElimInstance { get; set; }
            public int Red1 { get; set; }
            public bool Red1IsSurrogate { get; set; }
            public int Red2 { get; set; }
            public bool Red2IsSurrogate { get; set; }
            public int Red3 { get; set; }
            public bool Red3IsSurrogate { get; set; }
            public int Blue1 { get; set; }
            public bool Blue1IsSurrogate { get; set; }
            public int Blue2 { get; set; }
            public bool Blue2IsSurrogate { get; set; }
            public int Blue3 { get; set; }
            public bool Blue3IsSurrogate { get; set; }
            public string Status { get; set; }
            public DateTime StartedAt { get; set; }
            public string Winner { get; set; }
            public Result Result { get; set; }

            public Match() { }

            public string GetRedAllianceString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}, {1}, {2}", this.Red1, this.Red2, this.Red3);
                return sb.ToString();
            }

            public string GetBlueAllianceString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}, {1}, {2}", this.Blue1, this.Blue2, this.Blue3);
                return sb.ToString();
            }

            public BracketMatch ToBracketMatch()
            {
                BracketMatch bm = new BracketMatch();

                bm.DisplayName = this.DisplayName;
                bm.Status = this.Status;

                if (this.Red1 != 0)
                {
                    bm.RedAlliance = GetRedAllianceString();
                    bm.RedAllianceNum = alliances.IndexOf(alliances.FirstOrDefault(i => i.Contains(Red1.ToString()))) + 1;
                }
                else
                {
                    bm.RedAlliance = "";
                }

                if (this.Blue1 != 0)
                {
                    bm.BlueAlliance = GetBlueAllianceString();
                    bm.BlueAllianceNum = alliances.IndexOf(alliances.FirstOrDefault(i => i.Contains(Blue1.ToString()))) + 1;
                }
                else
                {
                    bm.BlueAlliance = "";
                }

                bm.Winner = this.Winner;

                return bm;
            }

            public MatchPreviewForDisplay ToMatchPreviewForDisplay()
            {
                MatchPreviewForDisplay mpfd = new MatchPreviewForDisplay();

                mpfd.Type = this.Type;

                if (this.DisplayName.StartsWith("Q") || this.DisplayName.StartsWith("S") || this.DisplayName.StartsWith("F"))
                {
                    mpfd.DisplayName = this.DisplayName;
                }
                else
                {
                    mpfd.DisplayName = "Q" + this.DisplayName;
                }

                mpfd.Status = this.Status;

                mpfd.Red1 = this.Red1;
                mpfd.Red2 = this.Red2;
                mpfd.Red3 = this.Red3;

                mpfd.Blue1 = this.Blue1;
                mpfd.Blue2 = this.Blue2;
                mpfd.Blue3 = this.Blue3;

                List<Ranking> ranks = getRankings();

                try
                {
                    double rawRedQA = (ranks.Find(i => i.TeamId == Red1).QualificationAverage + ranks.Find(i => i.TeamId == Red2).QualificationAverage + ranks.Find(i => i.TeamId == Red3).QualificationAverage) / 3;
                    double rawRedCanAvg = (ranks.Find(i => i.TeamId == Red1).ContainerPoints + ranks.Find(i => i.TeamId == Red2).ContainerPoints + ranks.Find(i => i.TeamId == Red3).ContainerPoints) / 3;
                    double rawRedCoopAvg = (ranks.Find(i => i.TeamId == Red1).CoopertitionPoints + ranks.Find(i => i.TeamId == Red2).CoopertitionPoints + ranks.Find(i => i.TeamId == Red3).CoopertitionPoints) / 3;
                    double rawBlueQA = (ranks.Find(i => i.TeamId == Blue1).QualificationAverage + ranks.Find(i => i.TeamId == Blue2).QualificationAverage + ranks.Find(i => i.TeamId == Blue3).QualificationAverage) / 3;
                    double rawBlueCanAvg = (ranks.Find(i => i.TeamId == Blue1).ContainerPoints + ranks.Find(i => i.TeamId == Blue2).ContainerPoints + ranks.Find(i => i.TeamId == Blue3).ContainerPoints) / 3;
                    double rawBlueCoopAvg = (ranks.Find(i => i.TeamId == Blue1).CoopertitionPoints + ranks.Find(i => i.TeamId == Blue2).CoopertitionPoints + ranks.Find(i => i.TeamId == Blue3).CoopertitionPoints) / 3;
                    mpfd.RedQA = Math.Round(rawRedQA, 2);
                    mpfd.RedCanAvg = Math.Round(rawRedCanAvg, 2);
                    mpfd.RedCoopAvg = Math.Round(rawRedCoopAvg, 2);
                    mpfd.BlueQA = Math.Round(rawBlueQA, 2);
                    mpfd.BlueCanAvg = Math.Round(rawBlueCanAvg, 2);
                    mpfd.BlueCoopAvg = Math.Round(rawBlueCoopAvg, 2);
                }
                catch (NullReferenceException)
                {
                    mpfd.RedQA = 0;
                    mpfd.BlueQA = 0;
                }

                mpfd.Winner = this.Winner;
                
                return mpfd;
            }

            public MatchResultsForDisplay ToMatchResultsForDisplay()
            {
                MatchResultsForDisplay mrfd = new MatchResultsForDisplay();
                StringBuilder sb = new StringBuilder();

                mrfd.Type = this.Type;

                if (this.DisplayName.StartsWith("Q") || this.DisplayName.StartsWith("S") || this.DisplayName.StartsWith("F"))
                {
                    mrfd.DisplayName = this.DisplayName;
                }
                else
                {
                    mrfd.DisplayName = "Q" + this.DisplayName;
                }

                mrfd.RedAlliance = GetRedAllianceString();

                mrfd.BlueAlliance = GetBlueAllianceString();

                if (this.Result != null)
                {
                    mrfd.RedScore = this.Result.RedScore.GetTotal();
                    mrfd.BlueScore = this.Result.BlueScore.GetTotal();

                    mrfd.RedAutoScore = this.Result.RedScore.GetAutoPoints();
                    mrfd.BlueAutoScore = this.Result.BlueScore.GetAutoPoints();

                    mrfd.RedFoulPoints = this.Result.RedScore.GetFoulPoints();
                    mrfd.BlueFoulPoints = this.Result.BlueScore.GetFoulPoints();

                    mrfd.RedCappedStacks = this.Result.RedScore.GetCappedStacks();
                    mrfd.BlueCappedStacks = this.Result.BlueScore.GetCappedStacks();

                    mrfd.RedCanEfficiency = Math.Round(this.Result.RedScore.GetCanEfficiencyRatio()*100,2);
                    mrfd.BlueCanEfficiency = Math.Round(this.Result.BlueScore.GetCanEfficiencyRatio()*100, 2);


                    mrfd.CoopertitionSet = this.Result.BlueScore.CoopertitionSet;
                    mrfd.CoopertitionStack = this.Result.BlueScore.CoopertitionStack;
                }
                else
                {
                    mrfd.RedScore = 0;
                    mrfd.BlueScore = 0;

                    mrfd.CoopertitionSet = false;
                    mrfd.CoopertitionStack = false;
                }

                mrfd.Status = this.Status;
                mrfd.Winner = this.Winner;

                return mrfd;

            }
        }

        public class MatchResultsForDisplay
        {
            public string Type { get; set; }
            public string DisplayName { get; set; }
            public string Status { get; set; }
            public string RedAlliance { get; set; }
            public string BlueAlliance { get; set; }
            public int RedScore { get; set; }
            public int BlueScore { get; set; }
            public int RedAutoScore { get; set; }
            public int BlueAutoScore { get; set; }
            public int RedFoulPoints { get; set; }
            public int BlueFoulPoints { get; set; }
            public int RedCappedStacks { get; set; }
            public int BlueCappedStacks { get; set; }
            public double RedCanEfficiency { get; set; }
            public double BlueCanEfficiency { get; set; }
            public bool CoopertitionSet { get; set; }
            public bool CoopertitionStack { get; set; }
            public string Winner { get; set; }

            public MatchResultsForDisplay() { }
        }

        public class MatchPreviewForDisplay
        {
            public string Type { get; set; }
            public string DisplayName { get; set; }
            public string Status { get; set; }
            public int Red1 { get; set; }
            public int Red2 { get; set; }
            public int Red3 { get; set; }
            public int Blue1 { get; set; }
            public int Blue2 { get; set; }
            public int Blue3 { get; set; }
            public double RedQA { get; set; }
            public double RedCanAvg { get; set; }
            public double RedCoopAvg { get; set; }
            public double BlueQA { get; set; }
            public double BlueCanAvg { get; set; }
            public double BlueCoopAvg { get; set; }
            public string Winner { get; set; }


            public MatchPreviewForDisplay() { }
        }

        public class BracketMatch
        {
            public string DisplayName { get; set; }
            public string Status { get; set; }
            public string RedAlliance { get; set; }
            public int RedAllianceNum { get; set; }
            public string BlueAlliance { get; set; }
            public int BlueAllianceNum { get; set; }
            public string Winner { get; set; }

            public BracketMatch() { }
        }

        public class Result
        {
            public int Id { get; set; }
            public int MatchId { get; set; }
            public int PlayNumber { get; set; }
            public Score RedScore { get; set; }
            public Score BlueScore { get; set; }
            public Dictionary<string, string> RedCards { get; set; }
            public Dictionary<string, string> BlueCards { get; set; }

            public Result() { }
        }

        public class Score
        {
            public bool AutoRobotSet { get; set; }
            public bool AutoContainerSet { get; set; }
            public bool AutoToteSet { get; set; }
            public bool AutoStackedToteSet { get; set; }
            public List<Stack> Stacks { get; set; }
            public bool CoopertitionSet { get; set; }
            public bool CoopertitionStack { get; set; }
            public List<Foul> Fouls { get; set; }
            public bool ElimDq { get; set; }

            public Score() { }

            public int GetCoopPoints()
            {
                int score = 0;

                score += CoopertitionSet ? 20 : 0;
                score += CoopertitionStack ? 40 : 0;

                return score;
            }

            public int GetAutoPoints()
            {
                int score = 0;

                score += AutoRobotSet ? 4 : 0;
                score += AutoContainerSet ? 8 : 0;
                score += AutoToteSet ? 6 : 0;
                score += AutoStackedToteSet ? 20 : 0;

                return score;
            }

            public int GetFoulPoints()
            {
                int score = 0;

                if (Fouls != null)
                {
                    foreach (Foul f in Fouls)
                    {
                        score -= 6;
                    }
                }

                return score;
            }

            public int GetTotal()
            {
                int score = 0;

                score += GetAutoPoints();
                score += CoopertitionSet ? 20 : 0;
                score += CoopertitionStack ? 40 : 0;

                if (Stacks != null)
                {
                    foreach (Stack s in Stacks)
                    {
                        score += s.GetValue();
                    }
                }

                score += GetFoulPoints();

                return score;
            }

            public int GetCappedStacks()
            {
                int cappedStacks = 0;

                if (this.Stacks != null)
                {
                    foreach (Stack s in this.Stacks)
                    {
                        if (s.Container)
                        {
                            cappedStacks++;
                        }
                    }
                }
                return cappedStacks;
            }

            public double GetCanEfficiencyRatio()
            {                
                double cappedStacks = 0;
                double totalCappedStacksPoints = 0;

                if (this.Stacks != null)
                {
                    foreach (Stack s in this.Stacks)
                    {
                        if (s.Container)
                        {
                            cappedStacks++;
                            totalCappedStacksPoints += s.GetValue();
                        }
                    }
                }

                if (cappedStacks > 0)
                {
                    return totalCappedStacksPoints / (cappedStacks * 42);
                }
                else
                {
                    return 0;
                }

            }
        }

        public class Stack
        {
            public int Totes { get; set; }
            public bool Container { get; set; }
            public bool Litter { get; set; }

            public Stack() { }

            public int GetValue()
            {
                int value = 0;

                value += 2 * this.Totes;
                value += Container ? 4 * this.Totes : 0;
                value += Litter ? 6 : 0;

                return value;
            }
        }

        public class Foul
        {
            public int TeamId { get; set; }
            public string Rule { get; set; }
            public int TimeInMatchSec { get; set; }

            public Foul() { }
        }

        public class Ranking2016
        {
            public int TeamId { get; set; }
            public int Rank { get; set; }
            public int RankingPoints { get; set; }
            public int AutoPoints { get; set; }
            public int ScaleChallengePoints { get; set; }
            public int GoalPoints { get; set; }
            public int DefensePoints { get; set; }
            public int Wins {get; set;}
            public int Losses {get; set;}
            public int Ties {get; set;}
            public double Random { get; set; }
            public int Disqualifications { get; set; }
            public int Played { get; set; }
            public string Nickname { get; set; }

            public Ranking2016() { }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0}. {1} ({2} RP, {3}-{4}-{5})", this.Rank.ToString(), this.TeamId.ToString(), this.RankingPoints, this.Wins, this.Losses, this.Ties);

                return sb.ToString();
            }

            public string ToStringNoNumbers()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0} ({1})", this.TeamId.ToString(), string.Format("{0:N2}", this.RankingPoints));

                return sb.ToString();
            }
        }
    
        public class Ranking
        {
            public int TeamId { get; set; }
            public int Rank { get; set; }
            public double QualificationAverage { get; set; }
            public int CoopertitionPoints { get; set; }
            public int AutoPoints { get; set; }
            public int ContainerPoints { get; set; }
            public int TotePoints { get; set; }
            public int LitterPoints { get; set; }
            public double Random { get; set; }
            public int Disqualifications { get; set; }
            public int Played { get; set; }
            public string Nickname { get; set; }

            public Ranking() { }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0}. {1} ({2})", this.Rank.ToString(), this.TeamId.ToString(), string.Format("{0:N2}", this.QualificationAverage));

                return sb.ToString();
            }

            public string ToStringNoNumbers()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0} ({1})", this.TeamId.ToString(), string.Format("{0:N2}", this.QualificationAverage));

                return sb.ToString();
            }

            
        }

        public class RankingsList2016
        {
            public List<Ranking2016> Rankings { get; set; }

            public RankingsList2016() { }
        }

        public class RankingsList
        {
            public List<Ranking> Rankings { get; set; }

            public RankingsList() { }
        }


        public static List<Match> getMatches(string level)
        {
            string uri = Properties.Settings.Default.apiUrl + "/matches/" + level;
            string api_response = Communicator.sendAndGetRawResponse(uri);

            if (api_response != null)
            {
                List<Match> matches = JsonConvert.DeserializeObject<List<Match>>(api_response);
                return matches;
            }
            else
            {
                return null;
            }
        }

        public static List<Ranking2016> getRankings2016()
        {
            string uri = Properties.Settings.Default.apiUrl + "/rankings";
            string api_response = Communicator.sendAndGetRawResponse(uri);

            if (api_response != null)
            {
                List<Ranking2016> rankings = JsonConvert.DeserializeObject<RankingsList2016>(api_response).Rankings;
                return rankings;
            }
            else
            {
                return null;
            }
        }


        public static List<Ranking> getRankings()
        {
            string uri = Properties.Settings.Default.apiUrl + "/rankings";
            string api_response = Communicator.sendAndGetRawResponse(uri);

            if (api_response != null)
            {
                List<Ranking> rankings = JsonConvert.DeserializeObject<RankingsList>(api_response).Rankings;
                return rankings;
            }
            else
            {
                return null;
            }
        }

        public static bool updateAlliances()
        {
            List<Match> elimMatches = getMatches("elimination");
            alliances = new List<string>();

            if (elimMatches.Count > 0)
            {
                alliances.Add(elimMatches.Find(i => i.DisplayName.StartsWith("QF1-1")).GetRedAllianceString());
                alliances.Add(elimMatches.Find(i => i.DisplayName.StartsWith("QF3-1")).GetRedAllianceString());
                alliances.Add(elimMatches.Find(i => i.DisplayName.StartsWith("QF4-1")).GetRedAllianceString());
                alliances.Add(elimMatches.Find(i => i.DisplayName.StartsWith("QF2-1")).GetRedAllianceString());
                alliances.Add(elimMatches.Find(i => i.DisplayName.StartsWith("QF2-1")).GetBlueAllianceString());
                alliances.Add(elimMatches.Find(i => i.DisplayName.StartsWith("QF4-1")).GetBlueAllianceString());
                alliances.Add(elimMatches.Find(i => i.DisplayName.StartsWith("QF3-1")).GetBlueAllianceString());
                alliances.Add(elimMatches.Find(i => i.DisplayName.StartsWith("QF1-1")).GetBlueAllianceString());

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Communicates with the CC API
        /// </summary>
        private class Communicator
        {
            public static string sendAndGetRawResponse(string uri)
            {
                var request = System.Net.WebRequest.Create(uri) as System.Net.HttpWebRequest;
                request.KeepAlive = true;

                request.Method = "GET";

                request.Accept = "application/json";
                request.ContentLength = 0;

                //request.Headers.Add("reader1234Five");

                string responseContent = null;

                try
                {
                    using (var response = request.GetResponse() as System.Net.HttpWebResponse)
                    {
                        using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                        {
                            responseContent = reader.ReadToEnd();
                        }
                    }

                    return responseContent;
                }
                catch (Exception ex)
                {                    
                    Console.WriteLine(ex.Message);
                }
                return responseContent;
            }
        }
    }
}
