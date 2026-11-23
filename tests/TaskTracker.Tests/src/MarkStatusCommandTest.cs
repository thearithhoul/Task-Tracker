using TaskTracker.Commands;
using TaskTracker.Models;
using TaskTracker.Services;

namespace TaskTracker.Test;

[Collection(ConsoleCollection.Name)]
public class MarkStatusCommandTest : IDisposable
{
    private const int InProgress = 0;
    private const int Done = 1;

    private readonly string _folder;
    private readonly LocalStorageService _storage;
    private readonly TaskCommand _command;

    public MarkStatusCommandTest()
    {
        _folder = Path.Combine(Path.GetTempPath(), "TaskTrackerTests", Guid.NewGuid().ToString("N"));
        _storage = new LocalStorageService(Path.Combine(_folder, "tasks.json"));
        _command = new TaskCommand(_storage);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder))
            Directory.Delete(_folder, recursive: true);
    }

    private void Seed() => _storage.Save(
    [
        new TaskItem { Id = 1, Description = "first", Status = InProgress },
        new TaskItem { Id = 2, Description = "second", Status = InProgress },
        new TaskItem { Id = 5, Description = "fifth", Status = Done },
    ]);

    [Fact]
    public void MarkDone_ChangesStatusAndPrintsMessage()
    {
        Seed();

        var (stdout, stderr) = Run(Done, "2");

        Assert.Equal(Done, _storage.Load().Single(t => t.Id == 2).Status);
        Assert.Contains("Task 2 marked as done", stdout);
        Assert.Equal("", stderr);
    }

    [Fact]
    public void MarkInProgress_ChangesStatusAndPrintsMessage()
    {
        Seed();

        var (stdout, stderr) = Run(InProgress, "5");

        Assert.Equal(InProgress, _storage.Load().Single(t => t.Id == 5).Status);
        Assert.Contains("Task 5 marked as in-progress", stdout);
        Assert.Equal("", stderr);
    }

    [Fact]
    public void Mark_SetsUpdatedAt()
    {
        Seed();
        var before = DateTime.Now;

        Run(Done, "1");

        var task = _storage.Load().Single(t => t.Id == 1);
        Assert.NotNull(task.UpdatedAt);
        Assert.InRange(task.UpdatedAt!.Value, before.AddSeconds(-1), DateTime.Now.AddSeconds(1));
    }

    [Fact]
    public void Mark_OnlyChangesTargetTask()
    {
        Seed();

        Run(Done, "1");

        var tasks = _storage.Load();
        Assert.Equal(3, tasks.Count);
        Assert.Equal(InProgress, tasks.Single(t => t.Id == 2).Status);
        Assert.Null(tasks.Single(t => t.Id == 2).UpdatedAt);
        Assert.Equal("first", tasks.Single(t => t.Id == 1).Description);   // description untouched
    }

    [Fact]
    public void Mark_UnknownId_PrintsErrorAndChangesNothing()
    {
        Seed();

        var (stdout, stderr) = Run(Done, "3");

        Assert.Equal("", stdout);
        Assert.Contains("Can't Find '3'", stderr);
        Assert.Equal(2, _storage.Load().Count(t => t.Status == InProgress));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("")]
    public void Mark_NonNumericId_PrintsInvalidId(string id)
    {
        Seed();

        var (stdout, stderr) = Run(Done, id);

        Assert.Equal("", stdout);
        Assert.Contains("Invalid ID", stderr);
    }

    [Theory]
    [InlineData(Done, "mark-done")]
    [InlineData(InProgress, "mark-in-progress")]
    public void Mark_NoArgs_PrintsUsageForThatCommand(int status, string expectedCommand)
    {
        var (stdout, stderr) = Run(status);

        Assert.Equal("", stdout);
        Assert.Contains($"Usage: {expectedCommand} <ID>", stderr);
    }

    [Fact]
    public void Mark_TooManyArgs_PrintsUsage()
    {
        Seed();

        var (stdout, stderr) = Run(Done, "1", "2");

        Assert.Equal("", stdout);
        Assert.Contains("Usage: mark-done <ID>", stderr);
        Assert.Equal(2, _storage.Load().Count(t => t.Status == InProgress));
    }

    private (string Stdout, string Stderr) Run(int status, params string[] args)
    {
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        using var outWriter = new StringWriter();
        using var errWriter = new StringWriter();

        try
        {
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            _command.MarkStatus(status, args);
            return (outWriter.ToString(), errWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
