namespace CavemanChronicles
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Load monsters on app start
            Task.Run(async () =>
            {
                var monsterLoader = Handler?.MauiContext?.Services.GetService<MonsterLoaderService>();
                if (monsterLoader != null)
                {
                    await monsterLoader.LoadAllMonsters();
                }
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Start with title page 
            var navigationPage = new NavigationPage(new TitlePage())
            {
                BarBackgroundColor = Colors.Black,
                BarTextColor = Color.FromArgb("#00FF00")
            };

            return new Window(navigationPage) { Title = "Caveman Chronicles" };
        }
    }
}