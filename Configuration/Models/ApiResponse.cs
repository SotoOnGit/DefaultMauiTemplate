using System.Net;

namespace Configuration.Models
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Data { get; set; }
        public ApiResponse()
        {
        }

        public ApiResponse(HttpStatusCode statusCode, string data)
        {
            StatusCode = statusCode;
            Data = data;
        }
    }
}
