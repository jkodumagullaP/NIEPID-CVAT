namespace CAT.AID.Helpers
{
    public static class ScoreHelper
    {
        public static int GetScore(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            return value switch
            {
                "Dependent" => 0,
                "Verbal Prompt" => 1,
                "Gestural Prompt" => 1,
                "Physical Prompt" => 1,
                "Independent" => 2,
                _ => 0
            };
        }
    }
}