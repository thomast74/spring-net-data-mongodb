// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NearQuery.cs" company="The original author or authors.">
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

using MongoDB.Bson;
using Spring.Data.MongoDb.Core.Geo;
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Query
{
    /// <summary>
    /// Builder class to build near-queries.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class NearQuery
    {
	    private readonly BsonDocument _criteria;
        private readonly IMetric _metric;
        
        private Query _query;
	    private double _maxDistance;

        /// <summary>
        /// Creates a new <see cref="NearQuery"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="metric"></param>
	    private NearQuery(Point point, IMetric metric) 
        {
            AssertUtils.ArgumentNotNull(point, "point");

		    _criteria = new BsonDocument {{"near", BsonValue.Create(point.ToList())}};

            _metric = metric;
		    if (metric != null) 
            {
			    Spherical(true);
			    DistanceMultiplier(metric);
		    }
	    }

        /// <summary>
        /// Creates a new <see cref="NearQuery"/> starting near the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
	    public static NearQuery Near(double x, double y)
        {
		    return Near(x, y, null);
	    }

        /// <summary>
        /// Creates a new <see cref="NearQuery"/> starting at the given coordinates using the given <see cref="IMetric"/>
        /// to adapt given values to further configuration. E.g. setting a <see cref="MaxDistance(double)"/> will
        /// be interpreted as a value of the initially set <see cref="IMetric"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="metric"></param>
	    public static NearQuery Near(double x, double y, IMetric metric)
        {
		    return Near(new Point(x, y), metric);
	    }

        /// <summary>
        /// Creates a new <see cref="NearQuery"/> starting at the given <see cref="Point"/>.
        /// </summary>
        /// <param name="point">must not be <code>null</code></param>
	    public static NearQuery Near(Point point)
        {
		    return Near(point, null);
	    }

        /// <summary>
	    /// Creates a <see cref="NearQuery"/> starting near the given <see cref="Point"/> using the given
	    /// <see cref="IMetric"/> to adapt given values to further configuration. E.g. setting a
	    /// <see cref="MaxDistance(double)"/> will be interpreted as a value of the initially set 
	    /// <see cref="IMetric"/>.
        /// </summary>
        /// <param name="point">must not be <code>null</code></param>
        /// <param name="metric"></param>
	    public static NearQuery Near(Point point, IMetric metric)
        {
            AssertUtils.ArgumentNotNull(point, "point");
		    return new NearQuery(point, metric);
	    }

        /// <summary>
        /// Returns the <see cref="IMetric"/> underlying the actual query.
        /// </summary>
	    public IMetric Metric
        {
	        get { return _metric; }
        }

        /// <summary>
        /// Configures the number of results to return.
        /// </summary>
        /// <param name="num"></param>
	    public NearQuery Num(int num)
        {
		    _criteria.Add("num", BsonValue.Create(num));
		    return this;
	    }

        /// <summary>
	    /// Sets the max distance results shall have from the configured origin. Will normalize the given value using a
	    /// potentially already configured <see cref="IMetric"/>.
        /// </summary>
        /// <param name="maxDistance"></param>
	    public NearQuery MaxDistance(double maxDistance)
        {
		    _maxDistance = GetNormalizedDistance(maxDistance, _metric);
		    return this;
	    }

        /// <summary>
	    /// Sets the maximum distance supplied in a given metric. Will normalize the distance but not
	    /// reconfigure the query's <see cref="IMetric"/>.
        /// </summary>
        /// <param name="maxDistance"></param>
        /// <param name="metric">must not be <code>null</code></param>
	    public NearQuery MaxDistance(double maxDistance, IMetric metric)
        {
            AssertUtils.ArgumentNotNull(metric, "metric");

		    Spherical(true);
		    return MaxDistance(GetNormalizedDistance(maxDistance, metric));
	    }

        /// <summary>
        /// Sets the maximum distance to the given <see cref="Distance"/>.
        /// </summary>
        /// <param name="distance">must not be <code>null</code></param>
	    public NearQuery MaxDistance(Distance distance)
        {
            AssertUtils.ArgumentNotNull(distance, "distance");

		    return MaxDistance(distance.Value, distance.Metric);
	    }

        /// <summary>
        /// Configures a distance multiplier the resulting distances get applied.
        /// </summary>
        /// <param name="distanceMultiplier"></param>
	    public NearQuery DistanceMultiplier(double distanceMultiplier) 
        {
		    _criteria.Add("distanceMultiplier", BsonValue.Create(distanceMultiplier));
		    return this;
	    }

        /// <summary>
        /// Configures the distance multiplier to the multiplier of the given <see cref="IMetric"/>. 
        /// Does <em>not</em> recalculate the <see cref="MaxDistance(double)"/>.
        /// </summary>
        /// <param name="metric">must not be <code>null</code></param>
	    public NearQuery DistanceMultiplier(IMetric metric)
        {
            AssertUtils.ArgumentNotNull(metric, "metric");
		    return DistanceMultiplier(metric.Multiplier);
	    }

        /// <summary>
        /// Configures whether to return spherical values for the actual distance.
        /// </summary>
        /// <param name="spherical"></param>
	    public NearQuery Spherical(bool spherical)
        {
		    _criteria.Add("spherical", BsonValue.Create(spherical));
		    return this;
	    }

        /// <summary>
        /// Will cause the results' distances being returned in kilometers. Sets <see cref="DistanceMultiplier(double)"/>
        /// and <see cref="Spherical"/> accordingly.
        /// </summary>
	    public NearQuery InKilometers() 
        {
		    return AdaptMetric(Metrics.Kilometers);
	    }

        /// <summary>
	    /// Will cause the results' distances being returned in miles. Sets <see cref="DistanceMultiplier(double)"/> and
	    /// <see cref="Spherical"/> accordingly.
        /// </summary>
	    public NearQuery InMiles()
        {
		    return AdaptMetric(Metrics.Miles);
	    }

        /// <summary>
        /// Configures the given <see cref="IMetric"/> to be used as base on for this query and recalculate 
        /// the maximum distance if no metric was set before.
        /// </summary>
        /// <param name="metric"></param>
	    private NearQuery AdaptMetric(IMetric metric)
        {
		    if (_metric == null && _maxDistance > 0)
            {
			    MaxDistance(_maxDistance, metric);
		    }

		    Spherical(true);
		    return DistanceMultiplier(metric);
	    }

        /// <summary>
        /// Adds an actual query to the <see cref="NearQuery"/> to restrict the objects considered for 
        /// the actual near operation.
        /// </summary>
        /// <param name="query"></param>
	    public NearQuery Query(Query query)
        {
		    _query = query;
		    return this;
	    }

        /// <summary>
        /// Returns the <see cref="BsonDocument"/> built by the <see cref="NearQuery"/>.
        /// </summary>
	    public BsonDocument ToDbObject()
        {
            var bsonDocument = new BsonDocument(_criteria.ToDictionary());

		    if (_query != null)
            {
			    bsonDocument.Add("query", BsonValue.Create(_query.QueryDocument));
		    }
		    if (_maxDistance > 0) 
            {
			    bsonDocument.Add("maxDistance", BsonValue.Create(_maxDistance));
		    }

		    return bsonDocument;
	    }

	    private double GetNormalizedDistance(double distance, IMetric metric)
        {
		    return metric == null ? distance : distance / metric.Multiplier;
	    }
    }
}
