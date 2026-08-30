namespace MeterImportV2.Helpers
{
    public static class TariffZone
    {
        public const string DayTariff = "ДЕНЬ";
        public const string NightTariff = "НОЧЬ";
        public const string ConstantTariff = "КРУГЛОСУТОЧНЫЙ";
        public static string Normalize(string? tariffZone)
        {
            return string.IsNullOrWhiteSpace(tariffZone) ? ConstantTariff : tariffZone.ToUpper();
        }
    }
}
