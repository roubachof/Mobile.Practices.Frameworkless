// ********************************************************************************************************************
// <author>Stephen Cleary</author>
// <date>08-2016</date>
// <version>v1.0.0-eta-02</version>
// <web>https://github.com/StephenCleary/Mvvm.Async/blob/master/src/Nito.Mvvm.Async/NotifyTask.cs</web>
// ********************************************************************************************************************

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Nito.Mvvm;

namespace SeLoger.Lab.Playground.Core.Nito
{
    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    public abstract partial class NotifyTaskBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Callback called when the task has been canceled.
        /// </summary>
        private readonly Action _whenCanceled;

        /// <summary>
        /// Callback called when the task is faulted.
        /// </summary>
        private readonly Action<Exception> _whenFaulted;

        /// <summary>
        /// Callback called when the task completed (successfully or not).
        /// </summary>
        private readonly Action _whenCompleted;

        /// <summary>
        /// Initializes a task notifier watching the specified task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        protected NotifyTaskBase(Task task, Action whenCanceled = null, Action<Exception> whenFaulted = null, Action whenCompleted = null)
        {
            Task = task;
            _whenCanceled = whenCanceled;
            _whenFaulted = whenFaulted;
            _whenCompleted = whenCompleted;
        }

        protected async Task MonitorTaskAsync(Task task)
        {
            try
            {
                await task;
            }
            catch
            {
            }
            finally
            {
                NotifyProperties(task);
            }
        }

        private void NotifyProperties(Task task)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;

