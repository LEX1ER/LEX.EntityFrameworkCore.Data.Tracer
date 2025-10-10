# LEX.EntityFrameworkCore.Data.Tracer

[![NuGet](https://img.shields.io/nuget/v/LEX1ER.EntityFrameworkCore.Data.Tracer.svg)](https://www.nuget.org/packages/LEX1ER.EntityFrameworkCore.Data.Tracer/)
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

When you add a new entity that implements `ITrace`, the tracer automatically records the **Create** action in your trace log entity.

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
   - **Action** – the type of operation (`Create`, `Update`, `Delete`)  
   - **ActionAt** – the timestamp of when the change occurred  
   - **ActionBy** – the current user ID (from `ICurrentUser`, if implemented)  

#### 🧾 Example Trace Log Entry

```json
{
  "EntityId": "b123f570-4ac1-4f53-bdf2-21e1a3e94a2e",
  "EntityName": "User",
  "EntityData": "{\"Name\": \"John Doe\", \"Email\": \"john@example.com\"}",
  "Action": "Create",
  "ActionAt": "2025-10-11T14:05:23Z",
  "ActionBy": "f4c8b5e2-abc1-4a09-a8e2-97e1c1b45c93"
}
```


