// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Query.cs" company="The original author or authors.">
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
using MongoDB.Driver;
using Spring.Data.Domain;
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Query
{
    /// <summary>
    /// Base query class
    /// </summary>
    /// <author>Thomas Risberg</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class Query
    {
        private IDictionary<string, Criteria> _criterias = new Dictionary<string, Criteria>();
        private Field _fieldSpec;
        private Sort _coreSort;
        private int _skip;
        private int _limit;
        private string _hint;

        public Field Field
        {
            get { return _fieldSpec ?? (_fieldSpec = new Field()); }
        }

        public int Skip
        {
            get { return _skip; }
        }

        public int Limit
        {
            get { return _limit; }
        }

        public string Hint
        {
            get { return _hint; }
        }
        
        /// <summary>
        /// Static factory method to create a Query using the provided criteria
        /// </summary>
        /// <param name="critera"></param>
        /// <returns>new query instance</returns>
        public static Query CreateQuery(Criteria critera)
        {
            return new Query(critera);
        }

        public Query()
        {
        }

        public Query(Criteria criteria)
        {
            AddCriteria(criteria);
        }

        public Query AddCriteria(Criteria criteria)
        {
            if (!_criterias.ContainsKey(criteria.Key))
            {
                _criterias.Add(criteria.Key, criteria);
            }
            else
            {
                throw new InvalidMongoDbApiUsageException("Due to limitations of the BsonDcoument, "
                                                          + "you can't add a second '" + criteria.Key + "' criteria. " +
                                                          "Query already contains '"
                                                          + _criterias[criteria.Key].CriteriaDocument + "'.");
            }
            return this;
        }

        public Query AddSkip(int skip)
        {
            this._skip = skip;
            return this;
        }

        /// <summary>
        /// Add a limit to the query object
        /// </summary>
        /// <param name="limit">number of records to return</param>
        /// <returns>this query object</returns>
        public Query AddLimit(int limit)
        {
            this._limit = limit;
            return this;
        }

        /// <summary>
        /// Configures the query to use the given hint when being executed.
        /// </summary>
        /// <param name="name">must not be <code>null</code> or empty</param>
        /// <returns></returns>
        public Query WithHint(string name)
        {
            AssertUtils.ArgumentHasText(name, "name");
            _hint = name;
            return this;
        }

        /// <summary>
        /// Sets the given pagination information on the <see cref="Query"/> instance. Will transparently set
        /// <see cref="_skip"/> and <see cref="_limit"/> as well as applying the <see cref="IPageable.Sort"/>
        /// <see cref="Sort"/> instance defined with the <see cref="IPageable"/>.
        /// </summary>
        /// <param name="pageable"></param>
        /// <returns></returns>
        public Query With(IPageable pageable)
        {
            if (pageable == null)
            {
                return this;
            }

            _limit = pageable.PageSize;
            _skip = pageable.Offset;

            return With(pageable.Sort);
        }

        /// <summary>
        /// Adds a <see cref="Sort"/> to the <see cref="Query"/> instance.
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        public Query With(Sort sort)
        {
            if (sort == null)
            {
                return this;
            }

            if (_coreSort == null)
            {
                _coreSort = sort;
            }
            else
            {
                _coreSort = _coreSort.And(sort);
            }

            return this;
        }

        public QueryDocument QueryDocument
        {
            get
            {
                var dbo = new QueryDocument ();
                foreach (var k in _criterias.Keys)
                {
                    dbo.Add(_criterias[k].CriteriaDocument);
                }
                return dbo;
            }
        }

        public FieldsDocument FieldsDocument
        {
            get
            {
                if (_fieldSpec == null)
                {
                    return null;
                }
                return _fieldSpec.FieldsDocument;
            }
        }

        public SortByDocument SortByDocument
        {
            get
            {
                if (_coreSort == null)
                {
                    return null;
                }

                var dbo = new SortByDocument();
                if (_coreSort != null)
                {
                    foreach(var order in _coreSort)
                    {
                        dbo.Add(order.Property, order.IsAscending ? 1 : -1);
                    }
                }
                return dbo;
            }
        }

        protected List<Criteria> Criteria
        {
            get { return new List<Criteria>(_criterias.Values); }
        }

        public override string  ToString()
        {
            return string.Format("Query: {0}, Fields: {1}, Sort: {2}",
                                 SerializationUtils.SerializeToJsonSafely(QueryDocument),
                                 SerializationUtils.SerializeToJsonSafely(FieldsDocument),
                                 SerializationUtils.SerializeToJsonSafely(SortByDocument));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;

	        var that = obj as Query;
	        if (that == null) return false;

            var criteriaEqual = _criterias.Equals(that.Criteria);
		    var fieldsEqual = _fieldSpec == null ? that._fieldSpec == null : _fieldSpec.Equals(that._fieldSpec);
		    var hintEqual = _hint == null ? that._hint == null : _hint.Equals(that._hint);
		    var skipEqual = _skip == that._skip;
		    var limitEqual = _limit == that._limit;

		    return criteriaEqual && fieldsEqual && hintEqual && skipEqual && limitEqual;
        }

        public override int  GetHashCode()
        {
		    int result = 17;

		    result += 31 * _criterias.GetHashCode();
		    result += 31 * ObjectUtils.NullSafeHashCode(_fieldSpec);
		    result += 31 * ObjectUtils.NullSafeHashCode(_hint);
		    result += 31 * _skip;
		    result += 31 * _limit;

		    return result;
        }
    }
}
