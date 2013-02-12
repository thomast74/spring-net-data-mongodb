#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoPersistentPropertyComparerTests.cs" company="The original author or authors.">
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
using NSubstitute;
using NUnit.Framework;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// Unit tests for <see cref="MongoPersistentPropertyComparer"/>.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class MongoPersistentPropertyComparerTests
    {
	    IMongoPersistentProperty _firstName;
	    IMongoPersistentProperty _lastName;
	    IMongoPersistentProperty _ssn;

        [SetUp]
        public void SetUp()
        {
            _firstName = Substitute.For<IMongoPersistentProperty>();
            _firstName.FieldOrder.Returns(20);

            _lastName = Substitute.For<IMongoPersistentProperty>();
            _lastName.FieldOrder.Returns(Int32.MaxValue);

            _ssn = Substitute.For<IMongoPersistentProperty>();
            _ssn.FieldOrder.Returns(10);
        }

        [Test]
	    public void OrdersPropertiesCorrectly()
        {
            var properties = new List<IMongoPersistentProperty> { _lastName, _firstName, _ssn };
            properties.Sort(new MongoPersistentPropertyComparer());

		    Assert.That(properties[0], Is.SameAs(_ssn));
            Assert.That(properties[1], Is.SameAs(_firstName));
            Assert.That(properties[2], Is.SameAs(_lastName));
	    }
    }
}
