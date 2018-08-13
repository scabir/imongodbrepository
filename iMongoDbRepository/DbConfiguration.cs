using System;

namespace iMongoDbRepo
{
    public class DbConfiguration
    {
        public string ConnectionString { get; set; }

        public string DbName { get; set; }

        public string Collection { get; set; }

        public bool AutoGenerateIds { get; set; } = true;
    }
}
