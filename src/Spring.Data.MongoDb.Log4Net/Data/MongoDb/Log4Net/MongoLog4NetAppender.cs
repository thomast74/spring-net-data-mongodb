using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using log4net;
using log4net.Appender;
using MongoDB.Bson;
using log4net.Core;
using log4net.Layout;
using log4net.Util;

namespace Spring.Data.MongoDb.Log4Net
{

    /// <summary>
    /// MongoDb Log4Net Appender
    /// </summary>
    public class MongoLog4NetAppender : AppenderSkeleton, IDisposable
    {
        /// <summary>
        /// Default database name if no name is specified in the appender configuration
        /// </summary>
        public const string DefaultDatabaseName = "log4net";

        /// <summary>
        /// Default collection name if no CollectionPattern was specified in the appender configuration
        /// </summary>
        public const string DefaultCollectionName = "logs";


        private static readonly string LEVEL = "level";
        private static readonly string LOGGER_NAME = "loggerName";
        private static readonly string APP_ID = "applicationId";
        private static readonly string TIMESTAMP = "timestamp";
        private static readonly string PROPERTIES = "properties";
        private static readonly string LOCATION_INFO = "locationInfo";
        private static readonly string EXCEPTION = "exception";
        private static readonly string DOMAIN = "domain";
        private static readonly string IDENTITY = "identity";
        private static readonly string THREAD = "thread";
        private static readonly string USER_NAME = "userName";
        private static readonly string HOST_NAME = "hostName";
        private static readonly string MESSAGE = "message";
        private static readonly string YEAR = "year";
        private static readonly string MONTH = "month";
        private static readonly string DAY = "day";
        private static readonly string HOUR = "hour";


        private IList<MongoAppenderParameter> _parameters;
        private MongoServer _mongo;
        private MongoDatabase _db;


        /// <summary>
        /// Connection to connect to a MongoDB instance or cluster
        /// mongodb://[username:password@]host1[:port1][,host2[:port2],...[,hostN[:portN]]][/[database][?options]]
        /// More details can be found at http://www.mongodb.org/display/DOCS/Connections
        /// 
        /// If no database specified <see cref="DefaultDatabaseName"/>
        /// </summary>
        public virtual string ConnectionString { get; set; }
        /// <summary>
        /// Defined the database name to use for logging informaiton to a mongoDB server
        /// </summary>
        public virtual string Database { get; set; }

        /// <summary>
        /// PatternLayout to defined the collection name
        /// If no CollectionPattern specified <see cref="DefaultCollectionName"/>
        /// </summary>
        public virtual string CollectionPattern { get; set; }

        /// <summary>
        /// An Id to identify the application if more then one application is using the 
        /// same database and collection for storing logging information
        /// </summary>
        public virtual string ApplicationId { get; set; }


        /// <summary>
        /// Creates the MongoDb Log4Net Appender
        /// </summary>
        public MongoLog4NetAppender()
        {
            _parameters = new List<MongoAppenderParameter>();
        }

        /// <summary>
        /// Define parameter to log specific elements of the logging event.
        /// </summary>
        /// <param name="parameter">Specifies the element key name and value pattern</param>
        public void AddParameter(MongoAppenderParameter parameter)
        {
            _parameters.Add(parameter);
        }

        /// <summary>
        /// Loges event into MongoDB database collection.
        /// If no paramaters specified default fields are written into document.
        /// </summary>
        /// <param name="loggingEvent">Event log into database collection</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            ConnectToMongo();
            var collectionName = GetCollectionName(loggingEvent);

            var dbo = _parameters.Count == 0 ? AppendWithoutParameters(loggingEvent) : AppendWithParameter(loggingEvent);

            _db.GetCollection(collectionName).Insert(dbo);
        }

        private BsonDocument AppendWithParameter(LoggingEvent loggingEvent)
        {
            var dbo = new BsonDocument();
            foreach (var parameter in _parameters)
            {
                dbo.Add(parameter.ParameterName, BsonValue.Create(parameter.Layout.Format(loggingEvent).ToString()));
            }

            return dbo;
        }

