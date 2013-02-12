#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoOperationsTests.cs" company="The original author or authors.">
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
#endregion

using System;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using Spring.Dao;
using System.Collections.Generic;
using Spring.Data.Mapping.Context;
using Spring.Data.MongoDb.Core.Convert;
using Spring.Data.MongoDb.Core.Geo;
using Spring.Data.MongoDb.Core.Mapping;
using Spring.Data.MongoDb.Core.Query;

namespace Spring.Data.MongoDb.Core
{
    [TestFixture]
    [Category(TestCategory.Unit)]
    public abstract class MongoOperationsTests
    {
        public delegate void DoWith(IMongoOperations operations);

	    private IMongoConverter _converter;
	    private Person _person;
	    private List<Person> _persons;

        [SetUp]
	    public void OperationsSetUp()
        {
		    _person = new Person("Oliver");
            _persons = new List<Person> {_person};
            _converter = new AbstractMongoConverter(this);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
	    public void RejectsNullForCollectionCallback()
        {
            GetOperations().Execute<object>("test", collection => null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
	    public void RejectsNullForCollectionCallback2()
        {
		    GetOperations().Execute<object>("collection", collection => null);
	    }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
	    public void RejectsNullForDbCallback()
        {
            GetOperations().Execute<object>(delegate(MongoDatabase database) { return null; });
	    }

        [Test]
	    public void ConvertsExceptionForCollectionExists()
        {
            DoWith d = operations => operations.CollectionExists("foo");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForCreateCollection()
	    {
	        DoWith d = operations => operations.CreateCollection("foo");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForCreateCollection2()
        {
            DoWith d = operations => operations.CreateCollection("foo", new CollectionOptions(1, 1, true));
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForDropCollection()
        {
            DoWith d = operations => operations.DropCollection("foo");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForExecuteCollectionCallback() 
        {
            DoWith d = operations => operations.Execute<object>("test", collection => null);
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForExecuteDbCallback() 
        {
            DoWith d = operations => operations.Execute<object>(delegate(MongoDatabase database) { return null; });
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForExecuteCollectionCallbackAndCollection()
        {
			DoWith d = operations => operations.Execute<object>("collection", null);
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForExecuteCommand() 
        {
    		DoWith d = operations => operations.ExecuteCommand(new CommandDocument());
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForExecuteStringCommand() 
        {
			DoWith d = operations => operations.ExecuteCommand("");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForExecuteInSession()
        {
            DoWith d = operations => operations.ExecuteInSession<object>(database => null);
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForGetCollection()
        {
			DoWith d = operations => operations.FindAll<object>();
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForGetCollectionWithCollectionName()
        {
			DoWith d = operations => operations.GetCollection("collection");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForGetCollectionWithCollectionNameAndType() 
        {
			DoWith d = operations => operations.FindAll<object>("collection");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForGetCollectionWithCollectionNameTypeAndReader()
        {
			DoWith d = operations => operations.FindAll<object>("collection");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForGetCollectionNames()
        {
			DoWith d = operations => operations.GetCollectionNames();
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForInsert()
        {
			DoWith d = operations => operations.Insert(_person);
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForInsert2() 
        {
			DoWith d = operations => operations.Insert(_person, "collection");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForInsertList()
        {
			DoWith d = operations => operations.InsertAll(_persons);
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void ConvertsExceptionForGetInsertList2() 
        {
			DoWith d = operations => operations.Insert(_persons, "collection");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void GeoNearRejectsNullNearQuery() 
        {
			DoWith d = operations => operations.GeoNear<Person>(null);
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void GeoNearRejectsNullNearQueryifCollectionGiven()
        {
			DoWith d = operations => operations.GeoNear<Person>(null, "collection");
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void GeoNearRejectsNullEntityClass()
        {
		    NearQuery query = NearQuery.Near(new Point(10, 20));

			DoWith d = operations => operations.GeoNear<object>(query);
            new Execution(this).AssertDataAccessException(d);
	    }

	    [Test]
	    public void GeoNearRejectsNullEntityClassIfCollectionGiven()
        {
		    NearQuery query = NearQuery.Near(new Point(10, 20));

			DoWith d = operations => operations.GeoNear<object>(query, "collection");
            new Execution(this).AssertDataAccessException(d);
	    }

        private class Execution
        {
            private readonly MongoOperationsTests _parent;

            public Execution(MongoOperationsTests parent)
            {
                _parent = parent;
            }
		    public void AssertDataAccessException(DoWith d)
            {
			    AssertException(d, typeof(DataAccessException));
		    }

		    private void AssertException(DoWith doWith, Type typeException)
            {
			    try
			    {
				    doWith(_parent.GetOperationsForExceptionHandling());
				    Assert.Fail("Expected " + typeException.Name + " but completed without any!");
			    } 
                catch (Exception ex)
                {
				    Assert.That(ex, Is.TypeOf(typeException), "Expected " + typeException.Name + " but got " + ex.GetType().Name);
			    }
		    }
	    }

        /// <summary>
        /// Expects an <see cref="IMongoOperations"/> instance that will be used to check that invoking
        /// methods on it will only cause <see cref="DataAccessException"/>s.
        /// </summary>
        /// <returns>An instance of MongoOperations</returns>
	    public abstract IMongoOperations GetOperationsForExceptionHandling();

        /// <summary>
        /// Gets a plain Mongo Operations instance
        /// </summary>
        /// <returns>A plain <see cref="IMongoOperations"/> instance</returns>
	    public abstract IMongoOperations GetOperations();




        private class AbstractMongoConverter : IMongoConverter
        {
            private readonly MongoOperationsTests _parent;

            public AbstractMongoConverter(MongoOperationsTests parent)
            {
                _parent = parent;
            }

            public void Write(object t, BsonDocument dbo)
            {
                dbo.Add("firstName", _parent._person.Firstname);
            }

            public T Read<T>(BsonDocument dbo)
            {
                return (T)(object)_parent._person;
            }

            public IMappingContext MappingContext
            {
                get { return null; }
            }

            public object ConvertToMongoType(object obj)
            {
                return null;
            }

            public MongoDBRef ToDBRef(object obj, IMongoPersistentProperty referingProperty)
            {
                return null;
            }
        }

    }
}
