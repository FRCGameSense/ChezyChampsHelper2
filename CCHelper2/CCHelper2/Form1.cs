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
        List<CCApi.Match> ccMatches = new List<CCApi.Match>();
        Size toteSize = new Size(40, 24);
        Size containerSize = new Size(30, 64);
        Size noodleSize = new Size(5, 24);

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
            this.Close();
        }

        public void CleanUpBeforeClosing()
        {
            Properties.Settings.Default.lastWindowLocation = Location;
            Properties.Settings.Default.lastWindowSize = Size;
            if (twitch != null)
            {
                twitch.Stop();
            }
            timer1.Stop();
            Properties.Settings.Default.timerRunning = false;
            Properties.Settings.Default.currentTopic = 0;
            Properties.Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Size = Properties.Settings.Default.lastWindowSize;
            Location = Properties.Settings.Default.lastWindowLocation;

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
                    //File.WriteAllBytes(@"C:\Windows\Fonts\futura_lt_medium.tff", Properties.Resources.futura_lt_medium);
                    //File.WriteAllBytes(@"C:\Windows\Fonts\futura_lt_light.tff", Properties.Resources.futura_lt_light);
                    //File.WriteAllBytes(@"C:\Windows\Fonts\futura_lt_heavy.tff", Properties.Resources.futura_lt_heavy);
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
            publishButton.Enabled = Properties.Settings.Default.enablePublishButton;
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
            updateRankingsTicker();
            updateTop10();
            publishTicker();

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
                case "postMatchTab":
                    publishPostMatchTab();
                    break;
                case "nextMatchTab":
                    publishNextMatchTab();
                    break;
                case "bracketTab":
                    publishBracket();
                    break;
                default:
                    break;
            }
        }

        private void publishBracket()
        {
            xsHandler.loadTagsFromXML();

            int[] qfScores = new int[8];
            int[] sfScores = new int[4];
            int[] fScores = new int[2];


            foreach (DataGridViewRow row in bracketDataGridView.Rows)
            {
                if (row.Cells["DisplayName"].Value.ToString().StartsWith("QF1-1"))
                {
                    xsHandler.changeXMLTag("alliance1", row.Cells["RedAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("alliance8", row.Cells["BlueAlliance"].Value.ToString());
                }
                else if (row.Cells["DisplayName"].Value.ToString().StartsWith("QF2-1"))
                {
                    xsHandler.changeXMLTag("alliance4", row.Cells["RedAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("alliance5", row.Cells["BlueAlliance"].Value.ToString());
                }
                else if (row.Cells["DisplayName"].Value.ToString().StartsWith("QF3-1"))
                {
                    xsHandler.changeXMLTag("alliance2", row.Cells["RedAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("alliance7", row.Cells["BlueAlliance"].Value.ToString());
                }
                else if (row.Cells["DisplayName"].Value.ToString().StartsWith("QF4-1"))
                {
                    xsHandler.changeXMLTag("alliance3", row.Cells["RedAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("alliance6", row.Cells["BlueAlliance"].Value.ToString());
                }
                else if (row.Cells["DisplayName"].Value.ToString().StartsWith("SF1-1"))
                {
                    xsHandler.changeXMLTag("semi1RedAlliance", row.Cells["RedAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("semi1RedAllianceNum", row.Cells["RedAllianceNum"].Value.ToString());
                    xsHandler.changeXMLTag("semi1BlueAlliance", row.Cells["BlueAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("semi1BlueAllianceNum", row.Cells["BlueAllianceNum"].Value.ToString());
                }
                else if (row.Cells["DisplayName"].Value.ToString().StartsWith("SF2-1"))
                {
                    xsHandler.changeXMLTag("semi2RedAlliance", row.Cells["RedAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("semi2RedAllianceNum", row.Cells["RedAllianceNum"].Value.ToString());
                    xsHandler.changeXMLTag("semi2BlueAlliance", row.Cells["BlueAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("semi2BlueAllianceNum", row.Cells["BlueAllianceNum"].Value.ToString());
                }
                else if (row.Cells["DisplayName"].Value.ToString().StartsWith("F-1"))
                {
                    xsHandler.changeXMLTag("finalsRedAlliance", row.Cells["RedAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("finalsRedAllianceNum", row.Cells["RedAllianceNum"].Value.ToString());
                    xsHandler.changeXMLTag("finalsBlueAlliance", row.Cells["BlueAlliance"].Value.ToString());
                    xsHandler.changeXMLTag("finalsBlueAllianceNum", row.Cells["BlueAllianceNum"].Value.ToString());
                }

                if (row.Cells["DisplayName"].Value.ToString().StartsWith("QF"))
                {
                    if (row.Cells["Winner"].Value.ToString() == "R")
                    {
                        int allianceNum = Convert.ToInt32(row.Cells["RedAllianceNum"].Value);
                        qfScores[ allianceNum - 1] += 1;
                    }
                    else if (row.Cells["Winner"].Value.ToString() == "B")
                    {
                        int allianceNum = Convert.ToInt32(row.Cells["BlueAllianceNum"].Value);
                        qfScores[allianceNum - 1] += 1;
                    }
                }
                else if (row.Cells["DisplayName"].Value.ToString().StartsWith("SF1"))
                {
                    if (row.Cells["Winner"].Value.ToString() == "R")
                    {
                        sfScores[0] += 1;
                    }
                    else if (row.Cells["Winner"].Value.ToString() == "B")
                    {
                        sfScores[1] += 1;
                    }
                }
                else if (row.Cells["DisplayName"].Value.ToString().StartsWith("SF2"))
                {
                    if (row.Cells["Winner"].Value.ToString() == "R")
                    {
                        sfScores[2] += 1;
                    }
                    else if (row.Cells["Winner"].Value.ToString() == "B")
                    {
                        sfScores[3] += 1;
                    }
                }
                else if (row.Cells["DisplayName"].Value.ToString().StartsWith("F"))
                {
                    if (row.Cells["Winner"].Value.ToString() == "R")
                    {
                        fScores[0] += 1;
                    }
                    else if (row.Cells["Winner"].Value.ToString() == "B")
                    {
                        fScores[1] += 1;
                    }
                }
            }

            for (int i = 0; i < 8; i++)
            {
                xsHandler.changeXMLTag(string.Format("alliance{0}Score",i+1), qfScores[i].ToString());
            }

            xsHandler.changeXMLTag("semi1RedScore", sfScores[0].ToString());
            xsHandler.changeXMLTag("semi1BlueScore", sfScores[1].ToString());
            xsHandler.changeXMLTag("semi2RedScore", sfScores[2].ToString());
            xsHandler.changeXMLTag("semi2BlueScore", sfScores[3].ToString());

            xsHandler.changeXMLTag("finalsRedScore", fScores[0].ToString());
            xsHandler.changeXMLTag("finalsBlueScore", fScores[1].ToString());

            xsHandler.writeXMLFile();
        }

        private void publishPostMatchTab()
        {
            if (postMatchDataGridView.SelectedRows != null)
            {
                xsHandler.loadTagsFromXML();

                DataGridViewRow selectedRow = postMatchDataGridView.SelectedRows[0];

                foreach (DataGridViewCell cell in selectedRow.Cells)
                {
                    string value = "";
                    if (cell.OwningColumn.Name.Contains("CanEfficiency"))
                    {
                        value = String.Format("{0:N1}%", cell.Value);
                    }
                    else
                    {
                        value = cell.Value.ToString();
                    }

                    xsHandler.changeXMLTag("postMatch" + cell.OwningColumn.Name, value);

                }

                CCApi.Match selectedMatch = new CCApi.Match();

                if (selectedRow.Cells["Type"].Value.ToString() == "qualification")
                {
                    selectedMatch = ccMatches.Find(i => i.Type == selectedRow.Cells["Type"].Value.ToString() && i.DisplayName == selectedRow.Cells["DisplayName"].Value.ToString().Substring(1));
                }
                else
                {
                    selectedMatch = ccMatches.Find(i => i.Type == selectedRow.Cells["Type"].Value.ToString() && i.DisplayName == selectedRow.Cells["DisplayName"].Value.ToString());
                }

                if (selectedMatch.Result != null)
                {
                    string stackImagePath = generateStacksImageFromMatch(selectedMatch);
                    xsHandler.changeXMLTag("postMatchStackImagePath", stackImagePath);
                }

                xsHandler.writeXMLFile();
            }
        }

        private void publishNextMatchTab()
        {
            if (nextMatchDataGridView.SelectedRows != null)
            {
                xsHandler.loadTagsFromXML();

                DataGridViewRow selectedRow = nextMatchDataGridView.SelectedRows[0];

                foreach (DataGridViewCell cell in selectedRow.Cells)
                {
                    xsHandler.changeXMLTag("nextMatch" + cell.OwningColumn.Name, cell.Value.ToString());
                }

                xsHandler.changeXMLTag("red1PicPath", setBotPic(Convert.ToInt32(selectedRow.Cells["Red1"].Value), "Red1"));
                xsHandler.changeXMLTag("red2PicPath", setBotPic(Convert.ToInt32(selectedRow.Cells["Red2"].Value), "Red2"));
                xsHandler.changeXMLTag("red3PicPath", setBotPic(Convert.ToInt32(selectedRow.Cells["Red3"].Value), "Red3"));

                xsHandler.changeXMLTag("blue1PicPath", setBotPic(Convert.ToInt32(selectedRow.Cells["Blue1"].Value), "Blue1"));
                xsHandler.changeXMLTag("blue2PicPath", setBotPic(Convert.ToInt32(selectedRow.Cells["Blue2"].Value), "Blue2"));
                xsHandler.changeXMLTag("blue3PicPath", setBotPic(Convert.ToInt32(selectedRow.Cells["Blue3"].Value), "Blue3"));
                
                xsHandler.writeXMLFile();
            }
        }

        private void publishTicker()
        {
            xsHandler.loadTagsFromXML();

            xsHandler.changeXMLTag("rankings", tickerTextBox.Text);

            xsHandler.writeXMLFile();
        }

        private void publishSplashTab()
        {
            xsHandler.loadTagsFromXML();

            xsHandler.changeXMLTag("splashTop", splashTopBox.Text);
            xsHandler.changeXMLTag("splashBottom", splashBottomBox.Text);

            xsHandler.writeXMLFile();
        }

        private void publishQuestion()
        {
            try
            {
                xsHandler.loadTagsFromXML();

                xsHandler.changeXMLTag("question", Properties.Settings.Default.PublishedQuestion);
                if (Properties.Settings.Default.PublishedAuthor == "")
                    xsHandler.changeXMLTag("author", Properties.Settings.Default.PublishedAuthor);
                else
                    xsHandler.changeXMLTag("author", "-" + Properties.Settings.Default.PublishedAuthor);

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

            xsHandler.changeXMLTag("caster1", Properties.Settings.Default.Host1);
            xsHandler.changeXMLTag("caster2", Properties.Settings.Default.Host2);
            xsHandler.changeXMLTag("host3", Properties.Settings.Default.Host3);
            xsHandler.changeXMLTag("host4", Properties.Settings.Default.Host4);
            xsHandler.changeXMLTag("guest1", Properties.Settings.Default.Guest1);
            xsHandler.changeXMLTag("guest2", Properties.Settings.Default.Guest2);

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
            publishButton.Enabled = Properties.Settings.Default.enablePublishButton;
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

                string backupFile = Path.Combine(Properties.Settings.Default.graphicsFolderLocation, "GSQuestionsBackup.txt");
                
                File.WriteAllText(backupFile, sb.ToString());
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            string questionsBackupFile = Path.Combine(Properties.Settings.Default.graphicsFolderLocation, "GSQuestionsBackup.txt");
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

        private string generateStacksImageFromMatch(CCApi.Match match)
        {
            string picsFolder = Properties.Settings.Default.graphicsFolderLocation;
            Bitmap allRedStacks = new Bitmap((10*toteSize.Width)+18, 238);
            Bitmap allBlueStacks = new Bitmap(allRedStacks.Size.Width, allRedStacks.Size.Height);
            Bitmap allStacks = new Bitmap(allRedStacks.Size.Width*2 + ((toteSize.Width * 2) + 2) + 20, 238);

            if (match.Result.RedScore.Stacks != null)
            {
                for (int i = 0; i < match.Result.RedScore.Stacks.Count; i++)
                {
                    Bitmap stack = generateStackImage(match.Result.RedScore.Stacks[i], "red");
                    Point drawLoc = new Point((toteSize.Width+2) * i, 0);
                    Graphics canvas = Graphics.FromImage(allRedStacks);
                    canvas.DrawImage(stack, drawLoc);
                }
            }

            if (match.Result.BlueScore.Stacks != null)
            {
                for (int i = 0; i < match.Result.BlueScore.Stacks.Count; i++)
                {
                    Bitmap stack = generateStackImage(match.Result.BlueScore.Stacks[i], "blue");
                    Point drawLoc = new Point((toteSize.Width + 2) * i, 0);
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

            string savePath = Path.Combine(picsFolder, "stacks.png");

            allStacks.Save(savePath, ImageFormat.Png);

            return savePath;

        }

        private Bitmap generateCoopertitionStackImage(CCApi.Match match)
        {
            Bitmap stackImage = new Bitmap((toteSize.Width*2)+2, (toteSize.Height*4)+3);
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
            Bitmap stackImage = new Bitmap(toteSize.Width, 238);

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
                    drawLoc.X = (toteSize.Width - containerSize.Width)/2;  //The container image is 12px narrower, so center it
                    drawLoc.Y -= containerSize.Height + 1;

                    canvas.FillRectangle(new SolidBrush(Color.DarkGreen), new Rectangle(drawLoc, containerSize));

                    if (stack.Litter)
                    {
                        drawLoc.X = (toteSize.Width - noodleSize.Width)/2;
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
            updateRankingsTicker();
        }

        private void updateRankingsTicker()
        {
            StringBuilder sb = new StringBuilder();

            List<CCApi.Ranking> rankings = CCApi.getRankings();

            if (rankings != null)
            {
                foreach (CCApi.Ranking rank in rankings)
                {
                    sb.AppendFormat("{0}  ", rank.ToString());
                }
            }
            tickerTextBox.Text = sb.ToString();
        }

        private void updateTop10()
        {
            xsHandler.loadTagsFromXML();

            List<CCApi.Ranking> rankings = CCApi.getRankings();

            foreach (CCApi.Ranking rank in rankings)
            {
                xsHandler.changeXMLTag("Rank" + rank.Rank.ToString(), rank.ToStringNoNumbers());
            }

            xsHandler.writeXMLFile();
        }

        private void nextMatchRefreshButton_Click(object sender, EventArgs e)
        {
            ccMatches = CCApi.getMatches("qualification");

            if (ccMatches != null)
            {
                ccMatches.AddRange(CCApi.getMatches("elimination"));


                List<CCApi.MatchPreviewForDisplay> matchPreviews = new List<CCApi.MatchPreviewForDisplay>();

                foreach (CCApi.Match m in ccMatches)
                {
                    matchPreviews.Add(m.ToMatchPreviewForDisplay());
                }

                nextMatchDataGridView.DataSource = matchPreviews;

                int latestMatchIndex = 0;

                foreach (DataGridViewRow row in nextMatchDataGridView.Rows)
                {
                    if (row.Cells["Status"].Value.ToString() == "complete")
                    {
                        latestMatchIndex = row.Index;
                        row.Selected = false;
                    }

                    switch (row.Cells["Winner"].Value.ToString())
                    {
                        case "R":
                            row.DefaultCellStyle.BackColor = Color.LightCoral;
                            break;
                        case "B":
                            row.DefaultCellStyle.BackColor = Color.LightBlue;
                            break;
                        case "T":
                            row.DefaultCellStyle.BackColor = Color.Khaki;
                            break;
                        default:
                            break;
                    }
                }

                nextMatchDataGridView.Rows[latestMatchIndex + 1].Selected = true;
                this.nextMatchDataGridView.Columns["RedQA"].DefaultCellStyle.Format = "0.00";
                this.nextMatchDataGridView.Columns["RedQA"].ValueType = typeof(Double);
                this.nextMatchDataGridView.Columns["RedCanAvg"].DefaultCellStyle.Format = "0.00";
                this.nextMatchDataGridView.Columns["RedCanAvg"].ValueType = typeof(Double);
                this.nextMatchDataGridView.Columns["RedCoopAvg"].DefaultCellStyle.Format = "0.00";
                this.nextMatchDataGridView.Columns["RedCoopAvg"].ValueType = typeof(Double);
                this.nextMatchDataGridView.Columns["BlueQA"].DefaultCellStyle.Format = "0.00";
                this.nextMatchDataGridView.Columns["BlueQA"].ValueType = typeof(Double);
                this.nextMatchDataGridView.Columns["BlueCanAvg"].DefaultCellStyle.Format = "0.00";
                this.nextMatchDataGridView.Columns["BlueCanAvg"].ValueType = typeof(Double);
                this.nextMatchDataGridView.Columns["BlueCoopAvg"].DefaultCellStyle.Format = "0.00";
                this.nextMatchDataGridView.Columns["BlueCoopAvg"].ValueType = typeof(Double);
                nextMatchDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
        }

        private void postMatchRefreshButton_Click(object sender, EventArgs e)
        {
            ccMatches = CCApi.getMatches("qualification");
            if (ccMatches != null)
            {
                ccMatches.AddRange(CCApi.getMatches("elimination"));
                List<CCApi.MatchResultsForDisplay> simpleMatches = new List<CCApi.MatchResultsForDisplay>();


                foreach (CCApi.Match m in ccMatches)
                {
                    simpleMatches.Add(m.ToMatchResultsForDisplay());
                }

                postMatchDataGridView.DataSource = simpleMatches;

                int latestMatchIndex = 0;

                foreach (DataGridViewRow row in postMatchDataGridView.Rows)
                {
                    if (row.Cells["Status"].Value.ToString() == "complete")
                    {
                        latestMatchIndex = row.Index;
                        row.Selected = false;
                    }

                    switch (row.Cells["Winner"].Value.ToString())
                    {
                        case "R":
                            row.DefaultCellStyle.BackColor = Color.LightCoral;
                            break;
                        case "B":
                            row.DefaultCellStyle.BackColor = Color.LightBlue;
                            break;
                        case "T":
                            row.DefaultCellStyle.BackColor = Color.Khaki;
                            break;
                        default:
                            break;
                    }
                }

                postMatchDataGridView.Rows[latestMatchIndex].Selected = true;
                postMatchDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.CleanUpBeforeClosing();
        }

        private void bracketRefreshButton_Click(object sender, EventArgs e)
        {
            List<CCApi.Match> elimMatches = CCApi.getMatches("elimination");
            CCApi.updateAlliances();
            List<CCApi.BracketMatch> simpleMatches = new List<CCApi.BracketMatch>();

            foreach (CCApi.Match m in elimMatches)
            {
                simpleMatches.Add(m.ToBracketMatch());
            }

            bracketDataGridView.DataSource = simpleMatches;

            foreach (DataGridViewRow row in bracketDataGridView.Rows)
            {
                switch (row.Cells["Winner"].Value.ToString())
                {
                    case "R":
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        break;
                    case "B":
                        row.DefaultCellStyle.BackColor = Color.LightBlue;
                        break;
                    case "T":
                        row.DefaultCellStyle.BackColor = Color.Khaki;
                        break;
                    default:
                        break;
                }
            }
        }

        private string setBotPic(int team, string position)
        {
            string teamPic = Path.Combine(Properties.Settings.Default.botPicsLocation, team.ToString() + ".png");
            string positionPic = Path.Combine(Properties.Settings.Default.botPicsLocation, position + ".png");
            if (File.Exists(teamPic))
            {
                //File.Delete(positionPic);
                File.Copy(teamPic, positionPic, true);
            }
            else
            {
                Bitmap placeHolder = new Bitmap(400, 300);
                Graphics canvas = Graphics.FromImage(placeHolder);
                Rectangle rect1 = new Rectangle(0, 0, 400, 300);
                Font font1 = new Font("futura_lt_medium", 48, FontStyle.Bold, GraphicsUnit.Point);

                // Create a StringFormat object with the each line of text, and the block 
                // of text centered on the page.
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;

                // Draw the text and the surrounding rectangle.
                canvas.FillRectangle(Brushes.White, rect1);
                canvas.DrawString(team.ToString(), font1, Brushes.Black, rect1, stringFormat);

                placeHolder.Save(teamPic, ImageFormat.Png);
                placeHolder.Save(positionPic, ImageFormat.Png);
            }

            return positionPic;
        }

        private void launchTickerWindowButton_Click(object sender, EventArgs e)
        {
            TickerWindow tw = new TickerWindow();

            tw.Show();
        }

        private void teamLookupButton_Click(object sender, EventArgs e)
        {
            List<CCApi.Match> lookupMatches = new List<CCApi.Match>();

            if (includeQualificationsCheckbox.Checked)
            {
                lookupMatches.AddRange(CCApi.getMatches("qualification"));
            }
            if (includeElimsCheckbox.Checked)
            {
                lookupMatches.AddRange(CCApi.getMatches("elimination"));
            }

            string imageLocation = Path.Combine(Properties.Settings.Default.botPicsLocation, teamLookupBox.Text + ".png");
            double averageCanEfficiency = 0;
            double qualificationAverage = 0;
            double autoAverage = 0;
            double coopAverage = 0;
            double stackAverage = 0;
            int numStacks = 0;
            int numPlayedMatches = 0;

            if (File.Exists(imageLocation))
            {
                teamLookupPictureBox.ImageLocation = imageLocation;
            }

            List<CCApi.Ranking> rankings = CCApi.getRankings();

            CCApi.Ranking rank = rankings.Find(i => i.TeamId == Convert.ToInt32(teamLookupBox.Text));

            if (rank != null)
            {

                int team = Convert.ToInt32(teamLookupBox.Text);

                List<CCApi.Match> teamMatches = lookupMatches.FindAll(i => (i.Blue1 == team || i.Blue2 == team || i.Blue3 == team || i.Red1 == team || i.Red2 == team || i.Red3 == team));

                List<CCApi.MatchResultsForDisplay> teamMatchesForDisplay = new List<CCApi.MatchResultsForDisplay>();
                foreach (CCApi.Match match in teamMatches)
                {
                    CCApi.MatchResultsForDisplay mrfd = match.ToMatchResultsForDisplay();
                    teamMatchesForDisplay.Add(mrfd);

                    if (mrfd.Status == "complete")
                    {
                        numPlayedMatches++;
                        if (mrfd.RedAlliance.Contains(team.ToString()))
                        {
                            averageCanEfficiency += mrfd.RedCanEfficiency;
                            qualificationAverage += mrfd.RedScore;
                            if (match.Result != null)
                            {
                                autoAverage += match.Result.RedScore.GetAutoPoints();
                                coopAverage += match.Result.RedScore.GetCoopPoints();
                                if (match.Result.RedScore.Stacks != null)
                                {
                                    foreach (CCApi.Stack stack in match.Result.RedScore.Stacks)
                                    {
                                        stackAverage += stack.GetValue();
                                        numStacks++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            averageCanEfficiency += mrfd.BlueCanEfficiency;
                            qualificationAverage += mrfd.BlueScore;
                            if (match.Result != null)
                            {
                                autoAverage += match.Result.BlueScore.GetAutoPoints();
                                coopAverage += match.Result.BlueScore.GetCoopPoints();
                                if (match.Result.BlueScore.Stacks != null)
                                {
                                    foreach (CCApi.Stack stack in match.Result.BlueScore.Stacks)
                                    {
                                        stackAverage += stack.GetValue();
                                        numStacks++;
                                    }
                                }
                            }
                        }
                    }

                }

                averageCanEfficiency /= numPlayedMatches;
                qualificationAverage /= numPlayedMatches;
                autoAverage /= numPlayedMatches;
                coopAverage /= numPlayedMatches;
                stackAverage /= numStacks;

                rankLabel.Text = rank.Rank.ToString();
                cerLabel.Text = averageCanEfficiency.ToString("0.00") + "%";
                qaLabel.Text = qualificationAverage.ToString("0.00");
                autoLabel.Text = autoAverage.ToString("0.00");
                coopLabel.Text = coopAverage.ToString("0.00");
                stacksLabel.Text = stackAverage.ToString("0.00");

                teamLookupDataGridView.DataSource = teamMatchesForDisplay;

                foreach (DataGridViewRow row in teamLookupDataGridView.Rows)
                {
                    switch (row.Cells["Winner"].Value.ToString())
                    {
                        case "R":
                            row.DefaultCellStyle.BackColor = Color.LightCoral;
                            break;
                        case "B":
                            row.DefaultCellStyle.BackColor = Color.LightBlue;
                            break;
                        case "T":
                            row.DefaultCellStyle.BackColor = Color.Khaki;
                            break;
                        default:
                            break;
                    }

                    if (row.Cells["RedAlliance"].Value.ToString().Contains(team.ToString()))
                    {
                        row.Cells["RedAlliance"].Style.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
                    }
                    else
                    {
                        row.Cells["BlueAlliance"].Style.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
                    }                    
                }
                teamLookupDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedTab.Name)
            {
                case "teamLookupTab":
                    break;
            }
        }


    }
}
