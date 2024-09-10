using System;
using System.Collections.Generic;

namespace StoryBlazeServer.Models
{
    public partial class Categorium
    {
        public Categorium()
        {
            Historia = new HashSet<Historia>();
        }

        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = null!;

        public virtual ICollection<Historia> Historia { get; set; }
    }
}
