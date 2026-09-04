namespace Inmobiliaria.Models;

public interface IRepositorioImagenInmueble
{
    int Alta(ImagenInmueble i);

    int Baja(int id);

    ImagenInmueble? ObtenerPorId(int id);

    IList<ImagenInmueble> BuscarPorInmueble(int inmuebleId);

    int EliminarTodasPorInmueble(int inmuebleId);
}
