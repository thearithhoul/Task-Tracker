using TaskTracker.Commands;
using TaskTracker.Models;
using TaskTracker.Services;

namespace TaskTracker.Test;

[Collection(ConsoleCollection.Name)]
public class UpdateCommandTest : IDisposable
{
    private readonly string _folder;
    private readonly LocalStorageService _storage;
    private readonly TaskCommand _command;

    public UpdateCommandTest()
    {
        // Isolated temp storage so the real %APPDATA% tasks.json is never touched.
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
        new TaskItem { Id = 1, Description = "first", Status = 0 },
        new TaskItem { Id = 2, Description = "second", Status = 1 },
        new TaskItem { Id = 5, Description = "fifth (gap in ids)", Status = 2 },
    ]);

    [Fact]
    public void Update_ExistingTask_ChangesDescription()
    {
        Seed();

        var (stdout, stderr) = Run("2", "updated text");

        var task = _storage.Load().Single(t => t.Id == 2);
        Assert.Equal("updated text", task.Description);
        Assert.Contains("Task 2 updated successfully", stdout);
        Assert.Equal("", stderr);
    }

    [Fact]
    public void Update_SetsUpdatedAt()
    {
        Seed();
        var before = DateTime.Now;

        Run("1", "new");

        var task = _storage.Load().Single(t => t.Id == 1);
        Assert.NotNull(task.UpdatedAt);
        Assert.InRange(task.UpdatedAt!.Value, before.AddSeconds(-1), DateTime.Now.AddSeconds(1));
    }

    [Fact]
    public void Update_OnlyChangesTargetTask()
    {
        Seed();

        Run("2", "updated text");

        var tasks = _storage.Load();
        Assert.Equal(3, tasks.Count);

        var first = tasks.Single(t => t.Id == 1);
        Assert.Equal("first", first.Description);
        Assert.Null(first.UpdatedAt);

        var second = tasks.Single(t => t.Id == 2);
        Assert.Equal(1, second.Status);                 // status untouched

        Assert.Equal("fifth (gap in ids)", tasks.Single(t => t.Id == 5).Description);
    }

    [Fact]
    public void Update_FindsTaskById_NotByPosition()
    {
        Seed();     // ids are 1, 2, 5 - id 5 is at index 2 and exceeds nothing about Count

        Run("5", "changed");

        Assert.Equal("changed", _storage.Load().Single(t => t.Id == 5).Description);
    }

    [Fact]
    public void Update_UnknownId_PrintsErrorAndChangesNothing()
    {
        Seed();

        var (stdout, stderr) = Run("3", "nope");     // 3 is within Count but no such id

        Assert.Equal("", stdout);
        Assert.Contains("Can't Find '3'", stderr);
        Assert.Equal(["first", "second", "fifth (gap in ids)"], _storage.Load().Select(t => t.Description));
    }

    [Fact]
    public void Update_EmptyStorage_PrintsNotFound()
    {
        var (stdout, stderr) = Run("1", "text");

        Assert.Equal("", stdout);
        Assert.Contains("Can't Find '1'", stderr);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("")]
    [InlineData("99999999999")]     // overflows int
    public void Update_NonNumericId_PrintsInvalidId(string id)
    {
        Seed();

        var (stdout, stderr) = Run(id, "text");

        Assert.Equal("", stdout);
        Assert.Contains("Invalid ID", stderr);
        Assert.Equal("first", _storage.Load().Single(t => t.Id == 1).Description);
    }

    [Theory]
    [InlineData]                            // no args
    [InlineData("1")]                       // missing name
    [InlineData("1", "a", "b")]             // too many args
    public void Update_WrongArgCount_PrintsUsage(params string[] args)
    {
        Seed();

        var (stdout, stderr) = Run(args);

        Assert.Equal("", stdout);
        Assert.Contains("Usage: update", stderr);
        Assert.Equal("first", _storage.Load().Single(t => t.Id == 1).Description);
    }

    [Fact]
    public void Update_SaveFails_PrintsFailedToSave()
    {
        // Load works (missing file -> empty list is not enough to find a task), so seed a real file first,
        // then make the file read-only so Save fails.
        Seed();
        string file = Path.Combine(_folder, "tasks.json");
        File.SetAttributes(file, FileAttributes.ReadOnly);

        try
        {
            var (stdout, stderr) = Run("1", "text");

            Assert.Equal("", stdout);
            Assert.Contains("Failed to", stderr);
        }
        finally
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }
    }

    private (string Stdout, string Stderr) Run(params string[] args)
    {
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        using var outWriter = new StringWriter();
        using var errWriter = new StringWriter();

        try
        {
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            _command.Update(args);
            return (outWriter.ToString(), errWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
