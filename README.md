# DapperEntityORM

DapperEntityORM is a C# ORM library that leverages the power of Dapper to simplify database operations. It provides an easy-to-use API that allows you to interact with your database using strongly typed models.

## Usage

To use DapperEntityORM, you need to create a model that represents a table in your database. Each property of the model represents a column in the table. You can also define attributes on each property to specify additional metadata, such as whether the column can be null or has a maximum length.

Here's an example of a model that represents an "Employee" table:

```csharp
public class Employee
{
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public DateTime HireDate { get; set; }
    public int DepartmentId { get; set; }
}
```
Once you have your model defined, you can use the `DapperEntity<T>` class to interact with the corresponding table in the database. Here's an example of how to retrieve all employees from the "Employee" table:

```csharp
var employees = DapperEntity<Employee>.Select();
```
DapperEntityORM also supports more advanced database operations such as filtering, ordering, and grouping records. You can use the `Select` method to retrieve records from the database with various filtering options, such as `WHERE`, `GROUP BY`, and `HAVING`. You can also use the `OrderBy` and `OrderByDescending` methods to sort records by one or more columns. Additionally, you can use the `Count`, `Sum`, `Min`, `Max`, and `Average` methods to retrieve aggregate data from the database.

DapperEntityORM also supports other SQL commands such as `JOIN`, `UNION`, `INTERSECT`, and `EXCEPT`. These commands can be used by specifying a custom SQL query using the `Query` method.

## Installation
You can install DapperEntityORM using NuGet:

```powershell
PM> Install-Package DapperEntityORM
```

## License
DapperEntityORM is licensed under the MIT License.
