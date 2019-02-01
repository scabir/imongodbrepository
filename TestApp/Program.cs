using System;
using System.Collections.Generic;
using iMongoDbRepository;

namespace TestApp
{
    public class Person
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }
    public class PersonEntity : Person, IMongoDbItem
    {
        public string _id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool Deleted { get; set; }
    }
    public class PersonRepo : MongoDbRepository<PersonEntity>
    {

    }
    class Program
    {
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
            var items = new List<PersonEntity>();
            int max = 1200000;

            //for (int i = 0; i < max; i++)
            //{
            //    items.Add(new PersonEntity());

            //    if (i % 10000 == 0 || i == max - 1)
            //    {
            //        personRepo.Insert(items);
            //        items = new List<PersonEntity>();
            //    }
            //}


            //Retreave all items in the db
            var all = personRepo.All();
        }
    }
}
