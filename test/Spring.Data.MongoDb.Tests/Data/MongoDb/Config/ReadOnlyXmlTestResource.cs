﻿#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReadOnlyXmlTestResource.cs" company="The original author or authors.">
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

using System;
using System.IO;
using System.Text;
using Spring.Core.IO;

namespace Spring.Data.MongoDb.Config
{
    /// <summary>
    /// Centralised resource getter for loading all of those XML object
    /// definition files used in the XML based object factory tests.
    /// </summary>
    /// <author>Rick Evans (.NET)</author>
    /// <author>Erich Eichinger (.NET)</author>
    public class ReadOnlyXmlTestResource : IResource 
    {
        private readonly IResource _underlyingResource;
        private const string TestDataFolder = ".";

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ReadOnlyXmlTestResource"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The filename/resourcename (e.g. foo.txt) of the XML file containing the object
        /// definitions.
        /// </param>
        public ReadOnlyXmlTestResource(string fileName)
        {
            if (ConfigurableResourceLoader.HasProtocol(fileName))
            {
                ConfigurableResourceLoader loader = new ConfigurableResourceLoader();
                _underlyingResource = loader.GetResource(fileName);
            }
            _underlyingResource = new FileSystemResource(fileName);
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ReadOnlyXmlTestResource"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The filename/resourcename (e.g. foo.txt) of the XML file containing the object
        /// definitions.
        /// </param>
        /// <param name="associatedTestType">
        /// The <see cref="System.Type"/> of the test class utilising said file.
        /// </param>
        public ReadOnlyXmlTestResource(string fileName, Type associatedTestType)
            : this(GetFilePath(fileName, associatedTestType))
        {
        }

        public IResource CreateRelative(string relativePath)
        {
            return _underlyingResource.CreateRelative(relativePath);
        }

        public bool IsOpen
        {
            get { return _underlyingResource.IsOpen; }
        }

        public Uri Uri
        {
            get { return _underlyingResource.Uri; }
        }

        public FileInfo File
        {
            get { return _underlyingResource.File; }
        }

        public string Description
        {
            get { return _underlyingResource.Description; }
        }

        public bool Exists
        {
            get { return _underlyingResource.Exists; }
        }

        public Stream InputStream
        {
            get { return _underlyingResource.InputStream; }
        }

        public static string GetFilePath(string fileName, Type associatedTestType)
        {
            if (associatedTestType == null)
            {
                return fileName;
            }

            // check filesystem
            StringBuilder path = new StringBuilder(TestDataFolder).Append(Path.DirectorySeparatorChar.ToString());
            path.Append(associatedTestType.Namespace.Replace(".", Path.DirectorySeparatorChar.ToString()));
            path.Append(Path.DirectorySeparatorChar.ToString()).Append(fileName);

            FileInfo file = new FileInfo(path.ToString());
            if (file.Exists)
            {
                return path.ToString();
            }

            // interpret as assembly resource
            fileName = "assembly://" + associatedTestType.Assembly.FullName + "/" + associatedTestType.Namespace + "/" +
                       fileName;

            return fileName;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
