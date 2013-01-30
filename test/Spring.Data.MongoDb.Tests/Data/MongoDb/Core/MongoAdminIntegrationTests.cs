// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoAdminIntegrationTests.cs" company="The original author or authors.">
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

using MongoDB.Driver;
using NUnit.Framework;
using Spring.Objects.Factory.Attributes;
using Spring.Testing.NUnit;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// This test class assumes that you are already running the MongoDB server.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class MongoAdminIntegrationTests : AbstractDependencyInjectionSpringContextTests
    {
	    private MongoDatabase _testAdminDb;

        [Autowired]
	    MongoServer _mongo;

        [SetUp]
	    public void Setup() 
        {

		    _mongo.GetDatabase("testAdminDb").Drop();
		    _testAdminDb = _mongo.GetDatabase("testAdminDb");
	    }

        [Test]
	    public void ServerStats()
        {
		    var result = _mongo.GetDatabase("admin").RunCommand("serverStatus");
            Assert.That(result.Ok, Is.True);
	    }

        protected override string[] ConfigLocations
        {
            get
            {
                return new[] { "assembly://Spring.Data.MongoDb.Tests/Spring.Resources/infrastructure.xml" };
            }
        }
    }
}
