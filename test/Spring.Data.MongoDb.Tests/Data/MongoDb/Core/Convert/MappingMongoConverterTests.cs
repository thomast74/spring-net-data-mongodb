#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingMongoConverterTests.cs" company="The original author or authors.">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using NSubstitute;
using NUnit.Framework;
using Spring.Context;
using Spring.Data.MongoDb.Core.Mapping;

namespace Spring.Data.MongoDb.Core.Convert
{
    /// <summary>
    /// Unit tests for <see cref="MappingMongoConverter"/>.
    /// </summary>
    /// <author>Oliver Gierk</author>
    /// <author>Thomas Trageser</author>
    [TestFixture]
    public class MappingMongoConverterTests
    {
	    MappingMongoConverter converter;
	    MongoMappingContext mappingContext;

        IMongoDbFactory factory;
        IApplicationContext context;

	    [SetUp]
	    public void SetUp()
	    {
	        factory = Substitute.For<IMongoDbFactory>();
	        context = Substitute.For<IApplicationContext>();

		    mappingContext = new MongoMappingContext();
		    mappingContext.ApplicationContext = context;
	        mappingContext.OnApplicationEvent(new ContextRefreshedEvent(context));

		    converter = new MappingMongoConverter(factory, mappingContext);
		    converter.AfterPropertiesSet();
	    }

	    [Test]
	    public void ConvertsAddressCorrectly() 
        {
		    Address address = new Address();
		    address.city = "New York";
		    address.street = "Broadway";

		    DBObject dbObject = new BasicDBObject();

		    converter.write(address, dbObject);

		    Assert.That(dbObject.get("city").toString(), is("New York"));
		    Assert.That(dbObject.get("street").toString(), is("Broadway"));
	    }

	    [Test]
	    public void ConvertsJodaTimeTypesCorrectly()
        {
		    List<TypeConverter> converters = new List<TypeConverter>();
		    converters.Add(new LocalDateToDateConverter());
		    converters.Add(new DateToLocalDateConverter());

		    CustomConversions conversions = new CustomConversions(converters);
		    mappingContext.SimpleTypeHolder = conversions.SimpleTypeHolder;

		    converter = new MappingMongoConverter(factory, mappingContext);
		    converter.CustomConversions = conversions;
		    converter.AfterPropertiesSet();

		    Person person = new Person();
		    person.BirthDate = new LocalDate();

		    BsonDocument dbObject = new BsonDocument();
		    converter.write(person, dbObject);

		    Assert.That(dbObject.GetValue("birthDate"), Is.TypeOf<LocalDate>());
            
		    Person result = converter.Read<Person>(dbObject);

		    Assert.That(result.BirthDate, Is.Not.Null);
	    }

	    [Test]
	    public void ConvertsCustomTypeOnConvertToMongoType()
        {
		    List<TypeConverter> converters = new List<TypeConverter>();
		    converters.Add(new LocalDateToDateConverter());
		    converters.Add(new DateToLocalDateConverter());

		    CustomConversions conversions = new CustomConversions(converters);
		    mappingContext.SimpleTypeHolder = conversions.SimpleTypeHolder;

		    converter = new MappingMongoConverter(factory, mappingContext);
		    converter.CustomConversions = conversions;
		    converter.AfterPropertiesSet();

		    LocalDate date = new LocalDate();
		    converter.ConvertToMongoType(date);
	    }

	    /**
	     * @see DATAMONGO-130
	     */
	    [Test]
	    public void WritesMapTypeCorrectly()
	    {
            CultureInfo locale = new CultureInfo("da-DK", false);

            Dictionary<CultureInfo, string> map = new Dictionary<CultureInfo, string>() { { locale, "Foo"} };

		    BsonDocument dbObject = new BsonDocument();
		    converter.Write(map, dbObject);

		    Assert.That(dbObject.GetValue(locale.ToString()).ToString(), Is.EqualTo("Foo"));
	    }

	    /**
	     * @see DATAMONGO-130
	     */
	    [Test]
	    public void ReadsMapWithCustomKeyTypeCorrectly()
        {
            CultureInfo locale = new CultureInfo("da-DK", false);

		    BsonDocument mapObject = new BsonDocument(locale.EnglishName, "Value");
		    BsonDocument dbObject = new BsonDocument("map", mapObject);

		    ClassWithMapProperty result = converter.Read<ClassWithMapProperty>(dbObject);
		    Assert.That(result.Map.Get(locale.EnglishName), Is.EqualTo("Value"));
	    }

	    /**
	     * @see DATAMONGO-128
	     */
	    [Test]
	    public void UsesDocumentsStoredTypeIfSubtypeOfRequest() 
        {
		    BsonDocument dbObject = new BsonDocument();
		    dbObject.Add("birthDate", new LocalDate());
		    dbObject.Add(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY, typeof(Person).Name);

		    Assert.That(converter.Read<Contact>(dbObject), Is.TypeOf<Person>());
	    }

	    /**
	     * @see DATAMONGO-128
	     */
	    [Test]
	    public void IgnoresDocumentsStoredTypeIfCompletelyDifferentTypeRequested()
        {
		    BsonDocument dbObject = new BsonDocument();
		    dbObject.Add("birthDate", new LocalDate());
		    dbObject.Add(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY, typeof(Person).Name);

		    Assert.That(converter.Read<BirthDateContainer>(dbObject), Is.TypeOf<BirthDateContainer>());
	    }

	    [Test]
	    public void WritesTypeDiscriminatorIntoRootObject() 
        {
		    Person person = new Person();

		    BsonDocument result = new BsonDocument();
		    converter.Write(person, result);

		    Assert.That(result.Contains(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY), Is.True);
		    Assert.That(result.GetValue(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY).ToString(), Is.EqualTo(typeof(Person).Name));
	    }

