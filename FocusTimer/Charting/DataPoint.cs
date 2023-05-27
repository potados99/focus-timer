using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Charting
{
    class DataPoint : DateTimePoint
    {
        private bool _IsSelected = false;

        public bool IsSelected
        {
            get { return _IsSelected; }
            set { 
                _IsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
}
