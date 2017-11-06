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
using CustomVisionCompanion.Services;
using CustomVisionCompanion.Extensions;
using System.Net;
using CustomVisionCompanion.Engine;
using System.Net.Http;
using CustomVisionCompanion.Engine.Extensions;

namespace CustomVisionCompanion.Views
{
    public sealed partial class MainPage : Page
    {
        private CustomVisionClient customVisionClient;

        public MainPage()
        {
            this.InitializeComponent();

            var qualities = Enum.GetValues(typeof(CameraCaptureUIMaxPhotoResolution)).Cast<CameraCaptureUIMaxPhotoResolution>()
                .Where(q => q == CameraCaptureUIMaxPhotoResolution.Large3M || q == CameraCaptureUIMaxPhotoResolution.MediumXga)
                .ToList();

            ImageQuality.ItemsSource = qualities;
            ImageQuality.SelectedItem = qualities.FirstOrDefault(q => q == CameraCaptureUIMaxPhotoResolution.Large3M);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            InitializeEndpoints();
            await LoadProjectsAsync();

            base.OnNavigatedTo(e);
        }

        private void InitializeEndpoints()
        {
            customVisionClient = new CustomVisionClient(Settings.PredictionKey, Settings.TrainingKey);
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                TakePhotoButton.IsEnabled = false;
                PickPhotoButton.IsEnabled = false;

                ProgressBar.Visibility = Visibility.Visible;

                // Get the list of all Custom Vision projects.
                var projects = await customVisionClient.GetProjectsAsync();
                ProjectList.ItemsSource = projects.ToDictionary(k => k.Id, v => v.Name);
                ProjectList.SelectedIndex = 0;

                TakePhotoButton.IsEnabled = projects.Any();
                PickPhotoButton.IsEnabled = projects.Any();
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
            {
                GotoSettingsPage();
            }
            catch (Exception ex)
            {
                await DialogService.ShowAsync(ex.Message);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void TakePhoto_Click(object sender, RoutedEventArgs e)
        {
            Enum.TryParse<CameraCaptureUIMaxPhotoResolution>(ImageQuality.SelectedItem.ToString(), out var resolution);
            var photo = await MediaPicker.TakePhotoAsync(resolution);
            await PredictAsync(photo, resolution);
        }

        private async void PickPhoto_Click(object sender, RoutedEventArgs e)
        {
            Enum.TryParse<CameraCaptureUIMaxPhotoResolution>(ImageQuality.SelectedItem.ToString(), out var resolution);
            var photo = await MediaPicker.PickPhotoAsync();
            await PredictAsync(photo, resolution);
        }

        private async Task PredictAsync(Stream photo, CameraCaptureUIMaxPhotoResolution resolution)
        {
            try
            {
                if (photo != null)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    TakePhotoButton.IsEnabled = false;
                    PickPhotoButton.IsEnabled = false;

                    CleanUp();

                    PreviewImage.Source = await photo.AsImageSourceAsync();

                    // Call the prediction endpoint of Custom Vision.
                    var size = GetResolution(resolution);
                    var result = await customVisionClient.PredictImageAsync((Guid)ProjectList.SelectedValue, photo, size.Width, size.Height);
                    VisionResults.ItemsSource = result.Predictions.Select(p => $"{p.Tag}: {p.Probability:P1}");

                    photo.Dispose();
                }
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                var message = "You need to set a default iteration in the 'Performance' tab of the Custom Vision Portal.";
                await DialogService.ShowAsync(message);
            }
            catch (Exception ex)
            {
                await DialogService.ShowAsync(ex.Message);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                TakePhotoButton.IsEnabled = true;
                PickPhotoButton.IsEnabled = true;
            }
        }

        private void CleanUp()
        {
            PreviewImage.Source = null;
            VisionResults.ItemsSource = null;
        }

        private void Settings_Click(object sender, RoutedEventArgs e) => GotoSettingsPage();

        private void GotoSettingsPage() => Frame.Navigate(typeof(SettingsPage));

        private (int Width, int Height) GetResolution(CameraCaptureUIMaxPhotoResolution resolution)
        {
            int width, height;

            switch (resolution)
            {
                case CameraCaptureUIMaxPhotoResolution.MediumXga:
                    width = 640;
                    height = 480;
                    break;

                case CameraCaptureUIMaxPhotoResolution.Large3M:
                default:
                    width = 1920;
                    height = 1080;
                    break;
            }

            return (width, height);
        }
    }
}
