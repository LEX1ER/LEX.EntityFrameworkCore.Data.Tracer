using System.Reflection; 
using LX.EntityFrameworkCore.Data.Tracer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Action = LX.EntityFrameworkCore.Data.Tracer.Enums.Action;

namespace LX.EntityFrameworkCore.Data.Tracer;

public class TraceDbContext<ITraceSource>(DbContextOptions options) : DbContext(options) where ITraceSource : class
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
    public DbSet<Trace> Traces { get; set; }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var nullableTransaction = base.Database.CurrentTransaction;
        var transaction = nullableTransaction ?? base.Database.BeginTransaction();
        try
        {
            TraceEntries<ITraceSource>();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            if (nullableTransaction == null) transaction.Commit();
            return result;
        }
        catch (Exception)
        {
            if (nullableTransaction == null) transaction.Rollback();
            throw;
        }
    }
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var nullableTransaction = base.Database.CurrentTransaction;
        var transaction = nullableTransaction ?? await base.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            TraceEntries<ITraceSource>();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken); ;
            if (nullableTransaction == null) await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            if (nullableTransaction == null) await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    
    private void TraceEntries<ITraceOutput>() where ITraceOutput : class
    {
        var traceEntries = new List<TraceEntry>();
        foreach (var entry in ChangeTracker.Entries<ITraceOutput>().ToList())
        {
            var traceEntry = new TraceEntry();
            if (entry.CurrentValues != null)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        traceEntry.Action = Action.Added;
                        break;
                    case EntityState.Deleted:
                        traceEntry.Action = Action.Deleted;
                        break;
                    case EntityState.Modified:
                        traceEntry.Action = Action.Modified;
                        break;
                }
            }
            traceEntries.Add(traceEntry);
            traceEntry.EntityName = entry.Metadata.ShortName();
            CreateTraceEntry(entry, entry, traceEntry);
            var entries = ChangeTracker.Entries();
            var navigations = entry.Navigations;
            foreach (var navigation in navigations)
            {
                if (navigation.CurrentValue != null)
                {
                    var navigationIsArray = navigation.CurrentValue.GetType().IsGenericType;
                    if (navigationIsArray)
                    {
                        var navigationEntries = ChangeTracker.Entries().Where(x => x.Metadata.ShortName() == navigation.Metadata.TargetEntityType.ShortName());

                        var isModifiedState = false;
                        var isAddedState = false;
                        var isDeletedState = false;

                        var hasAddedState = navigationEntries.Any(x => x.State == EntityState.Added);
                        var hasDeletedState = navigationEntries.Any(x => x.State == EntityState.Deleted);
                        var hasUnchangedState = navigationEntries.Any(x => x.State == EntityState.Unchanged);

                        var isAllAddedState = navigationEntries.All(x => x.State == EntityState.Added);
                        var isAllDeletedState = navigationEntries.All(x => x.State == EntityState.Deleted);

                        if (isAllAddedState)
                        {
                            isAddedState = true;
                        }
                        else if ((hasAddedState || hasDeletedState) && hasUnchangedState)
                        {
                            isModifiedState = true;
                        }
                        else if (isAllDeletedState)
                        {
                            isDeletedState = true;
                        }

                        if (isAddedState || isAllDeletedState && EntityState.Deleted == entry.State)
                        {
                            List<object> navigationTraceEntries = [];
                            foreach (var navigationEntry in navigationEntries)
                            {
                                CreateTraceEntry(navigationEntry, entry, traceEntry, navigation.Metadata.Name, navigationTraceEntries);
                            }

                            if (navigationTraceEntries.Count > 0)
                            {
                                traceEntry.Values[navigation.Metadata.Name] = navigationTraceEntries;
                            }
                        }
                        else if (isModifiedState || isDeletedState)
                        {
                            List<object> newTraceEntries = [];
                            List<object> oldTraceEntries = [];
                            var modifiedTraceEntry = new Dictionary<string, object>();

                            foreach (var navigationEntry in navigationEntries.Where(x => x.State == EntityState.Added || x.State == EntityState.Unchanged))
                            {
                                CreateTraceEntry(navigationEntry, entry, traceEntry, navigation.Metadata.Name, newTraceEntries, isModifiedState);
                            }
                            modifiedTraceEntry["new"] = newTraceEntries;

                            foreach (var navigationEntry in navigationEntries.Where(x => x.State == EntityState.Deleted || x.State == EntityState.Unchanged))
                            {
                                CreateTraceEntry(navigationEntry, entry, traceEntry, navigation.Metadata.Name, oldTraceEntries, isModifiedState);
                            }
                            modifiedTraceEntry["old"] = oldTraceEntries;

                            traceEntry.Values[navigation.Metadata.Name] = modifiedTraceEntry;
                        }

                    }
                    else
                    {
                        var navigationEntry = ChangeTracker.Entries().Single(x => x.Metadata.ShortName() == navigation.Metadata.TargetEntityType.ShortName());
                        CreateTraceEntry(navigationEntry, entry, traceEntry, navigation.Metadata.Name);
                    }
                }
            }
        }
        foreach (var traceEntry in traceEntries)
        {
            if (traceEntry.Values.Count > 0)
            {
                Set<Trace>().Add(traceEntry.ToTrace());
            }
        }
    }
    private static void CreateTraceEntry(EntityEntry mainEntry, EntityEntry subEntry, TraceEntry traceEntry, string? navigationPropertyName = null, List<object>? navigationTraceEntries = null, bool? isModifiedState = false)
    {
        var properties = mainEntry.Properties;
        Dictionary<string, object> navigationPropertyNameDictionary = default!;
        if (navigationPropertyName != null)
        {
            navigationPropertyNameDictionary = [];
        }

        foreach (var property in properties)
        {
            var propertyName = property.Metadata.Name;
            if (property.Metadata.IsPrimaryKey())
            {
                if (property.CurrentValue != null && navigationPropertyName == null)
                {
                    traceEntry.EntityId = (Guid)property.CurrentValue;

                    if (subEntry.State == EntityState.Added || subEntry.State == EntityState.Deleted || navigationTraceEntries != null)
                    {
                        traceEntry.Values[propertyName] = property.CurrentValue?.ToString() ?? "";
                    }
                }

                if (navigationPropertyName != null && property.CurrentValue != null && navigationTraceEntries != null)
                {
                    navigationPropertyNameDictionary[propertyName] = property.CurrentValue?.ToString() ?? "";
                }
            }
            else if (subEntry.State == EntityState.Added || subEntry.State == EntityState.Deleted && property.CurrentValue != null)
            {
                if (property.CurrentValue != null)
                {
                    if (navigationPropertyName == null)
                    {
                        traceEntry.Values[propertyName] = property.CurrentValue?.ToString() ?? "";
                    }
                    else if (navigationPropertyName != null)
                    {
                        var valueType = property.CurrentValue.GetType();
                        if (valueType.IsArray)
                        {
                            if ((property.CurrentValue as Array)?.Length > 0) navigationPropertyNameDictionary[propertyName] = property.CurrentValue;
                        }
                        else
                        {

                            navigationPropertyNameDictionary[propertyName] = property.CurrentValue?.ToString() ?? "";
                        }
                    }
                }
            }
            else if (subEntry.State == EntityState.Modified)
            {
                Dictionary<string, object> value = [];
                if (property.CurrentValue?.ToString() != property.OriginalValue?.ToString() && property.IsModified)
                {
                    if (navigationPropertyName == null)
                    {
                        value["new"] = property.CurrentValue?.ToString() ?? "";
                        value["old"] = property.OriginalValue?.ToString() ?? "";
                        traceEntry.Values[propertyName] = value;
                    }
                    else if (navigationPropertyName != null)
                    {
                        navigationPropertyNameDictionary[propertyName] = new
                        {
                            @new = property.CurrentValue?.ToString() ?? "",
                            old = property.OriginalValue?.ToString() ?? ""
                        };
                    }

                }
                else if (mainEntry.State == EntityState.Added || mainEntry.State == EntityState.Deleted || isModifiedState == true)
                {
                    if (navigationPropertyName != null)
                    {
                        navigationPropertyNameDictionary[propertyName] = property.CurrentValue?.ToString() ?? "";
                    }
                }
            }
        }

        if (navigationPropertyNameDictionary?.Count > 0 && navigationPropertyName != null)
        {
            traceEntry.Values[navigationPropertyName] = navigationPropertyNameDictionary;
            navigationTraceEntries?.Add(navigationPropertyNameDictionary);
        }
    }
}
