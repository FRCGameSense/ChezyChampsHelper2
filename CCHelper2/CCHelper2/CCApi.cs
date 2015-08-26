using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CCHelper2
{
    class CCApi
    {
        Communicator communicator = new Communicator();

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
        }

        public class Stack
        {
            public int Totes { get; set; }
            public bool Container { get; set; }
            public bool Litter { get; set; }

            public Stack() { }
        }

        public class Foul
        {
            public int TeamId { get; set; }
            public string Rule { get; set; }
            public int TimeInMatchSec { get; set; }

            public Foul() { }
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
        }

        public class RankingsList
        {
            public List<Ranking> Rankings { get; set; }

            public RankingsList() { }
        }


        public List<Match> getMatches(string level)
        {
            string uri = Properties.Settings.Default.apiUrl + "/matches/" + level;
            string api_response = communicator.sendAndGetRawResponse(uri);

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


        public List<Ranking> getRankings()
        {
            string uri = Properties.Settings.Default.apiUrl + "/rankings";
            string api_response = communicator.sendAndGetRawResponse(uri);

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

        /// <summary>
        /// Communicates with the CC API
        /// </summary>
        private class Communicator
        {
            public string sendAndGetRawResponse(string uri)
            {
                var request = System.Net.WebRequest.Create(uri) as System.Net.HttpWebRequest;
                request.KeepAlive = true;

                request.Method = "GET";

                request.Accept = "application/json";
                request.ContentLength = 0;

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
