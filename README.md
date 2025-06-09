# DapperEntityORM

DapperEntityORM is a C# ORM library that leverages the power of Dapper to simplify database operations. It provides an easy-to-use API that allows you to interact with your database using strongly typed models.

## Usage

To use DapperEntityORM, you need to create a model that represents a table in your database. Each property of the model represents a column in the table. You can also define attributes on each property to specify additional metadata, such as whether the column can be null or has a maximum length.

Here's an example of a model that represents an "Employee" table:

```csharp
[Table]
public class Employees : Entity<Employees>
{
    [Key(IsIdentity = true)]
    public int EmployeeId { get; set; }
    [Column]
    public string FirstName { get; set; }
    [Column]
    public string LastName { get; set; }
    [Column]
    public int Age { get; set; }
    [Column]
    public DateTime HireDate { get; set; }
    [Relation("EmployeeId")]
    public Departments Department { get; set; }
    [Relation("EmployeeId")]
    public List<Addresses> Address { get; set; } 
}

[Table("Addresses")]
public class Addresses : Entity<Address>
{
    [Key(IsIdentity = true)]
    public int Id { get; set; }
    [Column]
    public string Address { get; set; }
    [Column]
    public string City { get; set; }
    [Column]
    public string State { get; set; }
    [Column]
    public string Zip { get; set; }
}

[Table("Departments")]
public class Departments : Entity<Departments>
{
    [Key(IsIdentity = true)]
    public int Id { get; set; }
    [Column]
    public string Name { get; set; }
    [Column]
    public string Description { get; set; }
}
```
Once you have your model defined, you can use the Model class to interact with the corresponding table in the database. Here's an example of how to retrieve all employees from the "Employee" table with her department and address information:

```csharp
List<Employees> employees = Employees.Select(database).ToList();
```
or if you want to retrieve only one employee for example with id = 2:

```csharp
Employees employee = Employees.Select(database).Where(X => X.Id== 2).Single();
```
or 

``` csharp
Employees employee = new Employees(database);
employee.Load(2);
```
in this case the employee object will be loaded with the data of the employee with the primary key = 2.

Now with the employee you can update, delete or insert it to the database with the following methods:

### update
```csharp
employee.Update();
```
or
```csharp
employee.Update(out List<string> errors);
```

### insert
```csharp
employee.Insert();
```
or 
```csharp
employee.Insert(out List<string> errors);
```
### delete
```csharp
employee.Delete();
```
### save
```csharp
employee.Save();
```
or
```csharp
employee.Save(out List<string> errors);
```

and if you want to insert or update the employee with his department and address information you can only add an item to the list address or modified the deparment data and the method update will do the rest for you. However, to delete a record, you must use the Delete function, and for linked tables, you will have to manually delete them using their respective Delete method.
The method Save will do the rest for you, it will insert or update the record depending on the existence of the primary key in the database.

DapperEntityORM support async methods for all the methods mentioned above.

DapperEntityORM also supports more advanced database operations such as filtering, ordering, and grouping records. You can use the `Select` method to retrieve records from the database with various filtering options, such as `WHERE`, `GROUP BY`, and `HAVING`. You can also use the `OrderBy` and `OrderByDescending` methods to sort records by one or more columns. Additionally, you can use the `Count`, `Sum`, `Min`, `Max`, and `Average` methods to retrieve aggregate data from the database.

DapperEntityORM also supports other SQL commands such as `JOIN`, `UNION`, `INTERSECT`, and `EXCEPT`. These commands can be used by specifying a custom SQL query using the `Query` method. (Implementing in the future)

## Execute query's results
The results of the query's execution are returned with several methods:

- `ToSingle()`: returns a single result of the query.
- `ToEnumerable()`: returns an enumerable of the results of the query.
- `ToList()`: returns a list of the results of the query.
- `ToDictionary()`: returns a dictionary of the results of the query.
- `ToDictionaryKeyString()`: returns a dictionary of the results of the query with the key as a string.
- `ToDictionaryKeyGuid()`: returns a dictionary of the results of the query with the key as a Guid.
- `ToDictionaryKeyInt()`: returns a dictionary of the results of the query with the key as an int.

### Async methods
All the methods of the query's execution are available in asynchronous mode, to use them you must add the suffix "Async" to the name of the method.

- `ToSingleAsync()`: returns a single result of the query.
- `ToEnumerableAsync()`: returns an enumerable of the results of the query.
- `ToListAsync()`: returns a list of the results of the query.
- `ToDictionaryAsync()`: returns a dictionary of the results of the query.
- `ToDictionaryKeyStringAsync()`: returns a dictionary of the results of the query with the key as a string.
- `ToDictionaryKeyGuidAsync()`: returns a dictionary of the results of the query with the key as a Guid.
- `ToDictionaryKeyIntAsync()`: returns a dictionary of the results of the query with the key as an int.

# Attributes of the model
The attributes of the model are used to define the name of the table and the name of the columns in the database, the primary key and the relationship between the tables.

