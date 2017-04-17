using System;
using System.Text;

namespace Mobile.Practices.Frameworkless.ViewModels
{
    public enum DisplayState
    {
        NotStarted = 0,
        Loading,
        Result,
        Error,
    }

    public enum ErrorType
    {
        None = 0,
        Communication,
        Unhandled,
        NoResults
    }

    public struct ViewModelState
    {
        public ViewModelState(DisplayState state, ErrorType error = ErrorType.None, bool isRefreshed = false)
        {
            Display = state;
            Error = error;
            IsRefreshed = isRefreshed;
        }

        public ViewModelState(DisplayState state, bool isRefreshed)
            : this(state, ErrorType.None, isRefreshed)
        {
        }

        public DisplayState Display { get; }

        public bool IsRefreshed { get; }

        public ErrorType Error { get; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("ViewModelState (");
            sb.Append($" {Enum.GetName(typeof(DisplayState), Display)}");
            sb.Append($" {Enum.GetName(typeof(ErrorType), Error)}");

            string refreshState = IsRefreshed ? "has been refreshed" : "has not been refreshed";
            sb.Append($" {refreshState}");
            sb.Append(")");
            return sb.ToString();
        }
    }
}
