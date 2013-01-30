﻿#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoCollectionUtils.cs" company="The original author or authors.">
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
using Spring.Util;

namespace Spring.Data.MongoDb
{
    /// <summary>
    /// Helper class featuring helper methods for working with MongoDb collections.
    /// Mainly intended for internal use within the framework.
    /// </summary>
    /// <author>Thomas Risberg</author>   
    /// <author>Thomas Trageser</author>
    public static class MongoCollectionUtils
    {
        /// <summary>
        /// Obtains the collection name to use for the provided class
        /// </summary>
        /// <param name="entityType">The class to determine the preferred collection name for</param>
        /// <returns>
        /// The preferred collection name
        /// </returns>
	    public static string GetPreferredCollectionName(Type entityType)
        {
		    return Pluralizer.ToPlural(StringUtils.Uncapitalize(entityType.Name));
	    }
    }
}
