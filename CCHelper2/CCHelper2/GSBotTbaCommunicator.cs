using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace CCHelper2
{
    class GSBotTbaCommunicator
    {
        public const string tbaBaseUrl = "http://www.thebluealliance.com/api/v2";

        /// <summary>
        /// Returns a TBA Team Model fetched from thebluealliance.com
        /// </summary>
        /// <param name="teamKey">The team to get (ex. "frc254").</param>
        /// <returns>The team model corresponding to the requested team key.</returns>
        public TBATeam getTeam(string teamKey)
        {
            string url = tbaBaseUrl + "/team/" + teamKey;
            try
            {
                string json = getTbaJsonString(url);
                TBATeam team = JsonConvert.DeserializeObject<TBATeam>(json);
                return team;
            }
            catch (Exception ex)
            {
                if (ex.Message == "The remote server returned an error: (404) Not Found.")
                {
                    throw new Exception("Team does not exist in TBA's database.");
                }
                else
                {
                    throw new Exception("An unknown error occurred");
                }
            }
        }

        /// <summary>
        /// Returns a list of TBA Match Models for the given team at the given event.
        /// </summary>
        /// <param name="teamKey">The team to get the matches for (ex. "frc254")</param>
        /// <param name="eventKey">The event to get the matches for (ex. "2014casj")</param>
        /// <returns></returns>
        public List<TBAMatch> getTeamEventMatches(string teamKey, string eventKey)
        {
            string url = tbaBaseUrl + "/team/" + teamKey + "/event/" + eventKey + "/matches";
            string json = getTbaJsonString(url);
            return JsonConvert.DeserializeObject<List<TBAMatch>>(json);
        }

        /// <summary>
        /// Gets a list of events the team is registered for this year.
        /// </summary>
        /// <param name="teamKey">The team to get the events for (ex. "frc254")</param>
        /// <returns>A list of TBAEvents that the team is registered for this year.</returns>
        public List<TBAEvent> getTeamEvents(string teamKey)
        {
            string url = tbaBaseUrl + "/team/" + teamKey + "/events";
            string json = getTbaJsonString(url);
            return JsonConvert.DeserializeObject<List<TBAEvent>>(json);
        }

        /// <summary>
        /// Gets a list of events the team is registered for for a specific year.
        /// </summary>
        /// <param name="teamKey">The team to get the events for (ex. "frc254")</param>
        /// <param name="year">The requested year.</param>
        /// <returns>A list of TBAEvents the team is registered for for that year.</returns>
        public List<TBAEvent> getTeamEvents(string teamKey, string year)
        {
            string url = tbaBaseUrl + "/team/" + teamKey + "/" + year + "/events";
            string json = getTbaJsonString(url);
            return JsonConvert.DeserializeObject<List<TBAEvent>>(json);
        }

        /// <summary>
        /// Gets all matches for a requested team this year.
        /// </summary>
        /// <param name="teamKey">The team to get the events for (ex. "frc254")</param>
        /// <returns>A list of TBAMatches for that team this year.</returns>
        public List<TBAMatch> getAllTeamMatches(string teamKey)
        {
            List<TBAMatch> matches = new List<TBAMatch>();

            List<TBAEvent> events = getTeamEvents(teamKey);

            foreach (TBAEvent evt in events)
            {
                matches.AddRange(getTeamEventMatches(teamKey, evt.year + evt.event_code));
            }
            return matches;
        }

        /// <summary>
        /// Gets all matches for a requested team for a specific year.
        /// </summary>
        /// <param name="teamKey">The team to get the events for (ex. "frc254")</param>
        /// <param name="year">The requested year.</param>
        /// <returns>A list of TBAMatches for the requested team for the requested year.</returns>
        public List<TBAMatch> getAllTeamMatches(string teamKey, string year)
        {
            List<TBAMatch> matches = new List<TBAMatch>();

            List<TBAEvent> events = getTeamEvents(teamKey, year);

            foreach (TBAEvent evt in events)
            {
                matches.AddRange(getTeamEventMatches(teamKey, evt.year + evt.event_code));
            }
            return matches;
        }

        /// <summary>
        /// Gets a specified event
        /// </summary>
        /// <param name="eventKey">The TBA event key for the event.</param>
        /// <returns>The requested TBAEvent.</returns>
        public TBAEvent getEvent(string eventKey)
        {
            string url = tbaBaseUrl + "/event/" + eventKey;
            string json = getTbaJsonString(url);

            return JsonConvert.DeserializeObject<TBAEvent>(json);
        }


        /// <summary>
        /// Returns a string fetched from the TBA API at the specified url
        /// </summary>
        /// <param name="url">The constructed url.</param>
        /// <returns>A JSON parsable string</returns>
        public string getTbaJsonString(string url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            //Add the necessary security headers.
            request.Headers.Add("X-TBA-App-Id", "gamesense:gamesensebot:v01");

            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            return reader.ReadToEnd();
        }

        /// <summary>
        /// Calculates win-loss-tie for a given team in a given list of matches.
        /// </summary>
        /// <param name="teamKey">The team in question (ex. "frc254")</param>
        /// <param name="matches">The list of matches the team was in.</param>
        /// <returns>A TBATeamRecord object containing the record of the team.</returns>
        public TBATeamRecord getTeamRecordAtEvent(string teamKey, List<TBAMatch> matches)
        {
            TBATeamRecord record = new TBATeamRecord();

            foreach (TBAMatch match in matches)
            {
                if (match.alliances.blue.score > match.alliances.red.score)
                {
                    if (match.alliances.blue.teams.Contains(teamKey))
                    {
                        record.wins++;
                    }
                    else
                    {
                        record.losses++;
                    }
                }
                else if (match.alliances.red.score > match.alliances.blue.score)
                {
                    if (match.alliances.blue.teams.Contains(teamKey))
                    {
                        record.losses++;
                    }
                    else
                    {
                        record.wins++;
                    }
                }
                else if (match.alliances.blue.score == match.alliances.red.score)
                {
                    record.ties++;
                }
            }

            return record;
        }

        /// <summary>
        /// Gets a list of events on TBA for a specified year
        /// </summary>
        /// <param name="year">The year to get events for.</param>
        /// <returns></returns>
        public List<TBAEvent> getEventsByYear(int year)
        {
            string url = tbaBaseUrl + "/events/" + year.ToString();
            string json = getTbaJsonString(url);
            return JsonConvert.DeserializeObject<List<TBAEvent>>(json);
        }

        /// <summary>
        /// Gets a list of awards given out at the specified event
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public List<TBAEventAward> getEventAwards(string eventCode, int? year)
        {
            string url = tbaBaseUrl + "/event/" + year.ToString() + eventCode + "/awards";
            string json = getTbaJsonString(url);
            return JsonConvert.DeserializeObject<List<TBAEventAward>>(json);
        }
    }
}
