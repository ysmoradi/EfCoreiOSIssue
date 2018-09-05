This is a workaround for following issue:
https://github.com/xamarin/xamarin-macios/issues/4749

It happens when you insert/update a model with nullable properties to sqlite database using entity framework core on physical iOS devices only.

This problem has two workarounds (As far as we know!)

1- Use something like Sqlite PCL to insert/update records and use entity framework core for quering purposes only.

2- Define nullable props as object instead of Guid? as an example.
Based on information we've got from https://github.com/aspnet/Microsoft.Data.Sqlite/wiki/Data-Type-Mappings & https://github.com/aspnet/Microsoft.Data.Sqlite/blob/master/src/Microsoft.Data.Sqlite.Core/SqliteValueBinder.cs#L121 
we've developed some extension methods to map thoses object properties to correct database types. At runtime, those object properties will have the correct clr type too.

Sample:

```csharp

public class Customer
{
    public Guid Id { get; set; } // you don't have to define non nullable properties as object
    public object Salary { get; set; } // A nullable decimal property, instead of public decimal? Salary {get;set;}
}

modelBuilder.Entity<Customer>()
    .Property(c => c.Salary).AsNullableDecimal(); // we've introduced AsNullableDecimal extension method.
    
Customer customer = dbContext.Customers.First();

bool isDecimal = customer.Salary is decimal; // it's true!

```

Let's make this workaround as complete as possible. We can change our properties to their real types later when the root cause gets solved!

Note that to use this feature, you'll need VS 15.8+ and entity framework core 2.1+
