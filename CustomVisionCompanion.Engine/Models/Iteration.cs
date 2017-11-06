using System;
using System.Collections.Generic;
using System.Text;

namespace CustomVisionCompanion.Engine.Models
{
    public class Iteration
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastModified { get; set; }

        public DateTime? TrainedAt { get; set; }

        public bool IsDefault { get; set; }
    }
}
