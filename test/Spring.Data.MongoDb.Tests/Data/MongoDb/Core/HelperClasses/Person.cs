#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Person.cs" company="The original author or authors.">
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
using MongoDB.Bson;

namespace Spring.Data.MongoDb.Core.HelperClasses
{
    /// <summary>
    /// Test Helper Class
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    public class Person
    {
	    private ObjectId _id;
	    private string _firstName;
	    private int _age;
	    private Person _friend;
        private bool _active = true;

        public Person()
        {
		    _id = new ObjectId();
	    }

	    public Person(ObjectId id, String firstname) 
        {
		    _id = id;
		    _firstName = firstname;
	    }

	    public Person(String firstname, int age)
            : this()
        {
		    _firstName = firstname;
		    _age = age;
	    }

	    public Person(String firstname)
            : this()
        {
		    _firstName = firstname;
	    }


	    public ObjectId Id
	    {
		    get { return _id;}
            set { _id = value; }
	    }

	    public string FirstName
	    {
		    get { return _firstName;}
		    set { _firstName = value;}
	    }

	    public int Age
	    {
		    get { return _age;}
		    set { _age = value;}
	    }

	    public Person Friend
	    {
		    get { return _friend;}
		    set { _friend = value;}
	    }

        public bool IsActive { get { return _active; }}

        public override string ToString()
        {
            return "Person [id=" + _id + ", firstName=" + _firstName + ", age=" + _age + ", friend=" + _friend + "]";
        }

	    public override bool Equals(Object obj)
        {
		    if (obj == this) {
			    return true;
		    }

		    if (obj == null) {
			    return false;
		    }

		    if (GetType() != GetType())
            {
			    return false;
		    }

		    Person that = (Person) obj;

		    return _id == null ? false : _id.Equals(that._id);
	    }

	    public override int GetHashCode()
        {
		    return _id.GetHashCode();
	    }

    }
}
