#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicMongoPersistentProperty.cs" company="The original author or authors.">
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
using System.Numerics;
using System.Reflection;
using Common.Logging;
using MongoDB.Bson;
using Spring.Data.Mapping;
using Spring.Data.Mapping.Model;
using Spring.Data.MongoDb.Core.Mapping.Annotation;
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// MongoDB specific <see cref="IMongoPersistentProperty"/> implementation.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Patryk Wasik</author>
    /// <author>Thomas Trageser</author>
    public class BasicMongoPersistentProperty : AnnotationBasedPersistentProperty, IMongoPersistentProperty
    {
	    private static readonly ILog Log = LogManager.GetLogger<BasicMongoPersistentProperty>();

        private const string IdFieldName = "_id";

        private static readonly ISet<Type> SupportedIdTypes = new HashSet<Type>()
            {
                typeof (ObjectId),
                typeof (string),
                typeof (BigInteger)
            };

        private static readonly ISet<string> SupportedIdPropertyNames = new HashSet<string>()
            {
                "Id",
                "id",
                "_id"
            };

        private static readonly PropertyInfo InnerExceptionField = GetExceptionCauseField();

        private PropertyToFieldNameConverter _propertyToFieldNameConverter;

        /// <sumamry>
        /// Creates a new <see cref="BasicMongoPersistentProperty"/>.
        /// </sumamry>
        /// <param name="fieldInfo"></param>
        /// <param name="owner"></param>
        /// <param name="simpleTypeHolder"></param>
	    public BasicMongoPersistentProperty(PropertyInfo propertyInfo, IMongoPersistentEntity owner, SimpleTypeHolder simpleTypeHolder)
            : base(propertyInfo, owner, simpleTypeHolder)
        {
		    if (IsIdProperty && FieldName != IdFieldName) 
            {
			    Log.Warn("Customizing field name for id property not allowed! Custom name will not be considered!");
		    }

            if (FieldOrder < 0)
            {
                throw new ArgumentOutOfRangeException("propertyInfo", "Order must be greater or equal zero");                
            }

            _propertyToFieldNameConverter = new PropertyToFieldNameConverter();
	    }

	    public override bool IsAssociation 
        {
	        get
	        {
	            return IsDBRefPresent() || base.IsAssociation;
	        }
        }

        /// <summary>
        /// Also considers fields as id that are of supported id type and name.
        /// </summary>
	    public virtual bool IsIdProperty
        {
	        get
	        {
	            if (base.IsIdProperty)
	                return true;

	            // We need to support a wider range of ID types than just the ones that can be converted to an ObjectId
	            return SupportedIdPropertyNames.Contains(_propertyInfo.Name);
	        }
        }

        /// <summary>
        /// Returns the key to be used to store the value of the property inside a Mongo <see cref="BsonDocument"/>.
        /// </summary>
	    public virtual string FieldName
        {
            get
            {
                if (IsIdProperty)
                    return IdFieldName;

                var fieldAttribute = GetFieldAttribute();
                return fieldAttribute != null && StringUtils.HasText(fieldAttribute.Name)
                           ? fieldAttribute.Name
                           : StringUtils.Uncapitalize(_propertyInfo.Name);
            }
        }

	    public int FieldOrder
        {
	        get
	        {
	            var fieldAttribute = GetFieldAttribute();
	            return fieldAttribute != null ? fieldAttribute.Order : Int32.MaxValue;
	        }
        }

	    public override Association CreateAssociation()
        {
		    return new Association(this, null);
	    }

	    public bool IsDbReference
        {
	        get { return IsDBRefPresent(); }
        }

	    public DBRefAttribute DBRef
        {
	        get { return GetDBRefAttribute(); }
        }

	    public bool UsePropertyAccess
        {
	        get { return InnerExceptionField.Equals(PropertyInfo); }
        }

        public PropertyToFieldNameConverter PropertyToFieldNameConverter
        {
            get { return _propertyToFieldNameConverter; }
        }

        private static PropertyInfo GetExceptionCauseField()
        {
            return (typeof (Exception)).GetProperty("InnerException", BindingFlags.Instance | BindingFlags.Public);
        }

        private FieldAttribute GetFieldAttribute()
        {
            var fieldAttribute = Attribute.GetCustomAttribute(_propertyInfo, typeof(FieldAttribute)) as FieldAttribute;
            return fieldAttribute;
        }

        private DBRefAttribute GetDBRefAttribute()
        {
            var dbRefAttribute = Attribute.GetCustomAttribute(_propertyInfo, typeof(DBRefAttribute)) as DBRefAttribute;
            return dbRefAttribute;
        }

        private bool IsDBRefPresent()
        {
            return GetDBRefAttribute() != null;
        }
    }
}
