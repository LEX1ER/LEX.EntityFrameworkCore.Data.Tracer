using Newtonsoft.Json;
using Action = LX.EntityFrameworkCore.Data.Tracer.Enums.Action;

namespace LX.EntityFrameworkCore.Data.Tracer.Models;

public class TraceEntry
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = default!;
    public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
    public Action Action { get; set; }
    public string? ActionBy { get; set; }
    public DateTime ActionAt { get; set; } = DateTime.Now;
    public Trace ToTrace()
    {
        var trace = new Trace();
        trace.Action = Action;
        trace.ActionBy = ActionBy;
        trace.ActionAt = ActionAt;
        trace.EntityId = EntityId;
        trace.EntityName = EntityName;
        trace.EntityData = JsonConvert.SerializeObject(Values);
        return trace;
    }
}
