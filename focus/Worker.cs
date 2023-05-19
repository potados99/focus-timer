using focus.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace focus
{
    internal class Worker
    {
        public void StartWorking()
        {
            var watcher = new Watcher();
            var actuator = new Actuator();
            var settings = new Settings();
            var judge = new Judge(settings);

            watcher.OnFocused += (prev, current) =>
            {
                var result = judge.Decide(current);

                if (result == Judge.Result.ALLOW)
                {
                    // Do nothing
                } else
                {
                    actuator.SetFocus(prev);
                    actuator.Minimize(current);
                }
            };

            watcher.StartListening();
        }


    }
}
