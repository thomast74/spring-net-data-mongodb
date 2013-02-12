#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingMongoConverter.cs" company="The original author or authors.">
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
using System.Linq;
using System.Text;
using Common.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Spring.Context;
using Spring.Core.TypeConversion;
using Spring.Data.Mapping.Context;
using Spring.Data.Mapping.Model;
using Spring.Data.MongoDb.Core.Mapping;
using Spring.Data.Util;
using Spring.Util;

namespace Spring.Data.MongoDb.Core.Convert
{
    /// <summary>
    /// <see cref="IMongoConverter"/> that uses a <see cref="IMappingContext"/> to do sophisticated mapping of domain objects to
    /// <see cref="BsonDocument"/>.
    /// </summary>
    /// <author>Oliver Gierke</author>
    /// <author>Jon Brisbin</author>
    /// <author>Thomas Trageser</author>
    public class MappingMongoConverter : AbstractMongoConverter, IApplicationContext
    {
	    private static readonly ILog Log = LogManager.GetLogger<MappingMongoConverter>();

	    protected readonly IMappingContext mappingContext;
	    protected readonly SpelExpressionParser spelExpressionParser = new SpelExpressionParser();
	    protected readonly IMongoDbFactory mongoDbFactory;
	    protected readonly QueryMapper idMapper;
	    protected IApplicationContext applicationContext;
	    protected bool useFieldAccessOnly = true;
	    protected MongoTypeMapper typeMapper;
	    protected string mapKeyDotReplacement = null;

        /// <summary>
        /// Creates a new {@link MappingMongoConverter} given the new {@link MongoDbFactory} and {@link MappingContext}.
        /// </summary>
        /// <param name="mongoDbFactory">must not be <code>null</code></param>
        /// <param name="mappingContext">must not be <code>null</code></param>
	    public MappingMongoConverter(IMongoDbFactory mongoDbFactory, IMappingContext mappingContext)
        {
		    AssertUtils.ArgumentNotNull(mongoDbFactory, "mongoDbFactory");
		    AssertUtils.ArgumentNotNull(mappingContext, "mappingContext");

		    this.mongoDbFactory = mongoDbFactory;
		    this.mappingContext = mappingContext;
		    this.typeMapper = new DefaultMongoTypeMapper(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY, mappingContext);
		    this.idMapper = new QueryMapper(this);
	    }

        /// <summary>
	    /// Configures the {@link MongoTypeMapper} to be used to add type information to <see cref="BsonDocument"/>s
	    /// created by the converter and how to lookup type information from <see cref="BsonDocument"/>s when reading
	    /// them. Uses a {@link DefaultMongoTypeMapper} by default. Setting this to <code>null</code> will reset the
	    /// {@link TypeMapper} to the default one.
        /// </summary>
	    public MongoTypeMapper TypeMapper
        {
            get { return typeMapper; }
            set
            {
                typeMapper = value == null
                                 ? new DefaultMongoTypeMapper(DefaultMongoTypeMapper.DEFAULT_TYPE_KEY, mappingContext)
                                 : value;
            }
        }

        /// <summary>
	    /// Configure the characters dots potentially contained in a {@link Map} shall be replaced with. By default we
	    /// don't do any translation but rather reject a {@link Map} with keys containing dots causing the conversion 
	    /// for the entire object to fail. If further customization of the translation is needed, have a look at
	    /// {@link #potentiallyEscapeMapKey(String)} as well as {@link #potentiallyUnescapeMapKey(String)}.
        /// </summary>
	    public string MapKeyDotReplacement
        {
            get { return mapKeyDotReplacement; }
            set { mapKeyDotReplacement = value; }
        }

	    public override IMappingContext MappingContext
        {
	        get { return mappingContext; }
        }

        /// <summary>
	    /// Configures whether to use field access only for entity mapping. Setting this to true will force the
	    /// <see cref="IMongoConverter"/> to not go through getters or setters even if they are present for getting
	    /// and setting property values.
        /// </summary>
	    public bool UseFieldAccessOnly
        {
	        set { useFieldAccessOnly = value; }
        }

	    public IApplicationContext ApplicationContext
        {
	        set
	        {
	            applicationContext = value;
	        }
        }

	    public T Read<T>(Type clazz, BsonDocument dbo) where T : class
        {
		    return Read(ClassTypeInformation.From(clazz), dbo);
	    }

