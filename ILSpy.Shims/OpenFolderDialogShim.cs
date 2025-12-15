using System;

namespace Microsoft.Win32
{
    public class OpenFolderDialog
    {
        public bool Multiselect { get; set; }
        public string Title { get; set; }
        public string FolderName { get; set; }

        public bool? ShowDialog()
        {
            // Shim: no UI — return false to indicate cancel; calling code handles null/false
            return false;
        }
    }
}
