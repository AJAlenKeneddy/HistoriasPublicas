using System;
using System.Collections.Generic;

namespace StoryBlazeServer.Models;

public partial class Voto
{
    public int VotoId { get; set; }

    public int? FragmentoId { get; set; }

    public int? UsuarioId { get; set; }

    public int? Voto1 { get; set; }

    public DateTime? FechaVoto { get; set; }

    public virtual Fragmento? Fragmento { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
