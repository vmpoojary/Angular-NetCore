using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class PaginationParams
    {
                private const int MaxPageSize=50;
        public int PageNumber{get;set;}=1;
        private int _pageSize=10;
        private int myVar;
        public int PageSize
        {
            get =>_pageSize;
            set =>_pageSize=(value>_pageSize)?MaxPageSize:value ;
        }
    }
}