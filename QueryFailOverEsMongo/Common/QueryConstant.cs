namespace QueryFailOverEsMongo.Common
{
    public static class QueryConstant
    {
        public const int MaxLimit = 5000;
        public const int DefaultOffset = 0;
        public static bool IsSwitch = false;
        public static long PreviousSwitchTime = 0;
    }
}