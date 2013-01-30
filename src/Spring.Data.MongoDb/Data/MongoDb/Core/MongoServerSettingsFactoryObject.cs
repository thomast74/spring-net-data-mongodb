// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoServerSettingsFactoryObject.cs" company="The original author or authors.">
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
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using Spring.Objects.Factory;

namespace Spring.Data.MongoDb.Core
{
    public class MongoServerSettingsFactoryObject : IFactoryObject, IInitializingObject
    {
        private readonly MongoServerSettings _settings = new MongoServerSettings();

        private MongoCredentials _defaultCredentials;
        private MongoCredentialsStore _credentialsStore;
        private IEnumerable<MongoServerAddress> _servers;

        private ConnectionMode _connectionMode;
        private TimeSpan _connectTimeout;
        private GuidRepresentation _guidRepresentation;
        private bool _ipPv6;
        private TimeSpan _maxConnectionIdleTime;
        private TimeSpan _maxConnectionLifeTime;
        private int _maxConnectionPoolSize;
        private int _minConnectionPoolSize;
        private ReadPreference _readPreference;
        private string _replicaSetName;
        private WriteConcern _writeConcern;
        private TimeSpan _secondaryAcceptableLatency;
        private TimeSpan _socketTimeout;
        private bool _useSsl;
        private bool _verifySslCertificate;
        private int _waitQueueSize;
        private TimeSpan _waitQueueTimeout;

        public MongoServerSettingsFactoryObject()
        {
            _connectionMode = _settings.ConnectionMode;
            _connectTimeout = _settings.ConnectTimeout;
            _credentialsStore = _settings.CredentialsStore;
            _defaultCredentials = _settings.DefaultCredentials;
            _guidRepresentation = _settings.GuidRepresentation;
            _ipPv6 = _settings.IPv6;
            _maxConnectionIdleTime = _settings.MaxConnectionIdleTime;
            _maxConnectionLifeTime = _settings.MaxConnectionLifeTime;
            _maxConnectionPoolSize = _settings.MaxConnectionPoolSize;
            _minConnectionPoolSize = _settings.MinConnectionPoolSize;
            _readPreference = _settings.ReadPreference;
            _replicaSetName = _settings.ReplicaSetName;
            _writeConcern = _settings.WriteConcern;
            _secondaryAcceptableLatency = _settings.SecondaryAcceptableLatency;
            _servers = _settings.Servers;
            _socketTimeout = _settings.SocketTimeout;
            _useSsl = _settings.UseSsl;
            _verifySslCertificate = _settings.VerifySslCertificate;
            _waitQueueSize = _settings.WaitQueueSize;
            _waitQueueTimeout = _settings.WaitQueueTimeout;
        }

        //
        // Summary:
        //     Gets or sets the connection mode.
        public ConnectionMode ConnectionMode
        {
            get { return _connectionMode; }
            set { _connectionMode = value; }
        }

        //
        // Summary:
        //     Gets or sets the connect timeout.
        public TimeSpan ConnectTimeout
        {
            get { return _connectTimeout; }
            set { _connectTimeout = value; }
        }

        //
        // Summary:
        //     Gets or sets the credentials store.
        public MongoCredentialsStore CredentialsStore
        {
            get { return _credentialsStore; }
            set { _credentialsStore = value; }
        }

        //
        // Summary:
        //     Gets or sets the default credentials.
        public MongoCredentials DefaultCredentials
        {
            get { return _defaultCredentials; }
            set { _defaultCredentials = value; }
        }

        //
        // Summary:
        //     Gets or sets the representation to use for Guids.
        public GuidRepresentation GuidRepresentation
        {
            get { return _guidRepresentation; }
            set { _guidRepresentation = value; }
        }

        //
        // Summary:
        //     Gets or sets whether to use IpPv6.
        public bool IpPv6
        {
            get { return _ipPv6; }
            set { _ipPv6 = value; }
        }

        //
        // Summary:
        //     Gets or sets the max connection idle time.
        public TimeSpan MaxConnectionIdleTime
        {
            get { return _maxConnectionIdleTime; }
            set { _maxConnectionIdleTime = value; }
        }

