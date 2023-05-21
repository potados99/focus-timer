using FocusTimer.Lib;

namespace FocusTimer
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
                }
                else
                {
                    actuator.SetFocus(prev);
                    actuator.Minimize(current);
                }
            };

            watcher.StartListening();
        }


    }
}
