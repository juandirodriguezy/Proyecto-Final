namespace AsistenteIA.Helpers;

/// <summary>
/// Validador de prompts para consultas académicas según el requerimiento funcional RF02 / P01.
/// Valida longitud mínima (5 caracteres) y máxima (2000 caracteres) excluyendo espacios en blanco.
/// </summary>
public static class PromptValidator
{
    public const int MinLength = 5;
    public const int MaxLength = 2000;

    /// <summary>
    /// Determina si un prompt ingresado cumple con las restricciones de longitud y contenido.
    /// </summary>
    public static bool IsValid(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return false;

        int trimmedLength = prompt.Trim().Length;
        return trimmedLength >= MinLength && trimmedLength <= MaxLength;
    }

    /// <summary>
    /// Valida un prompt y retorna un mensaje de error descriptivo en caso de incumplimiento.
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) Validate(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return (false, "El prompt no puede estar vacío ni contener solo espacios.");

        int trimmedLength = prompt.Trim().Length;

        if (trimmedLength < MinLength)
            return (false, $"El prompt debe tener al menos {MinLength} caracteres significativos.");

        if (trimmedLength > MaxLength)
            return (false, $"El prompt no puede exceder los {MaxLength} caracteres.");

        return (true, null);
    }
}
