using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Foundation.StringUtility;

/*

Stuff to add:

SQL Logic 3 other database types - Done for all 3
A framework for seed data insertion - Done including simple link logic
A framework for automating the creation of the security system foundational records for the module
A framework for reading a SQL server schema and automating the creation of an implementation of this class - probably not worth the effort
Create generator implementation class for Auditor - Done
Create generator implementation class for Security - Done
*/

namespace Foundation.CodeGeneration
{
    /*
     * 
     * fix list - add dupe check on add controller fields
     * */
    public abstract class DatabaseGenerator
    {
        /*
        * 
        * The purpose of this class is to provide a standard way to define a database and to use that definition to produce structure SQL scripts that are compatible with MSSQL Server, PostgreSQL, MySQL and SQLite 
        * 
        */
        public Database database;
        public string moduleName;


        public const int MINIMUM_CHANGE_HISTORY_READ_PERMISSION_LEVEL = 10;
        public const int CHANGE_HISTORY_WRITE_PERMISSION_LEVEL = 255;               // only users with full admin rights can write to change history

        public enum DatabaseType
        {
            MSSQLServer = 0,
            PostgreSQL = 1,
            MySQL = 2,
            SQLite = 3
        }

        public abstract class DatabaseElement
        {
            public string name { get; set; }
            public string comment { get; set; }
            public abstract string CreateSQL(DatabaseType databaseType);

            internal void WriteComments(StringBuilder sb, int prefixTabs = 0)
            {
                if (string.IsNullOrEmpty(comment) == true)
                {
                    return;
                }

                if (comment.Contains(Environment.NewLine) == false)
                {
                    for (int i = 0; i < prefixTabs; i++)
                    {
                        sb.Append("\t");
                    }

                    sb.Append("-- ");
                    sb.AppendLine(comment);
                }
                else
                {
                    sb.AppendLine("/*");
                    sb.AppendLine(comment);
                    sb.AppendLine("*/");
                }
            }
        }

        public enum DataType
        {
            INTEGER_AUTO_NUMBER_KEY = 0,
            INTEGER_PRIMARY_KEY = 1,
            GUID = 10,
            GUID_PRIMARY_KEY = 11,
            STRING_PRIMARY_KEY = 12,
            STRING_HTML_COLOR = 13,     // Created as a string(10) and used for UI building to make it a color picker
            STRING_10 = 20,
            STRING_50 = 30,
            STRING_100 = 40,
            STRING_250 = 50,
            STRING_500 = 60,
            STRING_850 = 70,
            STRING_1000 = 80,
            STRING_2000 = 90,
            TEXT = 100,
            DATETIME = 110,
            DATE = 120,
            TIME = 121,
            MONEY = 130,                // foundational money data type.  NOT SUPPORTED BY SQLITE
            INTEGER = 140,              // 32 BITS
            SMALL_INTEGER = 141,        // 16 BITS
            TINY_INTEGER = 142,         // 8 BITS
            BIG_INTEGER = 150,          // 64 BITS
            BIG_INTEGER_AUTO_NUMBER_KEY = 151, // for really long keys 
            BIG_INTEGER_PRIMARY_KEY = 152,     // for really long keys
            SINGLE_PRECISION_FLOAT = 160,
            DOUBLE_PRECISION_FLOAT = 170,
            DECIMAL_38_22 = 180,        // 38 digits, 22 after the decimal.  This is biggest SQL server supports.  Add more of these as necessary for more granular specs NOT SUPPORTED BY SQLITE
            BINARY = 190,
            URI = 200,
            BOOL = 210,
            FOREIGN_KEY = 220,
            FOREIGN_KEY_BIG_INTEGER = 221,      // Link to long keys
            FOREIGN_KEY_GUID = 230,
            FOREIGN_KEY_STRING = 231,
            LAT_LONG = 240,              // will become numeric (11, 8) TO hold exactly what is needed for lats and longs  // NOT SUPPORTED BY SQLITE
            PDF = 250,
            PNG = 260,
            MP4 = 270
        }

        public class Database : DatabaseElement
        {
            protected string _schemaName;


            /// <summary>
            ///  To store the names and descriptions of the custom roles for the database
            /// </summary>
            protected Dictionary<string, string> _customRoles = new Dictionary<string, string>();

            /// <summary>
            /// To define a schema name to use when creating the database
            /// </summary>
            /// <param name="schemaName"></param>
            public void SetSchemaName(string schemaName)
            {
                _schemaName = schemaName;
            }


            public void AddCustomRole(string name, string description)
            { 
                this._customRoles.Add(name, description);
            }

            public class Table : DatabaseElement
            {
                /// <summary>
                /// 
                /// Instruction for controller code genertor to limit the size of data allowed on puts/posts
                /// 
                /// </summary>
                public int? maxPostBytes { get; set; }                


                /// <summary>
                /// 
                /// Custom role name for limiting write access.
                /// 
                /// Note - users with administrive role will also have access.
                /// 
                /// </summary>
                public string customWriteAccessRole { get; set; }


                /// <summary>
                /// 
                /// Custom role name for limiting read access.
                /// 
                /// Note - users with administrive role will also have access.
                /// 
                /// </summary>
                public string customReadAccessRole { get; set; }


                public class Field : DatabaseElement
                {
                    public DataType dataType { get; set; }

                    public bool nullable { get; set; }

                    //
                    // This is a secondary means to make an arbitrary field the primary key.  Will clash with the named int primary key data types.
                    //
                    // Note that the primary key named data types may not set this flag to true, which is not logically correct, but is just a needed logic hole to allow anything to be a primary key
                    //
                    public bool primaryKey { get; set; }

                    //
                    // This is a secondary means to make an arbitrary field auto increment.  Will clash with the named in primary kid id data types
                    //
                    // This is here for ad-hoc situations where an autoincrement is needed in a non-standard to the regular platform scenario, in order to allow that database to be defined.
                    //
                    public bool autoIncrement { get; set; }

                    public string defaultValue { get; set; }

                    public bool unique { get; set; }

                    public Table table { get; set; }

                    public bool readOnlyOnEdit { get; set; }

                    public int readPermissionLevelNeeded { get; set; }      // the minimum user read permission level needed for this field to appear in user interfaces.

                    public bool hideOnDefaultLists { get; set; } = false;   // True if this field should be excluded from default code generated list views

                    public Field()
                    {
                        //
                        // Default to a nullable string - Table needs to be assigned
                        //
                        dataType = DataType.STRING_500;
                        nullable = true;
                        primaryKey = false;         // see notes on definition
                        autoIncrement = false;      // see notes on definition
                        defaultValue = null;
                        unique = false;
                        readOnlyOnEdit = false;
                        readPermissionLevelNeeded = 0;

                        return;
                    }

                    public int? MaxStringLength()
                    {
                        int? output = null;


                        if (dataType == DataType.GUID)
                        {
                            return 100;  // more than ewe need
                        }
                        else if (dataType == DataType.STRING_10 || dataType == DataType.STRING_HTML_COLOR)
                        {
                            return 10;
                        }
                        else if (dataType == DataType.STRING_100)
                        {
                            return 100;
                        }
                        else if (dataType == DataType.STRING_1000)
                        {
                            return 1000;
                        }
                        else if (dataType == DataType.STRING_2000)
                        {
                            return 2000;
                        }
                        else if (dataType == DataType.STRING_250)
                        {
                            return 250;
                        }
                        else if (dataType == DataType.STRING_50)
                        {
                            return 50;
                        }
                        else if (dataType == DataType.STRING_500)
                        {
                            return 500;
                        }
                        else if (dataType == DataType.STRING_850)
                        {
                            return 850;
                        }

                        return output;
                    }

                    public Field(Table table)
                    {
                        //
                        // Default to a nullable string
                        //
                        dataType = DataType.STRING_500;
                        nullable = true;
                        defaultValue = null;
                        unique = false;
                        readOnlyOnEdit = false;

                        this.table = table;

                        return;
                    }

                    public Index CreateIndex(string name, bool isUnique = false, bool descending = false)
                    {
                        if (this.table == null)
                        {
                            throw new Exception("Table must be assigned on field before using field to create an index");
                        }

                        return this.table.CreateIndexForField(this, name, isUnique, descending);
                    }

                    public Index CreateIndex(bool isUnique = false)
                    {
                        if (this.table == null)
                        {
                            throw new Exception("Table must be assigned on field before using field to create an index");
                        }

                        return this.table.CreateIndexForField(this, isUnique);
                    }

                    // returns self so it can be chained along with other setter methods inline in a build string
                    public Field AddScriptComments(string comment)
                    {
                        this.comment = comment;

                        return this;
                    }

                    /// <summary>
                    /// 
                    /// Returns true if the field is part of the standard field set for an address
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool IsPartOfAddressFields()
                    {
                        if (this.name == "address1" ||
                            this.name == "address2" ||
                            this.name == "address3" ||
                            this.name == "city" ||
                            this.name == "stateId" ||
                            this.name == "postalCode")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }


                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for general text
                    /// 
                    /// </summary>
                    /// <returns></returns>

