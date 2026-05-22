namespace Core.Utils
{
    public class GatewayResponseWrapper<T>
    {
        public bool IsValid { get; set; }
        public bool Success => IsValid;
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public int StatusCode { get; set; } = 200;

        public void SetSuccess(T data, string message = "Success")
        {
            IsValid = true;
            Data = data;
            Message = message;
            StatusCode = 200;
        }

        public void SetError(string message, int statusCode = 400)
        {
            IsValid = false;
            Message = message;
            Errors.Add(message);
            StatusCode = statusCode;
        }
    }

    public class GatewayListResponseWrapper<T> : GatewayResponseWrapper<List<T>> { }

    public class GatewayPaginatedListResponseWrapper<T> : GatewayResponseWrapper<List<T>>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;

        public void SetSuccessWithPagination(List<T> data, int totalCount, int page, int pageSize, string message = "Success")
        {
            IsValid = true;
            Data = data;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
            Message = message;
            StatusCode = 200;
        }
    }
}
