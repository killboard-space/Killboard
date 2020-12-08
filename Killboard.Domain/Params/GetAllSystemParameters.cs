
namespace Killboard.Domain.Params
{
    public class GetAllSystemParameters
    {
        private const int maxPageSize = 1500;
        private int pageSize = 1200;
        public int PageNumber { get; set; } = 1;
        
        public int PageSize
        {
            get { return pageSize; }
            set
            {
                pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }
}
