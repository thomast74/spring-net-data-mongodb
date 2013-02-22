﻿#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConventionProfileFactory.cs" company="The original author or authors.">
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
using MongoDB.Bson.Serialization.Conventions;
using Spring.Objects.Factory;
using MongoDB.Bson.Serialization;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Factory to create <see cref="ConventionProfile"/>
    /// </summary>
    /// <author>Thomas Trageser</author>
    public class ConventionProfileFactory : IFactoryObject, IInitializingObject
    {
        private ConventionProfile _profile;

        public const string DefaultValueConventionProperty = "DefaultValueConvention";
        public IDefaultValueConvention DefaultValueConvention { get; set; }

        public const string ElementNameConventionProperty = "ElementNameConvention";
        public IElementNameConvention ElementNameConvention { get; set; }

        public const string ExtraElementsMemberConventionProperty = "ExtraElementsMemberConvention";
        public IExtraElementsMemberConvention ExtraElementsMemberConvention { get; set; }

        public const string IdGeneratorConventionProperty = "IdGeneratorConvention";
        public IIdGeneratorConvention IdGeneratorConvention { get; set; }

        public const string IdMemberConventionProperty = "IdMemberConvention";
        public IIdMemberConvention IdMemberConvention { get; set; }

        public const string IgnoreExtraElementsConventionProperty = "IgnoreExtraElementsConvention";
        public IIgnoreExtraElementsConvention IgnoreExtraElementsConvention { get; set; }

        public const string IgnoreIfDefaultConventionProperty = "IgnoreIfDefaultConvention";
        public IIgnoreIfDefaultConvention IgnoreIfDefaultConvention { get; set; }

        public const string IgnoreIfNullConventionProperty = "IgnoreIfNullConvention";
        public IIgnoreIfNullConvention IgnoreIfNullConvention { get; set; }

        public const string MemberFinderConventionProperty = "MemberFinderConvention";
        public IMemberFinderConvention MemberFinderConvention { get; set; }

        public const string SerializationOptionsConventionProperty = "SerializationOptionsConvention";
        public ISerializationOptionsConvention SerializationOptionsConvention { get; set; }



        public object GetObject()
        {
            return _profile;
        }

        public bool IsSingleton
        {
            get { return true; }
        }

        public Type ObjectType
        {
            get { return typeof (ConventionProfile); }
        }

        public void AfterPropertiesSet()
        {
            var defaultProfile = ConventionProfile.GetDefault();
            _profile = new ConventionProfile();

            _profile.SetDefaultValueConvention(DefaultValueConvention ?? defaultProfile.DefaultValueConvention);
            _profile.SetElementNameConvention(ElementNameConvention ?? defaultProfile.ElementNameConvention);
            _profile.SetExtraElementsMemberConvention(ExtraElementsMemberConvention ?? defaultProfile.ExtraElementsMemberConvention);
            _profile.SetIdGeneratorConvention(IdGeneratorConvention ?? defaultProfile.IdGeneratorConvention);
            _profile.SetIdMemberConvention(IdMemberConvention ?? defaultProfile.IdMemberConvention);
            _profile.SetIgnoreExtraElementsConvention(IgnoreExtraElementsConvention ?? defaultProfile.IgnoreExtraElementsConvention);
            _profile.SetIgnoreIfDefaultConvention(IgnoreIfDefaultConvention ?? defaultProfile.IgnoreIfDefaultConvention);
            _profile.SetIgnoreIfNullConvention(IgnoreIfNullConvention ?? defaultProfile.IgnoreIfNullConvention);
            _profile.SetMemberFinderConvention(MemberFinderConvention ?? defaultProfile.MemberFinderConvention);
            _profile.SetSerializationOptionsConvention(SerializationOptionsConvention ?? defaultProfile.SerializationOptionsConvention);

            BsonClassMap.RegisterConventions(_profile, (t) => { return true; });
        }
    }
}
