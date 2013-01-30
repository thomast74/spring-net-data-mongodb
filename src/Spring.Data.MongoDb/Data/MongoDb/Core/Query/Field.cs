// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Field.cs" company="The original author or authors.">
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
using MongoDB.Bson;
using MongoDB.Driver;

namespace Spring.Data.MongoDb.Core.Query
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Thomas Trageser</author>
    public class Field
    {
        private IDictionary<string, int> _criterias = new Dictionary<string, int>();
        private IDictionary<string, object> _slices = new Dictionary<string, object>();

        public Field Include(string key)
        {
            _criterias.Add(key, 1);
            return this;
        }

        public Field Exclude(string key)
        {
            _criterias.Add(key, 0);
            return this;
        }

        public Field Slice(string key, int size)
        {
            _slices.Add(key, size);
            return this;
        }

        public Field Slice(string key, int offset, int size)
        {
            _slices.Add(key, new [] { offset, size });
            return this;
        }

        public FieldsDocument FieldsDocument
        {
            get
            {
                var dbo = new FieldsDocument();
                foreach(var k in _criterias.Keys)
                {
                    dbo.Add(k, _criterias[k]);
                }
                foreach(var k in _slices.Keys)
                {
                    dbo.Add(k, new BsonDocument("$slice", BsonValue.Create(_slices[k])));
                }
                return dbo;
            }
        }

    }
}
