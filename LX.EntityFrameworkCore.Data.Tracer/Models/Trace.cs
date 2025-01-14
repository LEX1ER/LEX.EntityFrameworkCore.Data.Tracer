using Action = LX.EntityFrameworkCore.Data.Tracer.Enums.Action;

namespace LX.EntityFrameworkCore.Data.Tracer.Models;

public class Trace
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = default!;
    public string EntityData { get; set; } = default!;
    public Action Action { get; set; }
    public DateTime ActionAt { get; set; }
    public string? ActionBy { get; set; }
}
