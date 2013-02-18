#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoParser.cs" company="The original author or authors.">
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

using System.Xml;
using MongoDB.Driver;
using Spring.Core.TypeConversion;
using Spring.Data.MongoDb.Core;
using Spring.Data.MongoDb.Core.Converters;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Object definition parser for <see cref="MongoServer"/>
    /// </summary>
    /// <author></author>
    /// <author></author>
    public class MongoFactoryParser : IObjectDefinitionParser
    {
        /// <summary>
        /// Parse the specified XmlElement and register the resulting
        ///             ObjectDefinitions with the <see cref="P:Spring.Objects.Factory.Xml.ParserContext.Registry"/> IObjectDefinitionRegistry
        ///             embedded in the supplied <see cref="T:Spring.Objects.Factory.Xml.ParserContext"/>
        /// </summary>
        /// <remarks>
        /// <p>This method is never invoked if the parser is namespace aware
        ///             and was called to process the root node.
        ///             </p>
        /// </remarks>
        /// <param name="element">The element to be parsed.
        ///             </param><param name="parserContext">TThe object encapsulating the current state of the parsing process. 
        ///             Provides access to a IObjectDefinitionRegistry
        ///             </param>
        /// <returns>
        /// The primary object definition.
        /// </returns>
        public IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            IObjectDefinitionFactory factory = parserContext.ReaderContext.ObjectDefinitionFactory;
            IObjectDefinitionRegistry registry = parserContext.ReaderContext.Registry;
            AssertUtils.ArgumentNotNull(factory, "factory");

            RegisterTypeConverters();

		    string id = element.GetAttribute(ObjectNames.MongoId);
            string defaultedId = StringUtils.HasText(id) ? id : ObjectNames.MongoDefaultId;

            ObjectDefinitionBuilder mongoDefBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(MongoFactoryObject));

            ParseAttribute(element, mongoDefBuilder, MongoFactoryObject.UrlProperty, ObjectNames.MongoUrl);
            ParseAttribute(element, mongoDefBuilder, MongoFactoryObject.ReplicaSetSeedsProperty, ObjectNames.MongoReplicaSet);
            ParseAttribute(element, mongoDefBuilder, MongoFactoryObject.WriteConcernProperty, ObjectNames.MongoWriteConcern);

            ParseMongoClientSettings(element, mongoDefBuilder);

            registry.RegisterObjectDefinition(defaultedId, mongoDefBuilder.ObjectDefinition);

            return null;
        }

        private void RegisterTypeConverters()
        {
            TypeConverterRegistry.RegisterConverter(typeof(WriteConcern), new WriteConcernTypeConverter());
            TypeConverterRegistry.RegisterConverter(typeof(ReadPreference), new ReadPreferenceTypeConverter());
            TypeConverterRegistry.RegisterConverter(typeof(MongoServerAddress), new MongoServerAddressTypeConverter());
        }

        private void ParseAttribute(XmlElement element, ObjectDefinitionBuilder mongoDefBuilder, string property, string attribute)
        {
            string value = element.GetAttribute(attribute);
            if (StringUtils.HasText(value))
                mongoDefBuilder.AddPropertyValue(property, value);           
        }

        private void ParseMongoClientSettings(XmlElement element, ObjectDefinitionBuilder mongoDefBuilder)
	    {
	        if (!element.HasChildNodes)
	            return;

	        foreach (XmlElement node in element.ChildNodes)
	        {
	            if (node.LocalName != ObjectNames.MongoSettings)
                    continue;

	            var settingsDefBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof (MongoClientSettingsFactoryObject));

                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeConnectTimeout, MongoClientSettingsFactoryObject.ConnectTimeoutProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeConnectionMode, MongoClientSettingsFactoryObject.ConnectionModeProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeUsername, MongoClientSettingsFactoryObject.UsernameProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributePassword, MongoClientSettingsFactoryObject.PasswordProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeGuidRepresentation, MongoClientSettingsFactoryObject.GuidRepresentationProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeIPv6, MongoClientSettingsFactoryObject.IPv6Property);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeMaxConnectionIdleTime, MongoClientSettingsFactoryObject.MaxConnectionIdleTimePropert);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeMaxConnectionLifeTime, MongoClientSettingsFactoryObject.MaxConnectionLifeTimeProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeMaxConnectionPoolSize, MongoClientSettingsFactoryObject.MaxConnectionPoolSizeProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeMinConnectionPoolSize, MongoClientSettingsFactoryObject.MinConnectionPoolSizeProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeReadPreference, MongoClientSettingsFactoryObject.ReadPreferenceProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeReplicaSetName, MongoClientSettingsFactoryObject.ReplicaSetNameProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeSecondaryAcceptableLatency, MongoClientSettingsFactoryObject.SecondaryAcceptableLatencyProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeServer, MongoClientSettingsFactoryObject.ServerProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeServers, MongoClientSettingsFactoryObject.ServersProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeSocketTimeout, MongoClientSettingsFactoryObject.SocketTimeoutProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeUseSsl, MongoClientSettingsFactoryObject.UseSslProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeVerifySslCertificate, MongoClientSettingsFactoryObject.VerifySslCertificateProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeWaitQueueSize, MongoClientSettingsFactoryObject.WaitQueueSizeProeprty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeWaitQueueTimeout, MongoClientSettingsFactoryObject.WaitQueueTimeoutProperty);
                SetPropertyValue(settingsDefBuilder, node, ObjectNames.MongoSettingsAttributeWriteConcern, MongoClientSettingsFactoryObject.WriteConcernProperty);

                mongoDefBuilder.AddPropertyValue(MongoFactoryObject.MongoClientSettingsProperty, settingsDefBuilder.ObjectDefinition);

	            break;
	        }
	    }

        private void SetPropertyValue(ObjectDefinitionBuilder builder, XmlElement element, string attrName, string propertyName)
        {
            AssertUtils.ArgumentNotNull(builder, "builder");
            AssertUtils.ArgumentNotNull(element, "element");
            AssertUtils.ArgumentHasText(attrName, "attrName");
            AssertUtils.ArgumentHasText(propertyName, "propertyName");

            string attr = element.GetAttribute(attrName);
            if (StringUtils.HasText(attr))
            {
                builder.AddPropertyValue(propertyName, attr);
            }
        }
    }
}
