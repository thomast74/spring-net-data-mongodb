#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractMongoConverter.cs" company="The original author or authors.">
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

using MongoDB.Bson;
using MongoDB.Driver;
using Spring.Core.TypeConversion;
using Spring.Data.Convert;
using Spring.Data.Mapping.Context;
using Spring.Data.MongoDb.Core.Convert.Converters;
using Spring.Data.MongoDb.Core.Mapping;
using Spring.Objects.Factory;

namespace Spring.Data.MongoDb.Core.Convert
{
    /// <summary>
    /// Base class for <see cref="IMongoConverter"/> implementations and populates basic converters. 
    /// Allows registering {@link CustomConversions}.
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public abstract class AbstractMongoConverter : IMongoConverter, IInitializingObject
    {
	    protected CustomConversions _conversions;
	    private EntityInstantiators _instantiators;

        /// <summary>
        /// Creates a new <see cref="AbstractMongoConverter"/>.
        /// </summary>
	    public AbstractMongoConverter()
        {
            _conversions = new CustomConversions();
            _instantiators = new EntityInstantiators();
        }

        /// <summary>
        /// Registers the given custom conversions with the converter.
        /// </summary>
	    public CustomConversions CustomConversions
        {
            set { _conversions = value; }
        }

        /// <summary>
        /// Registers <see cref="EntityInstantiators"/> to customize entity instantiation.
        /// If null a default <see cref="EntityInstantiators"/> is used. 
        /// </summary>
	    public EntityInstantiators Instantiators
        {
            set { _instantiators = value == null ? new EntityInstantiators() : value; }
        }

	    public void AfterPropertiesSet()
        {
		    InitializeConverters();
	    }

        /// <summary>
        /// Registers additional converters that will be available when using the <see cref="TypeConversionUtils"/> 
        /// directly (e.g. for id conversion). These converters are not custom conversions as they'd introduce unwanted 
        /// conversions (e.g. ObjectId-to-String).
        /// </summary>
	    private void InitializeConverters()
        {
			TypeConverterRegistry.RegisterConverter(new StringToObjectIdConverter());
            TypeConverterRegistry.RegisterConverter(new BigIntegerToObjectIdConverter());
            TypeConverterRegistry.RegisterConverter(new ObjectIdToStringConverter());
            TypeConverterRegistry.RegisterConverter(new ObjectIdToBigIntegerConverter());
	    }

        public abstract IMappingContext MappingContext { get; }

        public abstract TTargetType Read<TTargetType>(BsonDocument source);

        public abstract void Write(object source, BsonDocument sink);

        public abstract object ConvertToMongoType(object obj);

        public abstract MongoDBRef ToDBRef(object obj, IMongoPersistentProperty referingProperty);
    }
}