                    public bool isTextDataType()
                    {
                        if (dataType == DataType.STRING_10 ||
                            dataType == DataType.STRING_100 ||
                            dataType == DataType.STRING_1000 ||
                            dataType == DataType.STRING_2000 ||
                            dataType == DataType.STRING_250 ||
                            dataType == DataType.STRING_50 ||
                            dataType == DataType.STRING_500 ||
                            dataType == DataType.STRING_850 ||
                            dataType == DataType.URI ||
                            dataType == DataType.TEXT ||
                            dataType == DataType.STRING_HTML_COLOR)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }


                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for a significant amount of text
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool isLargeTextDataType()
                    {
                        if (dataType == DataType.STRING_1000 ||
                            dataType == DataType.STRING_2000 ||
                            dataType == DataType.STRING_500 ||
                            dataType == DataType.STRING_850 ||
                            dataType == DataType.TEXT)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }


                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for a date time
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool isDateTimeDataType()
                    {
                        if (dataType == DataType.DATETIME)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }


                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for a date
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool isDateDataType()
                    {
                        if (dataType == DataType.DATE)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }


                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for a time
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool isTimeDataType()
                    {
                        if (dataType == DataType.TIME)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for a number
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool isNumericDataType()
                    {
                        if (dataType == DataType.DECIMAL_38_22 ||
                            dataType == DataType.DOUBLE_PRECISION_FLOAT ||
                            dataType == DataType.SINGLE_PRECISION_FLOAT ||
                            dataType == DataType.INTEGER ||
                            dataType == DataType.INTEGER_AUTO_NUMBER_KEY ||
                            dataType == DataType.INTEGER_PRIMARY_KEY ||
                            dataType == DataType.BIG_INTEGER ||
                            dataType == DataType.LAT_LONG ||
                            dataType == DataType.BIG_INTEGER_AUTO_NUMBER_KEY ||
                            dataType == DataType.BIG_INTEGER_PRIMARY_KEY ||
                            dataType == DataType.MONEY ||
                            dataType == DataType.SMALL_INTEGER ||
                            dataType == DataType.TINY_INTEGER)

                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }


                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for a bool
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool isBooleanDataType()
                    {
                        if (dataType == DataType.BOOL)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }


                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for a link to another table
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool isForeignKeyDataType()
                    {
                        if (dataType == DataType.FOREIGN_KEY ||
                            dataType == DataType.FOREIGN_KEY_GUID ||
                            dataType == DataType.FOREIGN_KEY_STRING ||
                            dataType == DataType.FOREIGN_KEY_BIG_INTEGER)

                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }


                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for an HTML color code
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool isHTMLColor()
                    {
                        return this.dataType == DataType.STRING_HTML_COLOR;
                    }


                    /// <summary>
                    /// 
                    ///  Helper to determine if the field is for an email address
                    /// 
                    /// </summary>
                    /// <returns></returns>
                    public bool isEmailAddress()
                    {
                        if (isTextDataType() == true)
                        {
                            if (this.name.ToLower().Contains("email") == true)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    public bool isPhoneNumber()
                    {
                        if (isTextDataType() == true || this.dataType == DataType.INTEGER)
                        {
                            if (this.name.ToLower().Contains("phone") == true)
                            {
                                return true;
                            }
                        }

                        return false;
                    }


                    public override string CreateSQL(DatabaseType databaseType)
                    {
                        StringBuilder sb = new StringBuilder();

                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                sb.Append("\t[");
                                sb.Append(this.name);
                                sb.Append("] ");

                                switch (this.dataType)
                                {
                                    case DataType.INTEGER_AUTO_NUMBER_KEY:

                                        sb.Append("INT IDENTITY PRIMARY KEY");

                                        break;

                                    case DataType.INTEGER_PRIMARY_KEY:

                                        sb.Append("INT PRIMARY KEY");

                                        break;

                                    case DataType.BIG_INTEGER_AUTO_NUMBER_KEY:

                                        sb.Append("BIGINT IDENTITY PRIMARY KEY");

                                        break;

                                    case DataType.BIG_INTEGER_PRIMARY_KEY:

                                        sb.Append("BIGINT PRIMARY KEY");

                                        break;

                                    case DataType.GUID_PRIMARY_KEY:

                                        sb.Append("UNIQUEIDENTIFIER PRIMARY KEY");

                                        break;

                                    case DataType.STRING_PRIMARY_KEY:

                                        sb.Append("NVARCHAR(500) PRIMARY KEY");

                                        break;

                                    case DataType.FOREIGN_KEY:

                                        sb.Append("INT");        // Needs to be the same field type as the auto number key

                                        break;

                                    case DataType.FOREIGN_KEY_BIG_INTEGER:

                                        sb.Append("BIGINT");        // Needs to be the same field type as the auto number key

                                        break;

                                    case DataType.BOOL:

                                        sb.Append("BIT");
                                        break;

                                    case DataType.BINARY:
                                    case DataType.PDF:
                                    case DataType.PNG:
                                    case DataType.MP4:

                                        sb.Append("VARBINARY(MAX)");
                                        break;

                                    case DataType.DATE:

                                        sb.Append("DATE");
                                        break;

                                    case DataType.TIME:

                                        sb.Append("TIME(7)");
                                        break;

                                    case DataType.DATETIME:

                                        sb.Append("DATETIME2(7)");
                                        break;

                                    case DataType.GUID:
                                    case DataType.FOREIGN_KEY_GUID:

                                        sb.Append("UNIQUEIDENTIFIER");
                                        break;

                                    case DataType.SMALL_INTEGER:

                                        sb.Append("SMALLINT");
                                        break;

                                    case DataType.TINY_INTEGER:

                                        sb.Append("TINYINT");
                                        break;

                                    case DataType.INTEGER:

                                        sb.Append("INT");
                                        break;

                                    case DataType.BIG_INTEGER:

                                        sb.Append("BIGINT");
                                        break;

                                    case DataType.SINGLE_PRECISION_FLOAT:

                                        sb.Append("REAL");          // SQL Server REAL is a 4 byte floating point
                                        break;

                                    case DataType.DOUBLE_PRECISION_FLOAT:

                                        sb.Append("FLOAT");         // SQL Server FLOAT is an 8 byte floating point
                                        break;

                                    case DataType.DECIMAL_38_22:

                                        sb.Append("NUMERIC(38,22)");
                                        break;

                                    case DataType.LAT_LONG:

                                        sb.Append("NUMERIC(11,8)");
                                        break;

                                    case DataType.MONEY:

                                        sb.Append("MONEY");
                                        break;

                                    case DataType.URI:

                                        sb.Append("NVARCHAR(1000)");
                                        break;

                                    case DataType.TEXT:

                                        sb.Append("NVARCHAR(MAX)");
                                        break;

                                    case DataType.STRING_10:
                                    case DataType.STRING_HTML_COLOR:

                                        sb.Append("NVARCHAR(10)");
                                        break;

                                    case DataType.STRING_50:

                                        sb.Append("NVARCHAR(50)");
                                        break;

                                    case DataType.STRING_100:

                                        sb.Append("NVARCHAR(100)");
                                        break;

                                    case DataType.STRING_250:

                                        sb.Append("NVARCHAR(250)");
                                        break;

                                    case DataType.STRING_500:

                                        sb.Append("NVARCHAR(500)");
                                        break;

                                    case DataType.STRING_850:

                                        sb.Append("NVARCHAR(850)");
                                        break;

                                    case DataType.STRING_1000:

                                        sb.Append("NVARCHAR(1000)");
                                        break;

                                    case DataType.STRING_2000:

                                        sb.Append("NVARCHAR(2000)");
                                        break;


                                    case DataType.FOREIGN_KEY_STRING:

                                        sb.Append("NVARCHAR(500)");
                                        break;

                                    default:
                                        throw new Exception("Unsupported data type of " + this.dataType.ToString());
                                }

                                if (this.autoIncrement == true)
                                {
                                    sb.Append(" IDENTITY");
                                }

                                if (this.primaryKey == true)
                                {
                                    sb.Append(" PRIMARY KEY");
                                }

                                if (this.nullable == true)
                                {
                                    sb.Append(" NULL");
                                }
                                else
                                {
                                    sb.Append(" NOT NULL");
                                }

                                if (this.unique == true)
                                {
                                    sb.Append(" UNIQUE");
                                }

                                if (this.defaultValue != null)
                                {
                                    switch (this.dataType)
                                    {
                                        case DataType.BOOL:
                                        case DataType.INTEGER:
                                        case DataType.BIG_INTEGER:
                                        case DataType.SINGLE_PRECISION_FLOAT:
                                        case DataType.DOUBLE_PRECISION_FLOAT:
                                        case DataType.DECIMAL_38_22:
                                        case DataType.MONEY:
                                        case DataType.FOREIGN_KEY:
                                        case DataType.FOREIGN_KEY_BIG_INTEGER:

                                            sb.Append(" DEFAULT " + defaultValue);
                                            break;

                                        case DataType.BINARY:
                                        case DataType.PDF:
                                        case DataType.PNG:
                                        case DataType.MP4:
                                        case DataType.STRING_10:
                                        case DataType.STRING_50:
                                        case DataType.STRING_100:
                                        case DataType.STRING_250:
                                        case DataType.STRING_500:
                                        case DataType.STRING_850:
                                        case DataType.STRING_1000:
                                        case DataType.STRING_2000:
                                        case DataType.TEXT:
                                        case DataType.STRING_HTML_COLOR:

                                            sb.Append(" DEFAULT '" + defaultValue.Replace("'", "''") + "'");

                                            break;

                                        default:
                                            throw new Exception("Unsupported data type of " + this.dataType.ToString());

                                    }
                                }

                                break;

                            case DatabaseType.MySQL:

                                sb.Append("\t`");
                                sb.Append(this.name);
                                sb.Append("` ");

                                switch (this.dataType)
                                {
                                    case DataType.INTEGER_AUTO_NUMBER_KEY:

                                        sb.Append("INT PRIMARY KEY");

                                        break;

                                    case DataType.INTEGER_PRIMARY_KEY:

                                        sb.Append("INT PRIMARY KEY");

                                        break;

                                    case DataType.BIG_INTEGER_AUTO_NUMBER_KEY:

                                        sb.Append("BIGINT PRIMARY KEY");

                                        break;

                                    case DataType.BIG_INTEGER_PRIMARY_KEY:

                                        sb.Append("BIGINT PRIMARY KEY");

                                        break;

                                    case DataType.GUID_PRIMARY_KEY:

                                        sb.Append("CHAR(38) PRIMARY KEY");

                                        break;

                                    case DataType.STRING_PRIMARY_KEY:

                                        sb.Append("CHAR(500) PRIMARY KEY");

                                        break;

                                    case DataType.FOREIGN_KEY:

                                        sb.Append("INT");        // Needs to be the same field type as the auto number key

                                        break;


                                    case DataType.FOREIGN_KEY_BIG_INTEGER:

                                        sb.Append("BIGINT");        // Needs to be the same field type as the auto number key

                                        break;
                                    case DataType.BOOL:

                                        sb.Append("BIT");
                                        break;

                                    case DataType.BINARY:
                                    case DataType.PDF:
                                    case DataType.PNG:
                                    case DataType.MP4:

                                        sb.Append("BLOB");
                                        break;

                                    case DataType.DATE:

                                        sb.Append("DATE");
                                        break;

                                    case DataType.TIME:

                                        sb.Append("TIME");
                                        break;

                                    case DataType.DATETIME:

                                        sb.Append("DATETIME");
                                        break;

                                    case DataType.GUID:
                                    case DataType.FOREIGN_KEY_GUID:

                                        sb.Append("CHAR(38)");
                                        break;

                                    case DataType.FOREIGN_KEY_STRING:

                                        sb.Append("VARCHAR(500)");
                                        break;

                                    case DataType.SMALL_INTEGER:

                                        sb.Append("SMALLINT");
                                        break;

                                    case DataType.TINY_INTEGER:

                                        sb.Append("TINYINT");
                                        break;

                                    case DataType.INTEGER:

                                        sb.Append("INT");
                                        break;

                                    case DataType.BIG_INTEGER:

                                        sb.Append("BIGINT");
                                        break;

                                    case DataType.SINGLE_PRECISION_FLOAT:

                                        sb.Append("FLOAT");
                                        break;

                                    case DataType.DOUBLE_PRECISION_FLOAT:

                                        sb.Append("DOUBLE");
                                        break;

                                    case DataType.DECIMAL_38_22:

                                        sb.Append("NUMERIC(38,22)");
                                        break;

                                    case DataType.LAT_LONG:

                                        sb.Append("NUMERIC(11,8)");
                                        break;


                                    case DataType.MONEY:

                                        sb.Append("DECIMAL(11,2)");
                                        break;

                                    case DataType.URI:

                                        sb.Append("VARCHAR(1000)");
                                        break;

                                    case DataType.TEXT:

                                        sb.Append("TEXT");
                                        break;

                                    case DataType.STRING_10:
                                    case DataType.STRING_HTML_COLOR:

                                        sb.Append("VARCHAR(10)");
                                        break;

                                    case DataType.STRING_50:

                                        sb.Append("VARCHAR(50)");
                                        break;

                                    case DataType.STRING_100:

                                        sb.Append("VARCHAR(100)");
                                        break;

                                    case DataType.STRING_250:

                                        sb.Append("VARCHAR(250)");
                                        break;

                                    case DataType.STRING_500:

                                        sb.Append("VARCHAR(500)");
                                        break;

                                    case DataType.STRING_850:

                                        sb.Append("VARCHAR(850)");
                                        break;


                                    case DataType.STRING_1000:

                                        sb.Append("VARCHAR(1000)");
                                        break;

                                    case DataType.STRING_2000:

                                        sb.Append("VARCHAR(2000)");
                                        break;

                                    default:
                                        throw new Exception("Unsupported data type of " + this.dataType.ToString());
                                }

                                if (this.autoIncrement == true)
                                {
                                    // not sure what would go here yet...
                                    // sb.Append(" IDENTITY");
                                }


                                if (this.primaryKey == true)
                                {
                                    sb.Append(" PRIMARY KEY");
                                }

                                if (this.nullable == true)
                                {
                                    sb.Append(" NULL");
                                }
                                else
                                {
                                    sb.Append(" NOT NULL");
                                }

                                if (this.dataType == DataType.INTEGER_AUTO_NUMBER_KEY)
                                {
                                    sb.Append(" AUTO_INCREMENT");
                                }

                                if (this.unique == true)
                                {
                                    sb.Append(" UNIQUE");
                                }

                                if (this.defaultValue != null)
                                {
                                    switch (this.dataType)
                                    {
                                        case DataType.BOOL:
                                        case DataType.INTEGER:
                                        case DataType.BIG_INTEGER:
                                        case DataType.SINGLE_PRECISION_FLOAT:
                                        case DataType.DOUBLE_PRECISION_FLOAT:
                                        case DataType.DECIMAL_38_22:
                                        case DataType.MONEY:
                                        case DataType.FOREIGN_KEY:
                                        case DataType.FOREIGN_KEY_BIG_INTEGER:

                                            sb.Append(" DEFAULT " + defaultValue);
                                            break;

                                        case DataType.BINARY:
                                        case DataType.PDF:
                                        case DataType.PNG:
                                        case DataType.MP4:
                                        case DataType.STRING_10:
                                        case DataType.STRING_50:
                                        case DataType.STRING_100:
                                        case DataType.STRING_250:
                                        case DataType.STRING_500:
                                        case DataType.STRING_850:
                                        case DataType.STRING_1000:
                                        case DataType.STRING_2000:
                                        case DataType.TEXT:
                                        case DataType.STRING_HTML_COLOR:

                                            sb.Append(" DEFAULT '" + defaultValue.Replace("'", "''") + "'");

                                            break;

                                        default:
                                            throw new Exception("Unsupported data type of " + this.dataType.ToString());

                                    }
                                }

                                break;

                            case DatabaseType.PostgreSQL:

                                sb.Append("\t\"");
                                sb.Append(this.name);
                                sb.Append("\" ");

                                switch (this.dataType)
                                {
                                    case DataType.INTEGER_AUTO_NUMBER_KEY:

                                        sb.Append("SERIAL PRIMARY KEY");
                                        break;


                                    case DataType.INTEGER_PRIMARY_KEY:

                                        sb.Append("INT PRIMARY KEY");

                                        break;

                                    case DataType.BIG_INTEGER_AUTO_NUMBER_KEY:

                                        sb.Append("BIGSERIAL PRIMARY KEY");
                                        break;


                                    case DataType.BIG_INTEGER_PRIMARY_KEY:

                                        sb.Append("BIGINT PRIMARY KEY");

                                        break;

                                    case DataType.GUID_PRIMARY_KEY:

                                        sb.Append("VARCHAR(50) PRIMARY KEY");

                                        break;

                                    case DataType.STRING_PRIMARY_KEY:

                                        sb.Append("VARCHAR(500) PRIMARY KEY");

                                        break;

                                    case DataType.FOREIGN_KEY:

                                        sb.Append("INT");        // Needs to be the same field type as the auto number key

                                        break;


                                    case DataType.FOREIGN_KEY_BIG_INTEGER:

                                        sb.Append("BIGINT");        // Needs to be the same field type as the auto number key

                                        break;

                                    case DataType.BOOL:

                                        sb.Append("BOOLEAN");
                                        break;

                                    case DataType.BINARY:
                                    case DataType.PDF:
                                    case DataType.PNG:
                                    case DataType.MP4:

                                        sb.Append("BYTEA");
                                        break;

                                    case DataType.DATE:

                                        sb.Append("DATE");
                                        break;

                                    case DataType.TIME:

                                        sb.Append("TIME");
                                        break;

                                    case DataType.DATETIME:

                                        sb.Append("TIMESTAMP");
                                        break;

                                    case DataType.GUID:
                                    case DataType.FOREIGN_KEY_GUID:

                                        sb.Append("VARCHAR(50)");
                                        break;

                                    case DataType.FOREIGN_KEY_STRING:

                                        sb.Append("VARCHAR(500)");
                                        break;
                                    case DataType.BIG_INTEGER:

                                        sb.Append("BIGINT");
                                        break;


                                    case DataType.SMALL_INTEGER:

                                        sb.Append("SMALLINT");
                                        break;

                                    case DataType.TINY_INTEGER:

                                        sb.Append("TINYINT");
                                        break;

                                    case DataType.INTEGER:

                                        sb.Append("INT");
                                        break;

                                    case DataType.SINGLE_PRECISION_FLOAT:

                                        sb.Append("REAL");          // PostgreSQL REAL is a 4 byte floating point
                                        break;

                                    case DataType.DOUBLE_PRECISION_FLOAT:

                                        sb.Append("DOUBLE PRECISION");  // PostgreSQL DOUBLE PRECISION is an 8 byte floating point
                                        break;

                                    case DataType.DECIMAL_38_22:

                                        sb.Append("NUMERIC(38,22)");
                                        break;

                                    case DataType.LAT_LONG:

                                        sb.Append("NUMERIC(11,8)");
                                        break;


                                    case DataType.MONEY:

                                        sb.Append("DECIMAL(11,2)");
                                        break;

                                    case DataType.URI:

                                        sb.Append("VARCHAR(1000)");
                                        break;

                                    case DataType.TEXT:

                                        sb.Append("TEXT");
                                        break;

                                    case DataType.STRING_10:
                                    case DataType.STRING_HTML_COLOR:

                                        sb.Append("VARCHAR(10)");
                                        break;

                                    case DataType.STRING_50:

                                        sb.Append("VARCHAR(50)");
                                        break;

                                    case DataType.STRING_100:

                                        sb.Append("VARCHAR(100)");
                                        break;

                                    case DataType.STRING_250:

                                        sb.Append("VARCHAR(250)");
                                        break;

                                    case DataType.STRING_500:

                                        sb.Append("VARCHAR(500)");
                                        break;

                                    case DataType.STRING_850:

                                        sb.Append("VARCHAR(850)");
                                        break;

                                    case DataType.STRING_1000:

                                        sb.Append("VARCHAR(1000)");
                                        break;

                                    case DataType.STRING_2000:

                                        sb.Append("VARCHAR(2000)");
                                        break;

                                    default:
                                        throw new Exception("Unsupported data type of " + this.dataType.ToString());
                                }


                                if (this.autoIncrement == true)
                                {
                                    sb.Append(" SERIAL");
                                }


                                if (this.primaryKey == true)
                                {
                                    sb.Append(" PRIMARY KEY");
                                }

                                if (this.nullable == true)
                                {
                                    sb.Append(" NULL");
                                }
                                else
                                {
                                    sb.Append(" NOT NULL");
                                }

                                if (this.unique == true)
                                {
                                    sb.Append(" UNIQUE");
                                }

                                if (this.defaultValue != null)
                                {
                                    switch (this.dataType)
                                    {
                                        case DataType.BOOL:

                                            sb.Append(" DEFAULT " + (defaultValue == "1" ? "true" : "false"));
                                            break;

                                        case DataType.INTEGER:
                                        case DataType.BIG_INTEGER:
                                        case DataType.SINGLE_PRECISION_FLOAT:
                                        case DataType.DOUBLE_PRECISION_FLOAT:
                                        case DataType.DECIMAL_38_22:
                                        case DataType.MONEY:
                                        case DataType.FOREIGN_KEY:
                                        case DataType.FOREIGN_KEY_BIG_INTEGER:

                                            sb.Append(" DEFAULT " + defaultValue);
                                            break;

                                        case DataType.BINARY:
                                        case DataType.PDF:
                                        case DataType.PNG:
                                        case DataType.MP4:
                                        case DataType.STRING_10:
                                        case DataType.STRING_50:
                                        case DataType.STRING_100:
                                        case DataType.STRING_250:
                                        case DataType.STRING_500:
                                        case DataType.STRING_850:
                                        case DataType.STRING_1000:
                                        case DataType.STRING_2000:
                                        case DataType.TEXT:
                                        case DataType.STRING_HTML_COLOR:

                                            sb.Append(" DEFAULT '" + defaultValue.Replace("'", "''") + "'");

                                            break;

                                        default:
                                            throw new Exception("Unsupported data type of " + this.dataType.ToString());

                                    }
                                }

                                break;

                            case DatabaseType.SQLite:

                                sb.Append("\t\"");
                                sb.Append(this.name);
                                sb.Append("\" ");

                                switch (this.dataType)
                                {
                                    case DataType.INTEGER_AUTO_NUMBER_KEY:

                                        sb.Append("INTEGER PRIMARY KEY AUTOINCREMENT");       // the intention here is that this is an alias for the SQLite rowId

                                        break;

                                    case DataType.INTEGER_PRIMARY_KEY:

                                        sb.Append("INTEGER PRIMARY KEY ASC");

                                        break;

                                    case DataType.BIG_INTEGER_AUTO_NUMBER_KEY:

                                        // Note that SQLite won't allow BIGINT PKs to be auto incrementing....  Be careful with this when doing the entity model, as it might break FKs...
                                        sb.Append("INTEGER PRIMARY KEY AUTOINCREMENT");       // the intention here is that this is an alias for the SQLite rowId

                                        break;

                                    case DataType.BIG_INTEGER_PRIMARY_KEY:

                                        // Note that SQLite won't allow BIGINT PKs to be auto incrementing....  Be careful with this when doing the entity model, as it might break FKs...
                                        sb.Append("BIGINT PRIMARY KEY ASC");

                                        break;

                                    case DataType.GUID_PRIMARY_KEY:

                                        sb.Append("VARCHAR(50) PRIMARY KEY ASC");

                                        break;

                                    case DataType.STRING_PRIMARY_KEY:

                                        sb.Append("VARCHAR(500) PRIMARY KEY ASC");

                                        break;

                                    case DataType.FOREIGN_KEY:

                                        sb.Append("INTEGER");        // Needs to be the same field type as the auto number key

                                        break;

                                    case DataType.FOREIGN_KEY_BIG_INTEGER:

                                        sb.Append("BIGINT");        // Needs to be the same field type as the auto number key

                                        break;

                                    case DataType.BOOL:

                                        sb.Append("BIT");
                                        break;

                                    case DataType.BINARY:
                                    case DataType.PDF:
                                    case DataType.PNG:
                                    case DataType.MP4:

                                        sb.Append("BLOB");
                                        break;

                                    case DataType.DATE:

                                        sb.Append("DATE");
                                        break;

                                    case DataType.TIME:

                                        sb.Append("TEXT");  // Inteded to store as 'HH:MM:SS'
                                        break;

                                    case DataType.DATETIME:

                                        sb.Append("DATETIME");
                                        break;

                                    case DataType.GUID:
                                    case DataType.FOREIGN_KEY_GUID:

                                        sb.Append("VARCHAR(50)");
                                        break;

                                    case DataType.FOREIGN_KEY_STRING:

                                        sb.Append("VARCHAR(500)");
                                        break;

                                    case DataType.SMALL_INTEGER:

                                        sb.Append("SMALLINT");
                                        break;

                                    case DataType.TINY_INTEGER:

                                        sb.Append("TINYINT");
                                        break;

                                    case DataType.INTEGER:

                                        sb.Append("INTEGER");
                                        break;

                                    case DataType.BIG_INTEGER:

                                        sb.Append("BIGINT");
                                        break;

                                    case DataType.SINGLE_PRECISION_FLOAT:

                                        sb.Append("REAL");          // SQLite stores this as 8 bytes, and has no native 4 byte floating point.
                                        break;

                                    case DataType.DOUBLE_PRECISION_FLOAT:

                                        sb.Append("REAL");          // SQLite stores this as 8 bytes
                                        break;

                                    case DataType.DECIMAL_38_22:

                                        sb.Append("NUMERIC");
                                        break;

                                    case DataType.LAT_LONG:

                                        sb.Append("NUMERIC");
                                        break;

                                    case DataType.MONEY:

                                        sb.Append("NUMERIC");
                                        break;

                                    case DataType.URI:

                                        sb.Append("VARCHAR(1000)");
                                        break;

                                    case DataType.TEXT:

                                        sb.Append("TEXT");
                                        break;

                                    case DataType.STRING_10:
                                    case DataType.STRING_HTML_COLOR:

                                        sb.Append("VARCHAR(10)");
                                        break;

                                    case DataType.STRING_50:

                                        sb.Append("VARCHAR(50)");
                                        break;

                                    case DataType.STRING_100:

                                        sb.Append("VARCHAR(100)");
                                        break;

                                    case DataType.STRING_250:

                                        sb.Append("VARCHAR(250)");
                                        break;

                                    case DataType.STRING_500:

                                        sb.Append("VARCHAR(500)");
                                        break;

                                    case DataType.STRING_850:

                                        sb.Append("VARCHAR(850)");
                                        break;

                                    case DataType.STRING_1000:

                                        sb.Append("VARCHAR(1000)");
                                        break;

                                    case DataType.STRING_2000:

                                        sb.Append("VARCHAR(2000)");
                                        break;

                                    default:
                                        throw new Exception("Unsupported data type of " + this.dataType.ToString());
                                }

                                if (this.primaryKey == true)
                                {
                                    sb.Append(" PRIMARY KEY");
                                }

                                if (this.autoIncrement == true)
                                {
                                    sb.Append(" AUTOINCREMENT");
                                }

                                if (this.nullable == true)
                                {
                                    sb.Append(" NULL");
                                }
                                else
                                {
                                    sb.Append(" NOT NULL");
                                }

                                if (this.unique == true)
                                {
                                    sb.Append(" UNIQUE");
                                }

                                if (this.defaultValue != null)
                                {
                                    switch (this.dataType)
                                    {
                                        case DataType.BOOL:
                                        case DataType.INTEGER:
                                        case DataType.BIG_INTEGER:
                                        case DataType.SINGLE_PRECISION_FLOAT:
                                        case DataType.DOUBLE_PRECISION_FLOAT:
                                        case DataType.DECIMAL_38_22:
                                        case DataType.MONEY:
                                        case DataType.FOREIGN_KEY:
                                        case DataType.FOREIGN_KEY_BIG_INTEGER:

                                            sb.Append(" DEFAULT " + defaultValue);
                                            break;

                                        case DataType.BINARY:
                                        case DataType.PDF:
                                        case DataType.PNG:
                                        case DataType.MP4:
                                        case DataType.STRING_10:
                                        case DataType.STRING_50:
                                        case DataType.STRING_100:
                                        case DataType.STRING_250:
                                        case DataType.STRING_500:
                                        case DataType.STRING_850:
                                        case DataType.STRING_1000:
                                        case DataType.STRING_2000:
                                        case DataType.TEXT:
                                        case DataType.STRING_HTML_COLOR:

                                            sb.Append(" DEFAULT '" + defaultValue.Replace("'", "''") + "'");

                                            break;

                                        default:
                                            throw new Exception("Unsupported data type of " + this.dataType.ToString());
                                    }
                                }


                                //
                                // Make string fields case insensitive like the other databases do by default.
                                //
                                switch (this.dataType)
                                {
                                    case DataType.URI:
                                    case DataType.TEXT:
                                    case DataType.STRING_10:
                                    case DataType.STRING_50:
                                    case DataType.STRING_100:
                                    case DataType.STRING_250:
                                    case DataType.STRING_500:
                                    case DataType.STRING_850:
                                    case DataType.STRING_1000:
                                    case DataType.STRING_2000:
                                    case DataType.GUID:             // The guid ones are important so that the compares will work for mix and matched alpha chars in guid strings.  For indexes too, for same reason.
                                    case DataType.GUID_PRIMARY_KEY:
                                    case DataType.STRING_HTML_COLOR:

                                        sb.Append(" COLLATE NOCASE");

                                        break;

                                    default:
                                        break;
                                }

                                break;


                            default:
                                throw new Exception("Unsupported Database Type");
                        }

                        return sb.ToString();
                    }

                    //
                    // Simple unique function to allow chaining on the build 
                    //
                    public Field EnforceUniqueness()
                    {
                        this.unique = true;

                        return this;
                    }
                }

                public void AddDataVisibilitySupport()
                {

                    Field versionNumberField = GetFieldByName("versionNumber");

                    if (versionNumberField != null)
                    {
                        throw new Exception("version number field aleady exists on this table.  Please add version control after adding data visibility support.");
                    }


                    // first, make sure that multi tenant support is also on this table.  Add it if it is not.
                    Field tenantGuidField = GetFieldByName("tenantGuid");

                    if (tenantGuidField == null)
                    {
                        AddMultiTenantSupport();
                    }

                    Table organizationTable = this.database.GetTable("Organization");
                    Table departmentTable = this.database.GetTable("Department");
                    Table teamTable = this.database.GetTable("Team");
                    Table userTable = this.database.GetTable("User");

                    if (organizationTable == null ||
                        departmentTable == null ||
                        teamTable == null ||
                        userTable == null)
                    {
                        throw new Exception("One or more of the tables needed for data visibility support is missing from the database.  Please add them before enabling data visibility support at the table level.");
                    }

                    AddForeignKeyField("organizationId", organizationTable, true).comment = "The Organization that a user must belong to in order to interact with this record.";
                    AddForeignKeyField("departmentId", departmentTable, true).comment = "The Department that a user must belong to in order to interact with this record.";
                    AddForeignKeyField("teamId", teamTable, true).comment = "The Team that a user must belong to in order to can interact with this record.";
                    AddForeignKeyField("userId", userTable, true).comment = "The user that owns this record.";


                    //
                    // If adding data visibility support, increase the command timeout for this table to be no less than 90.  This is because tables with this type of filtering support will likely have very many records and this level of timing buffer will likely be helpful in avoiding user time out erros.  Default timeout is 30 seconds.
                    //
                    if (commandTimeoutSeconds < 90)
                    {
                        commandTimeoutSeconds = 90;
                    }
                }


                public void AddVersionControl()
                {
                    //
                    // Add this last - or at least after the multi tenancy and data visibility fields are on!!!
                    //

                    //
                    // Note that the change history tables aren't intended to be looked at from a full list view.  They do not have the data visibility org fields and that is on purpose.  The intention is for this data to be seen in the context of the id of the row of the parent table by an admin user that can see across all data visibility slots.
                    //
                    // This is why the minimum read permission level on change history tables is high.
                    //
                    Field versionNumberField = AddIntField("versionNumber", false, 1);
                    versionNumberField.comment = "The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.";

                    versionNumberField.readOnlyOnEdit = true;

                    //
                    // Add change history table 
                    //
                    Table changeHistoryTable = this.database.AddTable(this.name + "ChangeHistory");
                    changeHistoryTable.comment = "The change history for records from the " + this.name + " table.";

                    //
                    // Use the more restrictive read permission level of either the minimum for any change history, or the read permission level for this table.
                    //
                    changeHistoryTable.minimumReadPermissionLevel = (MINIMUM_CHANGE_HISTORY_READ_PERMISSION_LEVEL > this.minimumReadPermissionLevel ? MINIMUM_CHANGE_HISTORY_READ_PERMISSION_LEVEL : this.minimumReadPermissionLevel);

                    //
                    // Lock down the writes to the change history table to only admin users, and default the controller code to be commented
                    //
                    changeHistoryTable.minimumWritePermissionLevel = CHANGE_HISTORY_WRITE_PERMISSION_LEVEL;      // only admins can write to change history tables.
                    changeHistoryTable.adminAccessNeededToWrite = true;
                    changeHistoryTable.SetTableToBeReadonlyForControllerCreationPurposes();


                    //
                    // Determine the data type of the id field.  We use the same for the change history table
                    //
                    Field idField = GetFieldByName("id");

                    if (idField == null)
                    {
                        throw new Exception($"Can't add change history to table {name} because it has no id field.");
                    }

                    if (idField.dataType == DataType.INTEGER_AUTO_NUMBER_KEY)
                    {
                        changeHistoryTable.AddIdField(true, false);     // use regular int auto number
                    }
                    else if (idField.dataType == DataType.INTEGER_PRIMARY_KEY)
                    {
                        changeHistoryTable.AddIdField(false);     // use regular int
                    }
                    else if (idField.dataType == DataType.BIG_INTEGER_AUTO_NUMBER_KEY)
                    {
                        changeHistoryTable.AddIdField(true, true);      // use big int auto number
                    }
                    else if (idField.dataType == DataType.BIG_INTEGER_PRIMARY_KEY)
                    {
                        changeHistoryTable.AddIdField(false, true);      // use big int
                    }
                    else
                    {
                        throw new Exception($"Unsupported id field type of {idField.dataType}.  Cannot use it for change history table creation.");
                    }



                    //
                    // If the host table has multi tenant support on, then turn multi tenant support on the change history table.
                    //
                    if (IsMultiTenantEnabled() == true)
                    {
                        changeHistoryTable.AddMultiTenantSupport(true);
                    }


                    //
                    //  Note that all these fields are indexed.
                    // 
                    changeHistoryTable.AddForeignKeyField(CamelCase(this.name) + "Id", this, false, false);     // Note no index created here because a covering one is added later in this function
                    changeHistoryTable.AddIntField("versionNumber", false).AddScriptComments("This is the version number that is being historized.").CreateIndex();
                    changeHistoryTable.AddDateTimeField("timeStamp", false).AddScriptComments("The time that the record version was created.").CreateIndex();

                    if (IsDataVisibilityEnabled() == true)
                    {
                        // in DV enabled tables, this field gets the local user id, and a foreign key relationship
                        changeHistoryTable.AddForeignKeyField("userId", this.database.GetTable("User"), false, true);
                    }
                    else
                    {
                        // In non DV enabled tables, this field gets the security user id 
                        changeHistoryTable.AddIntField("userId", false).CreateIndex();
                    }

                    changeHistoryTable.AddTextField("data", false).AddScriptComments("This stores the JSON representing the object's historical state.");


                    changeHistoryTable.AddSortSequence(changeHistoryTable.GetFieldByName("id"), true);      // could order by version number or timestamp, but id is indexed, and will do the same thing.

                    //
                    // Covering index on change history tables for fast “latest version” lookups:
                    // 
                    changeHistoryTable.CreateIndexForField(CamelCase(this.name) + "Id").AddCoveringFields((new List<string>() { "versionNumber", "timeStamp", "userId" }).ToArray());
                }

                public void AddMultiTenantSupport(bool disregardVersionNumber = false)
                {
                    //
                    // Ideally, this field is placed right after the id field.
                    //
                    if (this.fields.Count > 1 || fields.Count == 0 || (fields.Count == 1 && fields[0].name != "id"))
                    {
                        throw new Exception("This field should be the second field on a table after the ID field.");
                    }

                    Field versionNumberField = GetFieldByName("versionNumber");

                    if (versionNumberField != null && disregardVersionNumber == false)  // this just pertains to the order of the version nUmber field.  It's just for style.  This is an exception for the change history tables where the style is different.
                    {
                        throw new Exception("version number field already exists on this table.  Please add version control after adding the multi tenant support.");
                    }


                    Field tenantGuidField = AddField(new Database.Table.Field { name = "tenantGuid", dataType = DataType.GUID, nullable = false, table = this, readOnlyOnEdit = true });
                    tenantGuidField.comment = "The guid for the Tenant to which this record belongs.";
                    tenantGuidField.CreateIndex();      // we always want an index on the tenant guid field.


                    if (commandTimeoutSeconds < 60)
                    {
                        commandTimeoutSeconds = 60;     // make the command timeout no less than 60 seconds for multi tenant enabled tables as a precaution because the table could get quite large with many tenants.
                    }

                    //
                    // Turn multi tenant mode on for the database.
                    //
                    this.database.multiTenantEnabled = true;

                    return;
                }

                /// <summary>
                /// 
                /// This adds a standard set of address fields to the table.
                /// 
                /// It expects that a 'State' table is already in the database.
                /// 
                /// </summary>
                /// <exception cref="Exception"></exception>
                public void AddAddressFields(bool linkToStateTable = true)
                {
                    if (linkToStateTable == true)
                    {
                        Table stateTable = this.database.GetTable("State");

                        if (stateTable == null)
                        {
                            throw new Exception("Cannot add address tables when the database has no state table.");
                        }


                        this.AddString250Field("address1", true);
                        this.AddString250Field("address2", true);
                        this.AddString250Field("address3", true);
                        this.AddString250Field("city", true);
                        this.AddForeignKeyField(stateTable, true);
                        this.AddString10Field("postalCode");
                    }
                    else
                    {
                        this.AddString250Field("address1", true);
                        this.AddString250Field("address2", true);
                        this.AddString250Field("address3", true);
                        this.AddString250Field("city", true);
                        this.AddString100Field("state", true);
                        this.AddString10Field("postalCode");
                        this.AddString100Field("country", true);
                    }
                }


                public bool HasAddressFields()
                {
                    List<string> fieldNames = (from x in this.fields select x.name).ToList();

                    if (fieldNames.Contains("address1") &&
                        fieldNames.Contains("address2") &&
                        fieldNames.Contains("address3") &&
                        fieldNames.Contains("city") &&
                        (fieldNames.Contains("stateId") || fieldNames.Contains("state")) &&
                        fieldNames.Contains("postalCode"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }


                public bool HasField(string fieldName)
                {
                    bool output = true;

                    try
                    {
                        Field field = this.GetFieldByName(fieldName);

                        if (field != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        output = false;
                    }

                    return output;
                }


                public bool IsVersionControlEnabled()
                {

                    if (this.name.EndsWith("ChangeHistory") == true)
                    {
                        return false;
                    }

                    try
                    {
                        //
                        // Must have versionNumber field
                        //
                        Field version = GetFieldByName("versionNumber");

                        if (version != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                public bool IsDataVisibilityEnabled()
                {
                    try
                    {
                        //
                        // Must have all fields related to data visibility.  Will throw error if any is not found.
                        //
                        Field tenantGuid = GetFieldByName("tenantGuid");
                        Field organizationId = GetFieldByName("organizationId");
                        Field departmentId = GetFieldByName("departmentId");
                        Field teamId = GetFieldByName("teamId");
                        Field userId = GetFieldByName("userId");


                        if (tenantGuid != null &&
                            organizationId != null &&
                            departmentId != null &&
                            teamId != null &&
                            userId != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;

                        }

                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }


                public bool IsMultiTenantEnabled()
                {
                    Field tenantGuid = GetFieldByName("tenantGuid");

                    if (tenantGuid != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }

                //
                // returns self so it can be chained along with other setter methods inline in a build string
                //
                public Table AddScriptComments(string comment)
                {
                    if (comment.Contains("//") == true ||
                        comment.Contains("/*") == true ||
                        comment.Contains("*/") == true ||
                        comment.Contains("--") == true)
                    {
                        throw new Exception("Invalid characters in comment string.");
                    }

                    this.comment = comment;

                    return this;
                }



                /// <summary>
                /// Adds an integer or big integer primary key to a table.  
                /// 
                /// Use BigInt if you expect the table to ever grow beyond 2,147,483,647 rows.
                /// </summary>
                /// <param name="autoNumber"></param>
                /// <param name="useBigInt"></param>
                /// <returns></returns>
                public Field AddIdField(bool autoNumber = true, bool useBigInt = false)
                {
                    if (autoNumber == true)
                    {
                        Field idField;

                        if (useBigInt == false)
                        {
                            idField = AddField(new Database.Table.Field { name = "id", dataType = DataType.INTEGER_AUTO_NUMBER_KEY, nullable = false, table = this });
                        }
                        else
                        {
                            idField = AddField(new Database.Table.Field { name = "id", dataType = DataType.BIG_INTEGER_AUTO_NUMBER_KEY, nullable = false, table = this });
                        }

                        this.SetPrimaryKeyField(idField);

                        return idField;
                    }
                    else
                    {
                        Field idField;


                        if (useBigInt == false)
                        {
                            idField = AddField(new Database.Table.Field { name = "id", dataType = DataType.INTEGER_PRIMARY_KEY, nullable = false, table = this });
                        }
                        else
                        {
                            idField = AddField(new Database.Table.Field { name = "id", dataType = DataType.BIG_INTEGER_PRIMARY_KEY, nullable = false, table = this });
                        }

                        this.SetPrimaryKeyField(idField);

                        return idField;
                    }
                }
                //
                // This adds a series of fields suitable for storing a PDF.  It handles the filename, the file size, and the file content.  All are needed for the code generators to operate.
                //
                public void AddPDFFields(string fieldName, bool nullable = true, int readPermissionNeeded = 0)
                {
                    this.pdfRootFieldName = fieldName;

                    AddField(new Database.Table.Field { name = fieldName + "FileName", dataType = DataType.STRING_250, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the PDF data field setup" });
                    AddField(new Database.Table.Field { name = fieldName + "Size", dataType = DataType.BIG_INTEGER, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the PDF data field setup" });
                    AddField(new Database.Table.Field { name = fieldName + "Data", dataType = DataType.PDF, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the PDF data field setup" });
                    AddField(new Database.Table.Field { name = fieldName + "MimeType", dataType = DataType.STRING_100, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the PDF data field setup" });

                    return;
                }

                //
                // This adds a series of fields suitable for storing a PNG  It handles the filename, the file size, and the file content.  All are needed for the code generators to operate.
                //
                public void AddPNGFields(string fieldName, bool nullable = true, int readPermissionNeeded = 0)
                {
                    this.pngRootFieldName = fieldName;

                    AddField(new Database.Table.Field { name = fieldName + "FileName", dataType = DataType.STRING_250, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the PNG data field setup" });
                    AddField(new Database.Table.Field { name = fieldName + "Size", dataType = DataType.BIG_INTEGER, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the PNG data field setup" });
                    AddField(new Database.Table.Field { name = fieldName + "Data", dataType = DataType.PNG, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the PNG data field setup" });
                    AddField(new Database.Table.Field { name = fieldName + "MimeType", dataType = DataType.STRING_100, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the PNG data field setup" });

                    return;
                }

                //
                // This adds a series of fields suitable for storing an MP4  It handles the filename, the file size, and the file content.
                //
                public void AddMP4Fields(string fieldName, bool nullable = true, int readPermissionNeeded = 0)
                {
                    this.mp4RootFieldName = fieldName;

                    AddField(new Database.Table.Field { name = fieldName + "FileName", dataType = DataType.STRING_250, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the MP4 data field setup" });
                    AddField(new Database.Table.Field { name = fieldName + "Size", dataType = DataType.BIG_INTEGER, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the MP4 data field setup" });
                    AddField(new Database.Table.Field { name = fieldName + "Data", dataType = DataType.MP4, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the MP4 data field setup" });
                    AddField(new Database.Table.Field { name = fieldName + "MimeType", dataType = DataType.STRING_100, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the MP4 data field setup" });

                    return;
                }


                //
                // This adds a series of fields suitable for storing ad-hoc binary data.  Use this if the specialization to pdf, png or mp4 isn't appropriate.   It handles the filename, the file size, and the file content.
                //
                public void AddBinaryDataFields(string fieldName, bool nullable = true, int readPermissionNeeded = 0)
                {
                    this.binaryDataRootFieldName = fieldName;

                    Field fn = AddField(new Database.Table.Field { name = fieldName + "FileName", dataType = DataType.STRING_250, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the binary data field setup" });
                    Field fs = AddField(new Database.Table.Field { name = fieldName + "Size", dataType = DataType.BIG_INTEGER, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the binary data field setup" });
                    Field fd = AddField(new Database.Table.Field { name = fieldName + "Data", dataType = DataType.BINARY, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the binary data field setup" });
                    Field fmt = AddField(new Database.Table.Field { name = fieldName + "MimeType", dataType = DataType.STRING_100, nullable = nullable, table = this, readOnlyOnEdit = true, readPermissionLevelNeeded = readPermissionNeeded, comment = "Part of the binary data field setup" });

                    fn.hideOnDefaultLists = true;
                    fs.hideOnDefaultLists = true;
                    fd.hideOnDefaultLists = true;
                    fmt.hideOnDefaultLists = true;

                    return;
                }


                public Field AddDateTimeField(string fieldName, bool nullable = true)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.DATETIME, nullable = nullable, table = this });
                }


                public Field AddDateField(string fieldName, bool nullable = true, bool ignoreSQLiteWarning = false)
                {
                    if (ignoreSQLiteWarning == false)
                    {
                        throw new Exception("SQLite does not like this.  It won't map the fields right, so use DateTime instead..");
                    }

                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.DATE, nullable = nullable, table = this });
                }


                public Field AddTimeField(string fieldName, bool nullable = true, bool ignoreSQLiteWarning = false)
                {
                    if (ignoreSQLiteWarning == false)
                    {
                        throw new Exception("SQLite does not like this.  It won't map the fields right, so use DateTime instead..");
                    }

                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.TIME, nullable = nullable, table = this });
                }


                public Field AddTinyIntField(string fieldName, bool nullable = true, int? defaultValue = null)
                {
                    if (defaultValue.HasValue == false)
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.TINY_INTEGER, nullable = nullable, table = this });
                    }
                    else
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.TINY_INTEGER, nullable = nullable, table = this, defaultValue = defaultValue.ToString() });
                    }
                }


                public Field AddSmallIntField(string fieldName, bool nullable = true, int? defaultValue = null)
                {
                    if (defaultValue.HasValue == false)
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.SMALL_INTEGER, nullable = nullable, table = this });
                    }
                    else
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.SMALL_INTEGER, nullable = nullable, table = this, defaultValue = defaultValue.ToString() });
                    }
                }


                public Field AddIntField(string fieldName, bool nullable = true, int? defaultValue = null)
                {
                    if (defaultValue.HasValue == false)
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.INTEGER, nullable = nullable, table = this });
                    }
                    else
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.INTEGER, nullable = nullable, table = this, defaultValue = defaultValue.ToString() });
                    }
                }



                public Field AddLongField(string fieldName, bool nullable = true, long? defaultValue = null)
                {
                    if (defaultValue.HasValue == false)
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.BIG_INTEGER, nullable = nullable, table = this });
                    }
                    else
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.BIG_INTEGER, nullable = nullable, table = this, defaultValue = defaultValue.ToString() });
                    }
                }


                public Field AddSingleField(string fieldName, bool nullable = true, float? defaultValue = null)
                {
                    if (defaultValue.HasValue == false)
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.SINGLE_PRECISION_FLOAT, nullable = nullable, table = this });
                    }
                    else
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.SINGLE_PRECISION_FLOAT, nullable = nullable, table = this, defaultValue = defaultValue.ToString() });
                    }
                }


                public Field AddDoubleField(string fieldName, bool nullable = true, int? defaultValue = null)
                {
                    if (defaultValue.HasValue == false)
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.DOUBLE_PRECISION_FLOAT, nullable = nullable, table = this });
                    }
                    else
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.DOUBLE_PRECISION_FLOAT, nullable = nullable, table = this, defaultValue = defaultValue.ToString() });
                    }
                }


