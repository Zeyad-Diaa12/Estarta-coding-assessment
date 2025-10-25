namespace EmployeeStatus.Configuration;
public static class BusinessRulesConfiguration
{
    public static class Tax
    {
        public const decimal Threshold = 10000m;
        public const decimal Rate = 0.07m;
    }

    public static class Status
    {
        public const decimal GreenThreshold = 2000m;
        public const decimal OrangeThreshold = 2000m;
    }

    public static class SalaryAdjustments
    {
        public const decimal DecemberBonusMultiplier = 1.10m;
        public const decimal SummerDeductionMultiplier = 0.95m;
        public const int DecemberMonth = 12;
        public const int SummerStartMonth = 6;
        public const int SummerEndMonth = 8;
    }
}
