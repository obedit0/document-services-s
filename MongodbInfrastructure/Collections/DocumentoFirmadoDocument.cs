using MongoDB.Bson.Serialization.Attributes;

namespace MongodbInfrastructure.Collections;

public class DocumentoFirmadoDocument
{
    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("tipo")]
    public string Tipo { get; set; } = string.Empty;
}