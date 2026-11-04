namespace Facturama.Model.Catalog;

/// <summary>A catalog entry: a human readable <paramref name="Name"/> and its SAT/Facturama <paramref name="Value"/>.</summary>
/// <param name="Name">The display name (unique within a catalog).</param>
/// <param name="Value">The code to send to the API (not necessarily unique, e.g. <c>IVA</c> and <c>IVA RET</c> share <c>002</c>).</param>
public record CatalogEntry(string Name, string Value);

/// <summary>An entry of the <see cref="CfdiType"/> catalog.</summary>
/// <param name="Name">The display name.</param>
/// <param name="Value">The CFDI type code (<c>I</c>, <c>E</c>, <c>T</c>, <c>P</c>); shared by several entries.</param>
/// <param name="NameId">The Facturama id of the CFDI type.</param>
public sealed record CfdiTypeEntry(string Name, string Value, int NameId) : CatalogEntry(Name, Value);

/// <summary>An entry of the <see cref="CfdiUse"/> catalog.</summary>
/// <param name="Name">The display name.</param>
/// <param name="Value">The CFDI use code (e.g. <c>G03</c>).</param>
/// <param name="Moral">Whether the use is valid for legal entities (personas morales).</param>
/// <param name="Natural">Whether the use is valid for natural persons (personas físicas).</param>
public sealed record CfdiUseEntry(string Name, string Value, bool Moral, bool Natural) : CatalogEntry(Name, Value);
