#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DBRefAttribute.cs" company="The original author or authors.">
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
using MongoDB.Driver;

namespace Spring.Data.MongoDb.Core.Mapping.Annotation
{
    /// <summary>
    /// An annotation that indicates the annotated field is to be stored using a <see cref="MongoDBRef"/>.
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [AttributeUsage(AttributeTargets.Field)]
    public class DBRefAttribute : Attribute
    {
        private string _database;
        private string _collection;


        public DBRefAttribute()
            : this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Markes a field as <see cref="MongoDBRef"/>. If no database name is given the default value 
        /// of <see cref="String.Empty"/> is used and means it is stored in the same database.
        /// </summary>
        /// <param name="database">The database where the refered entity resides</param>
        /// <param name="collection">The collection where the reference is refering to</param>
        public DBRefAttribute(string database, string collection)
        {
            _database = database ?? string.Empty;
            _collection = collection ?? string.Empty;
        }

        /// <summary>
        /// The name of the database where the referred entity resides
        /// </summary>
        public string Database { get { return _database; } set { _database = value; } }

        /// <summary>
        /// The collection where the reference is refering to
        /// </summary>
        public string Colection { get { return _collection; } set { _collection = value; } }
    }
}
