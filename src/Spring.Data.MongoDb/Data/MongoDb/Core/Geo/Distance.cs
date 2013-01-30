// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Distance.cs" company="The original author or authors.">
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

using System.Text;
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Geo
{
    /// <summary>
    /// Value object to represent distances in a given metric.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class Distance
    {
	    private readonly double _value;
	    private readonly IMetric _metric;

        /// <summary>
        /// Creates a new <see cref="Distance"/>.
        /// </summary>
        /// <param name="value">the distance</param>
	    public Distance(double value) : this(value, Metrics.Neutral)
        {
	    }

        /// <summary>
        /// Creates a new <see cref="Distance"/> with the given <see cref="IMetric"/>.
        /// </summary>
        /// <param name="value">the distance</param>
        /// <param name="metric"></param>
	    public Distance(double value, IMetric metric)
        {
		    _value = value;
		    _metric = metric ?? Metrics.Neutral;
	    }

        /// <summary>
        /// Returns the distance assigned with this <see cref="Distance"/>.
        /// </summary>
	    public double Value
        {
	        get { return _value; }
        }

        /// <summary>
        /// Returns the normalized value regarding the underlying <see cref="IMetric"/>.
        /// </summary>
	    public double NormalizedValue
        {
	        get { return _value/_metric.Multiplier; }
        }

        /// <summary>
        /// Returns the metric assigned to this <see cref="Distance"/>
        /// </summary>
	    public IMetric Metric
        {
            get { return _metric; }
        }

        /// <summary>
        /// Adds the given distance to the current one. The resulting <see cref="Distance"/> will be in
        /// the same metric as the current one.
        /// </summary>
        /// <param name="other"></param>
	    public Distance Add(Distance other) 
        {
		    double newNormalizedValue = NormalizedValue + other.NormalizedValue;
		    return new Distance(newNormalizedValue * _metric.Multiplier, _metric);
	    }

        /// <summary>
        /// Adds the given <see cref="Distance"/> to the current one and forces the result to be in 
        /// a given <see cref="IMetric"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="metric"></param>
	    public Distance Add(Distance other, IMetric metric)
        {
		    double newLeft = NormalizedValue * metric.Multiplier;
		    double newRight = other.NormalizedValue * metric.Multiplier;
		    return new Distance(newLeft + newRight, metric);
	    }

	    public override bool Equals(object obj)
        {

		    if (this == obj) 
			    return true;

		    if (obj == null || GetType() == obj.GetType())
			    return false;

		    var that = (Distance)obj;

		    return _value.Equals(that._value) && ObjectUtils.NullSafeEquals(_metric, that._metric);
	    }

	    public override int GetHashCode()
        {
		    int result = 17;
		    result += 31 * _value.GetHashCode();
		    result += 31 * ObjectUtils.NullSafeHashCode(_metric);
		    return result;
	    }

	    public override string ToString()
        {
		    var builder = new StringBuilder();
		    builder.Append(_value);

		    if (_metric != Metrics.Neutral)
            {
			    builder.Append(" ").Append(_metric);
		    }

		    return builder.ToString();
	    }

    }
}
