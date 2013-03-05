#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingException.cs" company="The original author or authors.">
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
using System.Runtime.Serialization;

namespace Spring.Data.Mapping.Model
{
    /// <summary>
    /// Thrown in case of a Mapping issue
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Thomas Trageser</author>
    [Serializable]
    public class MappingException : SystemException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        public MappingException()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        public MappingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception.</param>
        /// <param name="inner">The inner exception.</param>
        public MappingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected MappingException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}