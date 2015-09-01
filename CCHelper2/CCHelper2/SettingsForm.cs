using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;


namespace CCHelper2
{
    public partial class SettingsForm : Form
    {
        xSplitHandler2 xsHandler;
        GSTwitterClient twitter;
        public SettingsForm()
        {
            InitializeComponent();
            xsHandler = new xSplitHandler2();
        }

        private void settingsXsplitBrowseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select the location of the XSplit installation folder.";
            folderBrowserDialog1.ShowNewFolderButton = false;

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                settingsXsplitLocationBox.Text = folderBrowserDialog1.SelectedPath;
            }

        }

        private void settingsSaveButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.XsplitInstallLocation = settingsXsplitLocationBox.Text;
            Properties.Settings.Default.graphicsFolderLocation = settingsBTLFolderLocationBox.Text;
            Properties.Settings.Default.apiUrl = apiUrlBox.Text;
            Properties.Settings.Default.botPicsLocation = botPicsLocationBox.Text;
            Properties.Settings.Default.enablePublishButton = enablePublishButtonCheckBox.Checked;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            settingsXsplitLocationBox.Text = Properties.Settings.Default.XsplitInstallLocation;
            settingsBTLFolderLocationBox.Text = Properties.Settings.Default.graphicsFolderLocation;
            botPicsLocationBox.Text = Properties.Settings.Default.botPicsLocation;
            apiUrlBox.Text = Properties.Settings.Default.apiUrl;
            enablePublishButtonCheckBox.Checked = Properties.Settings.Default.enablePublishButton;
            if (Properties.Settings.Default.twitterVerified)
            {
                twitterConnectbutton.Enabled = false;
                twitterConnectbutton.Text = "Already Verified";
            }
        }

        private void settingsCancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void settingsOverlaysBrowseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select the location of the Chezy Champs folder.";
            folderBrowserDialog1.ShowNewFolderButton = false;

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                settingsBTLFolderLocationBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure? \n\nThis will overwrite all settings in streamcontrol.xml", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DialogResult result = openFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    try
                    {
                        xsHandler.copyStreamControlXMLToXsplitLocation(openFileDialog1.FileName);
                        MessageBox.Show("Success!\n\nstreamcontrol.xml succesfully overwritten.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to overwrite streamcontrol.xml. \n\n" + ex.Message);
                    }
                }
            }
        }

        private void twitterConnectbutton_Click(object sender, EventArgs e)
        {
            twitter = new GSTwitterClient();
            twitter.getPIN();

            twitterInstructionsLabel.Visible = true;
            twitterVerifyButton.Enabled = true;
        }

        private void twitterVerifyButton_Click(object sender, EventArgs e)
        {
            twitter.Authenticate(twitterPinBox.Text);
            if (Properties.Settings.Default.twitterVerified)
            {
                twitterConnectbutton.Text = "Verified!";
                twitterVerifyButton.Enabled = false;
                twitterConnectbutton.Enabled = false;
            }
            else
            {
                twitterConnectbutton.Text = "Failed to Verify.";
                twitterVerifyButton.Enabled = true;
                twitterConnectbutton.Enabled = true;
                twitterPinBox.Clear();
            }
        }

        private void resetSettingsButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to reset to default settings?", "This cannot be undone.", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();
            }
        }

        private void enableResetCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            resetSettingsButton.Enabled = enableResetCheckBox.Checked;
        }

        private void botPicsFolderBrowseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select the location of the Chezy Champs robot pics.";
            folderBrowserDialog1.ShowNewFolderButton = false;

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                botPicsLocationBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }


    }
}
