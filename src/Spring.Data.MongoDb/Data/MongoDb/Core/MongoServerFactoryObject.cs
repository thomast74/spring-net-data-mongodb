// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoServerFactoryObject.cs" company="The original author or authors.">
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

using System;
using MongoDB.Driver;
using Spring.Dao;
using Spring.Dao.Support;
using Spring.Objects.Factory;

namespace Spring.Data.MongoDb.Core
{
    /// <summary>
    /// Convenient factory for configuring MongoDB.
    /// </summary>
    /// <author>Thomas Risberg</author>
    /// <author>Graeme Rocher</author>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class MongoServerFactoryObject : IFactoryObject, IDisposable, IPersistenceExceptionTranslator
    {
	    private MongoServer _mongo;

	    private MongoServerSettings _settings;
        private string _connectionString;
        private WriteConcern _writeConcern;
        private bool _singleton = true;

	    private IPersistenceExceptionTranslator _exceptionTranslator = new MongoExceptionTranslator();

        public MongoServerSettings Settings
        {
            set { _settings = value; }
        }

        public string ConnectionString
        {
            set { _connectionString = value; }
        }

        public WriteConcern WriteConcern
        {
            set { _writeConcern = value; }
        }

        public bool IsSingleton
        {
            get { return _singleton; }
            set { _singleton = value; }
        }

        public IPersistenceExceptionTranslator ExceptionTranslator
        {
            set { _exceptionTranslator = value; }
        }

        /// <summary>
        /// Return an instance of <see cref="MongoServer"/> of the object managed by this factory.
        /// If <see cref="IsSingleton" /> is <code>True</code> it will return the same instance,
        /// if <code>False</code> it will create always a new instance.
        /// </summary>
        /// <remarks>
        /// <note type="caution">If this method is being called in the context of an enclosing IoC container and
        ///             returns <see langword="null"/>, the IoC container will consider this factory
        ///             object as not being fully initialized and throw a corresponding (and most
        ///             probably fatal) exception.
        ///             </note>
        /// </remarks>
        /// <returns>
        /// An instance of <see cref="MongoServer"/>
        /// </returns>
        public object GetObject()
        {
            if (_singleton)
	        {
	            return _mongo ?? (_mongo = CreateInstance());
	        }

            return CreateInstance();
        }

        /// <summary>
        /// Return the typeof(<see cref="MongoServer"/>) that this
        ///             <see cref="MongoServerFactoryObject"/> creates.
        /// </summary>
        public Type ObjectType
        {
            get { return typeof (MongoServer); }
        }

        /// <summary>
        /// Translate the given exception thrown by a persistence framework to a
        ///             corresponding exception from Spring's generic DataAccessException hierarchy,
        ///             if possible.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Do not translate exceptions that are not understand by this translator:
        ///             for example, if coming from another persistence framework, or resulting
        ///             from user code and unrelated to persistence.
        /// </para>
        /// <para>
        /// Of particular importance is the correct translation to <see cref="T:Spring.Dao.DataIntegrityViolationException"/>
        ///             for example on constraint violation.  Implementations may use Spring ADO.NET Framework's 
        ///             sophisticated exception translation to provide further information in the event of SQLException as a root cause.
        /// </para>
        /// </remarks>
        /// <param name="ex">The exception thrown.</param>
        /// <returns>
        /// the corresponding DataAccessException (or 
        /// <code>
        /// null
        /// </code>
        ///  if the
        ///             exception could not be translated, as in this case it may result from
        ///             user code rather than an actual persistence problem)
        /// </returns>
        /// <seealso cref="T:Spring.Dao.DataIntegrityViolationException"/><seealso cref="T:Spring.Data.Support.ErrorCodeExceptionTranslator"/>
        /// <author>Rod Johnson</author>
        /// <author>Juergen Hoeller</author>
        /// <author>Mark Pollack (.NET)</author>
        public DataAccessException TranslateExceptionIfPossible(Exception ex)
        {
		    return _exceptionTranslator.TranslateExceptionIfPossible(ex);
	    }

        private MongoServer CreateInstance()
        {
            if (_settings == null && string.IsNullOrEmpty(_connectionString))
            {
                throw new ObjectCreationException("Either MongoServerSettings or ConnectionStirng needs to be provided.");
            }
            
            MongoServer mongo;
            if (!string.IsNullOrEmpty(_connectionString))
                mongo = MongoServer.Create(_connectionString);
            else
                mongo = MongoServer.Create(_settings);

            if (_writeConcern != null)
                mongo.Settings.WriteConcern = _writeConcern;

            if (_singleton)
                _mongo = mongo;

            return mongo;
        }

        /// <summary>
        /// In case of singleton a <see cref="MongoServer.Disconnect" /> is executed.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose() 
        {
            if (_mongo != null)
		        _mongo.Disconnect();
	    }
    }
}
