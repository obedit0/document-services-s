namespace KeynuaInfrastructure.Collections.Response;

public sealed record HttpResponseResult<T>(
    int StatusCode,
    bool IsSuccess,
    T? Content,
    //Dictionary<string, string> Headers,
    string Url
);