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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace punchy_lync
{
    public partial class MainWindow : Form, ILyncListener
    {
        readonly LyncClient client;
        readonly NotifyIcon taskBarIcon;
        ToolStripItem statusItem;
        ToolStripItem messageItem;

        public MainWindow()
        {
            InitializeComponent();
            taskBarIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.grey,
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };

            InitializeLight();

            client = new LyncClient();
            client.Register(this);
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
            statusItem.Text = "Offline";
            statusItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            statusItem.Font = new Font(statusItem.Font, FontStyle.Bold);
            statusItem.Image = Properties.Resources.grey.ToBitmap();

            messageItem = cm.Items.Add("Message");
            messageItem.Name = "Message";
            messageItem.Text = "";

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
            client.Unregister(this);
            Close();
        }

        public void UpdateStatus(StatusInfo status)
        {
            this.Invoke(new MethodInvoker(() =>
                {

                    switch (status.Color)
                    {
                        case Color.Red:
                            statusItem.Image = Properties.Resources.red.ToBitmap();
                            taskBarIcon.Icon = Properties.Resources.red;
                            break;
                        case Color.None:
                        case Color.Rainbow:
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
                }));
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

            light.SetBrightness(255);
            RainbowLight.Stop();

            if (c == Color.Red)
            {
                light.SaveColor(System.Drawing.Color.Red, Punchy.Constants.ColorSlot.Color2);
                light.TurnOn(Punchy.Constants.ColorSlot.Color2);
            }
            else if (c == Color.Rainbow)
            {
                RainbowLight.ShineBright();
            }
            else if (c == Color.Green)
            {
                light.TurnOn(Punchy.Constants.ColorSlot.Color1);
            }
            else
            {
                light.TurnOff();
            }
        }
    }

    public class RainbowLight
    {
        private const int TIMER_PERIOD = 5000;

        public static void Stop()
        {
            _timer.Change(System.Threading.Timeout.Infinite, TIMER_PERIOD);
        }

        public static void ShineBright()
        {
            _timer.Change(0, TIMER_PERIOD);
        }

        private static List<System.Drawing.Color> Colors = typeof(System.Drawing.Color)
                                                            .GetProperties()
                                                            .Where(info => info.PropertyType == typeof(System.Drawing.Color))
                                                            .Select(info => (System.Drawing.Color)info.GetValue(null, null))
                                                            .ToList();

        private static System.Threading.Timer _timer = new System.Threading.Timer(x => { 
            var light = Punchy.API.GetLights().FirstOrDefault();
            if (light == null) return;

            var r = new Random();
            var color = Colors[r.Next() % Colors.Count()];

            light.SetBrightness(255);
            light.SaveColor(color, Punchy.Constants.ColorSlot.Color2);
            light.TurnOn(Punchy.Constants.ColorSlot.Color2);
        });

    }
}