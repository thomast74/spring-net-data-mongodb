// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WriteConcernTypeConverter.cs" company="The original author or authors.">
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
using MongoDB.Driver;

namespace Spring.Data.MongoDb.Core.Converters
{
    /// <summary>
    /// Parse a string to a <see cref="WriteConcern"/>.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Thomas Trageser</author>
    public class WriteConcernTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(bool) || sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            WriteConcern writeConcern;

            if (value is string)
            {
                switch (((string)value).ToUpper())
                {
                    case "ACKNOWLEDGED":
                        writeConcern = WriteConcern.Acknowledged;
                        break;
                    case "NONE":
                    case "UNACKNOWLEDGED":
                        writeConcern = WriteConcern.Unacknowledged;
                        break;
                    case "JOURNALED":
                        writeConcern = new WriteConcern { W = 0, Journal = true };
                        break;
                    case "FSYNCED":
                        writeConcern = new WriteConcern { W = 0, FSync = true };
                        break;
                    case "WMAJORITY":
                        writeConcern = WriteConcern.WMajority;
                        break;
                    case "NORMAL":
                        writeConcern = new WriteConcern { W = 0 };
                        break;
                    case "W2":
                        writeConcern = WriteConcern.W2;
                        break;
                    case "W3":
                        writeConcern = WriteConcern.W3;
                        break;
                    case "W4":
                        writeConcern = WriteConcern.W4;
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("'{0}' is not a supported WriteConcern mode.", value));
                }
            }
            else if (value is bool)
            {
                writeConcern = ((bool)value) ? WriteConcern.Acknowledged : WriteConcern.Unacknowledged;
            }
            else
            {
                throw new InvalidOperationException(string.Format("Object value must be string or bool but is {0}", value.GetType().Name));
            }

            return writeConcern;
        }
    }
}
