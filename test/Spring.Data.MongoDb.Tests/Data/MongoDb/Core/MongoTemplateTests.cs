#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoTemplateTests.cs" company="The original author or authors.">
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
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NSubstitute;
using NUnit.Framework;
using Spring.Dao;
using Spring.Data.MongoDb.Core.HelperClasses;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Unit tests for <see cref="MongoTemplate"/>
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class MongoTemplateTests
    {
        private MongoTemplate _template;
        private IMongoDatabaseFactory _dbFactory;
        private MongoServer _mongo;
        private MongoDatabase _mongoDatabase;
        private MongoCollection<object> _mongoCollection;
        private CommandResult _okComandResult;
        private CommandResult _failComandResult;
        private FindAndModifyResult _okFindAndModifyResult;
        private FindAndModifyResult _failFindAndModifyResult;

        [SetUp]
        public void SetUp()
        {
            _mongo = MongoTestHelper.GetCachedMockMongoServer();
            _mongoDatabase = MongoTestHelper.GetCachedMockMongoDatabase("test", WriteConcern.Acknowledged);
            _mongoCollection = MongoTestHelper.CreateMockCollection<object>("test", "tests");

            CreateOkCommandResult();
            CreateFailCommandResult();
            CreateOkFindAndModifyResult();
            CreateFailFindAndModifyResult();

            _dbFactory = Substitute.For<IMongoDatabaseFactory>();
            _dbFactory.GetDatabase().Returns(_mongoDatabase);
            _mongoDatabase.GetCollection<object>(Arg.Any<string>()).Returns(_mongoCollection);

            _template = new MongoTemplate(_dbFactory);
        }

        [Test]
        public void CollectionExistsGeneric()
        {
            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.CollectionExists(Arg.Is("persons")).Returns(true);
            _mongoDatabase.CollectionExists(Arg.Is("notExists")).Returns(false);

            var exists = _template.CollectionExists<Person>();
            _mongoDatabase.Received(1).CollectionExists(Arg.Is("persons"));
            Assert.That(exists, Is.True);

            exists = _template.CollectionExists<NotExist>();
            _mongoDatabase.Received(1).CollectionExists(Arg.Is("notExists"));
            Assert.That(exists, Is.False);
        }

        [Test]
        public void CollectionExistsViaName()
        {
            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.CollectionExists(Arg.Is("persons")).Returns(true);
            _mongoDatabase.CollectionExists(Arg.Is("notExists")).Returns(false);

            var exists = _template.CollectionExists("persons");

            _mongoDatabase.Received(1).CollectionExists(Arg.Is("persons"));
            Assert.That(exists, Is.True);

            exists = _template.CollectionExists("notExists");
            _mongoDatabase.Received(1).CollectionExists(Arg.Is("notExists"));
            Assert.That(exists, Is.False);
        }

        [Test]
        public void CollectionExistsMustHaveCollectionName()
        {
            Assert.That(delegate
                {
                    _template.CollectionExists("");
                }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateCollectionViaGeneric()
        {
            var mongoCollection = MongoTestHelper.CreateMockCollection<Person>("unit", "persons");
            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.CreateCollection("persons").Returns(_okComandResult);
            _mongoDatabase.GetCollection<Person>("persons").Returns((MongoCollection) mongoCollection);

            MongoCollection collection = _template.CreateCollection<Person>();

            _mongoDatabase.Received(1).CreateCollection("persons");
            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            Assert.That(collection, Is.SameAs(mongoCollection));
        }

        [Test]
        public void CreateCollectionViaName()
        {
            var mongoCollection = MongoTestHelper.CreateMockCollection<Person>("unit", "persons");

            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.CreateCollection("persons").Returns(_okComandResult);
            _mongoDatabase.GetCollection<Person>("persons").Returns(mongoCollection);

            var collection = _template.CreateCollection<Person>("persons");

            _mongoDatabase.Received(1).CreateCollection("persons");
            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            Assert.That(collection, Is.SameAs(mongoCollection));
        }

        [Test]
        public void CreateCollectionWithOptions()
        {
            var options = CollectionOptions.SetMaxDocuments(20);

            var mongoCollection = MongoTestHelper.CreateMockCollection<Person>("unit", "persons");
            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.CreateCollection("persons", options).Returns(_okComandResult);
            _mongoDatabase.GetCollection<Person>("persons").Returns(mongoCollection);

            var collection = _template.CreateCollection<Person>("persons", options);

            _mongoDatabase.Received(1).CreateCollection("persons", options);
            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            Assert.That(collection, Is.EqualTo(mongoCollection));
        }

        [Test]
        public void CreateCollectionWithNotAllowedCollectionName()
        {
            Assert.That(delegate { _template.CreateCollection<Person>("my$names"); }, Throws.TypeOf<InvalidDataAccessResourceUsageException>());
            Assert.That(delegate { _template.CreateCollection<Person>("system.fun"); }, Throws.TypeOf<InvalidDataAccessResourceUsageException>());
            Assert.That(delegate { _template.CreateCollection<Person>("funny\0character"); }, Throws.TypeOf<InvalidDataAccessResourceUsageException>());
            Assert.That(delegate { _template.CreateCollection<Person>("012345678901234567890123456789012345678901234567890123456789012345678901234567891"); }, Throws.TypeOf<InvalidDataAccessResourceUsageException>());
        }

        [Test]
        public void CreateCollectionCommandResultOkFalse()
        {
            var mongoCollection = MongoTestHelper.CreateMockCollection<Person>("unit", "persons");

            _mongoDatabase.CreateCollection("jokes").Returns(_failComandResult);
            _mongoDatabase.GetCollection<Person>("jokes").Returns(mongoCollection);

            Assert.That(delegate { _template.CreateCollection<Person>("jokes"); }, Throws.TypeOf<InvalidDataAccessApiUsageException>());
        }


        [Test]
        public void CreateCollectionFailsIfNoCollectionName()
        {
            Assert.That(delegate
                {
                    _template.CreateCollection<Person>("");
                }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CountViaGeneric()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<BsonDocument> collection = MongoTestHelper.CreateMockCollection<BsonDocument>("integration", "persons");
            collection.Count().Returns(1);
            _mongoDatabase.GetCollection("persons").Returns(collection);


            long result = _template.Count<Person>();

            _mongoDatabase.Received(1).GetCollection("persons");
            collection.Received(1).Count();
        }

        [Test]
        public void CountViaName()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<BsonDocument> collection = MongoTestHelper.CreateMockCollection<BsonDocument>("integration", "persons");
            collection.Count().Returns(1);
            _mongoDatabase.GetCollection("persons").Returns(collection);


            long result = _template.Count("persons");

            _mongoDatabase.Received(1).GetCollection("persons");
            collection.Received(1).Count();
        }

        [Test]
        public void CountWithQueryViaGeneric()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");

            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<BsonDocument> collection = MongoTestHelper.CreateMockCollection<BsonDocument>("integration", "persons");
            collection.Count().Returns(1);
            _mongoDatabase.GetCollection("persons").Returns(collection);


            long result = _template.Count<Person>(query);

            _mongoDatabase.Received(1).GetCollection("persons");
            collection.Received(1).Count(query);
        }

        [Test]
        public void CountWithQueryViaName()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");

            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<BsonDocument> collection = MongoTestHelper.CreateMockCollection<BsonDocument>("integration", "persons");
            collection.Count().Returns(1);
            _mongoDatabase.GetCollection("persons").Returns(collection);


            long result = _template.Count("persons", query);

            _mongoDatabase.Received(1).GetCollection("persons");
            collection.Received(1).Count(query);
        }

        [Test]
        public void CountShouldFailWhenNoCollectionName()
        {
            Assert.That(delegate { _template.Count(""); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CountWithQueryShouldFailWhenNoCollectionName()
        {
            Assert.That(delegate { _template.Count("", null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CountWithQueryShouldFailWhenNoQueryProvided()
        {
            Assert.That(delegate { _template.Count("persons", null); }, Throws.TypeOf<ArgumentNullException>());            
        }

        [Test]
        public void DropCollectionViaGeneric()
        {
            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.DropCollection("persons").Returns(_okComandResult);

            _template.DropCollection<Person>();

            _mongoDatabase.Received(1).DropCollection("persons");
        }

        [Test]
        public void DropCollectionViaName()
        {
            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.DropCollection("persons").Returns(_okComandResult);

            _template.DropCollection("persons");

            _mongoDatabase.Received(1).DropCollection("persons");
        }

        [Test]
        public void DropCollectionMustHaveaCollectionName()
        {
            Assert.That(delegate { _template.DropCollection(""); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void DropCollectionFailsWhenCollectionDoesNotExists()
        {
            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.DropCollection("funny").Returns(_failComandResult);

            Assert.That(delegate { _template.DropCollection("funny"); }, Throws.TypeOf<InvalidDataAccessApiUsageException>());
        }

        [Test]
        public void FindAllViaGeneric()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);

            IList<Person> result = _template.FindAll<Person>();

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAllAs<Person>();
            Assert.That(result, Is.EqualTo(GenerateDummyDataPerson()));
        }

        [Test]
        public void FindAllViaName()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);

            IList<Person> result = _template.FindAll<Person>("persons");

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAllAs<Person>();
            Assert.That(result, Is.EqualTo(GenerateDummyDataPerson()));
        }

        [Test]
        public void FindAllFailsIfNoCollectionNameProvided()
        {
            Assert.That(delegate { _template.FindAll<Person>(""); }, Throws.TypeOf<ArgumentNullException>());            
        }

        [Test]
        public void FindByIdViaGeneric()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);

            _template.FindById<Person>(1);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindOneByIdAs<Person>(1);            
        }

        [Test]
        public void FindByIdViaName()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);

            _template.FindById<Person>("persons", 1);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindOneByIdAs<Person>(1);
        }

        [Test]
        public void FindByIdFailsIfIdIsNull()
        {
            Assert.That(delegate { _template.FindById<Person>(null); }, Throws.TypeOf < ArgumentNullException>());
            Assert.That(delegate { _template.FindById<Person>("Person", null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindByIdlFailsIfNoCollectionNameProvided()
        {
            Assert.That(delegate { _template.FindById<Person>("", 0); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindOneViaGeneric()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);


            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");

            _template.FindOne<Person>(query);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindOneAs<Person>(query);                        
        }

        [Test]
        public void FindOneViaName()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);

            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");

            _template.FindOne<Person>("persons", query);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindOneAs<Person>(query);                        
        }

        [Test]
        public void FindOneShouldFailIfNoCollectionNameProvided()
        {
            Assert.That(delegate { _template.FindOne<Person>("", null); }, Throws.TypeOf<ArgumentNullException>());            
        }

        [Test]
        public void FindOneShouldFailIfNoQueryProvided()
        {
            Assert.That(delegate { _template.FindOne<Person>("Person", null); }, Throws.TypeOf<ArgumentNullException>());                        
        }

        [Test]
        public void FindViaGeneric()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");


            _template.Find<Person>(query);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAs<Person>(query);
        }

        [Test]
        public void FindViaName()
        {
            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");

            _template.Find<Person>("persons", query);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAs<Person>(query);
        }

        [Test]
        public void FindShouldFailIfNoCollectionNameProvided()
        {
            Assert.That(delegate { _template.Find<Person>("", null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindShouldFailIfNoQueryProvided()
        {
            Assert.That(delegate { _template.Find<Person>("Person", null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindAndModifyViaGeneric()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");
            IMongoSortBy sortBy = new SortByBuilder<Person>().Ascending(p => p.Id);
            IMongoUpdate update = new UpdateBuilder<Person>().Set(p => p.FirstName, "Thomas T");

            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);
            collection.FindAndModify(query, sortBy, update, false, false).Returns(_okFindAndModifyResult);


            _template.FindAndModify<Person>(query, sortBy, update);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAndModify(query, sortBy, update, false, false);            
        }

        [Test]
        public void FindAndModifyViaName()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");
            IMongoSortBy sortBy = new SortByBuilder<Person>().Ascending(p => p.Id);
            IMongoUpdate update = new UpdateBuilder<Person>().Set(p => p.FirstName, "Thomas T");

            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);
            collection.FindAndModify(query, sortBy, update, false, false).Returns(_okFindAndModifyResult);


            _template.FindAndModify<Person>("persons", query, sortBy, update);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAndModify(query, sortBy, update, false, false);
        }

        [Test]
        public void FindAndModifyWithUpsert()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");
            IMongoSortBy sortBy = new SortByBuilder<Person>().Ascending(p => p.Id);
            IMongoUpdate update = new UpdateBuilder<Person>().Set(p => p.FirstName, "Thomas T");
            FindAndModifyOptions options = FindAndModifyOptions.Default();
            options.Upsert(true);

            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);
            collection.FindAndModify(query, sortBy, update, false, true).Returns(_okFindAndModifyResult);


            _template.FindAndModify<Person>("persons", query, sortBy, update, options);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAndModify(query, sortBy, update, false, true);
        }

        [Test]
        public void FindAndModifyWithReturnNew()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");
            IMongoSortBy sortBy = new SortByBuilder<Person>().Ascending(p => p.Id);
            IMongoUpdate update = new UpdateBuilder<Person>().Set(p => p.FirstName, "Thomas T");
            FindAndModifyOptions options = FindAndModifyOptions.Default();
            options.ReturnNew(true);

            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);
            collection.FindAndModify(query, sortBy, update, true, false).Returns(_okFindAndModifyResult);


            _template.FindAndModify<Person>("persons", query, sortBy, update, options);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAndModify(query, sortBy, update, true, false);
        }
        
        [Test]
        public void FindAndModifyShouldFailIfNoCollectionNameProvided()
        {
            Assert.That(delegate { _template.FindAndModify<Person>("", null, null, null, null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindAndModifyShouldFailIfNoQueryProvided()
        {
            Assert.That(delegate { _template.FindAndModify<Person>("Person", null, null, null, null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindAndModifyShouldFailIfNoSortByProvided()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");

            Assert.That(delegate { _template.FindAndModify<Person>("Person", query, null, null, null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindAndModifyShouldFailIfNoUpdateProvided()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");
            IMongoSortBy sortBy = new SortByBuilder<Person>().Ascending(p => p.FirstName);

            Assert.That(delegate { _template.FindAndModify<Person>("Person", query, sortBy, null, null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindAndRemoveViaGeneric()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");
            IMongoSortBy sortBy = new SortByBuilder<Person>().Ascending(p => p.Id);

            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            collection.FindAndRemove(query, sortBy).Returns(_okFindAndModifyResult);
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);


            _template.FindAndRemove<Person>(query, sortBy);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAndRemove(query, sortBy);
        }

        [Test]
        public void FindAndRemoveViaName()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");
            IMongoSortBy sortBy = new SortByBuilder<Person>().Ascending(p => p.Id);

            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);
            collection.FindAndRemove(query, sortBy).Returns(_okFindAndModifyResult);


            _template.FindAndRemove<Person>("persons", query, sortBy);

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            collection.Received(1).FindAndRemove(query, sortBy);
        }

        [Test]
        public void FindAndRemoveWithFailure()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");
            IMongoSortBy sortBy = new SortByBuilder<Person>().Ascending(p => p.Id);

            _mongoDatabase.ClearReceivedCalls();
            MongoCollection<Person> collection = MongoTestHelper.CreateMockCollection<Person>("integration", "persons");
            collection.ReturnsCollection(GenerateDummyDataPerson());
            _mongoDatabase.GetCollection<Person>("persons").Returns(collection);
            collection.FindAndRemove(query, sortBy).Returns(_failFindAndModifyResult);

            Assert.That(delegate { _template.FindAndRemove<Person>("persons", query, sortBy); },
                Throws.TypeOf<InvalidDataAccessApiUsageException>());
        }

        [Test]
        public void FindAndRemoveShouldFailIfNoCollectionNameProvided()
        {
            Assert.That(delegate { _template.FindAndRemove<Person>("", null, null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindAndRemoveShouldFailIfNoQueryProvided()
        {
            Assert.That(delegate { _template.FindAndRemove<Person>("Person", null, null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FindAndRemoveShouldFailIfNoSortByProvided()
        {
            IMongoQuery query = new QueryBuilder<Person>().Where(p => p.FirstName == "Thomas");

            Assert.That(delegate { _template.FindAndRemove<Person>("Person", query, null); }, Throws.TypeOf<ArgumentNullException>());
        }
        
        [Test]
        public void GetCollectionViaGeneric()
        {
            var mongoCollection = MongoTestHelper.CreateMockCollection<Person>("unit", "persons");

            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.GetCollection<Person>("persons").Returns(mongoCollection);

            var collection = _template.GetCollection<Person>();

            _mongoDatabase.Received(1).GetCollection<Person>("persons");
            Assert.That(collection, Is.SameAs(mongoCollection));                        
        }

        [Test]
        public void GetCollectionByName()
        {
            var mongoCollection = MongoTestHelper.CreateMockCollection<Person>("unit", "jokes");

            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.GetCollection<Person>("jokes").Returns(mongoCollection);

            var collection = _template.GetCollection<Person>("jokes");

            _mongoDatabase.Received(1).GetCollection<Person>("jokes");
            Assert.That(collection, Is.SameAs(mongoCollection));                                    
        }

        [Test]
        public void GetCollectionFailIfCollectionNameIsEmpty()
        {
            Assert.That(delegate { _template.GetCollection<Person>(""); }, Throws.TypeOf<ArgumentNullException>());            
        }

        [Test]
        public void GetCollectionThatWithNotAllowedCollectionName()
        {
            Assert.That(delegate { var collection = _template.GetCollection<Person>("jokes$notSoFunny"); }, Throws.TypeOf<InvalidDataAccessResourceUsageException>());
        }

        [Test]
        public void GetCollectionThrowsExceptionIfErrorHappen()
        {
            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.GetCollection<Person>("jokes").Returns(x => { throw new MongoInternalException("Error"); });

            Assert.That(delegate { _template.GetCollection<Person>("jokes"); }, Throws.TypeOf<InvalidDataAccessResourceUsageException>());
            _mongoDatabase.Received(1).GetCollection<Person>("jokes");
        }

        [Test]
        public void GetCollectioNames()
        {
            _mongoDatabase.ClearReceivedCalls();
            _mongoDatabase.GetCollectionNames().Returns(new List<string>() { "persons", "nerds", "geeks" });

            var names = _template.GetCollectionNames();

            _mongoDatabase.Received(1).GetCollectionNames();
            Assert.That(names, Is.Not.Null);
            Assert.That(names, Has.Count.EqualTo(3));
        }

        [Test]
        public void GetDatabaseReturnsDbFactoryDatabase()
        {
            _template.GetDatabase();

            _dbFactory.Received(1).GetDatabase();
        }

        [Test]
        public void GetDatabasefailsWhenNoDatabase()
        {
            _dbFactory.GetDatabase().Returns((MongoDatabase)null);

            Assert.That(delegate { _template.GetDatabase(); }, Throws.TypeOf<DataAccessResourceFailureException>());
        }

        [Test]
        public void RejectsNullDatabaseName()
        {
            Assert.That(delegate
                {
                    var template = new MongoTemplate(_mongo, null);
                },
                        Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void RejectsNullMongo()
        {
            Assert.That(delegate
                {
                    var template = new MongoTemplate(null, "database");
                },
                        Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetCollectionNameViaTypeName()
        {
            var collectionName = _template.GetCollectionName<Person>();

            Assert.That(collectionName, Is.EqualTo("persons"));
        }

        [Test]
        public void GetCollectionNameViaAttribute()
        {
            var collectionName = _template.GetCollectionName<PersonWithAttribute>();

            Assert.That(collectionName, Is.EqualTo("persons"));
        }

        [Test]
        public void ShouldGetIdFieldForQuery()
        {
            IMongoQuery query = _template.GetIdQueryFor(new Person("TT"));

            Assert.That(query, Is.Not.Null);
            Assert.That(query.ToString(), Is.EqualTo("{ \"_id\" : ObjectId(\"000000000000000000000000\") }"));
        }

        [Test]
        public void RemoveHandlesMongoExceptionProperly()
        {
            _mongoDatabase.GetCollection("collection").Returns(x => { throw new Exception("Exception"); });

            Assert.That(delegate { _template.Remove<Person>((Person)null); }, Throws.InstanceOf<ArgumentNullException>());
            Assert.That(delegate { _template.Remove<Person>("collection", (Person)null); }, Throws.InstanceOf<ArgumentNullException>());
            Assert.That(delegate { _template.Remove<Person>((string)null, (Person)null); }, Throws.InstanceOf<ArgumentNullException>());
            Assert.That(delegate { _template.Remove<Person>((IMongoQuery)null); }, Throws.InstanceOf<ArgumentNullException>());
            Assert.That(delegate { _template.Remove<Person>("collection", (IMongoQuery)null); }, Throws.InstanceOf<ArgumentNullException>());
            Assert.That(delegate { _template.Remove<Person>((string)null, (IMongoQuery)null); }, Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void ExecuteRejectsNullForCollectionCallback()
        {
            Assert.That(delegate
                {
                    _template.Execute<Person, object>("collection", null);
                }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ExecuteRejectsNullForCollectionNameMissing()
        {
            Assert.That(delegate
                {
                    _template.Execute<Person, object>((string) null, null);
                }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void RemoveExecutesExecuteWithCollectionCallback()
        {
            var template = new TestMongoTemplate(_dbFactory);

            template.Remove(new Person(new ObjectId("000000000000000000000001"), "Thomas"));

            Assert.That(template.WasExecuted, Is.True);
            Assert.That(template.CollectionName, Is.EqualTo("persons"));
            Assert.That(template.Func, Is.Not.Null);
        }

        private void CreateOkCommandResult()
        {
            var response = new BsonDocument();
            response.Add("ok", BsonValue.Create(true));
            _okComandResult = new CommandResult(null, response);
        }

        private void CreateFailCommandResult()
        {
            var response = new BsonDocument();
            response.Add("ok", BsonValue.Create(false));
            response.Add("errmsg", BsonValue.Create("Error happen from time to time"));
            _failComandResult = new CommandResult(null, response);
        }

        private void CreateOkFindAndModifyResult()
        {
            var response = new BsonDocument();
            response.Add("ok", BsonValue.Create(true));
            response.Add("value", (new Person()).ToBsonDocument());
            _okFindAndModifyResult = new FindAndModifyResult();
            _okFindAndModifyResult.Initialize(null, response);
        }

        private void CreateFailFindAndModifyResult()
        {
            var response = new BsonDocument();
            response.Add("ok", BsonValue.Create(false));
            response.Add("errmsg", BsonValue.Create("Error happen from time to time"));
            _failFindAndModifyResult = new FindAndModifyResult();
            _failFindAndModifyResult.Initialize(null, response);
        }

        private IList<Person> GenerateDummyDataPerson()
        {
            return new List<Person>()
                {
                    new Person(new ObjectId("000000000000000000000001"), "Thomas1"),
                    new Person(new ObjectId("000000000000000000000002"), "Thomas2"),
                    new Person(new ObjectId("000000000000000000000003"), "Thomas3")
                };
        }

        public class NotExist
        {
            public string Id { get; set; }
        }

    }

    public class TestMongoTemplate : MongoTemplate
    {
        private string _collectionName;
        private Type _returnType;
        private object _func;
        private bool _execute;

        public TestMongoTemplate(IMongoDatabaseFactory factory)
            : base(factory)
        {
        }

        public override TReturn Execute<T, TReturn>(string collectionName,
                                                 Func<MongoCollection, TReturn> collectionCallback)
        {
            _collectionName = collectionName;
            _func = collectionCallback;
            _returnType = typeof (TReturn);
            _execute = true;
            return default(TReturn);
        }

        public bool WasExecuted
        {
            get { return _execute; }
        }

        public string CollectionName
        {
            get { return _collectionName; }
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public object Func
        {
            get { return _func; }
        }
    }
}