## Table
 The attribute `[Table]` is used to define this class is a table in the database,you can specify the name of the database table with `[Table("NameofTable")]` or `[Table(TableName="NameofTable")]`,  if you do not specify the name of the table, the name of the model will be used as the name of the table in the database.

 ## Key
The attribute `[Key]` is used to define the primary key of the table, you can specify the name of the primary key in the database with `[Key("NameofPrimaryKey")]` or `[Key(Name="NameofPrimaryKey")]`, if you do not specify the name of the primary key, the name of the property will be used as the name of the primary key in the database.

you can specify if the primary key is autoincrement with `[Key(IsIdentity=true)]`, if you do not specify the primary key is autoincrement, the primary key will be not autoincrement.

## Columns
The attribute `[Column]` is used to define the columns of the table, you can specify the name of the column in the database with `[Column("NameofColumn")]` or `[Column(ColumName="NameofColumn")]`, if you do not specify the name of the column, the name of the property will be used as the name of the column in the database.
The Column attribute also has other properties such as: 

- `Required`: if the column is required, the value of the column cannot be null.
- `AllowNull`: if the column allows null, the value of the column can be null.
- `MaxLength`: if the column has a maximum length, the value of the column cannot exceed the maximum length.
- `MinLength`: if the column has a minimum length, the value of the column cannot be less than the minimum length.
- `AllowEmpty`: if the column allows empty, the value of the column can be empty.
- `RegExPattern`: if the column has a regular expression pattern, the value of the column must match the regular expression pattern.
- `ErrorMaximunMessage`: if the column has a maximum length, you can specify the error message of the maximum length.
- `ErrorMinimunMessage`: if the column has a minimum length, you can specify the error message of the minimum length.
- `ErrorRequiredMessage`: if the column is required, you can specify the error message of the required.
- `ErrorAllowNullMessage`: if the column allows null, you can specify the error message of the allow null.
- `ErrorAllowEmptyMessage`: if the column allows empty, you can specify the error message of the allow empty.
- `ErrorRegExPatternMessage`: if the column has a regular expression pattern, you can specify the error message of the regular expression pattern.
- `Ignore`: if the column is ignored, the column will not be used in the database operations.
- `IgnoreInUpdate`: if the column is ignored in update, the column will not be used in the update operation.
- `IgnoreInDelete`: if the column is ignored in delete, the column will not be used in the delete operation.

## Relationships
The attribute `[Relation]` is used to define the relationship between the tables, you can specify the name of the foreign key in the database with `[Relation("NameofForeignKey")]` or `[Relation(ForeignKey="NameofForeignKey")]`, if you do not specify the name of the foreign key, the name of the property will be used as the name of the foreign key in the database.
Additionally you can specify the table of the foreign key with `[Relation("NameofForeignKey", "NameofTable")]` or `[Relation(ForeignKey="NameofForeignKey", RelationTable="NameofTable")]`, if you do not specify the name of the table, the name of the property will be used as the name of the table in the database.
if you use a store procedure and you want to load multiple tables at once then you can use the property `[Relation(TableNumber="TableNumber")]` or `[Relation(ForeignKey="NameofForeignKey", RelationTable="NameofTable", TableNumber="numberoftable")]`, if you do not specify the name of the number of table, the number of the table will be 0.
Additionally the relation table is update when you update the main table if you can't update the relation table you can use the property `[Relation(IgnoreInUpdate=true)]` or `[Relation(IgnoreInInsert=true)].

### Validation
DapperEntityORM also supports validation of the fields of the model, you can use the `IsValid` method to validate the fields of the model and the update, insert method invoke IsValid method, it will return a list of errors if the model is not valid.
```csharp
bool isValid = employee.IsValid(out List<string> errors);
``` 
The type of validation is defined by the attributes of the model, for example, if you want to validate that the field is not null, you can use the attribute `[Column(Required=true)]` and if you want to validate that the field has a maximum length, you can use the attribute `[Colum(MaxLength=50)]` and so on.

### List validation attributes:
- MaxLength
- MinLength
- AllowEmpty
- Required
- AllowNull
- RegExPattern

and you can modified the error message of the validation for example for MaxLength with the attribute `[Column(ErrorMaximunMessage="Error message")]`.

## Installation
You can install DapperEntityORM using NuGet:

```powershell
PM> Install-Package DapperEntityORM
```

## License
DapperEntityORM is licensed under the MIT License.

## Sonar Qube
[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=LuigimonSoft_DapperEntityORM)](https://sonarcloud.io/summary/new_code?id=LuigimonSoft_DapperEntityORM)
[![SonarQube Cloud](https://sonarcloud.io/images/project_badges/sonarcloud-light.svg)](https://sonarcloud.io/summary/new_code?id=LuigimonSoft_DapperEntityORM)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=LuigimonSoft_DapperEntityORM&metric=coverage)](https://sonarcloud.io/summary/new_code?id=LuigimonSoft_DapperEntityORM)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=LuigimonSoft_DapperEntityORM&metric=bugs)](https://sonarcloud.io/summary/new_code?id=LuigimonSoft_DapperEntityORM)
