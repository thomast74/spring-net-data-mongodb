#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractMongoConfigurationTests.cs" company="The original author or authors.">
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
using Spring.Context.Attributes;
using Spring.Data.MongoDb.Core.Mapping.Annotation;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Unit tests for <see cref="AbstractMongoConfiguration"/>.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [Category(TestCategory.Unit)]
    public class AbstractMongoConfigurationTests
    {
	    [Test]
	    public void UsesConfigClassPackageAsBaseMappingPackage()
        {
		    AbstractMongoConfiguration configuration = new SampleMongoConfiguration();
		 
            Assert.That(configuration.GetMappingAssembly(), Is.EqualTo(typeof(SampleMongoConfiguration).AssemblyQualifiedName));
		    Assert.That(configuration.GetInitialEntitySet(), Has.Count.EqualTo(1));
		    Assert.That(configuration.GetInitialEntitySet(), Has.Member(typeof(Entity)));
	    }

	    [Test]
	    public void DoesNotScanPackageIfMappingPackageIsNull()
        {
		    AssertScanningDisabled(null);
	    }

	    [Test]
	    public void DoesNotScanPackageIfMappingPackageIsEmpty()
        {
		    AssertScanningDisabled("");
		    AssertScanningDisabled(" ");
	    }

	    private static void AssertScanningDisabled(string value)
	    {
	        AbstractMongoConfiguration configuration = new SampleNoScanningMongoConfiguration();

		    Assert.That(configuration.GetMappingAssembly(), Is.EqualTo(value));
		    Assert.That(configuration.GetInitialEntitySet(), Has.Count.EqualTo(0));
	    }

	    public class SampleMongoConfiguration : AbstractMongoConfiguration
        {
            public override string GetDatabaseName()
            {
			    return "database";
		    }

		    [ObjectDef]
		    public override MongoServer MongoServer()
            {
                var client = new MongoClient("mongodb://localhost");
	            return client.GetServer();
		    }
	    }

        public class SampleNoScanningMongoConfiguration : SampleMongoConfiguration
        {
            public override string GetMappingAssembly()
            {
                return string.Empty;
            }
        }

	    [Document]
	    public class Entity
        {
	    }
    }
}
