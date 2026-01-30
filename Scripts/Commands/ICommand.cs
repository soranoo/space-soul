/// <summary>
/// Contract for all player commands following the Command pattern.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Execute the command action.
    /// </summary>
    void Execute();

    /// <summary>
    /// Check if the command can be executed.
    /// </summary>
    bool CanExecute();
}
