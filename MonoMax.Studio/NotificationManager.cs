using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using cm = Caliburn.Micro;

namespace MonoMax.Studio
{
    public class Notification
    {
        public Notification(string message, int duration = 3000)
        {
            Message = message;
            Duration = duration;
            Start = DateTime.Now;
        }

        public bool CanBeCollected { get; private set; }
        public Guid Id { get; }
        public string Message { get; }
        public int Duration { get; }
        public DateTime Start { get; }
        public void RequestCollection() => CanBeCollected = true;
    }

    public class NotificationManager
    {
        private readonly DispatcherTimer _timer = 
            new DispatcherTimer(DispatcherPriority.ApplicationIdle);

        public cm.BindableCollection<Notification> Notifications { get; }


        public NotificationManager()
        {
            Notifications = new cm.BindableCollection<Notification>();
            _timer.Interval = TimeSpan.FromMilliseconds(60.0d);
            _timer.Tick += NotificationLoop;
            _timer.Start();
        }

        private void NotificationLoop(object sender, EventArgs e)
        {
            if (Notifications.Count == 0)
                return;

            foreach (var n in Notifications)
            {
                var elapsed = (DateTime.Now - n.Start).TotalMilliseconds;
                if (elapsed >= n.Duration)
                    n.RequestCollection();
            }

            for (int i = Notifications.Count - 1; i >= 0; i--)
            {
                if (Notifications[i].CanBeCollected)
                    Notifications.RemoveAt(i);
            }
        }

        public void AddNotification(string message)
        {
            Notifications.Add(new Notification(message));
        }

    }
}
