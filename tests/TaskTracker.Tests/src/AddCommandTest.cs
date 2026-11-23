using TaskTracker.Commands;
using TaskTracker.Models;
using TaskTracker.Services;

namespace TaskTracker.Test;

[Collection(ConsoleCollection.Name)]
public class AddCommandTest : IDisposable
{
    private readonly string _folder;
    private readonly LocalStorageService _storage;
    private readonly TaskCommand _command;

    public AddCommandTest()
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

    [Theory]
    [InlineData("\"Buy milk\"")]
    [InlineData("\"dfadgagafdgas adgagg gaha\"")]
    public void Add_NewTask_Valid_Args(string name)
    {
        var (stdout, stderr) = Run(name);

        Assert.Contains("Task added successfully", stdout);
        Assert.Contains("(ID: 1)", stdout);
        Assert.Equal("", stderr);
    }

    [Fact]
    public void Add_NewTask_AppearsInList()
    {
        Run("\"Buy milk\"");

        var tasks = _storage.Load();

        var task = Assert.Single(tasks);
        Assert.Equal(1, task.Id);
        Assert.Equal("\"Buy milk\"", task.Description);
        Assert.Equal(0, task.Status);
    }

    [Fact]
    public void Add_MultipleTasks_GetIncrementingIds()
    {
        var first = Run("\"First\"").Stdout;
        var second = Run("\"Second\"").Stdout;
        var third = Run("\"Third\"").Stdout;

        Assert.Contains("(ID: 1)", first);
        Assert.Contains("(ID: 2)", second);
        Assert.Contains("(ID: 3)", third);
        Assert.Equal([1, 2, 3], _storage.Load().Select(t => t.Id));
    }

    [Fact]
    public void Add_KeepsExistingTasks()
    {
        _storage.Save([new TaskItem { Id = 1, Description = "existing" }]);

        var (stdout, _) = Run("\"new one\"");

        Assert.Contains("(ID: 2)", stdout);
        var tasks = _storage.Load();
        Assert.Equal(2, tasks.Count);
        Assert.Equal("existing", tasks[0].Description);
        Assert.Equal("\"new one\"", tasks[1].Description);
    }

    [Fact]
    public void Add_SaveFails_PrintsFailedToAdd()
    {
        // A file where the storage folder should be makes Save fail.
        Directory.CreateDirectory(_folder);
        string blocker = Path.Combine(_folder, "blocker");
        File.WriteAllText(blocker, "i am a file");
        var command = new TaskCommand(new LocalStorageService(Path.Combine(blocker, "tasks.json")));

        var originalOut = Console.Out;
        var originalErr = Console.Error;
        using var outWriter = new StringWriter();
        using var errWriter = new StringWriter();
        try
        {
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            command.Add(["\"Buy milk\""]);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }

        Assert.DoesNotContain("successfully", outWriter.ToString());
        Assert.Contains("Failed to add", errWriter.ToString());
    }

    [Theory]
    [InlineData]                    // no args
    [InlineData("")]                // empty string
    [InlineData("   ")]             // whitespace only
    [InlineData("Buy milk")]        // no quotes (current rule requires them)
    public void Add_NewTask_InValid_Args(params string[] args)
    {
        var (stdout, stderr) = Run(args);

        Assert.Equal("", stdout);
        Assert.Contains("Usage: add", stderr);
        Assert.Empty(_storage.Load());      // nothing saved
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

            _command.Add(args);
            return (outWriter.ToString(), errWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }
    }
}
