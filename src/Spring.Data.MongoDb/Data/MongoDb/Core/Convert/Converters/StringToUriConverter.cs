#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringToUriConverter.cs" company="The original author or authors.">
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
using Spring.Core.TypeConversion;

namespace Spring.Data.MongoDb.Core.Convert.Converters
{
    /// <summary>
    /// Simple TypeConverter to convert a String into an Uri
    /// </summary>
    /// <author></author>
    /// <author></author>
    public class StringToUriConverter : AbstractTypeConverter<string, Uri>
    {
        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var uriString = value as string; 
            if (uriString != null)
            {
                Uri uri;
                if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                    return uri;
            }
            return null;
        }
    }
}
