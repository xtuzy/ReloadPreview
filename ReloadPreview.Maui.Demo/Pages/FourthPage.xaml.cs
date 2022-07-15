namespace ReloadPreview.Maui.Demo.Pages;

public partial class FourthPage : ContentPage
{
	public FourthPage()
	{
		InitializeComponent();
	}

	private void button_Clicked(object sender, EventArgs e)
	{
		label.Text = "Clicked Button";
	}
}