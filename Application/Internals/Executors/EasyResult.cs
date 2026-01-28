using Application.Internals.Adapters;

/* ********************************************************************************************************          
# * Copyright © 2026 Arify Labs - All rights reserved.   
# * 
# * Info                  : EasyResult Class to standardize success and failure responses
# *
# * By                    : Victor Jhampier Caxi Maquera
# * Email/Mobile/Phone    : victorjhampier@gmail.com | 968991714
# *
# * Creation date         : 01/01/2026
# * 
# **********************************************************************************************************/

namespace Application.Internals.Executors;

public sealed class EasyResult<T>
{
    public bool IsSuccess { get; set; }
    public int Status { get; set; }
    public T? SuccessValue { get; set; }
    public IReadOnlyCollection<ValidationResultAdapter> ValidationValues { get; set; } = Array.Empty<ValidationResultAdapter>();

    public static EasyResult<T> Success(T successValue) => new()
    {
        IsSuccess = true,
        Status = 200,
        SuccessValue = successValue
    };

    public static EasyResult<T> Failure(int status, IReadOnlyCollection<ValidationResultAdapter> validationValues) => new()
    {
        IsSuccess = false,
        Status = status,
        ValidationValues = validationValues
    };

    public static EasyResult<T> Empty() => new()
    {
        IsSuccess = true,
        Status = 204
    };
}
