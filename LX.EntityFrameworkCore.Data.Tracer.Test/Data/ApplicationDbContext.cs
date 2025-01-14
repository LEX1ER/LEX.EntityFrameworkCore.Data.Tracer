using System;
using LX.EntityFrameworkCore.Data.Tracer.Test.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LX.EntityFrameworkCore.Data.Tracer.Test.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : TraceDbContext(options)
{
    public DbSet<User> Users { get; set; }
}
