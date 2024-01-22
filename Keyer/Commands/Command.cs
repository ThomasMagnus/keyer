using System.Windows.Input;

namespace Keyer.Commands;

internal class Command(
    Action<object> execute,
    Func<object, bool> canExecute) : ICommand
{
    private readonly Action<object> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<object, bool> _canExecute = canExecute;

    public bool CanExecute(object? parameter) => _canExecute(parameter!);

    public void Execute(object? parameter)
    {
        execute(parameter!);
    }
    
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested += value;
    }
}