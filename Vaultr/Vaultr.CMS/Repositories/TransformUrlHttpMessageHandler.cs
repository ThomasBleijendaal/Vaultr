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
        TransformUri(request);
        return base.Send(request, cancellationToken);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        TransformUri(request);
        return base.SendAsync(request, cancellationToken);
    }

    private void TransformUri(HttpRequestMessage request)
    {
        if (request.RequestUri != null)
        {
            request.RequestUri = new Uri(request.RequestUri.ToString().Replace(".net/", $".net/{_keyVaultName}/"));
        }
    }
}
