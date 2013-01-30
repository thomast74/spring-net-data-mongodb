#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicMongoPersistentPropertyTests.cs" company="The original author or authors.">
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
using System.Reflection;
using NUnit.Framework;
using Spring.Data.Annotation;
using Spring.Data.Mapping.Model;
using Spring.Data.MongoDb.Core.Mapping.Annotation;
using Spring.Data.Util;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// Unit tests for <see cref="BasicMongoPersistentEntity"/>
    /// </summary>
    /// <author>OLiver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class BasicMongoPersistentPropertyTests
    {
	    IMongoPersistentEntity _entity;

        [SetUp]
	    public void SetUp()
        {
		    _entity = new BasicMongoPersistentEntity(ClassTypeInformation.From<Person>());
	    }

	    [Test]
	    public void UsesAnnotatedFieldName()
        {
	        string fieldName = GetPropertyFor(typeof(Person), "Firstname").FieldName;

		    Assert.That(fieldName, Is.EqualTo("foo"));
	    }

	    [Test]
	    public void ReturnsIdForIdProperty() 
        {
		    IMongoPersistentProperty property = GetPropertyFor(typeof(Person), "Id");

		    Assert.That(property.IsIdProperty, Is.True);
		    Assert.That(property.FieldName, Is.EqualTo("_id"));
	    }

	    [Test]
	    public void ReturnsPropertyNameForUnannotatedProperties()
	    {
	        string fieldName = GetPropertyFor(typeof (Person), "Lastname").FieldName;

            Assert.That(fieldName, Is.EqualTo("lastname"));
	    }

	    [Test]
	    public void PreventsNegativeOrder()
	    {
            Assert.That(delegate { GetPropertyFor(typeof(Person), "SSN"); }, Throws.TypeOf<ArgumentOutOfRangeException>());
	    }

	    /**
	     * @see DATAMONGO-553
	     */
	    [Test]
	    public void UsesPropertyAccessForThrowableCause() 
        {
		    IMongoPersistentProperty property = GetPropertyFor(typeof(Exception), "InnerException");

		    Assert.That(property.UsePropertyAccess, Is.True);
	    }

	    private IMongoPersistentProperty GetPropertyFor(Type classType, string property)
	    {
            PropertyInfo propertyInfo = classType.GetProperty(property, BindingFlags.Instance | BindingFlags.Public);
		    return new BasicMongoPersistentProperty(propertyInfo, _entity, new SimpleTypeHolder());
	    }

	    class Person 
        {
            [Id]
            public string Id { get; set; }

            [Field("foo")]
            public string Firstname { get; set; }

            public string Lastname { get; set; }

		    [Field(Order = -20)]
		    public string SSN { get; set; }
	    }
    }
}
