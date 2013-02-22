#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoConventionProfileParserTests.cs" company="The original author or authors.">
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
using System.Reflection;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Unit test for <see cref="ConventionProfileParser"/>
    /// </summary>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class MongoConventionProfileParserTests
    {
        [SetUp]
        public void SetUp()
        {
            NamespaceParserRegistry.RegisterParser(typeof(MongoNamespaceParser));
        }

        [Test]
        public void IfNoIdUseDefault()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                            <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
                                <mongo:convention-profile />
                            </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));

            Assert.That(factory.GetObjectDefinitionNames(), Contains.Item("MongoConventionProfile"));

            var conventionProfile = factory.GetObject("MongoConventionProfile") as ConventionProfile;

            Assert.That(conventionProfile, Is.Not.Null);
            Assert.That(conventionProfile.DefaultValueConvention, Is.TypeOf<NullDefaultValueConvention>());
            Assert.That(conventionProfile.ElementNameConvention, Is.TypeOf<MemberNameElementNameConvention>());
            Assert.That(conventionProfile.ExtraElementsMemberConvention, Is.TypeOf<NamedExtraElementsMemberConvention>());
            Assert.That(conventionProfile.IdGeneratorConvention, Is.TypeOf<LookupIdGeneratorConvention>());
            Assert.That(conventionProfile.IdMemberConvention, Is.TypeOf<NamedIdMemberConvention>());
            Assert.That(conventionProfile.IgnoreExtraElementsConvention, Is.TypeOf<NeverIgnoreExtraElementsConvention>());
            Assert.That(conventionProfile.IgnoreIfDefaultConvention, Is.TypeOf<NeverIgnoreIfDefaultConvention>());
            Assert.That(conventionProfile.IgnoreIfNullConvention, Is.TypeOf<NeverIgnoreIfNullConvention>());
            Assert.That(conventionProfile.MemberFinderConvention, Is.TypeOf<PublicMemberFinderConvention>());
            Assert.That(conventionProfile.SerializationOptionsConvention, Is.TypeOf<NullSerializationOptionsConvention>());
        }

        [Test]
        public void SetsConventionsCorrectly()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                            <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
                                <mongo:convention-profile
	                                default-value='Spring.Data.MongoDb.Config.DefaultValueConvention, Spring.Data.MongoDb.Tests'
	                                element-name='Spring.Data.MongoDb.Config.ElementNameConvention, Spring.Data.MongoDb.Tests'
	                                extra-elements-member='Spring.Data.MongoDb.Config.ExtraElementsMemberConvention, Spring.Data.MongoDb.Tests'
	                                id-generator='Spring.Data.MongoDb.Config.IdGeneratorConvention, Spring.Data.MongoDb.Tests'
	                                id-member='Spring.Data.MongoDb.Config.IdMemberConvention, Spring.Data.MongoDb.Tests'
	                                ignore-extra-elements='Spring.Data.MongoDb.Config.IgnoreExtraElementsConvention, Spring.Data.MongoDb.Tests'
	                                ignore-if-default='Spring.Data.MongoDb.Config.IgnoreIfDefaultConvention, Spring.Data.MongoDb.Tests'
	                                ignore-if-null='Spring.Data.MongoDb.Config.IgnoreIfNullConvention, Spring.Data.MongoDb.Tests'
	                                member-finder='Spring.Data.MongoDb.Config.MemberFinderConvention, Spring.Data.MongoDb.Tests'
	                                serialization-options='Spring.Data.MongoDb.Config.SerializationOptionsConvention, Spring.Data.MongoDb.Tests' />
                            </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));
            var conventionProfile = factory.GetObject<ConventionProfile>("MongoConventionProfile");

            Assert.That(conventionProfile, Is.Not.Null);
            Assert.That(conventionProfile.DefaultValueConvention, Is.TypeOf<DefaultValueConvention>());
            Assert.That(conventionProfile.ElementNameConvention, Is.TypeOf<ElementNameConvention>());
            Assert.That(conventionProfile.ExtraElementsMemberConvention, Is.TypeOf<ExtraElementsMemberConvention>());
            Assert.That(conventionProfile.IdGeneratorConvention, Is.TypeOf<IdGeneratorConvention>());
            Assert.That(conventionProfile.IdMemberConvention, Is.TypeOf<IdMemberConvention>());
            Assert.That(conventionProfile.IgnoreExtraElementsConvention, Is.TypeOf<IgnoreExtraElementsConvention>());
            Assert.That(conventionProfile.IgnoreIfDefaultConvention, Is.TypeOf<IgnoreIfDefaultConvention>());
            Assert.That(conventionProfile.IgnoreIfNullConvention, Is.TypeOf<IgnoreIfNullConvention>());
            Assert.That(conventionProfile.MemberFinderConvention, Is.TypeOf<MemberFinderConvention>());
            Assert.That(conventionProfile.SerializationOptionsConvention, Is.TypeOf<SerializationOptionsConvention>());
        }

        [Test]
        public void FailIfConventionTypeIsWrong()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                            <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
                                <mongo:convention-profile
	                                default-value='NotValidType' />
                            </objects>";
            Assert.That(delegate
                {
                    var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));
                },
                        Throws.TypeOf<ObjectDefinitionStoreException>());
        }

        [Test]
        public void SetIncludeFilterCorrectly()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                            <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
                                <mongo:convention-profile id='Profile1'
	                                default-value='Spring.Data.MongoDb.Config.DefaultValueConvention, Spring.Data.MongoDb.Tests'>
                                    <mongo:include-filter type='regex' expression='.*Include' />
                                </mongo:convention-profile>
                            </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));
            var profile1 = factory.GetObject<ConventionProfile>("Profile1");

            var lookupProfile = BsonClassMap.LookupConventions(typeof (MyEntityInclude));
            
            Assert.That(lookupProfile, Is.Not.Null);
            Assert.That(lookupProfile, Is.EqualTo(profile1));

            lookupProfile = BsonClassMap.LookupConventions(typeof(MyEntityExclude));
            Assert.That(lookupProfile, Is.Not.SameAs(profile1));
            Assert.That(lookupProfile.DefaultValueConvention, Is.Not.TypeOf<DefaultValueConvention>());
        }

        [Test]
        public void SetExcludeFiltersCorrectly()
        {
            string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
                            <objects xmlns='http://www.springframework.net' xmlns:mongo='http://www.springframework.net/mongo'>  
                                <mongo:convention-profile id='Profile1'
	                                default-value='Spring.Data.MongoDb.Config.DefaultValueConvention, Spring.Data.MongoDb.Tests'>
                                    <mongo:exclude-filter type='regex' expression='.*Exclude' />
                                </mongo:convention-profile>
                            </objects>";
            var factory = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));
            var profile1 = factory.GetObject<ConventionProfile>("Profile1");

            var lookupProfile = BsonClassMap.LookupConventions(typeof(MyEntityInclude));

            Assert.That(lookupProfile, Is.Not.Null);
            Assert.That(lookupProfile, Is.EqualTo(profile1));

            lookupProfile = BsonClassMap.LookupConventions(typeof(MyEntityExclude));
            Assert.That(lookupProfile, Is.Not.SameAs(profile1));
            Assert.That(lookupProfile.DefaultValueConvention, Is.Not.TypeOf<DefaultValueConvention>());
        }
    }

    public class MyEntityInclude
    {
        
    }

    public class MyEntityExclude
    {

    }

    public class DefaultValueConvention : IDefaultValueConvention
    {
        public object GetDefaultValue(MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class ElementNameConvention : IElementNameConvention
    {
        public string GetElementName(MemberInfo member)
        {
            throw new NotImplementedException();
        }
    }

    public class ExtraElementsMemberConvention : IExtraElementsMemberConvention
    {
        public string FindExtraElementsMember(Type type)
        {
            throw new NotImplementedException();
        }
    }

    public class IdGeneratorConvention : IIdGeneratorConvention
    {
        public IIdGenerator GetIdGenerator(MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class IdMemberConvention : IIdMemberConvention
    {
        public string FindIdMember(Type type)
        {
            throw new NotImplementedException();
        }
    }

    public class IgnoreExtraElementsConvention : IIgnoreExtraElementsConvention
    {
        public bool IgnoreExtraElements(Type type)
        {
            throw new NotImplementedException();
        }
    }

    public class IgnoreIfDefaultConvention : IIgnoreIfDefaultConvention
    {
        public bool IgnoreIfDefault(MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class IgnoreIfNullConvention : IIgnoreIfNullConvention
    {
        public bool IgnoreIfNull(MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class MemberFinderConvention : IMemberFinderConvention
    {
        public IEnumerable<MemberInfo> FindMembers(Type type)
        {
            throw new NotImplementedException();
        }
    }

    public class SerializationOptionsConvention : ISerializationOptionsConvention
    {
        public IBsonSerializationOptions GetSerializationOptions(MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }

}
