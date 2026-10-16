using System.Text.RegularExpressions;

namespace AsistenteIA.Helpers;

/// <summary>
/// Provee métodos utilitarios para limpiar y normalizar el formato de las respuestas
/// generadas por modelos de lenguaje (LLMs como Google Gemini), eliminando o transformando
/// marcas Markdown y LaTeX crudas que afean la interfaz gráfica.
/// </summary>
public static class TextFormatter
{
    private static readonly Regex BoldRegex = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
    private static readonly Regex ItalicRegex = new(@"\*(.+?)\*", RegexOptions.Compiled);
    private static readonly Regex HeadingRegex = new(@"^#{1,6}\s*", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex HorizontalRuleRegex = new(@"^---+", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex LatexMathBlockRegex = new(@"\$\$(.+?)\$\$", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex LatexMathInlineRegex = new(@"\$(.+?)\$", RegexOptions.Compiled);
    private static readonly Regex MultipleLineBreaksRegex = new(@"\n{3,}", RegexOptions.Compiled);

    /// <summary>
    /// Limpia el texto eliminando marcas de Markdown crudas (**negrita**, *cursiva*, ### títulos, etc.)
    /// y notación de fórmulas de LaTeX para asegurar legibilidad en controles Label estándar de MAUI.
    /// </summary>
    public static string CleanMarkdown(string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return string.Empty;

        string text = rawText.Trim();

        // 1. Normalizar bloques de matemáticas $$4 + 4 = 8$$ a "4 + 4 = 8"
        text = LatexMathBlockRegex.Replace(text, "$1");

        // 2. Normalizar matemáticas en línea $\mathbb{Z}$ o $x$ a texto simple
        text = LatexMathInlineRegex.Replace(text, "$1");

        // 3. Eliminar comandos LaTeX comunes (\mathbb{Z} -> Z, \frac{a}{b} -> a/b)
        text = text.Replace(@"\mathbb{Z}", "números enteros")
                   .Replace(@"\mathbb{R}", "números reales")
                   .Replace(@"\mathbb{N}", "números naturales");

        // 4. Limpiar negritas **palabra** -> palabra
        text = BoldRegex.Replace(text, "$1");

        // 5. Limpiar cursivas *palabra* -> palabra
        text = ItalicRegex.Replace(text, "$1");

        // 6. Limpiar encabezados Markdown (#, ##, ###)
        text = HeadingRegex.Replace(text, string.Empty);

        // 7. Limpiar divisores horizontales (---)
        text = HorizontalRuleRegex.Replace(text, string.Empty);

        // 8. Normalizar saltos de línea repetidos
        text = MultipleLineBreaksRegex.Replace(text, "\n\n");

        return text.Trim();
    }
}
