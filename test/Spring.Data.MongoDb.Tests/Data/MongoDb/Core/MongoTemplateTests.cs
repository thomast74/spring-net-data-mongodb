// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoTemplateTests.cs" company="The original author or authors.">
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
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NSubstitute;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Dao;
using Spring.Data.Annotation;
using Spring.Data.Mapping.Context;
using Spring.Data.MongoDb.Core.Convert;
using Spring.Data.MongoDb.Core.Mapping;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Unit tests for <see cref="MongoTemplate"/>.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class MongoTemplateTests : MongoOperationsTests
    {
        MongoTemplate _template;

        private IMongoDbFactory _factory;
        private MongoServer mongo;
        private MongoDatabase _db;
        private MongoCollection<BsonDocument> _collection;

	    //MappingMongoConverter converter;
	    MongoMappingContext mappingContext;

        [SetUp]
	    public void SetUp()
        {
            _collection = MongoTestHelper.CreateMockCollection<BsonDocument>();

            _db = _collection.Database;
            _db.GetCollection(Arg.Any<string>()).Returns(_collection);

            _factory = Substitute.For<IMongoDbFactory>();
            _factory.GetDatabase().Returns(_db);

		    mappingContext = new MongoMappingContext();
		    //converter = new MappingMongoConverter(_factory, mappingContext);
		    //_template = new MongoTemplate(_factory, converter);
	    }

        /* TODO Make it compile
         * 
	    [Test]
	    public void RejectsNullDatabaseName() 
        {
            Assert.That(delegate { new MongoTemplate(mongo, null); }, Throws.TypeOf<ArgumentNullException>());   
	    }

	    [Test]
	    public void RejectsNullMongo()
        {
            Assert.That(delegate { new MongoTemplate(null, "database"); }, Throws.TypeOf<ArgumentNullException>());   
	    }

	    [Test]
	    public void RemoveHandlesMongoExceptionProperly() 
        {
		    MongoTemplate template = MockOutGetDb();
	        _dbMock.Setup(d => d.GetCollection("collection")).Throws(new MongoException("Exception!"));
	        _db = _dbMock.Object;

	        Assert.That(delegate { template.Remove(null, "collection") }, Throws.TypeOf<DataAccessException>());
        }

	    [Test]
	    public void DefaultsConverterToMappingMongoConverter()
        {
		    MongoTemplate template = new MongoTemplate(mongo, "database");
	        var field = ReflectionTestUtils.getField(template, "mongoConverter");
	        Assert.That(field, Is.TypeOf<IMappingContext>());
	    }

	    [Test]
	    public void RejectsNotFoundMapReduceResource() 
        {
		    _template.ApplicationContext(new GenericApplicationContext());
	        Assert.That(delegate
	            {
	                _template.MapReduce("foo", "classpath:doesNotExist.js", "function() {}",
	                                   typeof (Person));
	            }, Throws.TypeOf<InvalidDataAccessApiUsageException>());
        }

	    [Test]
	    public void RejectsEntityWithNullIdIfNotSupportedIdType() 
        {
		    object entity = new NotAutogenerateableId();

	        Assert.That(delegate { _template.Save(entity); }, Throws.TypeOf<InvalidDataAccessApiUsageException>());
        }

	    [Test]
	    public void StoresEntityWithSetIdAlthoughNotAutogenerateable() 
        {
		    var entity = new NotAutogenerateableId { id = 1 };

	        _template.Save(entity);
	    }

	    [Test]
	    public void AutogeneratesIdForEntityWithAutogeneratableId() 
        {
		    converter.AfterPropertiesSet();

		    MongoTemplate template = spy(this._template);
		    doReturn(new ObjectId()).when(template).saveDBObject(Mockito.any(String.class), Mockito.any(DBObject.class),
				    Mockito.any(Class.class));

		    var entity = new AutogenerateableId();
		    template.Save(entity);

		    Assert.That(entity.id, Is.Not.Null);
	    }

	    [Test]
	    public void ConvertsUpdateConstraintsUsingConverters() 
        {
		    CustomConversions conversions = new CustomConversions(Collections.singletonList(MyConverter.INSTANCE));
		    this.converter.setCustomConversions(conversions);
		    this.converter.afterPropertiesSet();

		    Query query = new Query();
		    Update update = new Update().set("foo", new AutogenerateableId());

		    _template.UpdateFirst(query, update, Wrapper.class);

		    QueryMapper queryMapper = new QueryMapper(converter);
		    DBObject reference = queryMapper.GetMappedObject(update.UpdateObject, null);

		    verify(_collection, times(1)).update(Mockito.any(DBObject.class), eq(reference), anyBoolean(), anyBoolean());
	    }

	    [Test]
	    public void SetsUnpopulatedIdField() 
        {
		    NotAutogenerateableId entity = new NotAutogenerateableId();
		    _template.PopulateIdIfNecessary(entity, 5);

		    Assert.That(entity.id, Is.EqualTo(5));
	    }

	    [Test]
	    public void DoesNotSetAlreadyPopulatedId() 
        {
		    var entity = new NotAutogenerateableId();
		    entity.id = 5;

		    _template.PopulateIdIfNecessary(entity, 7);

		    Assert.That(entity.id, Is.EqualTo(5));
	    }

	    [Test]
	    public void RegistersDefaultEntityIndexCreatorIfApplicationContextHasOneForDifferentMappingContext() 
        {
		    GenericApplicationContext applicationContext = new GenericApplicationContext();
	        applicationContext.ObjectFactory.RegisterSingleton("foo",
	                                                           new MongoPersistentEntityIndexCreator(
	                                                               new MongoMappingContext(), _factory));

		    MongoTemplate mongoTemplate = new MongoTemplate(_factory, converter);
		    mongoTemplate.ApplicationContext = applicationContext;

		    Collection<ApplicationListener<>> listeners = applicationContext.getApplicationListeners();
		    Assert.That(listeners, Has.Count.EqualTo(1));

		    ApplicationListener<> listener = listeners.iterator().next();
		    Assert.That(listener, Is.TypeOf<MongoPersistentEntityIndexCreator>());

		    MongoPersistentEntityIndexCreator creator = (MongoPersistentEntityIndexCreator) listener;
		    Assert.That(creator.IsIndexCreatorFor(mappingContext), Is.True);
	    }

        private class AutogenerateableId
        {
            [Id] public BigInteger id;
        }

        private class NotAutogenerateableId
        {
            [Id] public int id;

            public Regex Id
            {
                get { return new Regex("."); }
            }
        }

        struct MyConverter : Converter<AutogenerateableId, string>
        {
		    public string Convert(AutogenerateableId source) 
            {
			    return source.ToString();
		    }
	    }

	    class Wrapper 
        {
		    AutogenerateableId foo;
	    }
        */

        /// <summary>
        /// Mocks out the <see cref="MongoTemplate.Database"/> method to return the <see cref="MongoDatabase"/> mock instead of executing 
        /// the actual behaviour.
        /// </summary>
	    private MongoTemplate MockOutGetDb()
        {
		    var templateMock = Substitute.For<MongoTemplate>();
	        templateMock.Database.Returns(_db);
            return templateMock;
        }

	    public override IMongoOperations GetOperationsForExceptionHandling()
	    {
	        var templateMock = Substitute.For<MongoTemplate>();
            templateMock.Database.Returns(x => { throw new MongoException("Error!"); });
            return templateMock;
	    }

	    public override IMongoOperations GetOperations()
        {
		    return _template;
	    }
    }
}