	    /**
	     * @see DATAMONGO-136
	     */
	    [Test]
	    public void WritesEnumsCorrectly()
        {
		    ClassWithEnumProperty value = new ClassWithEnumProperty();
		    value.sampleEnum = SampleEnum.FIRST;

		    BsonDocument result = new BsonDocument();
		    converter.Write(value, result);

		    Assert.That(result.GetValue("sampleEnum"), Is.TypeOf<string>());
		    Assert.That(result.GetValue("sampleEnum").ToString(), Is.EqualTo("FIRST"));
	    }

	    /**
	     * @see DATAMONGO-209
	     */
	    [Test]
	    public void WritesEnumCollectionCorrectly() 
        {
		    ClassWithEnumProperty value = new ClassWithEnumProperty();
		    value.Enums = new List<SampleEnum> { SampleEnum.FIRST };

		    BsonDocument result = new BsonDocument();
		    converter.Write(value, result);

		    Assert.That(result.GetValue("enums"), Is.TypeOf<BsonArray>());

	        BsonArray enums = (BsonArray)result.GetValue("enums");
		    Assert.That(enums.Count, Is.EqualTo(1));
		    Assert.That((string) enums[0], Is.EqualTo("FIRST"));
	    }

	    /**
	     * @see DATAMONGO-136
	     */
	    [Test]
	    public void ReadsEnumsCorrectly()
        {
		    BsonDocument dbObject = new BsonDocument("sampleEnum", "FIRST");
		    ClassWithEnumProperty result = converter.Read<ClassWithEnumProperty>(dbObject);

		    Assert.That(result.SampleEnum, Is.EqualTo(SampleEnum.FIRST));
	    } 

	    /**
	     * @see DATAMONGO-209
	     */
	    [Test]
	    public void ReadsEnumCollectionsCorrectly()
        {
		    BsonArray enums = new BsonArray();
		    enums.Add("FIRST");
		    BsonDocument dbObject = new BsonDocument("enums", enums);

		    ClassWithEnumProperty result = converter.Read<ClassWithEnumProperty>(dbObject);

		    Assert.That(result.Enums, Is.TypeOf<List>());
		    Assert.That(result.Enums.Count, Is.EqualTo(1));
		    Assert.That(result.Enums, Contains.Item(SampleEnum.FIRST));
	    }

	    /**
	     * @see DATAMONGO-144
	     */
	    [Test]
	    public void ConsidersFieldNameWhenWriting() 
        {
		    Person person = new Person();
		    person.Firstname = "Oliver";

		    BsonDocument result = new BsonDocument();
		    converter.Write(person, result);

		    Assert.That(result.Contains("foo"), is(true));
		    Assert.That(result.Contains("firstname"), is(false));
	    }

	    /**
	     * @see DATAMONGO-144
	     */
	    [Test]
	    public void considersFieldNameWhenReading() {

		    DBObject dbObject = new BasicDBObject("foo", "Oliver");
		    Person result = converter.read(Person.class, dbObject);

		    Assert.That(result.firstname, is("Oliver"));
	    }

	    [Test]
	    public void resolvesNestedComplexTypeForConstructorCorrectly() {

		    DBObject address = new BasicDBObject("street", "110 Southwark Street");
		    address.put("city", "London");

		    BasicDBList addresses = new BasicDBList();
		    addresses.add(address);

		    DBObject person = new BasicDBObject("firstname", "Oliver");
		    person.put("addresses", addresses);

		    Person result = converter.read(Person.class, person);
		    Assert.That(result.addresses, is(notNullValue()));
	    }

	    /**
	     * @see DATAMONGO-145
	     */
	    [Test]
	    public void writesCollectionWithInterfaceCorrectly() {

		    Person person = new Person();
		    person.firstname = "Oliver";

		    CollectionWrapper wrapper = new CollectionWrapper();
		    wrapper.contacts = Arrays.asList((Contact) person);

		    BasicDBObject dbObject = new BasicDBObject();
		    converter.write(wrapper, dbObject);

		    Object result = dbObject.get("contacts");
		    Assert.That(result, is(instanceOf(BasicDBList.class)));
		    BasicDBList contacts = (BasicDBList) result;
		    DBObject personDbObject = (DBObject) contacts.get(0);
		    Assert.That(personDbObject.get("foo").toString(), is("Oliver"));
		    Assert.That((String) personDbObject.get(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY), is(Person.class.getName()));
	    }

	    /**
	     * @see DATAMONGO-145
	     */
	    [Test]
	    public void readsCollectionWithInterfaceCorrectly() {

		    BasicDBObject person = new BasicDBObject(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY, Person.class.getName());
		    person.put("foo", "Oliver");

		    BasicDBList contacts = new BasicDBList();
		    contacts.add(person);

		    CollectionWrapper result = converter.read(CollectionWrapper.class, new BasicDBObject("contacts", contacts));
		    Assert.That(result.contacts, is(notNullValue()));
		    Assert.That(result.contacts.size(), is(1));
		    Contact contact = result.contacts.get(0);
		    Assert.That(contact, is(instanceOf(Person.class)));
		    Assert.That(((Person) contact).firstname, is("Oliver"));
	    }

	    [Test]
	    public void convertsLocalesOutOfTheBox() {
		    LocaleWrapper wrapper = new LocaleWrapper();
		    wrapper.locale = Locale.US;

		    DBObject dbObject = new BasicDBObject();
		    converter.write(wrapper, dbObject);

		    Object localeField = dbObject.get("locale");
		    Assert.That(localeField, is(instanceOf(String.class)));
		    Assert.That((String) localeField, is("en_US"));

		    LocaleWrapper read = converter.read(LocaleWrapper.class, dbObject);
		    Assert.That(read.locale, is(Locale.US));
	    }

