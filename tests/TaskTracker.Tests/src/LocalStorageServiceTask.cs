using TaskTracker.Models;
using TaskTracker.Services;

namespace TaskTracker.Test;

public class LocalStorageServiceTest : IDisposable
{
    private readonly string _folder;
    private readonly string _file;
    private readonly LocalStorageService _service;

    public LocalStorageServiceTest()
    {
        // Each test gets its own temp folder so the real %APPDATA% data is never touched.
        _folder = Path.Combine(Path.GetTempPath(), "TaskTrackerTests", Guid.NewGuid().ToString("N"));
        _file = Path.Combine(_folder, "tasks.json");
        _service = new LocalStorageService(_file);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder))
            Directory.Delete(_folder, recursive: true);
    }

    [Fact]
    public void Load_FileMissing_ReturnsEmptyList()
    {
        var tasks = _service.Load();

        Assert.Empty(tasks);
    }

    [Fact]
    public void Save_CreatesFolderAndFile()
    {
        bool saved = _service.Save([new TaskItem { Id = 1, Description = "Buy milk" }]);

        Assert.True(saved);
        Assert.True(File.Exists(_file));
    }

    [Fact]
    public void Save_ThenLoad_ReturnsSameTasks()
    {
        var created = new DateTime(2026, 1, 2, 3, 4, 5);
        var updated = new DateTime(2026, 2, 3, 4, 5, 6);
        List<TaskItem> original =
        [
            new() { Id = 1, Description = "Buy milk", Status = 1, CreatedAt = created },
            new() { Id = 2, Description = "dfadgagafdgas adgagg gaha", Status = 2, CreatedAt = created, UpdatedAt = updated },
        ];

        _service.Save(original);
        var loaded = _service.Load();

        Assert.Equal(2, loaded.Count);

        Assert.Equal(1, loaded[0].Id);
        Assert.Equal("Buy milk", loaded[0].Description);
        Assert.Equal(1, loaded[0].Status);
        Assert.Equal(created, loaded[0].CreatedAt);
        Assert.Null(loaded[0].UpdatedAt);

        Assert.Equal(2, loaded[1].Id);
        Assert.Equal("dfadgagafdgas adgagg gaha", loaded[1].Description);
        Assert.Equal(2, loaded[1].Status);
        Assert.Equal(updated, loaded[1].UpdatedAt);
    }

    [Fact]
    public void Save_EmptyList_ThenLoad_ReturnsEmptyList()
    {
        _service.Save([new TaskItem { Id = 1, Description = "x" }]);
        _service.Save([]);

        Assert.Empty(_service.Load());
    }

    [Fact]
    public void Save_OverwritesPreviousContent()
    {
        _service.Save([new TaskItem { Id = 1, Description = "old" }]);
        _service.Save([new TaskItem { Id = 2, Description = "new" }]);

        var loaded = _service.Load();

        var task = Assert.Single(loaded);
        Assert.Equal(2, task.Id);
        Assert.Equal("new", task.Description);
    }

    [Fact]
    public void Save_PreservesSpecialCharacters()
    {
        const string text = "Quote \" backslash \\ unicode é中 newline\nend";

        _service.Save([new TaskItem { Id = 1, Description = text }]);

        Assert.Equal(text, _service.Load()[0].Description);
    }

    [Fact]
    public void Load_JsonNull_ReturnsEmptyList()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(_file, "null");

        Assert.Empty(_service.Load());
    }

    [Fact]
    public void Load_InvalidJson_Throws()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(_file, "{ not valid json");

        Assert.ThrowsAny<System.Text.Json.JsonException>(() => _service.Load());
    }

    [Fact]
    public void Save_PathIsUnwritable_ReturnsFalse()
    {
        // A file where the folder should be makes Directory.CreateDirectory fail.
        Directory.CreateDirectory(_folder);
        string blocker = Path.Combine(_folder, "blocker");
        File.WriteAllText(blocker, "i am a file");
        var service = new LocalStorageService(Path.Combine(blocker, "tasks.json"));

        bool saved = service.Save([new TaskItem { Id = 1 }]);

        Assert.False(saved);
    }
}
