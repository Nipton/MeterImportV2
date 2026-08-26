namespace MeterImportV2.Helpers
{
    public class TupleStringComparer : IEqualityComparer<(string, string)>
    {
        public bool Equals((string, string) x, (string, string) y)
        {
            return string.Equals(x.Item1, y.Item1, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode((string, string) obj)
        {
            var h1 = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item1 ?? string.Empty);
            var h2 = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item2 ?? string.Empty);
            return HashCode.Combine(h1, h2);
        }
    }
}