	    /**
	     * @see DATAMONGO-161
	     */
	    [Test]
	    public void readsNestedMapsCorrectly() {

		    Map<String, String> secondLevel = new HashMap<String, String>();
		    secondLevel.put("key1", "value1");
		    secondLevel.put("key2", "value2");

		    Map<String, Map<String, String>> firstLevel = new HashMap<String, Map<String, String>>();
		    firstLevel.put("level1", secondLevel);
		    firstLevel.put("level2", secondLevel);

		    ClassWithNestedMaps maps = new ClassWithNestedMaps();
		    maps.nestedMaps = new LinkedHashMap<String, Map<String, Map<String, String>>>();
		    maps.nestedMaps.put("afield", firstLevel);

		    DBObject dbObject = new BasicDBObject();
		    converter.write(maps, dbObject);

		    ClassWithNestedMaps result = converter.read(ClassWithNestedMaps.class, dbObject);
		    Map<String, Map<String, Map<String, String>>> nestedMap = result.nestedMaps;
		    Assert.That(nestedMap, is(notNullValue()));
		    Assert.That(nestedMap.get("afield"), is(firstLevel));
	    }

	    /**
	     * @see DATACMNS-42, DATAMONGO-171
	     */
	    [Test]
	    public void writesClassWithBigDecimal() {

		    BigDecimalContainer container = new BigDecimalContainer();
		    container.value = BigDecimal.valueOf(2.5d);
		    container.map = Collections.singletonMap("foo", container.value);

		    DBObject dbObject = new BasicDBObject();
		    converter.write(container, dbObject);

		    Assert.That(dbObject.get("value"), is(instanceOf(String.class)));
		    Assert.That((String) dbObject.get("value"), is("2.5"));
		    Assert.That(((DBObject) dbObject.get("map")).get("foo"), is(instanceOf(String.class)));
	    }

	    /**
	     * @see DATACMNS-42, DATAMONGO-171
	     */
	    [Test]
	    public void readsClassWithBigDecimal() {

		    DBObject dbObject = new BasicDBObject("value", "2.5");
		    dbObject.put("map", new BasicDBObject("foo", "2.5"));

		    BasicDBList list = new BasicDBList();
		    list.add("2.5");
		    dbObject.put("collection", list);
		    BigDecimalContainer result = converter.read(BigDecimalContainer.class, dbObject);

		    Assert.That(result.value, is(BigDecimal.valueOf(2.5d)));
		    Assert.That(result.map.get("foo"), is(BigDecimal.valueOf(2.5d)));
		    Assert.That(result.collection.get(0), is(BigDecimal.valueOf(2.5d)));
	    }

	    [Test]
	    @SuppressWarnings("unchecked")
	    public void writesNestedCollectionsCorrectly() {

		    CollectionWrapper wrapper = new CollectionWrapper();
		    wrapper.strings = Arrays.asList(Arrays.asList("Foo"));

		    DBObject dbObject = new BasicDBObject();
		    converter.write(wrapper, dbObject);

		    Object outerStrings = dbObject.get("strings");
		    Assert.That(outerStrings, is(instanceOf(BasicDBList.class)));

		    BasicDBList typedOuterString = (BasicDBList) outerStrings;
		    Assert.That(typedOuterString.size(), is(1));
	    }

	    /**
	     * @see DATAMONGO-192
	     */
	    [Test]
	    public void readsEmptySetsCorrectly() {

		    Person person = new Person();
		    person.addresses = Collections.emptySet();

		    DBObject dbObject = new BasicDBObject();
		    converter.write(person, dbObject);
		    converter.read(Person.class, dbObject);
	    }

	    [Test]
	    public void convertsObjectIdStringsToObjectIdCorrectly() {
		    PersonPojoStringId p1 = new PersonPojoStringId("1234567890", "Text-1");
		    DBObject dbo1 = new BasicDBObject();

		    converter.write(p1, dbo1);
		    Assert.That(dbo1.get("_id"), is(instanceOf(String.class)));

		    PersonPojoStringId p2 = new PersonPojoStringId(new ObjectId().toString(), "Text-1");
		    DBObject dbo2 = new BasicDBObject();

		    converter.write(p2, dbo2);
		    Assert.That(dbo2.get("_id"), is(instanceOf(ObjectId.class)));
	    }

	    /**
	     * @see DATAMONGO-207
	     */
	    [Test]
	    public void convertsCustomEmptyMapCorrectly() {

		    DBObject map = new BasicDBObject();
		    DBObject wrapper = new BasicDBObject("map", map);

		    ClassWithSortedMap result = converter.read(ClassWithSortedMap.class, wrapper);

		    Assert.That(result, is(instanceOf(ClassWithSortedMap.class)));
		    Assert.That(result.map, is(instanceOf(SortedMap.class)));
	    }

	    /**
	     * @see DATAMONGO-211
	     */
	    [Test]
	    public void maybeConvertHandlesNullValuesCorrectly() {
		    Assert.That(converter.convertToMongoType(null), is(nullValue()));
	    }

	    [Test]
	    public void writesGenericTypeCorrectly() {

		    GenericType<Address> type = new GenericType<Address>();
		    type.content = new Address();
		    type.content.city = "London";

		    BasicDBObject result = new BasicDBObject();
		    converter.write(type, result);

		    DBObject content = (DBObject) result.get("content");
		    Assert.That(content.get("_class"), is(notNullValue()));
		    Assert.That(content.get("city"), is(notNullValue()));
	    }

	    [Test]
	    public void readsGenericTypeCorrectly() {

		    DBObject address = new BasicDBObject("_class", Address.class.getName());
		    address.put("city", "London");

		    GenericType<?> result = converter.read(GenericType.class, new BasicDBObject("content", address));
		    Assert.That(result.content, is(instanceOf(Address.class)));

	    }

	    /**
	     * @see DATAMONGO-228
	     */
	    [Test]
	    public void writesNullValuesForMaps() {

		    ClassWithMapProperty foo = new ClassWithMapProperty();
		    foo.map = Collections.singletonMap(Locale.US, null);

		    DBObject result = new BasicDBObject();
		    converter.write(foo, result);

		    Object map = result.get("map");
		    Assert.That(map, is(instanceOf(DBObject.class)));
		    Assert.That(((DBObject) map).keySet(), hasItem("en_US"));
	    }

