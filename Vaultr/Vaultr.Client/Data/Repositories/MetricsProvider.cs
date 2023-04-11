using Azure.Monitor.Query;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Extensions;
using Vaultr.Client.Core;
using Vaultr.Client.Data.Models;

namespace Vaultr.Client.Data.Repositories;

internal class MetricsProvider : IMetricsProvider
{
    private static readonly List<KeyVaultSecretMetric> Metrics = new();
    private static CancellationTokenSource _cancellationTokenSource { get; set; } = new();

    private readonly ICredentialProvider _credentialProvider;
    private IMediator? _mediator;

    public MetricsProvider(ICredentialProvider credentialProvider)
    {
        _credentialProvider = credentialProvider;
    }

    public KeyVaultSecretMetric? GetMetric(string keyVaultName, string secretName)
    {
        var item = Metrics.FirstOrDefault(x => x.KeyVaultName == keyVaultName && x.SecretName == secretName);

        if (item != null)
        {
            return item;
        }

        if (Metrics.Count == 0)
        {
            return null;
        }

        item = new KeyVaultSecretMetric
        {
            KeyVaultName = keyVaultName,
            SecretName = secretName,
            Reads = 0
        };

        Metrics.Add(item);

        return item;
    }

    void IMetricsProvider.AcceptMediator(IMediator mediator)
    {
        _mediator = mediator;
    }

    async Task IMetricsProvider.ReadMetricsAsync(object sender, StateChangedEventArgs args)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new();

        Metrics.Clear();

        var token = _cancellationTokenSource.Token;

        if (args.State.TenantId == null)
        {
            return;
        }

        var client = new LogsQueryClient(_credentialProvider.GetTokenCredential(args.State.TenantId));

        // TODO: loop over keyvaults + keep watching token
        // TODO: check if we should read metrics by configuring it on state
        var query = @"
AzureDiagnostics 
| where OperationName == 'SecretGet' and ResultType == 'Success'
| summarize count() by requestUri_s
| project uri = requestUri_s, count = count_
";

        var data = await client.QueryWorkspaceAsync(workspaceId, query, new QueryTimeRange(TimeSpan.FromHours(24)));

        var requestUriColumn = data.Value.Table.Columns.FindIndex(c => c.Name == "uri");
        var countColumn = data.Value.Table.Columns.FindIndex(c => c.Name == "count");

        if (!requestUriColumn.HasValue || !countColumn.HasValue)
        {
            return;
        }

        var metrics = data.Value.Table.Rows.Select(r => new KeyVaultSecretMetric
        {
            KeyVaultName = args.State.KeyVaults[0].Name,
            SecretName = Uri.TryCreate((string)r[requestUriColumn.Value], UriKind.Absolute, out var uri) ? uri.Segments[^1].Trim('/') : "",
            Reads = (long)r[countColumn.Value]
        }).ToList();

        if (!token.IsCancellationRequested)
        {
            Metrics.AddRange(metrics);

            _mediator?.NotifyEvent(
                this,
                new MetricsLoadedEventArgs(args.State.KeyVaults[0].Name));
        }
    }
}

internal class MetricsProviderMediatorEventRegistration : IMediatorEventListener
{
    private readonly IMetricsProvider _metricsProvider;
    private IDisposable? _registration;

    public MetricsProviderMediatorEventRegistration(IMetricsProvider metricsProvider)
    {
        _metricsProvider = metricsProvider;
    }

    public void RegisterListener(IMediator mediator)
    {
        _registration = mediator.RegisterCallback<StateChangedEventArgs>(_metricsProvider.ReadMetricsAsync);
        _metricsProvider.AcceptMediator(mediator);
    }

    public void Dispose()
    {
        _registration?.Dispose();
        _registration = null;
    }
}
