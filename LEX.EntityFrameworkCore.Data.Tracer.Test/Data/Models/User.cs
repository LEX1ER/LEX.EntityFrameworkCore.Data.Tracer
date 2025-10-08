using System;
using LEX.EntityFrameworkCore.Data.Tracer.Interfaces;

namespace LEX.EntityFrameworkCore.Data.Tracer.Test.Data.Models;

public class User : ITrace
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
