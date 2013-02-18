#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoClientSettingsFactoryObject.cs" company="The original author or authors.">
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
using MongoDB.Bson;
using MongoDB.Driver;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// A factory object for the construction of a MongoClientSettings instance to be used by MongoFactoryObject
    /// </summary>
    /// <author>Graeme Rocher</author>
    /// <author>Mark Pollack</author>
    /// <author>Thomas Trageser</author>
    public class MongoClientSettingsFactoryObject : IFactoryObject, IInitializingObject
    {
	    private static readonly MongoClientSettings MongoOptions = new MongoClientSettings();

        private TimeSpan _connectTimeout = MongoOptions.ConnectTimeout;
        private ConnectionMode _connectionMode = MongoOptions.ConnectionMode;
        private MongoCredentialsStore _credentialsStore = MongoOptions.CredentialsStore;
        private MongoCredentials _defaultCredentials = MongoOptions.DefaultCredentials;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private GuidRepresentation _guidRepresentation = MongoOptions.GuidRepresentation;
        private bool _ipv6 = MongoOptions.IPv6;
        private TimeSpan _maxConnectionIdleTime = MongoOptions.MaxConnectionIdleTime;
        private TimeSpan _maxConnectionLifeTime = MongoOptions.MaxConnectionLifeTime;
        private int _maxConnectionPoolSize = MongoOptions.MaxConnectionPoolSize;
        private int _minConnectionPoolSize = MongoOptions.MinConnectionPoolSize;
        private ReadPreference _readPreference = MongoOptions.ReadPreference;
        private string _replicaSetName = MongoOptions.ReplicaSetName;
        private TimeSpan _secondaryAcceptableLatency = MongoOptions.SecondaryAcceptableLatency;
        private bool _isServerSet;
        private MongoServerAddress _server = MongoOptions.Server;
        private MongoServerAddress[] _servers = MongoOptions.Servers.ToArray();
        private TimeSpan _socketTimeout = MongoOptions.SocketTimeout;
        private bool _useSsl = MongoOptions.UseSsl;
        private bool _verifySslCertificate = MongoOptions.VerifySslCertificate;
        private int _waitQueueSize = MongoOptions.WaitQueueSize;
        private TimeSpan _waitQueueTimeout = MongoOptions.WaitQueueTimeout;
        private WriteConcern _writeConcern = MongoOptions.WriteConcern;

        public const string ConnectTimeoutProperty = "ConnectTimeout";
        public TimeSpan ConnectTimeout
        {
            get { return _connectTimeout; }
            set { _connectTimeout = value; }
        }

        public const string ConnectionModeProperty = "ConnectionMode";
        public ConnectionMode ConnectionMode
        {
            get { return _connectionMode; }
            set { _connectionMode = value; }
        }
        
        public MongoCredentialsStore CredentialsStore
        {
            get { return _credentialsStore; }
            set { _credentialsStore = value; }
        }
        
        public MongoCredentials DefaultCredentials
        {
            get { return _defaultCredentials; }
            set { _defaultCredentials = value; }
        }

        public const string UsernameProperty = "Username";
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public const string PasswordProperty = "Password";
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public const string GuidRepresentationProperty = "GuidRepresentation";
        public GuidRepresentation GuidRepresentation
        {
            get { return _guidRepresentation; }
            set { _guidRepresentation = value; }
        }

        public const string IPv6Property = "IPv6";
        public bool IPv6
        {
            get { return _ipv6; }
            set { _ipv6 = value; }
        }

        public const string MaxConnectionIdleTimePropert = "MaxConnectionIdleTime";
        public TimeSpan MaxConnectionIdleTime
        {
            get { return _maxConnectionIdleTime; }
            set { _maxConnectionIdleTime = value; }
        }

        public const string MaxConnectionLifeTimeProperty = "MaxConnectionLifeTime";
        public TimeSpan MaxConnectionLifeTime
        {
            get { return _maxConnectionLifeTime; }
            set { _maxConnectionLifeTime = value; }
        }

        public const string MaxConnectionPoolSizeProperty = "MaxConnectionPoolSize";
        public int MaxConnectionPoolSize
        {
            get { return _maxConnectionPoolSize; }
            set { _maxConnectionPoolSize = value; }
        }

        public const string MinConnectionPoolSizeProperty = "MinConnectionPoolSize";
        public int MinConnectionPoolSize
        {
            get { return _minConnectionPoolSize; }
            set { _minConnectionPoolSize = value; }
        }

        public const string ReplicaSetNameProperty = "ReplicaSetName";
        public string ReplicaSetName
        {
            get { return _replicaSetName; }
            set { _replicaSetName = value; }
        }

        public const string ReadPreferenceProperty = "ReadPreference";
        public ReadPreference ReadPreference
        {
            get { return _readPreference; }
            set { _readPreference = value; }
        }

        public const string SecondaryAcceptableLatencyProperty = "SecondaryAcceptableLatency";
        public TimeSpan SecondaryAcceptableLatency
        {
            get { return _secondaryAcceptableLatency; }
            set { _secondaryAcceptableLatency = value; }
        }

        public const string ServerProperty = "Server";
        public MongoServerAddress Server
        {
            get { return _server; }
            set { 
                _server = value;
                _isServerSet = true;
            }
        }

        public const string ServersProperty = "Servers";
        public MongoServerAddress[] Servers
        {
            get { return _servers; }
            set
            {
                _servers = value;
                _isServerSet = false;
            }
        }

        public const string SocketTimeoutProperty = "SocketTimeout";
        public TimeSpan SocketTimeout
        {
            get { return _socketTimeout; }
            set { _socketTimeout = value; }
        }

        public const string UseSslProperty = "UseSsl";
        public bool UseSsl
        {
            get { return _useSsl; }
            set { _useSsl = value; }
        }

        public const string VerifySslCertificateProperty = "VerifySslCertificate";
        public bool VerifySslCertificate
        {
            get { return _verifySslCertificate; }
            set { _verifySslCertificate = value; }
        }

        public const string WaitQueueSizeProeprty = "WaitQueueSize";
        public int WaitQueueSize
        {
            get { return _waitQueueSize; }
            set { _waitQueueSize = value; }
        }

        public const string WaitQueueTimeoutProperty = "WaitQueueTimeout";
        public TimeSpan WaitQueueTimeout
        {
            get { return _waitQueueTimeout; }
            set { _waitQueueTimeout = value; }
        }

        public const string WriteConcernProperty = "WriteConcern";
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = value; }
        }
        



	    public void AfterPropertiesSet()
        {
		    MongoOptions.ConnectTimeout = _connectTimeout;
	        MongoOptions.ConnectionMode = _connectionMode;
	        MongoOptions.CredentialsStore = _credentialsStore;

            if (StringUtils.HasText(_username) && StringUtils.HasText(_password))
                MongoOptions.DefaultCredentials = new MongoCredentials(_username, _password);
            else
    	        MongoOptions.DefaultCredentials = _defaultCredentials;

	        MongoOptions.GuidRepresentation = _guidRepresentation;
	        MongoOptions.IPv6 = _ipv6;
	        MongoOptions.MaxConnectionIdleTime = _maxConnectionIdleTime;
	        MongoOptions.MaxConnectionLifeTime = _maxConnectionLifeTime;
	        MongoOptions.MaxConnectionPoolSize = _maxConnectionPoolSize;
	        MongoOptions.MinConnectionPoolSize = _minConnectionPoolSize;
	        MongoOptions.ReadPreference = _readPreference;
	        MongoOptions.ReplicaSetName = _replicaSetName;
	        MongoOptions.SecondaryAcceptableLatency = _secondaryAcceptableLatency;
            if (_isServerSet)
	            MongoOptions.Server = _server;
            else
	            MongoOptions.Servers = _servers;
	        MongoOptions.SocketTimeout = _socketTimeout;
	        MongoOptions.UseSsl = _useSsl;
	        MongoOptions.VerifySslCertificate = _verifySslCertificate;
	        MongoOptions.WaitQueueSize = _waitQueueSize;
	        MongoOptions.WaitQueueTimeout = _waitQueueTimeout;
	        MongoOptions.WriteConcern = _writeConcern;
	    }

	    public object GetObject()
        {
		    return MongoOptions;
	    }

	    public Type ObjectType
        {
	        get { return typeof (MongoClientSettings); }
        }

	    public bool IsSingleton
        {
	        get { return true; }
        }
    }
}
