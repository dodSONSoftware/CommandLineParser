using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorAndArchiveTool
{
    /// <summary>
    /// A simple class for all <see cref="System.Windows.Input.ICommand"/> needs.
    /// </summary>
    public class DelegateCommand
        : System.Windows.Input.ICommand
    {
        #region Ctor
        /// <summary>
        /// Instantiates a new <see cref="DelegateCommand"/> with the provided <see cref="Action{T1}"/> and <see cref="Predicate{T}"/>.
        /// </summary>
        /// <param name="execute">The <see cref="Action{T1}"/> to invoke when this command is executed.</param>
        /// <param name="canExecute">The <see cref="Predicate{T}"/> to invoke when this command needs to check if the <paramref name="execute"/> <see cref="Action{T1}"/> can be invoked.</param>
        public DelegateCommand(Action<object> execute,
                            Predicate<object> canExecute)
        {
            _Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _CanExecute = canExecute;
        }
        /// <summary>
        /// Instantiates a new <see cref="DelegateCommand"/> with the provided <see cref="Action{T1}"/>.
        /// </summary>
        /// <param name="execute">The <see cref="Action{T1}"/> to invoke when this command is executed.</param>
        public DelegateCommand(Action<object> execute)
            : this(execute, null) { }
        #endregion
        #region Private Fields
        private readonly Action<object> _Execute;
        private readonly Predicate<object> _CanExecute;
        #endregion
        #region ICommand Methods
        /// <summary>
        /// Occurs when the executability of this command has changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;
        /// <summary>
        /// Sends notification that the executability of this command has changed.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Returns whether this command can execute or not.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns><b>True</b> if this command can be executed; otherwise, <b>false</b>.</returns>
        public bool CanExecute(object parameter)
        {
            if (_CanExecute == null)
            {
                return true;
            }
            return _CanExecute(parameter);
        }
        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object parameter) => _Execute(parameter);
        #endregion
    }
}
