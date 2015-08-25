using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CCHelper2
{
    public partial class BTLQuestionForm : Form
    {
        public BTLQuestionForm()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ManualQuestion = "";
            Properties.Settings.Default.ManualAuthor = "";
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ManualQuestion = manualQuestionBox.Text;
            Properties.Settings.Default.ManualAuthor = manualAuthorBox.Text;
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
