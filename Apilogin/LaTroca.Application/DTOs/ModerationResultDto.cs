namespace LaTroca.Application.DTOs
{
    public class ModerationResultDto
    {
        public bool IsSafe { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = "safe";
    }
}
