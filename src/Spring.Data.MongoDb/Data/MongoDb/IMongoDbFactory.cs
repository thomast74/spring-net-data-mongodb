﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMongoDbFactory.cs" company="The original author or authors.">
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

using MongoDB.Driver;

namespace Spring.Data.MongoDb
{
    /// <summary>
    /// Interface for factories creating <see cref="MongoDatabase"/> instances.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Thomas Trageser</author>
    public interface IMongoDbFactory
    {
        /// <summary>
        /// Creates a default <see cref="MongoDatabase"/> instance.
        /// </summary>
        /// <returns>A MongoDatabase instance</returns>
	    MongoDatabase GetDatabase();

        /// <summary>
        /// Creates a <see cref="MongoDatabase"/> instance to access the database with the given name.
        /// </summary>
        /// <param name="dbName">must not be null or empty.</param>
        /// <returns>A MongoDatabase instance</returns>
        MongoDatabase GetDatabase(string dbName);
    }
}
