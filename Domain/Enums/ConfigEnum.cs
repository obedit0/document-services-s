using System.ComponentModel;
using System.Reflection;

namespace Domain.Enums;

public enum EstadoFirma
{
    PENDIENTE,
    EN_PROCESO,
    COMPLETADO,
    ANULADO,        //Cuando se extorna la solicitud
    EXPIRADO,
    ERROR,
    CANCELADO       //Cuando el usuario cancela la solicitud manualmente
}
public enum DocumentType
{
    [Description("DNI")]
    DNI = 1,

    [Description("Carné de extranjería")]
    CarneExtranjeria = 2,

    [Description("Pasaporte")]
    Pasaporte = 3,

    [Description("RUC")]
    RUC = 4,

    [Description("Código IBAN")]
    CodigoIBAN = 5,

    [Description("Código ABA")]
    CodigoABA = 6,

    [Description("Código SWIFT")]
    CodigoSWIFT = 7,

    [Description("Código Fiscal")]
    CodigoFiscal = 8,

    [Description("Carné F. Policiales")]
    CarneFuerzasPoliciales = 9,

    [Description("Carné F. Armadas")]
    CarneFuerzasArmadas = 10,

    [Description("DNI Menor")]
    DNIMenor = 11,

    [Description("Otros")]
    Otros = 12,

    [Description("Código SBS")]
    CodigoSBS = 13,

    [Description("Partida Nacimiento")]
    PartidaNacimiento = 14
}


public enum Nationality
{
    [Description("PERUANA")]
    Peruana = 0,

    [Description("EXTRANJERA")]
    Extranjera = 1
}


public enum Gender : int
{
    [Description("M")]
    Masculino = 1,

    [Description("F")]
    Femenino = 2,

    [Description("Otro")]
    Otro = 3,
}


public enum CurrencyType : int
{
    [Description("PEN")]
    Soles = 1,

    [Description("USD")]
    Dolares = 2
}

public enum Channel
{
    [Description("VENTANILLA")]
    Ventanilla = 1,

    [Description("MOVIL-WAP")]
    MovilWAP = 2,

    [Description("ATMs")]
    ATMs = 3,

    [Description("BANCO NACION")]
    BancoNacion = 4,

    [Description("BANCO DE CREDITO DEL PERU")]
    BancoCreditoPeru = 5,

    [Description("AHORRO WEB")]
    AhorroWeb = 6,

    [Description("KASNET")]
    Kasnet = 7,

    [Description("BILLETERA ELECTRONICA MOVIL - BIM")]
    BilleteraElectronicaBIM = 8,

    [Description("MASIVO")]
    Masivo = 9,

    [Description("BOTON PAGO NIUBIZ")]
    BotonPagoNiubiz = 10,

    [Description("APP")]
    App = 11,

    [Description("CCE")]
    CCE = 12
}

public enum ClientType
{
    [Description("NATURAL")]
    Natural = 1,

    [Description("JURIDICA CON FINES DE LUCRO")]
    JuridicaConFinesDeLucro = 2,

    [Description("JURIDICA SIN FINES DE LUCRO")]
    JuridicaSinFinesDeLucro = 3
}


public enum ClientStatus
{
    [Description("ACTIVO")]
    ActivoNatural = 1,

    [Description("FALLECIDO")]
    FallecidoNatural = 2,

    [Description("ACTIVO")]
    ActivoJuridicoConFines = 3,

    [Description("BAJA DE OFICIO")]
    BajaDeOficioJuridicoConFines = 4,

    [Description("ACTIVO")]
    ActivoJuriidcoSinFines = 5,

    [Description("BAJA DE OFICIO")]
    BajaDeOficioJuridicoSinFines = 6
}


public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString());
        if (member.Length == 0) return value.ToString();

        var desc = member[0].GetCustomAttribute<DescriptionAttribute>();
        return desc?.Description ?? value.ToString();
    }
}