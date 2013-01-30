// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Metrics.cs" company="The original author or authors.">
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

using Spring.Data.MongoDb.Core.Query;

namespace Spring.Data.MongoDb.Core.Geo
{
    /// <summary>
    /// Commonly used <see cref="Metrics"/> for <see cref="NearQuery"/>s.
    /// </summary>
    /// <author></author>
    /// <author></author>
    public struct Metrics : IMetric
    {
        public static readonly IMetric Kilometers = new Metrics(6378.137);
        public static readonly IMetric Miles = new Metrics(3963.191);
        public static readonly IMetric Neutral = new Metrics(1);

	    private readonly double _multiplier;

	    private Metrics(double multiplier)
        {
		    _multiplier = multiplier;
	    }

        /// <summary>
        /// Returns the multiplier to calculate metrics values from a base scale.
        /// </summary>
        /// <returns></returns>
        public double Multiplier
        {
	        get { return _multiplier; }
        }
    }
}
