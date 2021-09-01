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
    public partial class HeightCalculatorPage : ContentPage
    {
        public double CurrentPressure { get; set; }
        public double CurrentAltitude { get; set; }

        public BarometerMeasurement BarometerMeasurement { get; set; } = new BarometerMeasurement();

        public ObservableCollection<BarometerMeasurement> Measurements { get; set; } = new ObservableCollection<BarometerMeasurement>();
        public HeightCalculatorPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            Barometer.ReadingChanged += Barometer_ReadingChanged;

            if (!Barometer.IsMonitoring)
                Barometer.Start(SensorSpeed.UI);

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            if (Barometer.IsMonitoring)
                Barometer.Stop();
            base.OnDisappearing();
        }

        void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            var data = e.Reading;
            // Process Pressure
            CurrentPressure = data.PressureInHectopascals;
            CurrentAltitude = 44307.694 * (1 - Math.Pow(CurrentPressure / 1026.8, 0.190284));
        }

        private void SaveMeasurement_Clicked(object sender, EventArgs e)
        {
            BarometerMeasurement.Altitude = CurrentAltitude;
            BarometerMeasurement.Pressure = CurrentPressure;
            if (Measurements.Count > 0)
            {
                BarometerMeasurement.HeightChange = BarometerMeasurement.Altitude - Measurements.LastOrDefault().Altitude;
            }
            
            Measurements.Add(BarometerMeasurement);

            BarometerMeasurement = new BarometerMeasurement();
        }
    }
}