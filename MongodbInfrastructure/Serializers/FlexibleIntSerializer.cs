using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongodbInfrastructure.Serializers;

public class FlexibleIntSerializer : SerializerBase<int>
{
    public override int Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();

        return bsonType switch
        {
            BsonType.Int32 => context.Reader.ReadInt32(),
            BsonType.Int64 => (int)context.Reader.ReadInt64(),
            BsonType.Double => (int)context.Reader.ReadDouble(),
            BsonType.String => int.TryParse(context.Reader.ReadString(), out var result) ? result : 0,
            BsonType.Null => 0,
            _ => throw new BsonSerializationException($"Cannot deserialize BsonType {bsonType} to Int32")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, int value)
    {
        context.Writer.WriteInt32(value);
    }
}
