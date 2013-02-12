#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeoSpatialIndexedAttribute.cs" company="The original author or authors.">
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

namespace Spring.Data.MongoDb.Core.Index
{
    /// <summary>
    /// Mark a property to be indexed using MongoDB's geospatial indexing feature.
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Thomas Trageser</author>
    [AttributeUsage(AttributeTargets.Property)]
    public class GeoSpatialIndexedAttribute : Attribute
    {
        public GeoSpatialIndexedAttribute()
            : this(string.Empty)
        {            
        }

        public GeoSpatialIndexedAttribute(string collection)
        {
            Name = string.Empty;
            Collection = collection;
            Min = -180;
            Max = 180;
            Bits = 26;
        }

        /// <summary>
        /// Name of the property in the document that contains the [x, y] or radial coordinates to index.
        /// </summary>
	    public string Name { get; set; }

        /// <summary>
        /// Name of the collection in which to create the index.
        /// </summary>
	    public string Collection { get; set; }

        /// <summary>
        /// Minimum value for indexed values.
        /// </summary>
	    public int Min { get; set; }

        /// <summary>
        /// Maximum value for indexed values.
        /// </summary>
	    public int Max { get; set; }

        /// <summary>
        /// Bits of precision for boundary calculations.
        /// </summary>
	    public int Bits { get; set; }

    }
}
