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
using Microsoft.Cognitive.CustomVision;
using System.Threading.Tasks;
using CustomVisionCompanion.Services;
using CustomVisionCompanion.Extensions;
using Microsoft.Rest;
using System.Net;

namespace CustomVisionCompanion.Views
{
    public sealed partial class MainPage : Page
    {
        private TrainingApi trainingApi;
        private PredictionEndpoint predictionEndpoint;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var qualities = Enum.GetValues(typeof(CameraCaptureUIMaxPhotoResolution)).Cast<CameraCaptureUIMaxPhotoResolution>()
                .Where(q => q == CameraCaptureUIMaxPhotoResolution.Large3M || q == CameraCaptureUIMaxPhotoResolution.MediumXga)
                .ToList();

            ImageQuality.ItemsSource = qualities;
            ImageQuality.SelectedItem = qualities.FirstOrDefault(q => q == CameraCaptureUIMaxPhotoResolution.MediumXga);

            InitializeEndpoint();
            await LoadProjectsAsync();

            base.OnNavigatedTo(e);
        }

        private void InitializeEndpoint()
        {
            var trainingCredentials = new TrainingApiCredentials(Settings.TrainingKey);
            var predictionCredentials = new PredictionEndpointCredentials(Settings.PredictionKey);

            trainingApi = new TrainingApi(trainingCredentials);
            predictionEndpoint = new PredictionEndpoint(predictionCredentials);
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                TakePhotoButton.IsEnabled = false;
                PickPhotoButton.IsEnabled = false;

                ProgressBar.Visibility = Visibility.Visible;

                var projects = await trainingApi.GetProjectsAsync();
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
            var photo = await MediaPicker.TakePhotoAsync((CameraCaptureUIMaxPhotoResolution)Enum.Parse(typeof(CameraCaptureUIMaxPhotoResolution), ImageQuality.SelectedItem.ToString()));
            await PredictAsync(photo);
        }

        private async void PickPhoto_Click(object sender, RoutedEventArgs e)
        {
            var photo = await MediaPicker.PickPhotoAsync((CameraCaptureUIMaxPhotoResolution)Enum.Parse(typeof(CameraCaptureUIMaxPhotoResolution), ImageQuality.SelectedItem.ToString()));
            await PredictAsync(photo);
        }

        private async Task PredictAsync(Stream photo)
        {
            try
            {
                if (photo != null)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    TakePhotoButton.IsEnabled = false;
                    PickPhotoButton.IsEnabled = false;

                    CleanUp();

                    var bitmap = await photo.AsSoftwareBitmapAsync();
                    PreviewImage.Source = await bitmap.AsImageSourceAsync();
                    var imageBuffer = await photo.ToArrayAsync();
                    ImageSize.Text = $"{bitmap.PixelWidth}x{bitmap.PixelHeight} ({imageBuffer.Length.ToFileSize()})";

                    using (var ms = new MemoryStream(imageBuffer))
                    {
                        var result = await predictionEndpoint.PredictImageAsync((Guid)ProjectList.SelectedValue, ms);
                        VisionResults.ItemsSource = result.Predictions.Select(p => $"{p.Tag}: {p.Probability:P1}");
                    }
                }
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
            ImageSize.Text = string.Empty;
        }

        private void Settings_Click(object sender, RoutedEventArgs e) => GotoSettingsPage();

        private void GotoSettingsPage()=> Frame.Navigate(typeof(SettingsPage));
    }
}
