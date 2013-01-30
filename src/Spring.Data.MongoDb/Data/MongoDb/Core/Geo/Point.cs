// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Point.cs" company="The original author or authors.">
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
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Geo
{
    /// <summary>
    /// Represents a geospatial point value.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <autor>Oliver Gierke</autor>
    /// <author>Thomas Trageser</author>
    public class Point
    {
	    ///TODO: @Field(order = 10)
	    private readonly double _x;
	    ///TODO: @Field(order = 20)
	    private readonly double _y;

	    ///TODO: @PersistenceConstructor
	    public Point(double x, double y)
        {
		    _x = x;
		    _y = y;
	    }

	    public Point(Point point)
        {
		    AssertUtils.ArgumentNotNull(point, "point");

		    _x = point._x;
		    _y = point._y;
	    }

        public double X
        {
            get { return _x; }
        }

        public double Y
        {
            get { return _y; }
        }

        public double[] ToArray()
        {
		    return new [] { _x, _y };
	    }

	    public IList<double> ToList()
        {
		    return new List<double> { _x, _y };
	    }

	    public override bool Equals(object obj)
        {
		    if (this == obj)
			    return true;

            if (obj == null || GetType() != obj.GetType())
			    return false;

            var other = (Point) obj;
		    if (BitConverter.DoubleToInt64Bits(_x) != BitConverter.DoubleToInt64Bits(other._x))
            {
			    return false;
		    }
		    if (BitConverter.DoubleToInt64Bits(_y) != BitConverter.DoubleToInt64Bits(other._y))
            {
			    return false;
		    }
		    return true;
	    }

	    public override int GetHashCode()
        {
		    const int prime = 31;
		    var result = 1;
	        var temp = BitConverter.DoubleToInt64Bits(_x);
		    result = prime * result + (int) (temp ^ (temp >> 32));
		    temp = BitConverter.DoubleToInt64Bits(_y);
		    result = prime * result + (int) (temp ^ (temp >> 32));
		    return result;
	    }

	    public override string ToString()
        {
		    return string.Format("Point [latitude={0}, longitude={1}]", _x, _y);
	    }
    }
}
