using System;
using System.Collections;

using SeLoger.Lab.Playground.Core.Nito;
using SeLoger.Lab.Playground.Core.Services;

namespace SeLoger.Lab.Playground.Core.ViewModels.Extensions
{
    public static class NotifyTaskExtensions
    {
        private static ViewModelState ToViewModelState(NotifyTask notifyTask)
        {
            if (notifyTask == null)
            {
                return ViewModelState.NotStarted;
            }

            if (notifyTask.IsSuccessfullyCompleted)
            {
                return ViewModelState.SuccessfullyLoaded;
            }

            if (notifyTask.IsFaulted)
            {
                return notifyTask.InnerException is CommunicationException
                    ? ViewModelState.CommunicationError
                    : ViewModelState.UnhandledError;
            }

            if (notifyTask.IsNotCompleted)
            {
                return ViewModelState.Loading;
            }

            throw new InvalidOperationException("Shouldn't get there: a task is either Completed or not");
        }

        public static ViewModelState ToViewModelState<TResult>(this NotifyTask<TResult> notifyTask)
        {
            if (notifyTask == null)
            {
                return ViewModelState.NotStarted;
            }

            if (notifyTask.IsSuccessfullyCompleted)
            {
                var collection = notifyTask.Result as ICollection;
                if (collection?.Count == 0 || notifyTask.Result == null)
                {
                    return ViewModelState.SuccessfullyLoadedNoResults;
                }

                return ViewModelState.SuccessfullyLoaded;
            }

            if (notifyTask.IsFaulted)
            {
                return notifyTask.InnerException is CommunicationException
                    ? ViewModelState.CommunicationError
                    : ViewModelState.UnhandledError;
            }

            if (notifyTask.IsNotCompleted)
            {
                return ViewModelState.Loading;
            }

            throw new InvalidOperationException("Shouldn't get there: a task is either Completed or not");
        }
    }
}
