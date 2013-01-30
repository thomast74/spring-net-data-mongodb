#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicMongoPersistentEntityTests.cs" company="The original author or authors.">
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

using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.MongoDb.Core.Mapping.Annotation;
using Spring.Data.Util;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// Unit test class for <see cref="BasicMongoPersistentEntity"/>
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class BasicMongoPersistentEntityTests
    {
        private GenericApplicationContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = new GenericApplicationContext();
            ContextRegistry.RegisterContext(_context);
        }

        [TearDown]
        public void TearDown()
        {
            ContextRegistry.Clear();
        }

	    [Test]
	    public void SubclassInheritsAtDocumentAnnotation()
	    {
	        BasicMongoPersistentEntity entity = new BasicMongoPersistentEntity(ClassTypeInformation.From<Person>());

		    Assert.That(entity.Collection, Is.EqualTo("contacts"));
	    }

	    [Test]
	    public void EvaluatesSpELExpression() 
        {
            BasicMongoPersistentEntity entity = new BasicMongoPersistentEntity(ClassTypeInformation.From<Company>());
            entity.ApplicationContext = _context;

		    Assert.That(entity.Collection, Is.EqualTo("35"));
	    }

        [Test]
        public void UsesDefaultEntityNameWithDocumentAttribute()
        {
            IMongoPersistentEntity entity = new BasicMongoPersistentEntity(ClassTypeInformation.From<Building>());

            Assert.That(entity.Collection, Is.EqualTo("buildings"));
        }

        [Test]
        public void UsesDefaultEntityNameWithoutDocumentAttribute()
        {
            IMongoPersistentEntity entity = new BasicMongoPersistentEntity(ClassTypeInformation.From<Country>());

            Assert.That(entity.Collection, Is.EqualTo("countries"));
        }

	    [Test]
	    public void CollectionAllowsReferencingSpringBean()
	    {
		    CollectionProvider provider = new CollectionProvider();
		    provider.CollectionName = "reference";
            _context.DefaultListableObjectFactory.RegisterSingleton("myObject", provider);

		    BasicMongoPersistentEntity entity = new BasicMongoPersistentEntity(ClassTypeInformation.From<DynamicallyMapped>());
	        entity.ApplicationContext = _context;

		    Assert.That(entity.Collection, Is.EqualTo("reference"));
	    }

	    [Document("'contacts'")]
	    class Contact 
        {
	    }

	    class Person : Contact
        {
	    }

	    [Document("35")]
	    class Company
        {
	    }

        [Document]
        class Building
        {
        }

        class Country
        {
        }

        [Document("@(myObject).CollectionName")]
	    class DynamicallyMapped 
        {
	    }

	    class CollectionProvider 
        {
		    private string collectionName;

		    public string CollectionName
            {
		        get { return collectionName; }
                set { collectionName = value; }
            }
	    }
    }
}
