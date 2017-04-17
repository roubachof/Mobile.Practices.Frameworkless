// ********************************************************************************************************************
// <author>Stephen Cleary</author>
// <date>08-2016</date>
// <version>v1.0.0-eta-02</version>
// <web>https://github.com/StephenCleary/Mvvm.Async/blob/master/src/Nito.Mvvm.Async/AsyncCommand.cs</web>
// ********************************************************************************************************************

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Nito.Mvvm;

namespace Mobile.Practices.Frameworkless.Core.Nito
{
    /// <summary>
    /// An async version of <see cref="ICommand"/>.
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// Executes the asynchronous command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        Task ExecuteAsync(object parameter);
    }

    /// <summary>
    /// An async command that implements <see cref="ICommand"/>, forwarding <see cref="ICommand.Execute(object)"/> to <see cref="IAsyncCommand.ExecuteAsync(object)"/>.
    /// </summary>
    public abstract class AsyncCommandBase : IAsyncCommand
    {
        /// <summary>
        /// The local implementation of <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        private readonly WeakCanExecuteChanged _canExecuteChanged;

        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecute(object)"/>. May be <c>null</c>.
        /// </summary>
        protected readonly Func<object, bool> _canExecute;

        /// <summary>
        /// The cancel command.
        /// </summary>
        protected readonly CancelAsyncCommand _cancelCommand;

        /// <summary>
        /// Base constructor for asynchronous command.
        /// </summary>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        protected AsyncCommandBase(Func<object, bool> canExecute = null)
        {
            _cancelCommand = new CancelAsyncCommand();
            _canExecute = canExecute;
            _canExecuteChanged = new WeakCanExecuteChanged(this);
        }

        public abstract bool IsExecuting { get; }

        public ICommand CancelCommand => _cancelCommand;

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public abstract Task ExecuteAsync(object parameter);

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/>. Call this if you supply a delegate for the <see cref="ICommand.CanExecute(object)"/> implementation.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            _canExecuteChanged.OnCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged
        {
            add { _canExecuteChanged.CanExecuteChanged += value; }
            remove { _canExecuteChanged.CanExecuteChanged -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        async void ICommand.Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecute(object)"/>. If a <c>canExecute</c> delegate was passed to the constructor, then that delegate is invoked; otherwise, returns <c>false</c> whenever the async command is in progress.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public virtual bool CanExecute(object parameter = null)
        {
            if (_canExecute == null)
                return !IsExecuting;
            return !IsExecuting && _canExecute(parameter);
        }

        public void Dispose()
        {
            _cancelCommand?.Dispose();
        }

        protected sealed class CancelAsyncCommand : ICommand, IDisposable
        {
            private CancellationTokenSource cts = new CancellationTokenSource();
            private bool commandExecuting;

            public CancellationToken Token => this.cts.Token;

            public void NotifyCommandStarting()
            {
                this.commandExecuting = true;
                if (!this.cts.IsCancellationRequested)
                {
                    return;
                }

                this.cts = new CancellationTokenSource();
                this.RaiseCanExecuteChanged();
            }

            public void NotifyCommandFinished()
            {
                this.commandExecuting = false;
                this.RaiseCanExecuteChanged();
            }

            bool ICommand.CanExecute(object parameter)
            {
                return this.commandExecuting && !this.cts.IsCancellationRequested;
            }

            void ICommand.Execute(object parameter)
            {
                this.cts.Cancel();
                this.RaiseCanExecuteChanged();
            }

            public event EventHandler CanExecuteChanged;

            private void RaiseCanExecuteChanged()
            {
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Dispose()
            {
                this.cts?.Dispose();
            }
        }
    }
}