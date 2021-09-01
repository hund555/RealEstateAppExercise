using RealEstateApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CompassPage : ContentPage
    {
        public Property Property { get; set; }
        public string CurrentAspect { get; set; }
        public double RotationAngle { get; set; }
        public double CurrentHeading { get; set; }

        public CompassPage(Property property)
        {
            InitializeComponent();
            Property = property;
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            Compass.ReadingChanged += Compass_ReadingChanged;
            if (!Compass.IsMonitoring)
            {
                Compass.Start(SensorSpeed.UI);
            }
            base.OnAppearing();
        }

        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            CurrentHeading = e.Reading.HeadingMagneticNorth;

            if (CurrentHeading <= 45 || CurrentHeading > 315)
            {
                RotationAngle = 0;
                CurrentAspect = "North";
            }
            else if (CurrentHeading > 45 && CurrentHeading <= 135)
            {
                RotationAngle = 270;
                CurrentAspect = "East";
            }
            else if (CurrentHeading > 135 && CurrentHeading <= 225)
            {
                RotationAngle = 180;
                CurrentAspect = "South";
            }
            else if (CurrentHeading > 225 && CurrentHeading <= 315)
            {
                RotationAngle = 90;
                CurrentAspect = "West";
            }
        }

        protected override void OnDisappearing()
        {
            if (Compass.IsMonitoring)
            {
                Compass.Stop();
            }
            Compass.ReadingChanged -= Compass_ReadingChanged;
            Property.Aspect = CurrentAspect;
            base.OnDisappearing();
        }
    }
}