// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapReduceTiming.cs" company="The original author or authors.">
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

using System;

namespace Spring.Data.MongoDb.Core.Mapreduce
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MapReduceTiming
    {
	    private readonly long _mapTime;
        private readonly long _emitLoopTime;
        private readonly long _totalTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapTime"></param>
        /// <param name="emitLoopTime"></param>
        /// <param name="totalTime"></param>
	    public MapReduceTiming(long mapTime, long emitLoopTime, long totalTime)
        {
		    _mapTime = mapTime;
		    _emitLoopTime = emitLoopTime;
		    _totalTime = totalTime;
	    }

	    public long MapTime
        {
	        get { return _mapTime; }
        }

	    public long EmitLoopTime
        {
	        get { return _emitLoopTime; }
        }

	    public long TotalTime
        {
	        get { return _totalTime; }
        }

	    public override string ToString()
        {
		    return "MapReduceTiming [mapTime=" + _mapTime + ", emitLoopTime=" + _emitLoopTime + ", totalTime=" + _totalTime + "]";
	    }

	    public override int GetHashCode()
        {
		    int prime = 31;
		    int result = 1;
		    result = prime * result + (int) (_emitLoopTime ^ (_emitLoopTime >> 32));
		    result = prime * result + (int) (_mapTime ^ (_mapTime >> 32));
		    result = prime * result + (int) (_totalTime ^ (_totalTime >> 32));
		    return result;
	    }

	    public override bool Equals(Object obj)
        {
		    if (this == obj)
			    return true;
		    if (obj == null)
			    return false;
		    if (GetType() != obj.GetType())
			    return false;

		    var other = (MapReduceTiming) obj;
		    if (_emitLoopTime != other._emitLoopTime)
			    return false;
		    if (_mapTime != other._mapTime)
			    return false;
		    if (_totalTime != other._totalTime)
			    return false;
		    return true;
	    }

    }
}
