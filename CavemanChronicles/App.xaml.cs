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
            // Start with title page 
            var navigationPage = new NavigationPage(new TitlePage())
            {
                BarBackgroundColor = Colors.Black,
                BarTextColor = Color.FromArgb("#00FF00")
            };

            return new Window(navigationPage) { Title = "Caveman Chronicles" };
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Load monsters on app start
            try
            {
                // Get the main page to access services
                if (Windows.FirstOrDefault()?.Page is NavigationPage navPage)
                {
                    var monsterLoader = navPage.Handler?.MauiContext?.Services.GetService<MonsterLoaderService>();
                    if (monsterLoader != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Starting to load monsters...");
                        await monsterLoader.LoadAllMonsters();
                        System.Diagnostics.Debug.WriteLine("Monsters loaded successfully!");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("MonsterLoaderService not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading monsters on startup: {ex.Message}");
            }
        }
    }
}