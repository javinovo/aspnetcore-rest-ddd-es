namespace WebApp.Models
{
    public class CrearEquipo
    {
        public string Nombre { get; set; }
    }

    public class ActualizarNombreEquipo
    {
        public string NuevoNombre { get; set; }
        public int OriginalVersion { get; set; }
    }
}
