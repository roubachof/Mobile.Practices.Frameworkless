using System.Collections.Generic;

using MetroLog;
using MetroLog.Layouts;
using MetroLog.Targets;

using SeLoger.Contracts;

namespace Mobile.Practices.Frameworkless.Core
{
    public class MemoryTarget : SyncTarget
    {
        private readonly Queue<string> _queue;

        private readonly int _maxLines;

        public MemoryTarget(int maxLines)
            : base(new SingleLineLayout())
        {
            _maxLines = maxLines;
            _queue = new Queue<string>(maxLines);
        }

        protected override void Write(LogWriteContext context, LogEventInfo entry)
        {
            if (_queue.Count == _maxLines)
            {
                _queue.Dequeue();
            }

            _queue.Enqueue(Layout.GetFormattedString(context, entry));

            Contract.Ensures(() => _queue.Count <= _maxLines);
        }
    }
}
