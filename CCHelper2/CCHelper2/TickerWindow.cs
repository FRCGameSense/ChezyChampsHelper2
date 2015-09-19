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

namespace CCHelper2
{
    public partial class TickerWindow : Form
    {
        public TickerWindow()
        {
            InitializeComponent();
        }

        private void TickerWindow_Load(object sender, EventArgs e)
        {
            this.Location = Properties.Settings.Default.tickerWindowLocation;
            axShockwaveFlash1.Movie = Path.Combine(Properties.Settings.Default.XsplitInstallLocation, "Rankings Ticker.swf");
            axShockwaveFlash1.Dock = DockStyle.Fill;
            axShockwaveFlash1.Play();
        }

        private void TickerWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.tickerWindowLocation = this.Location;
            Properties.Settings.Default.Save();
        }
    }
}
