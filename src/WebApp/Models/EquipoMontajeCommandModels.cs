namespace WebApp.Models
{
    public class CrearEquipo
    {
        //public Guid EquipoId { get; set; }
        public string Nombre { get; set; }
    }

    public class ActualizarNombreEquipo
    {
        //public Guid EquipoId { get; set; }
        public string NuevoNombre { get; set; }
        public int OriginalVersion { get; set; }
    }
}
