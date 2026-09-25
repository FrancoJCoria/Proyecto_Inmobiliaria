namespace Inmobiliaria.Models;

/// <summary>
/// Reglas que comparten los endpoints que alimentan los desplegables con búsqueda.
/// Centralizadas acá para que la longitud mínima y el tope de resultados estén
/// definidos en un solo lugar, igual que las constantes de _ScriptDesplegable.cshtml.
/// </summary>
public static class BusquedaDesplegable
{
    /// <summary>
    /// Caracteres mínimos que debe tener el término escrito en el buscador. Con menos
    /// caracteres el prefijo coincide con casi todo (ej. "a") y la consulta no acota
    /// nada, así que ni siquiera se llega a consultar la base.
    /// </summary>
    public const int LongitudMinima = 3;

    /// <summary>
    /// Máximo de opciones que se devuelven por consulta. El repositorio lo aplica con
    /// un LIMIT, así que un término corto tampoco puede terminar bajando el catálogo.
    /// </summary>
    public const int MaximoOpciones = 20;

    /// <summary>
    /// Indica si el término alcanza para consultar. Si no alcanza se devuelve una lista
    /// vacía: es preferible mostrar "sin resultados" que el catálogo completo.
    /// </summary>
    public static bool EsTerminoUtil(string? termino)
    {
        return !string.IsNullOrWhiteSpace(termino) && termino.Trim().Length >= LongitudMinima;
    }
}
