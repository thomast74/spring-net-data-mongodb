#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeoIndexedAppConfig.cs" company="The original author or authors.">
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
using MongoDB.Driver;
using Spring.Context.Attributes;
using Spring.Data.MongoDb.Config;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    /// <author></author>
    /// <author></author>
    public class GeoIndexedAppConfig : AbstractMongoConfiguration
    {
        private readonly string _connectionString = "mongodb://localhost";

        public static string GeoDb = "database";
        public static string GeoCollection = "geolocation";

        public override string GetDatabaseName()
        {
	        return GeoDb;
        }

	    [ObjectDef]
	    public override MongoServer MongoServer()
        {
            var client = new MongoClient(_connectionString);
	        return client.GetServer();
        }

	    public override string GetMappingAssembly()
        {
	        return GetType().AssemblyQualifiedName;
        }

        [ObjectDef]
        public LoggingEventListener MappingEventsListener()
        {
		    return new LoggingEventListener();
	    }
    }
}
