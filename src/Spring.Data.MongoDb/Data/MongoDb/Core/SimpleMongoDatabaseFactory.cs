#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleMongoDbFactory.cs" company="The original author or authors.">
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
using System.Text.RegularExpressions;
using MongoDB.Driver;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Factory to create <see cref="MongoServer"/> instances from a <see cref="MongoServer"/> instance.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class SimpleMongoDatabaseFactory : IMongoDatabaseFactory, IDisposable
    {
        private readonly MongoServer _mongo;
        private readonly string _databaseName;
        private readonly bool _mongoInstanceCreated;
        private readonly MongoCredentials _credentials;
        private WriteConcern _writeConcern;


        /// <summary>
        /// Create an instance of <see cref="SimpleMongoDatabaseFactory"/> given the <see cref="MongoServer"/> instance and database name.
        /// </summary>
        /// <param name="mongo">Mongo instance, must not be null</param>
        /// <param name="databaseName">database name, not be null</param>
        public SimpleMongoDatabaseFactory(MongoServer mongo, string databaseName)
            : this(mongo, databaseName, null, false)
        {
        }

        /// <summary>
        /// Create an instance of <see cref="SimpleMongoDatabaseFactory"/> given the Mongo instance, database name, and username/password
        /// </summary>
        /// <param name="mongo">Mongo instance, must not be null</param>
        /// <param name="databaseName">Database name, must not be null</param>
        /// <param name="credentials">username and password</param>
        public SimpleMongoDatabaseFactory(MongoServer mongo, string databaseName, MongoCredentials credentials)
            : this(mongo, databaseName, credentials, false)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SimpleMongoDatabaseFactory"/> instance from the given <see cref="MongoUrl"/>.
        /// </summary>
        /// <param name="mongoUrl">MongoUrl to create a MongoServer instance, must not be <code>null</code></param>
        public SimpleMongoDatabaseFactory(MongoUrl mongoUrl)
            : this(CreateMongoServer(mongoUrl), mongoUrl.DatabaseName, mongoUrl.DefaultCredentials, true)
        {
        }

        private SimpleMongoDatabaseFactory(MongoServer mongo, string databaseName, MongoCredentials credentials,
                                     bool mongoInstanceCreated)
        {

            AssertUtils.ArgumentNotNull(mongo, "mongo");
            AssertUtils.ArgumentHasText(databaseName, "databaseName");
            AssertUtils.IsTrue(!Regex.IsMatch(databaseName, @"[^A-Za-z0-9-_]+"),
                               "Database name must only contain letters, numbers, underscores and dashes!");

            _mongo = mongo;
            _databaseName = databaseName;
            _mongoInstanceCreated = mongoInstanceCreated;
            _credentials = credentials;
        }

        public const string WriteConcernProperty = "WriteConcern";
        /// <summary>
        /// Get or sets the <see cref="WriteConcern"/> for the database to be  opened
        /// </summary>
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = value; }
        }

        /// <summary>
        /// Creates a default <see cref="MongoDb"/> instance.
        /// </summary>
        /// <returns>A MongoDatabase instance</returns>
        public MongoDatabase GetDatabase()
        {
            return GetDatabase(_databaseName);
        }

        /// <summary>
        /// Creates a <see cref="MongoDb"/> instance to access the database with the given name.
        /// </summary>
        /// <param name="dbName">must not be null or empty.</param>
        /// <returns>A MongoDatabase instance</returns>
        public MongoDatabase GetDatabase(string dbName)
        {
            AssertUtils.ArgumentHasText(dbName, "dbName");

            return MongoDbUtils.GetDatabase(_mongo, dbName, _credentials, _writeConcern);
        }

        /// <summary>
        /// Clean up the Mongo instance if it was created by the factory itself.
        /// </summary>
        public void Dispose()
        {
            if (_mongoInstanceCreated)
            {
                _mongo.Disconnect();
            }
        }

        private static MongoServer CreateMongoServer(MongoUrl mongoUrl)
        {
            var client = new MongoClient(mongoUrl);
            return client.GetServer();
        }
    }
}
