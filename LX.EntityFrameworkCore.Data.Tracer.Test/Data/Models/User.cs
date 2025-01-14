using System;
using LX.EntityFrameworkCore.Data.Tracer.Interfaces;

namespace LX.EntityFrameworkCore.Data.Tracer.Test.Data.Models;

public class User : ITrace
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
