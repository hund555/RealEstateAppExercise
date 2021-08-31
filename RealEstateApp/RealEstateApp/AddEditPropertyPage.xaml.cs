using RealEstateApp.Models;
using RealEstateApp.Services;
using System.Collections.ObjectModel;
using System.Linq;
using TinyIoC;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System;
using System.Threading;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEditPropertyPage : ContentPage
    {
        CancellationTokenSource cts;
        private IRepository Repository;

        #region PROPERTIES
        public ObservableCollection<Agent> Agents { get; }

        private Property _property;
        public Property Property
        {
            get => _property;
            set
            {
                _property = value;
                if (_property.AgentId != null)
                {
                    SelectedAgent = Agents.FirstOrDefault(x => x.Id == _property?.AgentId);
                }

            }
        }

        private Agent _selectedAgent;

        public Agent SelectedAgent
        {
            get => _selectedAgent;
            set
            {
                if (Property != null)
                {
                    _selectedAgent = value;
                    Property.AgentId = _selectedAgent?.Id;
                }
            }
        }

        public string StatusMessage { get; set; }

        public Color StatusColor { get; set; } = Color.White;
        #endregion

        public AddEditPropertyPage(Property property = null)
        {
            InitializeComponent();

            Repository = TinyIoCContainer.Current.Resolve<IRepository>();
            Agents = new ObservableCollection<Agent>(Repository.GetAgents());

            if (property == null)
            {
                Title = "Add Property";
                Property = new Property();
            }
            else
            {
                Title = "Edit Property";
                Property = property;
            }

            BindingContext = this;
        }

        private async void SaveProperty_Clicked(object sender, System.EventArgs e)
        {
            if (IsValid() == false)
            {
                StatusMessage = "Please fill in all required fields";
                StatusColor = Color.Red;
                try
                {
                    TimeSpan duration = TimeSpan.FromSeconds(5);
                    Vibration.Vibrate(duration);
                }
                catch (FeatureNotSupportedException)
                {
                    // Feature not supported on device
                }
                catch (Exception)
                {
                    // Other error has occurred.
                }
            }
            else
            {
                Repository.SaveProperty(Property);
                await Navigation.PopToRootAsync();
            }
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Property.Address)
                || Property.Beds == null
                || Property.Price == null
                || Property.AgentId == null)
                return false;

            return true;
        }

        private async void CancelSave_Clicked(object sender, System.EventArgs e)
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
            await Navigation.PopToRootAsync();
        }
        #region 3.1
        private async void GetLocationButton_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token);

                if (location != null)
                {
                    _property.Latitude = location.Latitude;
                    _property.Longitude = location.Longitude;

                    #region 3.3
                    var placemark = await Geocoding.GetPlacemarksAsync(location);
                    if (placemark != null)
                    {
                        var address = placemark.FirstOrDefault();
                        _property.Address = $"{address.SubThoroughfare} {address.Thoroughfare}, {address.Locality} {address.PostalCode} {address.CountryName}";
                    }
                    #endregion
                }
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException)
            {
                // Handle permission exception
            }
            catch (Exception)
            {
                // Handle not supported on device exception
            }

        }

        protected override void OnDisappearing()
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

            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }

            base.OnDisappearing();
        }
        #endregion

        #region 3.3
        private async void GetAddressLocationButton_Clicked(object sender, EventArgs e)
        {
            if (_property.Address == null)
            {
                await DisplayAlert("Alert", "Du skal indtaste en adresse", "Ok");
            }
            else
            {
                try
                {
                    var location = await Geocoding.GetLocationsAsync(_property.Address);
                    if (location != null)
                    {
                        var cords = location.FirstOrDefault();
                        _property.Latitude = cords.Latitude;
                        _property.Longitude = cords.Longitude;
                    }
                }
                catch (FeatureNotSupportedException)
                {

                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
        #endregion

        public Color BatteryMessageColor { get; set; }
        public string BatteryMessage { get; set; }

        #region 3.4
        protected override void OnAppearing()
        {
            #region 3.7
            var level = Battery.ChargeLevel; // returns 0.0 to 1.0 or 1.0 when on AC or no battery.

            var state = Battery.State;

            if (level <= 0.2)
            {
                BatteryMessage = "Dit batteri er ved at være fladt";
                BatteryMessageColor = Color.Red;
                if (state == BatteryState.Charging)
                {
                    BatteryMessageColor = Color.Yellow;
                }
                if (Battery.EnergySaverStatus == EnergySaverStatus.On)
                {
                    BatteryMessageColor = Color.Green;
                }
            }

            #endregion

            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {

            }
            else
            {
                GetAddressLocationButton.IsVisible = false;
                GetLocationButton.IsVisible = false;
                DisplayAlert("Internet connextion", "Du har ingen internet", "Ok");
            }
            base.OnAppearing();
        }

        #endregion

        #region 3.7

        public bool ToggleFlashlight { get; set; } = false;
        private async void Flashlight_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (ToggleFlashlight)
                {
                    await Flashlight.TurnOffAsync();
                    ToggleFlashlight = false;
                }
                else
                {
                    await Flashlight.TurnOnAsync();
                    ToggleFlashlight = true;
                }
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
            }
            catch (PermissionException)
            {
                // Handle permission exception
            }
            catch (Exception)
            {
                // Unable to turn on/off flashlight
            }
        }
        #endregion
    }
}