using System;
using System.Windows.Input;

namespace ChemicalSimulator.Commands
{
    /// <summary>
    /// Implementação de ICommand para comandos sem parâmetros
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        /// <summary>
        /// Força a reavaliação do CanExecute
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Implementação de ICommand para comandos com parâmetros
    /// </summary>
    /// <typeparam name="T">Tipo do parâmetro</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
                return true;
            
            // Se o parâmetro é null e T é um tipo nullable ou referência, permitir
            if (parameter == null)
            {
                // Se T é um tipo de valor não-nullable, retornar false
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                    return false;
                
                // Caso contrário, avaliar com null
                return _canExecute.Invoke(default(T)!);
            }
            
            // Se o parâmetro não é do tipo esperado, retornar false
            if (!(parameter is T))
                return false;
            
            return _canExecute.Invoke((T)parameter);
        }

        public void Execute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                return; // Não executar se o parâmetro é null para tipo de valor não-nullable
            
            _execute((T)parameter!);
        }

        /// <summary>
        /// Força a reavaliação do CanExecute
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
