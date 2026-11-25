namespace API.DTOs
{
    public class ApiResponse<T>
    {
        public int status { get; set; } = 200;
        public string alert { get; set; } = string.Empty;
        public T? data { get; set; }

        public static ApiResponse<T> Success(T data, string alert = "Operaci\u00f3n exitosa")
        {
            return new ApiResponse<T> { status = 200, alert = alert, data = data };
        }

        public static ApiResponse<T> Error(string alert, int status = 400)
        {
            return new ApiResponse<T> { status = status, alert = alert, data = default };
        }
    }
}
