using System.Composition;

namespace ICSharpCode.ILSpy
{
    [Export(typeof(IPlatformService))]
    [Shared]
    public class PlatformExports : WpfPlatformService
    {
        [ImportingConstructor]
        public PlatformExports(ICSharpCode.ILSpy.Docking.DockWorkspace dockWorkspace)
        {
            this.DockWorkspace = dockWorkspace;
        }
    }
}
