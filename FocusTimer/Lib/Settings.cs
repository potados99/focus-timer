using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Lib
{
    class Settings
    {
        public static List<string> GetApps()
        {
            return new List<string>
            {
                Properties.Settings.Default.App1,
                Properties.Settings.Default.App2,
                Properties.Settings.Default.App3
            };
        }

        public static void SetApps(List<string?> apps)
        {
            Properties.Settings.Default.App1 = apps[0];
            Properties.Settings.Default.App2 = apps[1];
            Properties.Settings.Default.App3 = apps[2];

            Properties.Settings.Default.Save();
        }

        public static int GetFocusLockDuration()
        {
            int got = Properties.Settings.Default.FocusLockDuration;
            int fallback = 10;

            return got <= 0 ? fallback : got;
        }

        public static void SetFocusLockDuration(int duration)
        {
            Properties.Settings.Default.FocusLockDuration = duration;

            Properties.Settings.Default.Save();
        }
    }
}
