using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Infrastructure.Data
{
    public static class VehicleDataSeeder
    {
        private static readonly DateTime FixedDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly Guid SystemUserId = Guid.Empty;

        // Fixed GUIDs for motorcycle type
        private static readonly Guid MotorcycleTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Brands
        private static readonly Guid HondaBrandId = Guid.Parse("b0000001-0000-0000-0000-000000000001");
        private static readonly Guid YamahaBrandId = Guid.Parse("b0000002-0000-0000-0000-000000000002");

        // Models
        private static readonly Guid WaveAlphaModelId = Guid.Parse("a0000001-0000-0000-0000-000000000001");
        private static readonly Guid AirBladeModelId = Guid.Parse("a0000002-0000-0000-0000-000000000002");
        private static readonly Guid ExciterModelId = Guid.Parse("a0000003-0000-0000-0000-000000000003");
        private static readonly Guid SiriusModelId = Guid.Parse("a0000004-0000-0000-0000-000000000004");

        // Variants - using 'e' prefix, 2 variants per model
        private static readonly Guid WaveAlphaRedVariantId = Guid.Parse("e0000001-0000-0000-0000-000000000001");
        private static readonly Guid WaveAlphaBlackVariantId = Guid.Parse("e0000002-0000-0000-0000-000000000002");
        private static readonly Guid AirBladeWhiteVariantId = Guid.Parse("e0000003-0000-0000-0000-000000000003");
        private static readonly Guid AirBladeBlueVariantId = Guid.Parse("e0000004-0000-0000-0000-000000000004");
        private static readonly Guid ExciterBlackVariantId = Guid.Parse("e0000005-0000-0000-0000-000000000005");
        private static readonly Guid ExciterRedVariantId = Guid.Parse("e0000006-0000-0000-0000-000000000006");
        private static readonly Guid SiriusBlueVariantId = Guid.Parse("e0000007-0000-0000-0000-000000000007");
        private static readonly Guid SiriusBlackVariantId = Guid.Parse("e0000008-0000-0000-0000-000000000008");

        // Part Categories
        private static readonly Guid EngineOilCategoryId = Guid.Parse("c0000001-0000-0000-0000-000000000001");
        private static readonly Guid TireCategoryId = Guid.Parse("c0000002-0000-0000-0000-000000000002");
        private static readonly Guid BatteryCategoryId = Guid.Parse("c0000003-0000-0000-0000-000000000003");
        private static readonly Guid BrakePadCategoryId = Guid.Parse("c0000004-0000-0000-0000-000000000004");
        private static readonly Guid SparkPlugCategoryId = Guid.Parse("c0000005-0000-0000-0000-000000000005");
        private static readonly Guid AirFilterCategoryId = Guid.Parse("c0000006-0000-0000-0000-000000000006");
        private static readonly Guid ChainCategoryId = Guid.Parse("c0000007-0000-0000-0000-000000000007");

        public static List<VehicleType> GetVehicleTypes()
        {
            return new List<VehicleType>
            {
                new VehicleType
                {
                    Id = MotorcycleTypeId,
                    Name = "Xe máy",
                    Code = "MOTORCYCLE",
                    Description = "Xe máy hai bánh, bao gồm xe số, xe tay ga, xe côn tay",
                    ImageUrl = "https://d3iova6424vljy.cloudfront.net/assets/c51a1e61-9aac-4d59-a01c-5b615be9e794.jpg",
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                }
            };
        }

        public static List<VehicleBrand> GetVehicleBrands()
        {
            return new List<VehicleBrand>
            {
                new VehicleBrand
                {
                    Id = HondaBrandId,
                    VehicleTypeId = MotorcycleTypeId,
                    Name = "Honda",
                    Code = "HONDA",
                    LogoUrl = "https://d3iova6424vljy.cloudfront.net/master/brands/0ea879b6-a573-4691-b00d-9f078f25b605.png",
                    Website = "https://www.honda.com.vn",
                    SupportPhone = "18001076",
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new VehicleBrand
                {
                    Id = YamahaBrandId,
                    VehicleTypeId = MotorcycleTypeId,
                    Name = "Yamaha",
                    Code = "YAMAHA",
                    LogoUrl = "https://d3iova6424vljy.cloudfront.net/master/brands/0ea879b6-a573-4691-b00d-9f078f25b605.png",
                    Website = "https://www.yamaha-motor.com.vn",
                    SupportPhone = "18006019",
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                }
            };
        }

        public static List<VehicleModel> GetVehicleModels()
        {
            return new List<VehicleModel>
            {
                // Honda Models
                new VehicleModel
                {
                    Id = WaveAlphaModelId,
                    VehicleBrandId = HondaBrandId,
                    Name = "Wave Alpha",
                    Code = "WAVE-ALPHA",
                    ManufactureYear = 2020,
                    FuelType = VehicleFuelType.Gasoline,
                    TransmissionType = VehicleTransmissionType.Manual,
                    EngineDisplacement = 110,
                    EngineCapacity = 1.10m,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new VehicleModel
                {
                    Id = AirBladeModelId,
                    VehicleBrandId = HondaBrandId,
                    Name = "Air Blade",
                    Code = "AIR-BLADE",
                    ManufactureYear = 2021,
                    FuelType = VehicleFuelType.Gasoline,
                    TransmissionType = VehicleTransmissionType.Automatic,
                    EngineDisplacement = 125,
                    EngineCapacity = 1.25m,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Yamaha Models
                new VehicleModel
                {
                    Id = ExciterModelId,
                    VehicleBrandId = YamahaBrandId,
                    Name = "Exciter",
                    Code = "EXCITER",
                    ManufactureYear = 2021,
                    FuelType = VehicleFuelType.Gasoline,
                    TransmissionType = VehicleTransmissionType.Manual,
                    EngineDisplacement = 155,
                    EngineCapacity = 1.55m,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new VehicleModel
                {
                    Id = SiriusModelId,
                    VehicleBrandId = YamahaBrandId,
                    Name = "Sirius",
                    Code = "SIRIUS",
                    ManufactureYear = 2020,
                    FuelType = VehicleFuelType.Gasoline,
                    TransmissionType = VehicleTransmissionType.Manual,
                    EngineDisplacement = 110,
                    EngineCapacity = 1.10m,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                }
            };
        }

        public static List<VehicleVariant> GetVehicleVariants()
        {
            return new List<VehicleVariant>
            {
                // Wave Alpha variants
                new VehicleVariant
                {
                    Id = WaveAlphaRedVariantId,
                    VehicleModelId = WaveAlphaModelId,
                    Color = "Đỏ đen",
                    HexCode = "#DC143C",
                    ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new VehicleVariant
                {
                    Id = WaveAlphaBlackVariantId,
                    VehicleModelId = WaveAlphaModelId,
                    Color = "Đen bạc",
                    HexCode = "#000000",
                    ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Air Blade variants
                new VehicleVariant
                {
                    Id = AirBladeWhiteVariantId,
                    VehicleModelId = AirBladeModelId,
                    Color = "Trắng ngọc trai",
                    HexCode = "#FFFFFF",
                    ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new VehicleVariant
                {
                    Id = AirBladeBlueVariantId,
                    VehicleModelId = AirBladeModelId,
                    Color = "Xanh dương",
                    HexCode = "#4169E1",
                    ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Exciter variants
                new VehicleVariant
                {
                    Id = ExciterBlackVariantId,
                    VehicleModelId = ExciterModelId,
                    Color = "Đen nhám",
                    HexCode = "#1C1C1C",
                    ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new VehicleVariant
                {
                    Id = ExciterRedVariantId,
                    VehicleModelId = ExciterModelId,
                    Color = "Đỏ GP",
                    HexCode = "#FF0000",
                    ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Sirius variants
                new VehicleVariant
                {
                    Id = SiriusBlueVariantId,
                    VehicleModelId = SiriusModelId,
                    Color = "Xanh đen",
                    HexCode = "#000080",
                    ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new VehicleVariant
                {
                    Id = SiriusBlackVariantId,
                    VehicleModelId = SiriusModelId,
                    Color = "Đen nhám",
                    HexCode = "#000000",
                    ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/models/04c7786f-3546-4526-8fe4-6e3ff3196d18.png",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                }
            };
        }

        public static List<PartCategory> GetPartCategories()
        {
            return new List<PartCategory>
            {
                new PartCategory
                {
                    Id = EngineOilCategoryId,
                    Name = "Dầu nhớt động cơ",
                    Code = "ENGINE-OIL",
                    Description = "Dầu bôi trơn động cơ, cần thay định kỳ theo km hoặc thời gian",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 1,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = true,
                    RequiresTimeTracking = true,
                    AllowsMultipleInstances = false,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartCategory
                {
                    Id = TireCategoryId,
                    Name = "Lốp xe",
                    Code = "TIRE",
                    Description = "Lốp trước và lốp sau, thay khi mòn hoặc đạt tuổi thọ",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 2,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = true,
                    RequiresTimeTracking = true,
                    AllowsMultipleInstances = true,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartCategory
                {
                    Id = BatteryCategoryId,
                    Name = "Ắc quy",
                    Code = "BATTERY",
                    Description = "Bình điện ắc quy, thay khi hết tuổi thọ hoặc không giữ điện",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 3,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = false,
                    RequiresTimeTracking = true,
                    AllowsMultipleInstances = false,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartCategory
                {
                    Id = BrakePadCategoryId,
                    Name = "Má phanh",
                    Code = "BRAKE-PAD",
                    Description = "Má phanh trước và sau, thay khi mòn",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 4,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = true,
                    RequiresTimeTracking = true,
                    AllowsMultipleInstances = true,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartCategory
                {
                    Id = SparkPlugCategoryId,
                    Name = "Bugi",
                    Code = "SPARK-PLUG",
                    Description = "Bugi đánh lửa động cơ, thay định kỳ",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 5,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = true,
                    RequiresTimeTracking = true,
                    AllowsMultipleInstances = false,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartCategory
                {
                    Id = AirFilterCategoryId,
                    Name = "Lọc gió",
                    Code = "AIR-FILTER",
                    Description = "Lọc không khí động cơ, cần vệ sinh và thay định kỳ",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 6,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = true,
                    RequiresTimeTracking = true,
                    AllowsMultipleInstances = false,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartCategory
                {
                    Id = ChainCategoryId,
                    Name = "Nhông sên dĩa",
                    Code = "CHAIN-SPROCKET",
                    Description = "Bộ truyền động xích và nhông, thay khi mòn",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 7,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = true,
                    RequiresTimeTracking = false,
                    AllowsMultipleInstances = false,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                }
            };
        }

        public static List<DefaultMaintenanceSchedule> GetDefaultMaintenanceSchedules()
        {
            // Chỉ tạo schedule cho 1 model (Wave Alpha) làm mẫu
            // Các model khác có thể thêm sau qua API
            return new List<DefaultMaintenanceSchedule>
            {
                // Wave Alpha - Dầu nhớt
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000001-0000-0000-0000-000000000001"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = EngineOilCategoryId,
                    InitialKm = 1000,
                    KmInterval = 3000,
                    MonthsInterval = 4,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                // Wave Alpha - Lốp xe
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000002-0000-0000-0000-000000000002"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = TireCategoryId,
                    InitialKm = 0,
                    KmInterval = 20000,
                    MonthsInterval = 36,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                // Wave Alpha - Ắc quy
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000003-0000-0000-0000-000000000003"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = BatteryCategoryId,
                    InitialKm = 0,
                    KmInterval = 0,
                    MonthsInterval = 24,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                // Wave Alpha - Má phanh
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000004-0000-0000-0000-000000000004"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = BrakePadCategoryId,
                    InitialKm = 0,
                    KmInterval = 10000,
                    MonthsInterval = 12,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                // Wave Alpha - Bugi
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000005-0000-0000-0000-000000000005"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = SparkPlugCategoryId,
                    InitialKm = 0,
                    KmInterval = 8000,
                    MonthsInterval = 12,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                // Wave Alpha - Lọc gió
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000006-0000-0000-0000-000000000006"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = AirFilterCategoryId,
                    InitialKm = 0,
                    KmInterval = 6000,
                    MonthsInterval = 6,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                // Wave Alpha - Nhông sên dĩa
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000007-0000-0000-0000-000000000007"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = ChainCategoryId,
                    InitialKm = 0,
                    KmInterval = 15000,
                    MonthsInterval = 0,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                }
            };
        }
    }
}
