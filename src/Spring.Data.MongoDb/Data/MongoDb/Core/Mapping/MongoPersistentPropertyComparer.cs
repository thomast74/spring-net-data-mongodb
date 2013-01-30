#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoPersistentPropertyComparer.cs" company="The original author or authors.">
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
using System.Text;
using Spring.Data.Mapping;
using Spring.Data.Mapping.Model;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// <see cref="IComparer{T}"/> implementation inspecting the <see cref="IMongoPersistentProperty"/>'s order.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MongoPersistentPropertyComparer : PersistentPropertyComparer<IPersistentProperty>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="o1"/> and <paramref name="o2"/>, 
        /// as shown in the following table.
        /// Value Meaning Less than zero<paramref name="o1"/> is less than <paramref name="o2"/>.
        /// Zero <paramref name="o1"/> equals <paramref name="o2"/>.
        /// Greater than zero<paramref name="o1"/> is greater than <paramref name="o2"/>.
        /// </returns>
        /// <param name="o1">The first object to compare.</param>
        /// <param name="o2">The second object to compare.</param>
        public override int Compare(IPersistentProperty o1, IPersistentProperty o2)
        {
            if (!(o1 is IMongoPersistentProperty) || !(o2 is IMongoPersistentProperty))
                throw new ArgumentException("Both values must be of type IMongoPersistentProperty.");

            var prop1 = o1 as IMongoPersistentProperty;
            var prop2 = o2 as IMongoPersistentProperty;

            return Compare(prop1, prop2);
        }

        public int Compare(IMongoPersistentProperty o1, IMongoPersistentProperty o2)
        {
            if (o1.FieldOrder == int.MaxValue)
            {
                return 1;
            }

            if (o2.FieldOrder == int.MaxValue)
            {
                return -1;
            }

            return o1.FieldOrder - o2.FieldOrder;
        }
    }
}
