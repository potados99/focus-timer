using System.Collections.Generic;

namespace focus.lib
{
    internal class Settings
    {
        public IEnumerable<string> GetProcessAllowList()
        {
            return new List<string> { "focus", "devenv", "chrome" };
        }
        public IEnumerable<string> GetWindowClassAllowList()
        {
            return new List<string> { "HwndWrapper[focus;;549b1f85-3acf-4749-b738-e34f27a370f8]", "HwndWrapper[DefaultDomain;;10eccd9d-91c9-4634-b394-d74d9fd52a4c]", "Chrome_WidgetWin_1" };
        }
    }
}
