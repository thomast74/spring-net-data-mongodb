#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectNames.cs" company="The original author or authors.">
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

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Helper that contains standard names for the mongo parsers within XML configuration
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Thomas Trageser</author>
    public class ObjectNames
    {
        /// ****************************************
        /// * Mongo Configuration Objects
        /// ****************************************
        public const string Mongo = "mongo";
        public const string DbFactory = "db-factory";
        public const string ConventionProfile = "convention-profile";

        public const string MappingContext = "MappingContext";
        public const string IndexHelper = "IndexCreationHelper";
	    public const string ValidatingEventListener = "ValidatingMongoEventListener";

        public const string ExcludeFilterElement = "exclude-filter";
        public const string IncludeFilterElement = "include-filter";

        /// ****************************************
        /// * Mongo Object Attributes and Elements
        /// ****************************************
        public const string MongoId = "id";
        public const string MongoSettings = "settings";
        public const string MongoUrl = "url";
        public const string MongoReplicaSet = "replica-set";
        public const string MongoWriteConcern = "write-concern";

        public const string MongoDefaultId = "Mongo";

        /// ********************************************************
        /// * Mongo Database Factort Attributes and Elements
        /// ********************************************************
        public const string MongoDatabaseFactoryMongoRef = "mongo-ref";
        public const string MongoDatabaseFactoryUrl = "url";
        public const string MongoDatabaseFactoryDbname = "dbname";
        public const string MongoDatabaseFactoryWriteConcern = "write-concern";
        public const string MongoDatabaseFactoryHost = "host";
        public const string MongoDatabaseFactoryPort = "port";
        public const string MongoDatabaseFactoryUsername = "username";
        public const string MongoDatabaseFactoryPassword = "password";

        public const string MongoDatabaseFactoryDefaultId = "MongoDatabaseFactory";
        public const string MongoDatabaseFactoryDefaultDatabaseName = "db";
        public const string MongoDatabaseFactoryDefaultHostname = "localhost";
        public const string MongoDatabaseFactoryDefaultPort = "27017";

        /// ********************************************************
        /// * Mongo ConventionProfile Factory Attribute and Elements
        /// ********************************************************
        public const string MongoConventionProfileId = "id";

        public const string ConventionProfileDefaultValue = "default-value";
        public const string ConventionProfileElementName = "element-name";
        public const string ConventionProfileExtraElementsMember = "extra-elements-member";
        public const string ConventionProfileIdGenerator = "id-generator";
        public const string ConventionProfileIdMember = "id-member";
        public const string ConventionProfileIgnoreExtraElements = "ignore-extra-elements";
        public const string ConventionProfileIgnoreIfDefault = "ignore-if-default";
        public const string ConventionProfileIgnoreIfNull = "ignore-if-null";
        public const string ConventionProfileMemberFinder = "member-finder";
        public const string ConventionProfileSerializationOptions = "serialization-options";
        
        public const string MongoConventionProfileDefaultId = "MongoConventionProfile";
        
        /// ****************************************
        /// * Mongo Settings Object Attributes
        /// ****************************************
        public const string MongoSettingsAttributeConnectTimeout = "connect-timeout";
        public const string MongoSettingsAttributeConnectionMode = "connection-mode";
        public const string MongoSettingsAttributeUsername = "username";
        public const string MongoSettingsAttributePassword = "password";
        public const string MongoSettingsAttributeGuidRepresentation = "guid-representation";
        public const string MongoSettingsAttributeIPv6 = "ipv6";
        public const string MongoSettingsAttributeMaxConnectionIdleTime = "max-connection-idle-time";
        public const string MongoSettingsAttributeMaxConnectionLifeTime = "max-connection-life-time";
        public const string MongoSettingsAttributeMaxConnectionPoolSize = "max-connection-pool-size";
        public const string MongoSettingsAttributeMinConnectionPoolSize = "min-connection-pool-size";
        public const string MongoSettingsAttributeReadPreference = "read-preference";
        public const string MongoSettingsAttributeReplicaSetName = "replica-set-name";
        public const string MongoSettingsAttributeSecondaryAcceptableLatency = "secondary-acceptable-latency";
        public const string MongoSettingsAttributeServer = "server";
        public const string MongoSettingsAttributeServers = "servers";
        public const string MongoSettingsAttributeSocketTimeout = "socket-timeout";
        public const string MongoSettingsAttributeUseSsl = "use-ssl";
        public const string MongoSettingsAttributeVerifySslCertificate = "verify-ssl-certificate";
        public const string MongoSettingsAttributeWaitQueueSize = "wait-queue-size";
        public const string MongoSettingsAttributeWaitQueueTimeout = "wait-queue-timeout";
        public const string MongoSettingsAttributeWriteConcern = "write-concern";
    }
}
