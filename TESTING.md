# Unit Testing Guide

## Overview

This project includes comprehensive unit tests for all service implementations using xUnit, Moq, and InMemory Database.

## Test Projects

### 1. **Bookmark.Service.Test**
Tests for `BookmarkService` implementation
- ✅ Create bookmark with validation
- ✅ Delete bookmark with SignalR notification
- ✅ Get bookmarks with filtering
- ✅ Handle notification failures gracefully

### 2. **Document.Service.Test**
Tests for `DocumentService` implementation
- ✅ Create document with file upload
- ✅ Update document metadata and files
- ✅ Delete document with cleanup
- ✅ File format validation
- ✅ SignalR notifications

### 3. **Document.Pages.Service.Test**
Tests for `DocumentPageService` implementation
- ✅ Get pages by document
- ✅ Update page content
- ✅ Create and delete pages
- ✅ Get pages with bookmarks
- ✅ Real-time notifications

### 4. **Notification.Service.Test** ⭐ NEW
Tests for `NotificationService` implementation
- ✅ Global notifications
- ✅ Document group notifications
- ✅ User-specific notifications
- ✅ Document events (Added/Updated/Deleted)
- ✅ Bookmark events (Created/Deleted)
- ✅ Page edit notifications
- ✅ Error handling

## Technologies Used

- **xUnit** - Test framework
- **Moq** - Mocking framework
- **InMemory Database** - EF Core in-memory provider for database testing
- **FluentAssertions** (optional) - For more readable assertions

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test Bookmark.Service.Test/
dotnet test Document.Service.Test/
dotnet test Document.Pages.Service.Test/
dotnet test Notification.Service.Test/
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run Tests in Verbose Mode
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~CreateBookmarkAsync_Should_Succeed_When_Valid"
```

## Test Structure

### Arrange-Act-Assert Pattern
All tests follow the AAA pattern:

```csharp
[Fact]
public async Task MethodName_Should_ExpectedBehavior()
{
    // Arrange - Setup test data and mocks
    var mockRepo = new Mock<IRepository>();
    mockRepo.Setup(r => r.GetAsync(1)).ReturnsAsync(testData);

    // Act - Execute the method being tested
    var result = await service.MethodName(1);

    // Assert - Verify the results
    Assert.NotNull(result);
    mockRepo.Verify(r => r.GetAsync(1), Times.Once);
}
```

## Key Test Scenarios

### 1. Success Paths
- Valid operations complete successfully
- Data is saved correctly
- Notifications are sent

### 2. Error Handling
- Null/invalid input validation
- Entity not found scenarios
- SignalR notification failures

### 3. Edge Cases
- Empty collections
- Boundary values
- Concurrent operations

### 4. Integration Points
- Repository calls are made correctly
- Context SaveChanges is called
- Notifications are sent to SignalR

## Mocking Strategy

### Services Mock
```csharp
var notificationServiceMock = new Mock<INotificationService>();
notificationServiceMock
    .Setup(n => n.NotifyDocumentDeletedAsync(It.IsAny<string>()))
    .Returns(Task.CompletedTask);
```

### Repository Mock
```csharp
var repoMock = new Mock<IDocumentRepository>();
repoMock.Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(new Document { Id = 1 });
```

### SignalR Hub Mock
```csharp
var hubContextMock = new Mock<IHubContext<NotificationHub>>();
var clientsMock = new Mock<IHubClients>();
var clientProxyMock = new Mock<IClientProxy>();

hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);
clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
```

## Test Coverage Goals

- **Lines**: > 80%
- **Branches**: > 70%
- **Methods**: > 90%

## Best Practices

1. **One Assert Per Test** (when possible)
2. **Clear Test Names** - `MethodName_Should_ExpectedBehavior_When_Condition`
3. **Independent Tests** - Each test should run independently
4. **Use InMemory DB** - For integration-style tests
5. **Mock External Dependencies** - SignalR, File System, etc.
6. **Test Both Success and Failure** paths
7. **Verify Side Effects** - Check that notifications are sent, repos are called

## Common Test Patterns

### Testing Async Methods
```csharp
[Fact]
public async Task TestAsync()
{
    var result = await service.MethodAsync();
    Assert.NotNull(result);
}
```

### Testing Exceptions
```csharp
[Fact]
public async Task Should_Throw_When_Invalid()
{
    var ex = await Assert.ThrowsAsync<ArgumentException>(
        () => service.Method(invalidInput)
    );
    Assert.Equal("Expected message", ex.Message);
}
```

### Testing with Theory
```csharp
[Theory]
[InlineData("test1")]
[InlineData("test2")]
[InlineData("test3")]
public async Task Should_Work_With_Various_Inputs(string input)
{
    var result = await service.Method(input);
    Assert.NotNull(result);
}
```

## Continuous Integration

Tests should be run automatically on:
- Every commit
- Pull requests
- Before deployment

## Troubleshooting

### InMemory Database Issues
- Each test uses a unique database name (Guid)
- Dispose context after tests
- Reset data between tests

### Moq Verification Failures
- Check method signatures match
- Verify setup expressions are correct
- Use `It.IsAny<T>()` for flexible matching

### SignalR Hub Testing
- Mock `IHubContext`, `IHubClients`, and `IClientProxy`
- Verify `SendCoreAsync` is called with correct parameters

## Future Improvements

- [ ] Add integration tests
- [ ] Add performance tests
- [ ] Add load tests for SignalR
- [ ] Increase code coverage to 90%+
- [ ] Add mutation testing

## References

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [EF Core Testing](https://docs.microsoft.com/en-us/ef/core/testing/)
