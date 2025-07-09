using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomManager.Shared.Common
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ServiceResponse<T> SuccessResponse(T data)
        {
            return new ServiceResponse<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ServiceResponse<T> ErrorResponse(string message)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message
            };
        }

        public static ServiceResponse<T> ErrorResponse(List<string> errors)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Errors = errors
            };
        }
    }
}
