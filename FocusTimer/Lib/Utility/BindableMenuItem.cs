using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace FocusTimer.Lib.Utility
{
    internal class BindableMenuItem
    {
        public delegate void CheckHandler(bool isChecked);
        public event CheckHandler OnCheck;

        public bool IsCheckable { get; set; }       

        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                Debug.WriteLine("!!&&@");
                _IsChecked = value;
                OnCheck?.Invoke(value);
            }
        }

        public Image? Icon { get; set; }
        public string Header { get; set; }
        public BindableMenuItem[] Children { get; set; }
    }
}
