using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using System.Threading;

namespace CCHelper2
{
    class GSTwitchClient
    {
        private IrcClient irc = new IrcClient();
        private string server = "irc.twitch.tv";
        private int port = 6667;
        private string channel = "#frcgamesense";
        private string oauth = "oauth:060e3h8ki99kxkfstsjyskwqc6k0s4";
        private List<string[]> questions = new List<string[]>();
        private GSBotCommands messageCommands = new GSBotCommands();
        public static Thread twitchThread;
        private string status;

        public string Status
        {
            get
            {
                return status;
            }
        }

        public List<string[]> QuestionsList
        {
            get
            {
                return questions;
            }
            set
            {
                //shouldn't set
            }
        }

        public GSTwitchClient(string myChannel)
        {
            this.channel = myChannel;
            irc.OnConnected += new EventHandler(OnConnected);
            irc.OnConnecting += new EventHandler(OnConnecting);
            irc.OnDisconnected += new EventHandler(OnDisconnected);
            irc.OnChannelMessage += new IrcEventHandler(OnChannelMessage);

            irc.AutoRetry = true;
            irc.AutoRetryDelay = 10;
            irc.AutoReconnect = true;
        }

        public void Connect()
        {
            try
            {
                irc.Connect(server, port);
            }
            catch (Exception e)
            {
                status = "Failed to connect: " + e.Message;
                Console.WriteLine("Failed to connect:n" + e.Message);
                Console.ReadKey();
            }
        }

        void OnPing(object sender, PingEventArgs e)
        {
            Console.WriteLine("Responded to ping at {0}", DateTime.Now.ToShortTimeString());
        }

        void OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected.");
            status = "Connected";
            irc.Login("gamesensebot", "GameSense Bot", 0, "gamesensebot", oauth);
            irc.RfcJoin(channel);
        }

        public void Listen()
        {
            twitchThread = new Thread(new ThreadStart(irc.Listen));
            twitchThread.Start();
        }

        public void Stop()
        {
            irc.Disconnect();
            status = "Disconnected";
            if (twitchThread != null)
            {
                twitchThread.Abort();
            }
        }

        void OnConnecting(object sender, EventArgs e)
        {
            status = "Connecting";
            Console.WriteLine("Connecting to {0}", server);
        }

        void OnDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected");
            status = "Disconnected";
        }

        void SendChannelMessage(string message)
        {
            irc.SendMessage(SendType.Message, channel, message, Priority.BelowMedium);
        }

        /// <summary>
        /// Recieve a message, process it, and handle it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnChannelMessage(object sender, IrcEventArgs e)
        {
            Console.WriteLine(e.Data.Type + ".");
            Console.WriteLine("(" + e.Data.Channel + ") <" + e.Data.Nick + "> " + e.Data.Message);

            //pull the message out of the object for ease of use
            string rawMessage = e.Data.Message;


            if (rawMessage.StartsWith("!"))
            {
                string[] messageArray;
                if (rawMessage.Contains(' '))
                {
                    messageArray = rawMessage.Split(' ');
                }
                else
                {
                    messageArray = new string[1] { rawMessage };
                }

                //check to see if the first item in the array matches one of a predefined set of commands
                if (messageArray[0] == messageCommands.Question.Command || messageArray[0] == messageCommands.QuestionShort.Command)
                {
                    //if its a question, add the question to the questions list.
                    parseAndAddQuestion(messageArray, e.Data.Nick);
                }
            }
        }

        private void SendErrorMessage(string error, string badCommand, string nick)
        {
            SendChannelMessage(nick + ", there was an error processing \"" + badCommand + "\": " + error);
            return;
        }

        /// <summary>
        /// Adds a question to the questions list.
        /// </summary>
        /// <param name="messageArray">The array of strings representing a question</param>
        /// <param name="nick">The nickname of the individual that asked the question</param>
        private void parseAndAddQuestion(string[] messageArray, string nick)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in messageArray)
            {
                if (!str.StartsWith("!"))
                {
                    if (str != messageArray.Last())
                    {
                        sb.Append(str + " ");
                    }
                    else
                    {
                        sb.Append(str);
                    }
                }

            }

            string parsed = sb.ToString();

            string[] question = { parsed, nick };
            questions.Add(question);
            try
            {
                SendChannelMessage("Thanks, " + nick + ". I've added  your question to the queue.");
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        /// <summary>
        /// Determines if a string contains only numerical digits.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>true if a string contains only numerical digits, false otherwise.</returns>
        public bool isNum(string str)
        {
            int teamNum;
            return int.TryParse(str, out teamNum);
        }
    }
}
