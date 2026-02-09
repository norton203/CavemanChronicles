using Microsoft.Maui.Controls;

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

        private async void OnSettings(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        // Hover effects for New Game button
        private void OnNewGamePointerEntered(object sender, PointerEventArgs e)
        {
            NewGameBorder.Stroke = new SolidColorBrush(Color.FromArgb("#FFD700"));
            NewGameBorder.StrokeThickness = 4;
            NewGameBorder.Opacity = 1.0;

            NewGameText.TextColor = Color.FromArgb("#FFFF00");
            NewGameShadow.TextColor = Color.FromArgb("#3a2817");

            NewGameLeftIcon.TextColor = Color.FromArgb("#FFFF00");
            NewGameRightIcon.TextColor = Color.FromArgb("#FFFF00");
        }

        private void OnNewGamePointerExited(object sender, PointerEventArgs e)
        {
            NewGameBorder.Stroke = new SolidColorBrush(Color.FromArgb("#654321"));
            NewGameBorder.StrokeThickness = 3;
            NewGameBorder.Opacity = 1.0;

            NewGameText.TextColor = Color.FromArgb("#FFD700");
            NewGameShadow.TextColor = Color.FromArgb("#1a0f0a");

            NewGameLeftIcon.TextColor = Color.FromArgb("#FFD700");
            NewGameRightIcon.TextColor = Color.FromArgb("#FFD700");
        }

        // Hover effects for Load Game button
        private void OnLoadGamePointerEntered(object sender, PointerEventArgs e)
        {
            LoadGameBorder.Stroke = new SolidColorBrush(Color.FromArgb("#00FFFF"));
            LoadGameBorder.StrokeThickness = 4;
            LoadGameBorder.Opacity = 1.0;

            LoadGameText.TextColor = Color.FromArgb("#00FFFF");
            LoadGameShadow.TextColor = Color.FromArgb("#003a3a");

            LoadGameLeftIcon.TextColor = Color.FromArgb("#00FFFF");
            LoadGameRightIcon.TextColor = Color.FromArgb("#00FFFF");
        }

        private void OnLoadGamePointerExited(object sender, PointerEventArgs e)
        {
            LoadGameBorder.Stroke = new SolidColorBrush(Color.FromArgb("#4a5f5f"));
            LoadGameBorder.StrokeThickness = 3;
            LoadGameBorder.Opacity = 1.0;

            LoadGameText.TextColor = Color.FromArgb("#00FFFF");
            LoadGameShadow.TextColor = Color.FromArgb("#001a1a");

            LoadGameLeftIcon.TextColor = Color.FromArgb("#00FFFF");
            LoadGameRightIcon.TextColor = Color.FromArgb("#00FFFF");
        }

        // Hover effects for Settings button
        private void OnSettingsPointerEntered(object sender, PointerEventArgs e)
        {
            SettingsBorder.Stroke = new SolidColorBrush(Color.FromArgb("#00FF00"));
            SettingsBorder.StrokeThickness = 4;
            SettingsBorder.Opacity = 1.0;

            SettingsText.TextColor = Color.FromArgb("#00FF00");
            SettingsShadow.TextColor = Color.FromArgb("#003a00");

            SettingsLeftIcon.TextColor = Color.FromArgb("#00FF00");
            SettingsRightIcon.TextColor = Color.FromArgb("#00FF00");
        }

        private void OnSettingsPointerExited(object sender, PointerEventArgs e)
        {
            SettingsBorder.Stroke = new SolidColorBrush(Color.FromArgb("#2d4d2d"));
            SettingsBorder.StrokeThickness = 3;
            SettingsBorder.Opacity = 1.0;

            SettingsText.TextColor = Color.FromArgb("#00FF00");
            SettingsShadow.TextColor = Color.FromArgb("#001a00");

            SettingsLeftIcon.TextColor = Color.FromArgb("#00FF00");
            SettingsRightIcon.TextColor = Color.FromArgb("#00FF00");
        }
    }
}