	    [Test]
	    public void writesBigIntegerIdCorrectly() {

		    ClassWithBigIntegerId foo = new ClassWithBigIntegerId();
		    foo.id = BigInteger.valueOf(23L);

		    DBObject result = new BasicDBObject();
		    converter.write(foo, result);

		    Assert.That(result.get("_id"), is(instanceOf(String.class)));
	    }

	    public void convertsObjectsIfNecessary() {

		    ObjectId id = new ObjectId();
		    Assert.That(converter.convertToMongoType(id), is((Object) id));
	    }

	    /**
	     * @see DATAMONGO-235
	     */
	    [Test]
	    public void writesMapOfListsCorrectly() {

		    ClassWithMapProperty input = new ClassWithMapProperty();
		    input.mapOfLists = Collections.singletonMap("Foo", Arrays.asList("Bar"));

		    BasicDBObject result = new BasicDBObject();
		    converter.write(input, result);

		    Object field = result.get("mapOfLists");
		    Assert.That(field, is(instanceOf(DBObject.class)));

		    DBObject map = (DBObject) field;
		    Object foo = map.get("Foo");
		    Assert.That(foo, is(instanceOf(BasicDBList.class)));

		    BasicDBList value = (BasicDBList) foo;
		    Assert.That(value.size(), is(1));
		    Assert.That((String) value.get(0), is("Bar"));
	    }

	    /**
	     * @see DATAMONGO-235
	     */
	    [Test]
	    public void readsMapListValuesCorrectly() {

		    BasicDBList list = new BasicDBList();
		    list.add("Bar");
		    DBObject source = new BasicDBObject("mapOfLists", new BasicDBObject("Foo", list));

		    ClassWithMapProperty result = converter.read(ClassWithMapProperty.class, source);
		    Assert.That(result.mapOfLists, is(not(nullValue())));
	    }

	    /**
	     * @see DATAMONGO-235
	     */
	    [Test]
	    public void writesMapsOfObjectsCorrectly() {

		    ClassWithMapProperty input = new ClassWithMapProperty();
		    input.mapOfObjects = new HashMap<String, Object>();
		    input.mapOfObjects.put("Foo", Arrays.asList("Bar"));

		    BasicDBObject result = new BasicDBObject();
		    converter.write(input, result);

		    Object field = result.get("mapOfObjects");
		    Assert.That(field, is(instanceOf(DBObject.class)));

		    DBObject map = (DBObject) field;
		    Object foo = map.get("Foo");
		    Assert.That(foo, is(instanceOf(BasicDBList.class)));

		    BasicDBList value = (BasicDBList) foo;
		    Assert.That(value.size(), is(1));
		    Assert.That((String) value.get(0), is("Bar"));
	    }

	    /**
	     * @see DATAMONGO-235
	     */
	    [Test]
	    public void readsMapOfObjectsListValuesCorrectly() {

		    BasicDBList list = new BasicDBList();
		    list.add("Bar");
		    DBObject source = new BasicDBObject("mapOfObjects", new BasicDBObject("Foo", list));

		    ClassWithMapProperty result = converter.read(ClassWithMapProperty.class, source);
		    Assert.That(result.mapOfObjects, is(not(nullValue())));
	    }

	    /**
	     * @see DATAMONGO-245
	     */
	    [Test]
	    public void readsMapListNestedValuesCorrectly() {

		    BasicDBList list = new BasicDBList();
		    BasicDBObject nested = new BasicDBObject();
		    nested.append("Hello", "World");
		    list.add(nested);
		    DBObject source = new BasicDBObject("mapOfObjects", new BasicDBObject("Foo", list));

		    ClassWithMapProperty result = converter.read(ClassWithMapProperty.class, source);
		    Object firstObjectInFoo = ((List<?>) result.mapOfObjects.get("Foo")).get(0);
		    Assert.That(firstObjectInFoo, is(instanceOf(Map.class)));
		    Assert.That((String) ((Map<?, ?>) firstObjectInFoo).get("Hello"), is(equalTo("World")));
	    }

	    /**
	     * @see DATAMONGO-245
	     */
	    [Test]
	    public void readsMapDoublyNestedValuesCorrectly() {

		    BasicDBObject nested = new BasicDBObject();
		    BasicDBObject doubly = new BasicDBObject();
		    doubly.append("Hello", "World");
		    nested.append("nested", doubly);
		    DBObject source = new BasicDBObject("mapOfObjects", new BasicDBObject("Foo", nested));

		    ClassWithMapProperty result = converter.read(ClassWithMapProperty.class, source);
		    Object foo = result.mapOfObjects.get("Foo");
		    Assert.That(foo, is(instanceOf(Map.class)));
		    Object doublyNestedObject = ((Map<?, ?>) foo).get("nested");
		    Assert.That(doublyNestedObject, is(instanceOf(Map.class)));
		    Assert.That((String) ((Map<?, ?>) doublyNestedObject).get("Hello"), is(equalTo("World")));
	    }

	    /**
	     * @see DATAMONGO-245
	     */
	    [Test]
	    public void readsMapListDoublyNestedValuesCorrectly() {

		    BasicDBList list = new BasicDBList();
		    BasicDBObject nested = new BasicDBObject();
		    BasicDBObject doubly = new BasicDBObject();
		    doubly.append("Hello", "World");
		    nested.append("nested", doubly);
		    list.add(nested);
		    DBObject source = new BasicDBObject("mapOfObjects", new BasicDBObject("Foo", list));

		    ClassWithMapProperty result = converter.read(ClassWithMapProperty.class, source);
		    Object firstObjectInFoo = ((List<?>) result.mapOfObjects.get("Foo")).get(0);
		    Assert.That(firstObjectInFoo, is(instanceOf(Map.class)));
		    Object doublyNestedObject = ((Map<?, ?>) firstObjectInFoo).get("nested");
		    Assert.That(doublyNestedObject, is(instanceOf(Map.class)));
		    Assert.That((String) ((Map<?, ?>) doublyNestedObject).get("Hello"), is(equalTo("World")));
	    }

