using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Model
{
    public class BaseEntity
    {
        public required string PartitionKey { get; set; }
        public required string RowKey { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public required string CreatedBy { get; set; }
        public required string UpdatedBy { get; set; }

        public BaseEntity()
        {

        }
    }
}
