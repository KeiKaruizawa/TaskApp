using TaskApp.MVVM.ViewModels;

namespace TaskApp.MVVM.Views;

public partial class MainView : ContentPage
{
	public MainView()
	{
		InitializeComponent();
		BindingContext = new MainViewModel();		
}
}