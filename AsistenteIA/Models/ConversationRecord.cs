using SQLite;

namespace AsistenteIA.Models;

/// <summary>
/// Entidad representativa de una consulta y respuesta almacenada en la base de datos local SQLite.
/// </summary>
[Table("Conversations")]
public class ConversationRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Prompt { get; set; } = string.Empty;

    [NotNull]
    public string Answer { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Category { get; set; } = "General";

    /// <summary>
    /// Indica si el estudiante ha contrastado y verificado la respuesta con fuentes académicas confiables.
    /// </summary>
    public bool IsVerified { get; set; }

    [Ignore]
    public bool IsPendingVerification => !IsVerified;

    [Ignore]
    public string FormattedDate => CreatedAt.ToLocalTime().ToString("g");

    [Ignore]
    public string ShortPrompt => Prompt.Length > 80 ? string.Concat(Prompt.AsSpan(0, 77), "...") : Prompt;
}
