using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Resources;

namespace CCHelper2
{
    public partial class Form1 : Form
    {
        xSplitHandler2 xsHandler;
        SettingsForm settingsForm;
        List<string[]> QuestionsAddedList = new List<string[]>();
        GSTwitchClient twitch;
        GSTwitterClient twitter;
        DataTable dataTable1, dataTable2;
        BindingSource source1, source2;
        int liveTime;
        GSBotTbaCommunicator tba = new GSBotTbaCommunicator();
        CCApi ccApi = new CCApi();

        public Form1()
        {
            InitializeComponent();
            xsHandler = new xSplitHandler2();
            dataTable1 = new DataTable();
            dataTable2 = new DataTable();

            dataTable1.RowChanged += dataTable1_RowChanged;

            source1 = new BindingSource();
            source1.DataSource = dataTable1;
            source2 = new BindingSource();
            source2.DataSource = dataTable2;

            dataTable1.Columns.Add("Question", typeof(string));
            dataTable1.Columns.Add("Author", typeof(string));
            dataTable2.Columns.Add("Question", typeof(string));
            dataTable2.Columns.Add("Author", typeof(string));

            dataGridView1.DataSource = source1;
            dataGridView1.Columns[0].Width = 320;
            dataGridView1.Columns[1].Width = 80;
            dataGridView2.DataSource = source2;
            dataGridView2.Columns[0].Width = 320;
            dataGridView2.Columns[1].Width = 80;

            toolTip1.SetToolTip(this.timeLabel, "Click - Play/Pause \nDouble Click - Reset");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ShutDown();
        }

        public void ShutDown()
        {
            twitch.Stop();
            timer1.Stop();
            Properties.Settings.Default.timerRunning = false;
            Properties.Settings.Default.currentTopic = 0;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.firstInstall)
            {
                MessageBox.Show("Welcome to CC Helper!  Please go to settings and set your file paths.");
                if (!xsHandler.checkXsplitLocation(Properties.Settings.Default.XsplitInstallLocation))
                {
                    Properties.Settings.Default.XsplitInstallLocation = "unknown";
                    Properties.Settings.Default.Save();
                }
                settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
            }

            if (!xsHandler.checkXsplitLocation(Properties.Settings.Default.XsplitInstallLocation))
            {
                MessageBox.Show("Unable to locate XSplit installation in the location saved in settings, please go to Settings to locate manually.");
            }
            else
            {
                if (Properties.Settings.Default.firstInstall)
                {
                    Properties.Settings.Default.firstInstall = false;
                    Properties.Settings.Default.Save();

                    string xml = System.IO.Path.Combine(Properties.Settings.Default.GSFolderLocation, "Software", "streamcontrol_base.xml");
                    xsHandler.copyStreamControlXMLToXsplitLocation(Properties.Settings.Default.XsplitInstallLocation);
                }
                Properties.Settings.Default.Save();
            }

            //load these from settings
            Host1ComboBox.Text = Properties.Settings.Default.Host1;
            Host2ComboBox.Text = Properties.Settings.Default.Host2;
            Guest1ComboBox.Text = Properties.Settings.Default.Guest1;
            Guest2ComboBox.Text = Properties.Settings.Default.Guest2;
            Host3ComboBox.Text = Properties.Settings.Default.Host3;
            Host4ComboBox.Text = Properties.Settings.Default.Host4;

            twitterHashtagBox.Text = Properties.Settings.Default.twitterSearchTag;

            dataGridView1.Columns[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;

            xsHandler = new xSplitHandler2();
            Properties.Settings.Default.timerRunning = false;
            Properties.Settings.Default.currentTopic = 0;
            verticalTickerDataGridView.Rows.Add("CLICK HERE TO ADD");

            Properties.Settings.Default.Save();

            dateTextBox.Text = DateTime.Now.ToShortDateString();
        }

