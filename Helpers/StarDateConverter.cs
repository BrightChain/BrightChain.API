namespace BrightChain.API.Helpers
{
    using StarDateCalc;

    public static class StarDateConverter
    {
        private const StarDateCalc.StarDate.LanguageCode languageCode = StarDateCalc.StarDate.LanguageCode.English;

        public static readonly DateTime BirthdayDateTime = new DateTime(year: 2021, month: 6, day: 16, hour: 3, minute: 33, second: 33);

        public static readonly double Offset = 99055.685D - new StarDate(StarDateConverter.languageCode).TNG(BirthdayDateTime);

        public static double StarDate(DateTime dateTime) => new StarDate(StarDateConverter.languageCode).TNG(dateTime) + Offset;

        public static double Birthday => StarDate(BirthdayDateTime);

        public static double Now => StarDate(DateTime.Now);
    }
}
