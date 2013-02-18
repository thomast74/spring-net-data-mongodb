#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoDatabaseFactoryParserTests.cs" company="The original author or authors.">
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

using System.Text;
using MongoDB.Driver;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Integration tests for <see cref="MongoDatabaseFactoryParser"/>.
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class MongoDatabaseFactoryParserTests
    {
        [SetUp]
        public void SetUp()
        {
            NamespaceParserRegistry.RegisterParser(typeof(MongoNamespaceParser));
        }

	    [Test]
	    public void CreatesDbFactoryBean()
	    {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                           <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
	                         <mongo:mongo url='mongodb://localhost' />
                             <mongo:db-factory id='first' mongo-ref='Mongo' write-concern='WMajority' />
                           </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));

            var mongoDatabase = factory.GetObject("first");

            Assert.That(mongoDatabase, Is.Not.Null);
	    }

	    [Test]
	    public void ReadsReplicasWriteConcernCorrectly()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                           <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
                             <mongo:db-factory id='second' write-concern='W2' />
                           </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));
            
		    var dbFactory = factory.GetObject<IMongoDatabaseFactory>("second");
		    var db = dbFactory.GetDatabase();

		    Assert.That(db.Settings.WriteConcern, Is.EqualTo(WriteConcern.W2));
	    }

	    [Test]
	    public void SetsUpMongoDbFactoryUsingAMongoUrl()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                           <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
                             <mongo:db-factory url='mongodb://username:password@localhost/database' />
                           </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));

		    IObjectDefinition definition = factory.GetObjectDefinition("MongoDatabaseFactory");
		    ConstructorArgumentValues constructorArguments = definition.ConstructorArgumentValues;

		    Assert.That(constructorArguments.ArgumentCount, Is.EqualTo(1));

	        var argument = constructorArguments.GetArgumentValue(0, typeof (MongoUrl));

		    Assert.That(argument, Is.Not.Null);

	        var dbFactory = factory.GetObject<IMongoDatabaseFactory>();
	        var database = dbFactory.GetDatabase();

            Assert.That(database.Credentials.Username, Is.EqualTo("username"));
            Assert.That(database.Credentials.Password, Is.EqualTo("password"));
        }

	    [Test]
	    public void SetsUpMongoDbFactoryUsingAMongoConnectStringWithoutCredentials()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                           <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
                             <mongo:db-factory url='mongodb://localhost/database' />
                           </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));

		    IObjectDefinition definition = factory.GetObjectDefinition("MongoDatabaseFactory");
		    ConstructorArgumentValues constructorArguments = definition.ConstructorArgumentValues;

		    Assert.That(constructorArguments.ArgumentCount, Is.EqualTo(1));

		    var argument = constructorArguments.GetArgumentValue(0, typeof(MongoUrl));
		    
            Assert.That(argument, Is.Not.Null);

		    var dbFactory = factory.GetObject<IMongoDatabaseFactory>("MongoDatabaseFactory");
		    var db = dbFactory.GetDatabase();

		    Assert.That(db.Name, Is.EqualTo("database"));
	    }

	    [Test]
	    public void RejectsConnectStringPlusDetailedConfiguration()
	    {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                           <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
                             <mongo:db-factory url='mongodb://localhost/database' username='username' password='password'/>
                           </objects>";
            
            Assert.That(
	            delegate
	                {
                        var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));
	                },
	            Throws.TypeOf<ObjectDefinitionStoreException>());
	    }
    }
}
