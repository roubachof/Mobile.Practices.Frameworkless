using System;

using SeLoger.Lab.Playground.Core.Services;

namespace SeLoger.Lab.Playground.Core.ViewModels.Extensions
{
    public static class PaginatorExtensions
    {
        public static ViewModelState ToViewModelState<TResult>(this Paginator<TResult> paginator)
        {
            if (!paginator.HasStarted)
            {
                return ViewModelState.NotStarted;
            }

            if (paginator.NotifyTask.IsSuccessfullyCompleted)
            {
                return paginator.LoadedCount == 0 
                    ? ViewModelState.SuccessfullyLoadedNoResults 
                    : ViewModelState.SuccessfullyLoaded;
            }

            if (paginator.NotifyTask.IsFaulted)
            {
                if (paginator.LoadedCount == 0)
                {
                    return paginator.NotifyTask.InnerException is CommunicationException
                               ? ViewModelState.CommunicationError
                               : ViewModelState.UnhandledError;
                }

                return ViewModelState.SuccessfullyLoaded;
            }
            
            if (paginator.NotifyTask.IsNotCompleted)
            {
                if (paginator.LoadedCount > 0)
                {
                    return ViewModelState.LoadingMore;
                }

                return ViewModelState.Loading;
            }

            throw new InvalidOperationException("Shouldn't get there: a task is either Completed or Not");
        }
    }
}
