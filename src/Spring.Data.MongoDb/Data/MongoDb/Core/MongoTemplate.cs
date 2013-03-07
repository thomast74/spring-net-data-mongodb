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
using System.Linq;
using Common.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Spring.Dao;
using Spring.Data.Mapping.Model;
using Spring.Data.MongoDb.Core.Attributes;
using Spring.Data.MongoDb.Support;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Implementation of <see cref="IMongoOperations"/>
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MongoTemplate : IMongoOperations
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        private readonly MongoExceptionTranslator _exceptionTranslator = new MongoExceptionTranslator();

        private IMongoDatabaseFactory _mongoDbFactory;
        private WriteConcern _writeConcern;

        /// <summary>
        /// Constructor used for a basic template configuration
        /// </summary>
        /// <param name="mongo"></param>
        /// <param name="databaseName"></param>
        public MongoTemplate(MongoServer mongo, string databaseName)
            : this(new SimpleMongoDatabaseFactory(mongo, databaseName))
        {
        }

        /// <summary>
        /// Constructor used for a template configuration with user credentials in the form of
        /// <see cref="MongoCredentials"/>
        /// </summary>
        /// <param name="mongo"></param>
        /// <param name="databaseName"></param>
        /// <param name="userCredentials"></param>
        public MongoTemplate(MongoServer mongo, string databaseName, MongoCredentials userCredentials)
            : this(new SimpleMongoDatabaseFactory(mongo, databaseName, userCredentials))
        {
        }

        /// <summary>
        /// Constructor used for a basic template configuration
        /// </summary>
        /// <param name="mongoDbFactory"></param>
        public MongoTemplate(IMongoDatabaseFactory mongoDbFactory)
        {
		    AssertUtils.ArgumentNotNull(mongoDbFactory, "mongoDbFactory");

		    _mongoDbFactory = mongoDbFactory;
    	}

        public WriteConcern WriteConcern { get { return _writeConcern; } set { _writeConcern = value; } }


        /// <summary>
        /// Check to see if a collection with a name indicated by the entity type exists.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <returns><code>true</code> if a collection with the given name is found, <code>false</code> otherwise.</returns>
        public bool CollectionExists<T>()
        {
            var collectionName = DetermineCollectionName(typeof (T));

            AssertUtils.ArgumentHasText(collectionName, "collectionName");

            return CollectionExists(collectionName);
        }

        /// <summary>
        /// Check to see if a collection with a given name exists.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <param name="collectionName">name of the collection</param>
        /// <returns><code>true</code> if a collection with the given name is found, <code>false</code> otherwise.</returns>
        public bool CollectionExists(string collectionName)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");

            MongoDatabase database = GetDatabase();
            if (database == null)
                throw new MongoException("Error while retrieving database to check if collection exists");

            return database.CollectionExists(collectionName);
        }

        /// <summary>
        /// Create an uncapped collection with a name based on the provided entity type.
        /// </summary>
        /// <returns>the created collection</returns>
        public MongoCollection<T> CreateCollection<T>()
        {
            Type type = typeof (T);

            return CreateCollection<T>(DetermineCollectionName(type));
        }

        /// <summary>
        /// Create a collect with a name based on the provided entity type using the options.
        /// </summary>
        /// <param name="collectionOptions">options to use when creating the collection</param>
        /// <returns>the created collection</returns>
        public MongoCollection<T> CreateCollection<T>(IMongoCollectionOptions collectionOptions)
        {
            Type type = typeof(T);

            return CreateCollection<T>(DetermineCollectionName(type), collectionOptions);
        }

        /// <summary>
        /// Create an uncapped collection with the provided name.
        /// </summary>
        /// <param name="collectionName">name of the collection</param>
        /// <returns>the created collection</returns>
        public MongoCollection<T> CreateCollection<T>(string collectionName)
        {
            return CreateCollection<T>(collectionName, null);
        }

        /// <summary>
        /// Create a collect with the provided name and options.
        /// </summary>
        /// <param name="collectionName">name of the collection</param>
        /// <param name="collectionOptions">options to use when creating the collection.</param>
        /// <returns>the created collection</returns>
        public MongoCollection<T> CreateCollection<T>(string collectionName, IMongoCollectionOptions collectionOptions)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");

            if (collectionName.Contains("\0"))
                throw new MongoException("Collection name should not contain a null character");
            if (collectionName.Contains("$"))
                throw new MongoException("Collection name should not contain a $ character");
            if (collectionName.StartsWith("system."))
                throw new MongoException("Collection name should not have 'system.' prefix");
            if (collectionName.Length > 80)
                throw new MongoException("Collection name should not longer than 80 characters");

            CommandResult result;
            MongoDatabase db = GetDatabase();
            
            if (db == null)
                throw new MongoException("No valid database to execute command");
            try
            {
                result = collectionOptions == null
                             ? db.CreateCollection(collectionName)
                             : db.CreateCollection(collectionName, collectionOptions);

                if (!result.Ok)
                    throw new MongoException(result.ErrorMessage);

                return db.GetCollection<T>(collectionName);
            }
            catch (Exception e)
            {
                throw new MongoException(e.Message);
            }
        }
        
        /// <summary>
        /// The collection name used for the specified type by this template.
        /// </summary>
        /// <returns>Collection Name</returns>
        public string GetCollectionName<T>()
        {
            return DetermineCollectionName(typeof(T));
        }

        /// <summary>
        /// A set of collection names.
        /// </summary>
        /// <returns></returns> list of collection names
        public IList<string> GetCollectionNames()
        {
            MongoDatabase database = GetDatabase();

            if (database == null)
                throw new MongoException("Error while retrieving database to get collection names");

            return database.GetCollectionNames().ToList();
        }
        
        /// <summary>
        /// Executes a command on the given Func.
        /// </summary>
        /// <typeparam name="TType">Type to determin the collection name</typeparam>
        /// <typeparam name="TReturn">The return type of the execution</typeparam>
        /// <param name="collectionCallback">the Func that will be run for the retrived collection </param>
        /// <returns></returns>
        public TReturn Execute<TType, TReturn>(Func<MongoCollection, TReturn> collectionCallback)
        {
		    return Execute(DetermineCollectionName(typeof(TType)), collectionCallback);
	    }

        /// <summary>
        /// Executes a command on the given Func.
        /// </summary>
        /// <typeparam name="TReturn">The return type of the execution</typeparam>
        /// <param name="collectionName">the collection to use for the execution</param>
        /// <param name="collectionCallback">the Func that will be run for the retrived collection </param>
        /// <returns></returns>
        public virtual TReturn Execute<TReturn>(string collectionName, Func<MongoCollection, TReturn> collectionCallback)
        {
            AssertUtils.ArgumentNotNull(collectionCallback, "collectionCallback");

            try
            {
                MongoCollection collection = GetAndPrepareCollection(GetDatabase(), collectionName);
                return collectionCallback(collection);
            }
            catch (Exception e)
            {
                throw PotentiallyConvertException(e);
            }
        }

        /// <summary>
        /// Gets the database from the <see cref="IMongoDatabaseFactory"/>
        /// </summary>
        /// <returns>
        /// A configured <see cref="MongoDatabase"/>
        /// </returns>
        public MongoDatabase GetDatabase()
        {
            return _mongoDbFactory.GetDatabase();
        }

        /// <summary>
        /// Remove the given object from the collection by id.
        /// </summary>
        /// <param name="objectToRemove">object to remove</param>
        public void Remove(object objectToRemove)
        {
            if (objectToRemove == null)
                throw new InvalidDataAccessApiUsageException("Object passed in to remove can't be null");

            string collectionName = DetermineCollectionName(objectToRemove.GetType());

            Remove(collectionName, objectToRemove);
        }

        /// <summary>
        /// Removes the given object from the given collection.
        /// </summary>
        /// <param name="objectToRemove">object to remove</param>
        /// <param name="collectionName">must not be <code>null</code> or empty.</param>
        public void Remove(string collectionName, object objectToRemove)
        {
            if (collectionName == null)
                throw new InvalidDataAccessApiUsageException("Collection name passed in to remove an object can't be null");

            if (objectToRemove == null)
                throw new InvalidDataAccessApiUsageException("Object passed in to remove can't be null");

            Remove(collectionName, GetIdQueryFor(objectToRemove));
        }

        /// <summary>
        /// Remove all documents that match the provided query document criteria from the the collection used 
        /// to store the entity type. The Class parameter is also used to help convert the Id of the object if 
        /// it is present in the query.
        /// </summary>
        /// <param name="query"></param>
        public void Remove<T>(IMongoQuery query)
        {
            string collectionName = DetermineCollectionName(typeof (T));

            Remove(collectionName, query);
        }

        /// <summary>
        /// Remove all documents from the specified collection that match the provided query document criteria. 
        /// There is no conversion/mapping done for any criteria using the id field.
        /// </summary>
        /// <param name="query">the query document that specifies the criteria used to remove a record</param>
        /// <param name="collectionName">name of the collection where the objects will removed</param>
        public void Remove(string collectionName, IMongoQuery query)
        {
            if (collectionName == null)
                throw new InvalidDataAccessApiUsageException("Collection name passed in to remove an object can't be null");

            if (query == null)
                throw new InvalidDataAccessApiUsageException("Query passed in to remove can't be null");


            Execute<WriteConcernResult>(collectionName, (MongoCollection collection) =>
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("remove using query: " + query + " in collection: " + collection.Name);
                    }

                    if (_writeConcern == null)
                    {
                        return collection.Remove(query);
                    }

                    return collection.Remove(query, _writeConcern);
                });
        }

        /// <summary>
        /// Tries to convert the given <see cref="SystemException"/> into a <see cref="DataAccessException"/> but returns the original
        /// exception if the conversation failed. Thus allows safe rethrowing of the return value.
        /// </summary>
        private Exception PotentiallyConvertException(Exception ex)
        {
            Exception resolved = _exceptionTranslator.TranslateExceptionIfPossible(ex);
            return resolved == null ? ex : resolved;
        }

        /// <summary>
        /// Try to gets the collection name based on entity type
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
	    private string DetermineCollectionName(Type entityType)
        {
		    if (entityType == null)
            {
			    throw new InvalidDataAccessApiUsageException("No class parameter provided, entity collection can't be determined!");
		    }

            var documentAttr = Attribute.GetCustomAttribute(entityType, typeof (DocumentAttribute)) as DocumentAttribute;

            if (documentAttr != null && StringUtils.HasText(documentAttr.CollectionNameName))
                return documentAttr.CollectionNameName;

            return Pluralizer.ToPluralWithUncapitalize(entityType.Name);
        }

        private MongoCollection GetAndPrepareCollection(MongoDatabase db, string collectionName)
        {
            try
            {
                return db.GetCollection(collectionName);
            }
            catch (SystemException e)
            {
                throw PotentiallyConvertException(e);
            }
        }

        /// <summary>
        /// Returns a <see cref="IMongoQuery"/> for the given entity by its id.
        /// </summary>
	    public IMongoQuery GetIdQueryFor(object obj)
        {
		    AssertUtils.ArgumentNotNull(obj, "obj");

            var classMap = BsonClassMap.LookupClassMap(obj.GetType());
            var idMember = classMap.IdMemberMap;

            if (idMember == null || !StringUtils.HasText(idMember.ElementName))
            {
			    throw new MappingException("No id property found for object of type " + obj.GetType().Name);
		    }

            object idProperty = idMember.Getter(obj);

            return Query.EQ(idMember.ElementName, BsonValue.Create(idProperty));
	    }









        public CommandResult RunCommand(string jsonCommand)
        {
            throw new NotImplementedException();
        }

        public CommandResult RunCommand(CommandDocument command)
        {
            throw new NotImplementedException();
        }

        public CommandResult RunCommand(CommandDocument command, int options)
        {
            throw new NotImplementedException();
        }

        public void ExecuteQuery(string collectionName, IMongoQuery query, Func<BsonDocument> dch)
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(Func<MongoDatabase, T> databaseCallback)
        {
            throw new NotImplementedException();
        }

        public T ExecuteInSession<T>(Func<MongoDatabase, T> databaseCallback)
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

        public void DropCollection<T>()
        {
            throw new NotImplementedException();
        }

        public void DropCollection(string collectionName)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<T> FindAll<T>()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<T> FindAll<T>(string collectionName)
        {
            throw new NotImplementedException();
        }

        public MapReduceResult MapReduce(string inputCollectionName, string mapFunction, string reduceFunction)
        {
            throw new NotImplementedException();
        }

        public MapReduceResult MapReduce(string inputCollectionName, string mapFunction, string reduceFunction, IMongoMapReduceOptions mapReduceOptions)
        {
            throw new NotImplementedException();
        }

        public MapReduceResult MapReduce(string inputCollectionName, IMongoQuery query, string mapFunction, string reduceFunction)
        {
            throw new NotImplementedException();
        }

        public MapReduceResult MapReduce(string inputCollectionName, IMongoQuery query, string mapFunction, string reduceFunction, IMongoMapReduceOptions mapReduceOptions)
        {
            throw new NotImplementedException();
        }

        public GeoNearResult<T> GeoNear<T>(IMongoQuery query, double x, double y, int limit)
        {
            throw new NotImplementedException();
        }

        public GeoNearResult<T> GeoNear<T>(string collectionName, IMongoQuery query, double x, double y, int limit)
        {
            throw new NotImplementedException();
        }

        public T FindOne<T>(IMongoQuery query)
        {
            throw new NotImplementedException();
        }

        public T FindOne<T>(IMongoQuery query, string collectionName)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<T> Find<T>(IMongoQuery query)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IList<T> Find<T>(string collectionName, IMongoQuery query)
        {
            throw new NotImplementedException();
        }

        public T FindById<T>(object id)
        {
            throw new NotImplementedException();
        }

        public T FindById<T>(string collectionName, object id)
        {
            throw new NotImplementedException();
        }

        public T FindAndModify<T>(IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public T FindAndModify<T>(string collectionName, IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public T FindAndModify<T>(IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update, FindAndModifyOptions options)
        {
            throw new NotImplementedException();
        }

        public T FindAndModify<T>(string collectionName, IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update, FindAndModifyOptions options)
        {
            throw new NotImplementedException();
        }

        public T FindAndRemove<T>(IMongoQuery query)
        {
            throw new NotImplementedException();
        }

        public T FindAndRemove<T>(string collectionName, IMongoQuery query)
        {
            throw new NotImplementedException();
        }

        public long Count<T>(IMongoQuery query)
        {
            throw new NotImplementedException();
        }

        public long Count(string collectionName, IMongoQuery query)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(T objectToSave)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(string collectionName, T objectToSave)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(System.Collections.Generic.IEnumerable<T> objectsToSave)
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(string collectionName, System.Collections.Generic.IEnumerable<T> objectsToSave)
        {
            throw new NotImplementedException();
        }

        public void InsertAll(System.Collections.Generic.IEnumerable<object> objectsToSave)
        {
            throw new NotImplementedException();
        }

        public void Save<T>(T objectToSave)
        {
            throw new NotImplementedException();
        }

        public void Save<T>(string collectionName, T objectToSave)
        {
            throw new NotImplementedException();
        }

        public WriteConcernResult Upsert<T>(IMongoQuery query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public WriteConcernResult Upsert(string collectionName, IMongoQuery query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public WriteConcernResult UpdateFirst<T>(IMongoQuery query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public WriteConcernResult UpdateFirst(string collectionName, IMongoQuery query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public WriteConcernResult UpdateMulti<T>(IMongoQuery query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }

        public WriteConcernResult UpdateMulti(string collectionName, IMongoQuery query, IMongoUpdate update)
        {
            throw new NotImplementedException();
        }
    }
}
