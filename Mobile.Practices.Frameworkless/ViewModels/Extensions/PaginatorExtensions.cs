using System;

using Mobile.Practices.Frameworkless.InfiniteList;
using Mobile.Practices.Frameworkless.Services;

namespace Mobile.Practices.Frameworkless.ViewModels.Extensions
{
    public static class PaginatorExtensions
    {
        public static ViewModelState ToViewModelState<TResult>(this Paginator<TResult> paginator)
        {
            if (!paginator.HasStarted)
            {
                return new ViewModelState(DisplayState.NotStarted);
            }

            if (paginator.NotifyTask.IsSuccessfullyCompleted)
            {
                if (paginator.LoadedCount == 0)
                {
                    return new ViewModelState(DisplayState.Error, ErrorType.NoResults);
                }

                return new ViewModelState(DisplayState.Result, paginator.HasRefreshed);
            }

            if (paginator.NotifyTask.IsFaulted)
            {
                ErrorType error = paginator.NotifyTask.InnerException is CommunicationException
                                      ? ErrorType.Communication
                                      : ErrorType.Unhandled;
                
                return new ViewModelState(
                    paginator.LoadedCount == 0 ? DisplayState.Error : DisplayState.Result,
                    error,
                    paginator.HasRefreshed);
            }
            
            if (paginator.NotifyTask.IsNotCompleted)
            {
                return new ViewModelState(
                    paginator.LoadedCount > 0 ? DisplayState.Result : DisplayState.Loading);
            }

            throw new InvalidOperationException("Shouldn't get there: a task is either Completed or Not");
        }
    }
}
