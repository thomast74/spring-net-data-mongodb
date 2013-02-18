// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoServerAddressTypeConverter.cs" company="The original author or authors.">
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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using MongoDB.Driver;

namespace Spring.Data.MongoDb.Core.Converters
{
    public class MongoServerAddressTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof (string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var valueString = value as string;
            if (valueString != null)
            {
                if (Regex.IsMatch(valueString, "[a-zA-Z0-9]*:[0-9]*"))
                {
                    int port;
                    string serverAddress = Regex.Match(valueString, "[a-zA-Z0-9]*:[0-9]*").Value;
                    var values = serverAddress.Split(':');
                    int.TryParse(values[1], out port);

                    return new MongoServerAddress(values[0], port);
                }

                return new MongoServerAddress(valueString);
            }

            throw new InvalidOperationException(string.Format("Object value must be string but is {0}", value.GetType().Name));
        }
    }
}
