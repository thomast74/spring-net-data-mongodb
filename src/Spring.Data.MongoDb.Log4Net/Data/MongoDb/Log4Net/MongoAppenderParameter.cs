using log4net.Layout;


namespace Spring.Data.MongoDb.Log4Net
{
    /// <summary>
    /// 
    /// </summary>
    public class MongoAppenderParameter
    {
        /// <summary>
        /// Defines the element key name in the mongoDB document
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Defined the pattern to use for the specified element key
        /// </summary>
        public IRawLayout Layout { get; set; }
    }
}
