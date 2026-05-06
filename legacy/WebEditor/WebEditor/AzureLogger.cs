using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using log4net.Appender;
using log4net.Core;
using Azure;
using Azure.Data.Tables;

namespace WebEditor
{
    public sealed class LogEventEntity : ITableEntity
    {
        public LogEventEntity()
        {
            var now = DateTime.UtcNow;
            PartitionKey = string.Format("{0:d19}", (DateTime.MaxValue - now).Ticks);
            RowKey = Guid.NewGuid().ToString();
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

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
        private readonly Level m_minLevel;
        private readonly TableClient m_table;
        private readonly string m_tableName = ConfigurationManager.AppSettings["AzureLogTable"];

        public TableStorageAppender(Level minLevel)
        {
            m_minLevel = minLevel;
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            m_table = new TableClient(connectionString, m_tableName);
            m_table.CreateIfNotExists();
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                if (m_minLevel != null && m_minLevel > loggingEvent.Level) return;

                var entity = new LogEventEntity
                {
                    Message = loggingEvent.RenderedMessage,
                    Level = loggingEvent.Level.Name,
                    LoggerName = loggingEvent.LoggerName,
                    Domain = loggingEvent.Domain,
                    ThreadName = loggingEvent.ThreadName,
                    Identity = loggingEvent.Identity
                };

                m_table.AddEntity(entity);
            }
            catch (Exception ex)
            {
                ErrorHandler.Error("Could not write log entry", ex);
            }
        }
    }
}