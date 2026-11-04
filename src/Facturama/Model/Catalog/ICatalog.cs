namespace Facturama.Model.Catalog;

/// <summary>An offline copy of a SAT/Facturama catalog.</summary>
/// <typeparam name="TEntry">The entry type.</typeparam>
public interface ICatalog<TEntry>
    where TEntry : CatalogEntry
{
    /// <summary>Gets every entry of the catalog.</summary>
    static abstract IReadOnlyList<TEntry> Entries { get; }

    /// <summary>Finds the entry with the given name.</summary>
    /// <param name="name">The exact entry name.</param>
    /// <returns>The matching entry.</returns>
    /// <exception cref="ArgumentException">There is no entry with that name.</exception>
    static abstract TEntry FindByName(string name);

    /// <summary>Finds the first entry with the given value. Use <see cref="FindAllByValue"/> when the value is not unique.</summary>
    /// <param name="value">The exact entry value.</param>
    /// <returns>The first matching entry.</returns>
    /// <exception cref="ArgumentException">There is no entry with that value.</exception>
    static abstract TEntry FindByValue(string value);

    /// <summary>Finds every entry with the given value (empty when there is none).</summary>
    /// <param name="value">The exact entry value.</param>
    /// <returns>The matching entries.</returns>
    static abstract IEnumerable<TEntry> FindAllByValue(string value);
}
