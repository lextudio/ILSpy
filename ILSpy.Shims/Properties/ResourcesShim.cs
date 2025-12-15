namespace ICSharpCode.ILSpy.Properties
{
    internal static class Resources
    {
        public static string ProjectExportFormatNonSDKHint => "The project was exported using the non-SDK project format.";
        public static string ProjectExportFormatSDKHint => "";
        public static string ProjectExportFormatChangeSettingHint => "Change the project export format setting to use the SDK-style project format.";
        public static string CouldNotUseSdkStyleProjectFormat => "Could not use SDK-style project format for this project.";
        public static string OpenExplorer => "Open Explorer";
        public static string Metadata { get { return "Metadata"; } }
		public static string DerivedTypes { get { return "Derived Types"; } }

		public static object Loading { get { return "Loading..."; } }

		public static string NewTab { get; internal set; }

		public static string Decompiling { get;set;}

		public static string DecompilationWasCancelled {get;set;}

		public static string ProjectExportPathTooLong { get;set;}
		public static string DecompilationCompleteInF1Seconds { get; internal set; }
		public static string DisplayCode { get; internal set; }
		public static string SaveCode { get; internal set; }

	}
}
