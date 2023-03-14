
namespace DapperEntityORM.Attributes
{
    /// <summary>
    /// RelationAttribute class to define the relation between two tables
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RelationAttribute: Attribute
    {
        /// <summary>
        /// RelationAttribute Constructor
        /// </summary>
        public RelationAttribute() { }

        /// <summary>
        /// RelationAttribute Constructor
        /// </summary>
        /// <param name="relationtable"></param>
        /// <param name="foreignKey"></param>
        /// <param name="tableNumber"></param>
        public RelationAttribute( string foreignKey, string relationtable="", int tableNumber = 0)
        {
            this.RelationTable = relationtable;
            this.ForeignKey = foreignKey;
            this.TableNumber = tableNumber;
        }

        /// <summary>
        /// Relation table name
        /// </summary>
        public string RelationTable { get; set; }

        /// <summary>
        /// Foreign key name
        /// </summary>
        public string ForeignKey { get; set; }

        /// <summary>
        /// Table number in the relation table only for multiple tables or store procedure
        /// </summary>
        public int TableNumber { get; set; }

    }
}
