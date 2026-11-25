using System;
using System.Threading.Tasks;
using API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace API.Services
{
    public class MongoLogService : IMongoLogService
    {
        private readonly ILogger<MongoLogService> _logger;
        private readonly IMongoCollection<Log>? _logsCollection;
        private readonly IMongoCollection<Log>? _exceptionsCollection;
        private readonly bool _isEnabled;

        public MongoLogService(IConfiguration configuration, ILogger<MongoLogService> logger)
        {
            _logger = logger;
            try
            {
                string connectionString = configuration.GetConnectionString("MongoConnection") 
                    ?? configuration.GetValue<string>("MongoConnectionString") 
                    ?? "mongodb://localhost:27017";

                string dbName = configuration.GetValue<string>("MongoDatabaseName") ?? "DirectoryLogs";

                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(dbName);
                _logsCollection = database.GetCollection<Log>("Logs");
                _exceptionsCollection = database.GetCollection<Log>("Exceptions");
                _isEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB initialization failed. Falling back to local console logging.");
                _isEnabled = false;
            }
        }

        public async Task LogEventAsync(string eventType, string message, string? userEmail = null, string? details = null)
        {
            _logger.LogInformation("[{EventType}] {Message} (User: {UserEmail})", eventType, message, userEmail ?? "anonymous");

            if (!_isEnabled || _logsCollection == null) return;

            try
            {
                var log = new Log
                {
                    EventType = eventType,
                    Message = message,
                    UserEmail = userEmail,
                    Details = details,
                    Date = DateTime.UtcNow.ToString("o"),
                    Timestamp = DateTime.UtcNow
                };

                await _logsCollection.InsertOneAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to persist log to MongoDB: {Message}", ex.Message);
            }
        }

        public async Task LogExceptionAsync(string message, string? stackTrace = null, string? requestPath = null)
        {
            _logger.LogError("Exception on {Path}: {Message}\n{StackTrace}", requestPath ?? "/", message, stackTrace);

            if (!_isEnabled || _exceptionsCollection == null) return;

            try
            {
                var log = new Log
                {
                    EventType = "EXCEPTION",
                    Message = message,
                    Details = $"Path: {requestPath}\nStack: {stackTrace}",
                    Date = DateTime.UtcNow.ToString("o"),
                    Timestamp = DateTime.UtcNow
                };

                await _exceptionsCollection.InsertOneAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to persist exception to MongoDB: {Message}", ex.Message);
            }
        }
    }
}