        /// <summary>
        /// Navigates to the next topic and publishes it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextButton_Click(object sender, EventArgs e)
        {
            //if there are topics
            if (verticalTickerDataGridView.Rows.Count > 0)
            {
                //if you're not out of topics, keep going
                if (Properties.Settings.Default.currentTopic < verticalTickerDataGridView.Rows.Count - 1)
                {
                    verticalTickerDataGridView.ClearSelection();
                    Properties.Settings.Default.currentTopic++;
                    //select the current topic
                    verticalTickerDataGridView.Rows[Properties.Settings.Default.currentTopic].Selected = true;
                }

                Properties.Settings.Default.Save();
            }
            publishTopics();
        }

        private void prevTopicButton_Click(object sender, EventArgs e)
        {
            //if there are topics
            if (verticalTickerDataGridView.Rows.Count > 0)
            {
                //select the previous topic
                if (Properties.Settings.Default.currentTopic > 0)
                {
                    verticalTickerDataGridView.ClearSelection();
                    verticalTickerDataGridView.Rows[Properties.Settings.Default.currentTopic - 1].Selected = true;
                    Properties.Settings.Default.currentTopic--;
                }
                Properties.Settings.Default.Save();
            }
            publishTopics();
        }

        private void verticalTickerDataGridView_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void publishButton_Click(object sender, EventArgs e)
        {
            verticalTickerDataGridView.ClearSelection();
            verticalTickerDataGridView.Rows[Properties.Settings.Default.currentTopic].Selected = true;
            publishTopics();

            switch (tabControl1.SelectedTab.Name)
            {
                case "castersTab":
                    publishCasters();
                    break;
                case "questionsTab":
                    publishQuestion();
                    break;
                case "splashTab":
                    publishSplashTab();
                    break;
                case "tickerTab":
                    publishTicker();
                    break;
                default:
                    break;
            }
        }

        private void publishTicker()
        {
            xsHandler.loadTagsFromXML();

            xsHandler.changeXMLTag("tickerText", tickerTextBox.Text, false);

            xsHandler.writeXMLFile();
        }

        private void publishSplashTab()
        {
            xsHandler.loadTagsFromXML();

            xsHandler.changeXMLTag("splashTop", splashTopBox.Text, false);
            xsHandler.changeXMLTag("splashBottom", splashBottomBox.Text, false);

            xsHandler.writeXMLFile();
        }

        private void publishQuestion()
        {
            try
            {
                xsHandler.loadTagsFromXML();

                xsHandler.changeXMLTag("question", Properties.Settings.Default.PublishedQuestion, true);
                if (Properties.Settings.Default.PublishedAuthor == "")
                    xsHandler.changeXMLTag("author", Properties.Settings.Default.PublishedAuthor, true);
                else
                    xsHandler.changeXMLTag("author", "-" + Properties.Settings.Default.PublishedAuthor, true);

                xsHandler.writeXMLFile();
            }
            catch (Exception)
            {
                MessageBox.Show("No question selected!");
            }

        }

        public void publishCasters()
        {
            //save these to settings so they can be referenced outside this tab.
            Properties.Settings.Default.Host1 = Host1ComboBox.Text;
            Properties.Settings.Default.Host2 = Host2ComboBox.Text;
            Properties.Settings.Default.Host3 = Host3ComboBox.Text;
            Properties.Settings.Default.Host4 = Host4ComboBox.Text;
            Properties.Settings.Default.Guest1 = Guest1ComboBox.Text;
            Properties.Settings.Default.Guest2 = Guest2ComboBox.Text;


            Properties.Settings.Default.Save();

            xsHandler.loadTagsFromXML();

            xsHandler.changeXMLTag("host1", Properties.Settings.Default.Host1, true);
            xsHandler.changeXMLTag("host2", Properties.Settings.Default.Host2, true);
            xsHandler.changeXMLTag("host3", Properties.Settings.Default.Host3, true);
            xsHandler.changeXMLTag("host4", Properties.Settings.Default.Host4, true);
            xsHandler.changeXMLTag("guest1", Properties.Settings.Default.Guest1, true);
            xsHandler.changeXMLTag("guest2", Properties.Settings.Default.Guest2, true);

            xsHandler.writeXMLFile();
        }

