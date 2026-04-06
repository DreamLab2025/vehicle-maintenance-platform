using Verendar.Garage.Domain.ValueObjects;
using GarageEntity = global::Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Infrastructure.Data.Seeders;

public static class GarageBranchDevSeeder
{
    public static readonly Guid DevOwnerUserId = Guid.Parse("11111111-1111-1111-1111-111111111222");

    public static readonly Guid DevGarageId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public static readonly Guid DevBranchHoanKiemId = Guid.Parse("22222222-2222-2222-2222-222222222211");
    public static readonly Guid DevBranchBaDinhId = Guid.Parse("22222222-2222-2222-2222-222222222212");

    public static readonly Guid DevProductHoanKiemOilFilterId = Guid.Parse("55555555-5555-5555-5555-555555555501");
    public static readonly Guid DevProductBaDinhBrakePadId = Guid.Parse("55555555-5555-5555-5555-555555555502");
    public static readonly Guid DevProductHoanKiemBatteryId = Guid.Parse("55555555-5555-5555-5555-555555555503");
    public static readonly Guid DevProductHoanKiemAirFilterId = Guid.Parse("55555555-5555-5555-5555-555555555504");
    public static readonly Guid DevProductBaDinhTireId = Guid.Parse("55555555-5555-5555-5555-555555555505");
    public static readonly Guid DevProductBaDinhSparkPlugId = Guid.Parse("55555555-5555-5555-5555-555555555506");

    public static readonly Guid DevPartCategoryOilFilterId = Guid.Parse("c0000009-0000-0000-0000-000000000009");
    public static readonly Guid DevPartCategoryBrakePadId = Guid.Parse("c0000004-0000-0000-0000-000000000004");
    public static readonly Guid DevPartCategoryBatteryId = Guid.Parse("c0000003-0000-0000-0000-000000000003");
    public static readonly Guid DevPartCategoryAirFilterId = Guid.Parse("c0000006-0000-0000-0000-000000000006");
    public static readonly Guid DevPartCategoryTireId = Guid.Parse("c0000002-0000-0000-0000-000000000002");
    public static readonly Guid DevPartCategorySparkPlugId = Guid.Parse("c0000005-0000-0000-0000-000000000005");

    public static readonly Guid DevServiceCategoryMaintenanceId = Guid.Parse("77777777-7777-7777-7777-777777777701");
    public static readonly Guid DevServiceCategoryRepairId = Guid.Parse("77777777-7777-7777-7777-777777777702");
    public static readonly Guid DevServiceCategoryElectricalId = Guid.Parse("77777777-7777-7777-7777-777777777703");

    public static readonly Guid DevServiceHoanKiemOilLaborId = Guid.Parse("88888888-8888-8888-8888-888888888801");
    public static readonly Guid DevServiceHoanKiemInspectionId = Guid.Parse("88888888-8888-8888-8888-888888888802");
    public static readonly Guid DevServiceHoanKiemBatteryLaborId = Guid.Parse("88888888-8888-8888-8888-888888888803");
    public static readonly Guid DevServiceHoanKiemAirFilterLaborId = Guid.Parse("88888888-8888-8888-8888-888888888804");
    public static readonly Guid DevServiceBaDinhBrakeLaborId = Guid.Parse("88888888-8888-8888-8888-888888888811");
    public static readonly Guid DevServiceBaDinhBrakeCheckId = Guid.Parse("88888888-8888-8888-8888-888888888812");
    public static readonly Guid DevServiceBaDinhTireLaborId = Guid.Parse("88888888-8888-8888-8888-888888888813");
    public static readonly Guid DevServiceBaDinhSparkLaborId = Guid.Parse("88888888-8888-8888-8888-888888888814");

    public static readonly Guid DevBundleHoanKiemComboId = Guid.Parse("99999999-9999-9999-9999-999999999901");
    public static readonly Guid DevBundleBaDinhComboId = Guid.Parse("99999999-9999-9999-9999-999999999902");

    public static readonly Guid DevBundleItemHoanKiemProductId = Guid.Parse("99999999-9999-9999-9999-999999999911");
    public static readonly Guid DevBundleItemHoanKiemServiceId = Guid.Parse("99999999-9999-9999-9999-999999999912");
    public static readonly Guid DevBundleItemBaDinhProductId = Guid.Parse("99999999-9999-9999-9999-999999999921");
    public static readonly Guid DevBundleItemBaDinhServiceId = Guid.Parse("99999999-9999-9999-9999-999999999922");

