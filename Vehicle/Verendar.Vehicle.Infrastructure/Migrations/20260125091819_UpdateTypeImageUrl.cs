using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verendar.Vehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTypeImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "VehicleTypes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000001"),
                column: "IconUrl",
                value: "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp");

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000002-0000-0000-0000-000000000002"),
                column: "IconUrl",
                value: "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp");

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000003-0000-0000-0000-000000000003"),
                column: "IconUrl",
                value: "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp");

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000004-0000-0000-0000-000000000004"),
                column: "IconUrl",
                value: "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp");

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000005-0000-0000-0000-000000000005"),
                column: "IconUrl",
                value: "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp");

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000006-0000-0000-0000-000000000006"),
                column: "IconUrl",
                value: "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp");

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000007-0000-0000-0000-000000000007"),
                column: "IconUrl",
                value: "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp");

            migrationBuilder.UpdateData(
                table: "VehicleBrands",
                keyColumn: "Id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"),
                column: "LogoUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/brands/0ea879b6-a573-4691-b00d-9f078f25b605.png");

            migrationBuilder.UpdateData(
                table: "VehicleBrands",
                keyColumn: "Id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"),
                column: "LogoUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/brands/0ea879b6-a573-4691-b00d-9f078f25b605.png");

            migrationBuilder.UpdateData(
                table: "VehicleTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ImageUrl",
                value: "https://d3iova6424vljy.cloudfront.net/assets/c51a1e61-9aac-4d59-a01c-5b615be9e794.jpg");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000001-0000-0000-0000-000000000001"),
                column: "ImageUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000002-0000-0000-0000-000000000002"),
                column: "ImageUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000003-0000-0000-0000-000000000003"),
                column: "ImageUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000004-0000-0000-0000-000000000004"),
                column: "ImageUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000005-0000-0000-0000-000000000005"),
                column: "ImageUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000006-0000-0000-0000-000000000006"),
                column: "ImageUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000007-0000-0000-0000-000000000007"),
                column: "ImageUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000008-0000-0000-0000-000000000008"),
                column: "ImageUrl",
                value: "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "VehicleTypes");

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000001-0000-0000-0000-000000000001"),
                column: "IconUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000002-0000-0000-0000-000000000002"),
                column: "IconUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000003-0000-0000-0000-000000000003"),
                column: "IconUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000004-0000-0000-0000-000000000004"),
                column: "IconUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000005-0000-0000-0000-000000000005"),
                column: "IconUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000006-0000-0000-0000-000000000006"),
                column: "IconUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "PartCategories",
                keyColumn: "Id",
                keyValue: new Guid("c0000007-0000-0000-0000-000000000007"),
                column: "IconUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "VehicleBrands",
                keyColumn: "Id",
                keyValue: new Guid("b0000001-0000-0000-0000-000000000001"),
                column: "LogoUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "VehicleBrands",
                keyColumn: "Id",
                keyValue: new Guid("b0000002-0000-0000-0000-000000000002"),
                column: "LogoUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000001-0000-0000-0000-000000000001"),
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000002-0000-0000-0000-000000000002"),
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000003-0000-0000-0000-000000000003"),
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000004-0000-0000-0000-000000000004"),
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000005-0000-0000-0000-000000000005"),
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000006-0000-0000-0000-000000000006"),
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000007-0000-0000-0000-000000000007"),
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "VehicleVariants",
                keyColumn: "Id",
                keyValue: new Guid("e0000008-0000-0000-0000-000000000008"),
                column: "ImageUrl",
                value: "");
        }
    }
}
