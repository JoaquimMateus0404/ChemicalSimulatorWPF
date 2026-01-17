using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// Classe base para todos os ViewModels, implementando INotifyPropertyChanged
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifica que uma propriedade foi alterada
        /// </summary>
        /// <param name="propertyName">Nome da propriedade (preenchido automaticamente)</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Define o valor de um campo e notifica se houver mudança
        /// </summary>
        /// <typeparam name="T">Tipo do campo</typeparam>
        /// <param name="field">Referência ao campo</param>
        /// <param name="value">Novo valor</param>
        /// <param name="propertyName">Nome da propriedade</param>
        /// <returns>True se o valor foi alterado</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
