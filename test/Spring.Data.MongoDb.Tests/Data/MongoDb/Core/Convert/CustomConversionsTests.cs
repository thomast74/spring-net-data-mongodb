    #region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomConversionsTests.cs" company="The original author or authors.">
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
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using Spring.Core.TypeConversion;
using Spring.Data.MongoDb.Core.Convert.Converters;

namespace Spring.Data.MongoDb.Core.Convert
{
    /// <summary>
    /// Unit tests for <see cref="CustomConversions"/>.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [NUnit.Framework.Category(TestCategory.Unit)]
    public class CustomConversionsTests
    {
	    [Test]
	    public void FindsBasicReadAndWriteConversions()
	    {
	        CustomConversions conversions = new CustomConversions(new List<TypeConverter>
	            {
	                new DoubleToStringConverter(),
	                new StringToDoubleConverter()
	            });

		    Assert.That(conversions.GetCustomWriteTarget(typeof(double), null), Is.TypeOf<string>());
		    Assert.That(conversions.GetCustomWriteTarget(typeof(string), null), Is.Null);

		    Assert.That(conversions.HasCustomReadTarget<string, double>(), Is.True);
		    Assert.That(conversions.HasCustomReadTarget<string, int>(), Is.False);

            Assert.That(conversions.HasCustomReadTarget(typeof(string), typeof(double)), Is.True);
		    Assert.That(conversions.HasCustomReadTarget(typeof(string), typeof(int)), Is.False);
	    }

	    [Test]
	    public void ConsidersSubtypesCorrectly()
        {
		    CustomConversions conversions = new CustomConversions(new List<TypeConverter> {
                new LongToStringConverter(),
				new StringToLongConverter()
            });

		    Assert.That(conversions.GetCustomWriteTarget<long>(), Is.TypeOf<string>());
		    Assert.That(conversions.HasCustomReadTarget<string, long>(), Is.True);
	    }

	    [Test]
	    public void ConsidersTypesWeRegisteredConvertersForAsSimple()
	    {
	        CustomConversions conversions = new CustomConversions(new List<TypeConverter> {new DoubleToStringConverter()});

		    Assert.That(conversions.IsSimpleType<Guid>(), Is.True);
		    Assert.That(conversions.IsSimpleType(typeof(Guid)), Is.True);
	    }

	    [Test]
	    public void ConsidersObjectIdToBeSimpleType()
        {
		    CustomConversions conversions = new CustomConversions();

		    Assert.That(conversions.IsSimpleType<ObjectId>(), Is.True);
		    Assert.That(conversions.IsSimpleType(typeof(ObjectId)), Is.True);

            Assert.That(conversions.HasCustomWriteTarget<ObjectId>(), Is.False);
            Assert.That(conversions.HasCustomWriteTarget(typeof(ObjectId)), Is.False);
	    }

	    [Test]
	    public void ConsidersCustomConverterForSimpleType()
	    {
	        CustomConversions conversions = new CustomConversions(new List<TypeConverter>
	            {
	                new ObjectIdToStringConverter()
	            });

		    Assert.That(conversions.IsSimpleType<ObjectId>(), Is.True);
		    Assert.That(conversions.HasCustomWriteTarget<ObjectId>(), Is.True);
		    Assert.That(conversions.HasCustomReadTarget<ObjectId, string>(), Is.True);
		    Assert.That(conversions.HasCustomReadTarget<ObjectId, object>(), Is.False);
	    }

	    [Test]
	    public void ConsidersMongoDBRefToBeSimpleTypes()
        {
		    CustomConversions conversions = new CustomConversions();

		    Assert.That(conversions.IsSimpleType<MongoDBRef>(), Is.True);
	    }

	    [Test]
	    public void PopulatesConversionServiceCorrectly()
	    {
	        CustomConversions conversions = new CustomConversions(new List<TypeConverter>
	            {
	                new StringToDoubleConverter()
	            });

		    Assert.That(TypeConverterRegistry.CanConvert<string, double>(), Is.True);
	    }

	    [Test]
	    public void DoesNotConsiderTypeSimpleIfOnlyReadConverterIsRegistered()
	    {
	        CustomConversions conversions = new CustomConversions(new List<TypeConverter>
	            {
	                new StringToDoubleConverter()
	            });

		    Assert.That(conversions.IsSimpleType<double>(), Is.False);
	    }

	    [Test]
	    public void DiscoversConvertersForSubtypesOfMongoTypes()
	    {
	        CustomConversions conversions = new CustomConversions(new List<TypeConverter>
	            {
	                new StringToIntegerConverter()
	            });

		    Assert.That(conversions.HasCustomReadTarget<string, int>(), Is.True);
		    Assert.That(conversions.HasCustomWriteTarget<string, int>(), Is.True);
	    }

	    [Test]
	    public void DoesNotHaveConverterForStringToBigIntegerByDefault()
        {
		    CustomConversions conversions = new CustomConversions();

		    Assert.That(conversions.HasCustomWriteTarget<string>(), Is.False);
		    Assert.That(conversions.GetCustomWriteTarget<string>(), Is.Null);

		    conversions = new CustomConversions(new List<TypeConverter>
                {
                    new StringToBigIntegerConverter()
                });

		    Assert.That(conversions.HasCustomWriteTarget<string>(), Is.False);
		    Assert.That(conversions.GetCustomWriteTarget<string>(), Is.Null);
	    }

	    [Test]
	    public void ConsidersBinaryASimpleType()
        {
		    CustomConversions conversions = new CustomConversions();

            Assert.That(conversions.IsSimpleType<BsonBinaryData>(), Is.True);
	    }

	    [Test]
	    public void HasWriteConverterForURL()
        {
		    CustomConversions conversions = new CustomConversions();

		    Assert.That(conversions.HasCustomWriteTarget<Uri>(), Is.True);
	    }

	    [Test]
	    public void ReadTargetForURL()
        {
		    CustomConversions conversions = new CustomConversions();

		    Assert.That(conversions.HasCustomReadTarget<string, Uri>(), Is.True);
	    }


        public class DoubleToStringConverter : AbstractTypeConverter<double, string>
        {
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is double)
                    value.ToString();

                return null;
            }
	    }

        public class StringToDoubleConverter : AbstractTypeConverter<string, double>
        {
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                string stringValue = value as string;

                if (stringValue != null)
                {
                    double result;
                    if (double.TryParse(stringValue, out result))
                        return result;
                }

                return null;
            }
	    }

        public class LongToStringConverter : AbstractTypeConverter<long, string>
        {
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is long)
                    value.ToString();

                return null;
            }
        }

        public class StringToLongConverter : AbstractTypeConverter<string, long>
        {
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                string stringValue = value as string;

                if (stringValue != null)
                {
                    long result;
                    if (long.TryParse(stringValue, out result))
                        return result;
                }

                return null;
            }
        }

        public class StringToIntegerConverter : AbstractTypeConverter<string, int>
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string stringValue = value as string;

                if (stringValue != null)
                {
                    int result;
                    if (int.TryParse(stringValue, out result))
                        return result;
                }

                return null;
            }
	    }

    }
}
