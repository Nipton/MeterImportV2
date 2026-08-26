namespace MeterImportV2.Helpers
{
    public static class TariffZoneHelper
    {
        public static string Normalize(string? tariffZone)
        {
            return string.IsNullOrWhiteSpace(tariffZone) ? "КРУГЛОСУТОЧНЫЙ" : tariffZone;
        }
    }
}
