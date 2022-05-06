using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class pageList<T> : List<T>
    {
        public pageList(IEnumerable<T> items, int Count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(Count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = Count;
            AddRange(items);
        }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; } //how many toal count in the query
        public static async Task<pageList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var Count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new pageList<T>(items, Count, pageNumber, pageSize);
        }
    }

}