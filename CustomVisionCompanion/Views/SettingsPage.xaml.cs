using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.System;
using CustomVisionCompanion.Services;

namespace CustomVisionCompanion.Views
{
    public sealed partial class SettingsPage : Page
    {
        private const string customVisionWebSite = "https://www.customvision.ai";

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void CustomVisionWebSite_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(customVisionWebSite));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            TrainingKeyTextBox.Text = Settings.TrainingKey ?? string.Empty;
            PredictionKeyTextBox.Text = Settings.PredictionKey ?? string.Empty;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Settings.TrainingKey = TrainingKeyTextBox.Text;
            Settings.PredictionKey = PredictionKeyTextBox.Text;

            base.OnNavigatingFrom(e);
        }
    }
}