	    /**
	     * @see DATAMONGO-259
	     */
	    [Test]
	    public void writesListOfMapsCorrectly() {

		    Map<String, Locale> map = Collections.singletonMap("Foo", Locale.ENGLISH);

		    CollectionWrapper wrapper = new CollectionWrapper();
		    wrapper.listOfMaps = new ArrayList<Map<String, Locale>>();
		    wrapper.listOfMaps.add(map);

		    DBObject result = new BasicDBObject();
		    converter.write(wrapper, result);

		    BasicDBList list = (BasicDBList) result.get("listOfMaps");
		    Assert.That(list, is(notNullValue()));
		    Assert.That(list.size(), is(1));

		    DBObject dbObject = (DBObject) list.get(0);
		    Assert.That(dbObject.containsField("Foo"), is(true));
		    Assert.That((String) dbObject.get("Foo"), is(Locale.ENGLISH.toString()));
	    }

	    /**
	     * @see DATAMONGO-259
	     */
	    [Test]
	    public void readsListOfMapsCorrectly() {

		    DBObject map = new BasicDBObject("Foo", "en");

		    BasicDBList list = new BasicDBList();
		    list.add(map);

		    DBObject wrapperSource = new BasicDBObject("listOfMaps", list);

		    CollectionWrapper wrapper = converter.read(CollectionWrapper.class, wrapperSource);

		    Assert.That(wrapper.listOfMaps, is(notNullValue()));
		    Assert.That(wrapper.listOfMaps.size(), is(1));
		    Assert.That(wrapper.listOfMaps.get(0), is(notNullValue()));
		    Assert.That(wrapper.listOfMaps.get(0).get("Foo"), is(Locale.ENGLISH));
	    }

	    /**
	     * @see DATAMONGO-259
	     */
	    [Test]
	    public void writesPlainMapOfCollectionsCorrectly() {

		    Map<String, List<Locale>> map = Collections.singletonMap("Foo", Arrays.asList(Locale.US));
		    DBObject result = new BasicDBObject();
		    converter.write(map, result);

		    Assert.That(result.containsField("Foo"), is(true));
		    Assert.That(result.get("Foo"), is(notNullValue()));
		    Assert.That(result.get("Foo"), is(instanceOf(BasicDBList.class)));

		    BasicDBList list = (BasicDBList) result.get("Foo");

		    Assert.That(list.size(), is(1));
		    Assert.That(list.get(0), is((Object) Locale.US.toString()));
	    }

	    /**
	     * @see DATAMONGO-285
	     */
	    [Test]
	    @SuppressWarnings({ "unchecked", "rawtypes" })
	    public void testSaveMapWithACollectionAsValue() {

		    Map<String, Object> keyValues = new HashMap<String, Object>();
		    keyValues.put("string", "hello");
		    List<String> list = new ArrayList<String>();
		    list.add("ping");
		    list.add("pong");
		    keyValues.put("list", list);

		    DBObject dbObject = new BasicDBObject();
		    converter.write(keyValues, dbObject);

		    Map<String, Object> keyValuesFromMongo = converter.read(Map.class, dbObject);

		    assertEquals(keyValues.size(), keyValuesFromMongo.size());
		    assertEquals(keyValues.get("string"), keyValuesFromMongo.get("string"));
		    assertTrue(List.class.isAssignableFrom(keyValuesFromMongo.get("list").getClass()));
		    List<String> listFromMongo = (List) keyValuesFromMongo.get("list");
		    assertEquals(list.size(), listFromMongo.size());
		    assertEquals(list.get(0), listFromMongo.get(0));
		    assertEquals(list.get(1), listFromMongo.get(1));
	    }

	    /**
	     * @see DATAMONGO-309
	     */
	    [Test]
	    @SuppressWarnings({ "unchecked" })
	    public void writesArraysAsMapValuesCorrectly() {

		    ClassWithMapProperty wrapper = new ClassWithMapProperty();
		    wrapper.mapOfObjects = new HashMap<String, Object>();
		    wrapper.mapOfObjects.put("foo", new String[] { "bar" });

		    DBObject result = new BasicDBObject();
		    converter.write(wrapper, result);

		    Object mapObject = result.get("mapOfObjects");
		    Assert.That(mapObject, is(instanceOf(BasicDBObject.class)));

		    DBObject map = (DBObject) mapObject;
		    Object valueObject = map.get("foo");
		    Assert.That(valueObject, is(instanceOf(BasicDBList.class)));

		    List<Object> list = (List<Object>) valueObject;
		    Assert.That(list.size(), is(1));
		    Assert.That(list, hasItem((Object) "bar"));
	    }

	    /**
	     * @see DATAMONGO-324
	     */
	    [Test]
	    public void writesDbObjectCorrectly() {

		    DBObject dbObject = new BasicDBObject();
		    dbObject.put("foo", "bar");

		    DBObject result = new BasicDBObject();

		    converter.write(dbObject, result);

		    result.removeField(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY);
		    Assert.That(dbObject, is(result));
	    }

	    /**
	     * @see DATAMONGO-324
	     */
	    [Test]
	    public void readsDbObjectCorrectly() {

		    DBObject dbObject = new BasicDBObject();
		    dbObject.put("foo", "bar");

		    DBObject result = converter.read(DBObject.class, dbObject);

		    Assert.That(result, is(dbObject));
	    }

	    /**
	     * @see DATAMONGO-329
	     */
	    [Test]
	    public void writesMapAsGenericFieldCorrectly() {

		    Map<String, A<String>> objectToSave = new HashMap<String, A<String>>();
		    objectToSave.put("test", new A<String>("testValue"));

		    A<Map<String, A<String>>> a = new A<Map<String, A<String>>>(objectToSave);
		    DBObject result = new BasicDBObject();

		    converter.write(a, result);

		    Assert.That((String) result.get(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY), is(A.class.getName()));
		    Assert.That((String) result.get("valueType"), is(HashMap.class.getName()));

		    DBObject object = (DBObject) result.get("value");
		    Assert.That(object, is(notNullValue()));

		    DBObject inner = (DBObject) object.get("test");
		    Assert.That(inner, is(notNullValue()));
		    Assert.That((String) inner.get(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY), is(A.class.getName()));
		    Assert.That((String) inner.get("valueType"), is(String.class.getName()));
		    Assert.That((String) inner.get("value"), is("testValue"));
	    }

