// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbHolder.cs" company="The original author or authors.">
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

using System.Collections.Concurrent;
using MongoDB.Driver;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Implementation of <see cref="IResourceHolder" /> for MongoDb
    /// </summary>
    /// <author>Thomas Trageser</author>
    public class DbHolder : ResourceHolderSupport
    {
        private static readonly object DefaultKey = new object();

        private readonly ConcurrentDictionary<object, MongoDatabase> _dbMap = new ConcurrentDictionary<object, MongoDatabase>();

        public DbHolder(MongoDatabase db)
        {
            AddDatabase(db);
        }

        public DbHolder(object key, MongoDatabase db)
        {
            AddDatabase(key, db);
        }

        public MongoDatabase GetDatabase()
        {
            return GetDatabase(DefaultKey);
        }

        public MongoDatabase GetDatabase(object key)
        {
            MongoDatabase db;
            _dbMap.TryGetValue(key, out db);
            return db;
        }

        public MongoDatabase GetAnyDatabase()
        {
            return _dbMap.Count > 0 ? _dbMap.Values.GetEnumerator().Current : null;
        }

        public void AddDatabase(MongoDatabase session)
        {
            AddDatabase(DefaultKey, session);
        }

        public void AddDatabase(object key, MongoDatabase session)
        {
            AssertUtils.ArgumentNotNull(key, "key");
            AssertUtils.ArgumentNotNull(session, "session");

            _dbMap.TryAdd(key, session);
        }

        public MongoDatabase RemoveDatabase(object key)
        {
            MongoDatabase old;
            _dbMap.TryRemove(key, out old);
            return old;
        }

        public bool ContainsDatabase(MongoDatabase session)
        {
            return _dbMap.Values.Contains(session);
        }

        public bool IsEmpty
        {
            get { return _dbMap.Count == 0; }
        }

        public bool DoesNotHoldNonDefaultDatabase
        {
            get
            {
                lock (_dbMap)
                {
                    return _dbMap.Count == 0 || (_dbMap.Count == 1 && _dbMap.ContainsKey(DefaultKey));
                }
            }
        }

    }
}
