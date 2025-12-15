using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ProjectRover.SearchResults;
using ProjectRover.ViewModels;

namespace ICSharpCode.ILSpy.Views;

public partial class SearchPane : UserControl
{
    public SearchPane()
    {
        InitializeComponent();
    }

    public TextBox SearchTextBoxControl => SearchTextBox;

    private void OnClearSearchClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            // TODO: vm.SearchText = string.Empty;
        }

        SearchTextBox.Focus();
    }

    private void OnSearchResultDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Handled)
            return;

        if (DataContext is not MainWindowViewModel vm || sender is not ListBox listBox)
            return;

        if (listBox.SelectedItem is SearchResult result)
        {
            //vm.NavigateToSearchResult(result);
            e.Handled = true;
        }
    }
}
