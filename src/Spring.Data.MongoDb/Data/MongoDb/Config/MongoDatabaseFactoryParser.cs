#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoDatabaseFactoryParser.cs" company="The original author or authors.">
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
    /// <see cref="IObjectDefinitionParser"/> to parse <code>db-factory</code> elements into <see cref="IObjectDefinition"/>s.
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MongoDatabaseFactoryParser : IObjectDefinitionParser
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
            AssertUtils.ArgumentNotNull(factory, "factory");

            RegisterTypeConverters();
            
		    string id = element.GetAttribute(ObjectNames.MongoId);
            string defaultId = StringUtils.HasText(id) ? id : ObjectNames.MongoDatabaseFactoryDefaultId;
		    string url = element.GetAttribute(ObjectNames.MongoDatabaseFactoryUrl);
		    string mongoRef = element.GetAttribute(ObjectNames.MongoDatabaseFactoryMongoRef);
		    string dbname = element.GetAttribute(ObjectNames.MongoDatabaseFactoryDbname);

            ObjectDefinitionBuilder dbFactoryBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(SimpleMongoDatabaseFactory));

            IObjectDefinition userCredentials = GetUserCredentialsObjectDefinition(element);
            ParseAttribute(element, dbFactoryBuilder, SimpleMongoDatabaseFactory.WriteConcernProperty, ObjectNames.MongoDatabaseFactoryWriteConcern);

		    if (StringUtils.HasText(url))
            {
			    if (StringUtils.HasText(mongoRef) || StringUtils.HasText(dbname) || userCredentials != null)
                {
				    parserContext.ReaderContext.ReportException(element, ObjectNames.DbFactory, "Configure either url or details individually!");
			    }

			    dbFactoryBuilder.AddConstructorArg(GetMongoUrl(url));
                parserContext.Registry.RegisterObjectDefinition(defaultId, dbFactoryBuilder.ObjectDefinition);

                return null;
            }

		    if (StringUtils.HasText(mongoRef))
            {
			    dbFactoryBuilder.AddConstructorArgReference(mongoRef);
		    }
            else
            {
			    dbFactoryBuilder.AddConstructorArg(RegisterMongoObjectDefinition(element));
		    }

		    dbname = StringUtils.HasText(dbname) ? dbname : ObjectNames.MongoDatabaseFactoryDefaultDatabaseName;
		    dbFactoryBuilder.AddConstructorArg(dbname);

		    if (userCredentials != null)
            {
			    dbFactoryBuilder.AddConstructorArg(userCredentials);
		    }

		    parserContext.Registry.RegisterObjectDefinition(defaultId, dbFactoryBuilder.ObjectDefinition);
            
            return null;
        }

        private void RegisterTypeConverters()
        {
            TypeConverterRegistry.RegisterConverter(typeof(WriteConcern), new WriteConcernTypeConverter());
        }

	    private IObjectDefinition RegisterMongoObjectDefinition(XmlElement element)
        {
		    var mongoBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(MongoFactoryObject));

	        string host = element.GetAttribute(ObjectNames.MongoDatabaseFactoryHost);
	        if (!StringUtils.HasText(host))
	            host = ObjectNames.MongoDatabaseFactoryDefaultHostname;

	        string port = element.GetAttribute(ObjectNames.MongoDatabaseFactoryPort);
	        if (!StringUtils.HasText(port))
	            port = ObjectNames.MongoDatabaseFactoryDefaultPort;

            string url = string.Format("mongodb://{0}:{1}", host, port);

	        mongoBuilder.AddPropertyValue(MongoFactoryObject.UrlProperty, url);
	        mongoBuilder.RawObjectDefinition.Source = element;

		    return mongoBuilder.ObjectDefinition;
	    }

        private void ParseAttribute(XmlElement element, ObjectDefinitionBuilder defBuilder, string property, string attribute)
        {
            string value = element.GetAttribute(attribute);
            if (StringUtils.HasText(value))
                defBuilder.AddPropertyValue(property, value);
        }

	    private IObjectDefinition  GetUserCredentialsObjectDefinition(XmlElement element)
        {
		    string username = element.GetAttribute(ObjectNames.MongoDatabaseFactoryUsername);
		    string password = element.GetAttribute(ObjectNames.MongoDatabaseFactoryPassword);

		    if (!StringUtils.HasText(username) && !StringUtils.HasText(password))
			    return null;

		    ObjectDefinitionBuilder userCredentialsBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(MongoCredentials));
		    userCredentialsBuilder.AddConstructorArg(StringUtils.HasText(username) ? username : null);
		    userCredentialsBuilder.AddConstructorArg(StringUtils.HasText(password) ? password : null);

	        userCredentialsBuilder.RawObjectDefinition.Source = element;

	        return userCredentialsBuilder.ObjectDefinition;
        }

	    private IObjectDefinition GetMongoUrl(string url)
        {
		    ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(MongoUrl));
            builder.AddConstructorArg(url);
            
		    return builder.ObjectDefinition;
	    }
    }
}
