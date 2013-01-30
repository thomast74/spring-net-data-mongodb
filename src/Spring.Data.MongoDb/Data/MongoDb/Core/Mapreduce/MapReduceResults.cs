// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapReduceResults.cs" company="The original author or authors.">
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
    /// Collects the results of performing a MapReduce operations.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MapReduceResults<T>
    {
	    private readonly IList<T> _mappedResults;
	    private readonly BsonDocument _rawResults;
	    private readonly string _outputCollection;
	    private readonly MapReduceTiming _mapReduceTiming;
        private readonly MapReduceCounts _mapReduceCounts;

        /// <summary>
        /// Creates a new <see cref="MapReduceResults{T}"/> from the given mapped results and the raw one.
        /// </summary>
        /// <param name="mappedResults">must not be <code>null</code></param>
        /// <param name="rawResults">must no tbe <code>null</code></param>
	    public MapReduceResults(List<T> mappedResults, BsonDocument rawResults) 
        {
            AssertUtils.ArgumentNotNull(mappedResults, "mappedResults");
            AssertUtils.ArgumentNotNull(rawResults, "rawResults");

		    _mappedResults = mappedResults;
		    _rawResults = rawResults;
		    _mapReduceTiming = ParseTiming(rawResults);
		    _mapReduceCounts = ParseCounts(rawResults);
		    _outputCollection = ParseOutputCollection(rawResults);
	    }

	    public IEnumerator<T> GetEnumerator()
	    {
	        return _mappedResults.GetEnumerator();
	    }

	    public MapReduceTiming Timing
        {
	        get { return _mapReduceTiming; }
        }

	    public MapReduceCounts Counts
        {
	        get { return _mapReduceCounts; }
        }

	    public string OutputCollection
        {
	        get { return _outputCollection; }
        }

        public BsonDocument RawResults
        {
            get { return _rawResults; }
        }

        private MapReduceTiming ParseTiming(BsonDocument rawResults)
        {
            var timingValue = rawResults.GetValue("timing");
            var timing = timingValue != null && timingValue.IsBsonDocument ? timingValue.ToBsonDocument() : null;

            if (timing == null)
                return new MapReduceTiming(-1, -1, -1);

            if (timing.GetValue("mapTime") != null && timing.GetValue("emitLoop") != null &&
                timing.GetValue("total") != null)
            {
                return new MapReduceTiming(AsLong(timing, "mapTime"), AsLong(timing, "emitLoop"),
                                           AsLong(timing, "total"));
            }

            return new MapReduceTiming(-1, -1, -1);
        }

        /// <summary>
        /// Returns the value of the source's field with the given key as {@link Long}.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        private long AsLong(BsonDocument source, string key)
        {
            var raw = source.GetValue(key);
            if (raw == null)
                return 0;

            return raw.IsInt64 ? raw.ToInt64() : raw.ToInt32();
        }

        /// <summary>
        /// Parses the raw {@link DBObject} result into a {@link MapReduceCounts} value object.
        /// </summary>
        /// <param name="rawResults"></param>
        private MapReduceCounts ParseCounts(BsonDocument rawResults)
        {
            var countsValue = rawResults.GetValue("counts");
            var counts = countsValue != null && countsValue.IsBsonDocument ? countsValue.ToBsonDocument() : null;

            if (counts == null)
                return MapReduceCounts.None;

            if (counts.GetValue("input") != null && counts.GetValue("emit") != null && counts.GetValue("output") != null)
            {
                return new MapReduceCounts(AsLong(counts, "input"), AsLong(counts, "emit"), AsLong(counts, "output"));
            }

            return MapReduceCounts.None;
        }

        /// <summary>
        /// Parses the output collection from the raw <see cref="BsonDocument"/> result.
        /// </summary>
        /// <param name="rawResults"></param>
        private string ParseOutputCollection(BsonDocument rawResults)
        {
            var resultField = rawResults.GetValue("result");

            if (resultField == null)
                return null;

            return resultField.IsBsonDocument
                       ? resultField.ToBsonDocument().GetValue("collection").ToString()
                       : resultField.ToString();
        }
    }
}
