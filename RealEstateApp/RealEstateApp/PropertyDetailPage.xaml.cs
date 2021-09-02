using RealEstateApp.Models;
using RealEstateApp.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TinyIoC;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PropertyDetailPage : ContentPage
    {
        public PropertyDetailPage(PropertyListItem propertyListItem)
        {
            InitializeComponent();

            Property = propertyListItem.Property;

            IRepository Repository = TinyIoCContainer.Current.Resolve<IRepository>();
            Agent = Repository.GetAgents().FirstOrDefault(x => x.Id == Property.AgentId);

            BindingContext = this;
        }

        public Agent Agent { get; set; }

        public Property Property { get; set; }

        private async void EditProperty_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new AddEditPropertyPage(Property));
        }

        #region 3.6
        public bool IsSpeaking { get; set; } = false;
        public float? VolumeInput { get; set; } = 0.75f;
        public float? PitchInput { get; set; } = 1.5f;
        public async Task SpeakNowDefaultSettings()
        {
            var settings = new SpeechOptions()
            {
                Volume = VolumeInput,
                Pitch = PitchInput
            };
            IsSpeaking = true;
            App.Cts = new CancellationTokenSource();
            
            await TextToSpeech.SpeakAsync(Property.Description, settings, cancelToken: App.Cts.Token);
            IsSpeaking = false;
            // This method will block until utterance finishes.
        }

        // Cancel speech if a cancellation token exists & hasn't been already requested.
        public void CancelSpeech()
        {
            if (App.Cts?.IsCancellationRequested ?? true)
                return;

            App.Cts.Cancel();
            IsSpeaking = false;
        }

        protected override void OnDisappearing()
        {
            CancelSpeech();
            base.OnDisappearing();
        }

        private async void StartSpeechbtn_Clicked(object sender, System.EventArgs e)
        {
            await SpeakNowDefaultSettings();
        }

        private void StopSpeechbtn_Clicked(object sender, System.EventArgs e)
        {
            CancelSpeech();
        }
        #endregion

        private async void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new ImageListPage(Property));
        }
    }
}