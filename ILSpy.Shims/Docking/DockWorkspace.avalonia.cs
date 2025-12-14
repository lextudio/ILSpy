using ICSharpCode.ILSpy.ViewModels;

namespace ICSharpCode.ILSpy.Docking
{
    /// <summary>
    /// WPF-specific extensions to <see cref="DockWorkspace"/>.
    /// </summary>
    public partial class DockWorkspace
    {
        public void ShowText(string textOutput)
		{
			//TOOD: ActiveTabPage.ShowTextView(textView => textView.ShowText(textOutput));
		}

		public void InitializeLayout()
		{
			// TODO:
		}

		public void ResetLayout()
		{
			// TODO:
		}
	}
}