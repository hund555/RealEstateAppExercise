using RealEstateApp.Services;
using RealEstateApp.Services.Repository;
using System;
using System.Threading;
using TinyIoC;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RealEstateApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var container = TinyIoCContainer.Current;
            container.Register<IRepository, MockRepository>();

            MainPage = new AppShell();
        }

        #region 3.6
        public static CancellationTokenSource Cts { get; set; }
        #endregion

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected async override void OnSleep()
        {
            #region 3.5
            try
            {
                Vibration.Cancel();
            }
            catch (FeatureNotSupportedException)
            {
                // Feature not supported on device
            }
            catch (Exception)
            {
                // Other error has occurred.
            }
            #endregion
            #region 3.6
            if (Cts?.IsCancellationRequested ?? true)
                return;

            Cts.Cancel();

            await Flashlight.TurnOffAsync();
            #endregion
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