	    [Test]
	    public void writesIntIdCorrectly() {

		    ClassWithIntId value = new ClassWithIntId();
		    value.id = 5;

		    DBObject result = new BasicDBObject();
		    converter.write(value, result);

		    Assert.That(result.get("_id"), is((Object) 5));
	    }

	    /**
	     * @see DATAMONGO-368
	     */
	    [Test]
	    @SuppressWarnings("unchecked")
	    public void writesNullValuesForCollection() {

		    CollectionWrapper wrapper = new CollectionWrapper();
		    wrapper.contacts = Arrays.<Contact> asList(new Person(), null);

		    DBObject result = new BasicDBObject();
		    converter.write(wrapper, result);

		    Object contacts = result.get("contacts");
		    Assert.That(contacts, is(instanceOf(Collection.class)));
		    Assert.That(((Collection<?>) contacts).size(), is(2));
		    Assert.That((Collection<Object>) contacts, hasItem(nullValue()));
	    }

	    /**
	     * @see DATAMONGO-379
	     */
	    [Test]
	    public void considersDefaultingExpressionsAtConstructorArguments() {

		    DBObject dbObject = new BasicDBObject("foo", "bar");
		    dbObject.put("foobar", 2.5);

		    DefaultedConstructorArgument result = converter.read(DefaultedConstructorArgument.class, dbObject);
		    Assert.That(result.bar, is(-1));
	    }

	    /**
	     * @see DATAMONGO-379
	     */
	    [Test]
	    public void usesDocumentFieldIfReferencedInAtValue() {

		    DBObject dbObject = new BasicDBObject("foo", "bar");
		    dbObject.put("something", 37);
		    dbObject.put("foobar", 2.5);

		    DefaultedConstructorArgument result = converter.read(DefaultedConstructorArgument.class, dbObject);
		    Assert.That(result.bar, is(37));
	    }

	    /**
	     * @see DATAMONGO-379
	     */
	    [Test](expected = MappingInstantiationException.class)
	    public void rejectsNotFoundConstructorParameterForPrimitiveType() {

		    DBObject dbObject = new BasicDBObject("foo", "bar");

		    converter.read(DefaultedConstructorArgument.class, dbObject);
	    }

	    /**
	     * @see DATAMONGO-358
	     */
	    [Test]
	    public void writesListForObjectPropertyCorrectly() {

		    Attribute attribute = new Attribute();
		    attribute.key = "key";
		    attribute.value = Arrays.asList("1", "2");

		    Item item = new Item();
		    item.attributes = Arrays.asList(attribute);

		    DBObject result = new BasicDBObject();

		    converter.write(item, result);

		    Item read = converter.read(Item.class, result);
		    Assert.That(read.attributes.size(), is(1));
		    Assert.That(read.attributes.get(0).key, is(attribute.key));
		    Assert.That(read.attributes.get(0).value, is(instanceOf(Collection.class)));

		    @SuppressWarnings("unchecked")
		    Collection<String> values = (Collection<String>) read.attributes.get(0).value;

		    Assert.That(values.size(), is(2));
		    Assert.That(values, hasItems("1", "2"));
	    }

	    /**
	     * @see DATAMONGO-380
	     */
	    [Test](expected = MappingException.class)
	    public void rejectsMapWithKeyContainingDotsByDefault() {
		    converter.write(Collections.singletonMap("foo.bar", "foobar"), new BasicDBObject());
	    }

	    /**
	     * @see DATAMONGO-380
	     */
	    [Test]
	    public void escapesDotInMapKeysIfReplacementConfigured() {

		    converter.setMapKeyDotReplacement("~");

		    DBObject dbObject = new BasicDBObject();
		    converter.write(Collections.singletonMap("foo.bar", "foobar"), dbObject);

		    Assert.That((String) dbObject.get("foo~bar"), is("foobar"));
		    Assert.That(dbObject.containsField("foo.bar"), is(false));
	    }

	    /**
	     * @see DATAMONGO-380
	     */
	    [Test]
	    @SuppressWarnings("unchecked")
	    public void unescapesDotInMapKeysIfReplacementConfigured() {

		    converter.setMapKeyDotReplacement("~");

		    DBObject dbObject = new BasicDBObject("foo~bar", "foobar");
		    Map<String, String> result = converter.read(Map.class, dbObject);

		    Assert.That(result.get("foo.bar"), is("foobar"));
		    Assert.That(result.containsKey("foobar"), is(false));
	    }

	    /**
	     * @see DATAMONGO-382
	     */
	    [Test]
	    public void convertsSetToBasicDBList() {

		    Address address = new Address();
		    address.city = "London";
		    address.street = "Foo";

		    Object result = converter.convertToMongoType(Collections.singleton(address));
		    Assert.That(result, is(instanceOf(BasicDBList.class)));

		    Set<?> readResult = converter.read(Set.class, (BasicDBList) result);
		    Assert.That(readResult.size(), is(1));
		    Assert.That(readResult.iterator().next(), is(instanceOf(Map.class)));
	    }

	    /**
	     * @see DATAMONGO-402
	     */
	    [Test]
	    public void readsMemberClassCorrectly() {

		    DBObject dbObject = new BasicDBObject("inner", new BasicDBObject("value", "FOO!"));

		    Outer outer = converter.read(Outer.class, dbObject);
		    Assert.That(outer.inner, is(notNullValue()));
		    Assert.That(outer.inner.value, is("FOO!"));
		    assertSyntheticFieldValueOf(outer.inner, outer);
	    }

	    /**
	     * @see DATAMONGO-347
	     */
	    [Test]
	    public void createsSimpleDBRefCorrectly() {

		    Person person = new Person();
		    person.id = "foo";

		    DBRef dbRef = converter.toDBRef(person, null);
		    Assert.That(dbRef.getId(), is((Object) "foo"));
		    Assert.That(dbRef.getRef(), is("person"));
	    }

