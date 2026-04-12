using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace Dita.Shared.Localization.Services;

public class JsonStringLocalizer(IDistributedCache cache) : IStringLocalizer
{
}