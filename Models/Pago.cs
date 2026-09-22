using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models;

public class Pago
{
    public int Id_pago { get; set; }

    [Required(ErrorMessage = "El concepto es obligatorio")]
    public string Concepto { get; set; } = "";

    [Required(ErrorMessage = "La fecha de pago es obligatoria")]
    public DateTime Fecha_pago { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "El importe debe ser mayor a cero")]
    public decimal Importe { get; set; }

    public bool Estado { get; set; } = true;

    [Required(ErrorMessage = "Hay que elegir una reserva")]
    public int Id_reserva { get; set; }

    public int Id_usuario_creador { get; set; }

    public int Id_usuario_anulador { get; set; }

    public override string ToString() => $"Pago #{Id_pago} - {Concepto}: {Importe.ToString("C")}";
}