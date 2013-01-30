// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReadPreferenceConverter.cs" company="The original author or authors.">
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
using System.ComponentModel;
using System.Reflection;
using MongoDB.Driver;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Parse a string value to a <see cref="ReadPreference"/>.
    /// </summary>
    /// <author>Thomas Trageser</author>
    public class ReadPreferenceConverter : TypeConverter
    {
        /// <summary>
        /// Checks if sourceType is a string object
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType">the type of object value to convert</param>
        /// <returns>Return <code>true</code> if object value type is a string, <code>false</code> for any other object value type</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        /// <summary>
        /// Converts the given string value into <see cref="ReadPreference"/> instance object.
        /// </summary>
        /// <returns>
        /// A ReadPreference instance object
        /// </returns>
        /// <exception cref="InvalidCastException">If given string value can not be matched to ReadPreference</exception>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture. </param><param name="value">The <see cref="T:System.Object"/> to convert. </param><exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var type = typeof (ReadPreference);
            try
            {
                var pi = type.GetProperty((string) value, BindingFlags.Public | BindingFlags.Static);
                return pi.GetValue(null, null);
            }
            catch (Exception)
            {
                throw new InvalidCastException(string.Format("Cast from '{0}' to a ReadPreference is not possible",
                                                             value));
            }
        }
    }
}
