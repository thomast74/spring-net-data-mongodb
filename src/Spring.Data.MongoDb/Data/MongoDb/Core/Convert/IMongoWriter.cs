// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMongoWriter.cs" company="The original author or authors.">
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

using MongoDB.Bson;
using MongoDB.Driver;
using Spring.Data.Convert;
using Spring.Data.MongoDb.Core.Mapping;

namespace Spring.Data.MongoDb.Core.Convert
{
    /// <summary>
    /// A MongoWriter is responsible for converting an object of type T to the native MongoDB representation DBObject
    /// </summary>
    /// <typeparam name="T">the type of the object to convert to a BsonDocument</typeparam>
    /// <author>Mark Pollack</author>
    /// <author>Thomas Risberg</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public interface IMongoWriter<T> : IEntityWriter<T, BsonDocument> where T : class 
    {
        /// <summary>
        /// Converts the given object into one Mongo will be able to store natively. If the given object can 
        /// already be stored as is, no conversion will happen.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
	    object ConvertToMongoType(T obj);

        /// <summary>
        /// Creates a <see cref="MongoDBRef"/> to refer to the given object.
        /// </summary>
        /// <param name="obj">
        /// the object to create a <see cref="MongoDBRef"/> to link to. The object's type has to carry an id attribute.
        /// </param>
        /// <param name="referingProperty">
        /// the client-side property referring to the object which might carry additional metadata for
        /// the <see cref="MongoDBRef"/> object to create. Can be <code>null</code>.
        /// </param>
        MongoDBRef ToDBRef(T obj, IMongoPersistentProperty referingProperty);
    }
}
