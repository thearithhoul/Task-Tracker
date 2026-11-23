
using Microsoft.VisualBasic;
using TaskTracker.Models;
using TaskTracker.Services;

namespace TaskTracker.Commands;
 
 

public class TaskCommand
{
    private readonly LocalStorageService _storage;

    private readonly List<string> status = ["done", "in-progress"];

    public TaskCommand(LocalStorageService? storage = null)
    {
        _storage = storage ?? new LocalStorageService();
    }

    public void Add(string[] args)
    {
        string? name = null;

        if(args.Length > 0)
        {
            name = args[0];
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.Error.WriteLine("Usage: add \"<name>\"");
            return;
        }

        
    

        List<TaskItem> taskItems=  _storage.Load();

        TaskItem newItem = new TaskItem()
        {
            Id = taskItems.Count + 1,
            Description = name,
            Status = 0,
        };
        taskItems.Add(newItem);
        
        var succ =  _storage.Save(taskItems);

        if (succ)
        {
            Console.WriteLine($"Task added successfully  (ID: {newItem.Id})");
        }
        else
        {
            Console.Error.WriteLine("Failed to add.");
        }
    }


    public void Update(string[] args)
    {
        if(args.Length != 2)
        {
            Console.Error.WriteLine("Usage: update <ID> \"<name>\"");
            return;
        }

        if(!int.TryParse(args[0], out int id))
        {
            Console.Error.WriteLine($"Invalid ID '{args[0]}'. ID must be a number");    
            return;
        }

        var list =  _storage.Load();

        var task = list.FirstOrDefault(t=> t.Id == id);

        if( task is null)
        {  
            Console.Error.WriteLine($"Can't Find '{args[0]}'.");    
            return ;
        }

        task.Description = args[1];
        task.UpdatedAt = DateTime.Now;

        if (_storage.Save(list))
        {
            Console.WriteLine($"Task {id} updated successfully");
        }
        else
        {
            Console.Error.WriteLine("Failed to updated.");
        }

    }

    public void Delete(string[] args)
    {
        if(args.Length != 1)
        {
            Console.Error.WriteLine("Usage: delete <ID>");
            return;
        }

        
        if(!int.TryParse(args[0], out int id))
        {
            Console.Error.WriteLine($"Invalid ID '{args[0]}'. ID must be a number");    
            return;
        }
        
        var list = _storage.Load();


        var task = list.FirstOrDefault(t=> t.Id == id);

        if(task is null)
        {  
            Console.Error.WriteLine($"Can't Find '{args[0]}'.");    
            return ;
        }

        list.Remove(task);

        
        if (_storage.Save(list))
        {
            Console.WriteLine($"Task {id} delete successfully");
        }
        else
        {
            Console.Error.WriteLine("Failed to delete.");
        }

    } 

    /// <summary>Sets the status of a task. status: 0 = in-progress, 1 = done.</summary>
    public void MarkStatus(int status, string[] args)
    {
        string label = StatusLabel(status);

        if (args.Length != 1)
        {
            Console.Error.WriteLine($"Usage: mark-{label} <ID>");
            return;
        }

        if (!int.TryParse(args[0], out int id))
        {
            Console.Error.WriteLine($"Invalid ID '{args[0]}'. ID must be a number");
            return;
        }

        var list = _storage.Load();
        var task = list.FirstOrDefault(t => t.Id == id);

        if (task is null)
        {
            Console.Error.WriteLine($"Can't Find '{args[0]}'.");
            return;
        }

        task.Status = status;
        task.UpdatedAt = DateTime.Now;

        if (_storage.Save(list))
        {
            Console.WriteLine($"Task {id} marked as {label}");
        }
        else
        {
            Console.Error.WriteLine("Failed to update status.");
        }
    }

    public void List(string[] args)
    {

        if(args.Length != 1)
        {
            Console.Error.WriteLine("Usage: delete <ID>");
            return;
        }
        
        List<TaskItem> taskItems = [];
        switch (args[0])
        {
            case "done":
                taskItems= filterTask(1, false);
                break;
            case "todo":
                 taskItems = filterTask(null, true);
                break;
            case "in-progress":
                taskItems=  filterTask(0, false);
                break;
            default:
                Console.Error.WriteLine("Usage: delete <ID>");
                return;
        }

        Console.WriteLine("=========== Task ==========");

        if (taskItems.Count == 0)
        {
            Console.WriteLine("No tasks found.");
        }

        foreach (var task in taskItems)
        {
            string updated = task.UpdatedAt is { } u ? $"{u:yyyy-MM-dd HH:mm}" : "-";

            Console.WriteLine($"ID:          {task.Id}");
            Console.WriteLine($"Description: {task.Description}");
            Console.WriteLine($"Status:      {StatusLabel(task.Status)}");
            Console.WriteLine($"Created:     {task.CreatedAt:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"Updated:     {updated}");
            Console.WriteLine("-----------------------------");
        }

        Console.WriteLine("=========== End ==========");
    }

    private static string StatusLabel(int status) => status switch
    {
        1 => "done",
        0 => "in-progress",
        _ => "todo",
    };

    private List<TaskItem> filterTask(int? status, bool all)
    {
        var list = _storage.Load();

        if(list.Count <0) return [];

        if(all) return list;
        
        return list.Where(t=> t.Status == status).ToList();
    }
}

