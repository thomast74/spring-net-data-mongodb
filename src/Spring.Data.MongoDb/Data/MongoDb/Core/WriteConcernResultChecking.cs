#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WriteResultChecking.cs" company="The original author or authors.">
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

using MongoDB.Driver;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Enum to represent how strict the check of <see cref="WriteConcernResult"/> shall be. It can either be skipped entirely
    /// (use <see cref="None"/>), or errors can be logged (<see cref="Log"/>) or cause an exception to be thrown 
    /// <see cref="Exception"/>.
    /// </summary>
    /// <author>Thomas Risberg</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public enum WriteConcernResultChecking
    {
        None, 
        Log, 
        Exception
    }
}
