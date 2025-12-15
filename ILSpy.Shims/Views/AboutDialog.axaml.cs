using Avalonia.Controls;
using ProjectRover.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ICSharpCode.ILSpy.Views;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();

        // TODO: if (!Design.IsDesignMode)
        //    DataContext = App.Current.Services.GetRequiredService<IAboutWindowViewModel>();
    }
}
