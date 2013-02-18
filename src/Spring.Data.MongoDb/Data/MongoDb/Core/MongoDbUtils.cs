// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoDbUtils.cs" company="The original author or authors.">
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

using System;
using Common.Logging;
using MongoDB.Driver;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Helper class featuring helper methods for internal MongoDb classes.
    /// </summary>
    /// <author>Thomas Risberg</author>
    /// <author>Graeme Rocher</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MongoDbUtils
    {
        private static readonly ILog Log = LogManager.GetLogger<MongoDbUtils>();

        /// <summary>
        /// Private constructor to prevent instantiation.
        /// </summary>
        private MongoDbUtils()
        {

        }

        /// <summary>
        /// Obtains a <see cref="MongoDatabase"/> connection for the given <see cref="MongoServer"/> instance and database name
        /// </summary>
        /// <param name="mongo">the <see cref="MongoServer"/> instance, must not be null.</param>
        /// <param name="databaseName">the database name, must not be null or empty.</param>
        public static MongoDatabase GetDatabase(MongoServer mongo, string databaseName)
        {
            return DoGetDatabase(mongo, databaseName, null, null, true);
        }

        public static MongoDatabase GetDatabase(MongoServer mongo, string databaseName, WriteConcern writeConcern)
        {
            return DoGetDatabase(mongo, databaseName, null, writeConcern, true);
        }


        /// <summary>
        /// Obtains a <see cref="MongoDatabase"/> connection for the given <see cref="MongoServer"/>instance, database name and credentials
        /// </summary>
        /// <param name="mongo">the <see cref="MongoServer"/> instance, must not be null.</param>
        /// <param name="databaseName">the database name, must not be null or empty.</param>
        /// <param name="credentials">the credentials to use, must not be null.</param>
        public static MongoDatabase GetDatabase(MongoServer mongo, string databaseName, MongoCredentials credentials, WriteConcern writeConcern)
        {
            AssertUtils.ArgumentNotNull(mongo, "mongo");
            AssertUtils.ArgumentHasText(databaseName, "databaseName");

            return DoGetDatabase(mongo, databaseName, credentials, writeConcern, true);
        }

        private static MongoDatabase DoGetDatabase(MongoServer mongo, string databaseName, MongoCredentials credentials,
                                                   WriteConcern writeConcern, bool allowCreate)
        {
            var dbHolder = (DatabaseHolder)TransactionSynchronizationManager.GetResource(mongo);

            // Do we have a populated holder and TX sync active?
            if (dbHolder != null && !dbHolder.IsEmpty && TransactionSynchronizationManager.SynchronizationActive)
            {
                var holderDatabase = dbHolder.GetDatabase(databaseName);

                // DB found but not yet synchronized
                if (holderDatabase != null && !dbHolder.SynchronizedWithTransaction)
                {

                    Log.Debug(
                        m => m("Registering Spring transaction synchronization for existing MongoDB {0}.", databaseName));

                    TransactionSynchronizationManager.RegisterSynchronization(new MongoSynchronization(dbHolder, mongo));
                    dbHolder.SynchronizedWithTransaction = true;
                }

                if (holderDatabase != null)
                {
                    return holderDatabase;
                }
            }

            // Lookup fresh database instance
            Log.Debug(m => m("Getting Mongo Database name=[{0}]", databaseName));

            if (writeConcern == null)
                writeConcern = WriteConcern.Acknowledged;

            var newDatabase = credentials != null
                                  ? mongo.GetDatabase(databaseName, credentials, writeConcern)
                                  : mongo.GetDatabase(databaseName, writeConcern);

            // TX sync active, bind new database to thread
            if (TransactionSynchronizationManager.SynchronizationActive)
            {

                Log.Debug(
                    m => m("Registering Spring transaction synchronization for MongoDB instance {0}.", databaseName));

                DatabaseHolder holderToUse = dbHolder;

                if (holderToUse == null)
                {
                    holderToUse = new DatabaseHolder(databaseName, newDatabase);
                }
                else
                {
                    holderToUse.AddDatabase(databaseName, newDatabase);
                }

                TransactionSynchronizationManager.RegisterSynchronization(new MongoSynchronization(holderToUse, mongo));
                holderToUse.SynchronizedWithTransaction = true;

                if (holderToUse != dbHolder)
                {
                    TransactionSynchronizationManager.BindResource(mongo, holderToUse);
                }
            }

            // Check whether we are allowed to return the DB.
            if (!allowCreate && !IsDbTransactional(newDatabase, mongo))
            {
                throw new InvalidOperationException("No Mongo DB bound to thread, "
                                                    +
                                                    "and configuration does not allow creation of non-transactional one here");
            }

            return newDatabase;
        }

        /// <summary>
        /// Return whether the given DB instance is transactional, that is, bound to the current thread by Spring's transaction
        /// facilities.
        /// </summary>
        /// <param name="db">the Database to check</param>
        /// <param name="mongo">the Mongo instance that the DB was created with (may be <code>null</code>)</param>
        /// <returns>whether the DB is transactional</returns>
        public static bool IsDbTransactional(MongoDatabase db, MongoServer mongo)
        {

            if (mongo == null)
            {
                return false;
            }
            var dbHolder = (DatabaseHolder)TransactionSynchronizationManager.GetResource(mongo);
            return dbHolder != null && dbHolder.ContainsDatabase(db);
        }

        /// <summary>
        /// Perform actual closing of the Mongo DB object, catching and logging any cleanup exceptions thrown.
        /// </summary>
        /// <param name="db">the DB to close (may be <code>null</code>)</param>
        public static void CloseDb(MongoDatabase db)
        {
            if (db != null)
            {
                Log.Debug(m => m("Closing Mongo DB object"));
                try
                {
                    db.RequestDone();
                }
                catch (Exception ex)
                {
                    Log.Debug(m => m("Unexpected exception on closing Mongo DB object", ex));
                }
            }
        }
    }
}