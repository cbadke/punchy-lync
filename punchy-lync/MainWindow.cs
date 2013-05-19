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
        Yellow,
        None
    }

    public partial class MainWindow : Form
    {
        readonly NotifyIcon taskBarIcon;
        ToolStripItem statusItem;
        ToolStripItem messageItem;

        Color _awayColor;

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
            var a = (ContactAvailability)client.Self.Contact.GetContactInformation(ContactInformationType.Availability);
            var n = (String)client.Self.Contact.GetContactInformation(ContactInformationType.PersonalNote);

            UpdateStatus(a, n ?? "");
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
            UpdateStatus(ContactAvailability.Offline, "");
            Close();
        }

        private void Status_Changed(object sender, Microsoft.Lync.Model.ContactInformationChangedEventArgs e)
        {
            var client = LyncClient.GetClient();
            var a = (ContactAvailability)client.Self.Contact.GetContactInformation(ContactInformationType.Availability);
            var n = (String)client.Self.Contact.GetContactInformation(ContactInformationType.PersonalNote);

            this.Invoke(new MethodInvoker(() =>
            {
                UpdateStatus(a, n ?? "");
            }));
        }

        private void UpdateStatus(ContactAvailability availability, string message)
        {
            statusItem.Text = availability.HumanReadable();
            messageItem.Text = message;

            switch (availability.ToColor())
            {
                case Color.Red:
                    statusItem.Image = Properties.Resources.red.ToBitmap();
                    taskBarIcon.Icon = Properties.Resources.red;
                    break;
                case Color.Yellow:
                case Color.None:
                    statusItem.Image = Properties.Resources.yellow.ToBitmap();
                    taskBarIcon.Icon = Properties.Resources.yellow;
                    break;
                default:
                    statusItem.Image = Properties.Resources.green.ToBitmap();
                    taskBarIcon.Icon = Properties.Resources.green;
                    break;
            }

            SetLight(availability.ToColor());
        }

        private void InitializeLight()
        {
            var light = Punchy.API.GetLights().FirstOrDefault();
            if (light == null) return;

            light.SaveColor(System.Drawing.Color.Green, Punchy.Constants.ColorSlot.Color1);
            light.SaveColor(System.Drawing.Color.Red, Punchy.Constants.ColorSlot.Color2);
            light.TurnOff();

            _awayColor = Color.Red;
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
                if (_awayColor != c)
                {
                    if (_awayColor == Color.Red)
                    {
                        light.SaveColor(System.Drawing.Color.Yellow, Punchy.Constants.ColorSlot.Color2);
                        _awayColor = Color.Yellow;
                    }
                    else
                    {
                        light.SaveColor(System.Drawing.Color.Red, Punchy.Constants.ColorSlot.Color2);
                        _awayColor = Color.Red;
                    }
                }

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
                case ContactAvailability.None:
                case ContactAvailability.TemporarilyAway:
                    return Color.Yellow;
                case ContactAvailability.Busy:
                case ContactAvailability.BusyIdle:
                case ContactAvailability.DoNotDisturb:
                    return Color.Red;
                case ContactAvailability.Free:
                case ContactAvailability.FreeIdle:
                    return Color.Green;
                case ContactAvailability.Offline:
                case ContactAvailability.Invalid:
                default:
                    return Color.None;
            }
        }
    }
}