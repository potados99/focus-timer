using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Lib.Utility
{
    public static class ObjectExtensions
    {
        public static log4net.ILog GetLogger(this object obj)
        {
            return log4net.LogManager.GetLogger(obj.GetType());
        }
    }
}
