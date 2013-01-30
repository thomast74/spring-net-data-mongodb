// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoServerSettingsFactoryObjectTests.cs" company="The original author or authors.">
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
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using Spring.Data.MongoDb.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Unit test for <see cref="MongoServerSettingsFactoryObject"/>
    /// </summary>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class MongoServerSettingsFactoryObjectTests
    {
        [Test]
        public void ReturnNewInstance()
        {
            var factory = new DefaultListableObjectFactory();
            factory.RegisterCustomConverter(typeof(WriteConcern), new WriteConcernConverter());
            factory.RegisterCustomConverter(typeof(ReadPreference), new ReadPreferenceConverter());

            var definition = new RootObjectDefinition(typeof(MongoServerSettingsFactoryObject));
            definition.PropertyValues.Add("ConnectionMode", "Direct");
            definition.PropertyValues.Add("ConnectTimeout", "00:00:30");
            definition.PropertyValues.Add("GuidRepresentation", "Standard");
            definition.PropertyValues.Add("IpPv6", "false");
            definition.PropertyValues.Add("MaxConnectionIdleTime", "00:00:31");
            definition.PropertyValues.Add("MaxConnectionLifeTime", "00:00:32");
            definition.PropertyValues.Add("MaxConnectionPoolSize", "2");
            definition.PropertyValues.Add("MinConnectionPoolSize", "1");
            definition.PropertyValues.Add("ReadPreference", "Primary");
            definition.PropertyValues.Add("ReplicaSetName", "Set");
            definition.PropertyValues.Add("WriteConcern", WriteConcern.Acknowledged);
            definition.PropertyValues.Add("SecondaryAcceptableLatency", "00:00:33");
            definition.PropertyValues.Add("SocketTimeout", "00:00:34");
            definition.PropertyValues.Add("UseSsl", "false");
            definition.PropertyValues.Add("VerifySslCertificate", "true");
            definition.PropertyValues.Add("WaitQueueSize", "4");
            definition.PropertyValues.Add("WaitQueueTimeout", "00:00:35");

            factory.RegisterObjectDefinition("factory", definition);

            var settings = factory.GetObject<MongoServerSettingsFactoryObject>("&factory");

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ConnectionMode, Is.EqualTo(ConnectionMode.Direct));
            Assert.That(settings.ConnectTimeout, Is.EqualTo(new TimeSpan(0, 0,30)));
            Assert.That(settings.GuidRepresentation, Is.EqualTo(GuidRepresentation.Standard));
            Assert.That(settings.IpPv6, Is.False);
            Assert.That(settings.MaxConnectionIdleTime, Is.EqualTo(new TimeSpan(0, 0 ,31)));
            Assert.That(settings.MaxConnectionLifeTime, Is.EqualTo(new TimeSpan(0, 0, 32)));
            Assert.That(settings.MaxConnectionPoolSize, Is.EqualTo(2));
            Assert.That(settings.ReadPreference, Is.EqualTo(ReadPreference.Primary));
            Assert.That(settings.WriteConcern, Is.EqualTo(WriteConcern.Acknowledged));
            Assert.That(settings.SecondaryAcceptableLatency, Is.EqualTo(new TimeSpan(0, 0, 33)));
            Assert.That(settings.SocketTimeout, Is.EqualTo(new TimeSpan(0, 0, 34)));
            Assert.That(settings.UseSsl, Is.False);
            Assert.That(settings.VerifySslCertificate, Is.True);
            Assert.That(settings.WaitQueueSize, Is.EqualTo(4));
            Assert.That(settings.WaitQueueTimeout, Is.EqualTo(new TimeSpan(0, 0, 35)));
        }
    }
}
