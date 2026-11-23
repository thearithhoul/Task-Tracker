# Task Tracker

A simple command-line task tracker written in C# (.NET 10).

## Project From [roadmap.sh](https://roadmap.sh/projects/task-tracker)

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Installation

```bash
git clone https://github.com/thearithhoul/Task-Tracker.git
cd Task-Tracker
dotnet pack src/TaskTracker -c Release -o nupkg
dotnet tool install -g task-cli --add-source ./nupkg
```

Open a new terminal and check it works:

```bash
task-cli --help
```

To update after pulling changes, run `dotnet pack` again and then `dotnet tool update -g task-cli --add-source ./nupkg`. To remove it, run `dotnet tool uninstall -g task-cli`.

## Example

# Adding a new task
task-cli add "Buy groceries"
# Output: Task added successfully (ID: 1)

# Updating and deleting tasks
task-cli update 1 "Buy groceries and cook dinner"
task-cli delete 1

# Marking a task as in progress or done
task-cli mark-in-progress 1
task-cli mark-done 1

# Listing all tasks
task-cli list

# Listing tasks by status
task-cli list done
task-cli list todo
task-cli list in-progress


## License

Released under the license in [LICENSE](LICENSE).
