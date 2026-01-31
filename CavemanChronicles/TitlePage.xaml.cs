
namespace CavemanChronicles
{
    public partial class TitlePage : ContentPage
    {
        public TitlePage()
        {
            InitializeComponent();
        }

        private async void OnNewGame(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CharacterCreationPage());
        }

        private async void OnLoadGame(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoadGamePage());
        }
    }
}