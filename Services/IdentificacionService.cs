using Microsoft.EntityFrameworkCore;
using QRCoder;
using ConectaInventory.Data;
using ConectaInventory.Models;

namespace ConectaInventory.Services;

// Generacion, reimpresion y validacion de codigos QR (HU-0103)
public class IdentificacionService
{
    private const string Prefijo = "CI-";
    private readonly ConectaInventoryContext _db;

    public IdentificacionService(ConectaInventoryContext db)
    {
        _db = db;
    }

    // Equipos que pueden etiquetarse (los dados de baja ya no)
    public async Task<List<Equipo>> ListarParaEtiquetadoAsync() =>
        await _db.Equipos.AsNoTracking()
            .Include(e => e.CodigoQR)
            .Where(e => e.Estado != EstadoEquipo.BajaDefinitiva)
            .OrderBy(e => e.Serial)
            .ToListAsync();

    public async Task<Equipo?> ObtenerConQrAsync(int equipoId) =>
        await _db.Equipos.AsNoTracking()
            .Include(e => e.CodigoQR)
            .FirstOrDefaultAsync(e => e.Id == equipoId);

    // CU1.06: genera el QR solo si el equipo aun no tiene uno
    public async Task<Resultado> GenerarAsync(int equipoId)
    {
        var equipo = await _db.Equipos.Include(e => e.CodigoQR).FirstOrDefaultAsync(e => e.Id == equipoId);
        if (equipo == null)
            return Resultado.Error("El equipo no existe.");
        if (equipo.Estado == EstadoEquipo.BajaDefinitiva)
            return Resultado.Error("No se etiqueta un equipo dado de baja.");
        if (equipo.CodigoQR != null)
            return Resultado.Error("El equipo ya tiene un QR. Use la opcion de reimprimir.");

        _db.CodigosQR.Add(new CodigoQR
        {
            EquipoId = equipo.Id,
            Valor = Prefijo + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo guardar el QR. No se entrega etiqueta.");
        }

        return Resultado.Ok(equipo.Id, "QR generado correctamente.");
    }

    // CU1.07: reimprimir registra el evento y conserva el mismo valor (no duplica identificador)
    public async Task<Resultado> ReimprimirAsync(int equipoId)
    {
        var qr = await _db.CodigosQR.FirstOrDefaultAsync(q => q.EquipoId == equipoId);
        if (qr == null)
            return Resultado.Error("El equipo aun no tiene QR. Use la opcion de generar.");

        qr.Reimpresiones++;
        qr.UltimaReimpresion = DateTime.Now;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo registrar la reimpresion.");
        }

        return Resultado.Ok(equipoId, "Reimpresion registrada. Se conserva el mismo codigo QR.");
    }

    // CU1.08: lectura valida y asociada a una ficha existente
    public async Task<(Resultado Resultado, Equipo? Equipo)> ValidarAsync(string codigo)
    {
        codigo = codigo.Trim().ToUpperInvariant();
        if (!codigo.StartsWith(Prefijo) || codigo.Length > 50)
            return (Resultado.Error("Lectura invalida: el codigo no tiene el formato de ConectaInventory."), null);

        var qr = await _db.CodigosQR.AsNoTracking()
            .Include(q => q.Equipo).ThenInclude(e => e.Categoria)
            .Include(q => q.Equipo).ThenInclude(e => e.Ubicacion)
            .FirstOrDefaultAsync(q => q.Valor == codigo);

        if (qr == null)
            return (Resultado.Error("Codigo desconocido: no esta asociado a ningun equipo."), null);

        return (Resultado.Ok(qr.EquipoId, "Codigo valido."), qr.Equipo);
    }

    // Dibuja el QR como SVG para mostrarlo e imprimirlo desde el navegador
    public static string GenerarSvg(string valor)
    {
        using var generador = new QRCodeGenerator();
        using var datos = generador.CreateQrCode(valor, QRCodeGenerator.ECCLevel.Q);
        return new SvgQRCode(datos).GetGraphic(6);
    }
}
