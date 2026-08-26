namespace MeterImportV2.Helpers
{
    public static class WordForms
    {
        public static string GetRecordsWord(int count)
        {
            int lastDigit = count % 10;
            int lastTwoDigits = count % 100;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
                return "записей";

            if (lastDigit == 1)
                return "запись";

            if (lastDigit >= 2 && lastDigit <= 4)
                return "записи";

            return "записей";
        }
    }
}
