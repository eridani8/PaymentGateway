using Microsoft.Extensions.Localization;
using MudBlazor;

namespace PaymentGateway.Web;

internal class ResXMudLocalizer(IStringLocalizer<MudResources> localizer) : MudLocalizer
{
    private readonly IStringLocalizer _localization = localizer;
    public override LocalizedString this[string key] => _localization[key];
}