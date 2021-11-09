namespace Vaultr.CMS.Repositories;

public class TransformUrlHttpMessageHandler : HttpClientHandler
{
    private readonly string _keyVaultName;

    public TransformUrlHttpMessageHandler(string keyVaultName)
    {
        _keyVaultName = keyVaultName;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        AddHeader(request);
        return base.Send(request, cancellationToken);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            var json = await request.Content.ReadAsStringAsync();
        }

        AddHeader(request);
        return await base.SendAsync(request, cancellationToken);
    }

    private void AddHeader(HttpRequestMessage request)
    {
        request.Headers.Add("x-vaultr-keyvault", _keyVaultName);
    }
}
