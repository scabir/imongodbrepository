#iMongoDB Repository
##Usage
All entities must be derived from iMongoDbItem interface. This interface contains _id and some extra data needed.

iMondoDbItem:
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
    public PersonRepo(DbConfiguration repositoryConfiguration) 
        : base(repositoryConfiguration)
    {

    }
} 
```

```CSharp
static void Main(string[] args)
{
    //Instantiate your repo.
    var personRepo = new PersonRepo(new DbConfiguration()
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
