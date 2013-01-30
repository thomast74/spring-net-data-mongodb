// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Criteria.cs" company="The original author or authors.">
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

using System.Collections.Generic;
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Geo
{
    /// <summary>
    /// Represents a geospatial circle value
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class Circle : IShape
    {
	    private readonly Point _center;
	    private readonly double _radius;

        /// <summary>
        /// Creates a new <see cref="Circle"/> from the given <see cref="Point"/> and radius
        /// </summary>
        /// <param name="center">must not be <code>null</code></param>
        /// <param name="radius">must be greater or equal to zero</param>
	    ///TODO: @PersistenceConstructor
	    public Circle(Point center, double radius)
        {
		    AssertUtils.ArgumentNotNull(center, "center");
            AssertUtils.IsTrue(radius >= 0, "Radius must not be negative!");

		    _center = center;
		    _radius = radius;
	    }

        /// <summary>
        /// Creates a new {@link Circle} from the given coordinates and radius.
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius">must be greater or equal to zero</param>
	    public Circle(double centerX, double centerY, double radius) : this(new Point(centerX, centerY), radius)
        {
	    }

        /// <summary>
        /// Returns the center of the {@link Circle}
        /// </summary>
        public Point Center
        {
            get { return _center; }
        }

        /// <summary>
        /// Returns the radius of the {@link Circle}
        /// </summary>
        /// <returns></returns>
        public double Radius
        {
            get { return _radius; }
        }

        public IList<object> ToList()
        {
		    return new List<object> {Center.ToList(), Radius};
	    }

        public string Command
        {
            get { return "$center"; }
        }

        public override string ToString()
        {
		    return string.Format("Circle [center={0}, radius={1}]", _center, _radius);
	    }
        
        public override bool Equals(object obj)
        {
		    if (this == obj)
			    return true;

		    if (obj == null || GetType() != obj.GetType())
			    return false;

		    var that = (Circle)obj;

		    return _center.Equals(that._center) && _radius.Equals(that._radius);
	    }

	    public override int  GetHashCode()
        {
		    var result = 17;
	        result += 31*_center.GetHashCode();
	        result += 31*_radius.GetHashCode();
		    return result;
	    }
    }
}
