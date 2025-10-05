using LX.EntityFrameworkCore.Data.Tracer.Interfaces;
using LX.EntityFrameworkCore.Data.Tracer.Test.Data;
using LX.EntityFrameworkCore.Data.Tracer.Test.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Action = LX.EntityFrameworkCore.Data.Tracer.Enums.Action;

namespace LX.EntityFrameworkCore.Data.Tracer.Test;

public class TraceDbContextTest
{
    private readonly ApplicationDbContext Context;
    public TraceDbContextTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("LX.EntityFrameworkCore.Data.Tracer.Db")
            .ConfigureWarnings(x =>
                x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options;

        var mockCurrentUser = new Mock<ICurrentUser>();
        mockCurrentUser.Setup(u => u.UserName).Returns("test-user");

        Context = new ApplicationDbContext(options, mockCurrentUser.Object);
    }

    [Theory]
    [InlineData("Lexter Rae Sabado", "LEX1ER", "LEX1ER")]
    public void CanAdd(string? name, string userName, string password)
    {
        var user = new User()
        {
            Name = name,
            UserName = userName,
            Password = password
        };
        Context.Users.Add(user);
        Context.SaveChanges();
        var userId = user.Id;

        var userInMemory = Context.Users.Single(x => x.Id == user.Id);
        Assert.True(userInMemory.Name == name);

        var traceInMemory = Context.Traces.Single(x => x.EntityId == userId);
        Assert.True(traceInMemory.EntityId == userId);
    }

    [Theory]
    [InlineData("Lexter Rae Sabado-Modified", "LEX1ER-Modified")]
    public void CanModify(string? name, string userName)
    {
        var oldName = "Lexter Rae Sabado";
        var oldUserName = "LEX1ER";
        var oldPassword = "LEX1ER";

        var user = new User()
        {
            Name = oldName,
            UserName = oldUserName,
            Password = oldPassword
        };
        Context.Users.Add(user);
        Context.SaveChanges();
        var userId = user.Id;

        var toBeModifiedUserInMemory = Context.Users.Single(x => x.Id == userId);
        Assert.True(toBeModifiedUserInMemory.Name == oldName);

        toBeModifiedUserInMemory.Name = name;
        toBeModifiedUserInMemory.UserName = userName;
        Context.SaveChanges();

        var userInMemory = Context.Users.Single(x => x.Id == userId);
        Assert.True(userInMemory.Name == name);
        Assert.True(userInMemory.UserName == userName);

        var tracesInMemory = Context.Traces.Where(x => x.EntityId == userId);
        Assert.Collection(tracesInMemory,
            traceInMemory => Assert.Equal(Action.Added, traceInMemory.Action),
            traceInMemory => Assert.Equal(Action.Modified, traceInMemory.Action));
    }

    [Fact]
    public void CanDelete()
    {
        var user = new User()
        {
            Name = "Lexter Rae Sabado",
            UserName = "LEX1ER",
            Password = "LEX1ER"
        };
        Context.Users.Add(user);
        Context.SaveChanges();
        var userId = user.Id;

        var toBeDeletedUserInMemory = Context.Users.Single(x => x.Id == userId);
        Assert.True(toBeDeletedUserInMemory.Name == user.Name);

        Context.Users.Remove(toBeDeletedUserInMemory);
        Context.SaveChanges();

        var userInMemory = Context.Users.Where(x => x.Id == userId);
        Assert.Empty(userInMemory);

        var tracesInMemory = Context.Traces.Where(x => x.EntityId == toBeDeletedUserInMemory.Id);
        Assert.Collection(tracesInMemory,
            traceInMemory => Assert.Equal(Action.Added, traceInMemory.Action),
            traceInMemory => Assert.Equal(Action.Deleted, traceInMemory.Action));
    }

    [Fact]
    public void CanRead()
    {
        var user = new User()
        {
            Name = "Lexter Rae Sabado",
            UserName = "LEX1ER",
            Password = "LEX1ER"
        };
        Context.Users.Add(user);
        Context.SaveChanges();
        var usersInMemory = Context.Users;
        Assert.NotEmpty(usersInMemory);
    }
}
