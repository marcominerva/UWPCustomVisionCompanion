using System;
using System.Collections.Generic;
using System.Text;

namespace CustomVisionCompanion.Engine.Models
{
    public class Project
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid? CurrentIterationId { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastModified { get; set; }

        public ProjectSettings Settings { get; set; }

        public string ThumbnailUri { get; set; }
    }
}
