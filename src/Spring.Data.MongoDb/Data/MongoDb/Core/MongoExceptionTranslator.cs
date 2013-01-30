// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoExceptionTranslator.cs" company="The original author or authors.">
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
using MongoDB.Driver;
using Spring.Dao;
using Spring.Dao.Support;

namespace Spring.Data.MongoDb.Core
{
    public class MongoExceptionTranslator : IPersistenceExceptionTranslator
    {

        public DataAccessException TranslateExceptionIfPossible(Exception ex)
        {

            // Check for well-known MongoException subclasses.

            // All other MongoExceptions
            if (ex is DuplicateKeyException)
            {
                return new DuplicateKeyException(ex.Message, ex);
            }
            if (ex is MongoAuthenticationException)
            {
                return new PermissionDeniedDataAccessException(ex.Message, ex);
            }
            if (ex is MongoCommandException)
            {
                return new InvalidDataAccessApiUsageException(ex.Message, ex);
            }
            if (ex is MongoConnectionException)
            {
                return new DataAccessResourceFailureException(ex.Message, ex);
            }
            if (ex is MongoQueryException)
            {
                return new InvalidDataAccessApiUsageException(ex.Message, ex);
            }
            if (ex is MongoInternalException)
            {
                return new InvalidDataAccessResourceUsageException(ex.Message, ex);
            }
            if (ex is MongoException)
            {
                return new UncategorizedMongoDbException(ex.Message, ex);
            }

            // If we get here, we have an exception that resulted from user code,
            // rather than the persistence provider, so we return null to indicate
            // that translation should not occur.
            return null;
        }
    }
}
