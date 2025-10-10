# LEX.EntityFrameworkCore.Data.Tracer

[![NuGet](https://img.shields.io/nuget/v/LEX.EntityFrameworkCore.Data.Tracer.svg)](https://www.nuget.org/packages/LEX.EntityFrameworkCore.Data.Tracer/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

An extension for **Entity Framework Core** that allows you to **trace data changes** (Create, Update, Delete) automatically for debugging, auditing, or logging purposes.

---

## 🚀 Features

- Tracks entity actions in `DbContext` (Create / Update / Delete)  
- Records before-and-after state changes  
- Supports custom user tracking via `ICurrentUser`  
- Lightweight and dependency-free (except EF Core + Newtonsoft.Json)  
- Easy integration with existing EF Core projects  

---

## 🧩 Installation

Install via NuGet:

```bash
dotnet add package LEX1ER.EntityFrameworkCore.Data.Tracer
```

## ⚙️ Usage

### 1. Inherit from `TraceDbContext<ITrace>`

Replace your `DbContext` base class with `TraceDbContext<ITrace>` where `ITrace` is your entity or log model implementing the `ITrace` interface.

### 2. Provide a current user implementation

Implement `ICurrentUser` to allow the tracer to identify who made the changes (optional but recommended).

---

### 🧩 Example Integration

```csharp
public interface ITrace
{
}
```

```csharp
public class User : ITrace
{
    ...
}
```

```csharp
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserName =>
        httpContextAccessor.HttpContext?.User?.Identity?.Name;
}
```

```csharp
public class ApplicationDbContext : TraceDbContext<ITrace>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser currentUser)
        : base(options, currentUser)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Need this for TraceConfiguration
        base.OnModelCreating(modelBuilder);
    }
}
```

## ✏️ Create Usage

When you add a new data on an entity that implements `ITrace`, the tracer automatically records the **Added** action in your trace log entity.

### Example

```csharp
var user = new User
{
    Name = "John Doe",
    Email = "john@example.com"
};

await context.Users.AddAsync(user, cancellationToken);
await context.SaveChangesAsync(cancellationToken);
```

### 🔍 What Happens

When you save changes, `TraceDbContext` automatically:

1. Detects the newly added `User` entity.  
2. Inserts a **Create** record into the created trace table (e.g., `Trace`).  
3. Logs the following details based on your `Trace` model:
   - **EntityId** – the primary key of the affected entity  
   - **EntityName** – the entity type (e.g., `User`)  
   - **EntityData** – the serialized JSON data of the entity state  
   - **Action** – the type of operation, represented by the enum:  
     - `Added`  
     - `Modified`  
     - `Deleted`  
     - `Restored`
   - **ActionAt** – the timestamp of when the change occurred  
   - **ActionBy** – the current userName (from `ICurrentUser`, if implemented)  

#### 🧾 Example Trace Log Entry

```json
{
  "EntityId": "b123f570-4ac1-4f53-bdf2-21e1a3e94a2e",
  "EntityName": "User",
  "EntityData": "{\"Name\": \"John Doe\", \"Email\": \"john@example.com\"}",
  "Action": "Added",
  "ActionAt": "2025-10-11T14:05:23Z",
  "ActionBy": "LoggedInUser_1"
}
```
### 💡 Tip: Avoid Duplicate Traces When Adding Related Entities

When adding entities that include **related data (navigation properties)**, use `.AsNoTracking()` on related entities before attaching them.  
This ensures Entity Framework Core does not mistakenly create **additional trace logs** for existing related records.

```csharp

// You need to put AsNoTracking here so it won't create any traces for Role.
var adminRole = context.Roles.AsNoTracking().SingleOrDefault(x=> x.Name == "Admin");

var userId = Guid.NewGuid();
var user = new User
{
    Name = "John Doe",
    Email = "john@example.com",
    Delegates = new ICollection<Delegate>
    {
        new Delegate()
        {
            roleId = adminRole.Id,
            userId = userId
        }
    }
};

await context.Users.AddAsync(user, cancellationToken);
await context.SaveChangesAsync(cancellationToken);
```

## 📝 Update Usage

When updating an entity, the tracer automatically detects which properties were modified and logs the **before** and **after** values.

### Example

```csharp
var user = await context.Users
    // Include navigation properties if you want them to be traced
    .Include(x => x.Profile)
    .SingleAsync(x => x.Id == request.Id, cancellationToken);

request.Adapt(user);

context.Users.Update(user);
await context.SaveChangesAsync(cancellationToken);
```
### 💡 Tip: Avoid Duplicate Traces When Updating Related Entities

If you’re loading data from another table or entity, use `.AsNoTracking()` to prevent Entity Framework Core from mistakenly creating **new trace logs** for unchanged entities.

```csharp
var user = await context.Users
    .AsNoTracking()
    .Include(x => x.Profile)
    .SingleAsync(x => x.Id == request.Id, cancellationToken);

// You need to put AsNoTracking here so it won't create new traces for Email.
// It will automatically update when setting a new value for user.Profile.Email.
var emails = context.UserEmails
    .AsNoTracking()
    .Where(e => e.ProfileId == user.Profile.Id)
    .ToList();

// Apply the change
var profile = user.Profile;
profile.Emails = emails;

// Then proceed with your update
context.Users.Update(user);
await context.SaveChangesAsync(cancellationToken);

```

### 🔍 What Happens

- `TraceDbContext` compares the **original** and **updated** values.  
- Only **modified fields** are logged in the `Traces` table.  
- The system automatically records both **old** and **new** property values.  

### 🧾 Example Trace Log Entry

```json
{
  "EntityId": "b123f570-4ac1-4f53-bdf2-21e1a3e94a2e",
  "EntityName": "User",
  "EntityData": "{\"Name\":{\"new\":\"Admin - 1\",\"old\":\"Admin\"}}",
  "Action": "Modified",
  "ActionAt": "2025-10-11T14:05:23Z",
  "ActionBy": "John123"
}
```
