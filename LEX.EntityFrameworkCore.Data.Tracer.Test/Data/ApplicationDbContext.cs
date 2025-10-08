using System;
using LEX.EntityFrameworkCore.Data.Tracer.Interfaces;
using LEX.EntityFrameworkCore.Data.Tracer.Test.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LEX.EntityFrameworkCore.Data.Tracer.Test.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser currentUser) : TraceDbContext<ITrace>(options, currentUser)
{
    public DbSet<User> Users { get; set; }
}
