# iMongoDbRepository
Requires .Net Standard 2.1+

## Usage
All entities must be derived from iMongoDbItem interface. This interface contains _id and some extra data needed.

iMongoDbItem:
```Csharp
public interface IMongoDbItem
{
    string _id { get; set; }
    DateTime CreatedOn { get; set; }
    DateTime ModifiedOn { get; set; }
    bool Deleted { get; set; }
}
```

Let's say you have an object called person as follows;
```CSharp
public class Person: IMongoDbItem
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string _id { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public bool Deleted { get; set; }
}
```

First create a repository class;
```Csharp
public class PersonRepo : MongoDbRepository<Person>
{

} 
```

```CSharp
static void Main(string[] args)
{
    //Instantiate your repo.
    var personRepo = new PersonRepo();

    //Configure it
    personRepo.Configure(new DbConfiguration()
    {
        ConnectionString = "mongodb://localhost:27017",//your MondoDb connection string
        DbName = "iTest",
        Collection = "iCollection"
    });

    //Your object is ready
    var person = new Person();

    //Insert person object
    personRepo.Insert(person);

    //Retreave all items in the db
    var all = personRepo.All();
}
```

First create your interface. If you will not add extra methods, just create an empty one. 
Then, derive your class from the interface and MongoDbRepository abstract class as follows. By this way, when this class is instanciated, methods on the abstract class will be called.

## Usage with Dependancy Injection
```Csharp
public class IPersonRepo : MongoDbRepository<Person>
{

} 

public class PersonRepo : IPersonRepo, MongoDbRepository<Person>
{

} 

```

Sample container configuration:
```Csharp
container.For<IPersonRepo>().Use<PersonRepo>();
```

When you instanciate or inject the IPersonRepo, you have the repository. However, do not forget to configure it by calling Configure method.

```Csharp

personRepo.Configure(new DbConfiguration()
    {
        ConnectionString = "mongodb://localhost:27017",//your MondoDb connection string
        DbName = "iTest",
        Collection = "iCollection"
    });
```