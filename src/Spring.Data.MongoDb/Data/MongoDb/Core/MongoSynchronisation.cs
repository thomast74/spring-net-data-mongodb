#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleMongoDbFactory.cs" company="The original author or authors.">
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

using Spring.Transaction.Support;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Mongo specific Resource Holder Synchronisation implementation
    /// </summary>
    public class MongoSynchronization : ResourceHolderSynchronization<IResourceHolder, object>
    {
        /// <summary>
        /// Creates a Mongo Synchronisation specific resource holder implementation
        /// </summary>
        /// <param name="resourceHolder">the object that holds the resource</param>
        /// <param name="resourceKey">the key to store the resourceHolder</param>
        public MongoSynchronization(IResourceHolder resourceHolder, object resourceKey)
            : base(resourceHolder, resourceKey)
        {
        }
    }
}