	    /**
	     * @see DATAMONGO-347
	     */
	    [Test]
	    public void createsDBRefWithClientSpecCorrectly() {

		    PropertyPath path = PropertyPath.from("person", PersonClient.class);
		    MongoPersistentProperty property = mappingContext.getPersistentPropertyPath(path).getLeafProperty();

		    Person person = new Person();
		    person.id = "foo";

		    DBRef dbRef = converter.toDBRef(person, property);
		    Assert.That(dbRef.getId(), is((Object) "foo"));
		    Assert.That(dbRef.getRef(), is("person"));
	    }

	    /**
	     * @see DATAMONGO-458
	     */
	    [Test]
	    public void readEmptyCollectionIsModifiable() {

		    DBObject dbObject = new BasicDBObject("contactsSet", new BasicDBList());
		    CollectionWrapper wrapper = converter.read(CollectionWrapper.class, dbObject);

		    Assert.That(wrapper.contactsSet, is(notNullValue()));
		    wrapper.contactsSet.add(new Contact() {
		    });
	    }

	    /**
	     * @see DATAMONGO-424
	     */
	    [Test]
	    public void readsPlainDBRefObject() {

		    DBRef dbRef = new DBRef(mock(DB.class), "foo", 2);
		    DBObject dbObject = new BasicDBObject("ref", dbRef);

		    DBRefWrapper result = converter.read(DBRefWrapper.class, dbObject);
		    Assert.That(result.ref, is(dbRef));
	    }

	    /**
	     * @see DATAMONGO-424
	     */
	    [Test]
	    public void readsCollectionOfDBRefs() {

		    DBRef dbRef = new DBRef(mock(DB.class), "foo", 2);
		    BasicDBList refs = new BasicDBList();
		    refs.add(dbRef);

		    DBObject dbObject = new BasicDBObject("refs", refs);

		    DBRefWrapper result = converter.read(DBRefWrapper.class, dbObject);
		    Assert.That(result.refs, hasSize(1));
		    Assert.That(result.refs, hasItem(dbRef));
	    }

	    /**
	     * @see DATAMONGO-424
	     */
	    [Test]
	    public void readsDBRefMap() {

		    DBRef dbRef = mock(DBRef.class);
		    BasicDBObject refMap = new BasicDBObject("foo", dbRef);
		    DBObject dbObject = new BasicDBObject("refMap", refMap);

		    DBRefWrapper result = converter.read(DBRefWrapper.class, dbObject);

		    Assert.That(result.refMap.entrySet(), hasSize(1));
		    Assert.That(result.refMap.values(), hasItem(dbRef));
	    }

	    /**
	     * @see DATAMONGO-424
	     */
	    [Test]
	    public void resolvesDBRefMapValue()
        {
		    DBRef dbRef = mock(DBRef.class);
		    when(dbRef.fetch()).thenReturn(new BasicDBObject());

		    BasicDBObject refMap = new BasicDBObject("foo", dbRef);
		    DBObject dbObject = new BasicDBObject("personMap", refMap);

		    DBRefWrapper result = converter.read(DBRefWrapper.class, dbObject);

		    Matcher isPerson = instanceOf(Person.class);

		    Assert.That(result.personMap.entrySet(), hasSize(1));
		    Assert.That(result.personMap.values(), hasItem(isPerson));
	    }

	    /**
	     * @see DATAMONGO-462
	     */
	    [Test]
	    public void writesURLsAsStringOutOfTheBox()
        {
		    URLWrapper wrapper = new URLWrapper();
		    wrapper.url = new URL("http://springsource.org");
		    DBObject sink = new BasicDBObject();

		    converter.write(wrapper, sink);

		    Assert.That(sink.get("url"), is((Object) "http://springsource.org"));
	    }

	    /**
	     * @see DATAMONGO-462
	     */
	    [Test]
	    public void readsURLFromStringOutOfTheBox()
        {
		    DBObject dbObject = new BasicDBObject("url", "http://springsource.org");
		    URLWrapper result = converter.read(URLWrapper.class, dbObject);
		    Assert.That(result.url, is(new URL("http://springsource.org")));
	    }

	    /**
	     * @see DATAMONGO-485
	     */
	    [Test]
	    public void writesComplexIdCorrectly()
        {
		    ComplexId id = new ComplexId();
		    id.innerId = 4711L;

		    ClassWithComplexId entity = new ClassWithComplexId();
		    entity.complexId = id;

		    DBObject dbObject = new BasicDBObject();
		    converter.write(entity, dbObject);

		    Object idField = dbObject.get("_id");
		    Assert.That(idField, is(notNullValue()));
		    Assert.That(idField, is(instanceOf(DBObject.class)));
		    Assert.That(((DBObject) idField).get("innerId"), is((Object) 4711L));
	    }

	    /**
	     * @see DATAMONGO-485
	     */
	    [Test]
	    public void readsComplexIdCorrectly()
        {
		    DBObject innerId = new BasicDBObject("innerId", 4711L);
		    DBObject entity = new BasicDBObject("_id", innerId);

		    ClassWithComplexId result = converter.read(ClassWithComplexId.class, entity);

		    Assert.That(result.complexId, is(notNullValue()));
		    Assert.That(result.complexId.innerId, is(4711L));
	    }

	    /**
	     * @see DATAMONGO-489
	     */
	    [Test]
	    public void readsArraysAsMapValuesCorrectly()
        {
		    BasicDBList list = new BasicDBList();
		    list.add("Foo");
		    list.add("Bar");

		    DBObject map = new BasicDBObject("key", list);
		    DBObject wrapper = new BasicDBObject("mapOfStrings", map);

		    ClassWithMapProperty result = converter.read(ClassWithMapProperty.class, wrapper);
		    Assert.That(result.mapOfStrings, is(notNullValue()));

		    String[] values = result.mapOfStrings.get("key");
		    Assert.That(values, is(notNullValue()));
		    Assert.That(values, is(arrayWithSize(2)));
	    }

