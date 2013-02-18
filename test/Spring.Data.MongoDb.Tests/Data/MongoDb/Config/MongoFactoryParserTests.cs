#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoParserTest.cs" company="The original author or authors.">
//   Copyright 2002-2013 the original author or authors.
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
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Integration tests for <see cref="MongoFactoryParser"/>
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class MongoFactoryParserTests
    {
        [SetUp]
	    public void SetUp()
        {
            NamespaceParserRegistry.RegisterParser(typeof(MongoNamespaceParser));
        }

	    [Test]
	    public void ReadsMongoAttributesCorrectly()
	    {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                           <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
	                            <mongo:mongo url='mongodb://localhost' write-concern='WMajority' />
                           </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));
            
            Assert.That(factory.GetObjectDefinitionNames(), Contains.Item("Mongo"));

            IObjectDefinition definition = factory.GetObjectDefinition("Mongo");

            Assert.That(definition, Is.Not.Null);

            IList<PropertyValue> values = definition.PropertyValues.PropertyValues;

            Assert.That(values, Contains.Item(new PropertyValue("WriteConcern", "WMajority")));
            Assert.That(values, Contains.Item(new PropertyValue("Url", "mongodb://localhost")));
        }

        [Test]
	    public void ReadsReplicaSetCorrectly()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                           <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
	                          <mongo:mongo id='Mongo2' replica-set='127.0.0.1:4711,127.0.0.1:4712' />
                           </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));

            Assert.That(factory.GetObjectDefinitionNames(), Contains.Item("Mongo2"));

            var server = factory.GetObject<MongoServer>("Mongo2");

            Assert.That(server, Is.Not.Null);
            Assert.That(server.Settings.Servers, Contains.Item(new MongoServerAddress("127.0.0.1", 4711)));
            Assert.That(server.Settings.Servers, Contains.Item(new MongoServerAddress("127.0.0.1", 4712)));       
        }

        [Test]
        public void ReadsMongoSettingsCorrectly()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                           <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>
                             <mongo:mongo id='Mongo3'>
                               <mongo:settings connection-mode='Direct'
                                    connect-timeout='00:01:30'
                                    guid-representation='Standard'
                                    ipv6='true'
                                    max-connection-idle-time='00:03:00'
                                    max-connection-life-time='00:05:00'
                                    max-connection-pool-size='10'
                                    min-connection-pool-size='5'
                                    read-preference='PrimaryPreferred'
                                    secondary-acceptable-latency='00:00:10'
                                    server='localhost:4711'
                                    username='funny'
                                    password='test'
                                    socket-timeout='00:00:30'
                                    use-ssl='true'
                                    verify-ssl-certificate='true'
                                    write-concern='WMajority' />
                            </mongo:mongo>
                        </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));

            var server = factory.GetObject<MongoServer>("Mongo3");

            Assert.That(server, Is.Not.Null);
            Assert.That(server.Settings.ConnectionMode, Is.EqualTo(ConnectionMode.Direct));
            Assert.That(server.Settings.ConnectTimeout, Is.EqualTo(TimeSpan.FromSeconds(90)));
            Assert.That(server.Settings.GuidRepresentation, Is.EqualTo(GuidRepresentation.Standard));
            Assert.That(server.Settings.IPv6, Is.True);
            Assert.That(server.Settings.MaxConnectionIdleTime, Is.EqualTo(TimeSpan.FromMinutes(3)));
            Assert.That(server.Settings.MaxConnectionLifeTime, Is.EqualTo(TimeSpan.FromMinutes(5)));
            Assert.That(server.Settings.MaxConnectionLifeTime, Is.EqualTo(TimeSpan.FromMinutes(5)));
            Assert.That(server.Settings.MaxConnectionPoolSize, Is.EqualTo(10));
            Assert.That(server.Settings.MinConnectionPoolSize, Is.EqualTo(5));
            Assert.That(server.Settings.ReadPreference, Is.EqualTo(ReadPreference.PrimaryPreferred));
            Assert.That(server.Settings.SecondaryAcceptableLatency, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(server.Settings.Server, Is.EqualTo(new MongoServerAddress("localhost", 4711)));
            Assert.That(server.Settings.DefaultCredentials, Is.EqualTo(new MongoCredentials("funny", "test")));
            Assert.That(server.Settings.SocketTimeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
            Assert.That(server.Settings.UseSsl, Is.True);
            Assert.That(server.Settings.VerifySslCertificate, Is.True);
            Assert.That(server.Settings.WriteConcern, Is.EqualTo(WriteConcern.WMajority));        
        }

        [Test]
        public void ReadsMongoSettingsServers()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                           <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>
                             <mongo:mongo id='Mongo3'>
                               <mongo:settings servers='localhost:4711,localhost:4712' />
                            </mongo:mongo>
                        </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));

            var server = factory.GetObject<MongoServer>("Mongo3");

            Assert.That(server, Is.Not.Null);
            Assert.That(server.Settings.Servers, Has.Count.EqualTo(2));
            Assert.That(server.Settings.Servers, Contains.Item(new MongoServerAddress("localhost", 4711)));
            Assert.That(server.Settings.Servers, Contains.Item(new MongoServerAddress("localhost", 4712)));
        }

    }
}
