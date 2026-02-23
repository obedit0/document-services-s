namespace SystemAPI.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class DefaultErrorCodeAttribute : Attribute
{
    public DefaultErrorCodeAttribute(string errorCode)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