	    protected T Read<T>(ITypeInformation type, BsonDocument dbo) where T : class
        {
		    return Read<T>(type, dbo, null);
	    }

	    protected T Read<T>(ITypeInformation type, BsonDocument dbo, object parent) where T : class
        {
		    if (null == dbo)
			    return null;

		    ITypeInformation typeToUse = typeMapper.ReadType(dbo, type);
		    Type rawType = typeToUse.Type;

		    if (_conversions.HasCustomReadTarget(dbo.GetType(), rawType))
		    {
		        return TypeConversionUtils.ConvertValueIfNecessary(rawType, dbo, "dbo") as T;
		    }

		    if (typeof(BsonDocument).IsAssignableFrom(rawType))
		    {
		        return dbo as T;
		    }

		    if (typeToUse.IsCollectionLike && dbo is BasicDBList)
            {
			    return (T) ReadCollectionOrArray(typeToUse, (BasicDBList) dbo, parent);
		    }

		    if (typeToUse.IsDictionary) 
            {
			    return (T) ReadDictionary(typeToUse, dbo, parent);
		    }

		    // Retrieve persistent entity info
	        IMongoPersistentEntity persistentEntity = (IMongoPersistentEntity) mappingContext
	                                                                               .GetPersistentEntity(typeToUse);
		    if (persistentEntity == null)
            {
			    throw new MappingException("No mapping metadata found for " + rawType.Name);
		    }

		    return Read<T>(persistentEntity, dbo, parent);
	    }

	    private ParameterValueProvider<MongoPersistentProperty> GetParameterProvider(IMongoPersistentEntity entity,
			    BsonDocument source, DefaultSpELExpressionEvaluator evaluator, object parent)
        {
		    MongoDbPropertyValueProvider provider = new MongoDbPropertyValueProvider(source, evaluator, parent);
		    PersistentEntityParameterValueProvider<MongoPersistentProperty> parameterProvider = new PersistentEntityParameterValueProvider<MongoPersistentProperty>(
				    entity, provider, parent);

		    return new ConverterAwareSpELExpressionParameterValueProvider(evaluator, conversionService, parameterProvider,
				    parent);
	    }

	    private T Read<T>(IMongoPersistentEntity entity, BsonDocument dbo, object parent) where T : class
        {
		    DefaultSpELExpressionEvaluator evaluator = new DefaultSpELExpressionEvaluator(dbo, spELContext);

		    ParameterValueProvider<MongoPersistentProperty> provider = getParameterProvider(entity, dbo, evaluator, parent);
		    EntityInstantiator instantiator = instantiators.getInstantiatorFor(entity);
		    S instance = instantiator.createInstance(entity, provider);

		    final BeanWrapper<MongoPersistentEntity<S>, S> wrapper = BeanWrapper.create(instance, conversionService);
		    final S result = wrapper.getBean();

		    // Set properties not already set in the constructor
		    entity.doWithProperties(new PropertyHandler<MongoPersistentProperty>() {
			    public void doWithPersistentProperty(MongoPersistentProperty prop) {

				    boolean isConstructorProperty = entity.isConstructorArgument(prop);
				    boolean hasValueForProperty = dbo.containsField(prop.getFieldName());

				    if (!hasValueForProperty || isConstructorProperty) {
					    return;
				    }

				    Object obj = getValueInternal(prop, dbo, evaluator, result);
				    wrapper.setProperty(prop, obj, useFieldAccessOnly);
			    }
		    });

		    // Handle associations
		    entity.doWithAssociations(new AssociationHandler<MongoPersistentProperty>() {
			    public void doWithAssociation(Association<MongoPersistentProperty> association) {
				    MongoPersistentProperty inverseProp = association.getInverse();
				    Object obj = getValueInternal(inverseProp, dbo, evaluator, result);

				    wrapper.setProperty(inverseProp, obj);

			    }
		    });

		    return result;
	    }

	    /* 
	     * (non-Javadoc)
	     * @see org.springframework.data.mongodb.core.convert.MongoWriter#toDBRef(java.lang.Object, org.springframework.data.mongodb.core.mapping.MongoPersistentProperty)
	     */
	    public DBRef ToDBRef(object obj, IMongoPersistentProperty referingProperty)
        {
		    org.springframework.data.mongodb.core.mapping.DBRef annotation = null;

		    if (referingProperty != null) {
			    annotation = referingProperty.getDBRef();
			    Assert.isTrue(annotation != null, "The referenced property has to be mapped with @DBRef!");
		    }

		    return createDBRef(obj, annotation);
	    }

