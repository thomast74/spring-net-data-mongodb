#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignableTypeFilter.cs" company="The original author or authors.">
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
using System.Linq;
using Common.Logging;
using Spring.Aop;
using Spring.Core.TypeResolution;

namespace Spring.Data.MongoDb.Core.TypeFilters
{
    /// <summary>
    /// A simple filter which matches classes that are assignable to a given type.
    /// </summary>
    public class AssignableTypeFilter : ITypeFilter
    {
        private static readonly ILog Logger = LogManager.GetLogger<AssignableTypeFilter>();

        private Type _requiredType;

        /// <summary>
        /// Create a Type Filter with required type
        /// </summary>
        /// <param name="expression">type name including assembly name</param>
        public AssignableTypeFilter(string expression)
        {
            GetRequiredType(expression);
        }

        /// <summary>
        /// Is loading a Type from a string passed to method in the form [Type.FullName], [Assembly.Name]
        /// </summary>
        private void GetRequiredType(string typeToLoad)
        {
            try
            {
                _requiredType = TypeResolutionUtils.ResolveType(typeToLoad);
            }
            catch (Exception)
            {
                _requiredType = null;
                Logger.Error("Can't load type defined in exoression:" + typeToLoad);
            }
        }

        /// <summary>
        /// Determine a match based on the given type object.
        /// </summary>
        /// <param name="type">Type to compare against</param>
        /// <returns>true if there is a match; false is there is no match</returns>
        public bool Matches(Type type)
        {
            if (_requiredType == null)
                return false;

            return (type.GetInterfaces().Any(i => i.Equals(_requiredType)) || _requiredType.Equals(type.BaseType));
        }
    }
}
