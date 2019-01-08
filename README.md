# iMongoDbRepository
Requires .Net Standard 2.1+

## Nuget Package
```
PM> Install-Package iMongoDbRepository
```
URL: https://www.nuget.org/packages/iMongoDbRepository


## Usage
All entities must be derived from an interface called ```IMongoDbItem```. This interface contains a mandatory field called ```_id```  and some extra data needed.

Let's say you have an object called person as follows;
```CSharp
public class Person
{
    public string Name { get; set; }
    public string Surname { get; set; }
}
```

Then you create an entity class for the person object just to satisfy IMongoDbItem interface.
```CSharp
public class PersonEntity: Person, IMongoDbItem
{
    public string _id { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public bool Deleted { get; set; }
}
```

First create a repository class;
```Csharp
public class PersonRepo : MongoDbRepository<PersonEntity>
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
    var person = new PersonEntity();

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
public class IPersonRepo : IRepository<PersonEntity>
{

} 

public class PersonRepo : MongoDbRepository<PersonEntity>, IPersonRepo
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
