namespace CavemanChronicles
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Start with character creation page wrapped in NavigationPage
            var navigationPage = new NavigationPage(new CharacterCreationPage())
            {
                BarBackgroundColor = Colors.Black,
                BarTextColor = Color.FromArgb("#00FF00")
            };

            return new Window(navigationPage) { Title = "Caveman Chronicles" };
        }
    }
}