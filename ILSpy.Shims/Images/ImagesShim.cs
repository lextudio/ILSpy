using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ICSharpCode.ILSpy
{
    internal static class Images
    {
        // Return null for the ImageSource shim; callers in AboutPage handle nulls gracefully.
        public static Bitmap OK => null;
        public static Bitmap Load(object owner, string path) => null;

		public static object SubTypes {
			get { return "SubTypes"; }
		}

		public static Bitmap ListFolder { get ;set; }
		public static Bitmap ListFolderOpen { get ;set; }

		public static Bitmap Header { get; internal set; }
		public static Bitmap MetadataTableGroup { get; internal set; }
		public static Bitmap Library { get; internal set; }
		public static Bitmap Namespace { get; internal set; }
		public static Bitmap FolderClosed { get; internal set; }
		public static Bitmap FolderOpen { get; internal set; }
		public static Bitmap MetadataTable { get; internal set; }
		public static Bitmap ExportedType { get; internal set; }
		public static Bitmap TypeReference { get; internal set; }
		public static Bitmap MethodReference { get; internal set; }
		public static Bitmap FieldReference { get; internal set; }
		public static Bitmap Interface { get; internal set; }
		public static Bitmap Class { get; internal set; }
		public static Bitmap Field { get; internal set; }
		public static Bitmap Method { get; internal set; }
		public static Bitmap Property { get; internal set; }
		public static Bitmap Event { get; internal set; }
		public static Bitmap Literal { get; internal set; }
		public static Bitmap Save { get; internal set; }
		public static Bitmap Assembly { get; internal set; }
		public static Bitmap ViewCode { get; internal set; }
		public static Bitmap AssemblyWarning { get; internal set; }
		public static Bitmap MetadataFile { get; internal set; }
		public static Bitmap FindAssembly { get; internal set; }
		public static Bitmap SuperTypes { get; internal set; }
		public static Bitmap ReferenceFolder { get; internal set; }
		public static Bitmap ResourceImage { get; internal set; }
		public static Bitmap Resource { get; internal set; }
		public static Bitmap ResourceResourcesFile { get; internal set; }
		public static Bitmap ResourceXml { get; internal set; }
		public static Bitmap ResourceXsd { get; internal set; }
		public static Bitmap ResourceXslt { get; internal set; }
		public static Bitmap Heap { get; internal set; }
		public static Bitmap Metadata { get; internal set; }

		public static Bitmap Search { get; internal set; }
	}
}