	    /**
	     * @see DATAMONGO-497
	     */
	    [Test]
	    public void ReadsEmptyCollectionIntoConstructorCorrectly()
        {
		    DBObject source = new BasicDBObject("attributes", new BasicDBList());

		    TypWithCollectionConstructor result = converter.read(TypWithCollectionConstructor.class, source);
		    Assert.That(result.attributes, is(notNullValue()));
	    }

	    private void AssertSyntheticFieldValueOf(object target, object expected)
        {
		    for (int i = 0; i < 10; i++)
            {
			    try
                {
				    Assert.That(ReflectionTestUtils.getField(target, "this$" + i), is(expected));
				    return;
			    } 
                catch (IllegalArgumentException e)
                {
				    // Suppress and try next
			    }
		    }

		    Asseet.Fail(String.format("Didn't find synthetic field on %s!", target));
	    }

	    /**
	     * @see DATAMGONGO-508
	     */
	    [Test]
	    public void eagerlyReturnsDBRefObjectIfTargetAlreadyIsOne() {

		    DB db = mock(DB.class);
		    DBRef dbRef = new DBRef(db, "collection", "id");

		    org.springframework.data.mongodb.core.mapping.DBRef annotation = mock(org.springframework.data.mongodb.core.mapping.DBRef.class);

		    Assert.That(converter.createDBRef(dbRef, annotation), is(dbRef));
	    }

	    /**
	     * @see DATAMONGO-523
	     */
	    [Test]
	    public void considersTypeAliasAnnotation() {

		    Aliased aliased = new Aliased();
		    aliased.name = "foo";

		    DBObject result = new BasicDBObject();
		    converter.write(aliased, result);

		    Object type = result.get("_class");
		    Assert.That(type, is(notNullValue()));
		    Assert.That(type.toString(), is("_"));
	    }

	    /**
	     * @see DATAMONGO-533
	     */
	    [Test]
	    public void marshalsThrowableCorrectly() {

		    ThrowableWrapper wrapper = new ThrowableWrapper();
		    wrapper.throwable = new Exception();

		    DBObject dbObject = new BasicDBObject();
		    converter.write(wrapper, dbObject);
	    }

	    class GenericType<T> {
		    T content;
	    }

	    class ClassWithEnumProperty {

		    SampleEnum sampleEnum;
		    List<SampleEnum> enums;
	    }

	    enum SampleEnum
        {
		    FIRST,
		    SECOND
	    }

	    class Address {
		    String street;
		    String city;
	    }

	    interface IContact {

	    }

	    static class Person : IContact
        {
		    [Id]
		    String id;

		    LocalDate birthDate;

		    [Field("foo")]
		    String firstname;

		    Set<Address> addresses;

		    public Person()
            {
		    }

		    [PersistenceConstructor]
		    public Person(Set<Address> addresses)
            {
			    this.addresses = addresses;
		    }
	    }

	    class ClassWithSortedMap
        {
		    SortedMap<String, String> map;
	    }

	    class ClassWithMapProperty
        {
		    Map<Locale, String> map;
		    Map<String, List<String>> mapOfLists;
		    Map<String, Object> mapOfObjects;
		    Map<String, String[]> mapOfStrings;
	    }

	    class ClassWithNestedMaps
        {
		    Map<String, Map<String, Map<String, String>>> nestedMaps;
	    }

	    class BirthDateContainer
        {
		    LocalDate birthDate;
	    }

	    static class BigDecimalContainer {
		    BigDecimal value;
		    Map<String, BigDecimal> map;
		    List<BigDecimal> collection;
	    }

	    static class CollectionWrapper {
		    List<Contact> contacts;
		    List<List<String>> strings;
		    List<Map<String, Locale>> listOfMaps;
		    Set<Contact> contactsSet;
	    }

	    static class LocaleWrapper {
		    Locale locale;
	    }

	    static class ClassWithBigIntegerId {
		    @Id
		    BigInteger id;
	    }

	    static class A<T> {

		    String valueType;
		    T value;

		    public A(T value) {
			    this.valueType = value.getClass().getName();
			    this.value = value;
		    }
	    }

	    static class ClassWithIntId {

		    @Id
		    int id;
	    }

	    static class DefaultedConstructorArgument {

		    String foo;
		    int bar;
		    double foobar;

		    DefaultedConstructorArgument(String foo, @Value("#root.something ?: -1") int bar, double foobar) {
			    this.foo = foo;
			    this.bar = bar;
			    this.foobar = foobar;
		    }
	    }

	    static class Item {
		    List<Attribute> attributes;
	    }

	    static class Attribute {
		    String key;
		    Object value;
	    }

	    static class Outer {

		    class Inner {
			    String value;
		    }

		    Inner inner;
	    }

	    static class PersonClient {

		    @org.springframework.data.mongodb.core.mapping.DBRef
		    Person person;
	    }

	    static class DBRefWrapper {

		    DBRef ref;
		    List<DBRef> refs;
		    Map<String, DBRef> refMap;
		    Map<String, Person> personMap;
	    }

	    static class URLWrapper {
		    URL url;
	    }

	    static class ClassWithComplexId {

		    @Id
		    ComplexId complexId;
	    }

	    static class ComplexId {
		    Long innerId;
	    }

	    static class TypWithCollectionConstructor {

		    List<Attribute> attributes;

		    public TypWithCollectionConstructor(List<Attribute> attributes) {
			    this.attributes = attributes;
		    }
	    }

	    @TypeAlias("_")
	    static class Aliased {
		    String name;
	    }

	    static class ThrowableWrapper {

		    Throwable throwable;
	    }

	    private class LocalDateToDateConverter implements Converter<LocalDate, Date> {

		    public Date convert(LocalDate source) {
			    return source.toDateMidnight().toDate();
		    }
	    }

	    private class DateToLocalDateConverter implements Converter<Date, LocalDate> {

		    public LocalDate convert(Date source) {
			    return new LocalDate(source.getTime());
		    }
	    }
    }
}
