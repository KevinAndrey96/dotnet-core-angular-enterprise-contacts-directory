using System.Threading.Tasks;
using API.Models;

namespace API.Services
{
    public interface IMongoLogService
    {
        Task LogEventAsync(string eventType, string message, string? userEmail = null, string? details = null);
        Task LogExceptionAsync(string message, string? stackTrace = null, string? requestPath = null);
    }
}