    public static readonly Guid DevMemberHoanKiemManagerId = Guid.Parse("44444444-4444-4444-4444-444444444401");
    public static readonly Guid DevMemberHoanKiemMechanic1Id = Guid.Parse("44444444-4444-4444-4444-444444444402");
    public static readonly Guid DevMemberHoanKiemMechanic2Id = Guid.Parse("44444444-4444-4444-4444-444444444403");
    public static readonly Guid DevMemberBaDinhManagerId = Guid.Parse("44444444-4444-4444-4444-444444444411");
    public static readonly Guid DevMemberBaDinhMechanic1Id = Guid.Parse("44444444-4444-4444-4444-444444444412");
    public static readonly Guid DevMemberBaDinhMechanic2Id = Guid.Parse("44444444-4444-4444-4444-444444444413");

    public static readonly Guid DevGarageStatusHistoryActivationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01");

    public static readonly Guid DevUserHoanKiemManager = Guid.Parse("33333333-3333-3333-3333-333333333301");
    public static readonly Guid DevUserHoanKiemMechanic1 = Guid.Parse("33333333-3333-3333-3333-333333333302");
    public static readonly Guid DevUserHoanKiemMechanic2 = Guid.Parse("33333333-3333-3333-3333-333333333303");
    public static readonly Guid DevUserBaDinhManager = Guid.Parse("33333333-3333-3333-3333-333333333311");
    public static readonly Guid DevUserBaDinhMechanic1 = Guid.Parse("33333333-3333-3333-3333-333333333312");
    public static readonly Guid DevUserBaDinhMechanic2 = Guid.Parse("33333333-3333-3333-3333-333333333313");

    public static async Task SeedAsync(GarageDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var garageExists = await db.Garages
            .IgnoreQueryFilters()
            .AnyAsync(g => g.Id == DevGarageId, cancellationToken);

        if (garageExists)
        {
            var membersPresent = await db.GarageMembers
                .IgnoreQueryFilters()
                .AnyAsync(m => m.Id == DevMemberHoanKiemManagerId, cancellationToken);
            var productsPresent = await db.GarageProducts
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Id == DevProductHoanKiemOilFilterId, cancellationToken);
            var categoriesPresent = await db.ServiceCategories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == DevServiceCategoryMaintenanceId, cancellationToken);
            var servicesPresent = await db.GarageServices
                .IgnoreQueryFilters()
                .AnyAsync(s => s.Id == DevServiceHoanKiemOilLaborId, cancellationToken);
            var bundlesPresent = await db.GarageBundles
                .IgnoreQueryFilters()
                .AnyAsync(b => b.Id == DevBundleHoanKiemComboId, cancellationToken);

            if (!(membersPresent && productsPresent && categoriesPresent && servicesPresent && bundlesPresent))
            {
                await SeedMissingDataAsync(
                    db,
                    logger,
                    seedCategories: !categoriesPresent,
                    seedServices: !servicesPresent,
                    seedMembers: !membersPresent,
                    seedProducts: !productsPresent,
                    seedBundles: !bundlesPresent,
                    cancellationToken);
            }

