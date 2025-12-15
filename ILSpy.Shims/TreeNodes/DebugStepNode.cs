using System.Collections.ObjectModel;

namespace ICSharpCode.ILSpy.TreeNodes
{
    public class DebugStepNode
    {
        public string Description { get; set; }
        public ObservableCollection<DebugStepNode> Children { get; } = new ObservableCollection<DebugStepNode>();
        public bool IsExpanded { get; set; }

        public DebugStepNode(string description)
        {
            Description = description;
        }
    }
}
