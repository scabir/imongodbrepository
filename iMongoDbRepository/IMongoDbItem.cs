using System;

namespace iMongoDbRepository
{
    public interface IMongoDbItem
    {
        string _id { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime ModifiedOn { get; set; }
        bool Deleted { get; set; }
    }
}
