namespace homework.Services;

public static class QualityStatusRule
{
    public const string Pass = "Pass";
    public const string Fail = "Fail";

    private const string MajorSeverity = "Major";

    public static string FromSeverity(string severityLevel) => severityLevel == MajorSeverity ? Fail : Pass;
}
