#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeoIndexedTests.cs" company="The original author or authors.">
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

using System.Linq;
using MongoDB.Driver;
using NUnit.Framework;
using Spring.Context;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// Integration test for GeoSpatial data
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class GeoIndexedTests
    {
	    private readonly string[] _collectionsToDrop = new string[]{ GeoIndexedAppConfig.GeoCollection, "Person" };
        private const string ConnectionString = "mongodb://localhost";

        IApplicationContext _applicationContext;
	    MongoTemplate _template;
	    MongoMappingContext _mappingContext;

        [SetUp]
	    public void SetUp()
        {
		    CleanDb();

		    _applicationContext = new AnnotationConfigApplicationContext(GeoIndexedAppConfig);
		    _template = _applicationContext.GetObject<MongoTemplate>();
		    _mappingContext = _applicationContext.GetObject<MongoMappingContext>();
	    }

	    [TearDown]
	    public void TearDown()
        {
		    CleanDb();
	    }

	    private void CleanDb()
        {
            MongoClient client = new MongoClient(ConnectionString);
	        MongoServer mongo = client.GetServer();
		    MongoDatabase db = mongo.GetDatabase(GeoIndexedAppConfig.GeoDb);
		    foreach (string coll in _collectionsToDrop)
            {
			    db.GetCollection(coll).Drop();
		    }
	    }

	    [Test]
	    public void TestGeoLocation()
        {
		    var geo = new GeoLocation(new double[] { 40.714346, -74.005966 });
		    _template.Insert(geo);

	        bool hasIndex = _template.Execute("geolocation", collection =>
	            {
	                var indexes = collection.GetIndexes();
	                return indexes.Any(index => "location".Equals(index.Name));
	            });

		    Assert.That(hasIndex, Is.True);
	    }
    }
}
