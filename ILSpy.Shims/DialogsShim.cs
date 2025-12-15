using System;

namespace Microsoft.Win32
{
    public class OpenFolderDialog
    {
        public bool Multiselect { get; set; }
        public string Title { get; set; }
        public string FolderName { get; set; }
		public string InitialDirectory { get; internal set; }

		public bool? ShowDialog()
        {
            // Shim: no UI — return false to indicate cancel; calling code handles null/false
            return false;
        }
    }

    public class SaveFileDialog
    {
        public string FileName { get; set; } = string.Empty;
        public string Filter { get; set; } = string.Empty;
        public string InitialDirectory { get; set; } = string.Empty;

        public bool? ShowDialog()
        {
            // Shim: no UI — return false to indicate cancel
            return false;
        }
    }

    public class OpenFileDialog
    {
        public string FileName { get; set; } = string.Empty;
        public string[] FileNames { get; set; } = Array.Empty<string>();
        public string Filter { get; set; } = string.Empty;
        public string InitialDirectory { get; set; } = string.Empty;
        public bool Multiselect { get; set; }
        public bool RestoreDirectory { get; set; }

        public bool? ShowDialog()
        {
            // Shim: no UI — return false to indicate cancel
            return false;
        }
    }
}
