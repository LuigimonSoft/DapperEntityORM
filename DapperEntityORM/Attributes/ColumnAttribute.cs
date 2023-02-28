using System;

namespace DapperEntityORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute: Attribute
    {
        private int _maxLength = -1;
        private int _minLength = -1;

        /// <summary>
        ///  Max length of the column
        /// </summary>
        /// <value>Default value -1 (no limit)</value>
        public int MaxLength{
            get { return _maxLength; }
            set { _maxLength = value; }
        }

        /// <summary>
        /// Min length of the column
        /// </summary>
        /// <value>Default value -1 (No minimum)</value>
        public int MinLength{
            get { return _minLength; }
            set { _minLength = value; }
        }

        /// <summary>
        /// Column name of the property
        /// </summary>
        public string ColumnName { get; set; }    

        /// <summary>
        /// Regular expression pattern to validate the column
        /// </summary>
        public string RegExPattern { get; set; }

        /// <summary>
        /// Allow empty string
        /// </summary>
        public bool AllowEmpty { set; get; }

        /// <summary>
        /// Null custom error message
        /// </summary>
        public string ErrorNullMessage { get; set; }

        /// <summary>
        /// Empty custom error message
        /// </summary>
        public string ErrorEmptyMessage { get; set; }

        /// <summary>
        /// Maximum custom error message
        /// </summary>
        public string ErrorMaximunMessage { get; set; }

        /// <summary>
        /// Minimum custom error message
        /// </summary>
        public string ErrorminimumMessage { get; set; }

        /// <summary>
        /// RegEx custom error message
        /// </summary>
        public string ErrorRegExMessage { get; set; }

        /// <summary>
        /// Property to ignore column
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Property to ignore column in an select query
        /// </summary>
        public bool IgnoreInSelect { get; set; }
        /// <summary>
        /// Property to ignore column in an insert query
        /// </summary>
        public bool IgnoreInInsert { get; set; }
        /// <summary>
        /// Property to ignore column in an update query
        /// </summary>
        public bool IgnoreInUpdate { get; set; }
        /// <summary>
        /// Property to ignore column in an delete query
        /// </summary>
        public bool IgnoreInDelete { get; set; }

        public ColumnAttribute(){}

        public ColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }

    }
}
