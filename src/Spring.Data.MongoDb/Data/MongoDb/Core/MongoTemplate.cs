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
using System.Text;
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
            return CollectionExists(DetermineCollectionName(typeof (T)));
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

            return Execute<bool>(db => db.CollectionExists(collectionName));
        }

        /// <summary>
        /// Returns the number of documents of the collection
        /// </summary>
        /// <typeparam name="T">the Type from where to get the collection</typeparam>
        /// <returns></returns>
        public long Count<T>()
        {
            return Count(DetermineCollectionName(typeof(T)));
        }
        
        /// <summary>
        /// Returns the number of documents of the collection
        /// </summary>
        /// <param name="collectionName">the name of the collection to get the count of</param>
        /// <returns></returns>
        public long Count(string collectionName)
        {
            AssertUtils.ArgumentHasText(collectionName, collectionName);

            return Execute<long>(collectionName, collection => collection.Count());
        }

        /// <summary>
        /// Returns the number of documents for the given <see cref="QueryDocument"/> by querying the collection of the given
        /// entity class.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public long Count<T>(IMongoQuery query)
        {
            return Count(DetermineCollectionName(typeof (T)), query);
        }

        /// <summary>
        /// Returns the number of documents for the given <see cref="QueryDocument"/> querying the given collection.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="collectionName">collectionName must not be <code>null</code> empty.</param>
        /// <returns></returns>
        public long Count(string collectionName, IMongoQuery query)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");
            AssertUtils.ArgumentNotNull(query, "query");

            return Execute<long>(collectionName, collection => collection.Count(query));
        }

        /// <summary>
        /// Create an uncapped collection with a name based on the provided entity type.
        /// </summary>
        /// <returns>the created collection</returns>
        public MongoCollection<T> CreateCollection<T>()
        {
            return CreateCollection<T>(DetermineCollectionName(typeof (T)));
        }

        /// <summary>
        /// Create a collect with a name based on the provided entity type using the options.
        /// </summary>
        /// <param name="collectionOptions">options to use when creating the collection</param>
        /// <returns>the created collection</returns>
        public MongoCollection<T> CreateCollection<T>(IMongoCollectionOptions collectionOptions)
        {
            return CreateCollection<T>(DetermineCollectionName(typeof(T)), collectionOptions);
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
            AssertUtils.ArgumentHasText(collectionName, collectionName);

            VerifyCollectionNameIsValid(collectionName);

            return Execute<MongoCollection<T>>(db =>
                {
                    CommandResult result = collectionOptions == null
                                               ? db.CreateCollection(collectionName)
                                               : db.CreateCollection(collectionName, collectionOptions);
                    if (!result.Ok)
                        throw new MongoCommandException(result.ErrorMessage);

                    return db.GetCollection<T>(collectionName);
                });
        }


        /// <summary>
        /// Drop the collection with the name indicated by the entity type.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <exception cref="MongoException">if collection could not have been dropped</exception>
        public void DropCollection<T>()
        {
            DropCollection(DetermineCollectionName(typeof(T)));
        }

        /// <summary>
        /// Drop the collection with the given name.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <param name="collectionName">name of the collection to drop/delete</param>
        /// <exception cref="MongoException">if collection could not have been dropped</exception>
        public void DropCollection(string collectionName)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");

            VerifyCollectionNameIsValid(collectionName);

            Execute<bool>(db =>
                {
                    CommandResult result = db.DropCollection(collectionName);

                    if (!result.Ok)
                        throw new MongoCommandException(result.ErrorMessage);

                    return true;
                });
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
            return Execute<TType, TReturn>(DetermineCollectionName(typeof(TType)), collectionCallback);
        }

        /// <summary>
        /// Executes a command on the given Func.
        /// </summary>
        /// <typeparam name="T">The type of the collection</typeparam>
        /// <typeparam name="TReturn">The return type of the execution</typeparam>
        /// <param name="collectionName">the collection to use for the execution</param>
        /// <param name="collectionCallback">the Func that will be run for the retrived collection </param>
        /// <returns></returns>
        public virtual TReturn Execute<T, TReturn>(string collectionName, Func<MongoCollection, TReturn> collectionCallback)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");
            AssertUtils.ArgumentNotNull(collectionCallback, "collectionCallback");

            VerifyCollectionNameIsValid(collectionName);

            try
            {
                MongoCollection<T> collection = GetCollection<T>(collectionName);
                return collectionCallback(collection);
            }
            catch (Exception e)
            {
                throw PotentiallyConvertException(e);
            }
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
            AssertUtils.ArgumentHasText(collectionName, "collectionName");
            AssertUtils.ArgumentNotNull(collectionCallback, "collectionCallback");

            VerifyCollectionNameIsValid(collectionName);

            try
            {
                MongoCollection collection = GetCollection(collectionName);
                return collectionCallback(collection);
            }
            catch (Exception e)
            {
                throw PotentiallyConvertException(e);
            }
        }

        /// <summary>
        /// Executes the given callback action, translating any exceptions as necessary.
        /// <p />
        /// Allows for returning a result object, that is a domain object or a collection of domain objects.
        /// </summary>
        /// <param name="databaseCallback">callback object that specifies the MongoDB actions to perform on the passed
        /// in DB instance.</param>
        /// <returns>a result object returned by the action or <code>null</code></returns>
        public TReturn Execute<TReturn>(Func<MongoDatabase, TReturn> databaseCallback)
        {
            AssertUtils.ArgumentNotNull(databaseCallback, "databaseCallback");

            try
            {
                return databaseCallback(GetDatabase());
            }
            catch (Exception e)
            {
                throw PotentiallyConvertException(e);
            }
        }

        /// <summary>
        /// Map the results of an ad-hoc query on the collection for the entity class to a List of the specified type.
        /// <p />
        /// The query is specified as a <see cref="QueryDocument"/> which can be created either using the <see cref="QueryBuilder{TDocument}"/> or
        /// the more feature rich <see cref="QueryDocument"/>.
        /// </summary>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <returns>the List of converted objects</returns>
        public IList<T> Find<T>(IMongoQuery query)
        {
            return Find<T>(DetermineCollectionName(typeof(T)), query);
        }

        /// <summary>
        /// Map the results of an ad-hoc query on the specified collection to a List of the specified type.
        /// <p />
        /// The query is specified as a <see cref="QueryDocument"/> which can be created either using the <see cref="QueryBuilder{TDocument}"/> 
        /// or the more feature rich <see cref="QueryDocument"/>.
        /// </summary>
        /// <param name="collectionName">name of the collection to retrieve the objects from</param>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <returns>the List of converted objects</returns>
        public IList<T> Find<T>(string collectionName, IMongoQuery query)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");
            AssertUtils.ArgumentNotNull(query, "query");

            return Execute<T, IList<T>>(collectionName, collection =>
                {
                    var result = collection.FindAs<T>(query);
                    return result.ToList();
                });
        }

        /// <summary>
        /// Query for a list of objects of type T from the collection used by the entity class.
        /// <p />
        /// If your collection does not contain a homogeneous collection of types, this operation will not be an 
        /// efficient way to map objects since the test for object type is done in the client and not on the server.
        /// </summary>
        /// <returns>the converted collection</returns>
        public IList<T> FindAll<T>()
        {
            return FindAll<T>(DetermineCollectionName(typeof (T)));
        }

        /// <summary>
        /// Query for a list of objects of type T from the specified collection.
        /// <p />
        /// If your collection does not contain a homogeneous collection of types, this operation will not be an 
        /// efficient way to map objects since the test for class type is done in the client and not on the server.
        /// </summary>
        /// <param name="collectionName"></param> collectionName name of the collection to retrieve the objects from
        /// <returns>the converted collection</returns>
        public IList<T> FindAll<T>(string collectionName)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");

            return Execute<T, IList<T>>(collectionName, collection =>
                {
                    var result = collection.FindAllAs<T>();
                    return result.ToList();
                });
        }

        public T FindAndModify<T>(IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update)
        {
            return FindAndModify<T>(DetermineCollectionName(typeof(T)), query, sortBy, update);
        }

        public T FindAndModify<T>(string collectionName, IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update)
        {
            return FindAndModify<T>(DetermineCollectionName(typeof (T)), query, sortBy, update,
                                    FindAndModifyOptions.Default());
        }

        public T FindAndModify<T>(IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update, FindAndModifyOptions options)
        {
            return FindAndModify<T>(DetermineCollectionName(typeof(T)), query, sortBy, update, options);
        }

        public T FindAndModify<T>(string collectionName, IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update, FindAndModifyOptions options)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");
            AssertUtils.ArgumentNotNull(query, "query");
            AssertUtils.ArgumentNotNull(sortBy, "sortBy");
            AssertUtils.ArgumentNotNull(update, "update");

            return Execute<T, T>(collectionName, collection =>
                {
                    FindAndModifyResult result = collection.FindAndModify(query, sortBy, update, options.IsReturnNew,
                                                                          options.IsUpsert);
                    if (!result.Ok)
                        throw new MongoCommandException(result.ErrorMessage);

                    return result.GetModifiedDocumentAs<T>();
                });
        }

        public T FindAndRemove<T>(IMongoQuery query, IMongoSortBy sortBy)
        {
            return FindAndRemove<T>(DetermineCollectionName(typeof(T)), query, sortBy);
        }

        public T FindAndRemove<T>(string collectionName, IMongoQuery query, IMongoSortBy sortBy) 
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");
            AssertUtils.ArgumentNotNull(query, "query");
            AssertUtils.ArgumentNotNull(sortBy, "sortBy");

            return Execute<T, T>(collectionName, collection =>
            {
                FindAndModifyResult result = collection.FindAndRemove(query, sortBy);
                if (!result.Ok)
                    throw new MongoCommandException(result.ErrorMessage);

                return result.GetModifiedDocumentAs<T>();
            });
        }

        /// <summary>
        /// Returns a document with the given id mapped onto the given type. The collection the query is ran against 
        /// will be derived from the given target type as well.
        /// </summary>
        /// <param name="id">the id of the document to return.</param>
        /// <returns>the document with the given id mapped onto the given target type.</returns>
        public T FindById<T>(object id)
        {
            return FindById<T>(DetermineCollectionName(typeof(T)), id);
        }

        /// <summary>
        /// Returns the document with the given id from the given collection mapped onto the given target class.
        /// </summary>
        /// <param name="id">the id of the document to return</param>
        /// <param name="collectionName">the collection to query for the document</param>
        /// <returns>the document with the given id mapped onto the given target type.</returns>
        public T FindById<T>(string collectionName, object id)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");
            AssertUtils.ArgumentNotNull(id, "id");

            return Execute<T, T>(collectionName, collection => collection.FindOneByIdAs<T>(BsonValue.Create(id)));
        }

        /// <summary>
        /// Map the results of an ad-hoc query on the collection for the entity class to a single instance of an 
        /// object of the specified type.
        /// <p />
        /// The query is specified as a <see cref="QueryDocument"/> which can be created either using the <see cref="QueryBuilder{TDocument}"/> 
        /// or the more feature rich <see cref="QueryDocument"/>.
        /// </summary>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <returns>the converted object</returns>
        public T FindOne<T>(IMongoQuery query)
        {
            return FindOne<T>(DetermineCollectionName(typeof (T)), query);
        }

        /// <summary>
        /// Map the results of an ad-hoc query on the specified collection to a single instance of an object of the
        /// specified type.
        /// <p />
        /// The query is specified as a <see cref="Query"/> which can be created either using the <see cref="QueryBuilder{TDocument}"/> 
        /// or the more feature rich <see cref="Query"/>.
        /// </summary>
        /// <param name="collectionName">name of the collection to retrieve the objects from</param>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <returns>the converted object</returns>
        public T FindOne<T>(string collectionName, IMongoQuery query)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");
            AssertUtils.ArgumentNotNull(query, "query");

            return Execute<T, T>(collectionName, collection => collection.FindOneAs<T>(query));
        }

        /// <summary>
        /// Get a collection by name derived form provided entity type, creating it if it doesn't exist.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <typeparam name="T">Type used to determin the collection name. Class map the retrieved collection to provided type</typeparam>
        /// <returns>an existing collection or a newly created one.</returns>
        public MongoCollection<T> GetCollection<T>()
        {
            return GetCollection<T>(DetermineCollectionName(typeof (T)));
        }

        /// <summary>
        /// Get a collection by name, creating it if it doesn't exist.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <typeparam name="T">Class map the retrieved collection to provided type</typeparam>
        /// <param name="collectionName">name of the collection</param>
        /// <returns>an existing collection or a newly created one.</returns>
        public MongoCollection<T> GetCollection<T>(string collectionName)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");

            VerifyCollectionNameIsValid(collectionName);

            return Execute<MongoCollection<T>>(db =>
                {
                    try
                    {
                        return db.GetCollection<T>(collectionName);
                    }
                    catch (Exception e)
                    {
                        throw PotentiallyConvertException(e);
                    }
                });
        }

        /// <summary>
        /// Get a collection by name, creating it if it doesn't exist.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <typeparam name="T">Class map the retrieved collection to provided type</typeparam>
        /// <param name="collectionName">name of the collection</param>
        /// <returns>an existing collection or a newly created one.</returns>
        public MongoCollection GetCollection(string collectionName)
        {
            AssertUtils.ArgumentHasText(collectionName, "collectionName");

            VerifyCollectionNameIsValid(collectionName);

            return Execute<MongoCollection>(db =>
            {
                try
                {
                    return db.GetCollection(collectionName);
                }
                catch (Exception e)
                {
                    throw PotentiallyConvertException(e);
                }
            });
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
            return Execute<IList<string>>(db =>
                {
                    return db.GetCollectionNames().ToList();
                });
        }
       
        
        /// <summary>
        /// Gets the database from the <see cref="IMongoDatabaseFactory"/>
        /// </summary>
        /// <returns>
        /// A configured <see cref="MongoDatabase"/>
        /// </returns>
        public MongoDatabase GetDatabase()
        {
            MongoDatabase db = _mongoDbFactory.GetDatabase();

            if (db == null)
                throw new DataAccessResourceFailureException("Factory did not provide a valid database");

            return db;
        }

        /// <summary>
        /// Remove the given object from the collection by id.
        /// </summary>
        /// <param name="objectToRemove">object to remove</param>
        public void Remove<T>(T objectToRemove)
        {
            AssertUtils.ArgumentNotNull(objectToRemove, "objectToRemove");

            Remove<T>(DetermineCollectionName(objectToRemove.GetType()), objectToRemove);
        }

        /// <summary>
        /// Removes the given object from the given collection.
        /// </summary>
        /// <param name="objectToRemove">object to remove</param>
        /// <param name="collectionName">must not be <code>null</code> or empty.</param>
        public void Remove<T>(string collectionName, T objectToRemove)
        {
            AssertUtils.ArgumentNotNull(objectToRemove, "objectToRemove");

            Remove<T>(collectionName, GetIdQueryFor(objectToRemove));
        }

        /// <summary>
        /// Remove all documents that match the provided query document criteria from the the collection used 
        /// to store the entity type. The Class parameter is also used to help convert the Id of the object if 
        /// it is present in the query.
        /// </summary>
        /// <param name="query"></param>
        public void Remove<T>(IMongoQuery query)
        {
            Remove<T>(DetermineCollectionName(typeof (T)), query);
        }

        /// <summary>
        /// Remove all documents from the specified collection that match the provided query document criteria. 
        /// There is no conversion/mapping done for any criteria using the id field.
        /// </summary>
        /// <param name="query">the query document that specifies the criteria used to remove a record</param>
        /// <param name="collectionName">name of the collection where the objects will removed</param>
        public void Remove<T>(string collectionName, IMongoQuery query)
        {
            AssertUtils.ArgumentNotNull(query, "query");

            Execute<T,bool>(collectionName, collection =>
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("remove using query: " + query + " in collection: " + collection.Name);
                    }

                    WriteConcernResult result = _writeConcern == null
                                                    ? collection.Remove(query)
                                                    : collection.Remove(query, _writeConcern);
                    
                    if (!result.Ok)
                        throw new MongoCommandException(result.LastErrorMessage);

                    return true;
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

        private void VerifyCollectionNameIsValid(string collectionName)
        {
            if (collectionName == null)
            {
                throw new ArgumentNullException("collectionName");
            }

            if (collectionName == "")
            {
                throw new InvalidDataAccessResourceUsageException("Collection name cannot be empty.");
            }

            if (collectionName.IndexOf('\0') != -1)
            {
                throw new InvalidDataAccessResourceUsageException("Collection name cannot contain null characters.");
            }

            if (collectionName.IndexOf('$') != -1)
            {
                throw new InvalidDataAccessResourceUsageException("Collection name cannot contain $ characters.");
            }

            if (collectionName.StartsWith("system."))
            {
                throw new InvalidDataAccessResourceUsageException("Collection name cannot start with 'system.'.");
            }

            if (Encoding.UTF8.GetBytes(collectionName).Length > 80)
            {
                throw new InvalidDataAccessResourceUsageException("Collection name cannot exceed 121 bytes (after encoding to UTF-8).");
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

        public T ExecuteInSession<T>(Func<MongoDatabase, T> databaseCallback)
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
