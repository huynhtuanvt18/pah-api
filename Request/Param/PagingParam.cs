namespace Request.Param
{
    public class PagingParam
    {
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value > maxPageSize ? maxPageSize : value;
            }
        }
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        const int maxPageSize = 50;
    }
}
