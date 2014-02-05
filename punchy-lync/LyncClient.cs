using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Lync.Model;

namespace punchy_lync
{
    public interface ILyncListener
    {
        void UpdateStatus(StatusInfo newStatus);
    }

    public class LyncClient
    {
        readonly System.Threading.Timer _updateTimer;
        readonly List<ILyncListener> _listeners;

        public LyncClient()
        {
            _listeners = new List<ILyncListener>();

            _updateTimer = new System.Threading.Timer((Object _) =>
            {
                Status_Changed(null, null);
            }, null, 0, 60000);

            var client = Microsoft.Lync.Model.LyncClient.GetClient();
            client.Self.Contact.ContactInformationChanged += Status_Changed;
        }

        private StatusInfo GetCurrentStatus()
        {
            try
            {
                var client = Microsoft.Lync.Model.LyncClient.GetClient();

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

        private void Status_Changed(object sender, object args)
        {
            var status = GetCurrentStatus();
            _listeners.ForEach(l => l.UpdateStatus(status));
        }

        public void Register(ILyncListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void Unregister(ILyncListener listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }
    }
}
