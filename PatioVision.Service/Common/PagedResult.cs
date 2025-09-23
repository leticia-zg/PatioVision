using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatioVision.Service.Common
{
    public record PagedResult<T>(
        IReadOnlyList<T> Items,
        int TotalItems,
        int PageNumber,
        int PageSize
    )
    {
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}