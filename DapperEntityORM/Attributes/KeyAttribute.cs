
namespace DapperEntityORM.Attributes
{
    /// <summary>
    /// Attribute to define the primary key of a table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyAttribute : Attribute
    {

        /// <summary>
        /// The name of the primary key
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the primary key
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The length of the primary key
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The default value of the primary key
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        //// The constructor of the KeyAttribute
        /// </summary>
        /// <param name="name">The name of the primary key</param>
        /// <param name="type">The type of the primary key</param>
        /// <param name="length">The length of the primary key</param>
        /// <param name="defaultValue">The default value of the primary key</param>
        public KeyAttribute(string name, Type type, int length, object defaultValue)
        {
            Name = name;
            Type = type;
            Length = length;
            DefaultValue = defaultValue;
        }
    }
}
