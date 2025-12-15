using Avalonia.Controls;
using ProjectRover.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ICSharpCode.ILSpy.Views;

public partial class UpdatePanel : UserControl
{
    public UpdatePanel()
    {
        InitializeComponent();
        
        //if (!Design.IsDesignMode)
            // TODO: DataContext = App.Current.Services.GetRequiredService<IUpdatePanelViewModel>();
    }
}
