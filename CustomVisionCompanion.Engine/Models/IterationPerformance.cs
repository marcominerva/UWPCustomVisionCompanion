using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomVisionCompanion.Engine.Models
{
    public class IterationPerformance
    {
        public double Precision { get; set; }

        public double PrecisionStdDeviation { get; set; }

        public double Recall { get; set; }

        public double RecallStdDeviation { get; set; }

        [JsonProperty("PerTagPerformance")]
        public IEnumerable<ImageTagPerformance> TagPerformance { get; set; }
    }
}
