// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupByResults.cs" company="The original author or authors.">
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

using System.Collections.Generic;
using MongoDB.Bson;
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Mapreduce
{
    /// <summary>
    /// 
    /// </summary>
    /// <author></author>
    /// <author></author>
    public class GroupByResults<T>
    {
	    private readonly IList<T> _mappedResults;
	    private readonly BsonDocument _rawResults;

	    private double _count;
	    private int _keys;
	    private string _serverUsed;

        /// <summary>
        /// Creates a <see cref="GroupByResults{T}"/>.
        /// </summary>
        /// <param name="mappedResults">must not be <code>null</code></param>
        /// <param name="rawResults">must not be <code>null</code></param>
        public GroupByResults(IList<T> mappedResults, BsonDocument rawResults)
        {
            AssertUtils.ArgumentNotNull(mappedResults, "mappedResults");
            AssertUtils.ArgumentNotNull(rawResults, "rawResults");

		    _mappedResults = mappedResults;
		    _rawResults = rawResults;
		    ParseKeys();
		    ParseCount();
		    ParseServerUsed();
	    }

	    public double Count
        {
	        get { return _count; }
        }

	    public int Keys
        {
	        get { return _keys; }
        }

	    public string ServerUsed
        {
	        get { return _serverUsed; }
        }

        public IEnumerator<T> GetEnumerator()
	    {
	        return _mappedResults.GetEnumerator();
	    }

	    public BsonValue RawResults
        {
	        get { return _rawResults; }
        }

	    private void ParseCount()
        {
            var value  = _rawResults.GetValue("count");
	        _count = value != null && value.IsDouble ? value.ToDouble() : 0;
	    }

	    private void ParseKeys()
	    {
	        var value = _rawResults.GetValue("keys");
	        _keys = value != null && value.IsInt32 ? value.ToInt32() : 0;
	    }

	    private void ParseServerUsed()
        {
		    // "serverUsed" : "127.0.0.1:27017"
            var value = _rawResults.GetValue("serverUsed");
	        _serverUsed = value != null && value.IsString ? value.ToString() : string.Empty;
	    }
    }
}
