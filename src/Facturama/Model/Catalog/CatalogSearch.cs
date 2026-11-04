namespace Facturama.Model.Catalog;

internal static class CatalogSearch
{
    internal static TEntry ByName<TEntry>(IReadOnlyList<TEntry> entries, string name, string label)
        where TEntry : CatalogEntry
    {
        foreach (TEntry entry in entries)
        {
            if (string.Equals(entry.Name, name, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        throw new ArgumentException($"{label} with name \"{name}\" was not found", nameof(name));
    }

    internal static TEntry ByValue<TEntry>(IReadOnlyList<TEntry> entries, string value, string label)
        where TEntry : CatalogEntry
    {
        foreach (TEntry entry in entries)
        {
            if (string.Equals(entry.Value, value, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        throw new ArgumentException($"{label} with value \"{value}\" was not found", nameof(value));
    }

    internal static IEnumerable<TEntry> AllByValue<TEntry>(IReadOnlyList<TEntry> entries, string value)
        where TEntry : CatalogEntry
        => entries.Where(entry => string.Equals(entry.Value, value, StringComparison.Ordinal));
}
