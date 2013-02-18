#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoFactoryObjectTests.cs" company="The original author or authors.">
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

using MongoDB.Driver;
using NUnit.Framework;
using Spring.Data.MongoDb.Core.Converters;
using Spring.Objects.Factory.Support;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Integration tests for <see cref="MongoFactoryObject"/>    
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class MongoFactoryObjectTests
    {
        [Test]
        public void ConvertsWriteConcernCorrectly()
        {

            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            factory.RegisterCustomConverter(typeof(WriteConcern), new WriteConcernTypeConverter());

            RootObjectDefinition definition = new RootObjectDefinition(typeof(MongoFactoryObject));
            definition.PropertyValues.Add("Url", "mongodb://localhost");
            definition.PropertyValues.Add("WriteConcern", "Acknowledged");
            factory.RegisterObjectDefinition("factory", definition);

            MongoFactoryObject obj = factory.GetObject<MongoFactoryObject>("&factory");
            Assert.That(ReflectionUtils.GetInstanceFieldValue(obj, "_writeConcern"),
                        Is.EqualTo(WriteConcern.Acknowledged));
        }
    }
}
