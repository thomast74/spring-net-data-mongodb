// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShape.cs" company="The original author or authors.">
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

using System.Collections.Generic;

namespace Spring.Data.MongoDb.Core.Geo
{
    /// <summary>
    /// Common interface for all shapes. Allows building MongoDB representations of them.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public interface IShape
    {
        /// <summary>
        /// Returns the {@link Shape} as a list of usually {@link Double} or {@link List}s of {@link Double}s. Wildcard bound
        /// to allow implementations to return a more concrete element type.
        /// </summary>
        IList<object> ToList();

        /// <summary>
        /// Returns the command to be used to create the {@literal $within} criterion.
        /// </summary>
        /// <returns></returns>
        string Command { get; }
    }
}