        public void publishTopics()
        {
            int currentTopicNum = Properties.Settings.Default.currentTopic;
            int topicsLeft = (verticalTickerDataGridView.Rows.Count - currentTopicNum) - 1;

            xsHandler.loadTagsFromXML();

            if (topicsLeft >= 8)
            {
                for (int i = 0; i < 8; i++)
                {
                    xsHandler.changeXMLTag("topic" + i, verticalTickerDataGridView.Rows[currentTopicNum + i].Cells[0].Value.ToString(), false);
                }
            }
            else
            {
                for (int i = 0; i < topicsLeft; i++)
                {
                    xsHandler.changeXMLTag("topic" + i, verticalTickerDataGridView.Rows[currentTopicNum + i].Cells[0].Value.ToString(), false);
                }
                for (int i = topicsLeft; i < 8; i++)
                {
                    xsHandler.changeXMLTag("topic" + i, "", false);
                }

            }

            xsHandler.writeXMLFile();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            liveTime++;

            updateTimeLabel();
        }

        private void timeLabel_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.timerRunning)
            {
                timer1.Stop();
                timeLabel.ForeColor = Color.Gray;
                Properties.Settings.Default.timerRunning = false;
            }
            else
            {
                timer1.Start();
                timeLabel.ForeColor = Color.White;
                Properties.Settings.Default.timerRunning = true;
            }

