#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionOptions.cs" company="The original author or authors.">
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

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Provides a simple wrapper to encapsulate the variety of settings you can use when creating a collection.
    /// </summary>
    /// <author>Thomas Risberg</author>
    /// <author>Thomas Trageser</author>
    public class CollectionOptions
    {
        private int _maxDocuments;
        private int _size;
        private bool _capped;

        /// <summary>
        /// Constructs a new <see cref="CollectionOptions"/> instance.
        /// </summary>
        /// <param name="size">the collection size in bytes, this data space is preallocated</param>
        /// <param name="maxDocuments">the maximum number of documents in the collection.</param>
        /// <param name="capped">true to created a "capped" collection (fixed size with auto-FIFO behavior 
        /// based on insertion order), false otherwise.</param>
        public CollectionOptions(int size, int maxDocuments, bool capped)
        {
            _maxDocuments = maxDocuments;
            _size = size;
            _capped = capped;
        }

        public int MaxDocuments
        {
            get { return _maxDocuments; }
            set { _maxDocuments = value; }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public bool IsCapped
        {
            get { return _capped; }
            set { _capped = value; }
        }
    }
}
