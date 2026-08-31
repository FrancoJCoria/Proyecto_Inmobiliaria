using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models;

public class Inmueble
{
    public int Id_inmueble { get; set; }
    public string direccion {get; set; }
    public int cupo {get; set; }
    public decimal precio_dia {get; set; }
    public decimal porcentaje_reserva {get; set; }
    public bool disponible {get; set; }
    public string portada {get; set; }
    public int id_propietario {get; set; }
    public int id_tipo {get; set; }
    public string estado {get; set; }
     public override string ToString()
    {
        return $"(Id_inmueble: {Id_inmueble}), Direccion: {direccion}, Cupo: {cupo}, Precio por dia: {precio_dia}, Porcentaje de reserva: {porcentaje_reserva}, Disponible: {disponible}, Portada: {portada}, Id_propietario: {id_propietario}, Id_tipo: {id_tipo}, Estado: {estado}";
    }
    }