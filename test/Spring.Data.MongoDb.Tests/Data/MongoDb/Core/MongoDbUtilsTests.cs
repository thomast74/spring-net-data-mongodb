#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoDbUtilsTests.cs" company="The original author or authors.">
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
using NSubstitute;
using NUnit.Framework;
using Spring.Transaction.Support;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Unit tests for <see cref="MongoDbUtils" />
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class MongoDbUtilsTest
    {
        private MongoServer _mongo;
        private string _message;

        [SetUp]
        public void Setup()
        {
            _mongo = MongoTestHelper.GetCachedMockMongoServer();
            _mongo.IsDatabaseNameValid(Arg.Any<string>(), out _message).Returns(true);
            _mongo.GetDatabase(Arg.Any<string>(), Arg.Any<WriteConcern>()).Returns(x => MongoTestHelper.GetMockMongoDatabase((string)x[0], (WriteConcern)x[1]));

            TransactionSynchronizationManager.InitSynchronization();
        }

        [TearDown]
        public void TearDown()
        {
            var keys = new object[TransactionSynchronizationManager.ResourceDictionary.Keys.Count];
            TransactionSynchronizationManager.ResourceDictionary.Keys.CopyTo(keys, 0);
            foreach (var key in keys)
            {
                TransactionSynchronizationManager.UnbindResource(key);
            }

            TransactionSynchronizationManager.ClearSynchronization();

            _mongo.ClearReceivedCalls();
            _mongo = null;
            MongoTestHelper.ClearCache();
        }

        [Test]
        public void ReturnsNewInstanceForDifferentDatabaseName()
        {
            var first = MongoDbUtils.GetDatabase(_mongo, "first");
            var second = MongoDbUtils.GetDatabase(_mongo, "second");

            Assert.That(second, Is.Not.EqualTo(first));
        }

        [Test]
        public void ReturnsSameInstanceForSameDatabaseName()
        {
            var first = MongoDbUtils.GetDatabase(_mongo, "first");

            Assert.That(first, Is.Not.Null);
            Assert.That(MongoDbUtils.GetDatabase(_mongo, "first"), Is.SameAs(first));
        }
    }
}