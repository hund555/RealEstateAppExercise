using RealEstateApp.Models;
using RealEstateApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TinyIoC;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PropertyListPage : ContentPage
    {
        public Location MyLocation { get; set; }
        IRepository Repository;
        public ObservableCollection<PropertyListItem> PropertiesCollection { get; set; } = new ObservableCollection<PropertyListItem>();

        public PropertyListPage()
        {
            InitializeComponent();

            Repository = TinyIoCContainer.Current.Resolve<IRepository>();
            LoadProperties();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            LoadProperties();
        }

        void OnRefresh(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            LoadProperties();
            list.IsRefreshing = false;
        }

        void LoadProperties()
        {
            PropertiesCollection.Clear();
            var items = Repository.GetProperties();
            
            foreach (Property item in items)
            {
                PropertyListItem p = new PropertyListItem(item);
                try
                {
                    p.Distance = Location.CalculateDistance((double)p.Property.Latitude, (double)p.Property.Longitude, MyLocation, DistanceUnits.Kilometers);
                }
                catch (Exception)
                {
                }
                PropertiesCollection.Add(p);
            }
            #region 3.2
            var newlist = new ObservableCollection<PropertyListItem>();
            foreach (var item in PropertiesCollection.OrderBy(d => d.Distance))
            {
                newlist.Add(item);
            }
            PropertiesCollection = newlist;
            #endregion
        }

        private async void ItemsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new PropertyDetailPage(e.Item as PropertyListItem));
        }

        private async void AddProperty_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditPropertyPage());
        }

        #region 3.2
        private async void SortAsync(object sender, EventArgs e)
        {
            try
            {
                MyLocation = await Geolocation.GetLastKnownLocationAsync();

                if (MyLocation == null)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    MyLocation = await Geolocation.GetLocationAsync(request);
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
                // Unable to get location
            }
            LoadProperties();
            
        }
        #endregion
    }
}