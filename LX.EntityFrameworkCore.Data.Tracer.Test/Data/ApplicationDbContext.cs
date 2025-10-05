using System;
using LX.EntityFrameworkCore.Data.Tracer.Interfaces;
using LX.EntityFrameworkCore.Data.Tracer.Test.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LX.EntityFrameworkCore.Data.Tracer.Test.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser currentUser) : TraceDbContext<ITrace>(options, currentUser)
{
    public DbSet<User> Users { get; set; }
}
