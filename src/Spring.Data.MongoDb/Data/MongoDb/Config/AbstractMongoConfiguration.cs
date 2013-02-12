#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractMongoConfiguration.cs" company="The original author or authors.">
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
using MongoDB.Driver;
using Spring.Collections.Generic;
using Spring.Context.Attributes;
using Spring.Data.MongoDb.Core;
using Spring.Data.MongoDb.Core.Convert;
using Spring.Data.MongoDb.Core.Mapping;
using Spring.Data.MongoDb.Core.Mapping.Annotation;
using Spring.Util;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Base class for Spring Data MongoDB configuration using code config.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [Configuration]
    public abstract class AbstractMongoConfiguration
    {
        /// <summary>
        /// Return the name of the database to connect to.
        /// </summary>
        /// <returns>must not be <code>null</code>.</returns>
	    public abstract string GetDatabaseName();

        /// <summary>
        /// Return the <see cref="MongoServer"/> instance to connect to.
        /// </summary>
	    [ObjectDef]
	    public abstract MongoServer MongoServer();

        /// <summary>
        /// Creates a <see cref="MongoTemplate"/>.
        /// </summary>
	    [ObjectDef]
	    public MongoTemplate MongoTemplate()
        {
		    return new MongoTemplate(MongoDbFactory(), MappingMongoConverter());
	    }

        /// <summary>
        /// Creates a <see cref="SimpleMongoDbFactory"/> to be used by the <see cref="MongoTemplate"/>. 
        /// Will use the <see cref="MongoServer"/> instance configured in <see cref="MongoServer()"/>.
        /// </summary>
	    [ObjectDef]
	    public SimpleMongoDbFactory MongoDbFactory()
        {
            MongoCredentials credentials = GetUserCredentials();

            return credentials == null
                       ? new SimpleMongoDbFactory(MongoServer(), GetDatabaseName())
                       : new SimpleMongoDbFactory(MongoServer(), GetDatabaseName(), credentials);
        }

        /// <summary>
        /// Return the base assembly to scan for mapped <see cref="DocumentAttribute"/>s. Will return the assembly qualified name
        /// of the configuration class' (the concrete class, not this one here) by default. So if you have a Mongo configuration 
        /// extending <see cref="AbstractMongoConfiguration"/> the base assembly of the type will be considered unless the method is
        /// overriden to implement alternate behaviour.
        /// </summary>
        /// <returns>
        /// The base namespace to scan for mapped <see cref="DocumentAttribute"/> classes or <code>null</code>to not enable 
        /// scanning for entities
        /// </returns>
	    public virtual string GetMappingAssembly()
        {
		    return GetType().AssemblyQualifiedName;
	    }

        /// <summary>
        /// Return <see cref="MongoCredentials"/> to be used when connecting to the MongoDB instance or <code>null</code> 
        /// if none shall be used.
        /// </summary>
	    public virtual MongoCredentials GetUserCredentials()
        {
		    return null;
	    }

        /// <summary>
        /// Creates a <see cref="MongoMappingContext"/> equipped with entity classes scanned from the mapping base package.
        /// </summary>
	    [ObjectDef]
	    public MongoMappingContext MongoMappingContext()
        {
		    var mappingContext = new MongoMappingContext
		        {
		            InitialEntitySet = GetInitialEntitySet(),
		            SimpleTypeHolder = CustomConversions().SimpleTypeHolder
		        };
            mappingContext.Initialize();

		    return mappingContext;
	    }

        /// <summary>
        /// Returns a {@link MappingContextIsNewStrategyFactory} wrapped into a {@link CachingIsNewStrategyFactory}.
        /// </summary>
	    [ObjectDef]
	    public IsNewStrategyFactory IsNewStrategyFactory()
        {
		    return new CachingIsNewStrategyFactory(new MappingContextIsNewStrategyFactory(MongoMappingContext()));
	    }

        /// <summary>
        /// Register custom <see cref="MongoTypeConverter{TSource,TTarget}"/>s in a <see cref="CustomConversions"/> object 
        /// if required. These <see cref="CustomConversions"/> will be registered with the {@link #mappingMongoConverter()}
        /// and <see cref="MongoMappingContext()"/>. Returns an empty <see cref="CustomConversions"/> instance by default.
        /// </summary>
        /// <returns>must not be <code>null</code>.</returns>
	    [ObjectDef]
	    public CustomConversions CustomConversions()
        {
		    return new CustomConversions();
	    }

        /// <summary>
        /// Creates a {@link MappingMongoConverter} using the configured {@link #mongoDbFactory()} and
        /// <see cref="MongoMappingContext()"/>. Will get <see cref="CustomConversions()"/> applied.
        /// </summary>
	    [ObjectDef]
	    public MappingMongoConverter MappingMongoConverter()
        {
		    var converter = new MappingMongoConverter(MongoDbFactory(), MongoMappingContext());
		    converter.CustomConversions = CustomConversions();
		    return converter;
	    }

        /// <summary>
        /// Scans the mapping base package for classes annotated with <see cref="DocumentAttribute"/>.
        /// </summary>
	    public Spring.Collections.Generic.ISet<Type> GetInitialEntitySet()
        {
		    string basePackage = GetMappingAssembly();
		    Spring.Collections.Generic.ISet<Type> initialEntitySet = new HashedSet<Type>();

		    if (StringUtils.HasText(basePackage))
		    {
		        var componentProvider = new ClassPathScanningCandidateComponentProvider(false);
			    componentProvider.addIncludeFilter(new AnnotationTypeFilter(Document.class));
			    componentProvider.addIncludeFilter(new AnnotationTypeFilter(Persistent.class));

			    foreach(ObjectDefinition candidate in componentProvider.FindCandidateComponents(basePackage))
                {
				    initialEntitySet.Add(ClassUtils.forName(candidate.getBeanClassName(),
						    AbstractMongoConfiguration.class.getClassLoader()));
			    }
		    }

		    return initialEntitySet;
	    }
    }
}
