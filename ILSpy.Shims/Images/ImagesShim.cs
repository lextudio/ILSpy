using System;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using ICSharpCode.Decompiler.TypeSystem;

namespace ICSharpCode.ILSpy
{
    internal static class Images
    {
        private static IImage Load(string path)
        {
            try
            {
                var uri = new Uri($"avares://ProjectRover/Assets/{path}");
                if (AssetLoader.Exists(uri))
                {
                    var source = SvgSource.Load(uri.ToString(), uri);
                    return new SvgImage { Source = source };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load image {path}: {ex}");
            }
            return null;
        }

        public static IImage OK => Load("StatusOKOutline.svg");
        
        public static IImage Load(object owner, string path)
        {
             if (string.IsNullOrEmpty(path)) return null;
             if (path == "Images/Warning") return Load("StatusWarningOutline.svg");
             if (path.EndsWith(".svg")) return Load(path);
             return null;
        }

		internal static ImageSource GetIcon(object @event, object value, bool isStatic)
		{
            // TODO: Implement dynamic icon selection
			return null;
		}

		internal static object GetOverlayIcon(Accessibility accessibility)
		{
			return null;
		}

		public static object SubTypes => "SubTypes";

		public static IImage ListFolder => Load("FolderClosed.svg");
		public static IImage ListFolderOpen => Load("FolderOpened.svg");

		public static IImage Header => Load("Header.svg");
		public static IImage MetadataTableGroup => Load("Tables.svg");
		public static IImage Library => Load("Assembly.svg");
		public static IImage Namespace => Load("Namespace.svg");
		public static IImage FolderClosed => Load("FolderClosed.svg");
		public static IImage FolderOpen => Load("FolderOpened.svg");
		public static IImage MetadataTable => Load("DataTable.svg");
		public static IImage ExportedType => Load("ClassPublic.svg");
		public static IImage TypeReference => Load("ClassPublic.svg");
		public static IImage MethodReference => Load("MethodPublic.svg");
		public static IImage FieldReference => Load("FieldPublic.svg");
		public static IImage Interface => Load("InterfacePublic.svg");
		public static IImage Class => Load("ClassPublic.svg");
		public static IImage Field => Load("FieldPublic.svg");
		public static IImage Method => Load("MethodPublic.svg");
		public static IImage Property => Load("PropertyPublic.svg");
		public static IImage Event => Load("EventPublic.svg");
		public static IImage Literal => Load("ConstantPublic.svg");
		public static IImage Save => Load("Save.svg");
		public static IImage Assembly => Load("Assembly.svg");
		public static IImage ViewCode => Load("ViewCode.svg");
		public static IImage AssemblyWarning => Load("ReferenceWarning.svg");
		public static IImage MetadataFile => Load("Metadata.svg");
		public static IImage FindAssembly => Load("Open.svg");
		public static IImage SuperTypes => Load("SuperTypes.svg");
		public static IImage ReferenceFolder => Load("ReferenceGroup.svg");
		public static IImage ResourceImage => Load("Resource.svg");
		public static IImage Resource => Load("Resources.svg");
		public static IImage ResourceResourcesFile => Load("ResourceFile.svg");
		public static IImage ResourceXml => Load("ResourceFile.svg");
		public static IImage ResourceXsd => Load("ResourceFile.svg");
		public static IImage ResourceXslt => Load("ResourceFile.svg");
		public static IImage Heap => Load("Heap.svg");
		public static IImage Metadata => Load("Metadata.svg");

		public static IImage Search => Load("Search.svg");
	}
}
