#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoOperationsTests.cs" company="The original author or authors.">
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
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;
using Spring.Dao;
using Spring.Data.MongoDb.Core.HelperClasses;

namespace Spring.Data.MongoDb.Core
{
    [TestFixture]
    [Category(TestCategory.Unit)]
    public abstract class MongoOperationsTests
    {
        public delegate void DoWith(IMongoOperations operations);

        private Person _person;
        private List<Person> _persons;

        [SetUp]
        public void OperationsSetUp()
        {
            _person = new Person("Oliver");
            _persons = new List<Person> { _person };
        }

        [Test]
        public void RejectsNullForCollectionCallback()
        {
            Assert.That(delegate
                {
                    GetOperations().Execute<Person, object>("test", collection => null);
                }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void RejectsNullForDbCallback()
        {
            Assert.That(delegate
                {
                    GetOperations().Execute<object>(db => null);
                }, Throws.TypeOf<ArgumentNullException>());
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
            DoWith d = operations => operations.CreateCollection<Person>("foo");
            new Execution(this).AssertDataAccessException(d);
        }

        [Test]
        public void ConvertsExceptionForCreateCollection2()
        {
            var options = new CollectionOptionsBuilder().SetMaxSize(1).SetMaxDocuments(1).SetCapped(true);

            DoWith d = operations => operations.CreateCollection<Person>("foo", options);
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
            DoWith d = operations => operations.Execute<Person, object>("test", collection => { return 0; });
            new Execution(this).AssertDataAccessException(d);
        }

        [Test]
        public void ConvertsExceptionForExecuteDbCallback()
        {
            DoWith d = operations => operations.Execute<object>(db => { return null; });
            new Execution(this).AssertDataAccessException(d);
        }

        [Test]
        public void ConvertsExceptionForExecuteCollectionCallbackAndCollection()
        {
            DoWith d = operations => operations.Execute<Person, object>("collection", null);
            new Execution(this).AssertDataAccessException(d);
        }

        [Test]
        public void ConvertsExceptionForRunCommand()
        {
            DoWith d = operations => operations.RunCommand(new CommandDocument());
            new Execution(this).AssertDataAccessException(d);
        }

        [Test]
        public void ConvertsExceptionForExecuteStringCommand()
        {
            DoWith d = operations => operations.RunCommand("");
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
            DoWith d = operations => operations.GetCollection<Person>("collection");
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
            DoWith d = operations => operations.Insert("collection", _person);
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
            DoWith d = operations => operations.Insert("collection", _persons);
            new Execution(this).AssertDataAccessException(d);
        }

        [Test]
        public void GeoNearRejectsNullNearQuery()
        {
            DoWith d = operations => operations.GeoNear<Person>(null, 0, 0, 0);
            new Execution(this).AssertDataAccessException(d);
        }

        [Test]
        public void GeoNearRejectsNullNearQueryifCollectionGiven()
        {
            DoWith d = operations => operations.GeoNear<Person>("collection", null, 0, 0, 0);
            new Execution(this).AssertDataAccessException(d);
        }

        [Test]
        public void GeoNearRejectsNullEntityClass()
        {
            DoWith d = operations => operations.GeoNear<object>(null, 10, 20, 1);
            new Execution(this).AssertDataAccessException(d);
        }

        [Test]
        public void GeoNearRejectsNullEntityClassIfCollectionGiven()
        {
            DoWith d = operations => operations.GeoNear<object>("collection", null, 10, 20, 1);
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
    }
}
