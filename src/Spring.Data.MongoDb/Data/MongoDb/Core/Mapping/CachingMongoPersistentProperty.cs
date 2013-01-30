#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachingMongoPersistentProperty.cs" company="The original author or authors.">
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Spring.Data.Mapping;
using Spring.Data.Mapping.Model;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// <see cref="IMongoPersistentProperty"/> caching access to <see cref="IPersistentProperty.IsIdProperty"/>
    /// and <see cref="IMongoPersistentProperty.FieldName"/>.
    /// </summary>
    /// <author></author>
    /// <author></author>
    public class CachingMongoPersistentProperty : BasicMongoPersistentProperty
    {
	    private bool? _isIdProperty;
	    private bool? _isAssociation;
	    private string _fieldName;

        /// <summary>
        /// Creates a new <see cref="CachingMongoPersistentProperty"/>
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <param name="owner"></param>
        /// <param name="simpleTypeHolder"></param>
        public CachingMongoPersistentProperty(PropertyInfo propertyInfo, IMongoPersistentEntity owner, SimpleTypeHolder simpleTypeHolder)
            : base(propertyInfo, owner, simpleTypeHolder)
        {
	    }

	    public override bool IsIdProperty
        {
	        get
	        {
	            if (_isIdProperty == null)
	            {
	                _isIdProperty = base.IsIdProperty;
	            }
	            return _isIdProperty.Value;
	        }
        }

	    public override bool IsAssociation 
        {
	        get
	        {
	            if (_isAssociation == null)
	            {
	                _isAssociation = base.IsAssociation;
	            }
	            return _isAssociation.Value;
	        }
        }

	    public override string FieldName
        {
	        get
	        {
	            if (string.IsNullOrEmpty(_fieldName))
	            {
	                _fieldName = base.FieldName;
	            }

	            return _fieldName;
	        }
        }
    }
}
