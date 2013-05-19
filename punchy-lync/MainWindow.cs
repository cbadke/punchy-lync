using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace punchy_lync
{
    public partial class MainWindow : Form
    {
        readonly NotifyIcon taskBarIcon;
        ToolStripItem statusItem;
        ToolStripItem messageItem;

        public MainWindow()
        {
            InitializeComponent();
            taskBarIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.red,
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };
        }

        protected override void OnShown(EventArgs e)
        {
            this.Opacity = Double.MinValue;
            this.Hide();
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var cm = new ContextMenuStrip();

            statusItem = cm.Items.Add("Status");
            statusItem.Name = "Status";
            statusItem.Text = "Status goes here";
            statusItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            statusItem.Font = new Font(statusItem.Font, FontStyle.Bold);
            statusItem.Image = Properties.Resources.red.ToBitmap();

            messageItem = cm.Items.Add("Message");
            messageItem.Name = "Message";
            messageItem.Text = "Online message goes here";

            var separator = cm.Items.Add("-");

            var quitItem = cm.Items.Add("Quit");
            quitItem.Text = "Quit";
            quitItem.Name = "Quit";
            quitItem.Click += Exit_Click;

            return cm;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            taskBarIcon.Visible = false;
            Close();
        }
    }
}