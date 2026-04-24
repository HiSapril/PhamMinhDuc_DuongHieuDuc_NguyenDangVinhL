using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Model.Models
{
    public class MasterDataValue : BaseEntity, IAuditTracker
    {
        public MasterDataValue() { }

        public MasterDataValue(string masterDataPartitionKey, string value)
        {
            this.PartitionKey = masterDataPartitionKey;
            this.RowKey = Guid.NewGuid().ToString();
            // Lưu ý: Trong ảnh, tham số 'value' chưa được gán vào property nào, 
            // thông thường nó sẽ được gán cho property 'Name'.
            this.Name = value;
            this.CreatedBy = string.Empty;
            this.UpdatedBy = string.Empty;
        }

        public bool IsActive { get; set; }
        public required string Name { get; set; }
    }
}
