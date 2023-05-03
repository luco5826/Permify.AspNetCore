using System.Text;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;
using Permify.AspNetCore.Extensions;
using Permify.AspNetCore.Implementations;
using Permify.AspNetCore.Interfaces;
using Permify.AspNetCore.Model;

namespace Permify.AspNetCore.Tests;

[TestFixture]
public class PermifySerivceTests
{
    private IPermifyAuthorizationService _service;
    private HttpClient _httpClient = new HttpClient();

    private string DEFAULT_SCHEMA = @"{""schema"":""entity user {} entity organization { relation admin @user\n relation member @user } entity repository { relation  parent @organization \n relation owner @user \n action push = owner \n action read = owner or parent.admin or parent.member \n  action delete = parent.admin or owner }""}";

    [SetUp]
    public async Task SetUpTests()
    {
        var permifyContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithName(Guid.NewGuid().ToString("D"))
            .WithImage("ghcr.io/permify/permify:latest")
            .WithCommand("serve")
            .WithPortBinding(3478, true) // gRPC port mapping
            .WithPortBinding(3476, true) // REST port mapping
            .Build();

        await permifyContainer.StartAsync().ConfigureAwait(false);

        int grpcMappedPort = permifyContainer.GetMappedPublicPort(3478);
        int restMappedPort = permifyContainer.GetMappedPublicPort(3476);

        // TODO: Wait for a specific event instead of a fixed timeout
        await Task.Delay(1000);

        var requestContent = new StringContent(DEFAULT_SCHEMA, Encoding.UTF8, "application/json");
        await _httpClient.PostAsync($"http://localhost:{restMappedPort}/v1/tenants/t1/schemas/write", requestContent);

        _service = new PermifyAuthorizationService(new PermifyOptions
        {
            Host = $"http://localhost:{grpcMappedPort}",
        });
    }

    [Test]
    [Parallelizable]
    [TestCase("organization", "admin", "user", "")]
    [TestCase("organization", "member", "user", "")]
    [TestCase("repository", "parent", "organization", "...")]
    [TestCase("repository", "owner", "user", "")]
    public async Task CreateRelationship(string entityType, string relation, string subjectType, string subjectRelation)
    {
        // Arrange

        // Act
        _ = await _service.CreateRelationship(
            new Entity("1", entityType),
            relation,
            new Subject("1", subjectType, subjectRelation)
            );

        var foundSubjects = await _service.ReadRelationship(
            new Entity("1", entityType), relation
            );

        // Assert
        Assert.AreEqual(1, foundSubjects.Count());
        Assert.AreEqual(subjectType, foundSubjects.First().Type);
    }

    [Test]
    public async Task Check()
    {
        // Arrange
        await _service.CreateRelationship(new Entity("10", "organization"), "admin", new Subject("20", "user"));
        await _service.CreateRelationship(new Entity("10", "organization"), "member", new Subject("21", "user"));
        await _service.CreateRelationship(new Entity("10", "organization"), "member", new Subject("22", "user"));
        await _service.CreateRelationship(new Entity("30", "repository"), "parent", new Subject("10", "organization", "..."));
        await _service.CreateRelationship(new Entity("30", "repository"), "owner", new Subject("21", "user"));

        // Act
        bool adminCanPush = await _service.Can(new Subject("20", "user"), "push", new Entity("30", "repository"));
        bool ownerCanPush = await _service.Can(new Subject("21", "user"), "push", new Entity("30", "repository"));
        bool memberCanPush = await _service.Can(new Subject("22", "user"), "push", new Entity("30", "repository"));

        bool adminCanRead = await _service.Can(new Subject("20", "user"), "read", new Entity("30", "repository"));
        bool ownerCanRead = await _service.Can(new Subject("21", "user"), "read", new Entity("30", "repository"));
        bool memberCanRead = await _service.Can(new Subject("22", "user"), "read", new Entity("30", "repository"));

        bool adminCanDelete = await _service.Can(new Subject("20", "user"), "delete", new Entity("30", "repository"));
        bool ownerCanDelete = await _service.Can(new Subject("21", "user"), "delete", new Entity("30", "repository"));
        bool memberCanDelete = await _service.Can(new Subject("22", "user"), "delete", new Entity("30", "repository"));

        // Assert
        Assert.IsFalse(adminCanPush);
        Assert.IsTrue(ownerCanPush);
        Assert.IsFalse(memberCanPush);

        Assert.IsTrue(adminCanRead);
        Assert.IsTrue(ownerCanRead);
        Assert.IsTrue(memberCanRead);

        Assert.IsTrue(adminCanDelete);
        Assert.IsTrue(ownerCanDelete);
        Assert.IsFalse(memberCanDelete);
    }

    [Test]
    public async Task SchemaLookup_RepositoryParentAdmin()
    {
        // Arrange

        // Act
        var actions = await _service.SchemaLookup("repository", new[] { "parent.admin" });

        // Assert
        Assert.AreEqual(2, actions.Count());
        Assert.IsTrue(actions.Where(a => a == "delete").Count() == 1);
        Assert.IsTrue(actions.Where(a => a == "read").Count() == 1);
    }

    [Test]
    public async Task SchemaLookup_RepositoryOwner()
    {
        // Arrange

        // Act
        var actions = await _service.SchemaLookup("repository", new[] { "owner" });

        // Assert
        Assert.AreEqual(3, actions.Count());
        Assert.IsTrue(actions.Where(a => a == "push").Count() == 1);
        Assert.IsTrue(actions.Where(a => a == "delete").Count() == 1);
        Assert.IsTrue(actions.Where(a => a == "read").Count() == 1);
    }

    [Test]
    public async Task DeleteRelationship()
    {
        // Arrange
        await _service.CreateRelationship(new Entity("30", "repository"), "owner", new Subject("21", "user"));

        // Act
        bool ownerCanPushBefore = await _service.Can(new Subject("21", "user"), "push", new Entity("30", "repository"));
        _ = await _service.DeleteRelationship(new Entity("30", "repository"), "owner", new Subject("21", "user"));
        bool ownerCanPushAfter = await _service.Can(new Subject("21", "user"), "push", new Entity("30", "repository"));

        // Assert
        Assert.IsTrue(ownerCanPushBefore);
        Assert.IsFalse(ownerCanPushAfter);
    }

    [Test]
    public async Task ReadRelationship()
    {
        // Arrange
        await _service.CreateRelationship(new Entity("10", "organization"), "member", new Subject("20", "user"));
        await _service.CreateRelationship(new Entity("10", "organization"), "member", new Subject("21", "user"));

        // Act
        var subjects = await _service.ReadRelationship(new Entity("10", "organization"), "member");

        // Assert
        Assert.AreEqual(2, subjects.Count());
        Assert.IsTrue(subjects.Where(s => s.Id == "20").Count() == 1);
        Assert.IsTrue(subjects.Where(s => s.Id == "21").Count() == 1);
    }
}
