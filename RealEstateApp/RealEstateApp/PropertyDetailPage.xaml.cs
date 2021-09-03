using Newtonsoft.Json;
using RealEstateApp.Models;
using RealEstateApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
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

        #region 4.3
        private async void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new ImageListPage(Property));
        }
        #endregion

        #region 5.1
        private async void PhoneNumber_Tapped(object sender, System.EventArgs e)
        {
            string choise = await DisplayActionSheet(Property.Vendor.Phone, "Cancel", null, "Call", "sms");
            try
            {
                switch (choise)
                {
                    case "Call":
                        PhoneDialer.Open(Property.Vendor.Phone);
                        break;
                    case "sms":
                        var message = new SmsMessage
                        {
                            Recipients = new List<string> { Property.Vendor.Phone },
                            Body = $"Hej, {Property.Vendor.FirstName}, angående {Property.Address}"
                        };
                        await Sms.ComposeAsync(message);
                        break;
                    default:
                        break;
                }
            }
            catch (ArgumentNullException)
            {
                // Number was null or white space
            }
            catch (FeatureNotSupportedException)
            {
                // Phone Dialer is not supported on this device.
            }
            catch (Exception)
            {
                // Other error has occurred.
            }
        }

        private async void Email_Tapped(object sender, EventArgs e)
        {
            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var attachmentFilePath = Path.Combine(folder, "property.txt");
                File.WriteAllText(attachmentFilePath, $"{Property.Address}");
                var message = new EmailMessage
                {
                    Subject = Property.Address,
                    To = new List<string> { Property.Vendor.Email }
                };
                message.Attachments.Add(new EmailAttachment(attachmentFilePath));
                await Email.ComposeAsync(message);
            }
            catch (ArgumentNullException)
            {
                // Number was null or white space
            }
            catch (FeatureNotSupportedException)
            {
                // Phone Dialer is not supported on this device.
            }
            catch (Exception)
            {
                // Other error has occurred.
            }
        }
        #endregion

        #region 5.2
        private async void OpenMaps_Clicked(object sender, EventArgs e)
        {
            var location = new Location((double)Property.Latitude, (double)Property.Longitude);
            
            await Map.OpenAsync(location);
        }

        private async void Directions_Clicked(object sender, EventArgs e)
        {
            var location = new Location((double)Property.Latitude, (double)Property.Longitude);
            var options = new MapLaunchOptions
            {
                Name = Property.Address,
                NavigationMode = NavigationMode.Driving
            };
            await Map.OpenAsync(location, options);
        }
        #endregion

        #region 5.3
        private async void OpenBrowserSystem_Clicked(object sender, EventArgs e)
        {
            var options = new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show,
                PreferredToolbarColor = Color.Blue,
                PreferredControlColor = Color.Red
            };
            await Browser.OpenAsync("Https://eucsyd.dk", options);
        }

        private async void OpenBrowserExternal_Clicked(object sender, EventArgs e)
        {
            var options = new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.External,
                TitleMode = BrowserTitleMode.Show,
                PreferredToolbarColor = Color.Blue,
                PreferredControlColor = Color.Green
            };
            await Browser.OpenAsync("Https://eucsyd.dk", options);
        }

        private async void OpenFile_Clicked(object sender, EventArgs e)
        {
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(Property.ContractFilePath)
            });
        }
        #endregion

        #region 5.4
        private async void ShareText_Clicked(object sender, EventArgs e)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Uri = "Https://eucsyd.dk",
                Subject = Property.Address,
                Text = $"{Property.Address}, {Property.Price}, {Property.Beds}",
                Title = "Share Property"
            });
        }

        private async void ShareFile_Clicked(object sender, EventArgs e)
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share Property Contract",
                File = new ShareFile(Property.ContractFilePath)
            });
        }

        private async void Clipboard_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(JsonConvert.SerializeObject(Property));
        }
        #endregion
    }
}