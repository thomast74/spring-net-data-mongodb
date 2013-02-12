#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoTemplate.cs" company="The original author or authors.">
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Common.Logging;
using MongoDB.Driver;
using Spring.Context;
using Spring.Data.Mapping.Context;
using Spring.Data.MongoDb.Core.Convert;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// 
    /// </summary>
    /// <author></author>
    /// <author></author>
    public class MongoTemplate : IMongoOperations, IApplicationContextAware
    {
        private static readonly ILog Log = LogManager.GetLogger<MongoTemplate>();
        private static readonly string ID_FIELD = "_id";

        private static readonly WriteConcernResultChecking DEFAULT_WRITE_RESULT_CHECKING =
            WriteConcernResultChecking.None;

        private static readonly ICollection<string> ITERABLE_CLASSES = new ReadOnlyCollection<string>(
            new List<string>()
                {
                    typeof (List<>).Name,
                    typeof (Collection<>).Name,
                    typeof (IEnumerable<>).Name
                });

        private readonly IMongoConverter mongoConverter;
        private readonly IMappingContext mappingContext;
        private readonly IMongoDbFactory mongoDbFactory;
        private readonly MongoExceptionTranslator exceptionTranslator = new MongoExceptionTranslator();
        private readonly QueryMapper mapper;

        /*
        private WriteConcern writeConcern;
        private WriteConcernResolver writeConcernResolver = DefaultWriteConcernResolver.INSTANCE;
        private WriteResultChecking writeResultChecking = WriteResultChecking.None;
        private ReadPreference readPreference;
        private IApplicationEventPublisher eventPublisher;
        private ResourceLoader resourceLoader;
        private MongoPersistentEntityIndexCreator indexCreator;
        */

        /// <summary>
        /// Constructor used for a basic template configuration
        /// </summary>
        /// <param name="mongo">must not be <code>null</code></param>
        /// <param name="databaseName">must not be <code>null</code> or empty</param>
        public MongoTemplate(MongoServer mongo, string databaseName)
            : this(new SimpleMongoDbFactory(mongo, databaseName), null)

        {
        }

        /// <summary>
        /// Constructor used for a template configuration with user credentials in the form of
        /// <see cref="userCredentials"/>
        /// </summary>
        /// <param name="mongo">must not be <code>null</code></param>
        /// <param name="databaseName">must not be <code>null</code> or empty</param>
        public MongoTemplate(MongoServer mongo, string databaseName, MongoCredentials userCredentials)
            : this(new SimpleMongoDbFactory(mongo, databaseName, userCredentials))
        {
        }

        /// <summary>
        /// Constructor used for a basic template configuration.
        /// </summary>
        /// <param name="mongoDbFactory">must not be null</param>
        public MongoTemplate(IMongoDbFactory mongoDbFactory)
            : this(mongoDbFactory, null)

        {
        }

        /// <summary>
        /// Constructor used for a basic template configuration.
        /// </summary>
        /// <param name="mongoDbFactory">must not be <code>nulll</param>
        /// <param name="mongoConverter"></param>
        public MongoTemplate(IMongoDbFactory mongoDbFactory, IMongoConverter mongoConverter)
        {
        }
       

        public IApplicationContext ApplicationContext
        {
            set {  }
        }

        /// <summary>
        /// Returns the default database from <see cref="IMongoDbFactory.GetDatabase()"/>
        /// </summary>
        public MongoDatabase Database { get { return mongoDbFactory.GetDatabase(); } }

        public string GetCollectionName<T>()
        {
            throw new NotImplementedException();
        }

        public CommandResult ExecuteCommand(string jsonCommand)
        {
            throw new NotImplementedException();
        }

        public CommandResult ExecuteCommand(CommandDocument command)
        {
            throw new NotImplementedException();
        }

        public CommandResult ExecuteCommand(CommandDocument command, int options)
        {
            throw new NotImplementedException();
        }

        public void ExecuteQuery(Query.Query query, IDocumentCallbackHandler dch, string collectionName)
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(Func<MongoDatabase, T> databaseCallback)
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(Func<MongoCollection, T> collectionCallback)
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(string collectionName, Func<MongoCollection, T> collectionCallback)
        {
            throw new NotImplementedException();
        }

        public T ExecuteInSession<T>(Func<MongoDatabase, T> databaseCallback)
        {
            throw new NotImplementedException();
        }

        public MongoCollection CreateCollection<T>()
        {
            throw new NotImplementedException();
        }

        public MongoCollection CreateCollection<T>(CollectionOptions collectionOptions)
        {
            throw new NotImplementedException();
        }

        public MongoCollection CreateCollection(string collectionName)
        {
            throw new NotImplementedException();
        }

        public MongoCollection CreateCollection(string collectionName, CollectionOptions collectionOptions)
        {
            throw new NotImplementedException();
        }

        public IList<string> GetCollectionNames()
        {
            throw new NotImplementedException();
        }

        public MongoCollection GetCollection<T>()
        {
            throw new NotImplementedException();
        }

        public MongoCollection GetCollection(string collectionName)
        {
            throw new NotImplementedException();
        }

        public bool CollectionExists<T>()
        {
            throw new NotImplementedException();
        }

        public bool CollectionExists(string collectionName)
        {
            throw new NotImplementedException();
        }

        public void DropCollection<T>()
        {
            throw new NotImplementedException();
        }

        public void DropCollection(string collectionName)
        {
            throw new NotImplementedException();
        }

        public IIndexOperations IndexOps<T>()
        {
            throw new NotImplementedException();
        }

        public IIndexOperations IndexOps(string collectionName)
        {
            throw new NotImplementedException();
        }

        public IList<T> FindAll<T>()
        {
            throw new NotImplementedException();
        }

        public IList<T> FindAll<T>(string collectionName)
        {
            throw new NotImplementedException();
        }

        public Mapreduce.GroupByResults<T> Group<T>(string inputCollectionName, IMongoGroupBy groupBy)
        {
            throw new NotImplementedException();
        }

        public Mapreduce.GroupByResults<T> Group<T>(Query.Criteria criteria, string inputCollectionName, IMongoGroupBy groupBy)
        {
            throw new NotImplementedException();
        }

        public Mapreduce.MapReduceResults<T> MapReduce<T>(string inputCollectionName, string mapFunction, string reduceFunction)
        {
            throw new NotImplementedException();
        }

        public Mapreduce.MapReduceResults<T> MapReduce<T>(string inputCollectionName, string mapFunction, string reduceFunction, IMongoMapReduceOptions mapReduceOptions)
        {
            throw new NotImplementedException();
        }

        public Mapreduce.MapReduceResults<T> MapReduce<T>(Query.Query query, string inputCollectionName, string mapFunction, string reduceFunction)
        {
            throw new NotImplementedException();
        }

        public Mapreduce.MapReduceResults<T> MapReduce<T>(Query.Query query, string inputCollectionName, string mapFunction, string reduceFunction, IMongoMapReduceOptions mapReduceOptions)
        {
            throw new NotImplementedException();
        }

        public GeoNearResult<T> GeoNear<T>(Query.NearQuery near)
        {
            throw new NotImplementedException();
        }

        public GeoNearResult<T> GeoNear<T>(Query.NearQuery near, string collectionName)
        {
            throw new NotImplementedException();
        }

        public T FindOne<T>(Query.Query query)
        {
            throw new NotImplementedException();
        }

        public T FindOne<T>(Query.Query query, string collectionName)
        {
            throw new NotImplementedException();
        }

        public IList<T> Find<T>(Query.Query query)
        {
            throw new NotImplementedException();
        }

        public IList<T> Find<T>(Query.Query query, string collectionName)
        {
            throw new NotImplementedException();
        }

        public T FindById<T>(object id)
        {
            throw new NotImplementedException();
        }

        public T FindById<T>(object id, string collectionName)
        {
            throw new NotImplementedException();
        }

        public T FindAndModify<T>(Query.Query query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public T FindAndModify<T>(Query.Query query, IMongoUpdate update, string collectionName)
        {
            throw new NotImplementedException();
        }

        public T FindAndModify<T>(Query.Query query, IMongoUpdate update, FindAndModifyOptions options)
        {
            throw new NotImplementedException();
        }

        public T FindAndModify<T>(Query.Query query, IMongoUpdate update, FindAndModifyOptions options, string collectionName)
        {
            throw new NotImplementedException();
        }

        public T FindAndRemove<T>(Query.Query query)
        {
            throw new NotImplementedException();
        }

        public T FindAndRemove<T>(Query.Query query, string collectionName)
        {
            throw new NotImplementedException();
        }

        public long Count<T>(Query.Query query)
        {
            throw new NotImplementedException();
        }

        public long Count(Query.Query query, string collectionName)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(T objectToSave)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(T objectToSave, string collectionName)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(ICollection<T> objectsToSave)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(ICollection<T> objectsToSave, string collectionName)
        {
            throw new NotImplementedException();
        }

        public void InsertAll<T>(ICollection<T> objectsToSave)
        {
            throw new NotImplementedException();
        }

        public void Save<T>(T objectToSave)
        {
            throw new NotImplementedException();
        }

        public void Save<T>(T objectToSave, string collectionName)
        {
            throw new NotImplementedException();
        }

        public SafeModeResult Upsert<T>(Query.Query query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public SafeModeResult Upsert(Query.Query query, IMongoUpdate update, string collectionName)
        {
            throw new NotImplementedException();
        }

        public SafeModeResult UpdateFirst<T>(Query.Query query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public SafeModeResult UpdateFirst(Query.Query query, IMongoUpdate update, string collectionName)
        {
            throw new NotImplementedException();
        }

        public SafeModeResult UpdateMulti<T>(Query.Query query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public SafeModeResult UpdateMulti(Query.Query query, IMongoUpdate update, string collectionName)
        {
            throw new NotImplementedException();
        }

        public void Remove<T>(T objectToRemove)
        {
            throw new NotImplementedException();
        }

        public void Remove<T>(T objecToRemove, string collectionName)
        {
            throw new NotImplementedException();
        }

        public void Remove<T>(Query.Query query)
        {
            throw new NotImplementedException();
        }

        public void Remove(Query.Query query, string collectionName)
        {
            throw new NotImplementedException();
        }

        public IMongoConverter GetConverter()
        {
            throw new NotImplementedException();
        }
    }
}

