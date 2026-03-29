namespace Verendar.Vehicle.Application.Mappings
{
    public static class PartQuestionnaireMappings
    {
        public static PartQuestionnaireResponse ToQuestionnaireResponse(
            this PartCategory category,
            IReadOnlyList<MaintenanceQuestion> questions)
        {
            return new PartQuestionnaireResponse
            {
                PartCategoryCode = category.Slug,
                PartCategoryName = category.Name,
                Questions = questions.Select(q => q.ToResponse()).ToList()
            };
        }

        public static PartQuestionItemDto ToResponse(this MaintenanceQuestion question)
        {
            return new PartQuestionItemDto
            {
                Id = question.Code,
                Group = question.Group.Code,
                GroupName = question.Group.Name,
                Question = question.QuestionText,
                AiQuestion = question.AiQuestion,
                Hint = question.Hint,
                Required = question.Required,
                Options = question.Options
                    .OrderBy(o => o.DisplayOrder)
                    .Select(o => o.ToResponse())
                    .ToList()
            };
        }

        public static PartQuestionOptionDto ToResponse(this MaintenanceQuestionOption option)
        {
            return new PartQuestionOptionDto
            {
                Id = option.OptionKey,
                Label = option.Label,
                Value = option.ValueForAi
            };
        }
    }
}
