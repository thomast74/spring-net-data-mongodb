#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoFactoryObject.cs" company="The original author or authors.">
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
using System.Linq;
using MongoDB.Driver;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Convenient factory for configuring MongoDB.
    /// </summary>
    /// <author>Thomas Risberg</author>
    /// <author>Graeme Rocher</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MongoFactoryObject : IFactoryObject, IInitializingObject, IDisposable
    {
	    private MongoServer _mongoServer;

	    private MongoClientSettings _mongoClientSettings;
	    private string _url;
	    private WriteConcern _writeConcern;
	    private string[] _replicaSetSeeds;

        public const string MongoClientSettingsProperty = "MongoClientSettings";
        public MongoClientSettings MongoClientSettings
        {
            get { return _mongoClientSettings; }
            set { _mongoClientSettings = value; }
        }

        public const string UrlProperty = "Url";
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public const string ReplicaSetSeedsProperty = "ReplicaSetSeeds";
        public string[] ReplicaSetSeeds
        {
            get { return _replicaSetSeeds; }
            set
            {
                if (value != null && value.Length > 0)
                {
                    _replicaSetSeeds = value.Where(s => s.Length > 0).ToArray();
                }
            }
        }

        public const string WriteConcernProperty = "WriteConcern";
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = value; }
        }

        public object GetObject()
        {
            return _mongoServer;
        }

        public Type ObjectType
        {
            get
            {
                return typeof(MongoServer);
            }
        }

	    public bool IsSingleton
        {
	        get { return true; }
        }

	    public void AfterPropertiesSet()
	    {
	        MongoClient mongoClient;

            if (_replicaSetSeeds != null && _replicaSetSeeds.Length > 0)
            {
                var url = new MongoUrl(string.Format("mongodb://{0}/?connect=replicaset", StringUtils.CollectionToCommaDelimitedString(_replicaSetSeeds)));
                var settings = MongoClientSettings.FromUrl(url);
                
                if (_writeConcern != null)
                    settings.WriteConcern = _writeConcern;

                mongoClient = new MongoClient(settings);
		    } 
            else if (_mongoClientSettings != null)
            {
                mongoClient = new MongoClient(_mongoClientSettings);
            }
            else if (StringUtils.HasText(_url))
            {
                var url = new MongoUrl(_url);
                var settings = MongoClientSettings.FromUrl(url);

                if (_writeConcern != null)
			        settings.WriteConcern = _writeConcern;

                mongoClient = new MongoClient(settings);
            }
            else
            {
                throw new ObjectCreationException("Mongo configuration needs to have at least one of (replica-set, mongo:settings, url)");
            }

	        _mongoServer = mongoClient.GetServer();
	    }

        public void Dispose()
        {
            if (_mongoServer != null)
                _mongoServer.Disconnect();
        }
    }
}
