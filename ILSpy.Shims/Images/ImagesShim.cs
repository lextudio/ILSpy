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

		internal static ImageSource GetIcon(object icon, object overlay, bool isStatic)
		{
			string name = null;
			string access = "Public";

			if (overlay is AccessOverlayIcon accessOverlay)
			{
				switch (accessOverlay)
				{
					case AccessOverlayIcon.Public: access = "Public"; break;
					case AccessOverlayIcon.Internal: access = "Internal"; break;
					case AccessOverlayIcon.Protected: access = "Protected"; break;
					case AccessOverlayIcon.Private: access = "Private"; break;
					case AccessOverlayIcon.ProtectedInternal: access = "Protected"; break;
					case AccessOverlayIcon.PrivateProtected: access = "Private"; break;
					case AccessOverlayIcon.CompilerControlled: access = "Private"; break;
				}
			}

			if (icon is TypeIcon typeIcon)
			{
				switch (typeIcon)
				{
					case TypeIcon.Class: name = "Class"; break;
					case TypeIcon.Struct: name = "Structure"; break;
					case TypeIcon.Interface: name = "Interface"; break;
					case TypeIcon.Delegate: name = "Delegate"; break;
					case TypeIcon.Enum: name = "Enum"; break;
					default: name = "Class"; break;
				}
			}
			else if (icon is MemberIcon memberIcon)
			{
				switch (memberIcon)
				{
					case MemberIcon.Literal: name = "Constant"; break;
					case MemberIcon.FieldReadOnly: name = "Field"; break;
					case MemberIcon.Field: name = "Field"; break;
					case MemberIcon.Property: name = "Property"; break;
					case MemberIcon.Method: name = "Method"; break;
					case MemberIcon.Event: name = "Event"; break;
					case MemberIcon.EnumValue: name = "EnumerationItem"; break;
					case MemberIcon.Constructor: name = "Method"; break;
					case MemberIcon.VirtualMethod: name = "Method"; break;
					case MemberIcon.Operator: name = "Method"; break;
					case MemberIcon.ExtensionMethod: name = "Method"; break;
					case MemberIcon.PInvokeMethod: name = "Method"; break;
					case MemberIcon.Indexer: name = "Property"; break;
					default: name = "Method"; break;
				}
			}

			if (name != null)
			{
				var image = Load($"{name}{access}.svg");
				if (image != null) return image as ImageSource;
				
				// Fallback to Public
				image = Load($"{name}Public.svg");
				if (image != null) return image as ImageSource;
			}

			return Class as ImageSource;
		}

		internal static object GetOverlayIcon(Accessibility accessibility)
		{
			// Return null for now (overlays not implemented in this shim)
			return null;
		}

		public static IImage SubTypes => Load("SubTypes.svg");

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
