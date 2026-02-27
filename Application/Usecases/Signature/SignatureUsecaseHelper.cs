using Application.Internals.Adapters;
using Domain.Entities.Config;
using Domain.Enums;

namespace Application.Usecases.Signature;

internal static class SignatureUsecaseHelper
{
    public static ChannelEnum? ParseChannel(string? value)
    {

        return Enum.TryParse<ChannelEnum>(value, true, out var channel) ? channel : null;
    }

    public static bool IsWithinTimeWindow(UploadTimeWindowConfig window, DateTime value)
    {
        var time = value.TimeOfDay;
        var from = TimeSpan.FromMinutes(window.FromMin);
        var to = TimeSpan.FromMinutes(window.ToMin);

        return from <= to
            ? time >= from && time <= to
            : time >= from || time <= to;
    }
}
