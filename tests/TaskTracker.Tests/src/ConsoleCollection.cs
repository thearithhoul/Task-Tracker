namespace TaskTracker.Test;

/// <summary>
/// Console.SetOut / SetError are process-wide, so test classes that capture console
/// output must not run in parallel with each other. Put such classes in this collection.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public class ConsoleCollection
{
    public const string Name = "Console";
}
