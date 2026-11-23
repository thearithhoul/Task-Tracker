using TaskTracker.Commands;

namespace TaskTracker;


public class Program
{
    public static int Main(string[] args)
    {

        if(args.Length ==0 || args[0] is "-h" or "--help")
        {
            PrintHelp();
            return 0;
        }

        var command = new TaskCommand();
        switch (args[0])
        {
            case "add":
                command.Add(args[1..]);
                return 1; 
            case "update":
                command.Update(args[1..]);        
                return 1;
            
            case "delete":
                command.Delete(args[1..]);
                return 1;

            case "list":
                command.List(args[1..]);
                return 1;

            case "mark-in-progress":
                command.MarkStatus(0, args[1..]);
                return 1;

            case "mark-done":
                command.MarkStatus(1, args[1..]);
                return 1;

            default:
                Console.Error.WriteLine($"Unknown command '{args[0]}'. Run 'task-cli --help'.");
                return 1;

        }


        
    }


    static void PrintHelp()
    {
        Console.WriteLine(
            """
            task-cli - a simple task tracker

            Usage: 
                task-cli [options] [command] [command-options] [arguments]

            options:
                -h|--help
            
            commands:
            add         Add a new task
            update      Update a task
            delete      Delete a task
            mark-in-progress   Mark a task as in progress
            mark-done   Mark a task as done
            list        List tasks
            """
        );
    }
}

