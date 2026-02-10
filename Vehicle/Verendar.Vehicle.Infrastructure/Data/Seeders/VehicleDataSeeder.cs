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
        private static readonly Guid BrakeFluidCategoryId = Guid.Parse("c0000008-0000-0000-0000-000000000008");
        private static readonly Guid OilFilterCategoryId = Guid.Parse("c0000009-0000-0000-0000-000000000009");
        private static readonly Guid CoolantCategoryId = Guid.Parse("c0000010-0000-0000-0000-000000000010");

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
                    IdentificationSigns = "Dầu đen, nhớt nhớt; động cơ nóng bất thường; đèn báo dầu sáng; tiếng kêu kim loại từ động cơ.",
                    ConsequencesIfNotHandled = "Mài mòn nhanh, kẹt piston; hỏng động cơ; chi phí sửa chữa rất cao hoặc phải thay máy.",
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
                    IdentificationSigns = "Gai lốp mòn dưới 1.6mm; nứt rạn; phồng lốp; lốp non hơi thường xuyên; rung lái khi chạy.",
                    ConsequencesIfNotHandled = "Trượt, mất lái khi phanh/trời mưa; nổ lốp khi chạy tốc độ cao; tai nạn nghiêm trọng.",
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
                    IdentificationSigns = "Khởi động yếu hoặc không nổ; đèn pha mờ; ắc quy phồng, rỉ nước; xe để vài ngày là hết điện.",
                    ConsequencesIfNotHandled = "Chết máy giữa đường; hỏng bình; ảnh hưởng bộ sạc và thiết bị điện; không khởi động được.",
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
                    IdentificationSigns = "Tiếng kêu ken két khi phanh; phanh không ăn; tay phanh/ pedal bị trễ; đĩa phanh xước, rỗ.",
                    ConsequencesIfNotHandled = "Mất phanh; đĩa phanh hỏng theo; va chạm, tai nạn do không dừng kịp.",
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
                    IdentificationSigns = "Khó nổ, giật khi tăng ga; tốn xăng; động cơ rung; bugi đen, dính dầu hoặc cháy trắng đầu điện cực.",
                    ConsequencesIfNotHandled = "Chết máy; đánh lửa kém gây cháy không hết nhiên liệu, hỏng cat; hao xăng, giảm công suất.",
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
                    IdentificationSigns = "Xe yếu, không bốc; tốn xăng; lọc gió bẩn, rách hoặc ẩm mốc; động cơ nổ không đều.",
                    ConsequencesIfNotHandled = "Bụi cát vào buồng đốt, mài mòn xy-lanh; giảm công suất; hỏng bugi, hao xăng lâu dài.",
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
                    IdentificationSigns = "Xích kêu lạch cạch; xích trùng dù đã chỉnh; răng nhông mòn vẹt; xích rỉ, khô dầu.",
                    ConsequencesIfNotHandled = "Đứt xích khi chạy, bó bánh; hỏng nhông, moay-ơ; nguy cơ té xe, hỏng hộp số.",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartCategory
                {
                    Id = BrakeFluidCategoryId,
                    Name = "Dầu phanh",
                    Code = "BRAKE-FLUID",
                    Description = "Dầu phanh thủy lực, thay định kỳ theo thời gian hoặc km",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 8,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = true,
                    RequiresTimeTracking = true,
                    AllowsMultipleInstances = false,
                    IdentificationSigns = "Dầu đổi màu đen hoặc vẩn đục; mức dầu thấp; phanh mềm hoặc trễ; có nước lẫn vào.",
                    ConsequencesIfNotHandled = "Sôi dầu khi phanh gấp; mất phanh; hỏng xy-lanh phanh; tai nạn do không dừng kịp.",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartCategory
                {
                    Id = OilFilterCategoryId,
                    Name = "Lọc dầu",
                    Code = "OIL-FILTER",
                    Description = "Lọc dầu động cơ, thay kèm mỗi lần thay dầu nhớt",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 9,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = true,
                    RequiresTimeTracking = true,
                    AllowsMultipleInstances = false,
                    IdentificationSigns = "Thay cùng chu kỳ dầu nhớt; lọc tắc khi đèn dầu sáng thường xuyên; dầu đen nhanh bất thường.",
                    ConsequencesIfNotHandled = "Dầu không lọc sạch, mài mòn động cơ; tắc lọc gây thiếu dầu; hỏng máy.",
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartCategory
                {
                    Id = CoolantCategoryId,
                    Name = "Nước làm mát",
                    Code = "COOLANT",
                    Description = "Dung dịch làm mát động cơ, kiểm tra và thay định kỳ",
                    IconUrl = "https://d3iova6424vljy.cloudfront.net/assets/OIP.webp",
                    DisplayOrder = 10,
                    Status = EntityStatus.Active,
                    RequiresOdometerTracking = true,
                    RequiresTimeTracking = true,
                    AllowsMultipleInstances = false,
                    IdentificationSigns = "Mức nước trong bình thấp; nước đổi màu rỉ; động cơ nóng quá; rò rỉ dưới gầm.",
                    ConsequencesIfNotHandled = "Động cơ quá nhiệt; bó máy; hỏng gioăng, xy-lanh; chi phí sửa rất cao.",
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
                },
                // Wave Alpha - Dầu phanh
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000008-0000-0000-0000-000000000008"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = BrakeFluidCategoryId,
                    InitialKm = 0,
                    KmInterval = 20000,
                    MonthsInterval = 24,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                // Wave Alpha - Lọc dầu
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000009-0000-0000-0000-000000000009"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = OilFilterCategoryId,
                    InitialKm = 1000,
                    KmInterval = 3000,
                    MonthsInterval = 4,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                // Wave Alpha - Nước làm mát
                new DefaultMaintenanceSchedule
                {
                    Id = Guid.Parse("d0000010-0000-0000-0000-000000000010"),
                    VehicleModelId = WaveAlphaModelId,
                    PartCategoryId = CoolantCategoryId,
                    InitialKm = 0,
                    KmInterval = 20000,
                    MonthsInterval = 24,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                }
            };
        }

        public static List<PartProduct> GetPartProducts()
        {
            return new List<PartProduct>
            {
                // Engine Oil Products
                new PartProduct
                {
                    Id = Guid.Parse("90000001-0000-0000-0000-000000000001"),
                    PartCategoryId = EngineOilCategoryId,
                    Name = "Shell Advance AX7 10W-40",
                    Brand = "Shell",
                    
                    Description = "Dầu nhớt bán tổng hợp cao cấp cho xe số",
                    
                    ReferencePrice = 85000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartProduct
                {
                    Id = Guid.Parse("90000002-0000-0000-0000-000000000002"),
                    PartCategoryId = EngineOilCategoryId,
                    Name = "Castrol Power1 10W-40",
                    Brand = "Castrol",
                    
                    Description = "Dầu nhớt tổng hợp cho xe tay ga",
                    
                    ReferencePrice = 95000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartProduct
                {
                    Id = Guid.Parse("90000003-0000-0000-0000-000000000003"),
                    PartCategoryId = EngineOilCategoryId,
                    Name = "Motul 5100 10W-40",
                    Brand = "Motul",
                    
                    Description = "Dầu nhớt bán tổng hợp cao cấp cho xe côn tay",
                    
                    ReferencePrice = 145000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Tire Products
                new PartProduct
                {
                    Id = Guid.Parse("90000004-0000-0000-0000-000000000004"),
                    PartCategoryId = TireCategoryId,
                    Name = "Michelin Pilot Street 80/90-17",
                    Brand = "Michelin",
                    
                    Description = "Lốp trước cho xe số",
                    
                    ReferencePrice = 420000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartProduct
                {
                    Id = Guid.Parse("90000005-0000-0000-0000-000000000005"),
                    PartCategoryId = TireCategoryId,
                    Name = "Dunlop D307 90/80-17",
                    Brand = "Dunlop",
                    
                    Description = "Lốp sau cho xe tay ga",
                    
                    ReferencePrice = 380000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Battery Products
                new PartProduct
                {
                    Id = Guid.Parse("90000006-0000-0000-0000-000000000006"),
                    PartCategoryId = BatteryCategoryId,
                    Name = "GS GTZ5S 12V 4Ah",
                    Brand = "GS Battery",
                    
                    Description = "Ắc quy khô cho xe máy",
                    
                    ReferencePrice = 350000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartProduct
                {
                    Id = Guid.Parse("90000007-0000-0000-0000-000000000007"),
                    PartCategoryId = BatteryCategoryId,
                    Name = "Yuasa YTX7A-BS 12V 6Ah",
                    Brand = "Yuasa",
                    
                    Description = "Ắc quy khô cao cấp cho xe tay ga",
                    
                    ReferencePrice = 480000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Brake Pad Products
                new PartProduct
                {
                    Id = Guid.Parse("90000008-0000-0000-0000-000000000008"),
                    PartCategoryId = BrakePadCategoryId,
                    Name = "Honda Genuine Brake Pad",
                    Brand = "Honda",
                    
                    Description = "Má phanh chính hãng Honda",
                    
                    ReferencePrice = 85000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Spark Plug Products
                new PartProduct
                {
                    Id = Guid.Parse("90000009-0000-0000-0000-000000000009"),
                    PartCategoryId = SparkPlugCategoryId,
                    Name = "NGK CPR7EA-9",
                    Brand = "NGK",
                    
                    Description = "Bugi iridium cao cấp",
                    
                    ReferencePrice = 95000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },
                new PartProduct
                {
                    Id = Guid.Parse("90000010-0000-0000-0000-000000000010"),
                    PartCategoryId = SparkPlugCategoryId,
                    Name = "Denso U24EPR9",
                    Brand = "Denso",
                    
                    Description = "Bugi chuẩn cho xe Honda",
                    
                    ReferencePrice = 45000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Air Filter Products
                new PartProduct
                {
                    Id = Guid.Parse("90000011-0000-0000-0000-000000000011"),
                    PartCategoryId = AirFilterCategoryId,
                    Name = "Ferrox Air Filter",
                    Brand = "Ferrox",
                    
                    Description = "Lọc gió thể thao có thể rửa",
                    
                    ReferencePrice = 120000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Chain Products
                new PartProduct
                {
                    Id = Guid.Parse("90000012-0000-0000-0000-000000000012"),
                    PartCategoryId = ChainCategoryId,
                    Name = "RK 428HSB Chain",
                    Brand = "RK",
                    
                    Description = "Sên cao cấp cho xe côn tay",
                    
                    ReferencePrice = 380000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Oil Filter Products
                new PartProduct
                {
                    Id = Guid.Parse("90000013-0000-0000-0000-000000000013"),
                    PartCategoryId = OilFilterCategoryId,
                    Name = "Honda Genuine Oil Filter",
                    Brand = "Honda",
                    
                    Description = "Lọc dầu chính hãng Honda",
                    
                    ReferencePrice = 65000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Brake Fluid Products
                new PartProduct
                {
                    Id = Guid.Parse("90000014-0000-0000-0000-000000000014"),
                    PartCategoryId = BrakeFluidCategoryId,
                    Name = "Castrol DOT 3 Brake Fluid",
                    Brand = "Castrol",
                    
                    Description = "Dầu phanh DOT 3",
                    
                    ReferencePrice = 45000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                },

                // Coolant Products
                new PartProduct
                {
                    Id = Guid.Parse("90000015-0000-0000-0000-000000000015"),
                    PartCategoryId = CoolantCategoryId,
                    Name = "Honda Genuine Coolant",
                    Brand = "Honda",
                    
                    Description = "Nước làm mát chính hãng Honda",
                    
                    ReferencePrice = 85000,
                    Status = EntityStatus.Active,
                    CreatedAt = FixedDate,
                    CreatedBy = SystemUserId
                }
            };
        }
    }
}
