using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;

namespace Inmobiliaria;

public class Reserva()
{
    [Required]
    public int Id_reserva{get; set;}

    [Required]
    public DateFormat Fecha_inicio{get; set;}

    [Required]
    public DateFormat Fecha_fin{get; set;}

     [Required]
    public DateFormat Fecha_fin_efectiva{get; set;}

     [Required]
    public DateFormat Monto_diario{get; set;}

    [Required]
    public bool Estado{get; set;}


    [Required]
    public int Id_inmueble{get; set;}

    [Required]
    public int Id_inquilino{get; set;}

    [Required]
    public int Id_usuario_creador{get; set;}

    [Required]
    public int Id_usuario_finalizador{get; set;}

    public override string ToString()
    {
        return $"Reserva #{Id_reserva} - Inmueble: {Id_inmueble}, Inquilino: {Id_inquilino}, Desde: {Fecha_inicio}, Hasta: {Fecha_fin}, Monto Diario: {Monto_diario}, Estado: {Estado}";
    }
}