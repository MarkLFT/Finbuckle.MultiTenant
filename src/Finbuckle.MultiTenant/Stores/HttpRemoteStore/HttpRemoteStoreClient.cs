// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant.Stores;

public class HttpRemoteStoreClient<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    private readonly IHttpClientFactory clientFactory;

    public HttpRemoteStoreClient(IHttpClientFactory clientFactory)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }

    public async Task<TTenantInfo?> TryGetByIdentifierAsync(string endpointTemplate, string identifier)
    {
        var client = clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        string uri = string.Empty;

        if(endpointTemplate.EndsWith('/'))
            uri = $"{endpointTemplate}{identifier}";
        else
            uri = $"{endpointTemplate}/{identifier}";

        //in original tests, a place holder was inserted fo rthe identifier, this was removed so GetAll will work.
        //var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.DefaultEndpointTemplateIdentifierToken, identifier);
        
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TTenantInfo>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return result;
    }

    public async Task<IEnumerable<TTenantInfo>> TryGetAllAsync(string endpointTemplate)
    { 
        var client = clientFactory.CreateClient(typeof(HttpRemoteStoreClient<TTenantInfo>).FullName!);
        
        // endpointTemplate now only contains base url.
        //var uri = endpointTemplate.Replace(HttpRemoteStore<TTenantInfo>.DefaultEndpointTemplateIdentifierToken, string.Empty);
        
        var response = await client.GetAsync(endpointTemplate);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<IEnumerable<TTenantInfo>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return result;
    }
}