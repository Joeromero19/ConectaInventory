namespace ConectaInventory.Services;

// Resultado de una operacion de negocio. Los controladores solo muestran
// la confirmacion cuando Exito es true, es decir, despues de persistir.
public class Resultado
{
    public bool Exito { get; private init; }
    public string Mensaje { get; private init; } = string.Empty;
    public int Id { get; private init; }

    public static Resultado Ok(int id, string mensaje = "") =>
        new() { Exito = true, Id = id, Mensaje = mensaje };

    public static Resultado Error(string mensaje) =>
        new() { Exito = false, Mensaje = mensaje };
}
