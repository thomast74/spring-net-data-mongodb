#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoTestHelper.cs" company="The original author or authors.">
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

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using NSubstitute;
using Spring.Data.MongoDb.Core;

namespace Spring.Data.MongoDb
{
    /// <summary>
    /// 
    /// </summary>
    /// <author></author>
    /// <author></author>
    public static class MongoTestHelper
    {
        private static string _message;
        private static MongoServer _server;
        private static MongoDatabase _datbase;


        /// <summary>
        /// Clears all cahced instnces
        /// </summary>
        public static void ClearCache()
        {
            _server = null;
            _datbase = null;
        }

        /// <summary>
        /// Creates a mocked MongoServer instance. If already created return the previously created one.
        /// </summary>
        /// <returns></returns>
        public static MongoServer GetCachedMockMongoServer()
        {
            if (_server == null)
            {
                var serverSettings = new MongoServerSettings
                {
                    Servers = new List<MongoServerAddress>
                            {
                                new MongoServerAddress("unittest")
                            }
                };
                _server = Substitute.For<MongoServerTestHelper>(serverSettings);
                _server.IsDatabaseNameValid(Arg.Any<string>(), out _message).Returns(x =>
                    {
                        x[1] = "message";
                        return true;
                    });
            }

            return _server;
        }

        /// <summary>
        /// Creates a mocked MongoDatabase instance.
        /// </summary>
        /// <returns></returns>
        public static MongoDatabase GetCachedMockMongoDatabase(string databaseName, WriteConcern writeConcern)
        {
            if (_datbase == null)
            {
                var server = GetCachedMockMongoServer();
                var settings = new MongoDatabaseSettings(server, databaseName);
                settings.WriteConcern = writeConcern;

                _datbase = Substitute.For<MongoDatabaseTestHelper>(server, settings);
                _datbase.IsCollectionNameValid(Arg.Any<string>(), out _message).Returns(x =>
                    {
                        x[1] = "";
                        return true;
                    });
            }
            return _datbase;
        }

        /// <summary>
        /// Creates a mocked MongoDatabase instance.
        /// </summary>
        /// <returns></returns>
        public static MongoDatabase GetMockMongoDatabase(string databaseName, WriteConcern writeConcern)
        {
            var server = GetCachedMockMongoServer();
            var settings = new MongoDatabaseSettings(server, databaseName);
            settings.WriteConcern = writeConcern;

            var datbase = Substitute.For<MongoDatabaseTestHelper>(server, settings);
            datbase.IsCollectionNameValid(Arg.Any<string>(), out _message).Returns(x =>
            {
                x[1] = "";
                return true;
            });
            
            return datbase;
        }

        /// <summary>
        /// Creates a mock of mongo collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        public static MongoCollection<T> CreateMockCollection<T>(string databaseName, string collectionName)
        {
            var database = GetMockMongoDatabase(databaseName, WriteConcern.Acknowledged);
            var settings = new MongoCollectionSettings<T>(database, collectionName);

            var m = Substitute.For<MongoCollectionTestHelper<T>>(database, settings);
            m.Database.Returns(database);

            return m;
        }

        public static MongoCollection<T> ReturnsCollection<T>(this MongoCollection<T> collection,
                                                              IEnumerable<T> enumerable)
        {
            var cursor = Substitute.For<MongoCursor<T>>(collection, Substitute.For<IMongoQuery>(), ReadPreference.Primary);
            cursor.GetEnumerator().Returns(enumerable.GetEnumerator());
            cursor.When(x => x.GetEnumerator())
                  .Do(callInfo => enumerable.GetEnumerator().Reset());
            // Reset Enumerator, incase the method is called multiple times

            cursor.SetSortOrder(Arg.Any<IMongoSortBy>()).Returns(cursor);
            cursor.SetLimit(Arg.Any<int>()).Returns(cursor);
            cursor.SetFields(Arg.Any<IMongoFields>()).Returns(cursor);
            cursor.SetFields(Arg.Any<string[]>()).Returns(cursor);
            cursor.SetFields(Arg.Any<string>()).Returns(cursor);
            cursor.SetSkip(Arg.Any<int>()).Returns(cursor);

            collection.Find(Arg.Any<IMongoQuery>()).Returns(cursor);
            collection.FindAs<T>(Arg.Any<IMongoQuery>()).Returns(cursor);
            collection.FindAll().Returns(cursor);
            collection.FindAllAs<T>().Returns(cursor);
            // You properly need to setup more methods of cursor here

            return collection;
        }
    }
}