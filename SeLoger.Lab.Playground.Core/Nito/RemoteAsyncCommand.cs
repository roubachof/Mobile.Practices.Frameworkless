using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Platform;
using SeLoger.LogicLayer.Platform;
using SeLoger.Services.Core;

namespace SeLoger.ViewModel.Core.Nito
{
    /// <summary>
    /// An async command which makes remote calls through the network stack.
    /// </summary>
    public class RemoteAsyncCommand<TResult> : AsyncCommand<TResult>
    {
        private readonly IUserInteractionService _userInteractionService;

        private readonly IConnectivityService _connectivityService;

        /// <summary>
        /// Pure DI to improve testability (http://blog.ploeh.dk/2014/06/10/pure-di/)
        /// </summary>
        public RemoteAsyncCommand(Func<object, CancellationToken, Task<TResult>> executeAsync, Func<object, bool> canExecute = null)
            : this(Mvx.Resolve<IUserInteractionService>(), Mvx.Resolve<IConnectivityService>(), executeAsync, canExecute)
        {
        }

        public RemoteAsyncCommand(
            IUserInteractionService userInteractionService, 
            IConnectivityService connectivityService, 
            Func<object, CancellationToken, Task<TResult>> executeAsync, 
            Func<object, bool> canExecute = null)
            : base(executeAsync, canExecute)
        {
            _userInteractionService = userInteractionService;
            _connectivityService = connectivityService;
        }

        public override Task<TResult> ExecuteGenericAsync(object parameter = null)
        {
            if (!_connectivityService.IsConnectedToNetwork())
            {
                _userInteractionService.ShowNoConnectivityAlert();
                return Task.FromResult(default(TResult));
            }

            return base.ExecuteGenericAsync(parameter);
        }
    }

    // <summary>
    /// An async command which makes remote calls through the network stack.
    /// </summary>
    public class RemoteAsyncCommand : AsyncCommand
    {
        private readonly IUserInteractionService _userInteractionService;

        private readonly IConnectivityService _connectivityService;

        /// <summary>
        /// Pure DI to improve testability (http://blog.ploeh.dk/2014/06/10/pure-di/)
        /// </summary>
        public RemoteAsyncCommand(Func<object, CancellationToken, Task> executeAsync, Func<object, bool> canExecute = null)
            : this(Mvx.Resolve<IUserInteractionService>(), Mvx.Resolve<IConnectivityService>(), executeAsync, canExecute)
        {
        }

        public RemoteAsyncCommand(
            IUserInteractionService userInteractionService,
            IConnectivityService connectivityService,
            Func<object, CancellationToken, Task> executeAsync,
            Func<object, bool> canExecute = null)
            : base(executeAsync, canExecute)
        {
            _userInteractionService = userInteractionService;
            _connectivityService = connectivityService;
        }

        public override Task ExecuteAsync(object parameter = null)
        {
            if (!_connectivityService.IsConnectedToNetwork())
            {
                _userInteractionService.ShowNoConnectivityAlert();
                return Task.FromResult(0);
            }

            return base.ExecuteAsync(parameter);
        }
    }
}
