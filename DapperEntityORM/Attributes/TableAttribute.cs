
namespace DapperEntityORM.Attributes
{
    /// <summary>
    /// TableAttribute class is used to define the table name of the entity class and table number for multi table query or store procedure 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute: Attribute
    {
        /// <summary>
        /// Name of the table
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Table number only for multitables query or store procedure
        /// </summary>
        /// <value>default value is 0</value>
        public int TableNumber { get; set; }

        /// <summary>
        /// TableAttribute constructor
        /// </summary>
        /// <param name="tableName"> Table name </param>
        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }

        /// <summary>
        /// TableAttribute constructor
        /// </summary>
        /// <param name="tableName"> Table name </param>
        /// <param name="tablenumber"> Table number only for multitables query or store procedure </param>
        public TableAttribute(string tableName, int tablenumber = 0)
        {
            TableName = tableName;
            TableNumber = tablenumber;
        }

    }
}