	    /**
	     * Root entry method into write conversion. Adds a type discriminator to the {@link DBObject}. Shouldn't be called for
	     * nested conversions.
	     * 
	     * @see org.springframework.data.mongodb.core.core.convert.MongoWriter#write(java.lang.Object, com.mongodb.DBObject)
	     */
	    public void write(object obj, DBObject dbo)
        {
		    if (null == obj)
			    return;

		    bool handledByCustomConverter = _conversions.GetCustomWriteTarget(obj.GetType(), typeof(DBObject)) != null;
		    ITypeInformation type = ClassTypeInformation.From(obj.GetType());

		    if (!handledByCustomConverter && !(dbo is BasicDBList))
            {
			    typeMapper.WriteType(type, dbo);
		    }

		    WriteInternal(obj, dbo, type);
	    }

        /// <summary>
        /// Internal write conversion method which should be used for nested invocations.
        /// </summary>
	    protected void WriteInternal(Object obj, DBObject dbo, ITypeInformation typeHint)
        {
		    if (null == obj)
			    return;

		    Type customTarget = _conversions.GetCustomWriteTarget(obj.getClass(), DBObject.class);

		    if (customTarget != null) {
			    DBObject result = conversionService.convert(obj, DBObject.class);
			    dbo.putAll(result);
			    return;
		    }

		    if (Map.class.isAssignableFrom(obj.getClass())) {
			    writeMapInternal((Map<Object, Object>) obj, dbo, ClassTypeInformation.MAP);
			    return;
		    }

		    if (Collection.class.isAssignableFrom(obj.getClass())) {
			    writeCollectionInternal((Collection<?>) obj, ClassTypeInformation.LIST, (BasicDBList) dbo);
			    return;
		    }

		    IMongoPersistentEntity entity = mappingContext.getPersistentEntity(obj.getClass());
		    writeInternal(obj, dbo, entity);
		    addCustomTypeKeyIfNecessary(typeHint, obj, dbo);
	    }

	    protected void writeInternal(Object obj, final DBObject dbo, IMongoPersistentEntity entity) {

		    if (obj == null) {
			    return;
		    }

		    if (null == entity) {
			    throw new MappingException("No mapping metadata found for entity of type " + obj.getClass().getName());
		    }

		    final BeanWrapper<MongoPersistentEntity<Object>, Object> wrapper = BeanWrapper.create(obj, conversionService);
		    final MongoPersistentProperty idProperty = entity.getIdProperty();

		    if (!dbo.containsField("_id") && null != idProperty) {

			    boolean fieldAccessOnly = idProperty.usePropertyAccess() ? false : useFieldAccessOnly;

			    try {
				    Object id = wrapper.getProperty(idProperty, Object.class, fieldAccessOnly);
				    dbo.put("_id", idMapper.convertId(id));
			    } catch (ConversionException ignored) {
			    }
		    }

		    // Write the properties
		    entity.doWithProperties(new PropertyHandler<MongoPersistentProperty>() {
			    public void doWithPersistentProperty(MongoPersistentProperty prop) {

				    if (prop.equals(idProperty)) {
					    return;
				    }

				    boolean fieldAccessOnly = prop.usePropertyAccess() ? false : useFieldAccessOnly;

				    Object propertyObj = wrapper.getProperty(prop, prop.getType(), fieldAccessOnly);

				    if (null != propertyObj) {
					    if (!conversions.isSimpleType(propertyObj.getClass())) {
						    writePropertyInternal(propertyObj, dbo, prop);
					    } else {
						    writeSimpleInternal(propertyObj, dbo, prop.getFieldName());
					    }
				    }
			    }
		    });

		    entity.doWithAssociations(new AssociationHandler<MongoPersistentProperty>() {
			    public void doWithAssociation(Association<MongoPersistentProperty> association) {
				    MongoPersistentProperty inverseProp = association.getInverse();
				    Type type = inverseProp.getType();
				    Object propertyObj = wrapper.getProperty(inverseProp, type, useFieldAccessOnly);
				    if (null != propertyObj) {
					    writePropertyInternal(propertyObj, dbo, inverseProp);
				    }
			    }
		    });
	    }

