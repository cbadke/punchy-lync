using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Lync.Model;

namespace punchy_lync
{
    public partial class MainWindow : Form
    {
        readonly NotifyIcon taskBarIcon;
        readonly System.Threading.Timer updateTimer;
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

            InitializeLight();

            var client = LyncClient.GetClient();
            client.Self.Contact.ContactInformationChanged += Status_Changed;

            updateTimer = new System.Threading.Timer((Object _) =>
            {
                Status_Changed(null, null);
            }, null, 0, 60000);
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
            UpdateStatus(new StatusInfo());
            Close();
        }

        private void Status_Changed(object sender, Microsoft.Lync.Model.ContactInformationChangedEventArgs e)
        {
            var status = CurrentStatus;
            this.Invoke(new MethodInvoker(() =>
            {
                UpdateStatus(status);
            }));
        }

        private StatusInfo CurrentStatus
        {
            get
            {
                try
                {
                    var client = LyncClient.GetClient();

                    var info = client.Self.Contact.GetContactInformation(new List<ContactInformationType>()
                    {
                        ContactInformationType.Availability,
                        ContactInformationType.PersonalNote,
                        ContactInformationType.CurrentCalendarState,
                        ContactInformationType.MeetingSubject,
                    });

                    return new StatusInfo
                    (
                        (ContactAvailability)info[ContactInformationType.Availability],
                        (String)info[ContactInformationType.PersonalNote],
                        (ContactCalendarState)info[ContactInformationType.CurrentCalendarState],
                        (String)info[ContactInformationType.MeetingSubject]
                    );
                }
                catch
                {
                    return new StatusInfo();
                }
            }
        }

        private void UpdateStatus(StatusInfo status)
        {
            switch (status.Color)
            {
                case Color.Red:
                    statusItem.Image = Properties.Resources.red.ToBitmap();
                    taskBarIcon.Icon = Properties.Resources.red;
                    break;
                case Color.None:
                    statusItem.Image = Properties.Resources.grey.ToBitmap();
                    taskBarIcon.Icon = Properties.Resources.grey;
                    break;
                default:
                    statusItem.Image = Properties.Resources.green.ToBitmap();
                    taskBarIcon.Icon = Properties.Resources.green;
                    break;
            }

            statusItem.Text = status.Availability;
            messageItem.Text = status.Note;
            SetLight(status.Color);
        }

        private void InitializeLight()
        {
            var light = Punchy.API.GetLights().FirstOrDefault();
            if (light == null) return;

            light.SaveColor(System.Drawing.Color.Green, Punchy.Constants.ColorSlot.Color1);
            light.SaveColor(System.Drawing.Color.Red, Punchy.Constants.ColorSlot.Color2);
            light.TurnOff();
        }

        private void SetLight(Color c)
        {
            var light = Punchy.API.GetLights().FirstOrDefault();
            if (light == null) return;

            if (c == Color.None)
            {
                light.TurnOff();
            }
            else if (c == Color.Green)
            {
                light.TurnOn(Punchy.Constants.ColorSlot.Color1);
            }
            else
            {
                light.TurnOn(Punchy.Constants.ColorSlot.Color2);
            }
            light.SetBrightness(255);
        }
    }
}