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
    public enum Color
    {
        Red,
        Green,
        None
    }

    public class StatusInfo
    {
        public Color Color;
        public String Availability;
        public String Note;

        public StatusInfo() : this(ContactAvailability.Offline, "", ContactCalendarState.Unknown, "") { }

        public StatusInfo(ContactAvailability Availability, String Note, ContactCalendarState CalendarState, String MeetingSubject)
        {
            var availability = Availability;
            var message = "";

            if (   Availability != ContactAvailability.Busy
                && Availability != ContactAvailability.DoNotDisturb
                && Availability != ContactAvailability.Offline)
            {
                if (CalendarState == ContactCalendarState.Busy)
                {
                    availability = ContactAvailability.Busy;
                    message = MeetingSubject ?? Note ?? "";
                }
                else
                {
                    availability = ContactAvailability.Free;
                }
            }

            this.Availability = availability.HumanReadable();
            this.Color = availability.ToColor();
            this.Note = message;
        }
    }

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

            InitializeLight();

            var client = Microsoft.Lync.Model.LyncClient.GetClient();
            client.Self.Contact.ContactInformationChanged += Status_Changed;

            UpdateStatus(CurrentStatus);
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
                    statusItem.Image = Properties.Resources.yellow.ToBitmap();
                    taskBarIcon.Icon = Properties.Resources.yellow;
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

    public static class Extensions
    {
        public static string HumanReadable(this ContactAvailability c)
        {
            switch (c)
            {
                case ContactAvailability.Away:
                    return "Away";
                case ContactAvailability.Busy:
                    return "Busy";
                case ContactAvailability.BusyIdle:
                    return "Busy";
                case ContactAvailability.DoNotDisturb:
                    return "Do Not Disturb";
                case ContactAvailability.Free:
                    return "Available";
                case ContactAvailability.FreeIdle:
                    return "Idle";
                case ContactAvailability.None:
                case ContactAvailability.Invalid:
                case ContactAvailability.Offline:
                    return "Offline";
                case ContactAvailability.TemporarilyAway:
                    return "Be Right Back";
                default:
                    return "";
            }
        }

        public static Color ToColor(this ContactAvailability c)
        {
            switch (c)
            {
                case ContactAvailability.Away:
                case ContactAvailability.TemporarilyAway:
                case ContactAvailability.Free:
                case ContactAvailability.FreeIdle:
                    return Color.Green;
                case ContactAvailability.Busy:
                case ContactAvailability.BusyIdle:
                case ContactAvailability.DoNotDisturb:
                    return Color.Red;
                case ContactAvailability.Offline:
                case ContactAvailability.Invalid:
                case ContactAvailability.None:
                default:
                    return Color.None;
            }
        }
    }
}