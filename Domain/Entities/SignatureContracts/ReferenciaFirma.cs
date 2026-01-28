namespace Domain.Entities.SignatureContracts;

public sealed class ReferenciaFirma
{
    public string Value { get; }

    public ReferenciaFirma(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ReferenciaFirma referencia) => referencia.Value;
}
