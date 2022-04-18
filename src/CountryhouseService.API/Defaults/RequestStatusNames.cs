namespace CountryhouseService.API.Defaults
{
    public static class RequestStatusNames
    {
        public const string PENDING = "PENDING";
        public const string REJECTED = "REJECTED";
        public const string ACCEPTED = "ACCEPTED";

        public static readonly string[] namesArray = { PENDING, REJECTED, ACCEPTED };
    }
}
