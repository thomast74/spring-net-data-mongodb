// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongeLog4NetAppenderTest.cs" company="The original author or authors.">
//   Copyright 2002-2012 the original author or authors.
//   
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
//   the License. You may obtain a copy of the License at
//   
//   http://www.apache.org/licenses/LICENSE-2.0
//   
//   Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
//   an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
//   specific language governing permissions and limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using log4net;
using log4net.Config;
using System.Threading;
using System.Security.Principal;


namespace Spring.Data.MongoDb.Log4Net
{
    /// <summary>
    /// </summary>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class MongoLog4NetAppenderTest
    {
        private readonly string _connectionString = "mongodb://localhost/?safe=true";
        private readonly string _databaseName = MongoLog4NetAppender.DefaultDatabaseName;
        private readonly string _collectionName = MongoLog4NetAppender.DefaultCollectionName;

        private MongoServer _mongo;
        private MongoDatabase _db;

        [SetUp]
        public void Setup()
        {
            _mongo = MongoServer.Create(_connectionString);
            _db = _mongo.GetDatabase(_databaseName);
            _db.GetCollection(_collectionName).Drop();
        }

        [Test]
        public void LogTimeStamp()
        {
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
                <log4net>
	                <appender name='mongo' type='Spring.Data.MongoDb.Log4Net.MongoLog4NetAppender, Spring.Data.MongoDb.Log4Net'>
		                <connectionString value='mongodb://localhost' />
		                <parameter>
			                <parameterName value='timestamp' />
			                <layout type='log4net.Layout.RawTimeStampLayout' />
		                </parameter>
	                </appender>
	                <root><level value='ALL' /><appender-ref ref='mongo' /></root></log4net>
                ")));
            var log = LogManager.GetLogger(typeof(MongoLog4NetAppenderTest));

            log.Info("message");

            var doc = _db.GetCollection(_collectionName).FindOneAs<BsonDocument>();
            Assert.That(doc.GetElement("timestamp").Value, Is.TypeOf<BsonDateTime>());
            Assert.That(doc.GetElement("timestamp").Value.AsDateTime, Is.InRange(DateTime.UtcNow.AddSeconds(-1), DateTime.Now));
        }

        [Test]
        public void LogLogLevel()
        {
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
                <log4net>
	                <appender name='mongo' type='Spring.Data.MongoDb.Log4Net.MongoLog4NetAppender, Spring.Data.MongoDb.Log4Net'>
		                <connectionString value='mongodb://localhost' />
                        <parameter>
                          <parameterName value='level' />
                          <layout type='log4net.Layout.PatternLayout' value='%level' />
                        </parameter>    
	                </appender>
	                <root><level value='ALL' /><appender-ref ref='mongo' /></root></log4net>
                ")));
            var log = LogManager.GetLogger(typeof(MongoLog4NetAppenderTest));

            log.Info("message");

            var doc = _db.GetCollection(_collectionName).FindOneAs<BsonDocument>();
            Assert.That(doc.GetElement("level").Value, Is.TypeOf<BsonString>());
            Assert.That(doc.GetElement("level").Value.AsString, Is.EqualTo("INFO"));
        }

        [Test]
        public void LogThread()
        {
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
                <log4net>
	                <appender name='mongo' type='Spring.Data.MongoDb.Log4Net.MongoLog4NetAppender, Spring.Data.MongoDb.Log4Net'>
		                <connectionString value='mongodb://localhost' />
		                <parameter>
			                <parameterName value='thread' />
			                <layout type='log4net.Layout.PatternLayout' value='%thread' />
		                </parameter>	                
                    </appender>
	                <root><level value='ALL' /><appender-ref ref='mongo' /></root></log4net>
                ")));
            var log = LogManager.GetLogger(typeof(MongoLog4NetAppenderTest));

            log.Info("message");

            var doc = _db.GetCollection(_collectionName).FindOneAs<BsonDocument>();
            Assert.That(doc.GetElement("thread").Value, Is.TypeOf<BsonString>());
            Assert.That(doc.GetElement("thread").Value.AsString, Is.EqualTo(Thread.CurrentThread.Name));
        }

        [Test]
        public void LogException()
        {
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
                <log4net>
	                <appender name='mongo' type='Spring.Data.MongoDb.Log4Net.MongoLog4NetAppender, Spring.Data.MongoDb.Log4Net'>
		                <connectionString value='mongodb://localhost' />
		                <parameter>
			                <parameterName value='exception' />
			                <layout type='log4net.Layout.ExceptionLayout' />
		                </parameter>                    
                    </appender>
	                <root><level value='ALL' /><appender-ref ref='mongo' /></root></log4net>
                ")));
            var log = LogManager.GetLogger(typeof(MongoLog4NetAppenderTest));

            try
            {
                throw new Exception("little problem");
            }
            catch (Exception e)
            {
                log.Error("ex", e);
            }

            var doc = _db.GetCollection(_collectionName).FindOneAs<BsonDocument>();
            Assert.That(doc.GetElement("exception").Value, Is.TypeOf<BsonString>());
            Assert.That(doc.GetElement("exception").Value.AsString, Is.StringContaining("Exception: little problem"));
        }

        [Test]
        public void LogThreadProperties()
        {
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
                <log4net>
	                <appender name='mongo' type='Spring.Data.MongoDb.Log4Net.MongoLog4NetAppender, Spring.Data.MongoDb.Log4Net'>
		                <connectionString value='mongodb://localhost' />
		                <parameter>
			                <parameterName value='contextProperty' />
			                <layout type='log4net.Layout.RawPropertyLayout'>
				                <key value='prop' />
			                </layout>
		                </parameter>                    
                    </appender>
	                <root><level value='ALL' /><appender-ref ref='mongo' /></root></log4net>
                ")));
            var log = LogManager.GetLogger(typeof(MongoLog4NetAppenderTest));

            ThreadContext.Properties["prop"] = "value";

            log.Info("message");

            var doc = _db.GetCollection(_collectionName).FindOneAs<BsonDocument>();
            Assert.That(doc.GetElement("contextProperty").Value.AsString, Is.EqualTo("value"));
        }

        [Test]
        public void LogGlobalProperties()
        {
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
                <log4net>
	                <appender name='mongo' type='Spring.Data.MongoDb.Log4Net.MongoLog4NetAppender, Spring.Data.MongoDb.Log4Net'>
		                <connectionString value='mongodb://localhost' />
		                <parameter>
			                <parameterName value='contextProperty' />
			                <layout type='log4net.Layout.RawPropertyLayout'>
				                <key value='prop' />
			                </layout>
		                </parameter>                    
                    </appender>
	                <root><level value='ALL' /><appender-ref ref='mongo' /></root></log4net>
                ")));
            var log = LogManager.GetLogger(typeof(MongoLog4NetAppenderTest));

            ThreadContext.Properties["prop"] = "value";

            log.Info("message");

            var doc = _db.GetCollection(_collectionName).FindOneAs<BsonDocument>();
            Assert.That(doc.GetElement("contextProperty").Value.AsString, Is.EqualTo("value"));
        }

        [Test]
        public void LogDefaultDocumentIfNoParametersDefined()
        {
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
                <log4net>
	                <appender name='mongo' type='Spring.Data.MongoDb.Log4Net.MongoLog4NetAppender, Spring.Data.MongoDb.Log4Net'>
		                <connectionString value='mongodb://localhost' />
                    </appender>
	                <root><level value='ALL' /><appender-ref ref='mongo' /></root></log4net>
                ")));
            var log = LogManager.GetLogger(typeof(MongoLog4NetAppenderTest));

			GlobalContext.Properties["GlobalProp"] = "GlobalValue";
			ThreadContext.Properties["ThreadProp"] = "ThreadValue";

			try
			{
				throw new Exception("little problem");
			}
			catch (Exception e)
			{
				log.Error("ex", e);
			}

			var doc = _db.GetCollection(_collectionName).FindOneAs<BsonDocument>();

			Assert.That(doc.GetElement("timestamp").Value.AsDateTime, Is.InRange(DateTime.UtcNow.AddSeconds(-1), DateTime.Now));
			Assert.That(doc.GetElement("level").Value.AsString, Is.EqualTo("ERROR"));
			Assert.That(doc.GetElement("thread").Value.AsString, Is.EqualTo(Thread.CurrentThread.Name));
			Assert.That(doc.GetElement("userName").Value.AsString, Is.EqualTo(WindowsIdentity.GetCurrent().Name));
			Assert.That(doc.GetElement("message").Value.AsString, Is.EqualTo("ex"));
            Assert.That(doc.GetElement("loggerName").Value.AsString, Is.EqualTo("Spring.Data.MongoDb.Log4Net.MongoLog4NetAppenderTest"));
			Assert.That(doc.GetElement("domain").Value.AsString, Is.EqualTo(AppDomain.CurrentDomain.FriendlyName));
			Assert.That(doc.GetElement("hostName").Value.AsString, Is.EqualTo(Environment.MachineName));

            var locInfo = doc.GetElement("locationInfo").Value.AsBsonDocument;
            Assert.That(locInfo.GetElement("fileName").Value.AsString, Is.StringEnding("MongoLog4NetAppenderTest.cs"));
            Assert.That(locInfo.GetElement("methodName").Value.AsString, Is.EqualTo("LogDefaultDocumentIfNoParametersDefined"));
			Assert.That(locInfo.GetElement("lineNumber").Value.AsString, Is.StringMatching(@"\d+"));
			Assert.That(locInfo.GetElement("className").Value.AsString, Is.EqualTo("Spring.Data.MongoDb.Log4Net.MongoLog4NetAppenderTest"));

			var exception = doc.GetElement("exception").Value.AsBsonDocument;
			Assert.That(exception.GetElement("message").Value.AsString, Is.EqualTo("little problem"));

			var properties = doc.GetElement("properties").Value.AsBsonDocument;
			Assert.That(properties.GetElement("GlobalProp").Value.AsString, Is.EqualTo("GlobalValue"));
			Assert.That(properties.GetElement("ThreadProp").Value.AsString, Is.EqualTo("ThreadValue"));
        }

        [Test]
        public void LogWithCollectionPattern()
        {
            var col = "myApp" + DateTime.Now.ToString("yyyyMM");
            _db.GetCollection(col).Drop();
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(@"
                <log4net>
                  <appender name='mongo' type='Spring.Data.MongoDb.Log4Net.MongoLog4NetAppender, Spring.Data.MongoDb.Log4Net'>
                    <connectionString value='mongodb://localhost/?safe=true' />
                    <collectionPattern value='%X{applicationId}%X{year}%X{month}' />
                    <applicationId value='myApp' />
                  </appender>
                  <root><level value='DEBUG' /><appender-ref ref='mongo' /></root></log4net>
                ")));
            var log = LogManager.GetLogger(typeof(MongoLog4NetAppenderTest));

            log.Info("message");

            Assert.That(_db.CollectionExists(col), Is.True);
            Assert.That(_db.GetCollection(col).Count(), Is.EqualTo(1));
        }
    }
}
