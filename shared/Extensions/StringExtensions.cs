using System.Text.RegularExpressions;

namespace CaptureSys.Shared.Extensions;

/// <summary>
/// Extensions pour les chaînes de caractères
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Nettoie le texte OCR des caractères indésirables
    /// </summary>
    public static string CleanOcrText(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Remplacer les caractères de contrôle
        text = Regex.Replace(text, @"[\x00-\x1F\x7F]", " ");
        
        // Normaliser les espaces
        text = Regex.Replace(text, @"\s+", " ");
        
        // Nettoyer les caractères spéciaux problématiques
        text = text.Replace("�", "").Replace("", "");
        
        return text.Trim();
    }

    /// <summary>
    /// Extrait les nombres d'une chaîne
    /// </summary>
    public static string ExtractNumbers(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return Regex.Replace(text, @"[^\d.,]", "");
    }

    /// <summary>
    /// Extrait les dates potentielles d'une chaîne
    /// </summary>
    public static IEnumerable<string> ExtractDates(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        // Patterns de dates courantes
        var patterns = new[]
        {
            @"\b\d{1,2}[\/\-\.]\d{1,2}[\/\-\.]\d{2,4}\b", // DD/MM/YYYY
            @"\b\d{2,4}[\/\-\.]\d{1,2}[\/\-\.]\d{1,2}\b", // YYYY/MM/DD
            @"\b\d{1,2}\s+\w+\s+\d{2,4}\b" // DD Month YYYY
        };

        foreach (var pattern in patterns)
        {
            var matches = Regex.Matches(text, pattern);
            foreach (Match match in matches)
            {
                yield return match.Value;
            }
        }
    }

    /// <summary>
    /// Vérifie si la chaîne est un email valide
    /// </summary>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    /// <summary>
    /// Tronque une chaîne à la longueur spécifiée
    /// </summary>
    public static string Truncate(this string text, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
            return text ?? string.Empty;

        return text[..(maxLength - suffix.Length)] + suffix;
    }
}
 