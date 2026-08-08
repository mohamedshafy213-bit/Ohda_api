using Entities.Models.BaseTables;
using System;
using System.Collections.Generic;
namespace Entities.Models.Tables;


public partial class Serlog : BaseTable
{
    public int Id { get; set; }

    public string? Sertimestamp { get; set; }

    public string? Serlevel { get; set; }

    public string? Sertemplate { get; set; }

    public string? Sermessage { get; set; }

    public string? Serexception { get; set; }

    public string? Serproperties { get; set; }

    public DateTime? Serts { get; set; }
}
