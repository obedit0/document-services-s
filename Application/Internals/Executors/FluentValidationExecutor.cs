using Application.Internals.Adapters;
using FluentValidation;

/* ********************************************************************************************************          
# * Copyright © 2026 Arify Labs - All rights reserved.   
# * 
# * Info                  : FluentValidExecutor Class to execute FluentValidation validators
# *
# * By                    : Victor Jhampier Caxi Maquera
# * Email/Mobile/Phone    : victorjhampier@gmail.com | 968991714
# *
# * Creation date         : 01/01/2026
# * 
# **********************************************************************************************************/

namespace Application.Internals.Executors;

internal static class FluentValidationExecutor
{
    public static IReadOnlyCollection<ValidationResultAdapter> Execute<T>(T input, IValidator<T> validator)
    {
        var validResult = validator.Validate(input);

        if (validResult.IsValid) return Array.Empty<ValidationResultAdapter>();

        return validResult.Errors
            .Select(x => new ValidationResultAdapter(
                x.ErrorCode,
                x.ErrorMessage,
                x.PropertyName))
            .ToList();
    }

    public static async Task<IReadOnlyCollection<ValidationResultAdapter>> ExecuteAsync<T>(
        T input,
        IValidator<T> validator,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(input, cancellationToken);

        return result.IsValid
            ? Array.Empty<ValidationResultAdapter>()
            : result.Errors.Select(e => new ValidationResultAdapter(
                e.ErrorCode,
                e.ErrorMessage,
                e.PropertyName)).ToList();
    }
}
