using Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongodbInfrastructure.Serializers;

public class ChannelSerializer : SerializerBase<int>
{
    public override int Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();

        return bsonType switch
        {
            BsonType.Int32 => context.Reader.ReadInt32(),
            BsonType.Int64 => (int)context.Reader.ReadInt64(),
            BsonType.Double => (int)context.Reader.ReadDouble(),
            BsonType.String => ParseChannelString(context.Reader.ReadString()),
            BsonType.Null => (int)Channel.Ventanilla,
            _ => (int)Channel.Ventanilla
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, int value)
    {
        context.Writer.WriteInt32(value);
    }

    private static int ParseChannelString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (int)Channel.Ventanilla;

        if (int.TryParse(value, out var numeric))
            return numeric;

        return Enum.TryParse<Channel>(value, true, out var channel)
            ? (int)channel
            : (int)Channel.Ventanilla;
    }
}