	    protected void WritePropertyInternal(object obj, DBObject dbo, IMongoPersistentProperty prop)
        {
		    if (obj == null)
			    return;

		    String name = prop.getFieldName();
		    ITypeInformation valueType = ClassTypeInformation.from(obj.getClass());
		    ITypeInformation type = prop.getTypeInformation();

		    if (valueType.isCollectionLike()) {
			    DBObject collectionInternal = createCollection(asCollection(obj), prop);
			    dbo.put(name, collectionInternal);
			    return;
		    }

		    if (valueType.isMap()) {
			    BasicDBObject mapDbObj = new BasicDBObject();
			    writeMapInternal((Map<Object, Object>) obj, mapDbObj, type);
			    dbo.put(name, mapDbObj);
			    return;
		    }

		    if (prop.isDbReference()) {
			    DBRef dbRefObj = createDBRef(obj, prop.getDBRef());
			    if (null != dbRefObj) {
				    dbo.put(name, dbRefObj);
				    return;
			    }
		    }

		    // Lookup potential custom target type
		    Type basicTargetType = conversions.getCustomWriteTarget(obj.getClass(), null);

		    if (basicTargetType != null) {
			    dbo.put(name, conversionService.convert(obj, basicTargetType));
			    return;
		    }

		    BasicDBObject propDbObj = new BasicDBObject();
		    addCustomTypeKeyIfNecessary(type, obj, propDbObj);

		    IMongoPersistentEntity entity = isSubtype(prop.getType(), obj.getClass()) ? mappingContext
				    .getPersistentEntity(obj.getClass()) : mappingContext.getPersistentEntity(type);

		    writeInternal(obj, propDbObj, entity);
		    dbo.put(name, propDbObj);
	    }

	    private boolean isSubtype(Type left, Type right) {
		    return left.isAssignableFrom(right) && !left.equals(right);
	    }

	    /**
	     * Returns given object as {@link Collection}. Will return the {@link Collection} as is if the source is a
	     * {@link Collection} already, will convert an array into a {@link Collection} or simply create a single element
	     * collection for everything else.
	     * 
	     * @param source
	     * @return
	     */
	    private static Collection<?> asCollection(Object source) {

		    if (source instanceof Collection) {
			    return (Collection<?>) source;
		    }

		    return source.getClass().isArray() ? CollectionUtils.arrayToList(source) : Collections.singleton(source);
	    }

	    /**
	     * Writes the given {@link Collection} using the given {@link MongoPersistentProperty} information.
	     * 
	     * @param collection must not be {@literal null}.
	     * @param property must not be {@literal null}.
	     * @return
	     */
	    protected DBObject createCollection(Collection<?> collection, MongoPersistentProperty property) {

		    if (!property.isDbReference()) {
			    return writeCollectionInternal(collection, property.getTypeInformation(), new BasicDBList());
		    }

		    BasicDBList dbList = new BasicDBList();

		    for (Object element : collection) {

			    if (element == null) {
				    continue;
			    }

			    DBRef dbRef = createDBRef(element, property.getDBRef());
			    dbList.add(dbRef);
		    }

		    return dbList;
	    }

	    /**
	     * Populates the given {@link BasicDBList} with values from the given {@link Collection}.
	     * 
	     * @param source the collection to create a {@link BasicDBList} for, must not be {@literal null}.
	     * @param type the {@link TypeInformation} to consider or {@literal null} if unknown.
	     * @param sink the {@link BasicDBList} to write to.
	     * @return
	     */
	    private BasicDBList WriteCollectionInternal(Collection<?> source, ITypeInformation type, BasicDBList sink)
        {
		    ITypeInformation componentType = type == null ? null : type.ComponentType;

		    for (Object element : source)
            {
			    Type elementType = element == null ? null : element.getClass();

			    if (elementType == null || conversions.isSimpleType(elementType)) {
				    sink.add(getPotentiallyConvertedSimpleWrite(element));
			    } else if (element instanceof Collection || elementType.isArray()) {
				    sink.add(writeCollectionInternal(asCollection(element), componentType, new BasicDBList()));
			    } else {
				    BasicDBObject propDbObj = new BasicDBObject();
				    writeInternal(element, propDbObj, componentType);
				    sink.add(propDbObj);
			    }
		    }

		    return sink;
	    }

