namespace Verendar.Vehicle.Infrastructure.Seeders;

public static partial class VehicleProductionDataSeed
{
    private static async Task SeedQuestionnaireAsync(VehicleDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        if (await db.MaintenanceQuestionGroups.AnyAsync(cancellationToken))
        {
            logger?.LogInformation("Maintenance questionnaire seed skipped (already present)");
            return;
        }

        var slugToId = await db.PartCategories
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Slug, p => p.Id, cancellationToken);

        var groups = QuestionnaireCatalog.Groups
            .Select(g => new MaintenanceQuestionGroup
            {
                Code = g.Code,
                Name = g.Name,
                DisplayOrder = g.DisplayOrder
            })
            .ToList();

        await db.MaintenanceQuestionGroups.AddRangeAsync(groups, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var groupIdByCode = groups.ToDictionary(x => x.Code, x => x.Id);

        var questions = new List<MaintenanceQuestion>();

        foreach (var def in QuestionnaireCatalog.Questions)
        {
            if (!groupIdByCode.TryGetValue(def.GroupCode, out var groupId))
                throw new InvalidOperationException($"Unknown group code: {def.GroupCode}");

            var q = new MaintenanceQuestion
            {
                GroupId = groupId,
                Code = def.Code,
                QuestionText = def.QuestionText,
                AiQuestion = def.AiQuestion,
                Hint = def.Hint,
                DisplayOrder = def.DisplayOrder,
                IsAskOncePerSession = def.IsAskOncePerSession,
                AppliesToAllPartCategories = def.AppliesToAllPartCategories,
                Required = true
            };

            var optOrder = 0;
            foreach (var o in def.Options)
            {
                optOrder++;
                q.Options.Add(new MaintenanceQuestionOption
                {
                    OptionKey = o.Key,
                    Label = o.Label,
                    ValueForAi = o.ValueForAi,
                    DisplayOrder = optOrder
                });
            }

            if (!def.AppliesToAllPartCategories && def.PartSlugs is { Length: > 0 })
            {
                foreach (var slug in def.PartSlugs)
                {
                    if (!slugToId.TryGetValue(slug, out var partId))
                        throw new InvalidOperationException($"Unknown part category slug in questionnaire seed: {slug}");

                    q.PartCategoryLinks.Add(new MaintenanceQuestionPartCategory
                    {
                        PartCategoryId = partId
                    });
                }
            }

            questions.Add(q);
        }

        await db.MaintenanceQuestions.AddRangeAsync(questions, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger?.LogInformation(
            "Seeded maintenance questionnaire: {GroupCount} groups, {QuestionCount} questions",
            groups.Count,
            questions.Count);
    }

    private static class QuestionnaireCatalog
    {

        public sealed record OptionDef(string Key, string Label, string ValueForAi);

        public sealed record QuestionDef(
    string Code,
    string GroupCode,
    string QuestionText,
    string AiQuestion,
    string? Hint,
    int DisplayOrder,
    bool IsAskOncePerSession,
    bool AppliesToAllPartCategories,
    string[]? PartSlugs,
    OptionDef[] Options);

        public static QuestionDef[] Questions =>
[
    new("SA-1", "General",
            "Mỗi ngày bạn thường chạy khoảng bao nhiêu km?",
            "What is your typical daily riding distance?",
            null, 1, true, true, null,
            [
                new("opt1", "Dưới 5 km", "Typical daily riding distance is less than 5 km"),
                new("opt2", "5 – 15 km", "Typical daily riding distance is 5 to 15 km"),
                new("opt3", "15 – 30 km", "Typical daily riding distance is 15 to 30 km"),
                new("opt4", "Trên 30 km", "Typical daily riding distance is over 30 km")
            ]),
        new("SA-2", "General",
            "Bạn thường chạy ở loại đường nào?",
            "What type of roads do you usually ride on?",
            null, 2, true, false, ["TIRE", "CHAIN-SPROCKET", "AIR-FILTER"],
            [
                new("opt1", "Đường nhựa tốt, ít bụi", "Usually rides on smooth paved roads with little dust"),
                new("opt2", "Đường đô thị đông xe, nhiều bụi", "Usually rides in busy urban traffic with heavy dust"),
                new("opt3", "Đường xấu, nhiều ổ gà hoặc bụi đất", "Usually rides on rough roads with potholes or heavy dirt and dust")
            ]),
        new("SA-3", "General",
            "Bạn có thường chở thêm người hoặc đồ nặng không?",
            "Do you often carry a passenger or heavy cargo?",
            null, 3, true, false, ["TIRE", "BRAKE-PAD"],
            [
                new("opt1", "Thường xuyên", "Often carries a passenger or heavy cargo"),
                new("opt2", "Thỉnh thoảng", "Sometimes carries a passenger or heavy cargo"),
                new("opt3", "Gần như không", "Rarely or never carries a passenger or heavy cargo")
            ]),

        new("BE-1", "Engine",
            "Bạn nhớ lần cuối đi thay dầu nhớt là bao lâu rồi không?",
            "How long ago was the last engine oil change?",
            "Không cần nhớ chính xác — ước lượng là đủ", 1, false, false, ["ENGINE-OIL", "OIL-FILTER"],
            [
                new("opt1", "Dưới 1 tháng", "Last engine oil change was less than 1 month ago"),
                new("opt2", "1 – 3 tháng", "Last engine oil change was 1 to 3 months ago"),
                new("opt3", "3 – 6 tháng", "Last engine oil change was 3 to 6 months ago"),
                new("opt4", "Trên 6 tháng", "Last engine oil change was over 6 months ago"),
                new("opt5", "Chưa bao giờ thay / không nhớ", "Has never changed engine oil or cannot remember when")
            ]),
        new("BE-2", "Engine",
            "Khi thay dầu, thợ có thay luôn lọc nhớt không?",
            "When changing engine oil, did the mechanic also replace the oil filter?",
            "Lọc nhớt thường được thay cùng với dầu — bạn có thể hỏi lại thợ", 2, false, false, ["OIL-FILTER"],
            [
                new("opt1", "Có, luôn thay cùng", "Oil filter is always replaced together with engine oil"),
                new("opt2", "Thỉnh thoảng có", "Oil filter is sometimes replaced with engine oil"),
                new("opt3", "Không / không biết", "Oil filter has not been replaced or not known")
            ]),
        new("BE-3", "Engine",
            "Buổi sáng khi đề máy, xe có nổ ngay không?",
            "Does the engine start immediately in the morning?",
            null, 3, false, false, ["SPARK-PLUG", "BATTERY"],
            [
                new("opt1", "Nổ ngay, không vấn đề", "Engine starts immediately with no issues in the morning"),
                new("opt2", "Phải đề 2 – 3 lần", "Engine requires 2 to 3 attempts to start in the morning"),
                new("opt3", "Hay bị khó nổ", "Engine often has difficulty starting in the morning")
            ]),
        new("BE-4", "Engine",
            "Khi tăng ga, xe có cảm giác mượt và bốc không?",
            "Does the engine feel responsive and smooth when applying throttle?",
            null, 4, false, false, ["SPARK-PLUG", "AIR-FILTER"],
            [
                new("opt1", "Bốc, mượt bình thường", "Engine feels responsive and smooth when throttle is applied"),
                new("opt2", "Hơi chậm, ì ì", "Engine feels sluggish when throttle is applied"),
                new("opt3", "Giật, hụt ga", "Engine misfires or hesitates when throttle is applied")
            ]),
        new("BE-5", "Engine",
            "Bạn có để ý thấy xe hao xăng hơn trước không?",
            "Have you noticed increased fuel consumption compared to before?",
            null, 5, false, false, ["ENGINE-OIL", "OIL-FILTER", "SPARK-PLUG", "AIR-FILTER"],
            [
                new("opt1", "Có, rõ rệt", "Fuel consumption has noticeably increased compared to before"),
                new("opt2", "Có, hơi tăng", "Fuel consumption has slightly increased compared to before"),
                new("opt3", "Không thay đổi", "Fuel consumption has not changed compared to before"),
                new("opt4", "Không để ý", "Has not noticed any change in fuel consumption")
            ]),
        new("BE-6", "Engine",
            "Bạn có nhớ đã thay bugi chưa?",
            "Have you ever replaced the spark plugs?",
            null, 6, false, false, ["SPARK-PLUG"],
            [
                new("opt1", "Chưa bao giờ", "Has never replaced spark plugs"),
                new("opt2", "Có (không nhớ khi nào)", "Has replaced spark plugs but cannot remember when"),
                new("opt3", "Có, dưới 5.000 km trước", "Replaced spark plugs less than 5000 km ago"),
                new("opt4", "Có, 5.000 – 10.000 km trước", "Replaced spark plugs between 5000 to 10000 km ago"),
                new("opt5", "Có, hơn 10.000 km trước", "Replaced spark plugs over 10000 km ago")
            ]),
        new("BE-7", "Engine",
            "Bạn có nhớ đã vệ sinh hoặc thay lọc gió chưa?",
            "Have you ever cleaned or replaced the air filter?",
            null, 7, false, false, ["AIR-FILTER"],
            [
                new("opt1", "Chưa bao giờ", "Has never cleaned or replaced the air filter"),
                new("opt2", "Có (không nhớ khi nào)", "Has cleaned or replaced the air filter but cannot remember when"),
                new("opt3", "Có, dưới 6 tháng trước", "Cleaned or replaced air filter less than 6 months ago"),
                new("opt4", "Có, 6 tháng – 1 năm trước", "Cleaned or replaced air filter between 6 months to 1 year ago"),
                new("opt5", "Có, hơn 1 năm trước", "Cleaned or replaced air filter over 1 year ago")
            ]),
        new("BE-8", "Engine",
            "Máy xe có phát ra tiếng khua hoặc tiếng gõ kim loại bất thường không?",
            "Does the engine make unusual knocking or ticking metallic sounds?",
            "Tiếng kêu từ máy thường xuất hiện khi dầu nhớt thiếu hoặc đã quá bẩn", 8, false, false, ["ENGINE-OIL"],
            [
                new("opt1", "Có, nghe rõ tiếng kêu", "Engine makes clearly audible knocking or ticking sounds"),
                new("opt2", "Thỉnh thoảng khi máy lạnh", "Engine makes occasional knocking sounds when cold"),
                new("opt3", "Không, máy chạy êm", "Engine runs quietly with no unusual sounds")
            ]),
        new("BF-1", "Engine",
            "Bạn có để ý dầu nhớt bị đen nhanh bất thường không?",
            "Have you noticed the engine oil turning black or dirty unusually fast?",
            "Lọc nhớt bẩn hoặc hỏng sẽ khiến dầu đen rất nhanh sau khi thay", 9, false, false, ["OIL-FILTER"],
            [
                new("opt1", "Có, đen rất nhanh", "Engine oil turns black very quickly after changing"),
                new("opt2", "Có, nhanh hơn trước một chút", "Engine oil turns black slightly faster than before"),
                new("opt3", "Bình thường", "Engine oil turns black at normal rate"),
                new("opt4", "Chưa bao giờ kiểm tra", "Has never checked the color of engine oil")
            ]),

        new("CP-1", "Brakes",
            "Khi bóp phanh, bạn cảm thấy thế nào?",
            "How does the brake feel when you press it?",
            null, 1, false, false, ["BRAKE-PAD", "BRAKE-FLUID"],
            [
                new("opt1", "Phanh ăn ngay, chắc tay", "Brake responds immediately and feels firm when pressed"),
                new("opt2", "Phải bóp mạnh hơn trước mới ăn", "Need to press brake harder than before for it to respond"),
                new("opt3", "Có tiếng kêu (ken két / rè rè)", "Brake makes squeaking or grinding sound when pressed"),
                new("opt4", "Bị rung khi phanh", "Brake vibrates when pressed")
            ]),
        new("CP-2", "Brakes",
            "Bạn có nhớ đã thay má phanh chưa?",
            "Have you ever replaced the brake pads?",
            null, 2, false, false, ["BRAKE-PAD"],
            [
                new("opt1", "Chưa bao giờ", "Has never replaced brake pads"),
                new("opt2", "Có (không nhớ khi nào)", "Has replaced brake pads but cannot remember when"),
                new("opt3", "Có, dưới 5.000 km trước", "Replaced brake pads less than 5000 km ago"),
                new("opt4", "Có, 5.000 – 10.000 km trước", "Replaced brake pads between 5000 to 10000 km ago"),
                new("opt5", "Có, hơn 10.000 km trước", "Replaced brake pads over 10000 km ago")
            ]),
        new("CP-3", "Brakes",
            "Bạn có nhớ đã thay dầu phanh chưa?",
            "Have you ever replaced or topped up the brake fluid?",
            "Dầu phanh thường thay theo năm — không liên quan đến km", 3, false, false, ["BRAKE-FLUID"],
            [
                new("opt1", "Chưa bao giờ", "Has never replaced or topped up brake fluid"),
                new("opt2", "Có (không nhớ khi nào)", "Has replaced or topped up brake fluid but cannot remember when"),
                new("opt3", "Có, dưới 1 năm trước", "Replaced or topped up brake fluid less than 1 year ago"),
                new("opt4", "Có, 1 – 2 năm trước", "Replaced or topped up brake fluid between 1 to 2 years ago"),
                new("opt5", "Có, hơn 2 năm trước", "Replaced or topped up brake fluid over 2 years ago")
            ]),
        new("CP-4", "Brakes",
            "Bạn có hay phải phanh nhiều không (đường đông, hay chạy xuống dốc)?",
            "Do you brake frequently due to heavy traffic or riding downhill?",
            null, 4, false, false, ["BRAKE-PAD", "BRAKE-FLUID"],
            [
                new("opt1", "Thường xuyên, phanh liên tục", "Brakes very frequently due to heavy traffic or downhill riding"),
                new("opt2", "Thỉnh thoảng", "Brakes moderately in normal riding conditions"),
                new("opt3", "Ít khi, đường thoáng", "Brakes rarely due to open roads and light traffic")
            ]),
        new("CF-2", "Brakes",
            "Khi phanh liên tục hoặc xuống dốc dài, bạn có thấy phanh yếu dần không?",
            "When braking continuously such as going downhill, do you notice the brakes becoming progressively weaker?",
            "Dầu phanh bị sôi hoặc lẫn nước sẽ gây mất lực phanh khi dùng liên tục", 5, false, false, ["BRAKE-FLUID"],
            [
                new("opt1", "Có, rõ rệt", "Brakes noticeably become weaker during continuous or downhill braking"),
                new("opt2", "Thỉnh thoảng thấy yếu hơn", "Brakes occasionally feel weaker during continuous or downhill braking"),
                new("opt3", "Không, phanh vẫn ổn", "Brakes remain effective during continuous or downhill braking"),
                new("opt4", "Chưa có điều kiện để nhận biết", "Has not had the opportunity to notice brake fade during continuous braking")
            ]),

        new("DT-1", "DrivetrainAndTires",
            "Khi chạy trời mưa hoặc đường ướt, xe có hay bị trơn không?",
            "Do you notice the vehicle slipping on wet roads or in rain?",
            null, 1, false, false, ["TIRE"],
            [
                new("opt1", "Có, trơn nhiều", "Vehicle slips noticeably on wet roads or in rain"),
                new("opt2", "Thỉnh thoảng", "Vehicle occasionally slips on wet roads or in rain"),
                new("opt3", "Không, bình thường", "Vehicle does not slip on wet roads or in rain"),
                new("opt4", "Chưa để ý", "Has not noticed if vehicle slips on wet roads or in rain")
            ]),
        new("DT-2", "DrivetrainAndTires",
            "Khi chạy nhanh, bạn có thấy xe bị rung không?",
            "Do you notice vibration when riding at higher speeds?",
            null, 2, false, false, ["CHAIN-SPROCKET"],
            [
                new("opt1", "Có, rung rõ", "Noticeable vibration when riding at higher speeds"),
                new("opt2", "Thỉnh thoảng", "Occasional vibration when riding at higher speeds"),
                new("opt3", "Không", "No vibration when riding at higher speeds")
            ]),
        new("DT-3", "DrivetrainAndTires",
            "Khi đang chạy, bạn có nghe tiếng kêu hoặc cảm giác giật từ phần bánh sau / xích không?",
            "Do you hear rattling sounds or feel jerking from the chain or rear wheel while riding?",
            null, 3, false, false, ["CHAIN-SPROCKET"],
            [
                new("opt1", "Có, nghe tiếng kêu lạ", "Hears unusual rattling or chain noise while riding"),
                new("opt2", "Có, xe bị giật khi lên/nhả ga", "Feels jerking when accelerating or releasing throttle"),
                new("opt3", "Không, bình thường", "No unusual sounds or jerking from chain or rear wheel area")
            ]),
        new("DT-4", "DrivetrainAndTires",
            "Bạn có nhớ đã thay lốp chưa?",
            "Have you ever replaced the tires?",
            null, 4, false, false, ["TIRE"],
            [
                new("opt1", "Chưa bao giờ", "Has never replaced tires"),
                new("opt2", "Có (không nhớ khi nào)", "Has replaced tires but cannot remember when"),
                new("opt3", "Có, dưới 1 năm trước", "Replaced tires less than 1 year ago"),
                new("opt4", "Có, 1 – 2 năm trước", "Replaced tires between 1 to 2 years ago"),
                new("opt5", "Có, hơn 2 năm trước", "Replaced tires over 2 years ago")
            ]),
        new("DT-5", "DrivetrainAndTires",
            "Bạn có nhớ đã tăng xích hoặc thay nhông xên dĩa chưa?",
            "Have you ever adjusted the chain tension or replaced the chain and sprocket set?",
            null, 5, false, false, ["CHAIN-SPROCKET"],
            [
                new("opt1", "Chưa bao giờ", "Has never adjusted chain or replaced chain and sprocket"),
                new("opt2", "Có (không nhớ khi nào)", "Has adjusted chain or replaced chain and sprocket but cannot remember when"),
                new("opt3", "Có, dưới 5.000 km trước", "Adjusted chain or replaced chain and sprocket less than 5000 km ago"),
                new("opt4", "Có, 5.000 – 15.000 km trước", "Adjusted chain or replaced chain and sprocket between 5000 to 15000 km ago"),
                new("opt5", "Có, hơn 15.000 km trước", "Adjusted chain or replaced chain and sprocket over 15000 km ago")
            ]),

        new("EB-1", "ElectricalAndCooling",
            "Khi để xe không chạy 2 – 3 ngày rồi đề máy, xe có khó nổ không?",
            "Is it hard to start the engine after leaving the vehicle unused for 2 to 3 days?",
            null, 1, false, false, ["BATTERY"],
            [
                new("opt1", "Vẫn nổ bình thường", "Engine starts normally after 2 to 3 days without use"),
                new("opt2", "Đề yếu hơn, phải cố", "Engine starts weaker and requires more effort after 2 to 3 days without use"),
                new("opt3", "Khó nổ hoặc không nổ được", "Engine is hard or impossible to start after 2 to 3 days without use"),
                new("opt4", "Tôi chạy xe mỗi ngày, không rõ", "Rides vehicle daily so cannot tell")
            ]),
        new("EB-2", "ElectricalAndCooling",
            "Đèn, còi và xi-nhan có hoạt động bình thường không?",
            "Do the lights, horn, and turn signals work normally?",
            null, 2, false, false, ["BATTERY"],
            [
                new("opt1", "Bình thường", "Lights, horn, and turn signals all work normally"),
                new("opt2", "Yếu hơn trước", "Lights, horn, or turn signals are weaker than before"),
                new("opt3", "Lúc được lúc không", "Lights, horn, or turn signals work intermittently")
            ]),
        new("EB-3", "ElectricalAndCooling",
            "Bạn có nhớ đã thay ắc quy chưa?",
            "Have you ever replaced the battery?",
            null, 3, false, false, ["BATTERY"],
            [
                new("opt1", "Chưa bao giờ", "Has never replaced the battery"),
                new("opt2", "Có (không nhớ khi nào)", "Has replaced the battery but cannot remember when"),
                new("opt3", "Có, dưới 1 năm trước", "Replaced the battery less than 1 year ago"),
                new("opt4", "Có, 1 – 2 năm trước", "Replaced the battery between 1 to 2 years ago"),
                new("opt5", "Có, hơn 2 năm trước", "Replaced the battery over 2 years ago")
            ]),
        new("EC-1", "ElectricalAndCooling",
            "Khi chạy đường dài hoặc trời nóng, xe có hay bị nóng máy không?",
            "Does the engine overheat when riding long distances or in hot weather?",
            null, 4, false, false, ["COOLANT", "ENGINE-OIL"],
            [
                new("opt1", "Có, nóng rõ rệt", "Engine overheats noticeably when riding long distances or in hot weather"),
                new("opt2", "Thỉnh thoảng hơi nóng", "Engine occasionally feels slightly hot when riding long distances or hot weather"),
                new("opt3", "Không", "Engine temperature stays normal when riding long distances or in hot weather"),
                new("opt4", "Không để ý", "Has not noticed any engine temperature issues")
            ]),
        new("EC-2", "ElectricalAndCooling",
            "Bạn có nhớ đã kiểm tra hoặc thay nước làm mát chưa?",
            "Have you ever checked or replaced the coolant?",
            "Một số xe không có nước làm mát — nếu không biết thì chọn \"không rõ\"", 5, false, false, ["COOLANT"],
            [
                new("opt1", "Chưa bao giờ kiểm tra", "Has never checked or replaced coolant"),
                new("opt2", "Có kiểm tra nhưng chưa thay", "Has checked but not replaced coolant"),
                new("opt3", "Có, thay dưới 1 năm trước", "Replaced coolant less than 1 year ago"),
                new("opt4", "Có, thay hơn 1 năm trước", "Replaced coolant over 1 year ago"),
                new("opt5", "Không rõ xe có nước làm mát không", "Does not know if the vehicle has a coolant system")
            ]),
        new("EC-3", "ElectricalAndCooling",
            "Bạn có hay chạy trong điều kiện kẹt xe lâu hoặc trời nắng nóng kéo dài không?",
            "Do you often ride in stop-and-go traffic or hot weather conditions for extended periods?",
            "Kẹt xe lâu làm máy không được làm mát bởi gió — nước làm mát chịu áp lực cao hơn bình thường", 6, false, false, ["COOLANT"],
            [
                new("opt1", "Thường xuyên", "Often rides in prolonged stop-and-go traffic or hot weather conditions"),
                new("opt2", "Thỉnh thoảng", "Sometimes rides in stop-and-go traffic or hot weather conditions"),
                new("opt3", "Hiếm khi", "Rarely rides in stop-and-go traffic or hot weather conditions")
            ]),
        new("EC-4", "ElectricalAndCooling",
            "Bạn có bao giờ nhìn thấy bình nước làm mát bị cạn không?",
            "Have you ever noticed the coolant reservoir level dropping or running low?",
            "Bình nước làm mát thường trong suốt — bạn có thể nhìn thấy mức nước từ bên ngoài", 7, false, false, ["COOLANT"],
            [
                new("opt1", "Có, thấy cạn nhiều lần", "Coolant reservoir has been noticed to be low or empty multiple times"),
                new("opt2", "Có, thấy cạn một lần", "Coolant reservoir was noticed to be low or empty once"),
                new("opt3", "Chưa bao giờ thấy cạn", "Coolant reservoir level has never been noticed to be low"),
                new("opt4", "Chưa bao giờ kiểm tra", "Has never checked the coolant reservoir level")
            ])
];

        public static (string Code, string Name, int DisplayOrder)[] Groups =>
        [
            ("General", "Thói quen chạy xe (chung)", 1),
            ("Engine", "Động cơ", 2),
            ("Brakes", "Hệ thống phanh", 3),
            ("DrivetrainAndTires", "Truyền động & lốp", 4),
            ("ElectricalAndCooling", "Điện & làm mát", 5)
        ];

    }
}
