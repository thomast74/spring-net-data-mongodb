#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoConventionProfileParser.cs" company="The original author or authors.">
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
using System.Xml;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Spring.Core.TypeResolution;
using Spring.Data.MongoDb.Core;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Convinient parser to create a <see cref="ConventionProfile"/> for MongoDB <see cref="BsonClassMap"/>
    /// </summary>
    /// <author>Thomas Trageser</author>
    public class MongoConventionProfileParser : IObjectDefinitionParser
    {

        public IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            IObjectDefinitionFactory factory = parserContext.ReaderContext.ObjectDefinitionFactory;
            AssertUtils.ArgumentNotNull(factory, "factory");

            string id = element.GetAttribute(ObjectNames.MongoConventionProfileId);
            string defaultId = StringUtils.HasText(id) ? id : ObjectNames.MongoConventionProfileDefaultId;

            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(ConventionProfileFactory));

            SetConvention(element, builder, ObjectNames.ConventionProfileDefaultValue, ConventionProfileFactory.DefaultValueConventionProperty);
            SetConvention(element, builder, ObjectNames.ConventionProfileElementName, ConventionProfileFactory.ElementNameConventionProperty);
            SetConvention(element, builder, ObjectNames.ConventionProfileExtraElementsMember, ConventionProfileFactory.ExtraElementsMemberConventionProperty);
            SetConvention(element, builder, ObjectNames.ConventionProfileIdGenerator, ConventionProfileFactory.IdGeneratorConventionProperty);
            SetConvention(element, builder, ObjectNames.ConventionProfileIdMember, ConventionProfileFactory.IdMemberConventionProperty);
            SetConvention(element, builder, ObjectNames.ConventionProfileIgnoreExtraElements, ConventionProfileFactory.IgnoreExtraElementsConventionProperty);
            SetConvention(element, builder, ObjectNames.ConventionProfileIgnoreIfDefault, ConventionProfileFactory.IgnoreIfDefaultConventionProperty);
            SetConvention(element, builder, ObjectNames.ConventionProfileIgnoreIfNull, ConventionProfileFactory.IgnoreIfNullConventionProperty);
            SetConvention(element, builder, ObjectNames.ConventionProfileMemberFinder, ConventionProfileFactory.MemberFinderConventionProperty);
            SetConvention(element, builder, ObjectNames.ConventionProfileSerializationOptions, ConventionProfileFactory.SerializationOptionsConventionProperty);

            parserContext.Registry.RegisterObjectDefinition(defaultId, builder.ObjectDefinition);

            return null;
        }

        private void SetConvention(XmlElement element, ObjectDefinitionBuilder builder, string attribute, string property)
        {
            string value = string.Empty;
            try
            {
                value = element.GetAttribute(attribute);
                if (StringUtils.HasText(value))
                {
                    Type type = TypeResolutionUtils.ResolveType(value);
                    object instance = Activator.CreateInstance(type);
                    builder.AddPropertyValue(property, instance);
                }
            }
            catch (TypeLoadException e)
            {
                throw new ObjectCreationException(string.Format("Convention of type '{0}' could not be loaded: {1}", attribute, value));
            }
        }
    }
}
