// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMongoPersistentProperty.cs" company="The original author or authors.">
//   Copyright 2002-2012 the original author or authors.
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

using System;
using MongoDB.Driver;
using Spring.Data.Mapping;
using Spring.Data.MongoDb.Core.Mapping.Annotation;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// MongoDB specific <see cref="IPersistentProperty"/> extension
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Patryk Wasik</author>
    /// <author>Thomas Trageser</author>
    public interface IMongoPersistentProperty : IPersistentProperty
    {
        /// <summary>
        /// Returns the name of the field a property is persisted to.
        /// </summary>
	    string FieldName { get; }

        /// <summary>
        /// Returns the order of the field if defined. Will return -1 if undefined.
        /// </summary>
	    int FieldOrder { get; }

        /// <summary>
        /// Returns whether the propert is a <see cref="MongoDBRef"/>. If this returns <code>true</code> you can expect
        /// <see cref="DBRef"/> to return an non-<code>null</code> value.
        /// </summary>
	    bool IsDbReference { get; }

        /// <summary>
        /// Returns the <see cref="MongoDBRef"/> if the property is a reference.
        /// </summary>
	    DBRefAttribute DBRef { get; }

        /// <summary>
        /// Returns whether the property is representing the version attribute of an entity.
        /// </summary>
	    bool IsVersionProperty { get; }

        /// <summary>
        /// Simple <see cref="Converter{TInput,TOutput}"/> implementation to transform a 
        /// <see cref="IMongoPersistentProperty" /> 
        /// into its field name.
        /// </summary>
        PropertyToFieldNameConverter PropertyToFieldNameConverter { get; }

        /// <summary>
        /// Returns whether property access shall be used for reading the property value. This means it will use the getter
        /// instead of field access.
        /// </summary>
	    bool UsePropertyAccess { get; }
    }
}
