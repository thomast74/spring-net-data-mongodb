#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoMappingContext.cs" company="The original author or authors.">
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
using System.Reflection;
using Spring.Context;
using Spring.Data.Mapping;
using Spring.Data.Mapping.Context;
using Spring.Data.Mapping.Model;
using Spring.Data.Util;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// Default implementation of a <see cref="IMappingContext"/> for MongoDB using <see cref="BasicMongoPersistentEntity"/> and
    /// <see cref="BasicMongoPersistentProperty"/> as primary abstractions.
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MongoMappingContext : AbstractMappingContext, IApplicationContextAware
    {
	    private IApplicationContext context;

        /// <summary>
        /// Creates a new <see cref="MongoMappingContext"/>.
        /// </summary>
	    public MongoMappingContext() 
        {
		    SimpleTypeHolder = MongoSimpleTypes.HOLDER;
	    }

	    public override bool ShouldCreatePersistentEntityFor(ITypeInformation typeInformation)
        {
		    return !MongoSimpleTypes.HOLDER.IsSimpleType(typeInformation.Type);
	    }

        public override IMutablePersistentEntity CreatePersistentEntity(ITypeInformation typeInformation)
        {
            var entity = new BasicMongoPersistentEntity(typeInformation);

            if (context != null)
            {
                entity.ApplicationContext = context;
            }

            return entity;
        }

        public override IPersistentProperty CreatePersistentProperty(PropertyInfo propertyInfo,
                                                                     IPersistentEntity owner,
                                                                     SimpleTypeHolder simpleTypeHolder)
        {
            if (owner is IMongoPersistentEntity)
                return new CachingMongoPersistentProperty(propertyInfo, (IMongoPersistentEntity)owner, simpleTypeHolder);

            throw new InvalidOperationException("'owner' is not derieved from IMongoPersistentEntity");
        }

        public override IApplicationContext ApplicationContext
        {
            set
            {
                context = value;
                base.ApplicationContext = value;
            }
        }
    }
}
