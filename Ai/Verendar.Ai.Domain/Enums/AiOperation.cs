using System.ComponentModel;

namespace Verendar.Ai.Domain.Enums;

public enum AiOperation
{
    [Description("Generate text")]
    GenerateText = 1,
    [Description("Generate image")]
    GenerateImage = 2,
    [Description("Generate audio")]
    GenerateAudio = 3,
    [Description("Generate video")]
    GenerateVideo = 4,
    [Description("Generate code")]
    GenerateCode = 5,
}
