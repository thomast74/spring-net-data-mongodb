#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoTemplateIntegrationTests.cs" company="The original author or authors.">
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

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;
using Spring.Dao;
using Spring.Data.MongoDb.Core.HelperClasses;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Integration Tests for <see cref="MongoTemplate"/>
    /// </summary>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class MongoTemplateIntegrationTests
    {
        private IMongoDatabaseFactory _dbFactory;
        private MongoTemplate _template;
        private MongoDatabase _database;

        [SetUp]
        public void SetUp()
        {
            var client = new MongoClient(new MongoUrl("mongodb://localhost"));
            _dbFactory = new SimpleMongoDatabaseFactory(client.GetServer(), "integrationTests");
            _database = _dbFactory.GetDatabase();
            SetupTestData(_database);
            
            _template = new MongoTemplate(_dbFactory);
        }

        [TearDown]
        public void TearDown()
        {
            _database.Drop();
        }

        [Test]
        public void CollectionExists()
        {
            var exists = _template.CollectionExists<Person>();
            Assert.That(exists, Is.True);

            exists = _template.CollectionExists("persons");
            Assert.That(exists, Is.True);

            exists = _template.CollectionExists("notSoFunny");
            Assert.That(exists, Is.False);
        }

        [Test]
        public void CreateCollectionWithDefaultOptions()
        {
            _template.CreateCollection<Person>("jokes");

            Assert.That(_database.CollectionExists("jokes"));
        }

        [Test]
        public void CreateCollectionWithOptions()
        {
            var options = CollectionOptions.SetCapped(true).SetMaxSize(8192);

            var jokes = _template.CreateCollection<Person>("jokes", options);
            Assert.That(jokes, Is.Not.Null);
            Assert.That(jokes.Name, Is.EqualTo("jokes"));
            Assert.That(jokes.IsCapped(), Is.True);
        }

        [Test]
        public void CreateCollectionThrowsExceptionIfAlreadyExsist()
        {
            Assert.That(delegate { _template.CreateCollection<Person>("persons"); }, Throws.TypeOf<InvalidDataAccessApiUsageException>());
        }

        [Test]
        public void DropCollectionViaGeneric()
        {
            _template.DropCollection<Person>();

            Assert.That(_database.CollectionExists("persons"), Is.False);
        }

        [Test]
        public void DropCollectionViaName()
        {
            _template.DropCollection("persons");

            Assert.That(_database.CollectionExists("persons"), Is.False);
        }

        [Test]
        public void DropCollectionThatDoesNotExists()
        {
            Assert.That(delegate { _template.DropCollection("funny"); }, Throws.TypeOf<InvalidDataAccessApiUsageException>());
        }

        [Test]
        public void FindAllViaGeneric()
        {
            IList<Person> result = _template.FindAll<Person>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(5));
            Assert.That(result[0].Id, Is.EqualTo(new ObjectId("000000000000000000000001")));
            Assert.That(result[0].FirstName, Is.EqualTo("Thomas"));
        }

        [Test]
        public void FindAllViaName()
        {
            IList<Person> result = _template.FindAll<Person>("persons");

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(5));
            Assert.That(result[0].Id, Is.EqualTo(new ObjectId("000000000000000000000001")));
            Assert.That(result[0].FirstName, Is.EqualTo("Thomas"));            
        }

        [Test]
        public void FindByIdShouldReturnPerson()
        {
            Person person = _template.FindById<Person>(new ObjectId("000000000000000000000001"));
            
            Assert.That(person, Is.Not.Null);
            Assert.That(person.Id, Is.EqualTo(new ObjectId("000000000000000000000001")));
            Assert.That(person.FirstName, Is.EqualTo("Thomas"));
        }

        [Test]
        public void FindByIdShouldReturnNullIfNotFound()
        {
            Person person = _template.FindById<Person>(new ObjectId("000000000000000000000010"));

            Assert.That(person, Is.Null);
        }

        [Test]
        public void FindOneShouldReturnPerson()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");

            Person person = _template.FindOne<Person>(query);

            Assert.That(person, Is.Not.Null);
            Assert.That(person.Id, Is.EqualTo(new ObjectId("000000000000000000000001")));
            Assert.That(person.FirstName, Is.EqualTo("Thomas"));
        }

        [Test]
        public void FindOneShouldReturnNullIfNotFound()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Liar");

            Person person = _template.FindOne<Person>(query);

            Assert.That(person, Is.Null);
        }

        [Test]
        public void GetCollectionViaGeneric()
        {
            var collection = _template.GetCollection<Person>();

            Assert.That(collection, Is.Not.Null);
            Assert.That(collection.Name, Is.EqualTo("persons"));
        }

        [Test]
        public void GetCollectionByName()
        {
            var collection = _template.GetCollection<Person>("persons");

            Assert.That(collection, Is.Not.Null);
            Assert.That(collection.Name, Is.EqualTo("persons"));
        }

        [Test]
        public void GetCollectionNames()
        {
            var names = _template.GetCollectionNames();

            Assert.That(names, Is.Not.Null);
            Assert.That(names, Has.Count.EqualTo(4), StringUtils.ArrayToDelimitedString(names, ","));
            Assert.That(names, Is.EqualTo(new List<string> {"geeks", "nerds", "persons", "system.indexes"}));
        }

        [Test]
        public void GetDatabaseFromDbFactory()
        {
            var database = _template.GetDatabase();

            Assert.That(database, Is.Not.Null);
            Assert.That(database.Name, Is.EqualTo("integrationTests"));
        }

        [Test]
        public void RemoveAPersonFromCollectionViaObject()
        {
            _template.Remove(new Person(new ObjectId("000000000000000000000001"), "Thomas"));

            var collection = _database.GetCollection<Person>("persons");

            Assert.That(collection.Count(), Is.EqualTo(4));
            Assert.That(collection.Find(Query<Person>.Where(p => p.FirstName == "Thomas")).Count(), Is.EqualTo(0));
        }

        [Test]
        public void RemoveAPersonFromCollectionViaQuery()
        {
            _template.Remove<Person>(Query.EQ("_id", BsonValue.Create(new ObjectId("000000000000000000000001"))));

            var collection = _database.GetCollection<Person>("persons");

            Assert.That(collection.Count(), Is.EqualTo(4));
            Assert.That(collection.Find(Query<Person>.Where(p => p.FirstName == "Thomas")).Count(), Is.EqualTo(0));
        }
        
        private void SetupTestData(MongoDatabase database)
        {
            database.CreateCollection("persons");
            database.CreateCollection("nerds");
            database.CreateCollection("geeks");

            var collection = database.GetCollection<Person>("persons");
            collection.RemoveAll();

            collection.Insert(new Person(new ObjectId("000000000000000000000001"), "Thomas"));
            collection.Insert(new Person(new ObjectId("000000000000000000000002"), "Bhawani"));
            collection.Insert(new Person(new ObjectId("000000000000000000000003"), "Ulrike"));
            collection.Insert(new Person(new ObjectId("000000000000000000000004"), "Barbara"));
            collection.Insert(new Person(new ObjectId("000000000000000000000005"), "Armin"));
        }
    }
}
