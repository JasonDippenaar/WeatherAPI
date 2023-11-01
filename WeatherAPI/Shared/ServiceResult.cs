namespace WeatherAPI.Shared
{
    public record ServiceResult<T>
    {
        public ServiceResult(bool success, string errorMessage, int statusCode)
        {
            Success = success;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        public ServiceResult(bool success, T? data)
        {
            Success = success;
            Data = data;
        }

        public bool Success { get; set; } = false;
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int StatusCode { get; set; } = -1;
    }
}
