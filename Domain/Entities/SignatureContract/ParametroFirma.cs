namespace Domain.Entities.SignatureContracts
{
    public class ParametroFirma
    {
        public string NombreParametro { get; set; } = string.Empty;
        public int Hora { get; set; }
        public int Minuto { get; set; }
        public string ZonaHoraria { get; set; } = "UTC";
        public string Descripcion { get; set; } = string.Empty;
        public int IdCanal { get; set; }
        public string Canal { get; set; }
    }
}