#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FindAndModifyOptions.cs" company="The original author or authors.">
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
#endregion

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Factory method to create a FindAndModifyOptions instance
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class FindAndModifyOptions
    {
        private bool _returnNew;
        private bool _upsert;

        private FindAndModifyOptions()
        {
        }

        public static FindAndModifyOptions Default()
        {
            return new FindAndModifyOptions();
        }

        public FindAndModifyOptions ReturnNew(bool returnNew)
        {
            _returnNew = returnNew;
            return this;
        }

        public FindAndModifyOptions Upsert(bool upsert)
        {
            _upsert = upsert;
            return this;
        }

        public bool IsReturnNew
        {
            get { return _returnNew; }
        }

        public bool IsUpsert
        {
            get { return _upsert; }
        }
    }
}