                public Field AddDecimalField(string fieldName, bool nullable = true, decimal? defaultValue = null, bool ignoreSQLiteWarning = false)
                {
                    if (ignoreSQLiteWarning == false)
                    {
                        throw new Exception("SQLite does not like this data type.  It won't map the fields right, so use another number type instead if you want to support SQLite.  This will be a numeric field in SQL server., and that isn't happy in SQLite");
                    }

                    if (defaultValue.HasValue == false)
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.DECIMAL_38_22, nullable = nullable, table = this });
                    }
                    else
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.DECIMAL_38_22, nullable = nullable, table = this, defaultValue = defaultValue.ToString() });
                    }
                }

                public Field AddBooleanField(string fieldName, bool nullable = true, bool? defaultValue = null)
                { 
                    return AddBoolField(fieldName, nullable, defaultValue);
                }

                public Field AddBoolField(string fieldName, bool nullable = true, bool? defaultValue = null)
                {

                    if (defaultValue.HasValue == true)
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.BOOL, nullable = nullable, table = this, defaultValue = (defaultValue.Value == true ? "1" : "0") });

                    }
                    else
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.BOOL, nullable = nullable, table = this, defaultValue = null });
                    }
                }


                public Field AddGuidField(string fieldName, bool nullable = true, bool primaryKey = false)
                {
                    if (primaryKey == false)
                    {
                        return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.GUID, nullable = nullable, table = this });
                    }
                    else
                    {
                        var field = AddField(new Database.Table.Field { name = fieldName, dataType = DataType.GUID_PRIMARY_KEY, nullable = nullable, table = this });
                        this.SetPrimaryKeyField(field);

                        return field;
                    }
                }


                public Field AddBinaryField(string fieldName, bool nullable = true)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.BINARY, nullable = nullable, table = this });
                }


                public Field AddMoneyField(string fieldName, bool nullable = true, bool ignoreSQLiteWarning = false)
                {
                    if (ignoreSQLiteWarning == false)
                    {
                        throw new Exception("SQLite does not like this data type.  It won't map the fields right, so use another number type instead if you want to support SQLite.  This will be a numeric field in SQL server, and that isn't happy in SQLite");
                    }

                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.MONEY, nullable = nullable, table = this });
                }


                public Field AddMoneyField(string fieldName, bool nullable = true, decimal defaultValue = 0, bool ignoreSQLiteWarning = false)
                {
                    if (ignoreSQLiteWarning == false)
                    {
                        throw new Exception("SQLite does not like this data type.  It won't map the fields right, so use another number type instead if you want to support SQLite.  This will be a numeric field in SQL server, and that isn't happy in SQLite");
                    }

                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.MONEY, nullable = nullable, defaultValue = defaultValue.ToString(), table = this });
                }


                public Field AddStringPrimaryKeyField(string fieldName)
                {
                    Database.Table.Field stringPrimaryKey = AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_PRIMARY_KEY, nullable = false, table = this });
                    this.SetPrimaryKeyField(stringPrimaryKey);
                    return stringPrimaryKey;
                }


                public Field AddHTMLColorField(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_HTML_COLOR, nullable = nullable, table = this, defaultValue = defaultValue });
                }

                public Field AddString10Field(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_10, nullable = nullable, table = this, defaultValue = defaultValue });
                }
                public Field AddString50Field(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_50, nullable = nullable, table = this, defaultValue = defaultValue });
                }

                public Field AddString100Field(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_100, nullable = nullable, table = this, defaultValue = defaultValue });
                }

                public Field AddString250Field(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_250, nullable = nullable, table = this, defaultValue = defaultValue });
                }

                public Field AddString500Field(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_500, nullable = nullable, table = this, defaultValue = defaultValue });
                }

                public Field AddString850Field(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_850, nullable = nullable, table = this, defaultValue = defaultValue });
                }

                public Field AddString1000Field(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_1000, nullable = nullable, table = this, defaultValue = defaultValue });
                }

                public Field AddString2000Field(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.STRING_2000, nullable = nullable, table = this, defaultValue = defaultValue });
                }

                public Field AddTextField(string fieldName, bool nullable = true, string defaultValue = null)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.TEXT, nullable = nullable, table = this, defaultValue = defaultValue });
                }

                public Field AddLatLongField(string fieldName, bool nullable = true)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.LAT_LONG, nullable = nullable, table = this });
                }

                public Field AddWebAddressField(string fieldName, bool nullable = true)
                {
                    return AddField(new Database.Table.Field { name = fieldName, dataType = DataType.URI, nullable = nullable, table = this });
                }

                public Field AddNameField(bool nameUnique = true, bool indexOnName = true)
                {

                    if (this.IsMultiTenantEnabled() == false)
                    {
                        //
                        // In single tenant tables, being unique just on the name field is enough.
                        //
                        Field nameField = new Database.Table.Field { name = "name", dataType = DataType.STRING_100, nullable = false, unique = nameUnique, table = this };
                        fields.Add(nameField);

                        if (indexOnName == true)
                        {
                            this.CreateIndexForField(nameField);
                        }

                        return nameField;
                    }
                    else
                    {
                        //
                        // Name alone isn't enough to be unique.  Need to add tenant buid
                        //
                        Field nameField = new Database.Table.Field { name = "name", dataType = DataType.STRING_100, nullable = false, table = this };
                        AddField(nameField);

                        if (nameUnique == true)
                        {
                            this.AddUniqueConstraint("tenantGuid", "name");
                        }

                        if (indexOnName == true)
                        {
                            this.CreateIndexForFields(new List<string> { "tenantGuid", "name" });
                        }

                        return nameField;
                    }
                }

                //
                // Helper to allow the quick adding of a name/description pair that will work in most scenarios.  
                //
                // The name field will always be not nullable, but the description field is optionally not nullable.
                //
                // 
                public void AddNameAndDescriptionFields(bool nameUnique = true, bool indexOnName = true, bool descriptionNullable = true)
                {
                    if (this.IsMultiTenantEnabled() == false)
                    {
                        //
                        // In single tenant tables, being unique just on the name field is enough.
                        //
                        Field nameField = new Database.Table.Field { name = "name", dataType = DataType.STRING_100, nullable = false, unique = nameUnique, table = this };
                        AddField(nameField);

                        Field descriptionField = new Database.Table.Field { name = "description", dataType = DataType.STRING_500, nullable = descriptionNullable, table = this };
                        AddField(descriptionField);

                        if (indexOnName == true)
                        {
                            this.CreateIndexForField(nameField);
                        }
                    }
                    else
                    {
                        //
                        // In multi-tenant scenarios, name alone isn't enough to be unique.  Need to add tenant buid
                        //

                        Field nameField = new Database.Table.Field { name = "name", dataType = DataType.STRING_100, nullable = false, table = this };
                        AddField(nameField);

                        if (nameUnique == true)
                        {
                            this.AddUniqueConstraint("tenantGuid", "name");
                        }

                        Field descriptionField = new Database.Table.Field { name = "description", dataType = DataType.STRING_500, nullable = descriptionNullable, table = this };
                        AddField(descriptionField);

                        if (indexOnName == true)
                        {
                            this.CreateIndexForFields(new List<string> { "tenantGuid", "name" });
                        }
                    }
                }

                public Field AddSequenceField(bool nullable = true)
                {
                    Field sequenceField = AddField(new Database.Table.Field { name = "sequence", dataType = DataType.INTEGER, nullable = nullable, table = this });

                    sequenceField.comment = "Sequence to use for sorting.";

                    return sequenceField;
                }


                public void AddLatLongFields(bool nullable = true, bool createIndexes = true)
                {
                    Field latField = AddField(new Database.Table.Field { name = "latitude", dataType = DataType.LAT_LONG, nullable = nullable, table = this });
                    Field longField = AddField(new Database.Table.Field { name = "longitude", dataType = DataType.LAT_LONG, nullable = nullable, table = this });

                    if (createIndexes == true)
                    {
                        latField.CreateIndex();
                        longField.CreateIndex();
                    }

                    return;
                }

                public void AddControlFields(bool includeObjectGuid = true, bool objectGuidMustBeUnique = true, bool indexOnObjectGuid = false)
                {
                    //
                    // In some situations, the object guid won't be unique, in a table.  For example, the data visibility add on tables, the user table has an object guid that isn't unique.  The uniqueness is concatentated with tenantguid on that table.
                    //
                    if (includeObjectGuid == true)
                    {
                        // field can't be null, and must be unique.
                        Field objectGuidField = AddField(new Database.Table.Field
                        {
                            name = "objectGuid",
                            dataType = DataType.GUID,
                            nullable = false,
                            table = this,
                            unique = objectGuidMustBeUnique
                        });

                        objectGuidField.readOnlyOnEdit = true;
                        objectGuidField.readPermissionLevelNeeded = 50;     // object guid needs permission level of 50 or higher to be seen.

                        if (objectGuidMustBeUnique == true)
                        {
                            objectGuidField.comment = "Unique identifier for this table.";
                        }
                        else
                        {
                            objectGuidField.comment = "This does not have to be unique.";
                        }

                        if (indexOnObjectGuid == true)
                        {
                            objectGuidField.CreateIndex();
                        }
                    }

                    Field activeField = AddField(new Database.Table.Field { name = "active", dataType = DataType.BOOL, nullable = false, defaultValue = "1", table = this });
                    activeField.comment = "Active from a business perspective flag.";
                    activeField.CreateIndex();

                    Field deletedField = AddField(new Database.Table.Field { name = "deleted", dataType = DataType.BOOL, nullable = false, defaultValue = "0", table = this });
                    deletedField.comment = "Soft deletion flag.";
                    deletedField.CreateIndex();
                }

                public Field AddField(Field field)
                {
                    if (FieldAlreadyExists(fields, field.name) == true)
                    {
                        throw new Exception($"Value of {field.name} already exists on table {this.name}");
                    }

                    field.table = this;
                    this.fields.Add(field);
                    return field;
                }

                public Index CreateIndexForField(string fieldName, bool descending = false)
                {
                    Index.IndexField indexField = new Index.IndexField(this.GetFieldByName(fieldName), descending);

                    return CreateIndexForField(indexField);
                }

                public Index CreateIndexForField(Field field, bool isUnique = false, bool descending = false)
                {
                    if (field == null)
                    {
                        throw new Exception("Cannot create index because field is null.");
                    }

                    //
                    // Reroute this to be indexed with tenantGuid for multi-tenant tables, but don't do this for the tenantGuid field itself in case it is individually indexed
                    //
                    if (this.IsMultiTenantEnabled() == true && field.name != "tenantGuid")
                    {
                        Field tenantField = this.GetFieldByName("tenantGuid");

                        if (tenantField == null)
                        {
                            throw new Exception($"Could not get tenantGuid field from table {name}.");
                        }

                        return this.CreateIndexForFields(new List<Field>() { tenantField, field }, isUnique);
                    }


                    //
                    // I_tablename_fieldname
                    //
                    string indexName = "I_" + this.name + "_" + field.name;

                    if (IndexAlreadyExists(indexes, indexName) == true)
                    {
                        throw new Exception("Value of " + indexName + " already exists");
                    }

                    Index index = new Index(this, indexName);

                    index.isUnique = isUnique;

                    Index.IndexField indexField = new Index.IndexField(field, descending);

                    index.indexFields.Add(indexField);

                    this.indexes.Add(index);

                    return index;
                }


                public Index CreateIndexForField(Field field, string indexName, bool isUnique = false, bool descending = false)
                {
                    if (field == null)
                    {
                        throw new Exception("Cannot create index because field is null.");
                    }

                    //
                    // I_tablename_fieldname
                    //
                    if (IndexAlreadyExists(indexes, indexName) == true)
                    {
                        throw new Exception("Value of " + indexName + " already exists");
                    }

                    Index index = new Index(this, indexName);

                    index.isUnique = isUnique;


                    Index.IndexField indexField = new Index.IndexField(field, descending);

                    index.indexFields.Add(indexField);

                    this.indexes.Add(index);

                    return index;
                }


                public Index CreateIndexForFields(List<string> fieldNames, bool isUnique = false)
                {
                    List<Field> fields = new List<Field>();

                    foreach (string fieldName in fieldNames)
                    {
                        fields.Add(GetFieldByName(fieldName));
                    }

                    return CreateIndexForFields(fields);
                }


                public Index CreateIndexForFields(Field[] fields)
                {
                    string indexName = "I_" + this.name;

                    foreach (Field field in fields)
                    {
                        indexName += ("_" + field.name);
                    }

                    if (IndexAlreadyExists(indexes, indexName) == true)
                    {
                        throw new Exception("Value of " + indexName + " already exists");
                    }


                    Index index = new Index(this, indexName);

                    foreach (Field field in fields)
                    {
                        Index.IndexField indexField = new Index.IndexField(field);
                        index.indexFields.Add(indexField);
                    }

                    this.indexes.Add(index);

                    return index;
                }


                public void SetTableToBeReadonlyForControllerCreationPurposes()
                {
                    //
                    // Make the controllers read only by setting the writing API controller methods to be expected to be overridden, so they are built as comments instead of real code.s
                    //
                    this.webAPIDeleteToBeOverridden = true;
                    this.webAPIPostToBeOverridden = true;
                    this.webAPIPutToBeOverridden = true;
                    this.webAPIRollbackToBeOverridden = true;
                }


                public Index CreateIndexForFields(List<Field> fields, bool isUnique = false)
                {
                    string indexName = "I_" + this.name;

                    foreach (Field field in fields)
                    {
                        indexName += ("_" + field.name);
                    }

                    if (IndexAlreadyExists(indexes, indexName) == true)
                    {
                        throw new Exception("Value of " + indexName + " already exists");
                    }


                    Index index = new Index(this, indexName);

                    foreach (Field field in fields)
                    {
                        Index.IndexField indexField = new Index.IndexField(field);

                        index.indexFields.Add(indexField);
                    }

                    index.isUnique = isUnique;

                    this.indexes.Add(index);

                    return index;
                }


                public Field GetFieldByName(string name)
                {
                    foreach (Field field in this.fields)
                    {
                        if (field.name == name)
                        {
                            return field;
                        }
                    }

                    return null;
                }

                protected static bool FieldAlreadyExists(List<Field> fields, string value)
                {
                    //
                    // Checks the values list for the existing presence of 'value'
                    //
                    foreach (Field existingElement in fields)
                    {
                        if (existingElement.name == value)
                        {
                            return true;
                        }
                    }

                    return false;
                }


                public class Index : DatabaseElement
                {
                    private Table table;

                    public bool isUnique { get; set; } = false;

                    public class IndexField : Table.Field
                    {
                        public bool descending { get; set; } = false;

                        public IndexField(Table.Field field)
                        {
                            this.autoIncrement = field.autoIncrement;
                            this.comment = field.comment;
                            this.dataType = field.dataType;
                            this.defaultValue = field.defaultValue;
                            this.name = field.name;
                            this.primaryKey = field.primaryKey;
                            this.table = field.table;
                            this.nullable = field.nullable;
                            this.unique = field.unique;
                            
                            this.descending = false;
                        }

                        public IndexField(Table.Field field, bool descending)
                        {
                            this.autoIncrement = field.autoIncrement;
                            this.comment = field.comment;
                            this.dataType = field.dataType;
                            this.defaultValue = field.defaultValue;
                            this.name = field.name;
                            this.primaryKey = field.primaryKey;
                            this.table = field.table;
                            this.nullable = field.nullable;
                            this.unique = field.unique;

                            this.descending = descending;
                        }
                    }
                    public List<IndexField> indexFields { get; set; }

                    private List<Field> coveringFields { get; set; }

                    public Index(Table table, string name)
                    {
                        this.name = name;
                        this.table = table;

                        indexFields = new List<IndexField>();
                    }

                    // returns self so it can be chained along with other setter methods inline in a build string
                    public Index SetComment(string comment)
                    {
                        this.comment = comment;

                        return this;
                    }

                    public IndexField AddField(string fieldName, bool descending = false)
                    {
                        Field field = this.table.GetFieldByName(fieldName);

                        if (field == null)
                        {
                            throw new Exception($"Field {fieldName} does not exist on table {this.table.name}, so index {this.name} cannot add it.");
                        }

                        foreach (IndexField existingField in this.indexFields)
                        {
                            if (existingField.name == fieldName)
                            {
                                throw new Exception($"Index {this.name} already has field named {fieldName}.");
                            }
                        }

                        Index.IndexField indexField = new Index.IndexField(field, descending);

                        this.indexFields.Add(indexField);

                        return indexField;
                    }


                    public void AddCoveringFields(string[] fieldNames)
                    {
                        coveringFields = new List<Field>();

                        foreach (string fieldName in fieldNames)
                        {
                            Table.Field field = this.table.GetFieldByName(fieldName);

                            if (field == null)
                            {
                                throw new ArgumentException($"Could not find field {fieldName} on table {this.table.name} for covering index creation.");
                            }

                            this.coveringFields.Add(field);
                        }
                    }


                    public override string CreateSQL(DatabaseType databaseType)
                    {
                        StringBuilder sb = new StringBuilder();

                        if (string.IsNullOrEmpty(this.comment) == true && this.table.database.disableComments == false)
                        {
                            this.comment = "Index on the " + this.table.name + " table's " + string.Join(",", from x in this.indexFields select x.name) + (indexFields.Count == 1 ? " field." : " fields.");
                        }


                        if (string.IsNullOrEmpty(this.comment) == false && this.table.database.disableComments == false)
                        {
                            switch (databaseType)
                            {
                                case DatabaseType.MSSQLServer:
                                case DatabaseType.MySQL:
                                case DatabaseType.PostgreSQL:
                                case DatabaseType.SQLite:

                                    WriteComments(sb);

                                    break;

                                default:
                                    throw new Exception("Unsupported database type");
                            }
                        }

                        string fixedName;

                        List<Field> fieldsForIndex = null;      // Used for SQLite and MySQL only

                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                if (this.isUnique == false)
                                {
                                    sb.Append("CREATE INDEX [");
                                }
                                else
                                {
                                    sb.Append("CREATE UNIQUE INDEX [");
                                }

                                fixedName = this.name;
                                if (fixedName.Length > 128)
                                {
                                    fixedName = fixedName.Substring(0, 128);
                                }

                                sb.Append(fixedName);

                                if (this.table.database.disableSchemaName == false)
                                {
                                    sb.Append("] ON [" + this.table.database._schemaName + "].[");
                                }
                                else
                                {
                                    sb.Append("] ON [");
                                }

                                sb.Append(table.name);
                                sb.Append("] (");

                                foreach (IndexField indexField in this.indexFields)
                                {
                                    sb.Append("[");
                                    sb.Append(indexField.name);
                                    sb.Append("]");

                                    if (indexField.descending == true)
                                    {
                                        sb.Append(" DESC");
                                    }

                                    if (indexField != this.indexFields.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.coveringFields != null && this.coveringFields.Count > 0)
                                {

                                    sb.Append(@") INCLUDE ( ");

                                    foreach (Field coveringField in this.coveringFields)
                                    {
                                        sb.Append(coveringField.name);

                                        if (coveringField != this.coveringFields.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.AppendLine(@" )");
                                }
                                else
                                {
                                    sb.AppendLine(")");
                                }


                                //
                                // Add where clause to filter out nulls if creating unique index on table with nullable fields or much pain will be caused when records have null values...
                                //
                                if (this.isUnique == true && this.indexFields.Where(x => x.nullable == true).Any() == true)
                                {
                                    sb.Append(" WHERE ");

                                    List<IndexField> nullableFields = this.indexFields.Where(x => x.nullable == true).ToList();

                                    for (int i = 0; i < nullableFields.Count; i++)
                                    {
                                        sb.Append($"[{nullableFields[i].name}] IS NOT NULL");

                                        if (i != nullableFields.Count - 1)
                                        {
                                            sb.Append(" AND ");
                                        }
                                    }

                                    sb.AppendLine();
                                }

                                sb.AppendLine("GO");

                                break;

                            case DatabaseType.MySQL:

                                if (this.isUnique == false)
                                {
                                    sb.Append("CREATE INDEX `");
                                }
                                else
                                {
                                    sb.Append("CREATE UNIQUE INDEX `");
                                }



                                fixedName = this.name;
                                if (fixedName.Length > 64)
                                {
                                    fixedName = fixedName.Substring(0, 64);
                                }
                                sb.Append(fixedName);

                                sb.Append("` ON `");
                                sb.Append(table.name);
                                sb.Append("` (");


                                fieldsForIndex = new List<Field>();

                                // First add the real index fields
                                if (this.indexFields != null && this.indexFields.Count > 0)
                                {
                                    fieldsForIndex.AddRange(this.indexFields);
                                }


                                // Then add the covering fields
                                if (this.coveringFields != null && this.coveringFields.Count > 0)
                                {
                                    foreach (Field coveringField in this.coveringFields)
                                    {
                                        IndexField indexField = new IndexField(coveringField);

                                        fieldsForIndex.Add(indexField);
                                    }
                                }


                                foreach (IndexField indexField in fieldsForIndex)
                                {
                                    sb.Append("`");

                                    sb.Append(indexField.name);
                                    sb.Append("`");

                                    if (indexField.descending == true)
                                    {
                                        sb.Append(" DESC");
                                    }

                                    if (indexField != fieldsForIndex.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                sb.AppendLine(");");

                                //
                                // Add where clause to filter out nulls if creating unique index on table with nullable fields or much pain will be caused when records have null values...
                                //
                                if (this.isUnique == true && this.indexFields.Where(x => x.nullable == true).Any() == true)
                                {
                                    sb.Append(" WHERE ");

                                    List<IndexField> nullableFields = this.indexFields.Where(x => x.nullable == true).ToList();

                                    for (int i = 0; i < nullableFields.Count; i++)
                                    {
                                        sb.Append($"`{nullableFields[i].name}` IS NOT NULL");

                                        if (i != nullableFields.Count - 1)
                                        {
                                            sb.Append(" AND ");
                                        }
                                    }

                                    sb.AppendLine();
                                }


                                break;

                            case DatabaseType.PostgreSQL:

                                if (this.isUnique == false)
                                {
                                    sb.Append("CREATE INDEX \"");
                                }
                                else
                                {
                                    sb.Append("CREATE UNIQUE INDEX \"");
                                }

                                fixedName = this.name;
                                if (fixedName.Length > 63)
                                {
                                    fixedName = fixedName.Substring(0, 63);
                                }
                                sb.Append(fixedName);

                                sb.Append("\" ON \"");
                                if (this.table.database.disableSchemaName == false)
                                {
                                    sb.Append(this.table.database.name);
                                    sb.Append("\".\"");
                                }

                                sb.Append(table.name);
                                sb.Append("\" (");

                                foreach (IndexField indexField in this.indexFields)
                                {
                                    sb.Append("\"");
                                    sb.Append(indexField.name);
                                    sb.Append("\"");

                                    if (indexField.descending == true)
                                    {
                                        sb.Append(" DESC");
                                    }

                                    if (indexField != this.indexFields.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.coveringFields != null && this.coveringFields.Count > 0)
                                {

                                    sb.Append(@") INCLUDE ( ");

                                    foreach (Field coveringField in this.coveringFields)
                                    {
                                        sb.Append(coveringField.name);

                                        if (coveringField != this.coveringFields.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.AppendLine(@" )");
                                }
                                else
                                {
                                    sb.AppendLine(")");
                                }

                                //
                                // Add where clause to filter out nulls if creating unique index on table with nullable fields or much pain will be caused when records have null values...
                                //
                                if (this.isUnique == true && this.indexFields.Where(x => x.nullable == true).Any() == true)
                                {
                                    sb.Append(" WHERE ");

                                    List<IndexField> nullableFields = this.indexFields.Where(x => x.nullable == true).ToList();

                                    for (int i = 0; i < nullableFields.Count; i++)
                                    {
                                        sb.Append($"\"{nullableFields[i].name}\" IS NOT NULL");

                                        if (i != nullableFields.Count - 1)
                                        {
                                            sb.Append(" AND ");
                                        }
                                    }

                                    sb.AppendLine(";");
                                }
                                else
                                { 
                                    sb.AppendLine(";"); 
                                }


                                break;

                            case DatabaseType.SQLite:

                                if (this.isUnique == false)
                                {
                                    sb.Append("CREATE INDEX \"");
                                }
                                else
                                {
                                    sb.Append("CREATE UNIQUE INDEX \"");
                                }

                                fixedName = this.name;

                                if (fixedName.Length > 62)          // SQLite limits the length of index names...
                                {
                                    //
                                    // This is a less than ideal situation that risks duplicating index names, which breaks the creation of the schema.
                                    //
                                    // No ideal path way here.  First try to fix by removing vowels.  If that's not enough, then take from the end
                                    //
                                    fixedName = "I_" + RemoveVowels(fixedName.Substring(2));

                                    //
                                    // If this is still not enough, use the last 62 characters.
                                    //
                                    if (fixedName.Length > 62)
                                    {
                                        fixedName = "I_" + fixedName.Substring(fixedName.Length - 60);
                                    }
                                }

                                sb.Append(fixedName);

                                sb.Append("\" ON \"");
                                sb.Append(table.name);
                                sb.Append("\" (");

                                fieldsForIndex = new List<Field>();

                                // First add the real index fields
                                if (this.indexFields != null && this.indexFields.Count > 0)
                                {
                                    fieldsForIndex.AddRange(this.indexFields);
                                }


                                // Then add the covering fields
                                if (this.coveringFields != null && this.coveringFields.Count > 0)
                                {
                                    foreach (Field coveringField in this.coveringFields)
                                    {
                                        IndexField indexField = new IndexField(coveringField);

                                        fieldsForIndex.Add(indexField);
                                    }
                                }


                                foreach (IndexField indexField in fieldsForIndex)
                                {
                                    sb.Append("\"");
                                    sb.Append(indexField.name);
                                    sb.Append("\"");

                                    if (indexField.descending == true)
                                    {
                                        sb.Append(" DESC");
                                    }

                                    if (indexField != fieldsForIndex.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                sb.AppendLine(")");

                                //
                                // Add where clause to filter out nulls if creating unique index on table with nullable fields or much pain will be caused when records have null values...
                                //
                                if (this.isUnique == true && this.indexFields.Where(x => x.nullable == true).Any() == true)
                                {
                                    sb.Append(" WHERE ");

                                    List<IndexField> nullableFields = this.indexFields.Where(x => x.nullable == true).ToList();

                                    for (int i = 0; i < nullableFields.Count; i++)
                                    {
                                        sb.Append($"\"{nullableFields[i].name}\" IS NOT NULL");

                                        if (i != nullableFields.Count - 1)
                                        {
                                            sb.Append(" AND ");
                                        }
                                    }

                                    sb.AppendLine(";");
                                }
                                else
                                {
                                    sb.AppendLine(";");
                                }

                                break;

                            default:
                                throw new Exception("Unhandled Database Type");
                        }

                        return sb.ToString();
                    }
                }

                protected static bool IndexAlreadyExists(List<Index> Indexes, string value)
                {
                    //
                    // Checks the values list for the existing presence of 'value'
                    //
                    foreach (Index existingElement in Indexes)
                    {
                        if (existingElement.name == value)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                protected static bool UniqueConstraintExists(List<UniqueConstraint> ucs, string value)
                {
                    //
                    // Checks the values list for the existing presence of 'value'
                    //
                    foreach (UniqueConstraint existingElement in ucs)
                    {
                        if (existingElement.name == value)
                        {
                            return true;
                        }
                    }

                    return false;
                }


                protected static bool SortSequenceExists(List<SortSequence> sss, Field field)
                {
                    //
                    // Checks the values list for the existing presence of 'value'
                    //
                    foreach (SortSequence ss in sss)
                    {
                        if (ss.name == field.name)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                public class SortSequence : DatabaseElement
                {
                    public Field field { get; set; }
                    public bool descending { get; set; }


                    public SortSequence(Field field, bool descending = false)
                    {
                        this.field = field;
                        this.descending = descending;
                    }

                    public override string CreateSQL(DatabaseType databaseType)
                    {
                        //
                        // This doesn't affect the SQL, only the webAPI code generation, so no SQL needs to be generated.
                        //
                        return null;
                    }
                }

                public SortSequence AddSortSequence(Field field, bool descending = false)
                {
                    if (SortSequenceExists(this.sortSequences, field) == false)
                    {
                        SortSequence ss = new SortSequence(field, descending);

                        this.sortSequences.Add(ss);

                        return ss;
                    }
                    else
                    {
                        throw new Exception("Sort Sequence already exists");
                    }
                }


                public SortSequence AddSortSequence(string fieldName, bool descending = false)
                {
                    Field field = GetFieldByName(fieldName);

                    if (field == null)
                    {
                        throw new Exception("Can't get field named " + fieldName);
                    }

                    if (SortSequenceExists(this.sortSequences, field) == false)
                    {
                        SortSequence ss = new SortSequence(field, descending);

                        this.sortSequences.Add(ss);

                        return ss;
                    }
                    else
                    {
                        throw new Exception("Sort Sequence already exists");
                    }
                }


                public class UniqueConstraint : DatabaseElement
                {
                    public List<Field> fields { get; set; }

                    public UniqueConstraint(string name, Field field)
                    {
                        this.name = name;
                        this.fields = new List<Field>();
                        this.fields.Add(field);
                    }

                    public UniqueConstraint(string name, List<Field> fields)
                    {
                        this.name = name;
                        this.fields = fields;
                    }

                    public UniqueConstraint(string name)
                    {
                        this.name = name;
                        this.fields = new List<Field>();
                    }

                    public override string CreateSQL(DatabaseType databaseType)
                    {
                        StringBuilder sb = new StringBuilder();

                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                sb.Append("\tCONSTRAINT [");
                                sb.Append(this.name);
                                sb.Append("] UNIQUE ( ");

                                foreach (Field field in fields)
                                {
                                    sb.Append("[");
                                    sb.Append(field.name);
                                    sb.Append("]");
                                    if (field != fields.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                sb.Append(") ");

                                break;

                            case DatabaseType.MySQL:

                                sb.Append("\tUNIQUE `" + this.name + "_Unique`( ");
                                foreach (Field field in fields)
                                {
                                    sb.Append("`");
                                    sb.Append(field.name);
                                    sb.Append("`");
                                    if (field != fields.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                sb.Append(" ) ");

                                break;

                            case DatabaseType.PostgreSQL:

                                sb.Append("\tCONSTRAINT \"");
                                sb.Append(this.name);
                                sb.Append("\" UNIQUE ( ");
                                foreach (Field field in fields)
                                {
                                    sb.Append("\"");
                                    sb.Append(field.name);
                                    sb.Append("\"");
                                    if (field != fields.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                sb.Append(") ");

                                break;

                            case DatabaseType.SQLite:

                                sb.Append("\tUNIQUE ( ");
                                foreach (Field field in fields)
                                {
                                    sb.Append("\"");
                                    sb.Append(field.name);
                                    sb.Append("\"");
                                    if (field != fields.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                sb.Append(") ");

                                break;

                            default:

                                throw new Exception("Unhandled Database Type");
                        }

                        return sb.ToString();
                    }
                }


                public void SetDisplayNameField(string fieldName)
                {
                    Field field = GetFieldByName(fieldName);

                    if (field != null)
                    {
                        if (this.displayNameFieldList.Contains(field) == false)
                        {
                            this.displayNameFieldList.Add(field);
                        }
                    }
                    else
                    {
                        throw new Exception("Cannot set display name field because field name provided can't be found as a field.");
                    }
                }


                public void SetDisplayNameField(Field field)
                {
                    if (field != null)
                    {
                        if (this.displayNameFieldList.Contains(field) == false)
                        {
                            this.displayNameFieldList.Add(field);
                        }
                    }
                    else
                    {
                        throw new Exception("Cannot set display name field because provided field is null.");
                    }
                }


                public void SetPrimaryKeyField(string fieldName)
                {
                    Field field = GetFieldByName(fieldName);

                    if (field != null)
                    {
                        this.primaryKeyField = field;
                    }
                    else
                    {
                        throw new Exception("Cannot set primary key field because provided field is null.");
                    }
                }


                public void SetPrimaryKeyField(Field field)
                {
                    if (field != null)
                    {
                        this.primaryKeyField = field;
                    }
                    else
                    {
                        throw new Exception("Cannot set primary key field because provided field is null.");
                    }
                }


                public class ForeignKey : DatabaseElement
                {
                    public Field field { get; set; }
                    public Table targetTable { get; set; }

                    public Table sourceTable { get; set; }

                    public ForeignKey(string name, Field field, Table targetTable, Table sourceTable)
                    {
                        this.name = name;
                        this.field = field;
                        this.targetTable = targetTable;
                        this.sourceTable = sourceTable;
                    }

                    public ForeignKey(string name)
                    {
                        this.name = name;
                    }

                    public override string CreateSQL(DatabaseType databaseType)
                    {
                        StringBuilder sb = new StringBuilder();


                        //
                        // Limit the name of the FK to 128 characters - this for SQL Server, but should be OK for all - might need further adjustment if it breaks on other DB types in the future...
                        // 
                        string foreignKeyName = this.name;


                        if (foreignKeyName.Length > 128)          // SQL Server limits FK names to 128 chars
                        {
                            //
                            // This is a less than ideal situation that risks duplicating index names, which breaks the creation of the schema.
                            //
                            // No ideal path way here.  First try to fix by removing vowels.  If that's not enough, then take from the end
                            //
                            foreignKeyName = "FK_" + RemoveVowels(foreignKeyName.Substring(3));

                            //
                            // If this is still not enough, use the last 62 characters.
                            //
                            if (foreignKeyName.Length > 128)
                            {
                                foreignKeyName = "FK_" + foreignKeyName.Substring(foreignKeyName.Length - 125);
                            }
                        }


                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                sb.Append("\tCONSTRAINT [");
                                sb.Append(foreignKeyName);
                                sb.Append("] FOREIGN KEY ([");
                                sb.Append(field.name);
                                if (this.targetTable.database.disableSchemaName == false)
                                {
                                    sb.Append("]) REFERENCES [" + this.targetTable.database._schemaName + "].[");
                                }
                                else
                                {
                                    sb.Append("]) REFERENCES [");
                                }
                                sb.Append(targetTable.name);
                                sb.Append("] ([" + this.targetTable.primaryKeyField.name + "])");

                                break;

                            case DatabaseType.MySQL:

                                sb.Append("\tFOREIGN KEY (`");
                                sb.Append(field.name);
                                sb.Append("`) REFERENCES `");
                                sb.Append(targetTable.name);
                                sb.Append("`(`" + this.targetTable.primaryKeyField.name + "`)");

                                break;

                            case DatabaseType.PostgreSQL:

                                sb.Append("\tCONSTRAINT \"");
                                sb.Append(field.name);
                                sb.Append("\" FOREIGN KEY (\"");
                                sb.Append(field.name);

                                if (this.targetTable.database.disableSchemaName == false)
                                {
                                    sb.Append("\") REFERENCES \"" + this.targetTable.database._schemaName + "\".\"");
                                }
                                else
                                {
                                    sb.Append("\") REFERENCES \"");
                                }
                                sb.Append(targetTable.name);
                                sb.Append("\"(\"" + this.targetTable.primaryKeyField.name + "\")");

                                break;

                            case DatabaseType.SQLite:

                                sb.Append("\tFOREIGN KEY (\"");
                                sb.Append(field.name);
                                sb.Append("\") REFERENCES \"");
                                sb.Append(targetTable.name);
                                sb.Append("\"(\"" + this.targetTable.primaryKeyField.name + "\")");

                                break;

                            default:

                                throw new Exception("Unhandled Database Type");
                        }

                        return sb.ToString();
                    }
                }

                protected static bool ForeignKeyAlreadyExists(List<ForeignKey> ForeignKeys, string value)
                {
                    //
                    // Checks the values list for the existing presence of 'value'
                    //
                    foreach (ForeignKey existingElement in ForeignKeys)
                    {
                        if (existingElement.name == value)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                public Database database { get; set; }

                public List<Field> fields { get; set; }
                public List<Index> indexes { get; set; }

                public List<Dictionary<string, string>> data { get; set; }

                public List<ForeignKey> foreignKeys { get; set; }

                public List<UniqueConstraint> uniqueConstraints { get; set; }

                public List<SortSequence> sortSequences { get; set; }


                //
                // These are instructions for the code generators more than the SQL generation.  
                //
                public bool isWritable { get; set; }                // Some tables may not ever need to be written to from the UI, regardless of user role.  If that's the case, make this false.  Default is true
                public bool adminAccessNeededToWrite { get; set; }  // assuming the isWritable is true, this controls whether or not admin access is needed to write to this table, as opposed to just write access

                public Field primaryKeyField { get; set; }


                public List<Field> displayNameFieldList { get; set; }

                public string displayNameForTable { get; set; }

                public bool canBeFavourited { get; set; }

                public int defaultPageSizeForListGetters { get; set; }

                public bool consolidateInserts { get; set; }

                public bool existenceCheckBeforeInsert { get; set; }


                public bool? enableIdentityInsert { get; set; }      // table level override for identity inserting

                /// <summary>
                /// 
                ///  Use this to get a list of sort sequences.  This will create a reasonable set to use if the database definition itself didn't specify sort values.
                /// 
                /// </summary>
                /// <returns></returns>
                public List<SortSequence> GetOrGenerateSortSequences()
                {
                    if (this.sortSequences.Count == 0)
                    {
                        //
                        // Create sort sequences.  
                        //

                        //
                        // Do we have a sequence field?
                        //
                        Field sequenceField = this.GetFieldByName("sequence");

                        if (sequenceField != null)
                        {
                            this.sortSequences.Add(new SortSequence(sequenceField, false));
                        }


                        //
                        // After sequence, put in the display names
                        //
                        if (this.displayNameFieldList.Count > 0)
                        {
                            foreach (Field field in this.displayNameFieldList)
                            {
                                this.sortSequences.Add(new SortSequence(field, false));
                            }
                        }
                        else
                        {
                            List<Field> stringFields = this.GetStringFields();

                            if (stringFields != null && stringFields.Count > 0)
                            {
                                //
                                // Sort by first 3 string fields.
                                //

                                for (int i = 0; i < stringFields.Count && i < 3; i++)
                                {
                                    Field field = stringFields[i];

                                    this.sortSequences.Add(new SortSequence(field, false));
                                }
                            }
                            else
                            {
                                //
                                //  As a last resort, sort by id.
                                // 
                                this.sortSequences.Add(new SortSequence(this.GetFieldByName("id"), false));
                            }
                        }
                    }

                    return this.sortSequences;
                }


                public Field GetFirstStringField()
                {
                    foreach (var field in this.fields)
                    {
                        if ((field.dataType == DataType.STRING_10 ||
                            field.dataType == DataType.STRING_100 ||
                            field.dataType == DataType.STRING_1000 ||
                            field.dataType == DataType.STRING_250 ||
                            field.dataType == DataType.STRING_2000 ||
                            field.dataType == DataType.STRING_50 ||
                            field.dataType == DataType.STRING_500 ||
                            field.dataType == DataType.STRING_850 ||
                            field.dataType == DataType.STRING_HTML_COLOR ||
                            field.dataType == DataType.URI) &&
                            field.name.ToUpper().EndsWith("GUID") == false) // temporary hack while I change data types around
                        {
                            return field;
                        }
                    }

                    return null;
                }

                public List<Field> GetStringFields()
                {
                    List<Field> output = new List<Field>();

                    foreach (var field in this.fields)
                    {
                        if ((field.dataType == DataType.STRING_10 ||
                            field.dataType == DataType.STRING_100 ||
                            field.dataType == DataType.STRING_1000 ||
                            field.dataType == DataType.STRING_250 ||
                            field.dataType == DataType.STRING_2000 ||
                            field.dataType == DataType.STRING_50 ||
                            field.dataType == DataType.STRING_500 ||
                            field.dataType == DataType.STRING_850 ||
                            field.dataType == DataType.STRING_HTML_COLOR ||
                            field.dataType == DataType.URI) &&
                            field.name.ToUpper().EndsWith("GUID") == false) // temporary hack while I change data types around
                        {
                            output.Add(field);
                        }
                    }

                    return output;
                }



                //
                // These are to be used when it is known ahead of time which functions will be manually overridden.  This way, the generator can comment these out during the time of code generation to save that manual step later when pasting in 
                // updated auto generated code.
                //
                public bool mvcDefineFieldsToBeOverridden { get; set; }
                public bool mvcDefineChildenEntitiesToBeOverridden { get; set; }
                public bool webAPIListGetterToBeOverridden { get; set; }
                public bool webAPIIdGetterToBeOverridden { get; set; }
                public bool webAPIPostToBeOverridden { get; set; }
                public bool webAPIPutToBeOverridden { get; set; }
                public bool webAPIDeleteToBeOverridden { get; set; }
                public bool webAPIRollbackToBeOverridden { get; set; }
                public bool webAPIGetListDataToBeOverridden { get; set; }
                public bool webAPIGetRowCountToBeOverridden { get; set; }

                public int minimumReadPermissionLevel { get; set; }
                public int minimumWritePermissionLevel { get; set; }

                public int commandTimeoutSeconds { get; set; }

                //
                // A table can only have one pdf, png, or mp4 'field'.  Each 'field' is expressed as a group of 3.  The filename, the size, and the data.
                //
                public string pdfRootFieldName { get; private set; }
                public string pngRootFieldName { get; private set; }
                public string mp4RootFieldName { get; private set; }
                public string binaryDataRootFieldName { get; private set; }

                /// <summary>
                /// The purpose of this is to allow a table to be selectively excluded from code generation (aside from SQL generation).  Useful for tables for external systems like OpenId where the schema needs OpenId tables in it, but they're not managed at all by our logic, and we don't want to grant visibility to them.
                /// </summary>
                public bool excludeFromCodeGeneration { get; set; }
                public bool AddAnyStringContainsParameterToWebAPI { get; set; }

                public void SetCodeGeneratorOverideExpectedRules(bool mvcDefineFieldsToBeOverridden = false,
                    bool mvcDefineChildrenEntitiesToBeOverridden = false,
                    bool webAPIListGetterToBeOverridden = false,
                    bool webAPIIdGetterToBeOverridden = false,
                    bool webAPIPostToBeOverridden = false,
                    bool webAPIPutToBeOverridden = false,
                    bool webAPIDeleteToBeOverridden = false,
                    bool webAPIGetListDataToBeOverridden = false,
                    bool webAPIGetRowCountToBeOverridden = false)
                {
                    this.mvcDefineFieldsToBeOverridden = mvcDefineFieldsToBeOverridden;
                    this.mvcDefineChildenEntitiesToBeOverridden = mvcDefineChildrenEntitiesToBeOverridden;
                    this.webAPIListGetterToBeOverridden = webAPIListGetterToBeOverridden;
                    this.webAPIIdGetterToBeOverridden = webAPIIdGetterToBeOverridden;
                    this.webAPIPostToBeOverridden = webAPIPostToBeOverridden;
                    this.webAPIPutToBeOverridden = webAPIPutToBeOverridden;
                    this.webAPIDeleteToBeOverridden = webAPIDeleteToBeOverridden;
                    this.webAPIGetListDataToBeOverridden = webAPIGetListDataToBeOverridden;
                    this.webAPIGetRowCountToBeOverridden = webAPIGetRowCountToBeOverridden;

                    return;
                }

                public Table()
                {
                    fields = new List<Field>();
                    indexes = new List<Index>();
                    foreignKeys = new List<ForeignKey>();
                    uniqueConstraints = new List<UniqueConstraint>();
                    sortSequences = new List<SortSequence>();
                    data = new List<Dictionary<string, string>>();

                    isWritable = true;
                    adminAccessNeededToWrite = false;
                    displayNameFieldList = new List<Field>();

                    consolidateInserts = false;
                    existenceCheckBeforeInsert = false;

                    minimumReadPermissionLevel = 0;
                    minimumWritePermissionLevel = 0;

                    canBeFavourited = false;

                    defaultPageSizeForListGetters = 0;      // 0 means all data.

                    enableIdentityInsert = null;

                    commandTimeoutSeconds = 30;

                    excludeFromCodeGeneration = false;
                    AddAnyStringContainsParameterToWebAPI = true;

                    SetCodeGeneratorOverideExpectedRules(); // default all expected overrides to be false so all code gets generated
                }

                public Table(string name)
                {
                    this.name = name;

                    fields = new List<Field>();
                    indexes = new List<Index>();
                    foreignKeys = new List<ForeignKey>();
                    uniqueConstraints = new List<UniqueConstraint>();
                    sortSequences = new List<SortSequence>();
                    data = new List<Dictionary<string, string>>();

                    isWritable = true;
                    adminAccessNeededToWrite = false;
                    displayNameFieldList = new List<Field>();

                    minimumReadPermissionLevel = 0;
                    minimumWritePermissionLevel = 0;

                    canBeFavourited = false;

                    defaultPageSizeForListGetters = 0;      // 0 means all data.

                    commandTimeoutSeconds = 30;

                    excludeFromCodeGeneration = false;
                    AddAnyStringContainsParameterToWebAPI = true;

                    SetCodeGeneratorOverideExpectedRules(); // default all expected overrides to be false so all code gets generated
                }

                public string DropSQL(DatabaseType databaseType)
                {
                    StringBuilder sb = new StringBuilder();

                    switch (databaseType)
                    {
                        case DatabaseType.MSSQLServer:

                            sb.Append("-- DROP TABLE [" + this.database._schemaName + "].[");
                            sb.Append(this.name);
                            sb.AppendLine("]");

                            break;

                        case DatabaseType.MySQL:

                            sb.Append("-- DROP TABLE `");
                            sb.Append(this.name);
                            sb.Append("`");
                            sb.AppendLine();

                            break;

                        case DatabaseType.PostgreSQL:

                            sb.Append("-- DROP TABLE \"");
                            sb.Append(this.database.name);
                            sb.Append("\".\"");
                            sb.Append(this.name);
                            sb.AppendLine("\"");

                            break;

                        case DatabaseType.SQLite:

                            sb.Append("-- DROP TABLE \"");
                            //sb.Append(this.database.name);
                            //sb.Append("\".\"");
                            sb.Append(this.name);
                            sb.AppendLine("\"");

                            break;

                        default:

                            throw new Exception("Unhandled Database Type");
                    }

                    return sb.ToString();

                }


                public string DisableIndexSQL(DatabaseType databaseType)
                {
                    StringBuilder sb = new StringBuilder();

                    switch (databaseType)
                    {
                        case DatabaseType.MSSQLServer:

                            sb.Append("-- ALTER INDEX ALL ON [" + this.database._schemaName + "].[");
                            sb.Append(this.name);
                            sb.AppendLine("] DISABLE");
                            break;

                        case DatabaseType.MySQL:

                            sb.Append("-- ALTER INDEX ALL ON `");
                            sb.Append(this.name);
                            sb.AppendLine("` DISABLE");

                            break;

                        case DatabaseType.PostgreSQL:

                            sb.Append("-- ALTER INDEX ALL ON \"");
                            sb.Append(this.name);
                            sb.AppendLine("\" DISABLE");

                            break;

                        case DatabaseType.SQLite:

                            sb.Append("-- ALTER INDEX ALL ON \"");
                            sb.Append(this.name);
                            sb.AppendLine("\" DISABLE");

                            break;

                        default:

                            throw new Exception("Unhandled Database Type");
                    }

                    return sb.ToString();
                }




                public string EnableIndexSQL(DatabaseType databaseType)
                {
                    StringBuilder sb = new StringBuilder();

                    switch (databaseType)
                    {
                        case DatabaseType.MSSQLServer:

                            sb.Append("-- ALTER INDEX ALL ON [" + this.database._schemaName + "].[");
                            sb.Append(this.name);
                            sb.AppendLine("] REBUILD");

                            break;

                        case DatabaseType.MySQL:

                            sb.Append("-- ALTER INDEX ALL ON `");
                            sb.Append(this.name);
                            sb.AppendLine("` REBUILD");

                            break;

                        case DatabaseType.PostgreSQL:

                            sb.Append("-- ALTER INDEX ALL ON \"");
                            sb.Append(this.name);
                            sb.AppendLine("\" REBUILD");

                            break;

                        case DatabaseType.SQLite:

                            sb.Append("-- ALTER INDEX ALL ON \"");
                            sb.Append(this.name);
                            sb.AppendLine("\" REBUILD");

                            break;

                        default:

                            throw new Exception("Unhandled Database Type");
                    }

                    return sb.ToString();
                }



                public override string CreateSQL(DatabaseType databaseType)
                {
                    StringBuilder sb = new StringBuilder();


                    if (this.database.disableTableCreation == false && string.IsNullOrEmpty(this.comment) == false)
                    {
                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:
                            case DatabaseType.MySQL:
                            case DatabaseType.PostgreSQL:
                            case DatabaseType.SQLite:

                                if (this.database.disableComments == false)
                                {
                                    WriteComments(sb);
                                }

                                break;

                            default:
                                throw new Exception("Unsupported database type");
                        }
                    }

                    if (this.database.disableTableCreation == false)
                    {
                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                if (this.database.disableSchemaName == false)
                                {
                                    sb.Append("CREATE TABLE [" + this.database._schemaName + "].[");
                                }
                                else
                                {
                                    sb.Append("CREATE TABLE [");
                                }

                                sb.Append(this.name);
                                sb.AppendLine("]");
                                sb.AppendLine("(");

                                break;

                            case DatabaseType.MySQL:

                                sb.Append("CREATE TABLE `");
                                sb.Append(this.name);
                                sb.Append("`");
                                sb.AppendLine("(");

                                break;

                            case DatabaseType.PostgreSQL:

                                sb.Append("CREATE TABLE \"");
                                if (this.database.disableSchemaName == false)
                                {
                                    sb.Append(this.database.name);
                                    sb.Append("\".\"");
                                }
                                else
                                {
                                    sb.Append("\"");
                                }
                                sb.Append(this.name);
                                sb.AppendLine("\"");
                                sb.AppendLine("(");

                                break;

                            case DatabaseType.SQLite:

                                sb.Append("CREATE TABLE \"");

                                //if (this.database.disableSchemaName == false)
                                //{
                                //    sb.Append(this.database.name);
                                //    sb.Append("\".\"");
                                //}
                                sb.Append(this.name);
                                sb.AppendLine("\"");
                                sb.AppendLine("(");

                                break;

                            default:

                                throw new Exception("Unhandled Database Type");
                        }

                        foreach (Database.Table.Field field in this.fields)
                        {
                            sb.Append(field.CreateSQL(databaseType));

                            if (field != this.fields.Last()
                                || (databaseType == DatabaseType.MySQL && this.foreignKeys.Count > 0)        // MySQL needs a trailing comma on the last field before the FKs start
                                || (databaseType == DatabaseType.PostgreSQL && this.foreignKeys.Count > 0)  // PostgreSQL needs a trailing comma on the last field before the FKs start
                                || (databaseType == DatabaseType.SQLite && this.foreignKeys.Count > 0)      // SQLite needs a trailing comma on the last field before the FKs start
                                || (databaseType == DatabaseType.MySQL && this.uniqueConstraints.Count > 0)        // MySQL needs a trailing comma on the last field before the unique keys start
                                || (databaseType == DatabaseType.PostgreSQL && this.uniqueConstraints.Count > 0)  // PostgreSQL needs a trailing comma on the last field before the UKs start
                                || (databaseType == DatabaseType.SQLite && this.uniqueConstraints.Count > 0))     // SQLite needs a trailing comma on the last field before the Uks start

                            {
                                // If the field has a comment, then put it on after the comma separating the fields
                                if (string.IsNullOrEmpty(field.comment) != true)
                                {
                                    sb.Append(",");

                                    switch (databaseType)
                                    {
                                        case DatabaseType.MSSQLServer:
                                        case DatabaseType.MySQL:
                                        case DatabaseType.PostgreSQL:
                                        case DatabaseType.SQLite:

                                            field.WriteComments(sb, 2);

                                            break;

                                        default:
                                            throw new Exception("Unsupported database type");
                                    }
                                }
                                else
                                {
                                    // no comment
                                    sb.AppendLine(",");
                                }

                            }
                            else
                            {
                                switch (databaseType)
                                {
                                    case DatabaseType.MSSQLServer:
                                    case DatabaseType.MySQL:
                                    case DatabaseType.PostgreSQL:
                                    case DatabaseType.SQLite:

                                        field.WriteComments(sb, 2);

                                        break;

                                    default:
                                        throw new Exception("Unsupported database type");
                                }

                                sb.AppendLine();
                            }
                        }

                        if (this.foreignKeys != null && this.foreignKeys.Count > 0)
                        {
                            foreach (Database.Table.ForeignKey fk in this.foreignKeys)
                            {
                                sb.Append(fk.CreateSQL(databaseType));

                                if (fk != this.foreignKeys.Last() || this.uniqueConstraints.Count > 0)
                                {
                                    sb.Append(",");
                                }

                                if (string.IsNullOrEmpty(fk.comment) == false)
                                {
                                    switch (databaseType)
                                    {
                                        case DatabaseType.MSSQLServer:
                                        case DatabaseType.MySQL:
                                        case DatabaseType.PostgreSQL:
                                        case DatabaseType.SQLite:

                                            fk.WriteComments(sb, 2);

                                            break;

                                        default:
                                            throw new Exception("Unsupported database type");
                                    }
                                }
                                else
                                {
                                    sb.AppendLine();
                                }
                            }
                        }

                        if (this.uniqueConstraints != null && this.uniqueConstraints.Count > 0)
                        {
                            foreach (Database.Table.UniqueConstraint uc in this.uniqueConstraints)
                            {
                                sb.Append(uc.CreateSQL(databaseType));

                                if (uc != this.uniqueConstraints.Last())
                                {
                                    sb.Append(",");
                                }

                                if (string.IsNullOrEmpty(uc.comment) == false)
                                {
                                    switch (databaseType)
                                    {
                                        case DatabaseType.MSSQLServer:
                                        case DatabaseType.MySQL:
                                        case DatabaseType.PostgreSQL:
                                        case DatabaseType.SQLite:

                                            uc.WriteComments(sb, 2);

                                            break;

                                        default:
                                            throw new Exception("Unsupported database type");
                                    }
                                }
                                else
                                {
                                    sb.AppendLine();
                                }
                            }
                        }


                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                sb.AppendLine(")");
                                sb.AppendLine("GO");
                                sb.AppendLine("");

                                break;

                            case DatabaseType.MySQL:

                                sb.AppendLine(");");

                                break;

                            case DatabaseType.PostgreSQL:

                                sb.AppendLine(");");

                                break;

                            case DatabaseType.SQLite:

                                sb.AppendLine(");");

                                break;

                            default:
                                throw new Exception("Unhandled Database Type");
                        }

                        //
                        // Add the table indexes right after the table creation
                        //
                        foreach (Database.Table.Index index in this.indexes)
                        {
                            sb.AppendLine(index.CreateSQL(databaseType));
                        }
                    }

                    //
                    // Add the data after the table creation
                    //
                    sb.AppendLine(CreateInsertSQLFromData(databaseType, data));

                    return sb.ToString();
                }

                //
                // The purpose of this function to is create a list of commands that can be executed one at a time by a program, instead of the above version which
                // is more intended to output into a big script to be executed as part of a big batch outside of a program.
                //
                public string CreateInsertSQLFromData(DatabaseType databaseType, List<Dictionary<string, string>> data, bool validateFieldExistence = true)
                {
                    if (data == null || data.Count == 0)
                    {
                        return "";
                    }

                    StringBuilder sb = new StringBuilder();

                    if (((this.database.enableIdentityInsert == true && this.enableIdentityInsert.HasValue == false) || (this.enableIdentityInsert.HasValue == true && this.enableIdentityInsert == true)) && HasIdentityField() == true)
                    {
                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                sb.AppendLine("SET IDENTITY_INSERT [" + this.database._schemaName + "].[" + this.name + "] ON");

                                sb.AppendLine("GO");

                                sb.AppendLine();
                                break;

                            default:

                                // 
                                // For now only SQL server can enable identity insert.
                                // 
                                break;
                        }
                    }

                    int insertCounter = 0;                      // can only consolidate up to 1000 rows, so restart after 1000

                    foreach (Dictionary<string, string> dictionary in data)
                    {
                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                if (this.consolidateInserts == false ||
                                    insertCounter == 0 ||
                                    existenceCheckBeforeInsert == true)
                                {
                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        string formattedFieldData = "";

                                        switch (this.primaryKeyField.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                if (formattedFieldData != null)
                                                {
                                                    formattedFieldData = "'" + formattedFieldData.Replace("'", "''") + "'";
                                                }
                                                else
                                                {
                                                    formattedFieldData = null;
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }

                                        if (this.database.disableSchemaName == false)
                                        {
                                            sb.AppendLine("IF NOT EXISTS ( SELECT 1 FROM [" + this.database._schemaName + "].[" + this.name + "] WHERE " + this.primaryKeyField.name + " = " + formattedFieldData + " )");
                                        }
                                        else
                                        {
                                            sb.AppendLine("IF NOT EXISTS ( SELECT 1 FROM [" + this.name + "] WHERE " + this.primaryKeyField.name + " = " + formattedFieldData + " )");
                                        }

                                        sb.AppendLine("BEGIN");
                                        sb.Append("\t");
                                    }


                                    if (this.database.disableSchemaName == false)
                                    {
                                        sb.Append("INSERT INTO [" + this.database._schemaName + "].[" + this.name + "] ( ");
                                    }
                                    else
                                    {
                                        sb.Append("INSERT INTO [" + this.name + "] ( ");
                                    }

                                    foreach (string fieldName in dictionary.Keys)
                                    {
                                        if (fieldName.StartsWith("link:") == true)
                                        {
                                            // link field
                                            string[] linkData = fieldName.Split(':');

                                            if (linkData.Length == 4)
                                            {
                                                string linkField = linkData[3];
                                                sb.Append("[");
                                                sb.Append(linkField);
                                                sb.Append("]");
                                            }
                                            else
                                            {
                                                throw new Exception("Unsupported link syntax.");
                                            }
                                        }
                                        else
                                        {
                                            // regular field
                                            sb.Append("[");
                                            sb.Append(fieldName);
                                            sb.Append("]");
                                        }

                                        if (fieldName != dictionary.Keys.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.Append(" ) VALUES ");
                                }

                                //
                                // Increment this after we've managed the initializer line because that checks this value for 0.
                                //
                                insertCounter++;

                                sb.Append(" ( ");

                                foreach (string fieldName in dictionary.Keys)
                                {
                                    if (fieldName.StartsWith("link:") == true)
                                    {
                                        string[] linkData = fieldName.Split(':');

                                        if (linkData.Length == 4)
                                        {
                                            string linkTable = linkData[1];
                                            string linkTableField = linkData[2];
                                            string linkField = linkData[3];
                                            string lookupValue = dictionary[fieldName];

                                            //
                                            // SQL Escape the lookup value
                                            //
                                            if (lookupValue != null)
                                            {
                                                lookupValue = lookupValue.Replace("'", "''");
                                            }


                                            sb.Append("( ");

                                            sb.Append("SELECT TOP 1 id FROM [" + this.database._schemaName + "].[" + linkTable + "] WHERE [");
                                            sb.Append(linkTableField);
                                            sb.Append("] = '");
                                            sb.Append(lookupValue);        // lookup value must be a string field..
                                            sb.Append("' )");
                                        }
                                        else
                                        {
                                            throw new Exception("Unsupported link syntax.");
                                        }
                                    }
                                    else
                                    {
                                        Field field = null;

                                        if (validateFieldExistence == true)
                                        {
                                            field = GetFieldByName(fieldName);
                                        }
                                        else
                                        {
                                            // assume string field if running in non-validation mode
                                            field = new Field();
                                            field.dataType = DataType.STRING_1000;
                                        }


                                        if (field == null)
                                        {
                                            throw new Exception("Could not get field.  Name is " + fieldName);
                                        }


                                        switch (field.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                string value = dictionary[fieldName];

                                                if (value != null)
                                                {
                                                    sb.Append("'");
                                                    sb.Append(value.Replace("'", "''"));
                                                    sb.Append("'");
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.BIG_INTEGER:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.FOREIGN_KEY:

                                                string numericValue = dictionary[fieldName];

                                                if (numericValue != null)
                                                {
                                                    sb.Append(numericValue);
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;


                                            case DataType.BOOL:

                                                string boolValue = dictionary[fieldName];

                                                if (boolValue != null)
                                                {
                                                    //
                                                    // We generally expect a 1 or 0 here, but if we get a boolean looking string, convert it to a 1 or a 0.
                                                    //
                                                    boolValue = boolValue.Trim().ToUpper();

                                                    if (boolValue == "TRUE" ||
                                                        boolValue == "YES" ||
                                                        boolValue == "T" ||
                                                        boolValue == "Y")
                                                    {
                                                        boolValue = "1";
                                                    }
                                                    else if (boolValue == "FALSE" ||
                                                        boolValue == "NO" ||
                                                        boolValue == "F" ||
                                                        boolValue == "N")
                                                    {
                                                        boolValue = "0";
                                                    }


                                                    sb.Append(boolValue);
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }
                                    }


                                    if (fieldName != dictionary.Keys.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.consolidateInserts == false || this.existenceCheckBeforeInsert == true)
                                {
                                    sb.AppendLine(" )");

                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        sb.AppendLine("END");
                                    }

                                    sb.AppendLine("GO");
                                }
                                else
                                {
                                    if (insertCounter == 1000)
                                    {
                                        //
                                        // restart after 1000.
                                        //
                                        sb.AppendLine(" )");


                                        sb.AppendLine("GO");


                                        insertCounter = 0;
                                    }
                                    else
                                    {
                                        if (dictionary != data.Last())
                                        {
                                            sb.Append(" ),");
                                        }
                                        else
                                        {
                                            sb.AppendLine(" )");

                                            sb.AppendLine("GO");
                                        }
                                    }
                                }

                                break;

                            case DatabaseType.MySQL:

                                if (this.consolidateInserts == false ||
                                    insertCounter == 0 ||
                                    existenceCheckBeforeInsert == true)
                                {
                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        sb.Append("INSERT IGNORE INTO `" + this.name + "` ( ");
                                    }
                                    else
                                    {
                                        sb.Append("INSERT INTO `" + this.name + "` ( ");
                                    }


                                    foreach (string fieldName in dictionary.Keys)
                                    {

                                        if (fieldName.StartsWith("link:") == true)
                                        {
                                            // link field
                                            string[] linkData = fieldName.Split(':');

                                            if (linkData.Length == 4)
                                            {
                                                string linkField = linkData[3];
                                                sb.Append("`");
                                                sb.Append(linkField);
                                                sb.Append("`");
                                            }
                                            else
                                            {
                                                throw new Exception("Unsupported link syntax.");
                                            }
                                        }
                                        else
                                        {
                                            // regular field
                                            sb.Append("`");
                                            sb.Append(fieldName);
                                            sb.Append("`");
                                        }

                                        if (fieldName != dictionary.Keys.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.Append(" ) VALUES ");
                                }

                                //
                                // Increment this after we've managed the initializer line because that checks this value for 0.
                                //
                                insertCounter++;

                                sb.Append(" ( ");

                                foreach (string fieldName in dictionary.Keys)
                                {
                                    if (fieldName.StartsWith("link:") == true)
                                    {
                                        string[] linkData = fieldName.Split(':');

                                        if (linkData.Length == 4)
                                        {
                                            string linkTable = linkData[1];
                                            string linkTableField = linkData[2];
                                            string linkField = linkData[3];
                                            string lookupValue = dictionary[fieldName];

                                            //
                                            // SQL Escape the lookup value
                                            //
                                            if (lookupValue != null)
                                            {
                                                lookupValue = lookupValue.Replace("`", "``");
                                            }

                                            sb.Append("( ");

                                            sb.Append("SELECT id FROM `" + linkTable + "` WHERE `");
                                            sb.Append(linkTableField);
                                            sb.Append("` = '");
                                            sb.Append(lookupValue);        // lookup value must be a string field..
                                            sb.Append("' LIMIT 1 )");
                                        }
                                        else
                                        {
                                            throw new Exception("Unsupported link syntax.");
                                        }
                                    }
                                    else
                                    {
                                        Field field = null;

                                        if (validateFieldExistence == true)
                                        {
                                            field = GetFieldByName(fieldName);
                                        }
                                        else
                                        {
                                            // assume string field if running in non-validation mode
                                            field = new Field();
                                            field.dataType = DataType.STRING_1000;
                                        }


                                        if (field == null)
                                        {
                                            throw new Exception("Could not get field.  Name is " + fieldName);
                                        }

                                        switch (field.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                string value = dictionary[fieldName];

                                                if (value != null)
                                                {
                                                    sb.Append("'");
                                                    sb.Append(value.Replace("'", "''"));
                                                    sb.Append("'");
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.BIG_INTEGER:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                string numericValue = dictionary[fieldName];

                                                if (numericValue != null)
                                                {
                                                    sb.Append(numericValue);
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }


                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }
                                    }


                                    if (fieldName != dictionary.Keys.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.consolidateInserts == false || this.existenceCheckBeforeInsert == true)
                                {
                                    sb.AppendLine(" );");
                                }
                                else
                                {
                                    if (insertCounter == 1000)
                                    {
                                        //
                                        // restart after 1000.
                                        //
                                        sb.AppendLine(" );");
                                        insertCounter = 0;
                                    }
                                    else
                                    {
                                        if (dictionary != data.Last())
                                        {
                                            sb.Append(" ),");
                                        }
                                        else
                                        {
                                            sb.AppendLine(" );");
                                        }
                                    }
                                }

                                break;

                            case DatabaseType.PostgreSQL:

                                if (this.consolidateInserts == false ||
                                            insertCounter == 0 ||
                                            existenceCheckBeforeInsert == true)
                                {
                                    if (this.database.disableSchemaName == false)
                                    {
                                        sb.Append("INSERT INTO \"" + this.database._schemaName + "\".\"" + this.name + "\" ( ");
                                    }
                                    else
                                    {
                                        sb.Append("INSERT INTO \"" + this.name + "\" ( ");
                                    }

                                    foreach (string fieldName in dictionary.Keys)
                                    {

                                        if (fieldName.StartsWith("link:") == true)
                                        {
                                            // link field
                                            string[] linkData = fieldName.Split(':');

                                            if (linkData.Length == 4)
                                            {
                                                string linkField = linkData[3];
                                                sb.Append("\"");
                                                sb.Append(linkField);
                                                sb.Append("\"");
                                            }
                                            else
                                            {
                                                throw new Exception("Unsupported link syntax.");
                                            }
                                        }
                                        else
                                        {
                                            // regular field
                                            sb.Append("\"");
                                            sb.Append(fieldName);
                                            sb.Append("\"");
                                        }

                                        if (fieldName != dictionary.Keys.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.Append(" ) VALUES ");
                                }

                                //
                                // Increment this after we've managed the initializer line because that checks this value for 0.
                                //
                                insertCounter++;

                                sb.Append(" ( ");

                                foreach (string fieldName in dictionary.Keys)
                                {
                                    if (fieldName.StartsWith("link:") == true)
                                    {
                                        string[] linkData = fieldName.Split(':');

                                        if (linkData.Length == 4)
                                        {
                                            string linkTable = linkData[1];
                                            string linkTableField = linkData[2];
                                            string linkField = linkData[3];
                                            string lookupValue = dictionary[fieldName];

                                            //
                                            // SQL Escape the lookup value
                                            //
                                            if (lookupValue != null)
                                            {
                                                lookupValue = lookupValue.Replace("'", "''");
                                            }


                                            sb.Append("( ");

                                            sb.Append("SELECT id FROM \"" + linkTable + "\" WHERE \"");
                                            sb.Append(linkTableField);
                                            sb.Append("\" = '");
                                            sb.Append(lookupValue);        // lookup value must be a string field..
                                            sb.Append("' LIMIT 1)");
                                        }
                                        else
                                        {
                                            throw new Exception("Unsupported link syntax.");
                                        }
                                    }
                                    else
                                    {
                                        Field field = null;

                                        if (validateFieldExistence == true)
                                        {
                                            field = GetFieldByName(fieldName);
                                        }
                                        else
                                        {
                                            // assume string field if running in non-validation mode
                                            field = new Field();
                                            field.dataType = DataType.STRING_1000;
                                        }

                                        if (field == null)
                                        {
                                            throw new Exception("Could not get field.  Name is " + fieldName);
                                        }

                                        switch (field.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                string value = dictionary[fieldName];

                                                if (value != null)
                                                {
                                                    sb.Append("'");
                                                    sb.Append(value.Replace("'", "''"));
                                                    sb.Append("'");
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            case DataType.BOOL:

                                                string numericValue = dictionary[fieldName];

                                                if (numericValue != null)
                                                {
                                                    sb.Append(numericValue == "1" ? "true" : "false");
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }
                                                break;


                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.BIG_INTEGER:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.FOREIGN_KEY:

                                                string boolValue = dictionary[fieldName];

                                                if (boolValue != null)
                                                {
                                                    sb.Append(boolValue);
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }
                                    }


                                    if (fieldName != dictionary.Keys.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.consolidateInserts == false || this.existenceCheckBeforeInsert == true)
                                {
                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        sb.AppendLine(" ) ON CONFLICT DO NOTHING;");
                                    }
                                    else
                                    {
                                        sb.AppendLine(" );");
                                    }

                                }
                                else
                                {
                                    if (insertCounter == 1000)
                                    {
                                        //
                                        // restart after 1000.
                                        //
                                        insertCounter = 0;

                                        if (existenceCheckBeforeInsert == true)
                                        {
                                            sb.AppendLine(" ) ON CONFLICT DO NOTHING;");
                                        }
                                        else
                                        {
                                            sb.AppendLine(" );");
                                        }
                                    }
                                    else
                                    {
                                        if (dictionary != data.Last())
                                        {
                                            sb.Append(" ),");
                                        }
                                        else
                                        {
                                            sb.AppendLine(" );");
                                        }
                                    }
                                }


                                break;

                            case DatabaseType.SQLite:

                                if (this.consolidateInserts == false ||
                                            insertCounter == 0 ||
                                            existenceCheckBeforeInsert == true)
                                {
                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        string formattedFieldData = "";

                                        switch (this.primaryKeyField.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                if (formattedFieldData != null)
                                                {
                                                    formattedFieldData = "'" + formattedFieldData.Replace("'", "''") + "'";
                                                }
                                                else
                                                {
                                                    formattedFieldData = null;
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.BIG_INTEGER:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }

                                        //if (this.database.disableSchemaName == false)
                                        //{
                                        //    sb.AppendLine("IF NOT EXISTS ( SELECT 1 FROM \"" + this.database.name + "\".\"" + this.name + "\" WHERE " + this.primaryKeyField.name + " = " + formattedFieldData + " )");
                                        //}
                                        //else
                                        //{
                                        sb.AppendLine("IF NOT EXISTS ( SELECT 1 FROM \"" + this.name + "\" WHERE " + this.primaryKeyField.name + " = " + formattedFieldData + " )");

                                        //}
                                        sb.AppendLine("BEGIN");
                                        sb.Append("\t");
                                    }

                                    //if (this.database.disableSchemaName == false)
                                    //{
                                    //    sb.Append("INSERT INTO \"" + this.database.name + "\".\"" + this.name + "\" ( ");
                                    //}
                                    //else
                                    //{
                                    sb.Append("INSERT INTO \"" + this.name + "\" ( ");
                                    //}

                                    foreach (string fieldName in dictionary.Keys)
                                    {

                                        if (fieldName.StartsWith("link:") == true)
                                        {
                                            // link field
                                            string[] linkData = fieldName.Split(':');

                                            if (linkData.Length == 4)
                                            {
                                                string linkField = linkData[3];
                                                sb.Append("\"");
                                                sb.Append(linkField);
                                                sb.Append("\"");
                                            }
                                            else
                                            {
                                                throw new Exception("Unsupported link syntax.");
                                            }
                                        }
                                        else
                                        {
                                            // regular field
                                            sb.Append("\"");
                                            sb.Append(fieldName);
                                            sb.Append("\"");
                                        }

                                        if (fieldName != dictionary.Keys.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.Append(" ) VALUES ");
                                }

                                //
                                // Increment this after we've managed the initializer line because that checks this value for 0.
                                //
                                insertCounter++;

                                sb.Append(" ( ");

                                foreach (string fieldName in dictionary.Keys)
                                {
                                    if (fieldName.StartsWith("link:") == true)
                                    {
                                        string[] linkData = fieldName.Split(':');

                                        if (linkData.Length == 4)
                                        {
                                            string linkTable = linkData[1];
                                            string linkTableField = linkData[2];
                                            string linkField = linkData[3];
                                            string lookupValue = dictionary[fieldName];

                                            //
                                            // SQL Escape the lookup value
                                            //
                                            if (lookupValue != null)
                                            {
                                                lookupValue = lookupValue.Replace("'", "''");
                                            }


                                            sb.Append("( ");

                                            sb.Append("SELECT id FROM \"" + linkTable + "\" WHERE \"");
                                            sb.Append(linkTableField);
                                            sb.Append("\" = '");
                                            sb.Append(lookupValue);        // lookup value must be a string field..
                                            sb.Append("' LIMIT 1)");
                                        }
                                        else
                                        {
                                            throw new Exception("Unsupported link syntax.");
                                        }
                                    }
                                    else
                                    {
                                        Field field = null;

                                        if (validateFieldExistence == true)
                                        {
                                            field = GetFieldByName(fieldName);
                                        }
                                        else
                                        {
                                            // assume string field if running in non-validation mode
                                            field = new Field();
                                            field.dataType = DataType.STRING_1000;
                                        }

                                        if (field == null)
                                        {
                                            throw new Exception("Could not get field.  Name is " + fieldName);
                                        }

                                        switch (field.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                string value = dictionary[fieldName];

                                                if (value != null)
                                                {
                                                    sb.Append("'");
                                                    sb.Append(value.Replace("'", "''"));
                                                    sb.Append("'");
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.BIG_INTEGER:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                string numericValue = dictionary[fieldName];

                                                if (numericValue != null)
                                                {
                                                    sb.Append(numericValue);
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }


                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }
                                    }


                                    if (fieldName != dictionary.Keys.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.consolidateInserts == false || this.existenceCheckBeforeInsert == true)
                                {
                                    sb.AppendLine(" );");

                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        sb.AppendLine("END");
                                    }

                                }
                                else
                                {
                                    if (insertCounter == 1000)
                                    {
                                        //
                                        // restart after 1000.
                                        //
                                        sb.AppendLine(" );");
                                        insertCounter = 0;
                                    }
                                    else
                                    {
                                        if (dictionary != data.Last())
                                        {
                                            sb.Append(" ),");
                                        }
                                        else
                                        {
                                            sb.AppendLine(" );");
                                        }
                                    }
                                }


                                break;


                            default:
                                throw new Exception("Unhandled Database Type");

                        }
                        sb.AppendLine();
                    }

                    if (((this.database.enableIdentityInsert == true && this.enableIdentityInsert.HasValue == false) ||
                        (this.enableIdentityInsert.HasValue == true && this.enableIdentityInsert == true)) && HasIdentityField() == true)
                    {
                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:


                                sb.AppendLine("SET IDENTITY_INSERT [" + this.database._schemaName + "].[" + this.name + "] OFF");

                                sb.AppendLine("GO");


                                sb.AppendLine();
                                break;

                            default:
                                // 
                                // For now only SQL server can enable identity insert.
                                // 
                                break;

                        }
                    }
                    return sb.ToString();
                }




                public List<string> CreateInsertSQLCommandListFromData(DatabaseType databaseType, List<Dictionary<string, string>> data, bool validateFieldExistence = true, bool includeSchemaName = false)
                {
                    List<string> output = new List<string>();

                    if (data == null || data.Count == 0)
                    {
                        return output;
                    }

                    StringBuilder sb = new StringBuilder();

                    string schemaName = "";

                    if (this.database.enableIdentityInsert == true && HasIdentityField() == true)
                    {
                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                schemaName = "";

                                if (includeSchemaName == true)
                                {
                                    schemaName = "[" + this.database._schemaName + "].";
                                }

                                sb.AppendLine("SET IDENTITY_INSERT " + schemaName + "[" + this.name + "] ON");

                                output.Add(sb.ToString());
                                sb.Clear();

                                break;

                            default:
                                // 
                                // For now only SQL server can enable identity insert.
                                // 
                                break;

                        }
                    }

                    int insertCounter = 0;                      // can only consolidate up to 1000 rows, so restart after 1000

                    foreach (Dictionary<string, string> dictionary in data)
                    {
                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                if (includeSchemaName == true)
                                {
                                    schemaName = "[" + this.database._schemaName + "].";
                                }

                                if (this.consolidateInserts == false ||
                                    insertCounter == 0 ||
                                    existenceCheckBeforeInsert == true)
                                {
                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        string formattedFieldData = "";

                                        switch (this.primaryKeyField.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                if (formattedFieldData != null)
                                                {
                                                    formattedFieldData = "'" + formattedFieldData.Replace("'", "''") + "'";
                                                }
                                                else
                                                {
                                                    formattedFieldData = null;
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.BIG_INTEGER:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }

                                        sb.AppendLine("IF NOT EXISTS ( SELECT 1 FROM " + schemaName + "[" + this.name + "] WHERE " + this.primaryKeyField.name + " = " + formattedFieldData + " )");
                                        sb.AppendLine("BEGIN");
                                        sb.Append("\t");
                                    }

                                    if (this.database.disableSchemaName == false)
                                    {
                                        sb.Append("INSERT INTO \" + schemaName + \"[" + this.name + "] ( ");
                                    }
                                    else
                                    {
                                        sb.Append("INSERT INTO \"[" + this.name + "] ( ");
                                    }

                                    foreach (string fieldName in dictionary.Keys)
                                    {

                                        if (fieldName.StartsWith("link:") == true)
                                        {
                                            // link field
                                            string[] linkData = fieldName.Split(':');

                                            if (linkData.Length == 4)
                                            {
                                                string linkField = linkData[3];
                                                sb.Append("[");
                                                sb.Append(linkField);
                                                sb.Append("]");
                                            }
                                            else
                                            {
                                                throw new Exception("Unsupported link syntax.");
                                            }
                                        }
                                        else
                                        {
                                            // regular field
                                            sb.Append("[");
                                            sb.Append(fieldName);
                                            sb.Append("]");
                                        }

                                        if (fieldName != dictionary.Keys.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.Append(" ) VALUES ");
                                }

                                //
                                // Increment this after we've managed the initializer line because that checks this value for 0.
                                //
                                insertCounter++;

                                sb.Append(" ( ");

                                foreach (string fieldName in dictionary.Keys)
                                {
                                    if (fieldName.StartsWith("link:") == true)
                                    {
                                        string[] linkData = fieldName.Split(':');

                                        if (linkData.Length == 4)
                                        {
                                            string linkTable = linkData[1];
                                            string linkTableField = linkData[2];
                                            string linkField = linkData[3];
                                            string lookupValue = dictionary[fieldName];

                                            //
                                            // SQL Escape the lookup value
                                            //
                                            if (lookupValue != null)
                                            {
                                                lookupValue = lookupValue.Replace("'", "''");
                                            }


                                            sb.Append("( ");

                                            sb.Append("SELECT TOP 1 id FROM " + schemaName + "[" + linkTable + "] WHERE [");
                                            sb.Append(linkTableField);
                                            sb.Append("] = '");
                                            sb.Append(lookupValue);        // lookup value must be a string field..
                                            sb.Append("' )");
                                        }
                                        else
                                        {
                                            throw new Exception("Unsupported link syntax.");
                                        }
                                    }
                                    else
                                    {
                                        Field field = null;

                                        if (validateFieldExistence == true)
                                        {
                                            field = GetFieldByName(fieldName);
                                        }
                                        else
                                        {
                                            // assume string field if running in non-validation mode
                                            field = new Field();
                                            field.dataType = DataType.STRING_1000;
                                        }

                                        if (field == null)
                                        {
                                            throw new Exception("Could not get field.  Name is " + fieldName);
                                        }

                                        switch (field.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                string value = dictionary[fieldName];

                                                if (value != null)
                                                {
                                                    sb.Append("'");
                                                    sb.Append(value.Replace("'", "''"));
                                                    sb.Append("'");
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                string numericValue = dictionary[fieldName];

                                                if (numericValue != null)
                                                {
                                                    sb.Append(numericValue);
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }


                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }
                                    }


                                    if (fieldName != dictionary.Keys.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.consolidateInserts == false || this.existenceCheckBeforeInsert == true)
                                {
                                    sb.AppendLine(" )");

                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        sb.AppendLine("END");
                                    }


                                    output.Add(sb.ToString());
                                    sb.Clear();

                                }
                                else
                                {
                                    if (insertCounter == 1000)
                                    {
                                        //
                                        // restart after 1000.
                                        //
                                        sb.AppendLine(" )");


                                        output.Add(sb.ToString());
                                        sb.Clear();


                                        insertCounter = 0;
                                    }
                                    else
                                    {
                                        if (dictionary != data.Last())
                                        {
                                            sb.Append(" ),");
                                        }
                                        else
                                        {
                                            sb.AppendLine(" )");

                                            output.Add(sb.ToString());
                                            sb.Clear();
                                        }
                                    }
                                }

                                break;

                            case DatabaseType.MySQL:

                                if (this.consolidateInserts == false ||
                                    insertCounter == 0 ||
                                    existenceCheckBeforeInsert == true)
                                {
                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        sb.Append("INSERT IGNORE INTO `" + this.name + "` ( ");
                                    }
                                    else
                                    {
                                        sb.Append("INSERT INTO `" + this.name + "` ( ");
                                    }


                                    foreach (string fieldName in dictionary.Keys)
                                    {

                                        if (fieldName.StartsWith("link:") == true)
                                        {
                                            // link field
                                            string[] linkData = fieldName.Split(':');

                                            if (linkData.Length == 4)
                                            {
                                                string linkField = linkData[3];
                                                sb.Append("`");
                                                sb.Append(linkField);
                                                sb.Append("`");
                                            }
                                            else
                                            {
                                                throw new Exception("Unsupported link syntax.");
                                            }
                                        }
                                        else
                                        {
                                            // regular field
                                            sb.Append("`");
                                            sb.Append(fieldName);
                                            sb.Append("`");
                                        }

                                        if (fieldName != dictionary.Keys.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.Append(" ) VALUES ");
                                }

                                //
                                // Increment this after we've managed the initializer line because that checks this value for 0.
                                //
                                insertCounter++;

                                sb.Append(" ( ");

                                foreach (string fieldName in dictionary.Keys)
                                {
                                    if (fieldName.StartsWith("link:") == true)
                                    {
                                        string[] linkData = fieldName.Split(':');

                                        if (linkData.Length == 4)
                                        {
                                            string linkTable = linkData[1];
                                            string linkTableField = linkData[2];
                                            string linkField = linkData[3];
                                            string lookupValue = dictionary[fieldName];

                                            //
                                            // SQL Escape the lookup value
                                            //
                                            if (lookupValue != null)
                                            {
                                                lookupValue = lookupValue.Replace("`", "``");
                                            }


                                            sb.Append("( ");

                                            sb.Append("SELECT id FROM `" + linkTable + "` WHERE `");
                                            sb.Append(linkTableField);
                                            sb.Append("` = '");
                                            sb.Append(lookupValue);        // lookup value must be a string field..
                                            sb.Append("' LIMIT 1 )");
                                        }
                                        else
                                        {
                                            throw new Exception("Unsupported link syntax.");
                                        }
                                    }
                                    else
                                    {
                                        Field field = null;

                                        if (validateFieldExistence == true)
                                        {
                                            field = GetFieldByName(fieldName);
                                        }
                                        else
                                        {
                                            // assume string field if running in non-validation mode
                                            field = new Field();
                                            field.dataType = DataType.STRING_1000;
                                        }


                                        if (field == null)
                                        {
                                            throw new Exception("Could not get field.  Name is " + fieldName);
                                        }

                                        switch (field.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                string value = dictionary[fieldName];

                                                if (value != null)
                                                {
                                                    sb.Append("'");
                                                    sb.Append(value.Replace("'", "''"));
                                                    sb.Append("'");
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.BIG_INTEGER:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                string numericValue = dictionary[fieldName];

                                                if (numericValue != null)
                                                {
                                                    sb.Append(numericValue);
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }


                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }
                                    }


                                    if (fieldName != dictionary.Keys.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.consolidateInserts == false || this.existenceCheckBeforeInsert == true)
                                {
                                    sb.AppendLine(" );");

                                    output.Add(sb.ToString());
                                    sb.Clear();
                                }
                                else
                                {
                                    if (insertCounter == 1000)
                                    {
                                        //
                                        // restart after 1000.
                                        //
                                        sb.AppendLine(" );");
                                        insertCounter = 0;

                                        output.Add(sb.ToString());
                                        sb.Clear();
                                    }
                                    else
                                    {
                                        if (dictionary != data.Last())
                                        {
                                            sb.Append(" ),");
                                        }
                                        else
                                        {
                                            sb.AppendLine(" );");

                                            output.Add(sb.ToString());
                                            sb.Clear();
                                        }
                                    }
                                }

                                break;

                            case DatabaseType.PostgreSQL:

                                if (this.consolidateInserts == false ||
                                            insertCounter == 0 ||
                                            existenceCheckBeforeInsert == true)
                                {
                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        string formattedFieldData = "";

                                        switch (this.primaryKeyField.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                if (formattedFieldData != null)
                                                {
                                                    formattedFieldData = "'" + formattedFieldData.Replace("'", "''") + "'";
                                                }
                                                else
                                                {
                                                    formattedFieldData = null;
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.BIG_INTEGER:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }

                                        sb.AppendLine("IF NOT EXISTS ( SELECT 1 FROM \"" + this.name + "\" WHERE " + this.primaryKeyField.name + " = " + formattedFieldData + " )");
                                        sb.AppendLine("BEGIN");
                                        sb.Append("\t");
                                    }

                                    sb.Append("INSERT INTO \"" + this.name + "\" ( ");

                                    foreach (string fieldName in dictionary.Keys)
                                    {

                                        if (fieldName.StartsWith("link:") == true)
                                        {
                                            // link field
                                            string[] linkData = fieldName.Split(':');

                                            if (linkData.Length == 4)
                                            {
                                                string linkField = linkData[3];
                                                sb.Append("\"");
                                                sb.Append(linkField);
                                                sb.Append("\"");
                                            }
                                            else
                                            {
                                                throw new Exception("Unsupported link syntax.");
                                            }
                                        }
                                        else
                                        {
                                            // regular field
                                            sb.Append("\"");
                                            sb.Append(fieldName);
                                            sb.Append("\"");
                                        }

                                        if (fieldName != dictionary.Keys.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.Append(" ) VALUES ");
                                }

                                //
                                // Increment this after we've managed the initializer line because that checks this value for 0.
                                //
                                insertCounter++;

                                sb.Append(" ( ");

                                foreach (string fieldName in dictionary.Keys)
                                {
                                    if (fieldName.StartsWith("link:") == true)
                                    {
                                        string[] linkData = fieldName.Split(':');

                                        if (linkData.Length == 4)
                                        {
                                            string linkTable = linkData[1];
                                            string linkTableField = linkData[2];
                                            string linkField = linkData[3];
                                            string lookupValue = dictionary[fieldName];

                                            //
                                            // SQL Escape the lookup value
                                            //
                                            if (lookupValue != null)
                                            {
                                                lookupValue = lookupValue.Replace("'", "''");
                                            }


                                            sb.Append("( ");

                                            sb.Append("SELECT id FROM \"" + linkTable + "\" WHERE \"");
                                            sb.Append(linkTableField);
                                            sb.Append("\" = '");
                                            sb.Append(lookupValue);        // lookup value must be a string field..
                                            sb.Append("' LIMIT 1)");
                                        }
                                        else
                                        {
                                            throw new Exception("Unsupported link syntax.");
                                        }
                                    }
                                    else
                                    {
                                        Field field = null;

                                        if (validateFieldExistence == true)
                                        {
                                            field = GetFieldByName(fieldName);
                                        }
                                        else
                                        {
                                            // assume string field if running in non-validation mode
                                            field = new Field();
                                            field.dataType = DataType.STRING_1000;
                                        }

                                        if (field == null)
                                        {
                                            throw new Exception("Could not get field.  Name is " + fieldName);
                                        }

                                        switch (field.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                string value = dictionary[fieldName];

                                                if (value != null)
                                                {
                                                    sb.Append("'");
                                                    sb.Append(value.Replace("'", "''"));
                                                    sb.Append("'");
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.BIG_INTEGER:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                string numericValue = dictionary[fieldName];

                                                if (numericValue != null)
                                                {
                                                    sb.Append(numericValue);
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }


                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }
                                    }


                                    if (fieldName != dictionary.Keys.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.consolidateInserts == false || this.existenceCheckBeforeInsert == true)
                                {
                                    sb.AppendLine(" );");

                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        sb.AppendLine("END");
                                    }

                                    output.Add(sb.ToString());
                                    sb.Clear();

                                }
                                else
                                {
                                    if (insertCounter == 1000)
                                    {
                                        //
                                        // restart after 1000.
                                        //
                                        sb.AppendLine(" );");
                                        insertCounter = 0;

                                        output.Add(sb.ToString());
                                        sb.Clear();
                                    }
                                    else
                                    {
                                        if (dictionary != data.Last())
                                        {
                                            sb.Append(" ),");
                                        }
                                        else
                                        {
                                            sb.AppendLine(" )");
                                        }

                                        output.Add(sb.ToString());
                                        sb.Clear();
                                    }
                                }

                                break;

                            case DatabaseType.SQLite:

                                schemaName = "";

                                if (includeSchemaName == true)
                                {
                                    schemaName = "\"" + this.database.name + "\".\"";
                                }

                                if (this.consolidateInserts == false ||
                                            insertCounter == 0 ||
                                            existenceCheckBeforeInsert == true)
                                {
                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        string formattedFieldData = "";

                                        switch (this.primaryKeyField.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                if (formattedFieldData != null)
                                                {
                                                    formattedFieldData = "'" + formattedFieldData.Replace("'", "''") + "'";
                                                }
                                                else
                                                {
                                                    formattedFieldData = null;
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.BIG_INTEGER:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                formattedFieldData = dictionary[this.primaryKeyField.name];

                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }

                                        //sb.AppendLine("IF NOT EXISTS ( SELECT 1 FROM \"" + schemaName + "\".\"" + this.name + "\" WHERE " + this.primaryKeyField.name + " = " + formattedFieldData + " )");

                                        sb.AppendLine("IF NOT EXISTS ( SELECT 1 FROM \"" + this.name + "\" WHERE " + this.primaryKeyField.name + " = " + formattedFieldData + " )");
                                        sb.AppendLine("BEGIN");
                                        sb.Append("\t");
                                    }

                                    //if (includeSchemaName == true && this.database.disableSchemaName == false)
                                    //{
                                    //    sb.Append("INSERT INTO \"" + schemaName + "\".\"" + this.name + "\" ( ");
                                    //}
                                    //else
                                    //{
                                    sb.Append("INSERT INTO \"" + this.name + "\" ( ");
                                    //}

                                    foreach (string fieldName in dictionary.Keys)
                                    {
                                        if (fieldName.StartsWith("link:") == true)
                                        {
                                            // link field
                                            string[] linkData = fieldName.Split(':');

                                            if (linkData.Length == 4)
                                            {
                                                string linkField = linkData[3];
                                                sb.Append("\"");
                                                sb.Append(linkField);
                                                sb.Append("\"");
                                            }
                                            else
                                            {
                                                throw new Exception("Unsupported link syntax.");
                                            }
                                        }
                                        else
                                        {
                                            // regular field
                                            sb.Append("\"");
                                            sb.Append(fieldName);
                                            sb.Append("\"");
                                        }

                                        if (fieldName != dictionary.Keys.Last())
                                        {
                                            sb.Append(", ");
                                        }
                                    }

                                    sb.Append(" ) VALUES ");
                                }

                                //
                                // Increment this after we've managed the initializer line because that checks this value for 0.
                                //
                                insertCounter++;

                                sb.Append(" ( ");

                                foreach (string fieldName in dictionary.Keys)
                                {
                                    if (fieldName.StartsWith("link:") == true)
                                    {
                                        string[] linkData = fieldName.Split(':');

                                        if (linkData.Length == 4)
                                        {
                                            string linkTable = linkData[1];
                                            string linkTableField = linkData[2];
                                            string linkField = linkData[3];
                                            string lookupValue = dictionary[fieldName];

                                            //
                                            // SQL Escape the lookup value
                                            //
                                            if (lookupValue != null)
                                            {
                                                lookupValue = lookupValue.Replace("'", "''");
                                            }


                                            sb.Append("( ");

                                            sb.Append("SELECT id FROM \"" + linkTable + "\" WHERE \"");
                                            sb.Append(linkTableField);
                                            sb.Append("\" = '");
                                            sb.Append(lookupValue);        // lookup value must be a string field..
                                            sb.Append("' LIMIT 1)");
                                        }
                                        else
                                        {
                                            throw new Exception("Unsupported link syntax.");
                                        }
                                    }
                                    else
                                    {
                                        Field field = null;

                                        if (validateFieldExistence == true)
                                        {
                                            field = GetFieldByName(fieldName);
                                        }
                                        else
                                        {
                                            // assume string field if running in non-validation mode
                                            field = new Field();
                                            field.dataType = DataType.STRING_1000;
                                        }

                                        if (field == null)
                                        {
                                            throw new Exception("Could not get field.  Name is " + fieldName);
                                        }

                                        switch (field.dataType)
                                        {
                                            case DataType.STRING_10:
                                            case DataType.STRING_50:
                                            case DataType.STRING_100:
                                            case DataType.STRING_250:
                                            case DataType.STRING_500:
                                            case DataType.STRING_850:
                                            case DataType.STRING_1000:
                                            case DataType.STRING_2000:
                                            case DataType.STRING_HTML_COLOR:
                                            case DataType.DATE:
                                            case DataType.TIME:
                                            case DataType.DATETIME:
                                            case DataType.GUID:
                                            case DataType.GUID_PRIMARY_KEY:
                                            case DataType.TEXT:
                                            case DataType.BINARY:
                                            case DataType.PDF:
                                            case DataType.PNG:
                                            case DataType.MP4:
                                            case DataType.FOREIGN_KEY_GUID:
                                            case DataType.FOREIGN_KEY_STRING:

                                                string value = dictionary[fieldName];

                                                if (value != null)
                                                {
                                                    sb.Append("'");
                                                    sb.Append(value.Replace("'", "''"));
                                                    sb.Append("'");
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }

                                                break;

                                            case DataType.INTEGER:
                                            case DataType.INTEGER_PRIMARY_KEY:
                                            case DataType.INTEGER_AUTO_NUMBER_KEY:
                                            case DataType.BIG_INTEGER:
                                            case DataType.SINGLE_PRECISION_FLOAT:
                                            case DataType.DOUBLE_PRECISION_FLOAT:
                                            case DataType.DECIMAL_38_22:
                                            case DataType.LAT_LONG:
                                            case DataType.MONEY:
                                            case DataType.BOOL:
                                            case DataType.FOREIGN_KEY:

                                                string numericValue = dictionary[fieldName];

                                                if (numericValue != null)
                                                {
                                                    sb.Append(numericValue);
                                                }
                                                else
                                                {
                                                    sb.Append("null");
                                                }


                                                break;

                                            default:

                                                throw new Exception("Unhandled data type");
                                        }
                                    }


                                    if (fieldName != dictionary.Keys.Last())
                                    {
                                        sb.Append(", ");
                                    }
                                }

                                if (this.consolidateInserts == false || this.existenceCheckBeforeInsert == true)
                                {
                                    sb.AppendLine(" );");

                                    if (existenceCheckBeforeInsert == true)
                                    {
                                        sb.AppendLine("END");
                                    }

                                    output.Add(sb.ToString());
                                    sb.Clear();
                                }
                                else
                                {
                                    if (insertCounter == 1000)
                                    {
                                        //
                                        // restart after 1000.
                                        //
                                        sb.AppendLine(" );");
                                        insertCounter = 0;

                                        output.Add(sb.ToString());
                                        sb.Clear();
                                    }
                                    else
                                    {
                                        if (dictionary != data.Last())
                                        {
                                            sb.Append(" ),");
                                        }
                                        else
                                        {
                                            sb.AppendLine(" );");

                                            output.Add(sb.ToString());
                                            sb.Clear();
                                        }
                                    }
                                }

                                break;


                            default:
                                throw new Exception("Unhandled Database Type");

                        }
                    }

                    if (this.database.enableIdentityInsert == true && HasIdentityField() == true)
                    {
                        switch (databaseType)
                        {
                            case DatabaseType.MSSQLServer:

                                if (includeSchemaName == true)
                                {
                                    schemaName = "[" + this.database._schemaName + "].";
                                }

                                sb.AppendLine("SET IDENTITY_INSERT " + schemaName + "[" + this.name + "] OFF");

                                output.Add(sb.ToString());
                                sb.Clear();

                                break;

                            default:
                                // 
                                // For now only SQL server can enable identity insert.
                                // 
                                break;

                        }
                    }

                    return output;
                }



                public bool HasIdentityField()
                {
                    bool output = false;

                    foreach (Field field in fields)
                    {
                        if (field.dataType == DataType.INTEGER_AUTO_NUMBER_KEY == true)
                        {
                            output = true;
                            break;
                        }
                    }


                    return output;
                }


                //
                // Use this version to automatically create the field name that makes sense given the table to link to
                //
                public Field AddForeignKeyField(Table tableToLinkTo, bool allowNulls, bool createIndex = true)
                {
                    // Note We do not want to replace reserved words here because we're suffixing the token with Id
                    string fieldName = CamelCase(tableToLinkTo.name, false) + "Id";



                    //
                    // Determine the data type of the id field on the table to link to.  We use the same for the foreign key field table
                    //
                    Field linkTableIdField = tableToLinkTo.GetFieldByName("id");


                    if (linkTableIdField == null)
                    {
                        throw new Exception($"Can't add foreign key because can't get id field from table {tableToLinkTo.name}.");
                    }


                    Field linkField;

                    if (linkTableIdField.dataType == DataType.INTEGER_AUTO_NUMBER_KEY || linkTableIdField.dataType == DataType.INTEGER_PRIMARY_KEY)
                    {
                        linkField = this.AddField(new Field { name = fieldName, dataType = DataType.FOREIGN_KEY, nullable = allowNulls, table = this });     // use regular int
                    }
                    else if (linkTableIdField.dataType == DataType.BIG_INTEGER_AUTO_NUMBER_KEY || linkTableIdField.dataType == DataType.BIG_INTEGER_PRIMARY_KEY)
                    {
                        linkField = this.AddField(new Field { name = fieldName, dataType = DataType.FOREIGN_KEY_BIG_INTEGER, nullable = allowNulls, table = this });      // use big int 
                    }
                    else
                    {
                        throw new Exception($"Unsupported id field type of {linkTableIdField.dataType}.  Cannot use it for foreign key creation.");
                    }


                    linkField.comment = "Link to the " + tableToLinkTo.name + " table.";

                    string fkName = "FK_" + this.name + "_" + tableToLinkTo.name + "_" + fieldName;

                    if (ForeignKeyAlreadyExists(foreignKeys, fkName) == true)
                    {
                        throw new Exception("Value of " + fkName + " already exists");
                    }

                    ForeignKey foreignKey = new ForeignKey(fkName, linkField, tableToLinkTo, this);
                    foreignKey.comment = "Foreign key to the " + tableToLinkTo.name + " table.";

                    this.foreignKeys.Add(foreignKey);

                    if (createIndex == true)
                    {
                        linkField.CreateIndex();
                    }

                    return linkField;
                }


                public Field AddForeignKeyField(string fieldName, Table tableToLinkTo, bool allowNulls, bool createIndex = true)
                {
                    //
                    // Use this when the key into the other table is an int (The vast majority if cases will be this).
                    //

                    //
                    // Determine the data type of the id field on the table to link to.  We use the same for the foreign key field table
                    //
                    Field linkTableIdField = tableToLinkTo.GetFieldByName("id");


                    if (linkTableIdField == null)
                    {
                        throw new Exception($"Can't add foreign key because can't get id field from table {tableToLinkTo.name}.");
                    }


                    Field linkField;

                    if (linkTableIdField.dataType == DataType.INTEGER_AUTO_NUMBER_KEY || linkTableIdField.dataType == DataType.INTEGER_PRIMARY_KEY)
                    {
                        linkField = this.AddField(new Field { name = fieldName, dataType = DataType.FOREIGN_KEY, nullable = allowNulls, table = this });     // use regular int
                    }
                    else if (linkTableIdField.dataType == DataType.BIG_INTEGER_AUTO_NUMBER_KEY || linkTableIdField.dataType == DataType.BIG_INTEGER_PRIMARY_KEY)
                    {
                        linkField = this.AddField(new Field { name = fieldName, dataType = DataType.FOREIGN_KEY_BIG_INTEGER, nullable = allowNulls, table = this });      // use big int 
                    }
                    else
                    {
                        throw new Exception($"Unsupported id field type of {linkTableIdField.dataType}.  Cannot use it for foreign key creation.");
                    }


                    linkField.comment = "Link to the " + tableToLinkTo.name + " table.";

                    string fkName = "FK_" + this.name + "_" + tableToLinkTo.name + "_" + fieldName;

                    if (ForeignKeyAlreadyExists(foreignKeys, fkName) == true)
                    {
                        throw new Exception("Value of " + fkName + " already exists");
                    }

                    ForeignKey foreignKey = new ForeignKey(fkName, linkField, tableToLinkTo, this);
                    foreignKey.comment = "Foreign key to the " + tableToLinkTo.name + " table.";

                    this.foreignKeys.Add(foreignKey);

                    if (createIndex == true)
                    {
                        linkField.CreateIndex();
                    }

                    return linkField;
                }


                public Field AddForeignKeyStringField(string fieldName, Table tableToLinkTo, bool allowNulls)
                {
                    //
                    // Use this when the key into another table is a string.  For rare cases like OpenID tables
                    //
                    Field linkField = this.AddField(new Field { name = fieldName, dataType = DataType.FOREIGN_KEY_STRING, nullable = allowNulls, table = this });
                    linkField.comment = "Link to the " + tableToLinkTo.name + " table.";

                    string fkName = "FK_" + this.name + "_" + tableToLinkTo.name + "_" + fieldName;

                    if (ForeignKeyAlreadyExists(foreignKeys, fkName) == true)
                    {
                        throw new Exception("Value of " + fkName + " already exists");
                    }


                    ForeignKey foreignKey = new ForeignKey(fkName, linkField, tableToLinkTo, this);
                    foreignKey.comment = "Foreign key to the " + tableToLinkTo.name + " table.";

                    this.foreignKeys.Add(foreignKey);

                    return linkField;
                }


                public Field AddForeignKeyGuidField(string fieldName, Table tableToLinkTo, bool allowNulls)
                {
                    //
                    // Use this when the key into another table is a guid.  For data visibility stuff initially.
                    //
                    Field linkField = this.AddField(new Field { name = fieldName, dataType = DataType.FOREIGN_KEY_GUID, nullable = allowNulls, table = this });
                    linkField.comment = "Link to the " + tableToLinkTo.name + " table.";

                    string fkName = "FK_" + this.name + "_" + tableToLinkTo.name + "_" + fieldName;

                    if (ForeignKeyAlreadyExists(foreignKeys, fkName) == true)
                    {
                        throw new Exception("Value of " + fkName + " already exists");
                    }


                    ForeignKey foreignKey = new ForeignKey(fkName, linkField, tableToLinkTo, this);
                    foreignKey.comment = "Foreign key to the " + tableToLinkTo.name + " table.";

                    this.foreignKeys.Add(foreignKey);

                    return linkField;
                }



                public UniqueConstraint AddUniqueConstraint(Field field, bool ignoreNullSanityCheck = false)
                {
                    if (field.nullable == true)
                    {
                        throw new Exception("Adding unique constraint on a nullable field is a bad idea.");
                    }

                    List<Field> fields = new List<Field>();

                    fields.Add(field);

                    return AddUniqueConstraint(fields, ignoreNullSanityCheck);
                }


                public UniqueConstraint AddUniqueConstraint(string fieldName1, string fieldName2, bool ignoreNullSanityCheck)
                {
                    List<Field> fields = new List<Field>();

                    fields.Add(GetFieldByName(fieldName1));

                    if (fieldName2 != null)
                    {
                        fields.Add(GetFieldByName(fieldName2));
                    }

                    return AddUniqueConstraint(fields, ignoreNullSanityCheck);
                }

                public UniqueConstraint AddUniqueConstraint(string fieldName1, string fieldName2, string fieldName3, bool ignoreNullSanityCheck)
                {
                    List<Field> fields = new List<Field>();

                    fields.Add(GetFieldByName(fieldName1));

                    if (fieldName2 != null)
                    {
                        fields.Add(GetFieldByName(fieldName2));
                    }

                    if (fieldName3 != null)
                    {
                        fields.Add(GetFieldByName(fieldName3));
                    }

                    return AddUniqueConstraint(fields, ignoreNullSanityCheck);
                }


                public UniqueConstraint AddUniqueConstraint(string fieldName1, string fieldName2 = null, string fieldName3 = null, string fieldName4 = null, bool ignoreNullSanityCheck = false)
                {
                    List<Field> fields = new List<Field>();

                    fields.Add(GetFieldByName(fieldName1));

                    if (fieldName2 != null)
                    {
                        fields.Add(GetFieldByName(fieldName2));
                    }

                    if (fieldName3 != null)
                    {
                        fields.Add(GetFieldByName(fieldName3));
                    }

                    if (fieldName4 != null)
                    {
                        fields.Add(GetFieldByName(fieldName4));
                    }

                    return AddUniqueConstraint(fields, ignoreNullSanityCheck);
                }


                public UniqueConstraint AddUniqueConstraint(List<string> fieldNames, bool ignoreNullSanityCheck = false)
                {
                    List<Field> fields = new List<Field>();


                    foreach (string fieldName in fieldNames)
                    { 
                        Field field = GetFieldByName(fieldName);

                        if (field != null)
                        {
                            fields.Add(GetFieldByName(fieldName));
                        }
                        else
                        {
                            throw new Exception($"Could not get field named {fieldName} on table {name}");
                        }
                    }

                    return AddUniqueConstraint(fields, ignoreNullSanityCheck);
                }


                public UniqueConstraint AddUniqueConstraint(List<Field> fields, bool ignoreNullSanityCheck = false)
                {
                    string ucName = "UC_" + this.name;

                    foreach (Field field in fields)
                    {
                        ucName += "_" + field.name;
                    }

                    if (UniqueConstraintExists(uniqueConstraints, ucName) == true)
                    {
                        throw new Exception($"Value of {ucName} already exists on table {this.name}");
                    }


                    UniqueConstraint uc = new UniqueConstraint(ucName);

                    uc.comment = "Uniqueness enforced on the " + this.name + " table's ";

                    //
                    // Nullable field in constraint sanity check
                    //
                    foreach (Field field in fields)
                    {
                        if (ignoreNullSanityCheck == false)
                        {
                            if (field.nullable == true)
                            {
                                throw new Exception($"Adding unique constraint on a nullable field is generally a bad idea.  This can be overridden if you know that this is what you want.  Field is {field.name} and table is {field.table.name}");
                            }
                        }

                        uc.fields.Add(field);

                        uc.comment += field.name;

                        if (field != fields.Last())
                        {
                            uc.comment += " and ";
                        }
                    }

                    if (uc.fields.Count == 1)
                    {
                        uc.comment += " field.";
                    }
                    else
                    {
                        uc.comment += " fields.";
                    }


                    //
                    // Constraint on multi-tenant table without including tenantGuid sanity check
                    //
                    if (this.IsMultiTenantEnabled() == true)
                    {
                        bool tenantGuidFoundInConstraintFields = false;
                        foreach (Field field in fields)
                        {
                            if (field.name.ToUpper() == "TENANTGUID")
                            {
                                tenantGuidFoundInConstraintFields = true;
                                break;
                            }
                        }

                        if (tenantGuidFoundInConstraintFields == false)
                        {
                            throw new Exception("Adding unique constraint on a multi-tenant table without a tenantGuid field is a bad idea.  Table is: " + this.name);
                        }
                    }

                    this.uniqueConstraints.Add(uc);

                    return uc;
                }

                public void AddData(Dictionary<string, string> dictionary)
                {
                    //
                    // string 1 is field name, string 2 is value to insert serialized to string
                    //
                    data.Add(dictionary);
                }

                public void SetMinimumPermissionLevels(int minimumReadPermissionNeeded, int minimumWritePermissionNeeded)
                {
                    this.minimumReadPermissionLevel = minimumReadPermissionNeeded;
                    this.minimumWritePermissionLevel = minimumWritePermissionNeeded;
                }

                public void SetCommandTimeoutSeconds(int commandTimeoutSeconds)
                {
                    this.commandTimeoutSeconds = commandTimeoutSeconds;
                }

                public Index CreateIndex(string indexName)
                {
                    Index index = new Index(this, indexName);

                    this.indexes.Add(index);

                    return index;
                }
            }

            public Table GetTable(string name)
            {
                foreach (Table table in this.tables)
                {
                    if (table.name == name)
                    {
                        return table;
                    }
                }

                return null;
            }


            // returns self so it can be chained along with other setter methods inline in a build string
            public Database SetComment(string comment)
            {
                this.comment = comment;

                return this;
            }

            protected static bool TableAlreadyExists(List<Table> tables, string value)
            {
                //
                // Checks the values list for the existing presence of 'value'
                //
                foreach (var existingTable in tables)
                {
                    if (existingTable.name == value)
                    {
                        return true;
                    }
                }

                return false;
            }


            public class View : DatabaseElement
            {
                //
                // This needs to be implemented manually on a server by server basis
                // 
                public string MSSQLServerQueryText { get; set; }
                public string SQLiteQueryText { get; set; }
                public string MySQLQueryText { get; set; }
                public string PostgreSQLQueryText { get; set; }

                public override string CreateSQL(DatabaseType databaseType)
                {
                    if (databaseType == DatabaseType.MSSQLServer)
                    {
                        return MSSQLServerQueryText;
                    }
                    else if (databaseType == DatabaseType.SQLite)
                    {
                        return SQLiteQueryText;
                    }
                    else if (databaseType == DatabaseType.MySQL)
                    {
                        return MySQLQueryText;
                    }
                    else if (databaseType == DatabaseType.PostgreSQL)
                    {
                        return PostgreSQLQueryText;
                    }
                    else
                    {
                        throw new Exception("Unsupported database type");
                    }
                }
            }

            public List<Table> tables { get; set; }

            public bool dataVisibilityEnabled { get; set; }

            public bool multiTenantEnabled { get; private set; }

            public bool disableDatabaseFileCreation { get; set; }

            public bool disableComments { get; set; }
            public bool disableSchemaName { get; set; }
            public bool disableTableCreation { get; set; }
            public bool enableIdentityInsert { get; set; }




            public Database(string name)
            {
                tables = new List<Table>();

                this.name = name;
                this.dataVisibilityEnabled = false;
                this.multiTenantEnabled = false;

                this.disableTableCreation = false;
                this.enableIdentityInsert = false;

                this.disableDatabaseFileCreation = false;
                this.disableComments = false;
                this.disableSchemaName = false;

                return;
            }


            /// <summary>
            /// 
            /// This creates the complete database creation scripts 
            /// 
            /// </summary>
            /// <param name="databaseType"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public override string CreateSQL(DatabaseType databaseType)
            {
                StringBuilder sb = new StringBuilder();

                if (string.IsNullOrEmpty(_schemaName) == true)
                {
                    switch (databaseType)
                    {
                        case DatabaseType.MSSQLServer:
                            _schemaName = "dbo";
                            break;

                        case DatabaseType.MySQL:
                        case DatabaseType.PostgreSQL:
                        case DatabaseType.SQLite:

                            // TBD

                            break;

                        default:
                            throw new Exception("Unsupported database type");
                    }
                }
                else
                {
                    switch (databaseType)
                    {
                        case DatabaseType.SQLite:
                            // SQLite doesn't support schemas, so null it out.
                            _schemaName = null;
                            break;

                        default:
                            // do nothing.
                            break;

                    }
                }


                if (string.IsNullOrEmpty(this.comment) == false && this.disableComments == false)
                {
                    switch (databaseType)
                    {
                        case DatabaseType.MSSQLServer:
                        case DatabaseType.MySQL:
                        case DatabaseType.PostgreSQL:
                        case DatabaseType.SQLite:

                            WriteComments(sb);

                            break;

                        default:
                            throw new Exception("Unsupported database type");
                    }
                }


                //
                // Create the database
                //
                switch (databaseType)
                {
                    case DatabaseType.MSSQLServer:

                        if (this.disableTableCreation == false)
                        {
                            sb.AppendLine("CREATE DATABASE [" + this.name + "]");
                            sb.AppendLine("GO");
                            sb.AppendLine("");
                            sb.AppendLine("ALTER DATABASE [" + this.name + "] SET RECOVERY SIMPLE");
                            sb.AppendLine("GO");
                            sb.AppendLine("");
                        }
                        sb.AppendLine("USE [" + this.name + "]");
                        sb.AppendLine("GO");
                        sb.AppendLine("");

                        break;

                    case DatabaseType.MySQL:

                        if (this.disableTableCreation == false)
                        {
                            sb.AppendLine("CREATE DATABASE `" + this.name + "`;");
                            sb.AppendLine("");
                        }
                        sb.AppendLine("USE `" + this.name + "`;");
                        sb.AppendLine("");

                        break;


                    case DatabaseType.PostgreSQL:

                        //
                        // Use PostgreSQL's schema construct for each of our databases.  This gives flexibility in config so the multiple system databases can each be a schema in a containing database as an exmample.
                        //
                        //if (this.disableTableCreation == false)
                        //{
                        //    sb.AppendLine("CREATE SCHEMA \"" + this.name + "\";");
                        //    sb.AppendLine("");
                        //}
                        //sb.AppendLine("SET search_path=\"" + this.name + "\";");
                        //sb.AppendLine("");


                        if (this.disableTableCreation == false)
                        {
                            sb.AppendLine(@"CREATE DATABASE """ + this.name + @"""
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    TEMPLATE=template0;

\connect """ + this.name + @"""   -- Run the create database first, then connect to it, and run the rest if running in a query tool rather than through psql scripting.
");

                            sb.AppendLine("");
                        }
                        sb.AppendLine("");


                        break;

                    case DatabaseType.SQLite:

                        if (this.disableTableCreation)
                        {
                            //
                            // Create a new database file
                            //
                            if (this.disableComments == false)
                            {
                                sb.AppendLine("-- Execute this using the command line sqlite3 command line shell utility you can get from sqlite.org, and use the .read <filename> command to execute this script file.");
                                sb.AppendLine("");
                            }

                            sb.Append("ATTACH DATABASE \"");
                            sb.Append(this.name);
                            sb.Append(".db\" AS \"");
                            sb.Append(this.name);
                            sb.AppendLine("\";");
                            sb.AppendLine("");
                        }

                        break;

                    default:
                        throw new Exception("Unhandled Database Type");
                }


                //
                // Create the schema if need be
                //
                if (string.IsNullOrEmpty(_schemaName) == false && _schemaName != "dbo")
                {
                    switch (databaseType)
                    {
                        case DatabaseType.MSSQLServer:

                            if (this.disableTableCreation == false)
                            {
                                sb.AppendLine("CREATE SCHEMA [" + this._schemaName + "]");
                                sb.AppendLine("GO");
                                sb.AppendLine("");
                            }

                            break;

                        case DatabaseType.PostgreSQL:

                            if (this.disableTableCreation == false)
                            {
                                sb.AppendLine("CREATE SCHEMA \"" + this._schemaName + "\"");
                                sb.AppendLine("");
                            }

                            break;

                        case DatabaseType.MySQL:
                        case DatabaseType.SQLite:

                            // TBD
                            break;

                        default:
                            throw new Exception("Unhandled Database Type");
                    }

                }

                //
                // First, create commented DROP table instructions just as a convenience for any DB work that may need to be done
                //
                if (this.disableTableCreation == false && this.disableComments == false)
                {
                    sb.AppendLine("/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */");
                    for (int i = tables.Count - 1; i >= 0; i--)
                    {
                        Table table = tables[i];
                        sb.Append(table.DropSQL(databaseType));
                    }

                    sb.AppendLine();


                    //
                    // Next, create commented DROP table INDEX instructions just as a convenience for any DB work that may need to be done
                    //
                    sb.AppendLine("/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */");
                    for (int i = tables.Count - 1; i >= 0; i--)
                    {
                        Table table = tables[i];
                        sb.Append(table.DisableIndexSQL(databaseType));
                    }

                    sb.AppendLine();
                    sb.AppendLine("/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */");
                    for (int i = tables.Count - 1; i >= 0; i--)
                    {
                        Table table = tables[i];
                        sb.Append(table.EnableIndexSQL(databaseType));
                    }
                    sb.AppendLine();
                }

                //
                // Now create the table creation SQL
                //
                foreach (Database.Table table in this.tables)
                {
                    sb.Append(table.CreateSQL(databaseType));
                }

                return sb.ToString();
            }



            /// <summary>
            /// 
            /// This creates just data insert scripts.
            /// 
            /// </summary>
            /// <param name="databaseType"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public string CreateInsertSQL(DatabaseType databaseType)
            {
                StringBuilder sb = new StringBuilder();

                if (string.IsNullOrEmpty(_schemaName) == true)
                {
                    switch (databaseType)
                    {
                        case DatabaseType.MSSQLServer:
                            _schemaName = "dbo";
                            break;

                        case DatabaseType.MySQL:
                        case DatabaseType.PostgreSQL:
                        case DatabaseType.SQLite:

                            // TBD

                            break;

                        default:
                            throw new Exception("Unsupported database type");
                    }

                }

                //
                // Connect to the database
                //
                switch (databaseType)
                {
                    case DatabaseType.MSSQLServer:

                        sb.AppendLine("USE [" + this.name + "]");
                        sb.AppendLine("GO");
                        sb.AppendLine("");

                        break;

                    case DatabaseType.MySQL:

                        sb.AppendLine("USE `" + this.name + "`;");
                        sb.AppendLine("");

                        break;


                    case DatabaseType.PostgreSQL:

                        sb.AppendLine("SET search_path=\"" + this.name + "\";");
                        sb.AppendLine("");

                        break;

                    case DatabaseType.SQLite:

                        break;

                    default:
                        throw new Exception("Unhandled Database Type");
                }



                //
                // Now create the table data insertion SQL
                //
                foreach (Database.Table table in this.tables)
                {
                    if (table.data != null && table.data.Count > 0)
                    {
                        sb.Append(table.CreateInsertSQLFromData(databaseType, table.data, false));
                    }
                }

                return sb.ToString();
            }


            public Table AddTable(string tableName, bool isWritable = true, bool adminAccessNeededToWrite = false)
            {
                if (TableAlreadyExists(tables, tableName) == true)
                {
                    throw new Exception("Value of " + tableName + " already exists");
                }

                Table table = new Table(tableName);

                table.isWritable = isWritable;
                table.adminAccessNeededToWrite = adminAccessNeededToWrite;

                table.database = this;
                this.tables.Add(table);

                return table;
            }

            public Table AddTable(Table table)
            {
                if (TableAlreadyExists(tables, table.name) == true)
                {
                    throw new Exception("Value of " + table.name + " already exists");
                }

                table.database = this;
                this.tables.Add(table);

                return table;
            }

            public Table AddStandardCountryTable(int readPermissionLevel = 1, int writePermissionLevel = 255)
            {
                Database.Table countryTable = this.AddTable("Country");
                countryTable.comment = "The master list of countries";
                countryTable.SetMinimumPermissionLevels(readPermissionLevel, writePermissionLevel);
                countryTable.AddIdField();
                countryTable.AddNameAndDescriptionFields(true, true, false);
                countryTable.AddString10Field("abbreviation", false);
                countryTable.AddString50Field("postalCodeFormat", true).AddScriptComments("The human readable postal code format for the country, if applicable.");
                countryTable.AddString50Field("postalCodeRegEx", true).AddScriptComments("The regular expression pattern for validation of the postal code, if applicable ");
                countryTable.AddSequenceField();
                countryTable.AddControlFields();

                countryTable.AddData(new Dictionary<string, string> { { "name", "Canada" },
                                                        { "description", "Canada" },
                                                        { "abbreviation", "CA" },
                                                        { "sequence", "1" },
                                                        { "postalCodeFormat", "A0A 0A0"},
                                                        { "postalCodeRegEx", "^[A-Z]\\d[A-Z] ?\\d[A-Z]\\d$"},
                                                        { "objectGuid", "5f3f3c1d-9ba8-48cd-ae6d-4f4d8a5c2bcb" } });

                countryTable.AddData(new Dictionary<string, string> { { "name", "USA" },
                                                        { "description", "United States of America" },
                                                        { "abbreviation", "US" },
                                                        { "sequence", "2" },
                                                        { "postalCodeFormat", "NNNNN or NNNNN-NNNN"},
                                                        { "postalCodeRegEx", "^\\d{5}(-\\d{4})?$')"},
                                                        { "objectGuid", "9b2b1de3-719f-4c8a-bb2f-6e903d4e74b5" } });

                return countryTable;
            }


            public Table AddStandardStateProvinceTable(Table countryTable, int readPermissionLevel = 1, int writePermissionLevel = 255, string tableName = "StateProvince")
            {
                Database.Table stateTable = this.AddTable(tableName);
                stateTable.comment = "The master list of states";
                stateTable.SetMinimumPermissionLevels(readPermissionLevel, writePermissionLevel);
                stateTable.AddIdField();
                stateTable.AddForeignKeyField(countryTable, false, true);
                stateTable.AddNameAndDescriptionFields(false, true, false);
                stateTable.fields.Add(new Database.Table.Field { name = "abbreviation", dataType = DataType.STRING_10, unique = false, nullable = false });
                stateTable.AddSequenceField();
                stateTable.AddControlFields();
                stateTable.AddUniqueConstraint("name", "countryId", false);
                stateTable.AddUniqueConstraint("abbreviation", "countryId", false);


                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Newfoundland" },
    { "description", "Newfoundland and Labrador" },
    { "abbreviation", "NL" },
    { "sequence", "1" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "a1eecf09-7362-42be-b5d1-90284e1c3075" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Ontario" },
    { "description", "Ontario" },
    { "abbreviation", "ON" },
    { "sequence", "2" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "b2e5d8f1-897b-4563-8131-7eeb6d0c80a4" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Alberta" },
    { "description", "Alberta" },
    { "abbreviation", "AB" },
    { "sequence", "3" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "c3fe34bc-9601-474f-b99f-55c7a9c71738" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "British Columbia" },
    { "description", "British Columbia" },
    { "abbreviation", "BC" },
    { "sequence", "4" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "d4b7ab65-8fc6-4746-b9f6-e9bcf5b8cf91" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Manitoba" },
    { "description", "Manitoba" },
    { "abbreviation", "MB" },
    { "sequence", "5" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "e5a8be2d-7a4e-43e5-83d5-d2cf77282c0d" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "New Brunswick" },
    { "description", "New Brunswick" },
    { "abbreviation", "NB" },
    { "sequence", "6" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "f6f2a6f4-3963-4539-a54f-bd7ed0be2b3b" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Northwest Territories" },
    { "description", "Northwest Territories" },
    { "abbreviation", "NT" },
    { "sequence", "7" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "078f1d72-20a4-4b78-8b2f-9c6d6e69f29a" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Nova Scotia" },
    { "description", "Nova Scotia" },
    { "abbreviation", "NS" },
    { "sequence", "8" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "179fbbf1-b651-4b7a-b17e-b65d6aeb7795" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Nunavut" },
    { "description", "Nunavut" },
    { "abbreviation", "NU" },
    { "sequence", "9" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "28a1b2ed-7554-48b5-b7f0-b0f2bc3f0a8f" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Prince Edward Island" },
    { "description", "Prince Edward Island" },
    { "abbreviation", "PE" },
    { "sequence", "10" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "39b8c1de-dc77-4b3b-b0f6-e41b6a557809" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Quebec" },
    { "description", "Quebec" },
    { "abbreviation", "QC" },
    { "sequence", "11" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "4b9e6f87-b15f-4858-b739-dc23714b83b7" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Saskatchewan" },
    { "description", "Saskatchewan" },
    { "abbreviation", "SK" },
    { "sequence", "12" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "5c12c0ea-23a0-43a3-a8c9-15d032de5643" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Yukon" },
    { "description", "Yukon" },
    { "abbreviation", "YT" },
    { "sequence", "13" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "6d1a81eb-fc4a-4c44-9e5a-079c32074749" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Other" },
    { "description", "Other" },
    { "abbreviation", "Other" },
    { "sequence", "99" },
    { "link:Country:name:countryId", "Canada" },
    { "objectGuid", "7e2f5bce-c2b0-4012-84b4-c982d78dce3e" }
});



                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Alabama" },
    { "description", "Alabama" },
    { "abbreviation", "AL" },
    { "sequence", "1" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "d003a92b-6cec-4d49-8baa-6b4fd8fc2f92" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Alaska" },
    { "description", "Alaska" },
    { "abbreviation", "AK" },
    { "sequence", "2" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "3aff430d-2752-4d91-ae08-656934438dac" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Arizona" },
    { "description", "Arizona" },
    { "abbreviation", "AZ" },
    { "sequence", "3" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "5c4ec86a-472a-4d6c-a278-b5e21352b644" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Arkansas" },
    { "description", "Arkansas" },
    { "abbreviation", "AR" },
    { "sequence", "4" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "cd58100a-e5b6-4fc0-a251-2e1a22e66836" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "California" },
    { "description", "California" },
    { "abbreviation", "CA" },
    { "sequence", "5" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "36a7adaa-f35a-40ca-8f24-231a3ebd1ad8" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Colorado" },
    { "description", "Colorado" },
    { "abbreviation", "CO" },
    { "sequence", "6" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "0210922a-348c-4181-a9e0-6054dd7bc655" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Connecticut" },
    { "description", "Connecticut" },
    { "abbreviation", "CT" },
    { "sequence", "7" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "4040cc1a-e6f4-454d-93aa-162c74fe50c6" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Delaware" },
    { "description", "Delaware" },
    { "abbreviation", "DE" },
    { "sequence", "8" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "01a5dc36-c285-4216-9fb6-811d5b8e8b48" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Florida" },
    { "description", "Florida" },
    { "abbreviation", "FL" },
    { "sequence", "9" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "5e0bb9f6-b6ca-4b42-832f-7c41a570fae4" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Georgia" },
    { "description", "Georgia" },
    { "abbreviation", "GA" },
    { "sequence", "10" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "c57ffded-5284-471a-898c-f4969f611dd7" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Hawaii" },
    { "description", "Hawaii" },
    { "abbreviation", "HI" },
    { "sequence", "11" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "9fcaa230-ded7-47a8-8a3e-dd1a756ca363" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Idaho" },
    { "description", "Idaho" },
    { "abbreviation", "ID" },
    { "sequence", "12" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "796c444b-7513-4823-ab11-94dae65dc0e5" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Illinois" },
    { "description", "Illinois" },
    { "abbreviation", "IL" },
    { "sequence", "13" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "d2a28ab4-09c1-437b-b70c-1424543c4128" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Indiana" },
    { "description", "Indiana" },
    { "abbreviation", "IN" },
    { "sequence", "14" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "3d9f6c85-6515-4147-adec-ab7dc6e95eab" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Iowa" },
    { "description", "Iowa" },
    { "abbreviation", "IA" },
    { "sequence", "15" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "cecfa624-ba4a-473e-a0fc-e91b007beab7" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Kansas" },
    { "description", "Kansas" },
    { "abbreviation", "KS" },
    { "sequence", "16" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "b155c44b-c3dd-4884-b715-71ab38596e00" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Kentucky" },
    { "description", "Kentucky" },
    { "abbreviation", "KY" },
    { "sequence", "17" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "152ad250-6174-45f7-a947-6c6c14a56494" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Louisiana" },
    { "description", "Louisiana" },
    { "abbreviation", "LA" },
    { "sequence", "18" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "c9260be6-9840-420c-acf4-7d82ef937160" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Maine" },
    { "description", "Maine" },
    { "abbreviation", "ME" },
    { "sequence", "19" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "97b79ed1-f1b0-44ef-bdd0-71caccd1465d" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Maryland" },
    { "description", "Maryland" },
    { "abbreviation", "MD" },
    { "sequence", "20" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "c0cf2ae1-ed20-4845-b860-ff008427359b" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Massachusetts" },
    { "description", "Massachusetts" },
    { "abbreviation", "MA" },
    { "sequence", "21" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "7801225d-a996-40cb-888e-49645ffdbb06" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Michigan" },
    { "description", "Michigan" },
    { "abbreviation", "MI" },
    { "sequence", "22" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "f9324013-0a60-43ea-b672-6999a821cb15" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Minnesota" },
    { "description", "Minnesota" },
    { "abbreviation", "MN" },
    { "sequence", "23" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "f43770fd-ceaf-4646-9943-08be6268c045" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Mississippi" },
    { "description", "Mississippi" },
    { "abbreviation", "MS" },
    { "sequence", "24" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "b193e806-5a5e-4d46-936c-b4b3a28e59c5" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Missouri" },
    { "description", "Missouri" },
    { "abbreviation", "MO" },
    { "sequence", "25" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "d57e6019-c221-465e-b92e-0b8d3da0ff80" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Montana" },
    { "description", "Montana" },
    { "abbreviation", "MT" },
    { "sequence", "26" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "2f10e38c-b937-459f-89d0-60f552687c46" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Nebraska" },
    { "description", "Nebraska" },
    { "abbreviation", "NE" },
    { "sequence", "27" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "85ad29eb-f1c6-4862-82bd-d4c91eea2838" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Nevada" },
    { "description", "Nevada" },
    { "abbreviation", "NV" },
    { "sequence", "28" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "95ad29eb-f1c6-4862-82bd-d4c91eea2887" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "New Hampshire" },
    { "description", "New Hampshire" },
    { "abbreviation", "NH" },
    { "sequence", "29" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "5e5d5651-a186-4cc1-b61a-f22c9d530e6f" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "New Jersey" },
    { "description", "New Jersey" },
    { "abbreviation", "NJ" },
    { "sequence", "30" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "ee4ab53d-dab1-4ba7-8363-ed616a779567" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "New Mexico" },
    { "description", "New Mexico" },
    { "abbreviation", "NM" },
    { "sequence", "31" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "be168b30-72bd-4942-b187-deff865a5e6a" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "New York" },
    { "description", "New York" },
    { "abbreviation", "NY" },
    { "sequence", "32" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "7c93f785-a069-4298-93dc-2ef5e00fd0a8" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "North Carolina" },
    { "description", "North Carolina" },
    { "abbreviation", "NC" },
    { "sequence", "33" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "af2af206-9f3c-419f-9731-9fc90f1bda1b" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "North Dakota" },
    { "description", "North Dakota" },
    { "abbreviation", "ND" },
    { "sequence", "34" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "3a8d0072-1457-4923-bf19-12b8748098ee" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Ohio" },
    { "description", "Ohio" },
    { "abbreviation", "OH" },
    { "sequence", "35" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "d1961e5f-1c25-46ef-9bca-30fe538fe5c9" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Oklahoma" },
    { "description", "Oklahoma" },
    { "abbreviation", "OK" },
    { "sequence", "36" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "b2bc6d1b-32b6-4026-b648-70ec7b5063b1" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Oregon" },
    { "description", "Oregon" },
    { "abbreviation", "OR" },
    { "sequence", "37" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "fbd6a82b-3f4b-49e0-b5ba-59ec47335c99" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Pennsylvania" },
    { "description", "Pennsylvania" },
    { "abbreviation", "PA" },
    { "sequence", "38" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "d9b34153-fb25-403d-a13e-37b2823fbf69" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Rhode Island" },
    { "description", "Rhode Island" },
    { "abbreviation", "RI" },
    { "sequence", "39" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "c1c32aa7-af93-4bf1-9acf-9ff591b1b8c5" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "South Carolina" },
    { "description", "South Carolina" },
    { "abbreviation", "SC" },
    { "sequence", "40" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "9d050cab-34a0-40eb-8592-2ee2a62e21a1" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "South Dakota" },
    { "description", "South Dakota" },
    { "abbreviation", "SD" },
    { "sequence", "41" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "e652bc14-13e0-4405-9feb-6b78dd0790dd" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Tennessee" },
    { "description", "Tennessee" },
    { "abbreviation", "TN" },
    { "sequence", "42" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "0d7a100b-792e-46ca-81e0-eaef7e78aec2" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Texas" },
    { "description", "Texas" },
    { "abbreviation", "TX" },
    { "sequence", "43" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "5384bf42-c1a8-47c8-998c-85c02838a299" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Utah" },
    { "description", "Utah" },
    { "abbreviation", "UT" },
    { "sequence", "44" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "6f4755b9-8a7a-4c52-a8a2-a464de793cbd" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Vermont" },
    { "description", "Vermont" },
    { "abbreviation", "VT" },
    { "sequence", "45" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "9dd23ade-bbf4-4d5a-9fd8-199af9005145" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Virginia" },
    { "description", "Virginia" },
    { "abbreviation", "VA" },
    { "sequence", "46" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "6071e23d-d660-4801-894e-0ca5783d6a31" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Washington" },
    { "description", "Washington" },
    { "abbreviation", "WA" },
    { "sequence", "47" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "cc5b5362-f9fc-406f-927d-d6c4e917f76d" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "West Virginia" },
    { "description", "West Virginia" },
    { "abbreviation", "WV" },
    { "sequence", "48" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "06d12574-b3b8-4392-87a1-76a8c42ccf7a" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Wisconsin" },
    { "description", "Wisconsin" },
    { "abbreviation", "WI" },
    { "sequence", "49" },
    { "link:Country:name:countryId", "USA" },
{ "objectGuid", "ebf4200d-b4f0-4a62-b2a9-256aab919241" }
});
                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Wyoming" },
    { "description", "Wyoming" },
    { "abbreviation", "WY" },
    { "sequence", "50" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "dfff135c-165b-42a9-81f9-a55f8d51c710" }
});

                stateTable.AddData(new Dictionary<string, string> {
    { "name", "Other" },
    { "description", "Other" },
    { "abbreviation", "Other" },
    { "sequence", "99" },
    { "link:Country:name:countryId", "USA" },
    { "objectGuid", "4ab041c0-9479-4a65-ba56-cbb70d82de75" }
});

                return stateTable;
            }


            public Table AddStandardTimeZoneTable(int readPermissionLevel = 1, int writePermissionLevel = 255)
            {
                Database.Table timeZoneTable = this.AddTable("TimeZone");
                timeZoneTable.comment = "Time zones master data list.";
                timeZoneTable.SetMinimumPermissionLevels(readPermissionLevel, writePermissionLevel);
                timeZoneTable.AddIdField();
                timeZoneTable.AddNameAndDescriptionFields(true, true, false);
                timeZoneTable.AddString50Field("ianaTimeZone", false).AddScriptComments("e.g., 'America/St.John's' (official IANA name)");
                timeZoneTable.AddString50Field("abbreviation", false);
                timeZoneTable.AddString50Field("abbreviationDaylightSavings", false);
                timeZoneTable.AddBoolField("supportsDaylightSavings", false, true);
                timeZoneTable.AddSingleField("standardUTCOffsetHours", false).AddScriptComments("The standard offset hours from UTC for this time zone.");
                timeZoneTable.AddSingleField("dstUTCOffsetHours", false).AddScriptComments("Use the same value here as the standard one for time zones that do not support DST");
                timeZoneTable.AddIntField("sequence").AddScriptComments("For sorting in drop downs");
                timeZoneTable.AddControlFields();


                // Major North American Time Zones
                // Ordered from East to West (highest UTC offset to lowest)
                // All offsets are negative west of UTC (standard convention)
                // supportsDaylightSavings: "1" = yes, "0" = no
                // Sequence numbers are assigned logically from east to west

                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Newfoundland Standard Time" },
    { "abbreviation", "NST" },
    { "abbreviationDaylightSavings", "NDT" },
    { "supportsDaylightSavings", "1" },
    { "standardUTCOffsetHours", "-3.5" },
    { "dstUTCOffsetHours", "-2.5" },
    { "description", "Newfoundland and southeastern Labrador (Canada)" },
    { "ianaTimeZone", "America/St_Johns" },
    { "sequence", "10" },
    { "objectGuid", "27129170-81b3-4c70-a7d4-0378dce8426f" }
});

                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Atlantic Standard Time" },
    { "abbreviation", "AST" },
    { "abbreviationDaylightSavings", "ADT" },
    { "supportsDaylightSavings", "1" },
    { "standardUTCOffsetHours", "-4" },
    { "dstUTCOffsetHours", "-3" },
    { "description", "Atlantic Canada (Nova Scotia, New Brunswick, PEI, parts of Quebec)" },
    { "ianaTimeZone", "America/Halifax" },
    { "sequence", "20" },
    { "objectGuid", "8f3d2a1b-4c5e-4d8f-9a2b-6e7f1c3d9a0b" }
});

                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Atlantic Standard Time (no DST)" },
    { "abbreviation", "AST" },
    { "abbreviationDaylightSavings", "AST" },
    { "supportsDaylightSavings", "0" },
    { "standardUTCOffsetHours", "-4" },
    { "dstUTCOffsetHours", "-4" },
    { "description", "Puerto Rico, US Virgin Islands, Dominican Republic" },
    { "ianaTimeZone", "America/Puerto_Rico" },
    { "sequence", "30" },
    { "objectGuid", "648d1e27-51b2-4e9b-ae9e-06dd856022e8" }
});

                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Eastern Standard Time" },
    { "abbreviation", "EST" },
    { "abbreviationDaylightSavings", "EDT" },
    { "supportsDaylightSavings", "1" },
    { "standardUTCOffsetHours", "-5" },
    { "dstUTCOffsetHours", "-4" },
    { "description", "Eastern United States, Eastern Canada (Ontario, Quebec)" },
    { "ianaTimeZone", "America/New_York" },
    { "sequence", "40" },
    { "objectGuid", "c4e5f6a7-8b9c-4d0e-1f2a-3b4c5d6e7f8a" }
});

                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Central Standard Time" },
    { "abbreviation", "CST" },
    { "abbreviationDaylightSavings", "CDT" },
    { "supportsDaylightSavings", "1" },
    { "standardUTCOffsetHours", "-6" },
    { "dstUTCOffsetHours", "-5" },
    { "description", "Central United States, Central Canada, Mexico (most), Central America" },
    { "ianaTimeZone", "America/Chicago" },
    { "sequence", "50" },
    { "objectGuid", "d5e6f7a8-9c0d-4e1f-2a3b-4c5d6e7f8a9b" }
});


                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Central Standard Time (no DST)" },
    { "abbreviation", "CST" },
    { "abbreviationDaylightSavings", "CST" },
    { "supportsDaylightSavings", "0" },
    { "standardUTCOffsetHours", "-6" },
    { "dstUTCOffsetHours", "-6" },
    { "description", "Central America (Guatemala, Costa Rica, Nicaragua, etc.)" },
    { "ianaTimeZone", "America/Guatemala" },
    { "sequence", "60" },
    { "objectGuid", "f2b768f4-6162-4f65-8eb8-6ae1c5a9dc88" }
});


                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Mountain Standard Time" },
    { "abbreviation", "MST" },
    { "abbreviationDaylightSavings", "MDT" },
    { "supportsDaylightSavings", "1" },
    { "standardUTCOffsetHours", "-7" },
    { "dstUTCOffsetHours", "-6" },
    { "description", "Mountain United States (except Arizona), Western Canada" },
    { "ianaTimeZone", "America/Denver" },
    { "sequence", "70" },
    { "objectGuid", "e6f7a8b9-0d1e-4f2a-3b4c-5d6e7f8a9b0c" }
});

                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Arizona Time" }, // Special case — does not observe DST
    { "abbreviation", "MST" },
    { "abbreviationDaylightSavings", "MST" },
    { "supportsDaylightSavings", "0" },
    { "standardUTCOffsetHours", "-7" },
    { "dstUTCOffsetHours", "-7" },
    { "description", "Arizona (United States) — does not observe Daylight Saving Time" },
    { "ianaTimeZone", "America/Phoenix" },
    { "sequence", "80" },
    { "objectGuid", "f7a8b9c0-1e2f-4a3b-5c6d-7e8f9a0b1c2d" }
});

                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Pacific Standard Time" },
    { "abbreviation", "PST" },
    { "abbreviationDaylightSavings", "PDT" },
    { "supportsDaylightSavings", "1" },
    { "standardUTCOffsetHours", "-8" },
    { "dstUTCOffsetHours", "-7" },
    { "description", "Western United States, Western Canada (British Columbia)" },
    { "ianaTimeZone", "America/Los_Angeles" },
    { "sequence", "90" },
    { "objectGuid", "a8b9c0d1-2f3a-4b5c-6d7e-8f9a0b1c2d3e" }
});

                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Alaska Standard Time" },
    { "abbreviation", "AKST" },
    { "abbreviationDaylightSavings", "AKDT" },
    { "supportsDaylightSavings", "1" },
    { "standardUTCOffsetHours", "-9" },
    { "dstUTCOffsetHours", "-8" },
    { "description", "Alaska (United States)" },
        { "ianaTimeZone", "America/Anchorage" },
    { "sequence", "100" },
    { "objectGuid", "b9c0d1e2-3a4b-5c6d-7e8f-9a0b1c2d3e4f" }
});

                timeZoneTable.AddData(new Dictionary<string, string>
{
    { "name", "Hawaii-Aleutian Standard Time" },
    { "abbreviation", "HST" },
    { "abbreviationDaylightSavings", "HST" },
    { "supportsDaylightSavings", "0" },
    { "standardUTCOffsetHours", "-10" },
    { "dstUTCOffsetHours", "-10" },
    { "description", "Hawaii and Aleutian Islands (United States) — no Daylight Saving Time" },
        { "ianaTimeZone", "Pacific/Honolulu" },
    { "sequence", "110" },
    { "objectGuid", "c0d1e2f3-4b5c-6d7e-8f9a-0b1c2d3e4f5a" }
});


                return timeZoneTable;
            }


            /// <summary>
            /// 
            /// This creates the module and roles in the security database.
            /// 
            /// it starts with the standard security roles of reader, writer, admin, and no access, and then adds in any custom ones.
            /// 
            /// </summary>
            /// <param name="moduleName"></param>
            /// <param name="databaseType"></param>
            /// <param name="disableComments"></param>
            /// <param name="securitySchemaName"></param>
            /// <param name="disableSchemaName"></param>
            /// <returns></returns>
            public string CreateSecurityConfigurationSQL(string moduleName, DatabaseType databaseType, bool disableComments = false, string securitySchemaName = null, bool disableSchemaName = false)
            {
                if (string.IsNullOrEmpty(securitySchemaName) == true)
                {
                    securitySchemaName = "Security";
                }


                StringBuilder sb = new StringBuilder();

                Database securityDatabase = new Database("Security");

                securityDatabase.disableComments = disableComments;
                securityDatabase.disableSchemaName = disableSchemaName;
                securityDatabase.SetSchemaName(securitySchemaName);

                if (this.disableComments == false)
                {
                    sb.Append(CreateCommentLine(databaseType, ""));
                    sb.Append(CreateCommentLine(databaseType, "Define the " + moduleName + " Module"));
                    sb.Append(CreateCommentLine(databaseType, ""));
                }


                List<Dictionary<string, string>> moduleData = new List<Dictionary<string, string>>();
                Dictionary<string, string> moduleRow = new Dictionary<string, string>();

                moduleRow.Add("name", moduleName);
                moduleRow.Add("description", "The " + moduleName + " Module");
                moduleData.Add(moduleRow);

                Table moduleTable = new Table("Module");
                moduleTable.database = securityDatabase;

                sb.AppendLine(moduleTable.CreateInsertSQLFromData(databaseType, moduleData, false));

                if (this.disableComments == false)
                {
                    sb.Append(CreateCommentLine(databaseType, ""));
                    sb.Append(CreateCommentLine(databaseType, "Define the " + moduleName + " Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles"));
                    sb.Append(CreateCommentLine(databaseType, ""));
                }

                //
                // Default security roles.
                //
                List<Dictionary<string, string>> defaultSecurityRoleData = new List<Dictionary<string, string>>();

                Dictionary<string, string> securityRoleAdministratorRow = new Dictionary<string, string>();
                securityRoleAdministratorRow.Add("name", moduleName + " Administrator");
                securityRoleAdministratorRow.Add("description", moduleName + " Administrator Role");
                securityRoleAdministratorRow.Add("link:Privilege:name:privilegeId", "Administrative");
                defaultSecurityRoleData.Add(securityRoleAdministratorRow);

                Dictionary<string, string> securityRoleReaderRow = new Dictionary<string, string>();
                securityRoleReaderRow.Add("name", moduleName + " Reader");
                securityRoleReaderRow.Add("description", moduleName + " Reader Role");
                securityRoleReaderRow.Add("link:Privilege:name:privilegeId", "Read Only");
                defaultSecurityRoleData.Add(securityRoleReaderRow);

                Dictionary<string, string> securityRoleReaderAndWriterRow = new Dictionary<string, string>();
                securityRoleReaderAndWriterRow.Add("name", moduleName + " Reader and Writer");
                securityRoleReaderAndWriterRow.Add("description", moduleName + " Reader and Writer Role");
                securityRoleReaderAndWriterRow.Add("link:Privilege:name:privilegeId", "Read and Write");
                defaultSecurityRoleData.Add(securityRoleReaderAndWriterRow);


                Dictionary<string, string> securityRoleNoAccessRow = new Dictionary<string, string>();
                securityRoleNoAccessRow.Add("name", moduleName + " No Access");
                securityRoleNoAccessRow.Add("description", moduleName + " No Access Role");
                securityRoleNoAccessRow.Add("link:Privilege:name:privilegeId", "No Access");
                defaultSecurityRoleData.Add(securityRoleNoAccessRow);



                Table securityRoleTable = new Table("SecurityRole");
                securityRoleTable.database = securityDatabase;

                sb.AppendLine(securityRoleTable.CreateInsertSQLFromData(databaseType, defaultSecurityRoleData, false));

                if (this.disableComments == false)
                {
                    sb.Append(CreateCommentLine(databaseType, ""));
                    sb.Append(CreateCommentLine(databaseType, "Link the " + moduleName + " module to the roles"));
                    sb.Append(CreateCommentLine(databaseType, ""));
                }

                List<Dictionary<string, string>> defaultModuleRoleData = new List<Dictionary<string, string>>();

                Dictionary<string, string> moduleRoleAdministratorRow = new Dictionary<string, string>();
                moduleRoleAdministratorRow.Add("link:Module:name:moduleId", moduleName);
                moduleRoleAdministratorRow.Add("link:SecurityRole:name:securityRoleId", moduleName + " Administrator");
                defaultModuleRoleData.Add(moduleRoleAdministratorRow);

                Dictionary<string, string> moduleRoleReaderRow = new Dictionary<string, string>();
                moduleRoleReaderRow.Add("link:Module:name:moduleId", moduleName);
                moduleRoleReaderRow.Add("link:SecurityRole:name:securityRoleId", moduleName + " Reader");
                defaultModuleRoleData.Add(moduleRoleReaderRow);

                Dictionary<string, string> moduleRoleReaderAndWriterRow = new Dictionary<string, string>();
                moduleRoleReaderAndWriterRow.Add("link:Module:name:moduleId", moduleName);
                moduleRoleReaderAndWriterRow.Add("link:SecurityRole:name:securityRoleId", moduleName + " Reader and Writer");
                defaultModuleRoleData.Add(moduleRoleReaderAndWriterRow);

                Dictionary<string, string> moduleRoleNoAccessRow = new Dictionary<string, string>();
                moduleRoleNoAccessRow.Add("link:Module:name:moduleId", moduleName);
                moduleRoleNoAccessRow.Add("link:SecurityRole:name:securityRoleId", moduleName + " No Access");
                defaultModuleRoleData.Add(moduleRoleNoAccessRow);


                Table moduleSecurityRoleTable = new Table("ModuleSecurityRole");
                moduleSecurityRoleTable.database = securityDatabase;

                sb.AppendLine(moduleSecurityRoleTable.CreateInsertSQLFromData(databaseType, defaultModuleRoleData, false));



                if (_customRoles.Count > 0)
                {
                    if (this.disableComments == false)
                    {
                        sb.Append(CreateCommentLine(databaseType, ""));
                        sb.Append(CreateCommentLine(databaseType, $"Define the custom roles for the {moduleName} module"));
                        sb.Append(CreateCommentLine(databaseType, ""));
                    }


                    //
                    // Now add in any custom roles defined in the database.
                    //
                    List<Dictionary<string, string>> customSecurityRoleData = new List<Dictionary<string, string>>();
                    List<Dictionary<string, string>> customModuleRoleData = new List<Dictionary<string, string>>();


                    foreach (KeyValuePair<string, string> customRole in _customRoles)
                    {

                        //
                        // Add the custom role
                        //
                        Dictionary<string, string> customRoleLine = new Dictionary<string, string>();
                        customRoleLine.Add("name", customRole.Key);
                        customRoleLine.Add("description", customRole.Value);
                        customRoleLine.Add("link:Privilege:name:privilegeId", "Custom");        // custom role type link
                        customSecurityRoleData.Add(customRoleLine);


                        //
                        // Add the custom role to module link
                        //
                        Dictionary<string, string> customModuleRoleLine = new Dictionary<string, string>();
                        customModuleRoleLine.Add("link:Module:name:moduleId", moduleName);
                        customModuleRoleLine.Add("link:SecurityRole:name:securityRoleId", customRole.Key);
                        customModuleRoleData.Add(customModuleRoleLine);
                    }

                    //
                    // Add the insertion lines for the custom roles to the script
                    //
                    sb.AppendLine(securityRoleTable.CreateInsertSQLFromData(databaseType, customSecurityRoleData, false));
                    sb.AppendLine(moduleSecurityRoleTable.CreateInsertSQLFromData(databaseType, customModuleRoleData, false));
                }
                //
                // Give the Admin user the module's Adminstrator role
                //
                if (this.disableComments == false)
                {
                    sb.Append(CreateCommentLine(databaseType, ""));
                    sb.Append(CreateCommentLine(databaseType, "Give the admin user administrative rights to the module"));
                    sb.Append(CreateCommentLine(databaseType, ""));
                }

                List<Dictionary<string, string>> userRoleData = new List<Dictionary<string, string>>();

                Table securityUserSecurityRoleTable = new Table("SecurityUserSecurityRole");
                securityUserSecurityRoleTable.database = securityDatabase;

                Dictionary<string, string> userRoleAdministratorRow = new Dictionary<string, string>();
                userRoleAdministratorRow.Add("link:SecurityUser:accountName:securityUserId", "Admin");
                userRoleAdministratorRow.Add("link:SecurityRole:name:securityRoleId", moduleName + " Administrator");
                userRoleAdministratorRow.Add("active", "1");
                userRoleAdministratorRow.Add("deleted", "0");
                userRoleData.Add(userRoleAdministratorRow);

                sb.AppendLine(securityUserSecurityRoleTable.CreateInsertSQLFromData(databaseType, userRoleData, false));

                
                return sb.ToString();
            }

            private string CreateCommentLine(DatabaseType databaseType, string comment)
            {
                StringBuilder sb = new StringBuilder();

                switch (databaseType)
                {
                    case DatabaseType.MSSQLServer:

                        sb.Append("-- ");
                        sb.AppendLine(comment);

                        break;

                    case DatabaseType.MySQL:

                        sb.Append("-- ");
                        sb.AppendLine(comment);

                        break;


                    case DatabaseType.PostgreSQL:
                        sb.Append("-- ");
                        sb.AppendLine(comment);

                        break;

                    case DatabaseType.SQLite:

                        sb.Append("-- ");
                        sb.AppendLine(comment);

                        break;

                    default:

                        throw new Exception("Unhandled Database Type");
                }

                return sb.ToString();
            }

            public class Client
            {
            }
        }

        public DatabaseGenerator(string databaseName, string moduleName)
        {
            this.database = new Database(databaseName);
            this.moduleName = moduleName;
        }

        public void GenerateDatabaseCreationScriptsInFolder(string folderPath = null, bool createSecurityConfigurationScripts = true, string optionalDatabaseNameFileSuffix = null)
        {
            if (folderPath == null)
            {
                folderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DatabaseCreationScripts");
            }


            //
            // Create the database creation scripts in each of the database variants
            //
            if (folderPath.EndsWith("\\") == false)
            {
                folderPath = folderPath + "\\";

            }

            System.IO.Directory.CreateDirectory(folderPath + this.database.name);
            System.IO.Directory.CreateDirectory(folderPath + this.database.name + "\\CreateScripts");

            //
            // Schema creation
            //
            System.IO.File.WriteAllText(folderPath + this.database.name + "\\CreateScripts\\Create_" + this.database.name + (optionalDatabaseNameFileSuffix != null ? "_" + optionalDatabaseNameFileSuffix : "") + "_Schema_MSSQLServer.sql", this.database.CreateSQL(DatabaseType.MSSQLServer));
            System.IO.File.WriteAllText(folderPath + this.database.name + "\\CreateScripts\\Create_" + this.database.name + (optionalDatabaseNameFileSuffix != null ? "_" + optionalDatabaseNameFileSuffix : "") + "_Schema_MySQL.sql", this.database.CreateSQL(DatabaseType.MySQL));
            System.IO.File.WriteAllText(folderPath + this.database.name + "\\CreateScripts\\Create_" + this.database.name + (optionalDatabaseNameFileSuffix != null ? "_" + optionalDatabaseNameFileSuffix : "") + "_Schema_PostgreSQL.sql", this.database.CreateSQL(DatabaseType.PostgreSQL));
            System.IO.File.WriteAllText(folderPath + this.database.name + "\\CreateScripts\\Create_" + this.database.name + (optionalDatabaseNameFileSuffix != null ? "_" + optionalDatabaseNameFileSuffix : "") + "_Schema_SQLite.sql", this.database.CreateSQL(DatabaseType.SQLite));



            //
            // Security configuration
            //
            if (createSecurityConfigurationScripts == true)
            {
                System.IO.File.WriteAllText(folderPath + this.database.name + "\\CreateScripts\\Create_" + this.database.name + (optionalDatabaseNameFileSuffix != null ? "_" + optionalDatabaseNameFileSuffix : "") + "_SecurityConfiguration_MSSQLServer.sql", this.database.CreateSecurityConfigurationSQL(this.moduleName, DatabaseType.MSSQLServer));
                System.IO.File.WriteAllText(folderPath + this.database.name + "\\CreateScripts\\Create_" + this.database.name + (optionalDatabaseNameFileSuffix != null ? "_" + optionalDatabaseNameFileSuffix : "") + "_SecurityConfiguration_MySQL.sql", this.database.CreateSecurityConfigurationSQL(this.moduleName, DatabaseType.MySQL));
                System.IO.File.WriteAllText(folderPath + this.database.name + "\\CreateScripts\\Create_" + this.database.name + (optionalDatabaseNameFileSuffix != null ? "_" + optionalDatabaseNameFileSuffix : "") + "_SecurityConfiguration_PostgreSQL.sql", this.database.CreateSecurityConfigurationSQL(this.moduleName, DatabaseType.PostgreSQL));
                System.IO.File.WriteAllText(folderPath + this.database.name + "\\CreateScripts\\Create_" + this.database.name + (optionalDatabaseNameFileSuffix != null ? "_" + optionalDatabaseNameFileSuffix : "") + "_SecurityConfiguration_SQLite.sql", this.database.CreateSecurityConfigurationSQL(this.moduleName, DatabaseType.SQLite));
            }
        }

        public string GenerateDatabaseCreationScripts(DatabaseType databaseType = DatabaseType.MSSQLServer)
        {
            return this.database.CreateSQL(databaseType);
        }



        /// <summary>
        /// 
        /// This writes the data objects in the database's tables to disk for all database types.
        /// 
        /// Note that the order of the writes is the order of the tables in the table list, so this should not be used if the order of the inserts is important.
        /// 
        /// In that case, write each table at a time.
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        public void GenerateDataInsertScriptsInFolder(string folderPath = null, string fileNameExtraIdentifier = null)
        {
            if (folderPath == null)
            {
                folderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DataInsertScripts");
            }


            //
            // Create the database creation scripts in each of the database variants
            //
            if (folderPath.EndsWith("\\") == false)
            {
                folderPath = folderPath + "\\";

            }

            System.IO.Directory.CreateDirectory(folderPath + this.database.name);
            System.IO.Directory.CreateDirectory(folderPath + this.database.name + "\\InsertScripts");

            System.IO.File.WriteAllText(folderPath + this.database.name + "\\InsertScripts\\Create_" + this.database.name + (fileNameExtraIdentifier != null ? "_" + fileNameExtraIdentifier : "") + "_Data_MSSQLServer.sql", this.database.CreateInsertSQL(DatabaseType.MSSQLServer));
            System.IO.File.WriteAllText(folderPath + this.database.name + "\\InsertScripts\\Create_" + this.database.name + (fileNameExtraIdentifier != null ? "_" + fileNameExtraIdentifier : "") + "_Data_MySQL.sql", this.database.CreateInsertSQL(DatabaseType.MySQL));
            System.IO.File.WriteAllText(folderPath + this.database.name + "\\InsertScripts\\Create_" + this.database.name + (fileNameExtraIdentifier != null ? "_" + fileNameExtraIdentifier : "") + "_Data_PostgreSQL.sql", this.database.CreateInsertSQL(DatabaseType.PostgreSQL));
            System.IO.File.WriteAllText(folderPath + this.database.name + "\\InsertScripts\\Create_" + this.database.name + (fileNameExtraIdentifier != null ? "_" + fileNameExtraIdentifier : "") + "_Data_SQLite.sql", this.database.CreateInsertSQL(DatabaseType.SQLite));
        }

        public string GenerateDataInsertScripts(DatabaseType databaseType = DatabaseType.MSSQLServer)
        {
            return this.database.CreateSQL(databaseType);
        }

        public void ClearAllData()
        {
            foreach (Database.Table table in this.database.tables)
            {
                if (table.data != null && table.data.Count > 0)
                {
                    table.data.Clear();
                }
            }
        }

        private static string RemoveVowels(string str)
        {
            return str.Replace("A", "").Replace("a", "")
                      .Replace("E", "").Replace("e", "")
                      .Replace("I", "").Replace("i", "")
                      .Replace("O", "").Replace("o", "")
                      .Replace("U", "").Replace("U", "");
        }
    }
}
