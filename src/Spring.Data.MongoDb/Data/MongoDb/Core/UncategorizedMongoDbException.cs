#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UncategorizedMongoDbException.cs" company="The original author or authors.">
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
using Spring.Dao;

namespace Spring.Data.MongoDb
{
    /// <summary>
    /// Exception thrown in case no specific MongoException can be translated
    /// </summary>
    /// <author>Thomas Trageser</author>
    public class UncategorizedMongoDbException : UncategorizedDataAccessException
    {
        /// <summary>
        /// Creates a UncategorizedMongoDbException with a message and the root cause
        /// </summary>
        /// <param name="message">the error message to report</param>
        /// <param name="cause">the root cause of the exception</param>
        public UncategorizedMongoDbException(string message, Exception cause)
            : base(message, cause)
        {
        }

    }
}
