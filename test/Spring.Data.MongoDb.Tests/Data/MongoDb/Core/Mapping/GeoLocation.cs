#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeoLocation.cs" company="The original author or authors.">
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
using Spring.Data.Annotation;
using Spring.Data.MongoDb.Core.Index;
using Spring.Data.MongoDb.Core.Mapping.Annotation;

namespace Spring.Data.MongoDb.Core.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Jon Brisbin</author>
    /// <author>Thomas Trageser</author>
    [Document("geolocation")]
    public class GeoLocation
    {
	    private ObjectId _id;
	    private double[] _location;

	    public GeoLocation(double[] location)
        {
		    _location = location;
	    }

        [Id]
        public ObjectId Id
        {
            get { return _id; }
            set { _id = value; }
        }

	    [GeoSpatialIndexed("geolocation")]
        public double[] Location
        {
            get { return _location; }
            set { _location = value; }
        }
    }
}
