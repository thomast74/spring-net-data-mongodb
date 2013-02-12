#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomConversions.cs" company="The original author or authors.">
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
using System.Linq;
using Spring.Collections.Generic;
using Spring.Core.TypeConversion;
using Spring.Data.Mapping.Model;
using Spring.Data.MongoDb.Core.Convert.Converters;
using Spring.Data.MongoDb.Core.Mapping;
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Convert
{
    /// <summary>
    /// Value object to capture custom conversion. That is essentially a <see cref="List{T}"/> of converters and 
    /// some additional logic around them. The converters are pretty much builds up two sets of types which Mongo
    /// basic types <see cref="MongoSimpleTypes.MONGO_SIMPLE_TYPES"/> can be converted into and from. These types 
    /// will be considered simple ones (which means they neither need deeper inspection nor nested conversion. Thus
    /// the <see cref="CustomConversions"/> also act as factory for <see cref="SimpleTypeHolder"/>.
    /// </summary>
    /// <author></author>
    /// <author></author>
    public class CustomConversions
    {
	    private readonly Spring.Collections.Generic.ISet<Type> _customSimpleTypes;
	    private readonly SimpleTypeHolder _simpleTypeHolder;

	    private readonly List<TypeConverter> _converters;

        /// <summary>
        /// Creates an empty <see cref="CustomConversions"/> object.
        /// </summary>
	    public CustomConversions() 
            : this(new List<TypeConverter>())
        {
	    }

        /// <summary>
        /// Creates a new <see cref="CustomConversions"/> instance registering the given converters.
        /// </summary>
        /// <param name="converters">must not be <code>null</code></param>
	    public CustomConversions(IList<TypeConverter> converters)
        {
		    AssertUtils.ArgumentNotNull(converters, "converters");

		    _customSimpleTypes = new HashedSet<Type>();

		    _converters = new List<TypeConverter>();
		    _converters.Add(new BigIntegerToStringConverter());
		    _converters.Add(new StringToBigIntegerConverter());
		    _converters.Add(new UriToStringConverter());
		    _converters.Add(new StringToUriConverter());
		    _converters.AddRange(converters);

		    foreach(var converter in converters)
            {
			    RegisterConversion(converter);
		    }

		    _simpleTypeHolder = new SimpleTypeHolder(_customSimpleTypes, MongoSimpleTypes.HOLDER);
	    }

        /// <summary>
        /// Returns the underlying <see cref="SimpleTypeHolder"/>.
        /// </summary>
	    public SimpleTypeHolder SimpleTypeHolder
        {
	        get { return _simpleTypeHolder; }
        }

        /// <summary>
        /// Returns whether the given type is considered to be simple. That means it's either a general simple type or we have
        /// a writing <see cref="AbstractTypeConverter{TSource,TTarget}"/> registered for a particular type.
        /// </summary>
        /// <param name="type">must not be <code>null</code></param>
	    public bool IsSimpleType(Type type)
        {
            AssertUtils.ArgumentNotNull(type, "type");

		    return _simpleTypeHolder.IsSimpleType(type);
	    }


        /// <summary>
        /// Returns whether the given type is considered to be simple. That means it's either a general simple type or we have
        /// a writing <see cref="AbstractTypeConverter{TSource,TTarget}"/> registered for a particular type.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
	    public bool IsSimpleType<TType>()
        {
		    return _simpleTypeHolder.IsSimpleType(typeof(TType));
	    }

        /// <summary>
        /// Registers a conversion for the given converter.
        /// </summary>
	    private void RegisterConversion(TypeConverter converter)
        {
            AssertUtils.ArgumentNotNull(converter, "converter");
	        AssertUtils.IsTrue(converter.GetType().BaseType == typeof(AbstractTypeConverter<,>),
	                           "Converter must derive from AbstractTypeConverter<TSource, TTarget>.");

	        TypeConverterRegistry.RegisterConverter(converter);
        }

        /// <summary>
        /// Returns the target type to convert to in case we have a custom conversion registered to convert the given source
        /// type into a Mongo native one.
        /// </summary>
        /// <param name="source">must not be <code>null</code></param>
	    public Type GetCustomWriteTarget(Type source)
        {
		    return GetCustomWriteTarget(source, null);
	    }

        /// <summary>
        /// Returns the target type to convert to in case we have a custom conversion registered to convert the given source
        /// type into a Mongo native one.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        public Type GetCustomWriteTarget<TSource>()
        {
            return GetCustomWriteTarget(typeof(TSource), null);
        }

        /// <summary>
	    /// Returns the target type we can write an onject of the given source type to. The returned type might be a subclass
	    /// oth the given expected type though. If {@code expectedTargetType} is {@literal null} we will simply return the
	    /// first target type matching or {@literal null} if no conversion can be found.
        /// </summary>
        /// <param name="source">must not be <code>null</code></param>
        /// <param name="expectedTargetType"></param>
	    public Type GetCustomWriteTarget(Type source, Type expectedTargetType)
        {
		    AssertUtils.ArgumentNotNull(source, "source");

		    return GetCustomTarget(source, expectedTargetType);
	    }

        /// <summary>
        /// Returns the target type we can write an onject of the given source type to. The returned type might be a subclass
        /// oth the given expected type though. If {@code expectedTargetType} is {@literal null} we will simply return the
        /// first target type matching or {@literal null} if no conversion can be found.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        public Type GetCustomWriteTarget<TSource, TTarget>()
        {
            return GetCustomWriteTarget(typeof(TSource), typeof(TTarget));
        }

        /// <summary>
	    /// Returns whether we have a custom conversion registered to write into a Mongo native type. The returned type might
	    /// be a subclass oth the given expected type though.
        /// </summary>
        /// <param name="source">must not be <code>null</code></param>
	    public bool HasCustomWriteTarget(Type source)
        {
		    return HasCustomWriteTarget(source, null);
	    }

        /// <summary>
        /// Returns whether we have a custom conversion registered to write an object of the given source type into an object
        /// of the given Mongo native target type.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        public bool HasCustomWriteTarget<TSource>()
        {
            return HasCustomWriteTarget(typeof(TSource), null);
        }

        /// <summary>
	    /// Returns whether we have a custom conversion registered to write an object of the given source type into an object
	    /// of the given Mongo native target type.
        /// </summary>
        /// <param name="source">must not be <code>null</code></param>
        /// <param name="expectedTargetType"></param>
	    public bool HasCustomWriteTarget(Type source, Type expectedTargetType)
        {
		    return GetCustomWriteTarget(source, expectedTargetType) != null;
	    }

        /// <summary>
        /// Returns whether we have a custom conversion registered to write an object of the given source type into an object
        /// of the given Mongo native target type.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        public bool HasCustomWriteTarget<TSource, TTarget>()
        {
            return GetCustomWriteTarget(typeof(TSource), typeof(TTarget)) != null;
        }

        /// <summary>
        /// Returns whether we have a custom conversion registered to read the given source into the given target type.
        /// </summary>
        /// <param name="source">must not be <code>null</code></param>
        /// <param name="expectedTargetType">must not be <code>null</code></param>
	    public bool HasCustomReadTarget(Type source, Type expectedTargetType)
        {
		    AssertUtils.ArgumentNotNull(source, "source");
		    AssertUtils.ArgumentNotNull(expectedTargetType, "expectedTargetType");

		    return GetCustomTarget(source, expectedTargetType) != null;
	    }

        /// <summary>
        /// Returns whether we have a custom conversion registered to read the given source into the given target type.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        public bool HasCustomReadTarget<TSource, TTarget>()
        {
            return GetCustomTarget(typeof(TSource), typeof(TTarget)) != null;
        }

        /// <summary>
        /// Inspects all registered converter in <see cref="TypeConverterRegistry"/> for ones that have a source compatible
        /// as source. Additinally checks assignability of the the taregt type if one is given.
        /// </summary>
        /// <param name="source">must not be <code>null</code></param>
        /// <param name="expectedTargetType"></param>
	    private static Type GetCustomTarget(Type source, Type expectedTargetType)
        {
		    AssertUtils.ArgumentNotNull(source, "source");

	        var converters = TypeConverterRegistry.GetConvertersFrom(source);
            if (expectedTargetType != null)
            {
                var converter = converters.FirstOrDefault(c => c.TargetType.IsAssignableFrom(expectedTargetType));
                return converter != null ? converter.TargetType : null;
            }

            return (converters != null && converters.Count > 0) ? converters.FirstOrDefault().TargetType : null;
        }
    }
}
