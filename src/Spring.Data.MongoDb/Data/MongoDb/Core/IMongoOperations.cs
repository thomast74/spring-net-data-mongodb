#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMongoOperations.cs" company="The original author or authors.">
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
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Interface that specifies a basic set of MongoDB operations. Implemented by <see cref="MongoTemplate"/>.
    /// Not often used but a useful option for extensibility and testability.
    /// </summary>
    /// <author>Thomas Risberg</author>
    /// <author>Mark Pollack</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public interface IMongoOperations
    {
        /// <summary>
        /// The collection name used for the specified type by this template.
        /// </summary>
        /// <returns>Collection Name</returns>
        string GetCollectionName<T>();

        /// <summary>
        /// Execute the a MongoDB command expressed as a JSON string. This will call the method JSON.parse
        /// that is part of the MongoDB driver to convert the JSON string to a DBObject. Any errors that result
        /// from executing this command will be converted into Spring's DAO exception hierarchy.
        /// </summary>
        /// <param name="jsonCommand">a MongoDB command expressed as a JSON string.</param>
        CommandResult RunCommand(string jsonCommand);

        /// <summary>
        /// Execute a MongoDB command. Any errors that result from executing this command will be converted
        /// into Spring's DAO exception hierarchy.
        /// </summary>
        /// <param name="command">a MongoDB command</param>
        CommandResult RunCommand(CommandDocument command);

        /// <summary>
        /// Execute a MongoDB command. Any errors that result from executing this command will be converted
        /// into Spring's DAO exception hierarchy.
        /// </summary>
        /// <param name="command">a MongoDB command</param>
        /// <param name="options">query options to use</param>
        CommandResult RunCommand(CommandDocument command, int options);

        /// <summary>
        /// Execute a MongoDB query and iterate over the query results on a per-document basis with a 
        /// DocumentCallbackHandler.
        /// </summary>
        /// <param name="collectionName">name of the collection to retrieve the objects from</param>
        /// <param name="query">
        /// the query class that specifies the criteria used to find a record and also an optional fields
        /// specification
        /// </param>
        /// <param name="dch">the handler that will extract results, one document at a time</param>
        void ExecuteQuery(string collectionName, IMongoQuery query, Func<BsonDocument> dch);

        /// <summary>
        /// Executes the given callback action, translating any exceptions as necessary.
        /// <p />
        /// Allows for returning a result object, that is a domain object or a collection of domain objects.
        /// </summary>
        /// <param name="databaseCallback">callback object that specifies the MongoDB actions to perform on the passed
        /// in DB instance.</param>
        /// <returns>a result object returned by the action or <code>null</code></returns>
        T Execute<T>(Func<MongoDatabase, T> databaseCallback);

        /// <summary>
        /// Executes the given callback action on the entity collection of the specified class.
        /// <p />
        /// Allows for returning a result object, that is a domain object or a collection of domain objects.
        /// </summary>
        /// <typeparam name="TType">The type of the collection</typeparam>
        /// <typeparam name="TReturn">The return type of the callback</typeparam>
        /// <param name="collectionCallback">callback object that specifies the MongoDB action</param>
        /// <returns></returns> a result object returned by the action or <tt>null</tt>
        TReturn Execute<TType, TReturn>(Func<MongoCollection, TReturn> collectionCallback);

        /// <summary>
        /// Executes the given callback action on the collection of the given name.
        /// <p />
        /// Allows for returning a result object, that is a domain object or a collection of domain objects.
        /// </summary>
        /// <typeparam name="TType">The type of the collection</typeparam>
        /// <typeparam name="TReturn">The return type of the callback</typeparam>
        /// <param name="collectionName">the name of the collection that specifies which DBCollection instance</param>
        /// <param name="collectionCallback">callback object that specifies the MongoDatabase action the callback action will be passed into</param>
        /// <returns>a result object returned by the action or <code>null</code></returns>
        TReturn Execute<TType, TReturn>(string collectionName, Func<MongoCollection, TReturn> collectionCallback);

        /// <summary>
        /// Executes the given callback within the same connection to the database so as to ensure 
        /// consistency in a write heavy environment where you may read the data that you wrote.
        /// <p />
        /// Allows for returning a result object, that is a domain object or a collection of domain objects.
        /// </summary>
        /// <param name="databaseCallback">callback that specified the MongoDB actions to perform on the DB instance</param>
        /// <returns>a result object returned by the action or <code>null</code></returns>
        T ExecuteInSession<T>(Func<MongoDatabase, T> databaseCallback);

        /// <summary>
        /// Create an uncapped collection with a name based on the provided entity type.
        /// </summary>
        /// <typeparam name="T">Class to map to collection to</typeparam>
        /// <returns>the created collection</returns>
        MongoCollection<T> CreateCollection<T>();

        /// <summary>
        /// Create a collect with a name based on the provided entity type using the options.
        /// </summary>
        /// <typeparam name="T">Class to map to collection to</typeparam>
        /// <param name="collectionOptions">options to use when creating the collection</param>
        /// <returns>the created collection</returns>
        MongoCollection<T> CreateCollection<T>(IMongoCollectionOptions collectionOptions);

        /// <summary>
        /// Create an uncapped collection with the provided name.
        /// </summary>
        /// <param name="collectionName">name of the collection</param>
        /// <returns>the created collection</returns>
        MongoCollection<T> CreateCollection<T>(string collectionName);

        /// <summary>
        /// Create a collect with the provided name and options.
        /// </summary>
        /// <typeparam name="T">Class to map to collection to</typeparam>
        /// <param name="collectionName">name of the collection</param>
        /// <param name="collectionOptions">options to use when creating the collection.</param>
        /// <returns>the created collection</returns>
        MongoCollection<T> CreateCollection<T>(string collectionName, IMongoCollectionOptions collectionOptions);

        /// <summary>
        /// A set of collection names.
        /// </summary>
        /// <returns></returns> list of collection names
        IList<string> GetCollectionNames();

        /// <summary>
        /// Get a collection by name derived form provided entity type, creating it if it doesn't exist.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <typeparam name="T">Type used to determin the collection name. Class map the retrieved collection to provided type</typeparam>
        /// <returns>an existing collection or a newly created one.</returns>
        MongoCollection<T> GetCollection<T>();

        /// <summary>
        /// Get a collection by name, creating it if it doesn't exist.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <typeparam name="T">Class map the retrieved collection to provided type</typeparam>
        /// <param name="collectionName">name of the collection</param>
        /// <returns>an existing collection or a newly created one.</returns>
        MongoCollection<T> GetCollection<T>(string collectionName);

        /// <summary>
        /// Check to see if a collection with a name indicated by the entity type exists.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <typeparam name="T">Type to determine the collection and name</typeparam>
        /// <returns><code>true</code> if a collection with the given name is found, <code>false</code> otherwise.</returns>
        bool CollectionExists<T>();

        /// <summary>
        /// Check to see if a collection with a given name exists.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <param name="collectionName">name of the collection</param>
        /// <returns><code>true</code> if a collection with the given name is found, <code>false</code> otherwise.</returns>
        bool CollectionExists(string collectionName);

        /// <summary>
        /// Drop the collection with the name indicated by the entity type.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <returns>true if collection was dropped successfully, false if an error happened</returns>
        void DropCollection<T>();

        /// <summary>
        /// Drop the collection with the given name.
        /// <p />
        /// Translate any exceptions as necessary.
        /// </summary>
        /// <param name="collectionName">name of the collection to drop/delete</param>
        /// <returns>true if collection was dropped successfully, false if an error happened</returns>
        void DropCollection(string collectionName);

        /// <summary>
        /// Query for a list of objects of type T from the collection used by the entity class.
        /// <p />
        /// If your collection does not contain a homogeneous collection of types, this operation will not be an 
        /// efficient way to map objects since the test for object type is done in the client and not on the server.
        /// </summary>
        /// <returns>the converted collection</returns>
        IList<T> FindAll<T>();

        /// <summary>
        /// Query for a list of objects of type T from the specified collection.
        /// <p />
        /// If your collection does not contain a homogeneous collection of types, this operation will not be an 
        /// efficient way to map objects since the test for class type is done in the client and not on the server.
        /// </summary>
        /// <param name="collectionName"></param> collectionName name of the collection to retrieve the objects from
        /// <returns>the converted collection</returns>
        IList<T> FindAll<T>(string collectionName);
            
        /// <summary>
        /// Execute a map-reduce operation. The map-reduce operation will be formed with an output type of INLINE
        /// </summary>
        /// <param name="inputCollectionName">the collection where the map-reduce will read from</param>
        /// <param name="mapFunction">The JavaScript map function</param>
        /// <param name="reduceFunction">The JavaScript reduce function</param>
        /// <returns>The results of the map reduce operation</returns> 
        MapReduceResult MapReduce(string inputCollectionName, string mapFunction, string reduceFunction);

        /// <summary>
        /// Execute a map-reduce operation that takes additional map-reduce options.
        /// </summary>
        /// <param name="inputCollectionName">the collection where the map-reduce will read from</param>
        /// <param name="mapFunction">The JavaScript map function</param>
        /// <param name="reduceFunction">The JavaScript reduce function</param>
        /// <param name="mapReduceOptions">Options that specify detailed map-reduce behavior</param>
        /// <returns>The results of the map reduce operation</returns>
        MapReduceResult MapReduce(string inputCollectionName, string mapFunction, string reduceFunction,
                                         IMongoMapReduceOptions mapReduceOptions);

        /// <summary>
        /// Execute a map-reduce operation that takes a query. The map-reduce operation will be formed with an 
        /// output type of INLINE
        /// </summary>
        /// <param name="query">The query to use to select the data for the map phase</param>
        /// <param name="inputCollectionName">the collection where the map-reduce will read from</param>
        /// <param name="mapFunction">The JavaScript map function</param>
        /// <param name="reduceFunction">The JavaScript reduce function</param>
        /// <returns>The results of the map reduce operation</returns>
        MapReduceResult MapReduce(string inputCollectionName, IMongoQuery query, string mapFunction,
                                         string reduceFunction);

        /// <summary>
        /// Execute a map-reduce operation that takes a query and additional map-reduce options
        /// </summary>
        /// <param name="query">The query to use to select the data for the map phase</param>
        /// <param name="inputCollectionName">The collection where the map-reduce will read from</param>
        /// <param name="mapFunction">The JavaScript map function</param>
        /// <param name="reduceFunction">The JavaScript reduce function</param>
        /// <param name="mapReduceOptions">Options that specify detailed map-reduce behavior</param>
        /// <returns>The results of the map reduce operation</returns>
        MapReduceResult MapReduce(string inputCollectionName, IMongoQuery query, string mapFunction,
                                         string reduceFunction, IMongoMapReduceOptions mapReduceOptions);

        /// <summary>
        /// Returns <see cref="GeoNearResult"/> for all entities matching the given <see cref="QueryDocument"/>. Will consider entity mapping
        /// information to determine the collection the query is ran against.
        /// </summary>
        /// <param name="query">must not be <code>null</code>.</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        GeoNearResult<T> GeoNear<T>(IMongoQuery query, double x, double y, int limit);

        /// <summary>
        /// Returns <see cref="GeoNearResult"/> for all entities matching the given <see cref="QueryDocument"/>. Will consider entity mapping
        /// information to determine the collection the query is ran against.
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="query">must not be <code>null</code>.</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        GeoNearResult<T> GeoNear<T>(string collectionName, IMongoQuery query, double x, double y, int limit);

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
        T FindOne<T>(IMongoQuery query);

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
        T FindOne<T>(string collectionName, IMongoQuery query);

        /// <summary>
        /// Map the results of an ad-hoc query on the collection for the entity class to a List of the specified type.
        /// <p />
        /// The query is specified as a <see cref="QueryDocument"/> which can be created either using the <see cref="QueryBuilder{TDocument}"/> or
        /// the more feature rich <see cref="QueryDocument"/>.
        /// </summary>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <returns>the List of converted objects</returns>
        IList<T> Find<T>(IMongoQuery query);

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
        IList<T> Find<T>(string collectionName, IMongoQuery query);

        /// <summary>
        /// Returns a document with the given id mapped onto the given type. The collection the query is ran against 
        /// will be derived from the given target type as well.
        /// </summary>
        /// <param name="id">the id of the document to return.</param>
        /// <returns>the document with the given id mapped onto the given target type.</returns>
        T FindById<T>(object id);

        /// <summary>
        /// Returns the document with the given id from the given collection mapped onto the given target class.
        /// </summary>
        /// <param name="id">the id of the document to return</param>
        /// <param name="collectionName">the collection to query for the document</param>
        /// <returns>the document with the given id mapped onto the given target type.</returns>
        T FindById<T>(string collectionName, object id);

        /// <summary>
        /// Map the results of an ad-hoc query on the collection for the entity type to a single instance of an 
        /// object of the specified type. The first document that matches the query is returned and also updated
        /// from the collection in the database.
        /// <p />
        /// The query is specified as a <see cref="QueryBuilder{TDocument}"/>.
        /// </summary>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an 
        /// optional fields specification</param>
        /// <param name="sortBy"></param>
        /// <param name="update">A builder for creating update modifiers.</param>
        /// <returns>the converted object</returns>
        T FindAndModify<T>(IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update);

        /// <summary>
        /// Map the results of an ad-hoc query on the collection for the entity type to a single instance of an 
        /// object of the specified type. The first document that matches the query is returned and also updated 
        /// from the collection in the database.
        /// <p />
        /// The query is specified as a <see cref="QueryBuilder{TDocument}"/>.
        /// </summary>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <param name="sortBy"></param>
        /// <param name="update">A builder for creating update modifiers.</param>
        /// <param name="collectionName">the collection to query</param>
        /// <returns>the converted object</returns>
        T FindAndModify<T>(string collectionName, IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update);

        /// <summary>
        /// Map the results of an ad-hoc query on the collection for the entity type to a single instance of an 
        /// object of the specified type. The first document that matches the query is returned and also updated 
        /// from the collection in the database.
        /// <p />
        /// The query is specified as a <see cref="QueryDocument"/> which can be created either using the <see cref="QueryBuilder{TDocument}"/>
        /// or the more feature rich <see cref="QueryDocument"/>.
        /// </summary>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <param name="sortBy"></param>
        /// <param name="update">A builder for creating update modifiers.</param>
        /// <param name="options"></param>
        /// <returns>the converted object</returns>
        T FindAndModify<T>(IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update, FindAndModifyOptions options);

        /// <summary>
        /// Map the results of an ad-hoc query on the collection for the entity type to a single instance of an 
        /// object of the specified type. The first document that matches the query is returned and also updated 
        /// from the collection in the database.
        /// <p />
        /// The query is specified as a <see cref="QueryDocument"/> which can be created either using the <see cref="QueryBuilder{TDocument}"/>
        /// or the more feature rich <see cref="QueryDocument"/>.
        /// </summary>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <param name="sortBy"></param>
        /// <param name="update">A builder for creating update modifiers.</param>
        /// <param name="options"></param>
        /// <param name="collectionName">the collection to query</param>
        /// <returns>the converted object</returns>
        T FindAndModify<T>(string collectionName, IMongoQuery query, IMongoSortBy sortBy, IMongoUpdate update, FindAndModifyOptions options);

        /// <summary>
        /// Map the results of an ad-hoc query on the collection for the entity type to a single instance of an 
        /// object of the specified type. The first document that matches the query is returned and also removed 
        /// from the collection in the database.
        /// <p />
        /// The query is specified as a <see cref="QueryDocument"/> which can be created either using the <see cref="QueryBuilder{TDocument}"/>
        /// or the more feature rich <see cref="QueryDocument"/>.
        /// </summary>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <param name="sortBy">the sort by clause to define the first document to remove</param>
        /// <returns>the converted object</returns>
        T FindAndRemove<T>(IMongoQuery query, IMongoSortBy sortBy);

        /// <summary>
        /// Map the results of an ad-hoc query on the specified collection to a single instance of an object of the
        /// specified type. The first document that matches the query is returned and also removed from the collection
        /// in the database.
        /// <p />
        /// The query is specified as a <see cref="QueryDocument"/> which can be created either using the <see cref="QueryBuilder{TDocument}"/> 
        /// or the more feature rich <see cref="QueryDocument"/>.
        /// </summary>
        /// <param name="collectionName">name of the collection to retrieve the objects from</param>
        /// <param name="query">the query class that specifies the criteria used to find a record and also an optional
        /// fields specification</param>
        /// <param name="sortBy">the sort by clause to define the first document to remove</param>
        /// <returns>the converted object</returns>
        T FindAndRemove<T>(string collectionName, IMongoQuery query, IMongoSortBy sortBy);

        /// <summary>
        /// Returns the number of documents of the collection
        /// </summary>
        /// <typeparam name="T">the Type from where to get the collection</typeparam>
        /// <returns></returns>
        long Count<T>();

        /// <summary>
        /// Returns the number of documents of the collection
        /// </summary>
        /// <param name="collectionName">the name of the collection to get the count of</param>
        /// <returns></returns>
        long Count(string collectionName);

        /// <summary>
        /// Returns the number of documents for the given <see cref="QueryDocument"/> by querying the collection of the given
        /// entity class.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        long Count<T>(IMongoQuery query);

        /// <summary>
        /// Returns the number of documents for the given <see cref="QueryDocument"/> querying the given collection.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="collectionName">collectionName must not be <code>null</code> empty.</param>
        /// <returns></returns>
        long Count(string collectionName, IMongoQuery query);

        /// <summary>
        /// Insert the object into the collection for the entity type of the object to save.
        /// <p />
        /// If you object has an "Id' property, it will be set with the generated Id from MongoDB. If your Id property
        /// is a String then MongoDB ObjectId will be used to populate that string. Otherwise, the conversion from 
        /// ObjectId to your property type will be handled by Spring's BeanWrapper class that leverages Spring 3.0's
        /// new Type Conversion API.
        /// See <a href="http://static.springsource.org/spring/docs/3.0.x/reference/validation.html#core-convert"> 
        /// Spring 3 Type Conversion"</a> for more details.
        /// <p />
        /// Insert is used to initially store the object into the database. To update an existing object use the save 
        /// method.
        /// </summary>
        /// <param name="objectToSave">the object to store in the collection.</param>
        void Insert<T>(T objectToSave);

        /// <summary>
        /// Insert the object into the specified collection.
        /// <p />
        /// Insert is used to initially store the object into the database. To update an existing object use the 
        /// save method.
        /// </summary>
        /// <param name="objectToSave">the object to store in the collection</param>
        /// <param name="collectionName">name of the collection to store the object in</param>
        void Insert<T>(string collectionName, T objectToSave);

        /// <summary>
        /// Insert a Collection of objects into a collection in a single batch write to the database.
        /// </summary>
        /// <param name="objectsToSave">the list of objects to save</param>
        void Insert<T>(IEnumerable<T> objectsToSave);

        /// <summary>
        /// Insert a list of objects into the specified collection in a single batch write to the database.
        /// </summary>
        /// <param name="objectsToSave">the list of objects to save</param>
        /// <param name="collectionName">name of the collection to store the object in</param>
        void Insert<T>(string collectionName, IEnumerable<T> objectsToSave);

        /// <summary>
        /// Insert a mixed Collection of objects into a database collection determining the collection name to 
        /// use based on the class.
        /// </summary>
        /// <param name="objectsToSave">the list of objects to save</param>
        void InsertAll(IEnumerable<object> objectsToSave);

        /// <summary>
        /// Save the object to the collection for the entity type of the object to save. This will perform an 
        /// insert if the object is not already present, that is an 'upsert'.
        /// <p />
        /// If you object has an "Id' property, it will be set with the generated Id from MongoDB. If your Id property
        /// is a String then MongoDB ObjectId will be used to populate that string. Otherwise, the conversion from 
        /// ObjectId to your property type will be handled by Spring's BeanWrapper class that leverages Spring 3.0's 
        /// new Type Conversion API.
        /// See <a href="http://static.springsource.org/spring/docs/3.0.x/reference/validation.html#core-convert"> 
        /// Spring 3 Type Conversion"</a> for more details.
        /// </summary>
        /// <param name="objectToSave">the object to store in the collection</param>
        void Save<T>(T objectToSave);

        /// <summary>
        /// Save the object to the specified collection. This will perform an insert if the object is not already
        /// present, that is an 'upsert'.
        /// <p />
        /// If you object has an "Id' property, it will be set with the generated Id from MongoDB. If your Id property
        /// is a String then MongoDB ObjectId will be used to populate that string. Otherwise, the conversion from 
        /// ObjectId to your property type will be handled by Spring's BeanWrapper class that leverages Spring 3.0's 
        /// new Type Cobnversion API.
        /// See <a href="http://static.springsource.org/spring/docs/3.0.x/reference/validation.html#core-convert">
        /// Spring 3 Type Conversion"</a> for more details.
        /// </summary>
        /// <param name="objectToSave">the object to store in the collection</param>
        /// <param name="collectionName">name of the collection to store the object in</param>
        void Save<T>(string collectionName, T objectToSave);

        /// <summary>
        /// Performs an upsert. If no document is found that matches the query, a new document is created and inserted by
        /// combining the query document and the update document.
        /// </summary>
        /// <param name="query">the query document that specifies the criteria used to select a record to be upserted
        /// </param>
        /// <param name="update">the update document that contains the updated object or $ operators to manipulate the 
        /// existing object</param>
        /// <returns>the SafeModeResult which lets you access the results of the previous write</returns>
        WriteConcernResult Upsert<T>(IMongoQuery query, IMongoUpdate update);

        /// <summary>
        /// Performs an upsert. If no document is found that matches the query, a new document is created and inserted by
        /// combining the query document and the update document.
        /// </summary>
        /// <param name="query">the query document that specifies the criteria used to select a record to be updated</param>
        /// <param name="update">the update document that contains the updated object or $ operators to manipulate the 
        /// existing object</param>
        /// <param name="collectionName">name of the collection to update the object in</param>
        /// <returns>the WriteResult which lets you access the results of the previous write</returns>
        WriteConcernResult Upsert(string collectionName, IMongoQuery query, IMongoUpdate update);

        /// <summary>
        /// Updates the first object that is found in the collection of the entity class that matches the query
        /// document with the provided update document.
        /// </summary>
        /// <param name="query">the query document that specifies the criteria used to select a record to be updated</param>
        /// <param name="update">update the update document that contains the updated object or $ operators to 
        /// manipulate the existing object</param>
        /// <returns>the SafeModeResult which lets you access the results of the previous write</returns>
        WriteConcernResult UpdateFirst<T>(IMongoQuery query, IMongoUpdate update);

        /// <summary>
        /// Updates the first object that is found in the specified collection that matches the query document criteria with
        /// the provided updated document.
        /// </summary>
        /// <param name="query">the query document that specifies the criteria used to select a record to be updated</param>
        /// <param name="update">the update document that contains the updated object or $ operators to manipulate
        /// the existing object</param>
        /// <param name="collectionName">name of the collection to update the object in</param>
        /// <returns>the SafeModeResult which lets you access the results of the previous write</returns>
        WriteConcernResult UpdateFirst(string collectionName, IMongoQuery query, IMongoUpdate update);

        /// <summary>
        /// Updates all objects that are found in the collection for the entity type that matches the query document 
        /// criteria with the provided updated document.
        /// </summary>
        /// <param name="query">the query document that specifies the criteria used to select a record to be updated</param>
        /// <param name="update">the update document that contains the updated object or $ operators to manipulate 
        /// the existing object</param>
        /// <returns>the SafeModeResult which lets you access the results of the previous write</returns>
        WriteConcernResult UpdateMulti<T>(IMongoQuery query, IMongoUpdate update);

        /// <summary>
        /// Updates all objects that are found in the specified collection that matches the query document criteria with the
        /// provided updated document.
        /// </summary>
        /// <param name="query">the query document that specifies the criteria used to select a record to be updated</param>
        /// <param name="update">the update document that contains the updated object or $ operators to manipulate 
        /// the existing object.</param>
        /// <param name="collectionName">name of the collection to update the object in</param>
        /// <returns>the WriteResult which lets you access the results of the previous write.</returns>
        WriteConcernResult UpdateMulti(string collectionName, IMongoQuery query, IMongoUpdate update);

        /// <summary>
        /// Remove the given object from the collection by id.
        /// </summary>
        /// <param name="objectToRemove">object to remove</param>
        void Remove<T>(T objectToRemove);

        /// <summary>
        /// Removes the given object from the given collection.
        /// </summary>
        /// <param name="objecToRemove">object to remove</param>
        /// <param name="collectionName">must not be <code>nukk</code> or empty.</param>
        void Remove<T>(string collectionName, T objecToRemove);

        /// <summary>
        /// Remove all documents that match the provided query document criteria from the the collection used 
        /// to store the entity type. The Class parameter is also used to help convert the Id of the object if 
        /// it is present in the query.
        /// </summary>
        /// <typeparam name="T">The type of the collection</typeparam>
        /// <param name="query"></param>
        void Remove<T>(IMongoQuery query);

        /// <summary>
        /// Remove all documents from the specified collection that match the provided query document criteria. 
        /// There is no conversion/mapping done for any criteria using the id field.
        /// </summary>
        /// <typeparam name="T">The type of the collection</typeparam>
        /// <param name="query">the query document that specifies the criteria used to remove a record</param>
        /// <param name="collectionName">name of the collection where the objects will removed</param>
        void Remove<T>(string collectionName, IMongoQuery query);
    }
}
