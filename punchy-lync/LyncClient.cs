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
        readonly System.Threading.Timer _clientCheckTimer;
        readonly System.Threading.Timer _updateTimer;
        readonly List<ILyncListener> _listeners;

        private const int SIGNIN_CHECK_TIMEOUT = 5000;
        private const int STATUS_CHECK_TIMEOUT = 60000;

        public LyncClient()
        {
            _listeners = new List<ILyncListener>();

            _updateTimer = new System.Threading.Timer((Object _) =>
            {
                Status_Changed(null, null);
            }, null, System.Threading.Timeout.Infinite, STATUS_CHECK_TIMEOUT);

            _clientCheckTimer = new System.Threading.Timer((Object _) =>
            {
                try
                {
                    var client = Microsoft.Lync.Model.LyncClient.GetClient();

                    if (client.State == ClientState.SignedIn)
                    {
                        _clientCheckTimer.Change(System.Threading.Timeout.Infinite, SIGNIN_CHECK_TIMEOUT);
                        _updateTimer.Change(0, STATUS_CHECK_TIMEOUT);

                        client.StateChanged += SignInChanged;
                        client.Self.Contact.ContactInformationChanged += Status_Changed;
                    }
                }
                catch
                {

                }
            }, null, 0, SIGNIN_CHECK_TIMEOUT);
        }

        void SignInChanged(object sender, ClientStateChangedEventArgs e)
        {
            if (e.NewState == ClientState.SignedOut)
            {
                try
                {
                    var client = Microsoft.Lync.Model.LyncClient.GetClient();

                    client.StateChanged -= SignInChanged;
                    client.Self.Contact.ContactInformationChanged -= Status_Changed;
                }
                catch
                {

                }

                _clientCheckTimer.Change(0, SIGNIN_CHECK_TIMEOUT);
                _updateTimer.Change(System.Threading.Timeout.Infinite, STATUS_CHECK_TIMEOUT);
            }
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
