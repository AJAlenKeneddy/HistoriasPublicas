using System;
using System.Collections.Generic;

namespace StoryBlazeServer.Models;

public partial class Categoria
{
    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Historia> Historia { get; set; } = new List<Historia>();
}
