using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Model.Models
{
    public class MasterDataKey : BaseEntity
    {
        public MasterDataKey() { }

        public MasterDataKey(string key)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = key;
            this.Name = string.Empty;
            this.CreatedBy = string.Empty;
            this.UpdatedBy = string.Empty;
        }

        public bool IsActive { get; set; }
        public required string Name { get; set; }
    }
}
