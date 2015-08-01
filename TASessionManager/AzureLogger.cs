using System;
using System.Configuration;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using log4net.Appender;
using log4net.Core;

namespace TASessionManager
{
    public sealed class LogEventEntity : TableServiceEntity
    {
        public LogEventEntity()
        {
            var now = DateTime.UtcNow;
            PartitionKey = string.Format("{0:d19}", (DateTime.MaxValue - now).Ticks);
            RowKey = Guid.NewGuid().ToString();
        }

        public string Identity { get; set; }
        public string ThreadName { get; set; }
        public string LoggerName { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Domain { get; set; }
        public string RoleInstance { get; set; }
        public string DeploymentId { get; set; }
    }

    public class TableStorageAppender : AppenderSkeleton
    {
        private TableServiceContext _dataContext;
        private string TableName = ConfigurationManager.AppSettings["LogTableName"];

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            var account = CloudStorageAccount.Parse(connectionString);

            var tableClient = account.CreateCloudTableClient();
            tableClient.CreateTableIfNotExist(TableName);

            _dataContext = tableClient.GetDataServiceContext();
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var entity = new LogEventEntity
                {
                    Message = loggingEvent.RenderedMessage,
                    Level = loggingEvent.Level.Name,
                    LoggerName = loggingEvent.LoggerName,
                    Domain = loggingEvent.Domain,
                    ThreadName = loggingEvent.ThreadName,
                    Identity = loggingEvent.Identity
                };

                _dataContext.AddObject(TableName, entity);
                _dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorHandler.Error("Could not write log entry", ex);
            }
        }
    }
}