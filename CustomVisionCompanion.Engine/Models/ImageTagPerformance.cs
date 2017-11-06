using System;
using System.Collections.Generic;
using System.Text;

namespace CustomVisionCompanion.Engine.Models
{
    public class ImageTagPerformance
    {
        public Guid TagId { get; set; }

        public string TagName { get; set; }

        public double Precision { get; set; }

        public double PrecisionStdDeviation { get; set; }

        public double Recall { get; set; }

        public double RecallStdDeviation { get; set; }
    }
}
