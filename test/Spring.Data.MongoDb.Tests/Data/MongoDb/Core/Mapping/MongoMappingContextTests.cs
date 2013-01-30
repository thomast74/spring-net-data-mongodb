#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoMappingContextTests.cs" company="The original author or authors.">
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
using System.Reflection;
using MongoDB.Driver;
using NSubstitute;
using NUnit.Framework;
using Spring.Collections.Generic;
using Spring.Context;
using Spring.Data.Annotation;
using Spring.Data.Mapping.Context;
using Spring.Data.Mapping.Model;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// Unit test for <see cref="MongoMappingContext"/>
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class MongoMappingContextTests
    {
	    IApplicationContext _applicationContext;

        [SetUp]
        public void SetUp()
        {
            _applicationContext = Substitute.For<IApplicationContext>();
        }

	    [Test]
	    public void AddsSelfReferencingPersistentEntityCorrectly()
        {
		    MongoMappingContext context = new MongoMappingContext();
	        Assert.That(delegate
	            {
	                context.InitialEntitySet = new HashedSet<Type>() { typeof(SampleClass) };
	                context.Initialize();
	            }, Throws.Nothing);
        }

	    [Test]
	    public void RejectsEntityWithMultipleIdProperties()
        {
		    MongoMappingContext context = new MongoMappingContext();
	        Assert.That(delegate { context.GetPersistentEntity<ClassWithMultipleIdProperties>(); },
	                    Throws.TypeOf<MappingException>());
        }

	    [Test]
	    public void DoesNotReturnPersistentEntityForMongoSimpleType() 
        {
		    MongoMappingContext context = new MongoMappingContext();
		    Assert.That(context.GetPersistentEntity<MongoDBRef>(), Is.Null);
	    }

	    [Test]
	    public void PopulatesAbstractMappingContextsApplicationCorrectly() 
        {
		    MongoMappingContext context = new MongoMappingContext();
		    context.ApplicationContext = _applicationContext;

	        FieldInfo fieldInfo = typeof (AbstractMappingContext).GetField("_applicationContext", BindingFlags.Instance | BindingFlags.NonPublic);
            var appContext = fieldInfo.GetValue(context);

            Assert.That(appContext, Is.Not.Null);
	    }

	    class ClassWithMultipleIdProperties 
        {
            [Id]
            public string MyId { get; set; }

            public string Id { get; set; }
	    }

	    public class SampleClass 
        {
		    Dictionary<string, SampleClass> children;
	    }
    }
}
