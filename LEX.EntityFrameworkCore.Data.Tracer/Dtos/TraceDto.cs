namespace LEX.EntityFrameworkCore.Data.Tracer.Dtos;

public class TraceDto
{
    public class TraceGetDto<T>
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string EntityName { get; set; } = default!;
        public T EntityData { get; set; } = default!;
        public string Action { get; set; } = default!;
        public DateTime ActionAt { get; set; }
        public string? ActionBy { get; set; }
    }
}
