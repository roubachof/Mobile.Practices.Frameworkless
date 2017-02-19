using System;
using System.Threading.Tasks;

namespace SeLoger.Lab.Playground.Core.Nito
{
    public abstract partial class NotifyTaskBase
    {
        public abstract class BuilderBase 
        {
            protected Action WhenCompleted { get; set; }
            protected Action WhenCanceled { get; set; }
            protected Action<Exception> WhenFaulted { get; set; }
        }

        public class Builder : BuilderBase
        {
            public Builder(Func<Task> task)
            {
                TaskFunc = task;
            }

            protected Func<Task> TaskFunc { get; }

            protected Action WhenSuccessfullyCompleted { get; private set; }

            public Builder WithWhenCompleted(Action whenCompleted)
            {
                WhenCompleted = whenCompleted;
                return this;
            }

            public Builder WithWhenCanceled(Action whenCanceled)
            {
                WhenCanceled = whenCanceled;
                return this;
            }

            public Builder WithWhenFaulted(Action<Exception> whenFaulted)
            {
                WhenFaulted = whenFaulted;
                return this;
            }

            public Builder WithWhenSuccessfullyCompleted(Action whenSuccessfullyCompleted)
            {
                WhenSuccessfullyCompleted = whenSuccessfullyCompleted;
                return this;
            }

            public NotifyTask Build()
            {
                return new NotifyTask(TaskFunc(), WhenCanceled, WhenFaulted, WhenCompleted, WhenSuccessfullyCompleted);
            }
        }

        public class Builder<TResult> : BuilderBase
        {
            public Builder(Func<Task<TResult>> taskFunc)
            {
                TaskFunc = taskFunc;
            }

            protected Func<Task<TResult>> TaskFunc { get; }

            protected Action<TResult> WhenSuccessfullyCompleted { get; private set; }

            protected TResult DefaultResult { get; private set; }

            public Builder<TResult> WithWhenCompleted(Action whenCompleted)
            {
                WhenCompleted = whenCompleted;
                return this;
            }

            public Builder<TResult> WithWhenCanceled(Action whenCanceled)
            {
                WhenCanceled = whenCanceled;
                return this;
            }

            public Builder<TResult> WithWhenFaulted(Action<Exception> whenFaulted)
            {
                WhenFaulted = whenFaulted;
                return this;
            }

            public Builder<TResult> WithWhenSuccessfullyCompleted(Action<TResult> whenSuccessfullyCompleted)
            {
                WhenSuccessfullyCompleted = whenSuccessfullyCompleted;
                return this;
            }

            public Builder<TResult> WithDefaultResult(TResult defaultResult)
            {
                DefaultResult = defaultResult;
                return this;
            }

            public NotifyTask<TResult> Build()
            {
                return new NotifyTask<TResult>(TaskFunc(), DefaultResult, WhenCanceled, WhenFaulted, WhenCompleted, WhenSuccessfullyCompleted);
            }
        }
    }
}