            await EnsureCatalogExpansionAndPartCategoryAlignmentAsync(db, logger, cancellationToken);
            await EnsureGarageStatusHistoryAsync(db, logger, cancellationToken);
            return;
        }

        var now = DateTime.UtcNow;
        var hours = CreateDefaultWorkingHours();

        var garage = new GarageEntity
        {
            Id = DevGarageId,
            OwnerId = DevOwnerUserId,
            BusinessName = "Garage Demo Verendar",
            ShortName = "Demo",
            Slug = "garage-demo-verendar",
            TaxCode = "0123456789",
            LogoUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/ENGINE-OIL.png",
            Status = GarageStatus.Active,
            CreatedAt = now,
            CreatedBy = DevOwnerUserId
        };

        var branchHoanKiem = new GarageBranch
        {
            Id = DevBranchHoanKiemId,
            GarageId = DevGarageId,
            Name = "Chi nhánh Hoàn Kiếm",
            Slug = "chi-nhanh-hoan-kiem-demo",
            Description = "Chi nhánh trung tâm — sửa chữa & bảo dưỡng ô tô.",
            CoverImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/BRAKE-PAD.png",
            Address = new Address
            {
                ProvinceCode = "01",
                WardCode = "00070",
                StreetDetail = "36 Phố Hàng Bạc"
            },
            Latitude = 21.0285,
            Longitude = 105.8542,
            WorkingHours = hours,
            PhoneNumber = "0901234567",
            TaxCode = "0312345678",
            Status = BranchStatus.Active,
            CreatedAt = now,
            CreatedBy = DevOwnerUserId
        };

        var branchBaDinh = new GarageBranch
        {
            Id = DevBranchBaDinhId,
            GarageId = DevGarageId,
            Name = "Chi nhánh Ba Đình",
            Slug = "chi-nhanh-ba-dinh-demo",
            Description = "Chi nhánh phụ — dịch vụ nhanh & đặt lịch online.",
            CoverImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/TIRE.png",
            Address = new Address
            {
                ProvinceCode = "01",
                WardCode = "00004",
                StreetDetail = "123 Đường Đội Cấn"
            },
            Latitude = 21.0345,
            Longitude = 105.8146,
            WorkingHours = hours,
            PhoneNumber = "0907654321",
            TaxCode = "0312345679",
            Status = BranchStatus.Active,
            CreatedAt = now,
            CreatedBy = DevOwnerUserId
        };

        var categories = BuildDevServiceCategories(now);
        var services = BuildDevServices(now);
        var members = BuildDevMembers(now);
        var products = BuildDevProducts(now);
        var bundles = BuildDevBundles(now);
        var statusHistory = BuildGarageStatusHistories(now);

        db.ServiceCategories.AddRange(categories);
        db.Garages.Add(garage);
        db.GarageBranches.AddRange(branchHoanKiem, branchBaDinh);
        db.GarageMembers.AddRange(members);
        db.GarageServices.AddRange(services);
        db.GarageProducts.AddRange(products);
        db.GarageBundles.AddRange(bundles);
        db.GarageStatusHistories.AddRange(statusHistory);
        await db.SaveChangesAsync(cancellationToken);

        logger?.LogInformation(
            "Seeded dev garage {GarageId}, branches {Branch1}/{Branch2}, {MemberCount} members, {ProductCount} products, {ServiceCount} services, {BundleCount} bundles, status history (owner {OwnerId}).",
            DevGarageId,
            DevBranchHoanKiemId,
            DevBranchBaDinhId,
            members.Length,
            products.Length,
            services.Length,
            bundles.Length,
            DevOwnerUserId);
    }

    private static async Task EnsureExpandedLaborServicesTrackedAsync(
        GarageDbContext db,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var hasElectrical = await db.ServiceCategories
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Id == DevServiceCategoryElectricalId, cancellationToken);
        if (!hasElectrical)
            db.ServiceCategories.Add(BuildElectricalServiceCategory(nowUtc));

        foreach (var svc in BuildExpandedServicesOnly(nowUtc))
        {
            if (await db.GarageServices.IgnoreQueryFilters().AnyAsync(s => s.Id == svc.Id, cancellationToken))
                continue;
            db.GarageServices.Add(svc);
        }
    }

    private static async Task EnsureCatalogExpansionAndPartCategoryAlignmentAsync(
        GarageDbContext db,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var added = 0;

        if (!await db.ServiceCategories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == DevServiceCategoryElectricalId, cancellationToken))
        {
            db.ServiceCategories.Add(BuildElectricalServiceCategory(now));
            added++;
        }

        foreach (var svc in BuildExpandedServicesOnly(now))
        {
            if (await db.GarageServices.IgnoreQueryFilters().AnyAsync(s => s.Id == svc.Id, cancellationToken))
                continue;
            db.GarageServices.Add(svc);
            added++;
        }

        foreach (var prd in BuildExpandedProductsOnly(now))
        {
            if (await db.GarageProducts.IgnoreQueryFilters().AnyAsync(p => p.Id == prd.Id, cancellationToken))
                continue;
            db.GarageProducts.Add(prd);
            added++;
        }

        await AlignLegacyProductPartCategoriesAsync(db, cancellationToken);

        if (added > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger?.LogInformation("Expanded garage dev catalog ({Added} new rows) for garage {GarageId}", added, DevGarageId);
        }
    }

    private static async Task AlignLegacyProductPartCategoriesAsync(GarageDbContext db, CancellationToken cancellationToken)
    {
        var oil = await db.GarageProducts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == DevProductHoanKiemOilFilterId, cancellationToken);
        if (oil != null && oil.PartCategoryId != DevPartCategoryOilFilterId)
            oil.PartCategoryId = DevPartCategoryOilFilterId;

        var brake = await db.GarageProducts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == DevProductBaDinhBrakePadId, cancellationToken);
        if (brake != null && brake.PartCategoryId != DevPartCategoryBrakePadId)
            brake.PartCategoryId = DevPartCategoryBrakePadId;

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureGarageStatusHistoryAsync(
        GarageDbContext db,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        if (await db.GarageStatusHistories
                .IgnoreQueryFilters()
                .AnyAsync(h => h.Id == DevGarageStatusHistoryActivationId, cancellationToken))
            return;

        if (!await db.Garages.IgnoreQueryFilters().AnyAsync(g => g.Id == DevGarageId, cancellationToken))
            return;

        var now = DateTime.UtcNow;
        db.GarageStatusHistories.Add(BuildGarageStatusHistories(now)[0]);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Seeded GarageStatusHistory (activation) for garage {GarageId}", DevGarageId);
    }

    private static GarageStatusHistory[] BuildGarageStatusHistories(DateTime nowUtc)
    {
        var changedAt = nowUtc.AddMonths(-11);
        return
        [
            new GarageStatusHistory
            {
                Id = DevGarageStatusHistoryActivationId,
                GarageId = DevGarageId,
                FromStatus = GarageStatus.Pending,
                ToStatus = GarageStatus.Active,
                ChangedByUserId = DevOwnerUserId,
                Reason = "Duyệt hồ sơ đăng ký garage (seed demo).",
                ChangedAt = changedAt,
                CreatedAt = changedAt,
                CreatedBy = DevOwnerUserId
            }
        ];
    }

    private static async Task SeedMissingDataAsync(
        GarageDbContext db,
        ILogger? logger,
        bool seedCategories,
        bool seedServices,
        bool seedMembers,
        bool seedProducts,
        bool seedBundles,
        CancellationToken cancellationToken)
    {
        var branchOk = await db.GarageBranches
                .IgnoreQueryFilters()
                .AnyAsync(b => b.Id == DevBranchHoanKiemId && b.GarageId == DevGarageId, cancellationToken)
            && await db.GarageBranches
                .IgnoreQueryFilters()
                .AnyAsync(b => b.Id == DevBranchBaDinhId && b.GarageId == DevGarageId, cancellationToken);

        if (!branchOk)
        {
            logger?.LogWarning(
                "Garage dev member backfill skipped: expected branches missing for garage {GarageId}",
                DevGarageId);
            return;
        }

        var membersAlready = await db.GarageMembers
            .IgnoreQueryFilters()
            .AnyAsync(m => m.Id == DevMemberHoanKiemManagerId, cancellationToken);
        var categoriesExist = await db.ServiceCategories
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Id == DevServiceCategoryMaintenanceId, cancellationToken);
        var servicesExist = await db.GarageServices
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Id == DevServiceHoanKiemOilLaborId, cancellationToken);
        var productsExist = await db.GarageProducts
            .IgnoreQueryFilters()
            .AnyAsync(p => p.Id == DevProductHoanKiemOilFilterId, cancellationToken);

        if (seedBundles && !productsExist)
            seedProducts = true;

        if (seedBundles && !servicesExist)
            seedServices = true;

        if (seedProducts && !servicesExist)
            seedServices = true;

        if (seedServices && !categoriesExist)
            seedCategories = true;

        var bundlesExist = await db.GarageBundles
            .IgnoreQueryFilters()
            .AnyAsync(b => b.Id == DevBundleHoanKiemComboId, cancellationToken);

        var now = DateTime.UtcNow;
        var categories = seedCategories && !categoriesExist ? BuildDevServiceCategories(now) : [];
        var services = seedServices && !servicesExist ? BuildDevServices(now) : [];
        var members = seedMembers && !membersAlready ? BuildDevMembers(now) : [];
        var products = seedProducts && !productsExist ? BuildDevProducts(now) : [];
        var bundles = seedBundles && !bundlesExist ? BuildDevBundles(now) : [];

        if (categories.Length > 0)
            db.ServiceCategories.AddRange(categories);

        if (services.Length > 0)
        {
            var hasElectrical = await db.ServiceCategories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == DevServiceCategoryElectricalId, cancellationToken);
            if (!hasElectrical)
                db.ServiceCategories.Add(BuildElectricalServiceCategory(now));

            db.GarageServices.AddRange(services);
        }

        if (members.Length > 0)
            db.GarageMembers.AddRange(members);

        if (products.Length > 0)
        {
            if (services.Length == 0)
                await EnsureExpandedLaborServicesTrackedAsync(db, now, cancellationToken);

            db.GarageProducts.AddRange(products);
        }

        if (bundles.Length > 0)
            db.GarageBundles.AddRange(bundles);

        await db.SaveChangesAsync(cancellationToken);

        logger?.LogInformation(
            "Backfilled dev data for garage {GarageId}: categories {Cat}, services {Svc}, members {Mem}, products {Prd}, bundles {Bnd}.",
            DevGarageId,
            categories.Length,
            services.Length,
            members.Length,
            products.Length,
            bundles.Length);
    }

    private static GarageMember[] BuildDevMembers(DateTime nowUtc) =>
    [
        CreateMember(DevMemberHoanKiemManagerId, DevBranchHoanKiemId, DevUserHoanKiemManager, MemberRole.Manager,
            "Nguyễn Văn Minh", "manager.hk@verendar.dev", "0911110001", nowUtc),
        CreateMember(DevMemberHoanKiemMechanic1Id, DevBranchHoanKiemId, DevUserHoanKiemMechanic1, MemberRole.Mechanic,
            "Trần Thị Lan", "mechanic1.hk@verendar.dev", "0911110002", nowUtc),
        CreateMember(DevMemberHoanKiemMechanic2Id, DevBranchHoanKiemId, DevUserHoanKiemMechanic2, MemberRole.Mechanic,
            "Lê Hoàng Nam", "mechanic2.hk@verendar.dev", "0911110003", nowUtc),
        CreateMember(DevMemberBaDinhManagerId, DevBranchBaDinhId, DevUserBaDinhManager, MemberRole.Manager,
            "Phạm Quốc Anh", "manager.bd@verendar.dev", "0922220001", nowUtc),
        CreateMember(DevMemberBaDinhMechanic1Id, DevBranchBaDinhId, DevUserBaDinhMechanic1, MemberRole.Mechanic,
            "Hoàng Thị Mai", "mechanic1.bd@verendar.dev", "0922220002", nowUtc),
        CreateMember(DevMemberBaDinhMechanic2Id, DevBranchBaDinhId, DevUserBaDinhMechanic2, MemberRole.Mechanic,
            "Đỗ Văn Hùng", "mechanic2.bd@verendar.dev", "0922220003", nowUtc),
    ];

    private static GarageProduct[] BuildDevProducts(DateTime nowUtc) =>
    [
        ..BuildCoreProducts(nowUtc),
        ..BuildExpandedProductsOnly(nowUtc),
    ];

    private static GarageProduct[] BuildCoreProducts(DateTime nowUtc) =>
    [
        new()
        {
            Id = DevProductHoanKiemOilFilterId,
            GarageBranchId = DevBranchHoanKiemId,
            PartCategoryId = DevPartCategoryOilFilterId,
            Name = "Lọc nhớt động cơ (OEM-style)",
            Description = "Lọc nhớt — PartCategoryId trùng Vehicle catalog (OIL-FILTER).",
            MaterialPrice = new Money { Amount = 180_000, Currency = "VND" },
            EstimatedDurationMinutes = 30,
            ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/OIL-FILTER.png",
            CompatibleVehicleTypes = "Sedan,SUV",
            ManufacturerKmInterval = 10_000,
            ManufacturerMonthInterval = 6,
            Status = ProductStatus.Active,
            InstallationServiceId = DevServiceHoanKiemOilLaborId,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevProductBaDinhBrakePadId,
            GarageBranchId = DevBranchBaDinhId,
            PartCategoryId = DevPartCategoryBrakePadId,
            Name = "Má phanh trước — Honda/Toyota phổ biến",
            Description = "Má phanh — PartCategoryId trùng Vehicle (BRAKE-PAD).",
            MaterialPrice = new Money { Amount = 650_000, Currency = "VND" },
            EstimatedDurationMinutes = 45,
            ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/BRAKE-PAD.png",
            CompatibleVehicleTypes = "Sedan,CUV",
            ManufacturerKmInterval = 40_000,
            ManufacturerMonthInterval = 24,
            Status = ProductStatus.Active,
            InstallationServiceId = DevServiceBaDinhBrakeLaborId,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
    ];

    private static GarageProduct[] BuildExpandedProductsOnly(DateTime nowUtc) =>
    [
        new()
        {
            Id = DevProductHoanKiemBatteryId,
            GarageBranchId = DevBranchHoanKiemId,
            PartCategoryId = DevPartCategoryBatteryId,
            Name = "Ắc quy MF 60Ah",
            Description = "Ắc quy khô — Vehicle PartCategory BATTERY.",
            MaterialPrice = new Money { Amount = 1_850_000, Currency = "VND" },
            EstimatedDurationMinutes = 40,
            ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/BATTERY.png",
            CompatibleVehicleTypes = "Sedan,SUV",
            ManufacturerKmInterval = null,
            ManufacturerMonthInterval = 24,
            Status = ProductStatus.Active,
            InstallationServiceId = DevServiceHoanKiemBatteryLaborId,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevProductHoanKiemAirFilterId,
            GarageBranchId = DevBranchHoanKiemId,
            PartCategoryId = DevPartCategoryAirFilterId,
            Name = "Lọc gió động cơ",
            Description = "Lọc gió — Vehicle PartCategory AIR-FILTER.",
            MaterialPrice = new Money { Amount = 220_000, Currency = "VND" },
            EstimatedDurationMinutes = 25,
            ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/AIR-FILTER.png",
            CompatibleVehicleTypes = "Sedan,SUV,CUV",
            ManufacturerKmInterval = 15_000,
            ManufacturerMonthInterval = 12,
            Status = ProductStatus.Active,
            InstallationServiceId = DevServiceHoanKiemAirFilterLaborId,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevProductBaDinhTireId,
            GarageBranchId = DevBranchBaDinhId,
            PartCategoryId = DevPartCategoryTireId,
            Name = "Lốp 205/55R16 (1 quả)",
            Description = "Lốp xe — Vehicle PartCategory TIRE.",
            MaterialPrice = new Money { Amount = 2_400_000, Currency = "VND" },
            EstimatedDurationMinutes = 50,
            ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/TIRE.png",
            CompatibleVehicleTypes = "Sedan",
            ManufacturerKmInterval = 50_000,
            ManufacturerMonthInterval = 48,
            Status = ProductStatus.Active,
            InstallationServiceId = DevServiceBaDinhTireLaborId,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevProductBaDinhSparkPlugId,
            GarageBranchId = DevBranchBaDinhId,
            PartCategoryId = DevPartCategorySparkPlugId,
            Name = "Bugi iridium (bộ 4)",
            Description = "Bugi — Vehicle PartCategory SPARK-PLUG.",
            MaterialPrice = new Money { Amount = 890_000, Currency = "VND" },
            EstimatedDurationMinutes = 35,
            ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/SPARK-PLUG.png",
            CompatibleVehicleTypes = "Sedan,SUV",
            ManufacturerKmInterval = 40_000,
            ManufacturerMonthInterval = 24,
            Status = ProductStatus.Active,
            InstallationServiceId = DevServiceBaDinhSparkLaborId,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
    ];

    private static ServiceCategory BuildElectricalServiceCategory(DateTime nowUtc) =>
        new()
        {
            Id = DevServiceCategoryElectricalId,
            Name = "Điện & ắc quy",
            Slug = "dev-dien-ac-quy",
            Description = "Ắc quy, điện thân xe (seed dev).",
            IconUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/BATTERY.png",
            DisplayOrder = 3,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        };

    private static ServiceCategory[] BuildDevServiceCategories(DateTime nowUtc) =>
    [
        new()
        {
            Id = DevServiceCategoryMaintenanceId,
            Name = "Bảo dưỡng định kỳ",
            Slug = "dev-bao-duong-dinh-ky",
            Description = "Danh mục seed dev",
            IconUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/ENGINE-OIL.png",
            DisplayOrder = 1,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevServiceCategoryRepairId,
            Name = "Sửa chữa",
            Slug = "dev-sua-chua",
            Description = "Danh mục seed dev",
            IconUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/BRAKE-PAD.png",
            DisplayOrder = 2,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        BuildElectricalServiceCategory(nowUtc),
    ];

    private static GarageService[] BuildDevServices(DateTime nowUtc) =>
    [
        ..BuildCoreServices(nowUtc),
        ..BuildExpandedServicesOnly(nowUtc),
    ];

    private static GarageService[] BuildCoreServices(DateTime nowUtc) =>
    [
        new()
        {
            Id = DevServiceHoanKiemOilLaborId,
            GarageBranchId = DevBranchHoanKiemId,
            ServiceCategoryId = DevServiceCategoryMaintenanceId,
            Name = "Công tháo lắp — thay lọc dầu",
            Description = "Nhân công thay lọc dầu động cơ.",
            ImageUrl = null,
            LaborPrice = new Money { Amount = 150_000, Currency = "VND" },
            EstimatedDurationMinutes = 30,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevServiceHoanKiemInspectionId,
            GarageBranchId = DevBranchHoanKiemId,
            ServiceCategoryId = DevServiceCategoryMaintenanceId,
            Name = "Kiểm tra tổng quát 21 hạng mục",
            Description = "Kiểm tra nhanh trước / sau bảo dưỡng.",
            ImageUrl = null,
            LaborPrice = new Money { Amount = 120_000, Currency = "VND" },
            EstimatedDurationMinutes = 45,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevServiceBaDinhBrakeLaborId,
            GarageBranchId = DevBranchBaDinhId,
            ServiceCategoryId = DevServiceCategoryRepairId,
            Name = "Công tháo lắp — thay má phanh trước",
            Description = "Nhân công thay má phanh.",
            ImageUrl = null,
            LaborPrice = new Money { Amount = 250_000, Currency = "VND" },
            EstimatedDurationMinutes = 60,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevServiceBaDinhBrakeCheckId,
            GarageBranchId = DevBranchBaDinhId,
            ServiceCategoryId = DevServiceCategoryMaintenanceId,
            Name = "Kiểm tra hệ thống phanh",
            Description = "Đo độ mòn, kiểm tra dầu phanh.",
            ImageUrl = null,
            LaborPrice = new Money { Amount = 80_000, Currency = "VND" },
            EstimatedDurationMinutes = 20,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
    ];

    private static GarageService[] BuildExpandedServicesOnly(DateTime nowUtc) =>
    [
        new()
        {
            Id = DevServiceHoanKiemBatteryLaborId,
            GarageBranchId = DevBranchHoanKiemId,
            ServiceCategoryId = DevServiceCategoryElectricalId,
            Name = "Tháo lắp & cân chỉnh ắc quy",
            Description = "Nhân công thay ắc quy, vệ sinh cực.",
            ImageUrl = null,
            LaborPrice = new Money { Amount = 200_000, Currency = "VND" },
            EstimatedDurationMinutes = 40,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevServiceHoanKiemAirFilterLaborId,
            GarageBranchId = DevBranchHoanKiemId,
            ServiceCategoryId = DevServiceCategoryMaintenanceId,
            Name = "Thay lọc gió động cơ",
            Description = "Tháo hộp lọc, thay element, vệ sinh.",
            ImageUrl = null,
            LaborPrice = new Money { Amount = 95_000, Currency = "VND" },
            EstimatedDurationMinutes = 25,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevServiceBaDinhTireLaborId,
            GarageBranchId = DevBranchBaDinhId,
            ServiceCategoryId = DevServiceCategoryRepairId,
            Name = "Tháo lắp & cân bằng lốp",
            Description = "Tháo lắp lốp, cân bằng động.",
            ImageUrl = null,
            LaborPrice = new Money { Amount = 180_000, Currency = "VND" },
            EstimatedDurationMinutes = 50,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
        new()
        {
            Id = DevServiceBaDinhSparkLaborId,
            GarageBranchId = DevBranchBaDinhId,
            ServiceCategoryId = DevServiceCategoryMaintenanceId,
            Name = "Thay bugi động cơ",
            Description = "Tháo nắp máy, thay bugi, chỉnh moment.",
            ImageUrl = null,
            LaborPrice = new Money { Amount = 320_000, Currency = "VND" },
            EstimatedDurationMinutes = 35,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        },
    ];

    private static GarageBundle[] BuildDevBundles(DateTime nowUtc) =>
    [
        new()
        {
            Id = DevBundleHoanKiemComboId,
            GarageBranchId = DevBranchHoanKiemId,
            Name = "Combo lọc dầu + kiểm tra",
            Description = "Phụ tùng lọc dầu (có lắp) + gói kiểm tra tổng quát.",
            ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/OIL-FILTER.png",
            DiscountAmount = new Money { Amount = 50_000, Currency = "VND" },
            DiscountPercent = null,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId,
            Items =
            [
                new GarageBundleItem
                {
                    Id = DevBundleItemHoanKiemProductId,
                    GarageBundleId = DevBundleHoanKiemComboId,
                    ProductId = DevProductHoanKiemOilFilterId,
                    ServiceId = null,
                    IncludeInstallation = true,
                    SortOrder = 1,
                    CreatedAt = nowUtc,
                    CreatedBy = DevOwnerUserId
                },
                new GarageBundleItem
                {
                    Id = DevBundleItemHoanKiemServiceId,
                    GarageBundleId = DevBundleHoanKiemComboId,
                    ProductId = null,
                    ServiceId = DevServiceHoanKiemInspectionId,
                    IncludeInstallation = false,
                    SortOrder = 2,
                    CreatedAt = nowUtc,
                    CreatedBy = DevOwnerUserId
                },
            ]
        },
        new()
        {
            Id = DevBundleBaDinhComboId,
            GarageBranchId = DevBranchBaDinhId,
            Name = "Combo má phanh + kiểm tra phanh",
            Description = "Phụ tùng má phanh (có lắp) + kiểm tra hệ thống phanh.",
            ImageUrl = "https://d3iova6424vljy.cloudfront.net/master/part-categories/BRAKE-PAD.png",
            DiscountAmount = null,
            DiscountPercent = 5m,
            Status = ProductStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId,
            Items =
            [
                new GarageBundleItem
                {
                    Id = DevBundleItemBaDinhProductId,
                    GarageBundleId = DevBundleBaDinhComboId,
                    ProductId = DevProductBaDinhBrakePadId,
                    ServiceId = null,
                    IncludeInstallation = true,
                    SortOrder = 1,
                    CreatedAt = nowUtc,
                    CreatedBy = DevOwnerUserId
                },
                new GarageBundleItem
                {
                    Id = DevBundleItemBaDinhServiceId,
                    GarageBundleId = DevBundleBaDinhComboId,
                    ProductId = null,
                    ServiceId = DevServiceBaDinhBrakeCheckId,
                    IncludeInstallation = false,
                    SortOrder = 2,
                    CreatedAt = nowUtc,
                    CreatedBy = DevOwnerUserId
                },
            ]
        },
    ];

    private static GarageMember CreateMember(
        Guid id,
        Guid garageBranchId,
        Guid userId,
        MemberRole role,
        string displayName,
        string email,
        string? phoneNumber,
        DateTime nowUtc) =>
        new()
        {
            Id = id,
            GarageBranchId = garageBranchId,
            UserId = userId,
            Role = role,
            DisplayName = displayName,
            Email = email,
            PhoneNumber = phoneNumber,
            AvatarUrl = null,
            Status = MemberStatus.Active,
            CreatedAt = nowUtc,
            CreatedBy = DevOwnerUserId
        };

    private static WorkingHours CreateDefaultWorkingHours()
    {
        var schedule = new Dictionary<DayOfWeek, DaySchedule>();
        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            if (day == DayOfWeek.Sunday)
            {
                schedule[day] = new DaySchedule { IsClosed = true, OpenTime = default, CloseTime = default };
            }
            else
            {
                schedule[day] = new DaySchedule
                {
                    IsClosed = false,
                    OpenTime = new TimeOnly(8, 0),
                    CloseTime = new TimeOnly(18, 0)
                };
            }
        }

        return new WorkingHours { Schedule = schedule };
    }
}
