using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace focus.lib
{
    internal class Settings
    {
        public IEnumerable<string> GetProcessAllowList()
        {
            return new List<string> { "focus", "devenv", "chrome" };
        }
    }
}