            Properties.Settings.Default.Save();
        }

        private void timeLabel_DoubleClick(object sender, EventArgs e)
        {
            timer1.Stop();
            liveTime = 0;
            timeLabel.ForeColor = Color.White;
            Properties.Settings.Default.timerRunning = false;
            updateTimeLabel();

        }

        public void updateTimeLabel()
        {
            var span = new TimeSpan(0, 0, liveTime);
            var str = span.ToString(@"mm\:ss");

            timeLabel.Text = str;

        }

        private void dataGridView2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = dataGridView2.HitTest(e.X, e.Y).RowIndex;

                if (currentMouseOverRow >= 0)
                {
                    dataGridView2.ClearSelection();
                    dataGridView2.Rows[currentMouseOverRow].Selected = true;

                    ContextMenu m = new ContextMenu();
                    MenuItem menuPublishQuestion = new MenuItem("Publish");
                    MenuItem menuMoveToTop = new MenuItem("Move To Top");
                    MenuItem menuDeleteQuestion = new MenuItem("Delete");
                    MenuItem menuCopyNextQuestionPrompt = new MenuItem("Copy Next Question Prompt");

                    menuMoveToTop.Click += new EventHandler(menuQueueMoveToTop_Click);
                    menuDeleteQuestion.Click += new EventHandler(menuQueueDeleteQuestion_Click);
                    menuPublishQuestion.Click += new EventHandler(menuPublishQuestion_Click);
                    menuCopyNextQuestionPrompt.Click += new EventHandler(menuCopyNextQuestionPrompt_Click);

                    m.MenuItems.Add(menuPublishQuestion);
                    m.MenuItems.Add(menuMoveToTop);
                    m.MenuItems.Add(menuDeleteQuestion);
                    m.MenuItems.Add(menuCopyNextQuestionPrompt);
                    m.Show(dataGridView2, new Point(e.X, e.Y));
                }
            }
        }

        private void menuCopyNextQuestionPrompt_Click(object sender, EventArgs e)
        {
            DataRow qRow = (DataRow)dataTable2.Rows[dataGridView2.Rows.GetFirstRow(DataGridViewElementStates.Selected)];

            string nextQuestion = qRow[0].ToString();
            Clipboard.SetText("Next Question: \"" + nextQuestion + "\"");
        }

        private void menuPublishQuestion_Click(object sender, EventArgs e)
        {
            DataRow qRow = (DataRow)dataTable2.Rows[dataGridView2.Rows.GetFirstRow(DataGridViewElementStates.Selected)];
            Properties.Settings.Default.PublishedQuestion = qRow[0].ToString();
            Properties.Settings.Default.PublishedAuthor = qRow[1].ToString();

            Properties.Settings.Default.Save();

            publishQuestion();
        }

        private void menuQueueDeleteQuestion_Click(object sender, EventArgs e)
        {
            MenuItem clickedItem = sender as MenuItem;
            dataTable2.Rows.RemoveAt(dataGridView2.Rows.GetFirstRow(DataGridViewElementStates.Selected));
            dataGridView2.ClearSelection();

        }

        private void menuQueueMoveToTop_Click(object sender, EventArgs e)
        {
            MenuItem clickedItem = sender as MenuItem;
            int index = dataGridView2.Rows.GetFirstRow(DataGridViewElementStates.Selected);
            DataRow qRow = dataTable2.Rows[index];
            DataRow newRow = dataTable2.NewRow();
            newRow.ItemArray = qRow.ItemArray;
            dataTable2.Rows.RemoveAt(index);
            dataTable2.Rows.InsertAt(newRow, 0);
            dataGridView2.ClearSelection();
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = dataGridView1.HitTest(e.X, e.Y).RowIndex;

                if (currentMouseOverRow >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[currentMouseOverRow].Selected = true;

                    ContextMenu m = new ContextMenu();
                    MenuItem menuQueueQuestion = new MenuItem("Add To Queue");
                    MenuItem menuDeleteQuestion = new MenuItem("Delete");

                    menuQueueQuestion.Click += new EventHandler(menuQueueQuestion_Click);
                    menuDeleteQuestion.Click += new EventHandler(menuDeleteQuestion_Click);

                    m.MenuItems.Add(menuQueueQuestion);
                    m.MenuItems.Add(menuDeleteQuestion);
                    m.Show(dataGridView1, new Point(e.X, e.Y));
                }
            }
        }

        private void menuQueueQuestion_Click(object sender, EventArgs e)
        {
            MenuItem clickedItem = sender as MenuItem;
            DataRow qRow = (DataRow)dataTable1.Rows[dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected)];
            dataTable2.ImportRow(qRow);
            dataGridView1.ClearSelection();
        }

        private void menuDeleteQuestion_Click(object sender, EventArgs e)
        {
            MenuItem clickedItem = sender as MenuItem;
            dataTable1.Rows.RemoveAt(dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected));
            dataGridView1.ClearSelection();

        }

        private void clearQuestionButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PublishedAuthor = "";
            Properties.Settings.Default.PublishedQuestion = "";
            Properties.Settings.Default.Save();

            xsHandler.changeXMLTag("question", Properties.Settings.Default.PublishedQuestion, true);
            if (Properties.Settings.Default.PublishedAuthor == "")
                xsHandler.changeXMLTag("author", Properties.Settings.Default.PublishedAuthor, true);
            else
                xsHandler.changeXMLTag("author", "-" + Properties.Settings.Default.PublishedAuthor, true);

            xsHandler.writeXMLFile();
        }

        private void clearQuestionsButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("This cannot be undone.", "Are you sure?", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                dataTable1.Rows.Clear();
                QuestionsAddedList.Clear();
            }
        }

        private void manualQuestionButton_Click(object sender, EventArgs e)
        {
            BTLQuestionForm manualForm = new BTLQuestionForm();
            manualForm.FormClosed += manualForm_FormClosed;

            manualForm.ShowDialog();
        }

        private void manualForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Properties.Settings.Default.ManualQuestion != "")
            {
                DataRow row = dataTable1.NewRow();
                row["question"] = Properties.Settings.Default.ManualQuestion;
                row["author"] = Properties.Settings.Default.ManualAuthor;
                dataTable1.Rows.Add(row);
            }
        }

        private void twitterGetQuestionsButton_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.twitterVerified)
            {
                Properties.Settings.Default.twitterSearchTag = twitterHashtagBox.Text;
                Properties.Settings.Default.Save();

                twitter = new GSTwitterClient();
                twitter.Authenticate();
                twitter.StartGettingQuestions();

                IEnumerable<string[]> questionsNotAdded = twitter.QuestionsList.Except(QuestionsAddedList, new QuestionComparer());

                foreach (string[] q in questionsNotAdded)
                {
                    DataRow row = dataTable1.NewRow();
                    row["question"] = q[0];
                    row["author"] = q[1];
                    dataTable1.Rows.Add(q);
                }

                QuestionsAddedList = twitter.QuestionsList;
            }
        }

        class QuestionComparer : IEqualityComparer<string[]>
        {
            public bool Equals(string[] s1, string[] s2)
            {
                if (s1[0] == s2[0] && s1[1] == s2[1])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public int GetHashCode(string[] s)
            {
                int hCode = s[0].Length + s[1].Length;
                return hCode.GetHashCode();
            }
        }

        private void twitchGetQuestionsButton_Click(object sender, EventArgs e)
        {
            if (twitchGetQuestionsButton.Text == "Start Getting Questions")
            {
                twitch = new GSTwitchClient("#frcgamesense");
                twitch.Connect();
                twitch.Listen();
                backgroundWorker1.RunWorkerAsync();
                twitchGetQuestionsButton.Text = "Stop Getting Questions";
            }
            else
            {
                twitch.Stop();
                backgroundWorker1.CancelAsync();
                twitchGetQuestionsButton.Text = "Start Getting Questions";
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (!worker.CancellationPending)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    worker.ReportProgress(0);
                    Thread.Sleep(1000);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, EventArgs e)
        {
            if (twitch.QuestionsList.Count > 0)
            {
                foreach (string[] array in twitch.QuestionsList)
                {
                    DataRow row = dataTable1.NewRow();
                    row["question"] = array[0];
                    row["author"] = array[1];
                    dataTable1.Rows.Add(row);
                }
                twitch.QuestionsList.Clear();
            }

            String currentTime = DateTime.Now.ToString("HH:mm") + " PT";
            String currentDate = DateTime.Now.ToString("MM/dd/yy");

            xsHandler.loadTagsFromXML();

            xsHandler.changeXMLTag("timeDate", currentTime + " " + currentDate, false);

            xsHandler.writeXMLFile();
        }

        private void dataTable1_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (!Properties.Settings.Default.ImportingQuestions)
            {
                StringBuilder sb = new StringBuilder();
                //sb.AppendLine("Question, Author");
                foreach (DataRow row in dataTable1.Rows)
                {
                    sb.AppendLine(row.ItemArray[0].ToString() + '\t' + row.ItemArray[1].ToString());
                }

                string backupFile = Path.Combine(Properties.Settings.Default.GSFolderLocation, "Documents", "GSQuestionsBackup.txt");

                File.WriteAllText(backupFile, sb.ToString());
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            string questionsBackupFile = Path.Combine(Properties.Settings.Default.GSFolderLocation, "Documents", "GSQuestionsBackup.txt");
            if (File.Exists(questionsBackupFile))
            {
                DialogResult result = MessageBox.Show("Found a questions backup file. \n\nWould you like to import the backed up questions?", "Backup File Found", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    importBackedUpQuestions(questionsBackupFile);
                }
            }
        }

        private void importBackedUpQuestions(string filePath)
        {
            if (File.Exists(filePath))
            {
                StreamReader fileReader = new StreamReader(filePath, false);
                Properties.Settings.Default.ImportingQuestions = true;
                while (fileReader.Peek() != -1)
                {
                    string row = fileReader.ReadLine();
                    string[] question = row.Split('\t');
                    dataTable1.Rows.Add(question);
                    Console.WriteLine(dataTable1.Rows[0].ItemArray[0]);
                    QuestionsAddedList.Add(question);
                }
                fileReader.Dispose();
                fileReader.Close();
            }
            Properties.Settings.Default.ImportingQuestions = false;
        }

        private void timeLabel_MouseDown(object sender, MouseEventArgs e)
        {
            timeLabel.BackColor = Color.FromArgb(228, 97, 22);
        }

        private void timeLabel_MouseUp(object sender, MouseEventArgs e)
        {
            timeLabel.BackColor = Color.FromArgb(50, 44, 40);
        }

        private void lastWeeksResultsButton_Click(object sender, EventArgs e)
        {
            DateTime thisDay = Convert.ToDateTime(dateTextBox.Text);
            List<TBAEvent> events = tba.getEventsByYear(thisDay.Year);

            StringBuilder sb = new StringBuilder();

            foreach (TBAEvent evt in events)
            {
                DateTime startDate = Convert.ToDateTime(evt.start_date);

                if ((startDate - thisDay).TotalDays >= -7 && (startDate - thisDay).TotalDays < 0)
                {
                    List<TBAEventAward> awards = tba.getEventAwards(evt.event_code, evt.year);

                    if (awards.Count > 0)
                    {
                        sb.Append(evt.name + ": ");

                        sb.Append("Chairman's (");
                        TBAEventAward ca = awards.Find(x => x.award_type.Equals(0));
                        sb.Append(ca.recipient_list.First<TBAAwardRecipient>().team_number.ToString() + "),");


                        sb.Append(" Winners (");
                        TBAEventAward winners = awards.Find(x => x.award_type.Equals(1));
                        int counter = 1;
                        foreach (TBAAwardRecipient rcpt in winners.recipient_list)
                        {
                            sb.Append(rcpt.team_number.ToString());
                            if (counter < winners.recipient_list.Count)
                            {
                                sb.Append(",");
                            }
                            else
                            {
                                sb.Append("),");
                            }
                            counter++;
                        }

                        sb.Append(" Finalists (");
                        TBAEventAward finalists = awards.Find(x => x.award_type.Equals(2));
                        counter = 1;
                        foreach (TBAAwardRecipient rcpt in finalists.recipient_list)
                        {
                            sb.Append(rcpt.team_number.ToString());
                            if (counter < finalists.recipient_list.Count)
                            {
                                sb.Append(",");
                            }
                            else
                            {
                                sb.Append(") | ");
                            }
                            counter++;
                        }
                    }
                }
            }
            string resultsString = sb.ToString();
            tickerTextBox.Text = resultsString.Substring(0, resultsString.Length - 3);
        }

        private void nextWeeksResultsButton_Click(object sender, EventArgs e)
        {
            DateTime thisDay = Convert.ToDateTime(dateTextBox.Text);
            List<TBAEvent> events = tba.getEventsByYear(thisDay.Year);

            StringBuilder sb = new StringBuilder();

            foreach (TBAEvent evt in events)
            {
                DateTime startDate = Convert.ToDateTime(evt.start_date);

                if ((startDate - thisDay).TotalDays >= 0 && (startDate - thisDay).TotalDays < 7)
                {
                    sb.Append(evt.name + " (");
                    DateTime start = Convert.ToDateTime(evt.start_date);
                    DateTime end = Convert.ToDateTime(evt.end_date);
                    sb.Append(start.Month + "/" + start.Day + "-" + end.Month + "/" + end.Day + ") | ");
                }
            }

            string eventsString = sb.ToString();

            tickerTextBox.Text = eventsString.Substring(0, eventsString.Length - 3);
        }

        private void verticalTickerDataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = verticalTickerDataGridView.HitTest(e.X, e.Y).RowIndex;

                if (currentMouseOverRow >= 0)
                {
                    ContextMenu m = new ContextMenu();
                    MenuItem menuInsertTopics = new MenuItem("Insert");
                    MenuItem menuDeleteTopics = new MenuItem("Delete");

                    menuInsertTopics.Click += new EventHandler(menuInsertTopics_Click);
                    menuDeleteTopics.Click += new EventHandler(menuDeleteTopics_Click);

                    m.MenuItems.Add(menuInsertTopics);
                    m.MenuItems.Add(menuDeleteTopics);
                    m.Show(verticalTickerDataGridView, new Point(e.X, e.Y));
                }
            }
        }

        private void menuInsertTopics_Click(object sender, EventArgs e)
        {
            MenuItem clickedItem = sender as MenuItem;

            int topMostIndex = 1000;
            foreach (DataGridViewCell cell in verticalTickerDataGridView.SelectedCells)
            {
                if (cell.RowIndex < topMostIndex)
                {
                    topMostIndex = cell.RowIndex;
                }
            }

            foreach (DataGridViewCell selectedCell in verticalTickerDataGridView.SelectedCells)
            {
                DataGridViewRow insertCell = new DataGridViewRow();
                verticalTickerDataGridView.Rows.Insert(topMostIndex, insertCell);
            }

        }

        private void menuDeleteTopics_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell selectedCell in verticalTickerDataGridView.SelectedCells)
            {
                verticalTickerDataGridView.Rows.RemoveAt(selectedCell.RowIndex);
            }
        }

        private void clearQuestionButton_Click_1(object sender, EventArgs e)
        {
            Properties.Settings.Default.PublishedAuthor = "";
            Properties.Settings.Default.PublishedQuestion = "";
            Properties.Settings.Default.Save();

            xsHandler.loadTagsFromXML();

            xsHandler.changeXMLTag("question", Properties.Settings.Default.PublishedQuestion, true);
            xsHandler.changeXMLTag("author", Properties.Settings.Default.PublishedAuthor, true);

            xsHandler.writeXMLFile();
        }

        private void getRankingsButton_Click(object sender, EventArgs e)
        {
            List<CCApi.Ranking> rankings = ccApi.getRankings();
        }

        private void getResultsButton_Click(object sender, EventArgs e)
        {
            List<CCApi.Match> matches = ccApi.getMatches("qualification");
        }

        private void generateStacksPicsButton_Click(object sender, EventArgs e)
        {
            List<CCApi.Match> matches = ccApi.getMatches("qualification");

            CCApi.Match match = matches.Find(i => i.Id == Convert.ToInt16(matchNumberBox.Text));

            generateStackImagesFromMatch(match);
        }

        private void generateStackImagesFromMatch(CCApi.Match match)
        {
            string picsFolder = @"C:\Users\ttremblay\Google Drive\GameSense\Shows\GameSense @\Graphics\New\Chezy Champs 2015";
            Bitmap allRedStacks = new Bitmap(525, 238);
            Bitmap allBlueStacks = new Bitmap(525, 238);
            Bitmap allStacks = new Bitmap(1200, 238);

            if (match.Result.RedScore.Stacks != null)
            {
                for (int i = 0; i < match.Result.RedScore.Stacks.Count; i++)
                {
                    Bitmap stack = generateStackImage(match.Result.RedScore.Stacks[i], "red");
                    Point drawLoc = new Point(50 * i, 0);
                    Graphics canvas = Graphics.FromImage(allRedStacks);
                    canvas.DrawImage(stack, drawLoc);
                }
            }

            if (match.Result.BlueScore.Stacks != null)
            {
                for (int i = 0; i < match.Result.BlueScore.Stacks.Count; i++)
                {
                    Bitmap stack = generateStackImage(match.Result.BlueScore.Stacks[i], "blue");
                    Point drawLoc = new Point(50 * i, 0);
                    Graphics canvas = Graphics.FromImage(allBlueStacks);
                    canvas.DrawImage(stack, drawLoc);
                }
            }

            allRedStacks.RotateFlip(RotateFlipType.RotateNoneFlipX);

            Graphics allStacksCanvas = Graphics.FromImage(allStacks);
            allStacksCanvas.DrawImage(allRedStacks, new Point(0, 0));
            allStacksCanvas.DrawImage(allBlueStacks, new Point(allStacks.Width - allBlueStacks.Width));

            Bitmap coopStack = generateCoopertitionStackImage(match);
            allStacksCanvas.DrawImage(coopStack, new Point(allStacks.Width / 2 - coopStack.Width / 2, allStacks.Height - coopStack.Height ));

            allStacks.Save(Path.Combine(picsFolder, "stacks.png"), ImageFormat.Png);

        }

        private Bitmap generateCoopertitionStackImage(CCApi.Match match)
        {
            Size toteSize = new Size(48, 24);
            Bitmap stackImage = new Bitmap(98, 99);
            SolidBrush coopToteColor = new SolidBrush(Color.FromArgb(255,192,0));
            Graphics canvas = Graphics.FromImage(stackImage);

            if (match.Result.BlueScore.CoopertitionSet && match.Result.RedScore.CoopertitionSet)
            {   
                Point bottomLeft = new Point(0, stackImage.Height - toteSize.Height);
                Point bottomRight = new Point(stackImage.Width - toteSize.Width, stackImage.Height - toteSize.Height);
                Point topLeft = new Point(0, stackImage.Height - toteSize.Height*2-1);
                Point topRight = new Point(stackImage.Width - toteSize.Width, stackImage.Height - toteSize.Height * 2 - 1);

                canvas.FillRectangle(coopToteColor, new Rectangle(bottomLeft, toteSize));
                canvas.FillRectangle(coopToteColor, new Rectangle(bottomRight, toteSize));
                canvas.FillRectangle(coopToteColor, new Rectangle(topLeft, toteSize));
                canvas.FillRectangle(coopToteColor, new Rectangle(topRight, toteSize));
            }
            else if (match.Result.BlueScore.CoopertitionStack && match.Result.RedScore.CoopertitionStack)
            {
                Point drawLoc = new Point(stackImage.Width/2 - toteSize.Width/2, stackImage.Height);
                for (int i = 0; i < 4; i++)
                {
                    drawLoc.Y -= toteSize.Height;  //build from the bototm up
                    if (i != 0)
                    {
                        drawLoc.Y -= 1;  //add 1 pixel of space between totes
                    }
                    canvas.FillRectangle(coopToteColor, new Rectangle(drawLoc, toteSize));
                }
            }

            return stackImage;

        }
        

        private Bitmap generateStackImage(CCApi.Stack stack, string color)
        {
            Bitmap stackImage = new Bitmap(48, 238);
            Size toteSize = new Size(48, 24);
            Size containerSize = new Size(36, 64);
            Size noodleSize = new Size(6, 24);

            using (Graphics canvas = Graphics.FromImage(stackImage))
            {
                Point drawLoc = new Point(0, stackImage.Height);                

                for (int i = 0; i < stack.Totes; i++)
                {
                    drawLoc.Y -= toteSize.Height;  //build from the bototm up
                    if (i != 0)
                    {
                        drawLoc.Y -= 1;  //add 1 pixel of space between totes
                    }
                    canvas.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(drawLoc, toteSize));                    
                }                
                
                if (stack.Container)
                {
                    drawLoc.X = 6;  //The container image is 12px narrower, so center it
                    drawLoc.Y -= containerSize.Height + 1;

                    canvas.FillRectangle(new SolidBrush(Color.DarkGreen), new Rectangle(drawLoc, containerSize));

                    if (stack.Litter)
                    {
                        drawLoc.X = 21;
                        drawLoc.Y -= noodleSize.Height;
                        Rectangle noodle = new Rectangle(drawLoc, noodleSize);
                        if (color == "red")
                        {
                            canvas.FillRectangle(new SolidBrush(Color.Red), new Rectangle(drawLoc, noodleSize));
                        }
                        if (color == "blue")
                        {
                            canvas.FillRectangle(new SolidBrush(Color.Blue), new Rectangle(drawLoc, noodleSize));
                        }
                        
                    }
                }                                

                canvas.Save();
                
            }
            return stackImage;
            
        }

        private void getRankingsTickerButton_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            List<CCApi.Ranking> rankings = ccApi.getRankings();

            foreach (CCApi.Ranking rank in rankings)
            {
                sb.AppendFormat("{0}  ", rank.ToString());
            }

            tickerTextBox.Text = sb.ToString();
        }

    }
}
