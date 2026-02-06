using Domain.Entities.SignatureContracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongodbInfrastructure.Collections
{
    public class ParametroFirmaDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("cNombreParametro")]
        public string NombreParametro { get; set; }

        [BsonElement("nHora")]
        public int Hora { get; set; }

        [BsonElement("nMinuto")]
        public int Minuto { get; set; }

        [BsonElement("nZonaHoraria")]
        public string ZonaHoraria { get; set; }

        [BsonElement("cDescripcion")]
        public string Descripcion { get; set; }

        [BsonElement("idCanal")]
        public int IdCanal { get; set; }

        [BsonElement("cCanal")]
        public string Canal { get; set; }

        public ParametroFirma ToDomain()
        {
            return new ParametroFirma
            {
                NombreParametro = NombreParametro,
                Hora = Hora,
                Minuto = Minuto,
                ZonaHoraria = ZonaHoraria,
                Descripcion = Descripcion,
                IdCanal = IdCanal,
                Canal = Canal
            };
        }
    }
}