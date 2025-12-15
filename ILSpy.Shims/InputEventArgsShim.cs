using System;
using System.Windows.Input;

namespace System.Windows
{
    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);
    
    public class RoutedEventArgs : EventArgs
    {
        public bool Handled { get; set; }
    }
}
