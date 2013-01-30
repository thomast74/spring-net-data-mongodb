// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapReduceCounts.cs" company="The original author or authors.">
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

namespace Spring.Data.MongoDb.Core.Mapreduce
{
    /// <summary>
    /// Value object to encapsulate results of a map-reduce count.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MapReduceCounts
    {
	    public readonly static MapReduceCounts None = new MapReduceCounts(-1, -1, -1);

	    private readonly long _inputCount;
	    private readonly long _emitCount;
        private readonly long _outputCount;

        /// <summary>
        /// Creates a new <see cref="MapReduceCounts"/> using the given input count, emit count, and output count.
        /// </summary>
        /// <param name="inputCount"></param>
        /// <param name="emitCount"></param>
        /// <param name="outputCount"></param>
	    public MapReduceCounts(long inputCount, long emitCount, long outputCount) 
        {
		    _inputCount = inputCount;
		    _emitCount = emitCount;
		    _outputCount = outputCount;
	    }

	    public long InputCount
        {
	        get { return _inputCount; }
        }

	    public long EmitCount
        {
	        get { return _emitCount; }
        }

	    public long OutputCount 
        {
	        get { return _outputCount; }
        }

	    public override string ToString()
	    {
	        return "MapReduceCounts [inputCount=" + _inputCount + ", emitCount=" + _emitCount + ", outputCount=" +
	               _outputCount + "]";
	    }

	    public override int GetHashCode()
        {
		    int prime = 31;
		    long result = 1;

		    result = prime * result + _emitCount;
		    result = prime * result + _inputCount;
		    result = prime * result + _outputCount;

		    return System.Convert.ToInt32(result);
	    }

	    public override bool Equals(object obj)
        {
		    if (this == obj) return true;
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;

            var other = (MapReduceCounts) obj;

		    if (_emitCount != other._emitCount) return false;
            if (_inputCount != other._inputCount) return false;
            if (_outputCount != other._outputCount) return false;

            return true;
	    }

    }
}
