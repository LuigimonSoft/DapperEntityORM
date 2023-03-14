# DapperEntityORM

DapperEntityORM is a C# ORM library that leverages the power of Dapper to simplify database operations. It provides an easy-to-use API that allows you to interact with your database using strongly typed models.

## Usage

To use DapperEntityORM, you need to create a model that represents a table in your database. Each property of the model represents a column in the table. You can also define attributes on each property to specify additional metadata, such as whether the column can be null or has a maximum length.

Here's an example of a model that represents an "Employee" table:

```csharp
[Table("Employees")]
public class Employees : Entity<Employees>
{
    [Key]
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
    [Key]
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
    [Key]
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
Employees employee = new Employees();
employee.Load(2,database);
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