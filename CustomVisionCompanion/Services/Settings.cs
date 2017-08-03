using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CustomVisionCompanion.Services
{
    public static class Settings
    {
        private const string trainingKey = nameof(trainingKey);
        private const string predictionKey = nameof(predictionKey);

        private static ApplicationDataContainer settings;

        static Settings()
        {
            settings = ApplicationData.Current.RoamingSettings;
        }

        public static string TrainingKey
        {
            get => settings.Values[trainingKey]?.ToString();
            set => settings.Values[trainingKey] = value;
        }

        public static string PredictionKey
        {
            get => settings.Values[predictionKey]?.ToString();
            set => settings.Values[predictionKey] = value;
        }
    }
}