	    /**
	     * Writes the given {@link Map} to the given {@link DBObject} considering the given {@link TypeInformation}.
	     * 
	     * @param obj must not be {@literal null}.
	     * @param dbo must not be {@literal null}.
	     * @param propertyType must not be {@literal null}.
	     * @return
	     */
	    protected DBObject WriteMapInternal(Map<Object, Object> obj, DBObject dbo, TypeInformation<?> propertyType) {

		    for (Map.Entry<Object, Object> entry : obj.entrySet()) {
			    Object key = entry.getKey();
			    Object val = entry.getValue();
			    if (conversions.isSimpleType(key.getClass())) {
				    // Don't use conversion service here as removal of ObjectToString converter results in some primitive types not
				    // being convertable
				    String simpleKey = potentiallyEscapeMapKey(key.toString());
				    if (val == null || conversions.isSimpleType(val.getClass())) {
					    writeSimpleInternal(val, dbo, simpleKey);
				    } else if (val instanceof Collection || val.getClass().isArray()) {
					    dbo.put(simpleKey,
							    writeCollectionInternal(asCollection(val), propertyType.getMapValueType(), new BasicDBList()));
				    } else {
					    DBObject newDbo = new BasicDBObject();
					    ITypeInformation valueTypeInfo = propertyType.isMap() ? propertyType.getMapValueType()
							    : ClassTypeInformation.OBJECT;
					    writeInternal(val, newDbo, valueTypeInfo);
					    dbo.put(simpleKey, newDbo);
				    }
			    } else {
				    throw new MappingException("Cannot use a complex object as a key value.");
			    }
		    }

		    return dbo;
	    }

        /// <summary>
        /// Potentially replaces dots in the given map key with the configured map key replacement if configured or aborts
        /// conversion if none is configured.
        /// </summary> 
        /// <param name="source"></param>
	    protected String PotentiallyEscapeDictionaryKey(string source)
        {
		    if (!source.Contains("."))
            {
			    return source;
		    }

		    if (mapKeyDotReplacement == null)
            {
			    throw new MappingException(string.Format("Dictionary key '{0}' contains dots but no replacement was configured! Make "
					    + "sure map keys don't contain dots in the first place or configure an appropriate replacement!", source));
		    }

		    return source.Replace(".", mapKeyDotReplacement);
	    }

        /// <summary>
        /// Translates the map key replacements in the given key just read with a dot in case a map key replacement has been
        /// configured.
        /// </summary>
        /// <param name="source"></param>
	    protected string PotentiallyUnescapeDictionaryKey(string source)
        {
		    return mapKeyDotReplacement == null ? source : source.Replace(mapKeyDotReplacement, ".");
	    }

	    /**
	     * Adds custom type information to the given {@link DBObject} if necessary. That is if the value is not the same as
	     * the one given. This is usually the case if you store a subtype of the actual declared type of the property.
	     * 
	     * @param type
	     * @param value must not be {@literal null}.
	     * @param dbObject must not be {@literal null}.
	     */
	    protected void AddCustomTypeKeyIfNecessary(ITypeInformation type, object value, DBObject dbObject)
        {
		    ITypeInformation actualType = type != null ? type.ActualType : type;
		    Type reference = actualType == null ? typeof(object) : actualType.Type;

		    bool notTheSameClass = value.GetType() != (reference);
		    if (notTheSameClass) {
			    typeMapper.WriteType(value.GetType(), dbObject);
		    }
	    }

        /// <summary>
        /// Writes the given simple value to the given <see cref="BsonDocument"/>. Will store enum names for enum values.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dbObject"></param>
        /// <param name="key"></param>
	    private void WriteSimpleInternal(object value, BsonDocument dbObject, string key)
        {
		    dbObject.Add(key, GetPotentiallyConvertedSimpleWrite(value));
	    }

        /// <summary>
        /// Checks whether we have a custom conversion registered for the given value into an arbitrary simple Mongo type.
        /// Returns the converted value if so. If not, we perform special enum handling or simply return the value as is.
        /// </summary>
        /// <param name="value"></param>
	    private object GetPotentiallyConvertedSimpleWrite(object value)
        {
		    if (value == null)
			    return null;

		    Type customTarget = _conversions.GetCustomWriteTarget(value.GetType(), null);

