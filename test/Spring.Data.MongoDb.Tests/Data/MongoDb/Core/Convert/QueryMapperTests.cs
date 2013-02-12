#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryMapperTests.cs" company="The original author or authors.">
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

using NUnit.Framework;
using Spring.Data.MongoDb.Core.Mapping;

namespace Spring.Data.MongoDb.Core.Convert
{
    /// <summary>
    /// Unit tests for <see cref="QueryMapper"/>
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    [Category(TestCategory.Unit)]
    public class QueryMapperTests
    {
        private QueryMapper mapper;
        private MongoMappingContext context;
        //private MappingMongoConverter converter;

        private IMongoDbFactory factory ;

        [SetUp]
        public void SetUp()
        {
            context = new MongoMappingContext();

            //converter = new MappingMongoConverter(factory, context);
            //converter.AfterPropertiesSet();

            //mapper = new QueryMapper(converter);
        }

        /* TODO Make it compile

	    [Test]
	    public void TranslatesIdPropertyIntoIdKey() 
        {
		    DBObject query = new BasicDBObject("foo", "value");
		    MongoPersistentEntity<?> entity = context.getPersistentEntity(Sample);

		    DBObject result = mapper.getMappedObject(query, entity);
		    Assert.That(result.get("_id"), is(notNullValue()));
		    Assert.That(result.get("foo"), is(nullValue()));
	    }

	    [Test]
	    public void ConvertsStringIntoObjectId() 
        {
		    DBObject query = new BasicDBObject("_id", new ObjectId().toString());
		    DBObject result = mapper.getMappedObject(query, context.getPersistentEntity(IdWrapper));
		    Assert.That(result.get("_id"), is(instanceOf(ObjectId)));
	    }

	    [Test]
	    public void HandlesBigIntegerIdsCorrectly() 
        {
		    DBObject dbObject = new BasicDBObject("id", new BigInteger("1"));
		    DBObject result = mapper.getMappedObject(dbObject, context.getPersistentEntity(IdWrapper));
		    Assert.That(result.get("_id"), is((Object) "1"));
	    }

	    [Test]
	    public void HandlesObjectIdCapableBigIntegerIdsCorrectly() 
        {
		    ObjectId id = new ObjectId();
		    DBObject dbObject = new BasicDBObject("id", new BigInteger(id.toString(), 16));
		    DBObject result = mapper.getMappedObject(dbObject, context.getPersistentEntity(IdWrapper));
		    Assert.That(result.get("_id"), is((Object) id));
	    }

	    [Test]
	    public void Translates$NeCorrectly() 
        {
		    Criteria criteria = where("foo").ne(new ObjectId().toString());

		    DBObject result = mapper.getMappedObject(criteria.getCriteriaObject(), context.getPersistentEntity(Sample));
		    Object object = result.get("_id");
		    Assert.That(object, is(instanceOf(DBObject)));
		    DBObject dbObject = (DBObject) object;
		    Assert.That(dbObject.get("$ne"), is(instanceOf(ObjectId)));
	    }

	    [Test]
	    public void HandlesEnumsCorrectly() 
        {
		    Query query = query(where("foo").is(Enum.INSTANCE));
		    DBObject result = mapper.getMappedObject(query.getQueryObject(), null);

		    Object object = result.get("foo");
		    Assert.That(object, is(instanceOf(String)));
	    }

	    [Test]
	    public void HandlesEnumsInNotEqualCorrectly() 
        {
		    Query query = query(where("foo").ne(Enum.INSTANCE));
		    DBObject result = mapper.getMappedObject(query.getQueryObject(), null);

		    Object object = result.get("foo");
		    Assert.That(object, is(instanceOf(DBObject)));

		    Object ne = ((DBObject) object).get("$ne");
		    Assert.That(ne, is(instanceOf(String)));
		    Assert.That(ne.toString(), is(Enum.INSTANCE.name()));
	    }

	    [Test]
	    public void HandlesEnumsIn$InCorrectly() 
        {
		    Query query = query(where("foo").in(Enum.INSTANCE));
		    DBObject result = mapper.getMappedObject(query.getQueryObject(), null);

		    Object object = result.get("foo");
		    Assert.That(object, is(instanceOf(DBObject)));

		    Object in = ((DBObject) object).get("$in");
		    Assert.That(in, is(instanceOf(BasicDBList)));

		    BasicDBList list = (BasicDBList) in;
		    Assert.That(list.size(), is(1));
		    Assert.That(list.get(0), is(instanceOf(String)));
		    Assert.That(list.get(0).toString(), is(Enum.INSTANCE.name()));
	    }

	    [Test]
	    public void HandlesNativelyBuiltQueryCorrectly() 
        {
		    DBObject query = new QueryBuilder().or(new BasicDBObject("foo", "bar")).get();
		    mapper.getMappedObject(query, null);
	    }

	    [Test]
	    public void HandlesAllPropertiesIfDBObject() 
        {
		    DBObject query = new BasicDBObject();
		    query.put("foo", new BasicDBObject("$in", Arrays.asList(1, 2)));
		    query.put("bar", new Person());

		    DBObject result = mapper.getMappedObject(query, null);
		    Assert.That(result.get("bar"), is(notNullValue()));
	    }

	    [Test]
	    public void TransformsArraysCorrectly() 
        {
		    Query query = new BasicQuery("{ 'tags' : { '$all' : [ 'green', 'orange']}}");

		    DBObject result = mapper.getMappedObject(query.getQueryObject(), null);
		    Assert.That(result, is(query.getQueryObject()));
	    }

	    [Test]
	    public void DoesHandleNestedFieldsWithDefaultIdNames() 
        {
		    BasicDBObject dbObject = new BasicDBObject("id", new ObjectId().toString());
		    dbObject.put("nested", new BasicDBObject("id", new ObjectId().toString()));

		    MongoPersistentEntity<?> entity = context.getPersistentEntity(ClassWithDefaultId);

		    DBObject result = mapper.getMappedObject(dbObject, entity);
		    Assert.That(result.get("_id"), is(instanceOf(ObjectId)));
		    Assert.That(((DBObject) result.get("nested")).get("_id"), is(instanceOf(ObjectId)));
	    }

	    [Test]
	    public void DoesNotTranslateNonIdPropertiesFor$NeCriteria() 
        {
		    ObjectId accidentallyAnObjectId = new ObjectId();

		    Query query = Query.query(Criteria.where("id").is("id_value").and("publishers")
				    .ne(accidentallyAnObjectId.toString()));

		    DBObject dbObject = mapper.getMappedObject(query.getQueryObject(), context.getPersistentEntity(UserEntity));
		    Assert.That(dbObject.get("publishers"), is(instanceOf(DBObject)));

		    DBObject publishers = (DBObject) dbObject.get("publishers");
		    Assert.That(publishers.containsField("$ne"), is(true));
		    Assert.That(publishers.get("$ne"), is(instanceOf(String)));
	    }

	    [Test]
	    public void UsesEntityMetadataInOr() 
        {
		    Query query = query(new Criteria().orOperator(where("foo").is("bar")));
		    DBObject result = mapper.getMappedObject(query.getQueryObject(), context.getPersistentEntity(Sample));

		    Assert.That(result.keySet(), hasSize(1));
		    Assert.That(result.keySet(), hasItem("$or"));

		    BasicDBList ors = getAsDBList(result, "$or");
		    Assert.That(ors, hasSize(1));
		    DBObject criterias = getAsDBObject(ors, 0);
		    Assert.That(criterias.keySet(), hasSize(1));
		    Assert.That(criterias.get("_id"), is(notNullValue()));
		    Assert.That(criterias.get("foo"), is(nullValue()));
	    }

	    [Test]
	    public void TranslatesPropertyReferenceCorrectly() 
        {
		    Query query = query(where("field").is(new CustomizedField()));
		    DBObject result = mapper
				    .getMappedObject(query.getQueryObject(), context.getPersistentEntity(CustomizedField));

		    Assert.That(result.containsField("foo"), is(true));
		    Assert.That(result.keySet().size(), is(1));
	    }

	    [Test]
	    public void TranslatesNestedPropertyReferenceCorrectly() 
        {
		    Query query = query(where("field.field").is(new CustomizedField()));
		    DBObject result = mapper
				    .getMappedObject(query.getQueryObject(), context.getPersistentEntity(CustomizedField));

		    Assert.That(result.containsField("foo.foo"), is(true));
		    Assert.That(result.keySet().size(), is(1));
	    }

	    [Test]
	    public void ReturnsOriginalKeyIfNoPropertyReference() 
        {
		    Query query = query(where("bar").is(new CustomizedField()));
		    DBObject result = mapper
				    .getMappedObject(query.getQueryObject(), context.getPersistentEntity(CustomizedField));

		    Assert.That(result.containsField("bar"), is(true));
		    Assert.That(result.keySet().size(), is(1));
	    }

	    [Test]
	    public void ConvertsAssociationCorrectly() 
        {
		    Reference reference = new Reference();
		    reference.id = 5L;

		    Query query = query(where("reference").is(reference));
		    DBObject object = mapper.getMappedObject(query.getQueryObject(), context.getPersistentEntity(WithDBRef));

		    Object referenceObject = object.get("reference");

		    Assert.That(referenceObject, is(instanceOf(com.mongodb.DBRef)));
	    }

	    [Test]
	    public void ConvertsNestedAssociationCorrectly() 
        {
		    Reference reference = new Reference();
		    reference.id = 5L;

		    Query query = query(where("withDbRef.reference").is(reference));
		    DBObject object = mapper.getMappedObject(query.getQueryObject(),
				    context.getPersistentEntity(WithDBRefWrapper));

		    Object referenceObject = object.get("withDbRef.reference");

		    Assert.That(referenceObject, is(instanceOf(com.mongodb.DBRef)));
	    }

	    [Test]
	    public void ConvertsInKeywordCorrectly() 
        {
		    Reference first = new Reference();
		    first.id = 5L;

		    Reference second = new Reference();
		    second.id = 6L;

		    Query query = query(where("reference").in(first, second));
		    DBObject result = mapper.getMappedObject(query.getQueryObject(), context.getPersistentEntity(WithDBRef));

		    DBObject reference = DBObjectUtils.getAsDBObject(result, "reference");
		    Assert.That(reference.containsField("$in"), is(true));
	    }

	    [Test]
	    public void CorrectlyConvertsNullReference() 
        {
		    Query query = query(where("reference").is(null));
		    DBObject object = mapper.getMappedObject(query.getQueryObject(), context.getPersistentEntity(WithDBRef));

		    Assert.That(object.get("reference"), is(nullValue()));
	    }



	    class IdWrapper 
        {
		    private object id;

	        public object Id { get { return id; } set { id = value; } }
	    }

	    class ClassWithDefaultId
        {
		    private string id;
		    private ClassWithDefaultId nested;

	        public string Id { get { return id; } set { id = value; } }
	        public ClassWithDefaultId Nested { get { return nested;  } set { nested = value; } }
	    }

	    class Sample 
        {
            [Id]
		    private string foo;

            public string Foo { get { return foo; } set { foo = value; } }
	    }

	    class BigIntegerId 
        {
		    [Id]
		    private BigInteger id;

            public BigInteger Id { get { return id; } set { id = value; } }
	    }

	    enum Enum 
        {
		    INSTANCE
	    }

	    class UserEntity 
        {
		    private string id;
		    private List<string> publishers = new List<string>();

            public string Id { get { return id; } set { id = value; } }
            public List<string> Publishers { get { return publishers; } set { publishers = value; } }
	    }

	    class CustomizedField
        {
		    [Field("foo")]
		    private CustomizedField field;

	        public CustomizedField Field { get { return field; } set { field = value; } }
	    }

	    class WithDBRef 
        {
		    [DBRef]
		    private Reference reference;

	        public Reference Reference { get { return reference; } set { reference = value; } }
	    }

	    class Reference 
        {
		    private long id;

	        public long Id { get { return id; } set { id = value; } }
	    }
         
	    class WithDBRefWrapper 
        {
		    private WithDBRef withDbRef;

	        public WithDBRef WithDbRef { get { return withDbRef; } set { withDbRef = value; } }
	    }
        */
    }
}
