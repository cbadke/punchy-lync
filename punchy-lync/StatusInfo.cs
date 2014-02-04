using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (CalendarState == ContactCalendarState.OutsideWorkPeriod &&
                (Availability == ContactAvailability.Away ||
                 Availability == ContactAvailability.Invalid ||
                 Availability == ContactAvailability.None ||
                 Availability == ContactAvailability.Offline ||
                 Availability == ContactAvailability.TemporarilyAway))
            {
                availability = ContactAvailability.None;
            }
            else if(CalendarState == ContactCalendarState.Busy)
            {
                availability = ContactAvailability.Busy;
                message = MeetingSubject ?? Note ?? "";
            }
            else
            {
                availability = Availability;
                message = Note ?? "";
            }

            this.Availability = availability.HumanReadable();
            this.Color = availability.ToColor();
            this.Note = message;
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