		    if (customTarget != null)
            {
			    return TypeConversionUtils.ConvertValueIfNecessary(customTarget, value, "value");
		    } 

            return value.GetType().IsEnum ? value.ToString() : value;
	    }

        /// <summary>
        /// Checks whether we have a custom conversion for the given simple object. Converts the given value if so, applies
        /// {@link Enum} handling or returns the value as is.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
	    private Object GetPotentiallyConvertedSimpleRead(object value, Type target)
        {
		    if (value == null || target == null) {
			    return value;
		    }

		    if (_conversions.HasCustomReadTarget(value.GetType(), target))
		    {
		        return TypeConversionUtils.ConvertValueIfNecessary(target, value, "value");
		    }

		    if (target.IsEnum)
            {
			    return Enum.Parse(target, value.ToString());
		    }

            return target.IsAssignableFrom(value.GetType())
                       ? value
                       : TypeConversionUtils.ConvertValueIfNecessary(target, value, "value");
        }

	    protected DBRef createDBRef(Object target, org.springframework.data.mongodb.core.mapping.DBRef dbref) {

		    Assert.notNull(target);

		    if (target instanceof DBRef) {
			    return (DBRef) target;
		    }

		    IMongoPersistentEntity targetEntity = mappingContext.getPersistentEntity(target.getClass());

		    if (null == targetEntity) {
			    throw new MappingException("No mapping metadata found for " + target.getClass());
		    }

		    MongoPersistentProperty idProperty = targetEntity.getIdProperty();

		    if (idProperty == null) {
			    throw new MappingException("No id property found on class " + targetEntity.getType());
		    }

		    BeanWrapper<MongoPersistentEntity<Object>, Object> wrapper = BeanWrapper.create(target, conversionService);
		    Object id = wrapper.getProperty(idProperty, Object.class, useFieldAccessOnly);

		    if (null == id) {
			    throw new MappingException("Cannot create a reference to an object with a NULL id.");
		    }

		    DB db = mongoDbFactory.getDb();
		    db = dbref != null && StringUtils.hasText(dbref.db()) ? mongoDbFactory.getDb(dbref.db()) : db;

		    return new DBRef(db, targetEntity.getCollection(), idMapper.convertId(id));
	    }

	    protected Object getValueInternal(MongoPersistentProperty prop, DBObject dbo, SpELExpressionEvaluator eval,
			    Object parent) {

		    MongoDbPropertyValueProvider provider = new MongoDbPropertyValueProvider(dbo, spELContext, parent);
		    return provider.getPropertyValue(prop);
	    }

	    /**
	     * Reads the given {@link BasicDBList} into a collection of the given {@link TypeInformation}.
	     * 
	     * @param targetType must not be {@literal null}.
	     * @param sourceValue must not be {@literal null}.
	     * @return the converted {@link Collection} or array, will never be {@literal null}.
	     */
	    @SuppressWarnings("unchecked")
	    private Object readCollectionOrArray(ITypeInformation targetType, BasicDBList sourceValue, Object parent) {

		    Assert.notNull(targetType);

		    Type collectionType = targetType.getType();

		    if (sourceValue.isEmpty()) {
			    return getPotentiallyConvertedSimpleRead(new HashSet<Object>(), collectionType);
		    }

		    collectionType = Collection.class.isAssignableFrom(collectionType) ? collectionType : List.class;

		    Collection<Object> items = targetType.getType().isArray() ? new ArrayList<Object>() : CollectionFactory
				    .createCollection(collectionType, sourceValue.size());
		    ITypeInformation componentType = targetType.getComponentType();
		    Type rawComponentType = componentType == null ? null : componentType.getType();

		    for (int i = 0; i < sourceValue.size(); i++) {

			    Object dbObjItem = sourceValue.get(i);

			    if (dbObjItem instanceof DBRef) {
				    items.add(DBRef.class.equals(rawComponentType) ? dbObjItem : read(componentType, ((DBRef) dbObjItem).fetch(),
						    parent));
			    } else if (dbObjItem instanceof DBObject) {
				    items.add(read(componentType, (DBObject) dbObjItem, parent));
			    } else {
				    items.add(getPotentiallyConvertedSimpleRead(dbObjItem, rawComponentType));
			    }
		    }

		    return getPotentiallyConvertedSimpleRead(items, targetType.getType());
	    }

