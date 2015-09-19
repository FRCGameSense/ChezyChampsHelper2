using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;
using System.Diagnostics;
using System.Threading;

namespace CCHelper2
{
    class GSTwitterClient
    {
        private string _consumerKey = "G5SzzVNoeHP5CcSSLPj0gYDeI";
        private string _consumerSecret = "XdJkL4CliuzwFRzT1vtgu5prGnE3LD3r9ulJYopkWFH3rQbvVA";
        private OAuthRequestToken _requestToken;
        public List<string[]> QuestionsList;


        private TwitterService service;

        public GSTwitterClient()
        {
            service = new TwitterService(_consumerKey, _consumerSecret);
            QuestionsList = new List<string[]>();
        }

        public void getPIN()
        {
            // Step 1 - Retrieve an OAuth Request Token
            _requestToken = service.GetRequestToken();

            // Step 2 - Redirect to the OAuth Authorization URL
            Uri uri = service.GetAuthorizationUri(_requestToken);
            Process.Start(uri.ToString());
        }

        public void Authenticate(string pin)
        {
            // Step 3 - Exchange the Request Token for an Access Token
            string verifier = pin;
            OAuthAccessToken access = service.GetAccessToken(_requestToken, verifier);

            // Step 4 - User authenticates using the Access Token
            service.AuthenticateWith(access.Token, access.TokenSecret);
            if (access.Token != "?")
            {
                Properties.Settings.Default.twitterAccessToken = access.Token;
                Properties.Settings.Default.twitterAccessTokenSecret = access.TokenSecret;
                Properties.Settings.Default.twitterVerified = true;
                Properties.Settings.Default.Save();
            }
        }

        public void Authenticate()
        {
            service.AuthenticateWith(Properties.Settings.Default.twitterAccessToken, Properties.Settings.Default.twitterAccessTokenSecret);
        }

        public void GetMentions()
        {
            IEnumerable<TwitterStatus> mentions = service.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions());

            //var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());
            foreach (var tweet in mentions)
            {
                Console.WriteLine("{0} says '{1}'", tweet.User.ScreenName, tweet.Text);
            }
        }

        public void StartGettingQuestions()
        {
            SearchOptions searchOptions = new SearchOptions();
            searchOptions.Q = Properties.Settings.Default.twitterSearchTag;
            searchOptions.Count = 30;
            searchOptions.Resulttype = TwitterSearchResultType.Mixed;
            searchOptions.IncludeEntities = false;
            TwitterSearchResult tweets = service.Search(searchOptions);

            if (tweets != null)
            {
                foreach (var tweet in tweets.Statuses)
                {
                    string[] question = { tweet.Text, tweet.User.ScreenName };
                    QuestionsList.Add(question);
                }
            }
        }

        public void Listen()
        {
            while (true)
            {

            }
        }
    }
}
