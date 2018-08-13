using System;
using System.Collections.Generic;
using System.Text;

namespace iMongoDbRepo
{
    public interface IMongoDbItem
    {
        string _id { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime ModifiedOn { get; set; }
        bool Deleted { get; set; }
    }
}
