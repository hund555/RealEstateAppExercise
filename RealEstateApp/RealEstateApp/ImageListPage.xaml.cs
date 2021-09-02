using RealEstateApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageListPage : ContentPage
    {
        public Property Property { get; set; }
        private int _position;

        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public ImageListPage(Property property)
        {
            Property = property;

            InitializeComponent();
            BindingContext = this;
        }
        protected override void OnAppearing()
        {
            Accelerometer.ShakeDetected += Accelerometer_ShakeDetected;
            Accelerometer.Start(SensorSpeed.Game);

            base.OnAppearing();
        }
        void Accelerometer_ShakeDetected(object sender, EventArgs e)
        {
            _position++;
        }

        protected override void OnDisappearing()
        {
            Accelerometer.ShakeDetected -= Accelerometer_ShakeDetected;

            Accelerometer.Stop();
            base.OnDisappearing();
        }
    }
}