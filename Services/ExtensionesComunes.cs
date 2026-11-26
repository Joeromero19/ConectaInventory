using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ConectaInventory.Services;

public static class ExtensionesComunes
{
    // Id del usuario autenticado (claim NameIdentifier definida en el login)
    public static int GetUserId(this ClaimsPrincipal usuario) =>
        int.Parse(usuario.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // Los seriales se comparan siempre normalizados
    public static string NormalizarSerial(this string serial) => serial.Trim().ToUpperInvariant();

    // "EnDiagnostico" -> "En Diagnostico" para mostrarlo en pantalla
    public static string Texto(this Enum valor) =>
        Regex.Replace(valor.ToString(), "(?<!^)([A-Z])", " $1");
}
