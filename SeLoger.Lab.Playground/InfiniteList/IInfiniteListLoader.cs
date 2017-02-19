namespace SeLoger.Lab.Playground.InfiniteList
{
    /// <summary>
    /// Interface IInfiniteListLoader
    /// </summary>
    public interface IInfiniteListLoader
    {
        /// <summary>
        /// Called when [scroll].
        /// </summary>
        /// <param name="visibleIndex">Index of the visible item.</param>
        void OnScroll(int visibleIndex);
    }
}