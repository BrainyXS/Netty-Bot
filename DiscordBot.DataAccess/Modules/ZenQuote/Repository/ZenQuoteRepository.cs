using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordBot.DataAccess.Entities;
using DiscordBot.DataAccess.Modules.ZenQuote.BusinessLogic;
using DiscordBot.DataAccess.NHibernate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Linq;

namespace DiscordBot.DataAccess.Modules.ZenQuote.Repository;

public class ZenQuoteRepository : IZenQuoteRepository
{
    private readonly ISessionFactoryProvider _provider;

    public ZenQuoteRepository(ISessionFactoryProvider provider)
    {
        _provider = provider;
    }

    public async Task<IEnumerable<ZenQuoteRegistrationData>> LoadAllRegistrations()
    {
        using (var session = _provider.OpenSession())
        {
            var entities = await session.Query<ZenQuoteRegistrationEntity>().ToListAsync();
            var datas = entities.Select(MapToData);
            return datas;
        }
    }

    public async Task<string> RetrieveQuoteOfTheDayAsync()
    {
        var http = new HttpClient();
        var json = await http.GetStringAsync("https://zenquotes.io/api/today");
        var quote =
            (JsonConvert.DeserializeObject((JsonConvert.DeserializeObject(json) as JArray)[0]
                .ToString()) as JObject).First.First.ToString();
        return quote;
    }

    public async Task SaveRegistrationAsync(ZenQuoteRegistrationData registration)
    {
        var entity = new ZenQuoteRegistrationEntity
        {
            Id = registration.Id,
            ChannelId = registration.Channelid.ToString(),
            GuildId = registration.GuildId.ToString()
        };
        using (var session = _provider.OpenSession())
        {
            await session.SaveOrUpdateAsync(entity);
            await session.FlushAsync();
        }
    }

    public async Task RemoveRegistrationAsync(long registrationId)
    {
        using (var session = _provider.OpenSession())
        {
            var entity = await session.LoadAsync<ZenQuoteRegistrationEntity>(registrationId);
            await session.DeleteAsync(entity);
            await session.FlushAsync();
        }
    }

    private ZenQuoteRegistrationData MapToData(ZenQuoteRegistrationEntity entity)
    {
        return new ZenQuoteRegistrationData
        {
            Channelid = ulong.Parse(entity.ChannelId),
            Id = entity.Id,
            GuildId = ulong.Parse(entity.GuildId)
        };
    }
}