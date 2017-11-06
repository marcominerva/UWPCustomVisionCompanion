using System;
using System.Collections.Generic;
using System.Text;

namespace CustomVisionCompanion.Engine.Models
{
    public class ImageTagPrediction
    {
        public Guid TagId { get; set; }

        public string Tag { get; set; }

        public double Probability { get; set; }
    }
}
