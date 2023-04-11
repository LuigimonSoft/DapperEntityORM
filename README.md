# DapperEntityORM

DapperEntityORM is a C# ORM library that leverages the power of Dapper to simplify database operations. It provides an easy-to-use API that allows you to interact with your database using strongly typed models.

## Usage

To use DapperEntityORM, you need to create a model that represents a table in your database. Each property of the model represents a column in the table. You can also define attributes on each property to specify additional metadata, such as whether the column can be null or has a maximum length.

Here's an example of a model that represents an "Employee" table:

```csharp
[Table("Employees")]
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

DapperEntityORM also supports more advanced database operations such as filtering, ordering, and grouping records. You can use the `Select` method to retrieve records from the database with various filtering options, such as `WHERE`, `GROUP BY`, and `HAVING`. You can also use the `OrderBy` and `OrderByDescending` methods to sort records by one or more columns. Additionally, you can use the `Count`, `Sum`, `Min`, `Max`, and `Average` methods to retrieve aggregate data from the database.

DapperEntityORM also supports other SQL commands such as `JOIN`, `UNION`, `INTERSECT`, and `EXCEPT`. These commands can be used by specifying a custom SQL query using the `Query` method. (Implementing in the future)

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

and you can use the attribute `[Column("DatabaseNameColum")]` or `[Column(ColumName="DatabaseNameColum")]` to specify the name of the column in the database.

## Installation
You can install DapperEntityORM using NuGet:

```powershell
PM> Install-Package DapperEntityORM
```

## License
DapperEntityORM is licensed under the MIT License.
