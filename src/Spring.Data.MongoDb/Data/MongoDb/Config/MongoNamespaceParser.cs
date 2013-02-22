#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoNamespaceParser.cs" company="The original author or authors.">
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

using Spring.Objects.Factory.Xml;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// <see cref="NamespaceParserSupport"/> implementation for MongoDb repositories.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/mongo",
            SchemaLocationAssemblyHint = typeof(MongoNamespaceParser),
            SchemaLocation = "/Spring.Data.MongoDb.Config/spring-mongo-1.0.xsd")
    ]    
    public class MongoNamespaceParser : NamespaceParserSupport
    {
        /// <summary>
        /// Register the <see cref="IObjectDefinitionParser"/> for
        ///    <see cref="MongoFactoryParser" />
        /// </summary>
        public override void Init()
        {
            RegisterObjectDefinitionParser(ObjectNames.Mongo, new MongoFactoryParser());
            RegisterObjectDefinitionParser(ObjectNames.DbFactory, new MongoDatabaseFactoryParser());
            RegisterObjectDefinitionParser(ObjectNames.ConventionProfile, new ConventionProfileParser());
        }
    }
}
