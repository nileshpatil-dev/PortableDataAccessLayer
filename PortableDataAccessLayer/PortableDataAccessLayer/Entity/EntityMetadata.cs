using System;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace DataAccess.Entity
{
    public class EntityMetadata
    {
        public bool IsMasterEntity { get; set; }
        public string Name { get; set; }
        public string Query { get; set; }
        public SqlDataAdapter Adapter { get; set; }
        public string TableName { get; set; }
        public string TableIdentityColumn { get; set; }
        public List<EntityMetadataDependency> Dependencies { get; set; }
    }

    public class EntityMetadataDependency
    {
        public string Name { get; set; }
        public string TableName { get; set; }
        public string ParentTableIdentityColumn { get; set; }
        public string ChildTableIdentityColumn { get; set; }
    }
}
