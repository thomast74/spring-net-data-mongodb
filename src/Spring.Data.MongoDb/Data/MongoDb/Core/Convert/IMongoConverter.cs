// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMongoOperations.cs" company="The original author or authors.">
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

using System;
using MongoDB.Bson;
using Spring.Data.Convert;

namespace Spring.Data.MongoDb.Core.Convert
{
    /// <summary>
    /// Central Mongo specific converter interface which combines <see cref="IMongoWriter{T}"/> and <see cref="IMongoReader{T}"/>.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public interface IMongoConverter : IEntityConverter<object, BsonDocument>, IMongoWriter<object>, IMongoReader<object>
    {
    }
}
