namespace Domain.Enums;

public static class HttpMessageEnum
{
    public static string SUCCESS_RESPONSE = "La petición se completó exitosamente";
    public static string SUCCESS_EMPTY_RESPONSE = "La solicitud se completó exitosamente, pero su respuesta no tiene ningún contenido";
    public static string INTERNAL_SERVER_ERROR = "La solicitud no pudo ser procesada debido a un problema interno";
    public static string INVALID_REQUEST = "La petición no pudo completarse debido a que la solicitud no fue válida";
}
