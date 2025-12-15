using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ICSharpCode.ILSpy
{
    internal static class Images
    {
        // Return null for the ImageSource shim; callers in AboutPage handle nulls gracefully.
        public static IImage OK => null;

        public static IImage Load(object owner, string path) => null;





		public static object SubTypes {
			get { return "SubTypes"; }
		}

		public static object ListFolder { get { return "ListFolder"; } }
		public static object ListFolderOpen { get { return "ListFolderOpen"; } }

		public static object Header { get; internal set; }
		public static object MetadataTableGroup { get; internal set; }
		public static object Library { get; internal set; }
		public static object Namespace { get; internal set; }
		public static object FolderClosed { get; internal set; }
		public static object FolderOpen { get; internal set; }
		public static object MetadataTable { get; internal set; }
		public static object ExportedType { get; internal set; }
		public static object TypeReference { get; internal set; }
		public static object MethodReference { get; internal set; }
		public static object FieldReference { get; internal set; }
		public static object Interface { get; internal set; }
		public static object Class { get; internal set; }
		public static Bitmap Save { get; internal set; }
		public static object Assembly { get; internal set; }
		public static Bitmap ViewCode { get; internal set; }
		public static object AssemblyWarning { get; internal set; }
		public static object MetadataFile { get; internal set; }
		public static object FindAssembly { get; internal set; }
		public static object SuperTypes { get; internal set; }
		public static object ReferenceFolder { get; internal set; }
		public static object ResourceImage { get; internal set; }
		public static object Resource { get; internal set; }
		public static object ResourceResourcesFile { get; internal set; }
		public static object ResourceXml { get; internal set; }
		public static object ResourceXsd { get; internal set; }
		public static object ResourceXslt { get; internal set; }
	}
}
