#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttributeTypeFilter.cs" company="The original author or authors.">
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
using Common.Logging;
using Spring.Aop;
using Spring.Core.TypeResolution;

namespace Spring.Data.MongoDb.Core.TypeFilters
{
    /// <summary>
    /// A simple filter which matches classes with a given attribute,
    /// checking inherited annotations as well.
    /// </summary>
    public class AttributeTypeFilter : ITypeFilter
    {
        private static readonly ILog Logger = LogManager.GetLogger<AttributeTypeFilter>();

        private Type _requiredType;
        /// <summary>
        /// Creates a Type Filter with required type attribute
        /// </summary>
        /// <param name="expression"></param>
        public AttributeTypeFilter(string expression)
        {
            GetRequiredType(expression);
        }

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

            return (Attribute.GetCustomAttribute(type, _requiredType) != null);
        }
    }
}
