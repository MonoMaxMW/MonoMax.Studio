using MonoMax.Studio.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio
{
    public class AppManager
    {
        public static AppManager Instance { get; }

        static AppManager()
        {
            Instance = new AppManager();

        }

        public AppManager()
        {
            Root = new Node() { Key = "Root", ImageKey = "root", IsExpanded = true };
            NotificationManager = new NotificationManager();
        }

        public NotificationManager NotificationManager { get; }
        public INode Root { get; }
    }
}
