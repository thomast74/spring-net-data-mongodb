#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleMongoDbFactoryTests.cs" company="The original author or authors.">
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
using MongoDB.Driver;
using NSubstitute;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Unit tests for <see cref="SimpleMongoDatabaseFactory" />
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class SimpleMongoDbFactoryTests
    {
        MongoServer _mongo;

        [SetUp]
        public void SetUp()
        {
            var settings = new MongoServerSettings();
            _mongo = Substitute.For<MongoServerTestHelper>(settings);
        }

        [Test]
        public void RejectsIllegalDatabaseNames()
        {
            RejectsDatabaseName("foo.bar");
            RejectsDatabaseName("foo!bar");
            RejectsDatabaseName("foo bar");
            RejectsDatabaseName("foo$bar");
            RejectsDatabaseName("foo/bar");
            RejectsDatabaseName("foo\\bar");
            RejectsDatabaseName("foo\0bar");
        }

        [Test]
        public void AllowsDatabaseNames()
        {
            new SimpleMongoDatabaseFactory(_mongo, "foo-bar");
            new SimpleMongoDatabaseFactory(_mongo, "foo_bar");
            new SimpleMongoDatabaseFactory(_mongo, "foo01231bar");

            Assert.True(true);
        }

        [Test]
        public void MongoUriConstructor()
        {
            var uriBuilder = new MongoUrlBuilder("mongodb://myUsername:myPassword@localhost/myDatabase");
            IMongoDatabaseFactory mongoDbFactory = new SimpleMongoDatabaseFactory(uriBuilder.ToMongoUrl());

            Assert.That(ReflectionUtils.GetInstanceFieldValue(mongoDbFactory, "_credentials"), Is.EqualTo(new MongoCredentials("myUsername", "myPassword")));
            Assert.That(ReflectionUtils.GetInstanceFieldValue(mongoDbFactory, "_databaseName").ToString(), Is.EqualTo("myDatabase"));
        }

        private void RejectsDatabaseName(string databaseName)
        {
            try
            {
                new SimpleMongoDatabaseFactory(_mongo, databaseName);
                Assert.Fail("Expected database name " + databaseName + " to be rejected!");
            }
            catch (ArgumentException)
            {
            }
        }

    }
}