            if (task.IsCanceled)
            {
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("Status"));
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("IsCanceled"));

                _whenCanceled?.Invoke();
            }
            else if (task.IsFaulted)
            {
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("Exception"));
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("InnerException"));
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("ErrorMessage"));
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("Status"));
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("IsFaulted"));

                _whenFaulted?.Invoke(task.Exception.InnerException);
            }
            else
            {
                OnSuccessfullyCompleted(propertyChanged);
            }

            propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("IsCompleted"));
            propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("IsNotCompleted"));

            _whenCompleted?.Invoke();
        }

        protected virtual void OnSuccessfullyCompleted(PropertyChangedEventHandler propertyChanged)
        {
            propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("Status"));
            propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("IsSuccessfullyCompleted"));
        }

        /// <summary>
        /// Gets the task being watched. This property never changes and is never <c>null</c>.
        /// </summary>
        public Task Task { get; private set; }

        /// <summary>
        /// Gets a task that completes successfully when <see cref="Task"/> completes (successfully, faulted, or canceled). This property never changes and is never <c>null</c>.
        /// </summary>
        public Task TaskCompleted { get; protected set; }

        /// <summary>
        /// Gets the current task status. This property raises a notification when the task completes.
        /// </summary>
        public TaskStatus Status => Task.Status;

        /// <summary>
        /// Gets whether the task has completed. This property raises a notification when the value changes to <c>true</c>.
        /// </summary>
        public bool IsCompleted => Task.IsCompleted;

        /// <summary>
        /// Gets whether the task is busy (not completed). This property raises a notification when the value changes to <c>false</c>.
        /// </summary>
        public bool IsNotCompleted => !Task.IsCompleted;

        /// <summary>
        /// Gets whether the task has completed successfully. This property raises a notification when the value changes to <c>true</c>.
        /// </summary>
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

        /// <summary>
        /// Gets whether the task has been canceled. This property raises a notification only if the task is canceled (i.e., if the value changes to <c>true</c>).
        /// </summary>
        public bool IsCanceled => Task.IsCanceled;

        /// <summary>
        /// Gets whether the task has faulted. This property raises a notification only if the task faults (i.e., if the value changes to <c>true</c>).
        /// </summary>
        public bool IsFaulted => Task.IsFaulted;

        /// <summary>
        /// Gets the wrapped faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        public AggregateException Exception => Task.Exception;

        /// <summary>
        /// Gets the original faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        public Exception InnerException => Exception?.InnerException;

        /// <summary>
        /// Gets the error message for the original faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        public string ErrorMessage => InnerException?.Message;

        /// <summary>
        /// Event that notifies listeners of property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new task notifier watching the specified task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        public static NotifyTask Create(Task task)
        {
            return new NotifyTask(task);
        }

        /// <summary>
        /// Creates a new task notifier watching the specified task.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        /// <param name="task">The task to watch.</param>
        /// <param name="defaultResult">The default "result" value for the task while it is not yet complete.</param>
        public static NotifyTask<TResult> Create<TResult>(Task<TResult> task, TResult defaultResult = default(TResult))
        {
            return new NotifyTask<TResult>(task, defaultResult);
        }

        /// <summary>
        /// Executes the specified asynchronous code and creates a new task notifier watching the returned task.
        /// </summary>
        /// <param name="asyncAction">The asynchronous code to execute.</param>
        public static NotifyTask Create(Func<Task> asyncAction)
        {
            return Create(asyncAction());
        }

        /// <summary>
        /// Executes the specified asynchronous code and creates a new task notifier watching the returned task.
        /// </summary>
        /// <param name="asyncAction">The asynchronous code to execute.</param>
        /// <param name="defaultResult">The default "result" value for the task while it is not yet complete.</param>
        public static NotifyTask<TResult> Create<TResult>(Func<Task<TResult>> asyncAction, TResult defaultResult = default(TResult))
        {
            return Create(asyncAction(), defaultResult);
        }
    }

    public sealed class NotifyTask : NotifyTaskBase, INotifyPropertyChanged
    {
        /// <summary>
        /// Callback called when the task successfully completed.
        /// </summary>
        private readonly Action _whenSuccessfullyCompleted;

        /// <summary>
        /// Initializes a task notifier watching the specified task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        internal NotifyTask(Task task, Action whenCanceled = null, Action<Exception> whenFaulted = null, Action whenCompleted = null, Action whenSuccessfullyCompleted = null)
            :base(task, whenCanceled, whenFaulted, whenCompleted)
        {
            _whenSuccessfullyCompleted = whenSuccessfullyCompleted;
            TaskCompleted = MonitorTaskAsync(task);
        }

        protected override void OnSuccessfullyCompleted(PropertyChangedEventHandler propertyChanged)
        {
            base.OnSuccessfullyCompleted(propertyChanged);

            _whenSuccessfullyCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    public sealed class NotifyTask<TResult> : NotifyTaskBase, INotifyPropertyChanged
    {
        /// <summary>
        /// The "result" of the task when it has not yet completed.
        /// </summary>
        private readonly TResult _defaultResult;

        /// <summary>
        /// Callback called when the task successfully completed.
        /// </summary>
        private readonly Action<TResult> _whenSuccessfullyCompleted;

        /// <summary>
        /// Initializes a task notifier watching the specified task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        /// <param name="defaultResult">The value to return from <see cref="Result"/> while the task is not yet complete.</param>
        internal NotifyTask(
            Task<TResult> task, 
            TResult defaultResult = default(TResult), 
            Action whenCanceled = null, 
            Action<Exception> whenFaulted = null, 
            Action whenCompleted = null, 
            Action<TResult> whenSuccessfullyCompleted = null)
            : base(task, whenCanceled, whenFaulted, whenCompleted)
        {
            _defaultResult = defaultResult;
            _whenSuccessfullyCompleted = whenSuccessfullyCompleted;
            Task = task;
            TaskCompleted = MonitorTaskAsync(task);
        }

        protected override void OnSuccessfullyCompleted(PropertyChangedEventHandler propertyChanged)
        {
            propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("Result"));
            base.OnSuccessfullyCompleted(propertyChanged);

            _whenSuccessfullyCompleted?.Invoke(Result);
        }

        /// <summary>
        /// Gets the task being watched. This property never changes and is never <c>null</c>.
        /// </summary>
        public new Task<TResult> Task { get; private set; }

        /// <summary>
        /// Gets the result of the task. Returns the "default result" value specified in the constructor if the task has not yet completed successfully. This property raises a notification when the task completes successfully.
        /// </summary>
        public TResult Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : _defaultResult;
    }
}