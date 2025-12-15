using System;
using System.Windows.Input;

namespace System.Windows
{
    public class RoutedEventArgs : EventArgs
    {
        public bool Handled { get; set; }
    }
}
