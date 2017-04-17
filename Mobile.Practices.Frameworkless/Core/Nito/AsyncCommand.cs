// ********************************************************************************************************************
// <author>Stephen Cleary</author>
// <date>08-2016</date>
// <version>v1.0.0-eta-02</version>
// <web>https://github.com/StephenCleary/Mvvm.Async/blob/master/src/Nito.Mvvm.Async/AsyncCommand.cs</web>
// ********************************************************************************************************************

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Nito.Mvvm;

namespace Mobile.Practices.Frameworkless.Core.Nito
{
    /// <summary>
    /// A basic asynchronous command, which (by default) is disabled while the command is executing.
    /// </summary>
    public class AsyncCommand : AsyncCommandBase, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.
        /// </summary>
        private readonly Func<object, CancellationToken, Task> _executeAsync;

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        public AsyncCommand(Func<object, CancellationToken, Task> executeAsync, Func<object, bool> canExecute = null)
            : base(canExecute)
        {
            _executeAsync = executeAsync;
        }

        /// <summary>
        /// Represents the execution of the asynchronous command.
        /// </summary>
        public NotifyTask Execution { get; private set; }

        /// <summary>
        /// Whether the asynchronous command is currently executing.
        /// </summary>
        public override bool IsExecuting
        {
            get
            {
                if (Execution == null)
                    return false;
                return Execution.IsNotCompleted;
            }
        }

        /// <summary>
        /// Executes the asynchronous command. Any exceptions from the asynchronous delegate are captured and placed on <see cref="Execution"/>; they are not propagated to the UI loop.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public override async Task ExecuteAsync(object parameter = null)
        {
            _cancelCommand.NotifyCommandStarting();
            Execution = NotifyTaskBase.Create(_executeAsync(parameter, _cancelCommand.Token));

            base.OnCanExecuteChanged();
            var propertyChanged = PropertyChanged;
            propertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get("Execution"));
            propertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get("IsExecuting"));
            await Execution.TaskCompleted;
            _cancelCommand.NotifyCommandFinished();

            base.OnCanExecuteChanged();
            PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get("IsExecuting"));
            await Execution.Task;
        }

        /// <summary>
        /// Raised when any properties on this instance have changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// A basic asynchronous command, which (by default) is disabled while the command is executing.
    /// </summary>
    public class AsyncCommand<TResult> : AsyncCommandBase, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.
        /// </summary>
        private readonly Func<object, CancellationToken, Task<TResult>> _executeAsync;

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        public AsyncCommand(Func<object, CancellationToken, Task<TResult>> executeAsync, Func<object, bool> canExecute = null)
            : base(canExecute)
        {
            _executeAsync = executeAsync;
        }

        /// <summary>
        /// Represents the execution of the asynchronous command.
        /// </summary>
        public NotifyTask<TResult> Execution { get; private set; }

        /// <summary>
        /// Whether the asynchronous command is currently executing.
        /// </summary>
        public override bool IsExecuting
        {
            get
            {
                if (Execution == null)
                    return false;
                return Execution.IsNotCompleted;
            }
        }

        /// <summary>
        /// Executes the asynchronous command. Any exceptions from the asynchronous delegate are captured and placed on <see cref="Execution"/>; they are not propagated to the UI loop.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public override async Task ExecuteAsync(object parameter = null)
        {
            await ExecuteGenericAsync(parameter);
        }

        /// <summary>
        /// Executes the asynchronous command. Any exceptions from the asynchronous delegate are captured and placed on <see cref="Execution"/>; they are not propagated to the UI loop.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public virtual async Task<TResult> ExecuteGenericAsync(object parameter = null)
        {
            _cancelCommand.NotifyCommandStarting();
            Execution = NotifyTaskBase.Create(_executeAsync(parameter, _cancelCommand.Token));

            base.OnCanExecuteChanged();
            var propertyChanged = PropertyChanged;
            propertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get("Execution"));
            propertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get("IsExecuting"));
            await Execution.TaskCompleted;
            _cancelCommand.NotifyCommandFinished();

            base.OnCanExecuteChanged();
            PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get("IsExecuting"));
            return await Execution.Task;
        }

        /// <summary>
        /// Raised when any properties on this instance have changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}