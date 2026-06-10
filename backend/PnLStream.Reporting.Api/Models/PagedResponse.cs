using System;
using System.Collections.Generic;
using System.Text;

namespace PnLStream.Common.Contracts
{
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = [];

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public bool HasNext { get; set; }

        public bool HasPrevious { get; set; }
    }
}
