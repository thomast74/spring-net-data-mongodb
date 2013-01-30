// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIndexOperations.cs" company="The original author or authors.">
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

using System.Collections.Generic;
using MongoDB.Driver;
using Spring.Data.MongoDb.Core.Index;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Index operations on a collection.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public interface IIndexOperations
    {
        /// <summary>
        /// Ensure that an index for the provided <see cref="IIndexDefinition"/> exists for the collection indicated by the entity
        /// class. If not it will be created.
        /// </summary>
        /// <param name="indexDefinition">must not be <code>null</code></param>
        void EnsureIndex(IIndexDefinition indexDefinition);

        /// <summary>
        /// Drops an index from this collection.
        /// </summary>
        /// <param name="name">name of index to drop</param>
        void DropIndex(string name);

        /// <summary>
        /// Drops all indices from this collection.
        /// </summary>
        void DropAllIndexes();

        /// <summary>
        /// Clears all indices that have not yet been applied to this collection.
        /// </summary>
        void ResetIndexCache();

        /// <summary>
        /// Returns the index information on the collection.
        /// </summary>
        /// <returns>index information on the collection</returns>
        IList<IndexInfo> GetIndexInfo();
    }
}
