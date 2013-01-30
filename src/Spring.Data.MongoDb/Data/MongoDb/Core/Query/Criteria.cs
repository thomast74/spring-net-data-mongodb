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
using System.Linq;
using System.Text.RegularExpressions;
using Spring.Data.MongoDb.Core.Geo;
using Spring.Util;
using MongoDB.Bson;

namespace Spring.Data.MongoDb.Core.Query
{
    /// <summary>
    /// Central class for creating queries. It follows a fluent API style so that you can easily chain together multiple
    /// criteria. Static import of the 'Criteria.where' method will improve readability.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class Criteria : ICriteria
    {
        /// <summary>
        /// Custom "not-null" object as we have to be able to work with <code>null</code> values as well.
        /// </summary>
	    private static readonly object NOT_SET = new object();

	    private string _key;

	    private readonly IList<Criteria> _criteriaChain;

	    private readonly IDictionary<string, object> _criteria = new SortedList<string, object>();

	    private object _isValue = NOT_SET;

	    public Criteria() 
        {
		    _criteriaChain = new List<Criteria>();
	    }

	    public Criteria(string key) 
        {
		    _criteriaChain = new List<Criteria> {this};
	        _key = key;
	    }

	    protected Criteria(IList<Criteria> criteriaChain, string key)
        {
		    _criteriaChain = criteriaChain;
		    _criteriaChain.Add(this);
		    _key = key;
	    }

        /// <summary>
        /// Static factory method to create a Criteria using the provided key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>new Criteria instance</returns>
	    public static Criteria Where(string key) 
        {
		    return new Criteria(key);
	    }

        /// <summary>
        /// Static factory method to create a Criteria using the provided key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>new criteria instance</returns>
        public Criteria And(string key)
        {
		    return new Criteria(_criteriaChain, key);
	    }

        /// <summary>
        /// Creates a criterion using equality
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
	    public Criteria Is(object o)
        {
		    if (_isValue != NOT_SET)
		    {
		        throw new InvalidMongoDbApiUsageException(
		            "Multiple 'is' values declared. You need to use 'and' with multiple criteria");
		    }
		    if (LastOperatorWasNot())
		    {
		        throw new InvalidMongoDbApiUsageException("Invalid query: 'not' can't be used with 'is' - use 'ne' instead.");
		    }
		    _isValue = o;
		    return this;
	    }

	    private bool LastOperatorWasNot()
        {
		    return _criteria.Count > 0 && "$not".Equals(_criteria.Keys.Last());
	    }

        /// <summary>
        /// Creates a criterion using the $ne operator
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Criteria Ne(object o)
        {
		    _criteria.Add("$ne", o);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $lt operator
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Criteria Lt(object o)
        {
		    _criteria.Add("$lt", o);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $lte operator
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Criteria Lte(object o)
        {
		    _criteria.Add("$lte", o);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $gt operator
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Criteria Gt(object o)
        {
		    _criteria.Add("$gt", o);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $gte operator
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Criteria Gte(object o) 
        {
		    _criteria.Add("$gte", o);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $in operator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
	    public Criteria In<T>(T[] o)
        {
	        if (o == null || o.Length == 0)
	            return this;

		    return In<T>(o.ToList());
	    }

        /// <summary>
        /// Creates a criterion using the $in operator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <returns></returns>
	    public Criteria In<T>(IEnumerable<T> c)
	    {
	        if (c == null)
	            return this;

		    _criteria.Add("$in", c);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $nin operator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public Criteria Nin<T>(T[] o)
	    {
	        if (o == null || o.Length == 0)
	            return this;

		    return Nin<T>(o.ToList());
	    }

        /// <summary>
        /// Creates a criterion using the $nin operator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <returns></returns>
	    public Criteria Nin<T>(IEnumerable<T> c)
	    {
	        if (c == null)
	            return this;

		    _criteria.Add("$nin", c);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $mod operator
        /// </summary>
        /// <param name="value"></param>
        /// <param name="remainder"></param>
        /// <returns></returns>
        public Criteria Mod(double value, double remainder)
        {
		    IList<object> l = new List<object>();
		    l.Add(value);
		    l.Add(remainder);
		    _criteria.Add("$mod", l);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $all operator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public Criteria All<T>(T[] o)
        {
		    return All<T>(o.ToList());
	    }

        /// <summary>
        /// Creates a criterion using the $all operator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <returns></returns>
	    public Criteria All<T>(IEnumerable<T> c)
        {
		    _criteria.Add("$all", c);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $size operator
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public Criteria Size(int s)
        {
		    _criteria.Add("$size", s);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $exists operator
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
	    public Criteria Exists(bool b)
        {
		    _criteria.Add("$exists", b);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $type operator
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Criteria Type(int t)
        {
		    _criteria.Add("$type", t);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $not meta operator which affects the clause directly following
        /// </summary>
        /// <returns></returns>
        public Criteria Not()
        {
		    return Not(null);
	    }

        /// <summary>
        /// Creates a criterion using the $not meta operator which affects the clause directly following
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
	    private Criteria Not(object value)
        {
		    _criteria.Add("$not", value);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using a $regex
        /// </summary>
        /// <param name="re"></param>
        /// <returns></returns>
        public Criteria Regex(string re)
        {
		    return Regex(re, null);
	    }

        /// <summary>
        /// Creates a criterion using a $regex and $options
        /// </summary>
        /// <param name="re"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Criteria Regex(string re, string options)
        {
		    return Regex(ToPattern(re, options));
	    }

        /// <summary>
        /// Syntactical sugar for {@link #is(Object)} making obvious that we create a regex predicate.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
	    public Criteria Regex(Regex pattern)
        {
		    AssertUtils.ArgumentNotNull(pattern, "pattern");

		    if (LastOperatorWasNot())
			    return Not(pattern);

		    _isValue = pattern;

		    return this;
	    }

	    private Regex ToPattern(string regex, string options) 
        {
		    AssertUtils.ArgumentNotNull(regex, "regex");

	        return string.IsNullOrEmpty(options) ? new BsonRegularExpression(regex, options).ToRegex() : new Regex(regex);
        }

        /// <summary>
        /// Creates a geospatial criterion using a $within $center operation.
        /// </summary>
        /// <param name="circle">must not be <code>null</code></param>
        /// <returns></returns>
	    public Criteria WithinSphere(Circle circle)
        {
		    AssertUtils.ArgumentNotNull(circle, "circle");

		    _criteria.Add("$within", new BsonDocument("$centerSphere", BsonValue.Create(circle.ToList())));
		    return this;
	    }

	    public Criteria Within(IShape shape)
        {

		    AssertUtils.ArgumentNotNull(shape, "shape");

		    _criteria.Add("$within", new BsonDocument(shape.Command, BsonValue.Create(shape.ToList())));
		    return this;
	    }

        /// <summary>
        /// Creates a geospatial criterion using a $near operation
        /// </summary>
        /// <param name="point"></param>
	    public Criteria Near(Point point)
        {
		    AssertUtils.ArgumentNotNull(point, "point");

		    _criteria.Add("$near", point.ToList());
		    return this;
	    }

        /// <summary>
        /// Creates a geospatial criterion using a $nearSphere operation.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
	    public Criteria NearSphere(Point point)
        {
		    AssertUtils.ArgumentNotNull(point, "point");

		    _criteria.Add("$nearSphere", point.ToList());
		    return this;
	    }

        /// <summary>
        /// Creates a geospatical criterion using a $maxDistance operation, for use with $near
        /// </summary>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public Criteria MaxDistance(double maxDistance)
        {
		    _criteria.Add("$maxDistance", maxDistance);
		    return this;
	    }

        /// <summary>
        /// Creates a criterion using the $elemMatch operator
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
	    public Criteria ElemMatch(Criteria c)
        {
		    _criteria.Add("$elemMatch", c.CriteriaDocument);
		    return this;
	    }

        /// <summary>
        /// Creates an 'or' criteria using the $or operator for all of the provided criteria
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
	    public Criteria OrOperator(Criteria[] criteria)
        {
		    BsonArray bsonArray = CreateCriteriaList(criteria);
		    _criteriaChain.Add(new Criteria("$or").Is(bsonArray));
		    return this;
	    }

        /// <summary>
        /// Creates a 'nor' criteria using the $nor operator for all of the provided criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
	    public Criteria NorOperator(Criteria[] criteria)
        {
            BsonArray bsonList = CreateCriteriaList(criteria);
		    _criteriaChain.Add(new Criteria("$nor").Is(bsonList));
		    return this;
	    }

        /// <summary>
        /// Creates an 'and' criteria using the $and operator for all of the provided criteria
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
	    public Criteria AndOperator(Criteria[] criteria)
        {
            BsonArray bsonList = CreateCriteriaList(criteria);
		    _criteriaChain.Add(new Criteria("$and").Is(bsonList));
		    return this;
	    }

        public string Key
        {
            get { return _key; }
        }


        public BsonDocument CriteriaDocument
        {
            get
            {
                if (_criteriaChain.Count == 1)
                {
                    return _criteriaChain[0].SingleCriteriaDocument;
                }

                var criteriaObject = new BsonDocument();
                foreach (var c in _criteriaChain)
                {
                    var dbo = c.SingleCriteriaDocument;
                    foreach (var k in dbo.Elements)
                    {
                        SetValue(criteriaObject, k.Name, dbo.Add(k));
                    }
                }
                return criteriaObject;
            }
        }

        protected BsonDocument SingleCriteriaDocument
        {
            get
            {
                var dbo = new BsonDocument();
                var not = false;
                foreach (var k in _criteria.Keys)
                {
                    var value = _criteria[k];
                    if (not)
                    {
                        var notDbo = new BsonDocument();
                        notDbo.Add(k, BsonValue.Create(value));
                        dbo.Add("$not", notDbo);
                        not = false;
                    }
                    else
                    {
                        if ("$not".Equals(k) && value == null)
                            not = true;
                        else
                            dbo.Add(k, BsonValue.Create(value));
                    }
                }
                var queryCriteria = new BsonDocument();
                if (_isValue != NOT_SET)
                {
                    queryCriteria.Add(_key, BsonValue.Create(_isValue));
                    queryCriteria.Add(dbo);
                }
                else
                {
                    queryCriteria.Add(_key, dbo);
                }
                return queryCriteria;
            }
        }

        private BsonArray CreateCriteriaList(Criteria[] criterias)
        {
            var bsonList = new BsonArray();
		    foreach(var c in criterias)
            {
			    bsonList.Add(c.CriteriaDocument);
		    }
		    return bsonList;
	    }

        private void SetValue(BsonDocument dbo, string key, object value)
        {
		    object existing = dbo.GetElement(key);
		    if (existing == null) 
            {
			    dbo.Add(key, BsonValue.Create(value));
		    }
            else 
            {
			    throw new InvalidMongoDbApiUsageException("Due to limitations of the com.mongodb.BasicDBObject, "
					    + "you can't add a second '" + key + "' expression specified as '" + key + " : " + value + "'. "
					    + "Criteria already contains '" + key + " : " + existing + "'.");
		    }
	    }

	    public override bool Equals(object obj)
        {
		    if (this == obj)
			    return true;

		    if (obj == null || GetType() != obj.GetType())
			    return false;

		    var that = (Criteria) obj;

		    if (_criteriaChain.Count != that._criteriaChain.Count)
			    return false;

		    for (var i = 0; i < _criteriaChain.Count; i++)
            {
			    var left = _criteriaChain[i];
			    var right = that._criteriaChain[i];

			    if (!SimpleCriteriaEquals(left, right))
				    return false;
		    }

		    return true;
	    }

	    private bool SimpleCriteriaEquals(Criteria left, Criteria right)
        {

		    var keyEqual = left.Key == null ? right.Key == null : left.Key.Equals(right.Key);
		    var criteriaEqual = left._criteria.Equals(right._criteria);
		    var valueEqual = IsEqual(left._isValue, right._isValue);

		    return keyEqual && criteriaEqual && valueEqual;
	    }

	    private bool IsEqual(object left, object right)
        {
		    if (left == null)
			    return right == null;

		    if (left is Regex)
			    return right is Regex && left.ToString().Equals(right.ToString());

		    return ObjectUtils.NullSafeEquals(left, right);
	    }

	    public override int GetHashCode()
        {
		    int result = 17;

            result += ObjectUtils.NullSafeHashCode(_key);
		    result += _criteria.GetHashCode();
            result += ObjectUtils.NullSafeHashCode(_isValue);

		    return result;
	    }

    }
}
