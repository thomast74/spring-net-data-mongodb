#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldAttribute.cs" company="The original author or authors.">
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
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Mapping.Annotation
{
    /// <summary>
    /// Annotation to define custom metadata for document fields.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class FieldAttribute : Attribute
    {
        private string _name;
        private int _order;

        /// <summary>
        /// Create an empty <see cref="FieldAttribute"/> with an <see cref="string.Empty"/> name and <see cref="int.MaxValue"/>
        /// as default values.
        /// </summary>
        public FieldAttribute()
            : this(string.Empty, Int32.MaxValue)
        {
        }

        /// <summary>
        /// Creates a <see cref="FieldAttribute"/> with a name and an order.
        /// </summary>
        /// <param name="name">name of the field to use instead of <see cref="MemberInfo.Name"/></param>
        /// <param name="order">if order is not givinh <see cref="int.MaxValue"/> is used.</param>
        public FieldAttribute(string name, int order = Int32.MaxValue)
        {
            Name = name ?? string.Empty;
            Order = order;
        }

        /// <summary>
        /// The name of the field if <see cref="MemberInfo.Name"/> is not wanted.
        /// </summary>
        public string Name { get { return _name; } set { _name = value ?? string.Empty; } }

        /// <summary>
        /// The order in which the field should apear in the document.
        /// </summary>
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

    }
}