	    /**
	     * Reads the given {@link DBObject} into a {@link Map}. will recursively resolve nested {@link Map}s as well.
	     * 
	     * @param type the {@link Map} {@link TypeInformation} to be used to unmarshall this {@link DBObject}.
	     * @param dbObject
	     * @return
	     */
	    @SuppressWarnings("unchecked")
	    protected Map<Object, Object> readMap(ITypeInformation type, DBObject dbObject, Object parent) {

		    Assert.notNull(dbObject);

		    Type mapType = typeMapper.readType(dbObject, type).getType();
		    Map<Object, Object> map = CollectionFactory.createMap(mapType, dbObject.keySet().size());
		    Map<String, Object> sourceMap = dbObject.toMap();

		    for (Entry<String, Object> entry : sourceMap.entrySet()) {
			    if (typeMapper.isTypeKey(entry.getKey())) {
				    continue;
			    }

			    Object key = potentiallyUnescapeMapKey(entry.getKey());

			    ITypeInformation keyTypeInformation = type.getComponentType();
			    if (keyTypeInformation != null) {
				    Type keyType = keyTypeInformation.getType();
				    key = conversionService.convert(key, keyType);
			    }

			    Object value = entry.getValue();
			    ITypeInformation valueType = type.getMapValueType();
			    Type rawValueType = valueType == null ? null : valueType.getType();

			    if (value instanceof DBObject) {
				    map.put(key, read(valueType, (DBObject) value, parent));
			    } else if (value instanceof DBRef) {
				    map.put(key, DBRef.class.equals(rawValueType) ? value : read(valueType, ((DBRef) value).fetch()));
			    } else {
				    Type valueClass = valueType == null ? null : valueType.getType();
				    map.put(key, getPotentiallyConvertedSimpleRead(value, valueClass));
			    }
		    }

		    return map;
	    }

	    protected <T> List<?> unwrapList(BasicDBList dbList, TypeInformation<T> targetType) {
		    List<Object> rootList = new ArrayList<Object>();
		    for (int i = 0; i < dbList.size(); i++) {
			    Object obj = dbList.get(i);
			    if (obj instanceof BasicDBList) {
				    rootList.add(unwrapList((BasicDBList) obj, targetType.getComponentType()));
			    } else if (obj instanceof DBObject) {
				    rootList.add(read(targetType, (DBObject) obj));
			    } else {
				    rootList.add(obj);
			    }
		    }
		    return rootList;
	    }

	    @SuppressWarnings("unchecked")
	    public Object convertToMongoType(Object obj) {

		    if (obj == null) {
			    return null;
		    }

		    Type target = conversions.getCustomWriteTarget(obj.getClass());
		    if (target != null) {
			    return conversionService.convert(obj, target);
		    }

		    if (null != obj && conversions.isSimpleType(obj.getClass())) {
			    // Doesn't need conversion
			    return getPotentiallyConvertedSimpleWrite(obj);
		    }

		    if (obj instanceof BasicDBList) {
			    return maybeConvertList((BasicDBList) obj);
		    }

		    if (obj instanceof DBObject) {
			    DBObject newValueDbo = new BasicDBObject();
			    for (String vk : ((DBObject) obj).keySet()) {
				    Object o = ((DBObject) obj).get(vk);
				    newValueDbo.put(vk, convertToMongoType(o));
			    }
			    return newValueDbo;
		    }

		    if (obj instanceof Map) {
			    DBObject result = new BasicDBObject();
			    for (Map.Entry<Object, Object> entry : ((Map<Object, Object>) obj).entrySet()) {
				    result.put(entry.getKey().toString(), convertToMongoType(entry.getValue()));
			    }
			    return result;
		    }

		    if (obj.getClass().isArray()) {
			    return maybeConvertList(Arrays.asList((Object[]) obj));
		    }

		    if (obj instanceof Collection) {
			    return maybeConvertList((Collection<?>) obj);
		    }

		    DBObject newDbo = new BasicDBObject();
		    this.write(obj, newDbo);
		    return removeTypeInfoRecursively(newDbo);
	    }

	    public BasicDBList maybeConvertList(Iterable<?> source) {
		    BasicDBList newDbl = new BasicDBList();
		    for (Object element : source) {
			    newDbl.add(convertToMongoType(element));
		    }
		    return newDbl;
	    }

