namespace Verendar.Vehicle.Application.Dtos
{
    public class PartQuestionnaireResponse
    {
        public string PartCategoryCode { get; set; } = string.Empty;
        public string PartCategoryName { get; set; } = string.Empty;
        public List<PartQuestionItemDto> Questions { get; set; } = [];
    }

    public class PartQuestionItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string AiQuestion { get; set; } = string.Empty;
        public string? Hint { get; set; }
        public List<PartQuestionOptionDto> Options { get; set; } = [];
        public bool Required { get; set; } = true;
    }

    public class PartQuestionOptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
