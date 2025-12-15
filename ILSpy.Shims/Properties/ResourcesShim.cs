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

		public static string DebugSteps { get { return "Debug Steps"; } }

		public static string NewList { get { return "New List"; } }
		public static string ListExistsAlready { get { return "A list with that name already exists."; } }
		public static string ListsResetConfirmation { get { return "Are you sure you want to reset lists?"; } }
		public static string ListDeleteConfirmation { get { return "Are you sure you want to delete this list?"; } }
		public static string RenameList { get { return "Rename List"; } }
		public static string AddPreconfiguredList { get { return "Add preconfigured list"; } }

		// Additional resource keys used by ManageAssemblyListsDialog
		public static string ManageAssemblyLists { get { return "Manage Assembly Lists"; } }
		public static string C_lone { get { return "Clone"; } }
		public static string R_ename { get { return "Rename"; } }
		public static string OpenListDialog__Delete { get { return "Delete"; } }
		public static string _Reset { get { return "Reset"; } }
		public static string Close { get { return "Close"; } }

		public static string ProjectExportPathTooLong { get;set;}
		public static string DecompilationCompleteInF1Seconds { get; internal set; }
		public static string DisplayCode { get; internal set; }
		public static string SaveCode { get; internal set; }

		// Keys used by OpenFromGacDialog
		public static string OpenFrom { get { return "Open From"; } }
		public static string _Search { get { return "Search"; } }
		public static string OpenListDialog__Open { get { return "Open"; } }
		public static string Cancel { get { return "Cancel"; } }

		// AboutPage / update-related resources
		public static string _Help { get { return "Help"; } }
		public static string _About { get { return "About"; } }
		public static string About { get { return "About"; } }
		public static string ILSpyVersion { get { return "ILSpy version: "; } }
		public static string AutomaticallyCheckUpdatesEveryWeek { get { return "Automatically check for updates every week"; } }
		public static string ILSpyAboutPageTxt { get { return "ILSpyAboutPageTxt"; } }
		public static string CheckUpdates { get { return "Check for Updates"; } }
		public static string Checking { get { return "Checking..."; } }
		public static string UsingLatestRelease { get { return "You are using the latest release."; } }
		public static string VersionAvailable { get { return "Version {0} is available."; } }
		public static string Download { get { return "Download"; } }
		public static string UsingNightlyBuildNewerThanLatestRelease { get { return "You are using a nightly build newer than the latest release."; } }

		public static string NETFrameworkVersion { get; internal set; }

		// Added missing keys used by UpdatePanelViewModel
		public static string CheckAgain { get { return "Check again"; } }
		public static string ILSpyVersionAvailable { get { return "A new ILSpy version is available."; } }
		public static string UpdateILSpyFound { get { return "No updates for ILSpy were found."; } }

	}
}
