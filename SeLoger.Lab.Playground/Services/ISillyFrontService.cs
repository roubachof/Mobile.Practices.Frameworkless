using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SeLoger.Lab.Playground.Models;

namespace SeLoger.Lab.Playground.Services
{
    public class UnexpectedException : Exception
    {
        public UnexpectedException(string message)
            : base(message)
        {
        }
    }

    public class CommunicationException : Exception
    {
        public CommunicationException(string message)
            : base(message)
        {
        }
    }

    public interface ISillyFrontService
    {
        Task<IReadOnlyList<SillyDudeModel>> GetAllSillyPeople();

        Task<PageResult<SillyDudeModel>> GetSillyPeoplePage(int pageNumber, int pageSize);

        Task<SillyDudeModel> GetSilly(int id);
    }

    public class PageResult<TItem>
    {
        public static readonly PageResult<TItem> Empty = new PageResult<TItem>(0, new List<TItem>());

        public PageResult(int totalCount, IReadOnlyList<TItem> items)
        {
            TotalCount = totalCount;
            Items = items;
        }

        public int TotalCount { get; }

        public IReadOnlyList<TItem> Items { get; }
    }

}
