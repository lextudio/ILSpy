using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Windows.Input;

namespace ICSharpCode.ILSpy
{
    public partial class DebugStepsView : UserControl
    {
        public System.Collections.ObjectModel.ObservableCollection<ICSharpCode.ILSpy.TreeNodes.DebugStepNode> TreeRoot { get; } = new System.Collections.ObjectModel.ObservableCollection<ICSharpCode.ILSpy.TreeNodes.DebugStepNode>();

        public DebugStepsView()
        {
            InitializeComponent();
            // populate with a small sample so design-time and runtime show something until real VM is bound
            if (TreeRoot.Count == 0)
            {
                var root = new ICSharpCode.ILSpy.TreeNodes.DebugStepNode("Root");
                root.Children.Add(new ICSharpCode.ILSpy.TreeNodes.DebugStepNode("Step 1"));
                root.Children.Add(new ICSharpCode.ILSpy.TreeNodes.DebugStepNode("Step 2"));
                TreeRoot.Add(root);
            }
        }

        private void ShowStateAfter_Click(object? sender, RoutedEventArgs e)
        {
            // no-op shim
        }

        private void ShowStateBefore_Click(object? sender, RoutedEventArgs e)
        {
            // no-op shim
        }

        private void DebugStep_Click(object? sender, RoutedEventArgs e)
        {
            // no-op shim
        }

        private void tree_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            // no-op shim
        }
    }
}
