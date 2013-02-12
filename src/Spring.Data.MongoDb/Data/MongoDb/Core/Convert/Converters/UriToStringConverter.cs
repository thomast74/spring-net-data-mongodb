#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriToStringConverter.cs" company="The original author or authors.">
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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Spring.Core.TypeConversion;

namespace Spring.Data.MongoDb.Core.Convert.Converters
{
    /// <summary>
    /// Simple TypeConverter to convert Uri into String
    /// </summary>
    /// <author>Thomas Trageser</author>
    public class UriToStringConverter : AbstractTypeConverter<Uri, string>
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is Uri)
            {
                return value.ToString();
            }
            return null;
        }
    }
}
