// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializationUtils.cs" company="The original author or authors.">
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Spring.Data.MongoDb.Core.Query
{
    /// <summary>
    /// Utility methods for JSON serialization.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public static class SerializationUtils
    {
	    /// <summary>
	    /// Serializes the given object into pseudo-JSON meaning it's trying to create a JSON representation as far as possible
	    /// but falling back to the given object's {@link Object#toString()} method if it's not serializable. Useful for
	    /// printing raw {@link DBObject}s containing complex values before actually converting them into Mongo native types.
	    /// </summary>
	    /// <param name="value"></param>
	    /// <returns></returns>
	    public static string SerializeToJsonSafely(object value)
        {
		    if (value == null)
            {
			    return null;
		    }

		    try
            {
			    return JsonConvert.SerializeObject(value);
		    }
            catch (Exception e)
            {
                var type = value.GetType();
                if (value is BsonDocument) 
                {
				    return ToString(((BsonDocument)value).ToDictionary());
			    }
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition()
                        .GetInterfaces()
                        .Any(t => t.GetGenericTypeDefinition() == typeof (IEnumerable<>)))
                {
                    return ToString((IEnumerable<object>) value);
                }
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition()
                        .GetInterfaces()
                        .Any(t => t.GetGenericTypeDefinition() == typeof (IDictionary<,>)))
                {
                    return ToString((IDictionary<string, object>) value);
                }

                return string.Format("{{ object: {0} }}", value.ToString());
		    }
	    }

	    private static string ToString(IDictionary<string, object> source)
	    {
            return IterableToDelimitedString<KeyValuePair<string, object>>(source.GetEnumerator(), "{ ", " }",
                                             new Converter<KeyValuePair<string, object>, object>(
                                                 input => string.Format("{0}: {1}", input.Key, SerializeToJsonSafely(input.Value))));
	    }

	    private static String ToString(IEnumerable<object> source)
	    {
	        return IterableToDelimitedString(source.GetEnumerator(), "[ ", " ]", SerializeToJsonSafely);
	    }

        /// <summary>
        /// Creates a string representation from the given {@link Iterable} prepending the postfix, applying the given
        /// {@link Converter} to each element before adding it to the result {@link String}, concatenating each element with
        /// {@literal ,} and applying the postfix.
        /// </summary>
        /// <param name="source"></param> 
        /// <param name="prefix"></param>
        /// <param name="postfix"></param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        private static string IterableToDelimitedString<T>(IEnumerator<T> source, string prefix, string postfix,
                                                           Converter<T, object> transformer)
        {
            var builder = new StringBuilder(prefix);
            var first = true;

            while (source.MoveNext())
            {
                if (!first)
                    builder.Append(", ");
                if (first) first = false;

                builder.Append(transformer.Invoke(source.Current));
            }

            return builder.Append(postfix).ToString();
        }
    }
}
