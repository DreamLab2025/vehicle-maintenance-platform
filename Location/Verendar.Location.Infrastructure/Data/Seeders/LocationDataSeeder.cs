namespace Verendar.Location.Infrastructure.Data.Seeders;

/// <summary>
/// Seed data factory for Location service entities
/// Data extracted from: https://github.com/thanglequoc/vietnamese-provinces-database
/// Original SQL source: postgres_ImportData_vn_units.sql
/// </summary>
public static class LocationDataSeeder
{
    public static List<AdministrativeRegion> GetAdministrativeRegions() => new()
    {
        new() { Id = 1, Name = "Đông Bắc Bộ" },
        new() { Id = 2, Name = "Tây Bắc Bộ" },
        new() { Id = 3, Name = "Đồng bằng sông Hồng" },
        new() { Id = 4, Name = "Bắc Trung Bộ" },
        new() { Id = 5, Name = "Duyên hải Nam Trung Bộ" },
        new() { Id = 6, Name = "Tây Nguyên" },
        new() { Id = 7, Name = "Đông Nam Bộ" },
        new() { Id = 8, Name = "Đồng bằng sông Cửu Long" },
    };

    public static List<AdministrativeUnit> GetAdministrativeUnits() => new()
    {
        new() { Id = 1, Name = "Thành phố trực thuộc trung ương", Abbreviation = "TP" },
        new() { Id = 2, Name = "Tỉnh", Abbreviation = "Tỉnh" },
        new() { Id = 3, Name = "Phường", Abbreviation = "P" },
        new() { Id = 4, Name = "Xã", Abbreviation = "X" },
        new() { Id = 5, Name = "Đặc khu tại hải đảo", Abbreviation = "Đặc khu" },
    };

    public static List<Province> GetProvinces() => new()
    {
        new() { Code = "01", Name = "Hà Nội", AdministrativeRegionId = 3 },
        new() { Code = "04", Name = "Cao Bằng", AdministrativeRegionId = 1 },
        new() { Code = "08", Name = "Tuyên Quang", AdministrativeRegionId = 1 },
        new() { Code = "11", Name = "Điện Biên", AdministrativeRegionId = 2 },
        new() { Code = "12", Name = "Lai Châu", AdministrativeRegionId = 2 },
        new() { Code = "14", Name = "Sơn La", AdministrativeRegionId = 2 },
        new() { Code = "15", Name = "Lào Cai", AdministrativeRegionId = 2 },
        new() { Code = "19", Name = "Thái Nguyên", AdministrativeRegionId = 1 },
        new() { Code = "20", Name = "Lạng Sơn", AdministrativeRegionId = 1 },
        new() { Code = "22", Name = "Quảng Ninh", AdministrativeRegionId = 1 },
        new() { Code = "24", Name = "Bắc Ninh", AdministrativeRegionId = 3 },
        new() { Code = "25", Name = "Phú Thọ", AdministrativeRegionId = 3 },
        new() { Code = "31", Name = "Hải Phòng", AdministrativeRegionId = 3 },
        new() { Code = "33", Name = "Hưng Yên", AdministrativeRegionId = 3 },
        new() { Code = "37", Name = "Ninh Bình", AdministrativeRegionId = 3 },
        new() { Code = "38", Name = "Thanh Hóa", AdministrativeRegionId = 4 },
        new() { Code = "40", Name = "Nghệ An", AdministrativeRegionId = 4 },
        new() { Code = "42", Name = "Hà Tĩnh", AdministrativeRegionId = 4 },
        new() { Code = "44", Name = "Quảng Trị", AdministrativeRegionId = 4 },
        new() { Code = "46", Name = "Huế", AdministrativeRegionId = 4 },
        new() { Code = "48", Name = "Đà Nẵng", AdministrativeRegionId = 5 },
        new() { Code = "51", Name = "Quảng Ngãi", AdministrativeRegionId = 5 },
        new() { Code = "52", Name = "Gia Lai", AdministrativeRegionId = 6 },
        new() { Code = "56", Name = "Khánh Hòa", AdministrativeRegionId = 5 },
        new() { Code = "66", Name = "Đắk Lắk", AdministrativeRegionId = 6 },
        new() { Code = "68", Name = "Lâm Đồng", AdministrativeRegionId = 6 },
        new() { Code = "75", Name = "Đồng Nai", AdministrativeRegionId = 7 },
        new() { Code = "79", Name = "Hồ Chí Minh", AdministrativeRegionId = 7 },
        new() { Code = "80", Name = "Tây Ninh", AdministrativeRegionId = 7 },
        new() { Code = "82", Name = "Đồng Tháp", AdministrativeRegionId = 8 },
        new() { Code = "86", Name = "Vĩnh Long", AdministrativeRegionId = 8 },
        new() { Code = "91", Name = "An Giang", AdministrativeRegionId = 8 },
        new() { Code = "92", Name = "Cần Thơ", AdministrativeRegionId = 8 },
        new() { Code = "96", Name = "Cà Mau", AdministrativeRegionId = 8 },
    };

