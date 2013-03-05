#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoCollectionTestHelper.cs" company="The original author or authors.">
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

using MongoDB.Driver;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Test helper to mock a <see cref="MongoCollection"/> class
    /// </summary>
    /// <author>Thomas Trageser</author>
    public class MongoCollectionTestHelper<T> : MongoCollection<T>
    {
        public MongoCollectionTestHelper(MongoDatabase database, MongoCollectionSettings<T> settings)
            : base(database, settings)
        {
        }

        public new MongoCollectionSettings Settings
        {
            get { return base.Settings; }
        }

    }
}
