using System;
using System.Collections;

using Mobile.Practices.Frameworkless.Core.Nito;
using Mobile.Practices.Frameworkless.Services;

namespace Mobile.Practices.Frameworkless.ViewModels.Extensions
{
    public static class NotifyTaskExtensions
    {
        private static ViewModelState ToViewModelState(NotifyTask notifyTask)
        {
            if (notifyTask == null)
            {
                return new ViewModelState(DisplayState.NotStarted);
            }

            if (notifyTask.IsSuccessfullyCompleted)
            {
                return new ViewModelState(DisplayState.Result);
            }

            if (notifyTask.IsFaulted)
            {
                return new ViewModelState(
                    DisplayState.Error,
                    notifyTask.InnerException is CommunicationException ? ErrorType.Communication : ErrorType.Unhandled);
            }

            if (notifyTask.IsNotCompleted)
            {
                return new ViewModelState(DisplayState.Loading);
            }

            throw new InvalidOperationException("Shouldn't get there: a task is either Completed or not");
        }

        public static ViewModelState ToViewModelState<TResult>(this NotifyTask<TResult> notifyTask)
        {
            if (notifyTask == null)
            {
                return new ViewModelState(DisplayState.NotStarted);
            }

            if (notifyTask.IsSuccessfullyCompleted)
            {
                var collection = notifyTask.Result as ICollection;
                if (collection?.Count == 0 || notifyTask.Result == null)
                {
                    return new ViewModelState(DisplayState.Error, ErrorType.NoResults);
                }

                return new ViewModelState(DisplayState.Result);
            }

            if (notifyTask.IsFaulted)
            {
                return new ViewModelState(
                    DisplayState.Error,
                    notifyTask.InnerException is CommunicationException ? ErrorType.Communication : ErrorType.Unhandled);
            }

            if (notifyTask.IsNotCompleted)
            {
                return new ViewModelState(DisplayState.Loading);
            }

            throw new InvalidOperationException("Shouldn't get there: a task is either Completed or not");
        }
    }
}
