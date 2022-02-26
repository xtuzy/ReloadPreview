namespace ReloadPreview.Maui.Demo
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
CounterLabel.Text="New Page 6";
CounterLabel.TextColor=Colors.Green;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            CounterLabel.Text = $"The Current count is a: {count}";
CounterLabel.TextColor=Colors.Red;
            SemanticScreenReader.Announce(CounterLabel.Text);
        }
    }
}