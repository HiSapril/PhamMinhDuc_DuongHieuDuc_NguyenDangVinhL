using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASCWeb1.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceRequestMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceRequestMessages",
                columns: table => new
                {
                    PartitionKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RowKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FromDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestMessages", x => new { x.PartitionKey, x.RowKey });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequestMessages");
        }
    }
}
