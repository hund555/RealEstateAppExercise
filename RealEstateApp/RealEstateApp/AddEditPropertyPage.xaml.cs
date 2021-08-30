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
            await Navigation.PopToRootAsync();
        }

        private async void GetLocationButton_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token);
                _property.Latitude = location.Latitude;
                _property.Longitude = location.Longitude;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Handle not supported on device exception
            }
            
        }
        protected override void OnDisappearing()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
            base.OnDisappearing();
        }
    }
}