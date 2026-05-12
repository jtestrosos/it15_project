using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using production_system.Data;
using production_system.Models;
using production_system.Services;
using Xunit;

namespace production_system.Tests.Services;

public class FacilityAccessServiceTests : IDisposable
{
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly ApplicationDbContext _dbContext;

    public FacilityAccessServiceTests()
    {
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();

        // Setup InMemory Database for ApplicationDbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;
        _dbContext = new ApplicationDbContext(options);

        // Setup ScopeFactory to return our InMemory DbContext
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(x => x.GetService(typeof(ApplicationDbContext)))
            .Returns(_dbContext);

        var mockScope = new Mock<IServiceScope>();
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
    }

    [Fact]
    public async Task GetCurrentUserFacilityIdAsync_UserNotAuthenticated_ReturnsNull()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())); // Unauthenticated
        _mockAuthStateProvider.Setup(a => a.GetAuthenticationStateAsync()).ReturnsAsync(authState);
        
        var service = new FacilityAccessService(_mockAuthStateProvider.Object, _mockScopeFactory.Object);

        // Act
        var result = await service.GetCurrentUserFacilityIdAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserFacilityIdAsync_UserAuthenticatedButNotInDb_ReturnsNull()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "test-user-id") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var authState = new AuthenticationState(new ClaimsPrincipal(identity));
        
        _mockAuthStateProvider.Setup(a => a.GetAuthenticationStateAsync()).ReturnsAsync(authState);
        
        var service = new FacilityAccessService(_mockAuthStateProvider.Object, _mockScopeFactory.Object);

        // Act
        var result = await service.GetCurrentUserFacilityIdAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserFacilityIdAsync_UserAuthenticatedAndInDb_ReturnsFacilityId()
    {
        // Arrange
        var userId = "test-user-123";
        var expectedFacilityId = 42;

        _dbContext.Users.Add(new ApplicationUser { Id = userId, FacilityID = expectedFacilityId });
        await _dbContext.SaveChangesAsync();

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var authState = new AuthenticationState(new ClaimsPrincipal(identity));
        
        _mockAuthStateProvider.Setup(a => a.GetAuthenticationStateAsync()).ReturnsAsync(authState);
        
        var service = new FacilityAccessService(_mockAuthStateProvider.Object, _mockScopeFactory.Object);

        // Act
        var result = await service.GetCurrentUserFacilityIdAsync();

        // Assert
        Assert.Equal(expectedFacilityId, result);
    }

    [Theory]
    [InlineData("Superadmin", true)]
    [InlineData("ProductionManager", false)]
    [InlineData("ProductionWorker", false)]
    public async Task IsSuperAdminAsync_ReturnsExpectedResultBasedOnRole(string role, bool expectedResult)
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Role, role) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var authState = new AuthenticationState(new ClaimsPrincipal(identity));
        
        _mockAuthStateProvider.Setup(a => a.GetAuthenticationStateAsync()).ReturnsAsync(authState);
        
        var service = new FacilityAccessService(_mockAuthStateProvider.Object, _mockScopeFactory.Object);

        // Act
        var result = await service.IsSuperAdminAsync();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task GetCurrentUserIdAsync_ReturnsUserIdFromClaims()
    {
        // Arrange
        var expectedUserId = "user-abc-123";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, expectedUserId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var authState = new AuthenticationState(new ClaimsPrincipal(identity));
        
        _mockAuthStateProvider.Setup(a => a.GetAuthenticationStateAsync()).ReturnsAsync(authState);
        
        var service = new FacilityAccessService(_mockAuthStateProvider.Object, _mockScopeFactory.Object);

        // Act
        var result = await service.GetCurrentUserIdAsync();

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
