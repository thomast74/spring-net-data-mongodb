#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicMongoPersistentEntity.cs" company="The original author or authors.">
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
using Spring.Context;
using Spring.Data.Mapping.Model;
using Spring.Data.MongoDb.Core.Mapping.Annotation;
using Spring.Data.Util;
using Spring.Util;
using Spring.Expressions;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// MongoDB specific <see cref="IMongoPersistentEntity" /> implementation that adds Mongo specific meta-data such as the
    /// collection name and the like.
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class BasicMongoPersistentEntity : BasicPersistentEntity, IMongoPersistentEntity, IApplicationContextAware
    {
	    private readonly string _collection;
	    private readonly IExpression _expression;
	    
        private IApplicationContext _context;

        /// <summary>
        /// Creates a new <see cref="BasicMongoPersistentEntity"/> with the given <see cref="ITypeInformation"/>. Will default the
        /// collection name to the entities simple type name.
        /// </summary>
        /// <param name="typeInformation"></param>
	    public BasicMongoPersistentEntity(ITypeInformation typeInformation)
            : base(typeInformation, new MongoPersistentPropertyComparer())
        {
		    Type rawType = typeInformation.Type;
		    string fallback = MongoCollectionUtils.GetPreferredCollectionName(rawType);

	        var document = Attribute.GetCustomAttribute(rawType, typeof (DocumentAttribute)) as DocumentAttribute;
            if (document != null && StringUtils.HasText(document.Collection))
            {
                _collection =  document.Collection;
                _expression = Expression.Parse(_collection);
            }
            else
                _collection = fallback;
        }

	    public IApplicationContext ApplicationContext
        {
	        set { _context = value; }
        }

	    public string Collection
        {
	        get { return _expression == null ? _collection : System.Convert.ToString(_expression.GetValue(_context)); }
        }
    }
}