    public static List<Ward> GetWards() => new()
    {
        // Sample wards from Hà Nội (Province Code "01")
        // In production, all 3,321+ wards would be loaded from the Vietnamese Provinces Database
        new() { Code = "00004", Name = "Ba Đình", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00008", Name = "Ngọc Hà", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00025", Name = "Giảng Võ", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00070", Name = "Hoàn Kiếm", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00082", Name = "Cửa Nam", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00091", Name = "Phú Thượng", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00097", Name = "Hồng Hà", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00103", Name = "Tây Hồ", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00118", Name = "Bồ Đề", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00127", Name = "Việt Hưng", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00136", Name = "Phúc Lợi", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00145", Name = "Long Biên", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00160", Name = "Nghĩa Đô", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00166", Name = "Cầu Giấy", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00175", Name = "Yên Hòa", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00190", Name = "Ô Chợ Dừa", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00199", Name = "Láng", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00226", Name = "Văn Miếu - Quốc Tử Giám", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00229", Name = "Kim Liên", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00235", Name = "Đống Đa", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00256", Name = "Hai Bà Trưng", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00283", Name = "Vĩnh Tuy", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00292", Name = "Bạch Mai", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00301", Name = "Vĩnh Hưng", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00316", Name = "Định Công", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00322", Name = "Tương Mai", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00328", Name = "Lĩnh Nam", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00331", Name = "Hoàng Mai", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00337", Name = "Hoàng Liệt", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00340", Name = "Yên Sở", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00352", Name = "Phương Liệt", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00364", Name = "Khương Đình", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00367", Name = "Thanh Xuân", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00592", Name = "Từ Liêm", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00598", Name = "Thượng Cát", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00602", Name = "Đông Ngạc", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00611", Name = "Xuân Đỉnh", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00613", Name = "Tây Tựu", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00619", Name = "Phú Diễn", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00622", Name = "Xuân Phương", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00634", Name = "Tây Mỗ", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00637", Name = "Đại Mỗ", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00643", Name = "Thanh Liệt", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "09552", Name = "Kiến Hưng", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "09556", Name = "Hà Đông", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "09562", Name = "Yên Nghĩa", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "09568", Name = "Phú Lương", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "09574", Name = "Sơn Tây", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "09604", Name = "Tùng Thiện", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "09886", Name = "Dương Nội", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "10015", Name = "Chương Mỹ", ProvinceCode = "01", AdministrativeUnitId = 3 },
        new() { Code = "00376", Name = "Sóc Sơn", ProvinceCode = "01", AdministrativeUnitId = 4 },
        new() { Code = "00382", Name = "Kim Anh", ProvinceCode = "01", AdministrativeUnitId = 4 },
    };
}
