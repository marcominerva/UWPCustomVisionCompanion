using CustomVisionCompanion.Engine.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CustomVisionCompanion.Engine
{
    public class CustomVisionClient : IDisposable
    {
        private const string DefaultCustomVisionEndPoint = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/";

        public string TrainingKey { get; set; }

        public string PredictionKey { get; set; }

        private HttpClient client;

        public CustomVisionClient(string predictionKey, string trainingKey = null, string customVisionEndpoint = DefaultCustomVisionEndPoint)
        {
            PredictionKey = predictionKey;
            TrainingKey = trainingKey;

            client = new HttpClient
            {
                BaseAddress = new Uri(customVisionEndpoint.EndsWith("/") ? customVisionEndpoint : customVisionEndpoint += "/")
            };
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            var request = CreateTrainingRequest("Training/projects");
            var content = await SendRequestAsync<IEnumerable<Project>>(request);

            return content;
        }

        public async Task<IEnumerable<Iteration>> GetIterationsAsync(Guid projectId)
        {
            var request = CreateTrainingRequest($"Training/projects/{projectId}/iterations");
            var content = await SendRequestAsync<IEnumerable<Iteration>>(request);

            return content.OrderByDescending(i => i.TrainedAt).ToList();
        }
        public async Task<IterationPerformance> GetIterationPerformanceAsync(Guid projectId, Guid iterationId, double threshold = 0.9)
        {
            var request = CreateTrainingRequest($"Training/projects/{projectId}/iterations/{iterationId}/performance?threshold={threshold.ToString(CultureInfo.InvariantCulture)}");
            var content = await SendRequestAsync<IterationPerformance>(request);

            return content;
        }

        public async Task<ImagePredictionResult> PredictImageAsync(Guid projectId, Stream image, Guid? iterationId = null)
        {
            var request = CreatePredictRequest(projectId, iterationId);

            request.Content = new StreamContent(image);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var content = await SendRequestAsync<ImagePredictionResult>(request);
            return content;
        }

        public Task<ImagePredictionResult> PredictImageAsync(Guid projectId, Uri imageUrl, Guid? iterationId = null)
            => PredictImageAsync(projectId, imageUrl.ToString(), iterationId);

        public async Task<ImagePredictionResult> PredictImageAsync(Guid projectId, string imageUrl, Guid? iterationId = null)
        {
            var request = CreatePredictRequest(projectId, iterationId);

            request.Content = new StringContent(JsonConvert.SerializeObject(new
            {
                Url = imageUrl
            }));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var content = await SendRequestAsync<ImagePredictionResult>(request);
            return content;
        }

        private HttpRequestMessage CreateTrainingRequest(string uri)
        {
            EnsureTrainingKeySet();

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Training-Key", TrainingKey);

            return request;
        }

        private HttpRequestMessage CreatePredictRequest(Guid projectId, Guid? iterationId)
        {
            EnsurePredictionKeySet();

            var endpoint = $"Prediction/{projectId}/image?iterationId={iterationId}";
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Prediction-Key", PredictionKey);

            return request;
        }

        private async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
        {
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContentString = await response.Content.ReadAsStringAsync();
                var content = JsonConvert.DeserializeObject<T>(responseContentString);

                return content;
            }
            else
            {
                var exception = new HttpOperationException(response.ReasonPhrase)
                {
                    Request = request,
                    Response = response
                };

                throw exception;
            }
        }

        private void EnsureTrainingKeySet()
        {
            if (string.IsNullOrWhiteSpace(TrainingKey))
            {
                throw new ArgumentNullException(nameof(TrainingKey));
            }
        }

        private void EnsurePredictionKeySet()
        {
            if (string.IsNullOrWhiteSpace(PredictionKey))
            {
                throw new ArgumentNullException(nameof(PredictionKey));
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