        private BsonDocument AppendWithoutParameters(LoggingEvent loggingEvent)
        {
            var dbo = new BsonDocument();
            if (!string.IsNullOrEmpty(ApplicationId))
                dbo.Add(APP_ID, ApplicationId);
            dbo.Add(TIMESTAMP, loggingEvent.TimeStamp);
            dbo.Add(LEVEL, loggingEvent.Level.ToString());
            dbo.Add(LOGGER_NAME, loggingEvent.LoggerName);
            dbo.Add(DOMAIN, loggingEvent.Domain);
            dbo.Add(IDENTITY, loggingEvent.Identity);
            dbo.Add(MESSAGE, loggingEvent.RenderedMessage);
            dbo.Add(THREAD, loggingEvent.ThreadName);
            dbo.Add(USER_NAME, loggingEvent.UserName);
            dbo.Add(HOST_NAME, Environment.MachineName);
            
            var properties = loggingEvent.GetProperties();
            if (properties != null && properties.Count > 0)
            {
                var propsDbo = new BsonDocument();
                foreach (DictionaryEntry entry in properties)
                {
                    propsDbo.Add(entry.Key.ToString(), BsonValue.Create(entry.Value));
                }
                dbo.Add(PROPERTIES, propsDbo);
            }

            if (loggingEvent.ExceptionObject != null)
                dbo.Add(EXCEPTION, GetException(loggingEvent.ExceptionObject));

            if (loggingEvent.LocationInformation != null)
            {
                var locDbo = new BsonDocument
                                 {
                                     {"className", loggingEvent.LocationInformation.ClassName},
                                     {"fileName", loggingEvent.LocationInformation.FileName},
                                     {"lineNumber", loggingEvent.LocationInformation.LineNumber},
                                     {"methodName", loggingEvent.LocationInformation.MethodName}
                                 };
                dbo.Add(LOCATION_INFO, locDbo);
            }

            return dbo;
        }

        private BsonDocument GetException(Exception e)
        {
            var exception = new BsonDocument
                                {
                                    {"message", e.Message},
                                    {"source", e.Source ?? string.Empty},
                                    {"stackTrace", e.StackTrace ?? string.Empty}
                                };

            if (e.InnerException != null)
                exception.Add("innerException", GetException(e.InnerException));

            return exception;            
        }

        private void ConnectToMongo()
        {
            try
            {
                if (_db != null)
                    return;

                _mongo = MongoServer.Create(ConnectionString);
                _db = _mongo.GetDatabase(string.IsNullOrEmpty(Database) ? "log4net" : Database);
            }
            catch (Exception e)
            {
                throw new SystemException(e.Message, e);
            }
        }

        private string GetCollectionName(LoggingEvent loggingEvent)
        {
            if (string.IsNullOrEmpty(CollectionPattern))
                return "logs";

            SetMdcProperties();
            var pattern = new Layout2RawLayoutAdapter(new PatternLayout(CollectionPattern));
            var collection = pattern.Format(loggingEvent) as string;
            RemoveMdcProperties();

            return collection;
        }

        private void SetMdcProperties()
        {
            if (!string.IsNullOrEmpty(ApplicationId))
                MDC.Set(APP_ID, ApplicationId);

            MDC.Set(YEAR, DateTime.Now.ToString("yyyy"));
            MDC.Set(MONTH, DateTime.Now.ToString("MM"));
            MDC.Set(DAY, DateTime.Now.ToString("dd"));
            MDC.Set(HOUR, DateTime.Now.ToString("HH"));            
        }

        private void RemoveMdcProperties()
        {
            if (!string.IsNullOrEmpty(ApplicationId))
                MDC.Remove(APP_ID);

            MDC.Remove(YEAR);
            MDC.Remove(MONTH);
            MDC.Remove(DAY);
            MDC.Remove(HOUR);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_mongo != null)
                _mongo.Disconnect();
        }

    }
}