        //
        // Summary:
        //     Gets or sets the max connection life time.
        public TimeSpan MaxConnectionLifeTime
        {
            get { return _maxConnectionLifeTime; }
            set { _maxConnectionLifeTime = value; }
        }

        //
        // Summary:
        //     Gets or sets the max connection pool size.
        public int MaxConnectionPoolSize
        {
            get { return _maxConnectionPoolSize; }
            set { _maxConnectionPoolSize = value; }
        }

        //
        // Summary:
        //     Gets or sets the min connection pool size.
        public int MinConnectionPoolSize
        {
            get { return _minConnectionPoolSize; }
            set { _minConnectionPoolSize = value; }
        }

        //
        // Summary:
        //     Gets or sets the read preferences.
        public ReadPreference ReadPreference
        {
            get { return _readPreference; }
            set { _readPreference = value; }
        }

        //
        // Summary:
        //     Gets or sets the name of the replica set.
        public string ReplicaSetName
        {
            get { return _replicaSetName; }
            set { _replicaSetName = value; }
        }

        //
        // Summary:
        //     Gets or sets the SafeMode to use.
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = value; }
        }

        //
        // Summary:
        //     Gets or sets the acceptable latency for considering a replica set member
        //     for inclusion in load balancing when using a read preference of Secondary,
        //     SecondaryPreferred, and Nearest.
        public TimeSpan SecondaryAcceptableLatency
        {
            get { return _secondaryAcceptableLatency; }
            set { _secondaryAcceptableLatency = value; }
        }

        //
        // Summary:
        //     Gets or sets the list of server addresses (see also Server if using only
        //     one address).
        public IEnumerable<MongoServerAddress> Servers
        {
            get { return _servers; }
            set { _servers = value; }
        }

        //
        // Summary:
        //     Gets or sets the socket timeout.
        public TimeSpan SocketTimeout
        {
            get { return _socketTimeout; }
            set { _socketTimeout = value; }
        }

        //
        // Summary:
        //     Gets or sets whether to use SSL.
        public bool UseSsl
        {
            get { return _useSsl; }
            set { _useSsl = value; }
        }

        //
        // Summary:
        //     Gets or sets whether to verify an SSL certificate.
        public bool VerifySslCertificate
        {
            get { return _verifySslCertificate; }
            set { _verifySslCertificate = value; }
        }

        //
        // Summary:
        //     Gets or sets the wait queue size.
        public int WaitQueueSize
        {
            get { return _waitQueueSize; }
            set { _waitQueueSize = value; }
        }

        //
        // Summary:
        //     Gets or sets the wait queue timeout.
        public TimeSpan WaitQueueTimeout
        {
            get { return _waitQueueTimeout; }
            set { _waitQueueTimeout = value; }
        }


        public void AfterPropertiesSet()
	    {
	        _settings.ConnectTimeout = ConnectTimeout;
	        _settings.ConnectionMode = ConnectionMode;
	        //_settings.CredentialsStore = CredentialsStore;
	        _settings.GuidRepresentation = GuidRepresentation;
	        _settings.IPv6 = IpPv6;
	        _settings.MaxConnectionIdleTime = MaxConnectionIdleTime;
	        _settings.MaxConnectionLifeTime = MaxConnectionLifeTime;
	        _settings.MaxConnectionPoolSize = MaxConnectionPoolSize;
	        _settings.MinConnectionPoolSize = MinConnectionPoolSize;
	        _settings.ReadPreference = ReadPreference;
	        _settings.ReplicaSetName = ReplicaSetName;
	        _settings.WriteConcern = WriteConcern;
	        _settings.SecondaryAcceptableLatency = SecondaryAcceptableLatency;
	        _settings.Servers = Servers;
	        _settings.SocketTimeout = SocketTimeout;
	        _settings.UseSsl = UseSsl;
	        _settings.VerifySslCertificate = VerifySslCertificate;
	        _settings.WaitQueueSize = WaitQueueSize;
	        _settings.WaitQueueTimeout = WaitQueueTimeout;
	    }

	    public object GetObject()
        {
		    return _settings;
	    }

	    public Type ObjectType
        {
	        get { return typeof (MongoServerSettings); }
        }

	    public bool IsSingleton
        {
	        get { return true; }
        }
    }
}