	    /**
	     * Removes the type information from the conversion result.
	     * 
	     * @param object
	     * @return
	     */
	    private Object removeTypeInfoRecursively(Object object) {

		    if (!(object instanceof DBObject)) {
			    return object;
		    }

		    DBObject dbObject = (DBObject) object;
		    String keyToRemove = null;
		    for (String key : dbObject.keySet()) {

			    if (typeMapper.isTypeKey(key)) {
				    keyToRemove = key;
			    }

			    Object value = dbObject.get(key);
			    if (value instanceof BasicDBList) {
				    for (Object element : (BasicDBList) value) {
					    removeTypeInfoRecursively(element);
				    }
			    } else {
				    removeTypeInfoRecursively(value);
			    }
		    }

		    if (keyToRemove != null) {
			    dbObject.removeField(keyToRemove);
		    }

		    return dbObject;
	    }

	    private class MongoDbPropertyValueProvider implements PropertyValueProvider<MongoPersistentProperty> {

		    private final DBObject source;
		    private final SpELExpressionEvaluator evaluator;
		    private final Object parent;

		    public MongoDbPropertyValueProvider(DBObject source, SpELContext factory, Object parent) {
			    this(source, new DefaultSpELExpressionEvaluator(source, factory), parent);
		    }

		    public MongoDbPropertyValueProvider(DBObject source, DefaultSpELExpressionEvaluator evaluator, Object parent) {

			    Assert.notNull(source);
			    Assert.notNull(evaluator);

			    this.source = source;
			    this.evaluator = evaluator;
			    this.parent = parent;
		    }

		    /* 
		     * (non-Javadoc)
		     * @see org.springframework.data.convert.PropertyValueProvider#getPropertyValue(org.springframework.data.mapping.PersistentProperty)
		     */
		    public <T> T getPropertyValue(MongoPersistentProperty property) {

			    String expression = property.getSpelExpression();
			    Object value = expression != null ? evaluator.evaluate(expression) : source.get(property.getFieldName());

			    if (value == null) {
				    return null;
			    }

			    return readValue(value, property.getTypeInformation(), parent);
		    }
	    }

	    /**
	     * Extension of {@link SpELExpressionParameterValueProvider} to recursively trigger value conversion on the raw
	     * resolved SpEL value.
	     * 
	     * @author Oliver Gierke
	     */
	    private class ConverterAwareSpELExpressionParameterValueProvider extends
			    SpELExpressionParameterValueProvider<MongoPersistentProperty> {

		    private final Object parent;

		    /**
		     * Creates a new {@link ConverterAwareSpELExpressionParameterValueProvider}.
		     * 
		     * @param evaluator must not be {@literal null}.
		     * @param conversionService must not be {@literal null}.
		     * @param delegate must not be {@literal null}.
		     */
		    public ConverterAwareSpELExpressionParameterValueProvider(SpELExpressionEvaluator evaluator,
				    ConversionService conversionService, ParameterValueProvider<MongoPersistentProperty> delegate, Object parent) {

			    super(evaluator, conversionService, delegate);
			    this.parent = parent;
		    }

		    /* 
		     * (non-Javadoc)
		     * @see org.springframework.data.mapping.model.SpELExpressionParameterValueProvider#potentiallyConvertSpelValue(java.lang.Object, org.springframework.data.mapping.PreferredConstructor.Parameter)
		     */
		    @Override
		    protected <T> T potentiallyConvertSpelValue(Object object, Parameter<T, MongoPersistentProperty> parameter) {
			    return readValue(object, parameter.getType(), parent);
		    }
	    }

	    private T ReadValue<T>(object value, ITypeInformation type, object parent)
        {
		    Type rawType = type.Type;

		    if (_conversions.HasCustomReadTarget(value.GetType(), rawType))
		    {
		        return (T) conversionService.convert(value, rawType);
		    } 
            else if (value is MongoDBRef)
            {
			    return (T) (rawType == typeof(MongoDBRef)) ? value : Read(type, ((MongoDBRef) value).fetch(), parent);
		    } 
            else if (value is BsonArray) 
            {
			    return (T) ReadCollectionOrArray(type, (BsonArray) value, parent);
		    } 
            else if (value is DBObject)
            {
			    return (T) Read<T>(type, (DBObject) value, parent);
		    } 
            else
            {
			    return (T) GetPotentiallyConvertedSimpleRead(value, rawType);
		    }
	    }
    }
}
