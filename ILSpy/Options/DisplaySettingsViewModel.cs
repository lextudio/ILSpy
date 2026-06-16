using ICSharpCode.ILSpyX.Settings;
using System.Xml.Linq;
using System;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

#if !ROMA_UNO
using System.Windows.Media;
using System.Windows;
#endif

using TomsToolbox.Wpf;
using ICSharpCode.ILSpy.Themes;

namespace ICSharpCode.ILSpy.Options
{
	[ExportOptionPage(Order = 20)]
	[NonShared]
	public class DisplaySettingsViewModel : ObservableObjectBase, IOptionPage
	{
		private DisplaySettings settings = new();
#if !ROMA_UNO
		private FontFamily[] fontFamilies;
#endif
		private SessionSettings sessionSettings;

		public DisplaySettingsViewModel()
		{
#if !ROMA_UNO
			fontFamilies = [settings.SelectedFont];

			Task.Run(FontLoader).ContinueWith(continuation => {
				FontFamilies = continuation.Result;
				if (continuation.Exception == null)
					return;
				foreach (var ex in continuation.Exception.InnerExceptions)
				{
					MessageBox.Show(ex.ToString());
				}
			});
#endif
		}

		public string Title => Properties.Resources.Display;

		public DisplaySettings Settings {
			get => settings;
			set => SetProperty(ref settings, value);
		}

		public SessionSettings SessionSettings {
			get => sessionSettings;
			set => SetProperty(ref sessionSettings, value);
		}

#if !ROMA_UNO
		public FontFamily[] FontFamilies {
			get => fontFamilies;
			set => SetProperty(ref fontFamilies, value);
		}
#else
		public string[] FontFamilies { get; } =
			SkiaSharp.SKFontManager.Default.GetFontFamilies()
				.OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
				.ToArray();
#endif

		public double[] FontSizes { get; } = Enumerable.Range(6, 24 - 6 + 1).Select(i => (double)i).ToArray();

		public void Load(SettingsSnapshot snapshot)
		{
			Settings = snapshot.GetSettings<DisplaySettings>();
			SessionSettings = snapshot.GetSettings<SessionSettings>();
		}

#if !ROMA_UNO
		static bool IsSymbolFont(FontFamily fontFamily)
		{
			foreach (var tf in fontFamily.GetTypefaces())
			{
				try
				{
					if (tf.TryGetGlyphTypeface(out GlyphTypeface glyph))
						return glyph.Symbol;
				}
				catch (Exception)
				{
					return true;
				}
			}
			return false;
		}

		static FontFamily[] FontLoader()
		{
			return Fonts.SystemFontFamilies
				.Where(ff => !IsSymbolFont(ff))
				.OrderBy(ff => ff.Source)
				.ToArray();
		}
#endif

		public void LoadDefaults()
		{
			Settings.LoadFromXml(new XElement("empty"));
			SessionSettings.Theme = ThemeManager.Current.DefaultTheme;
		}
	}
}
