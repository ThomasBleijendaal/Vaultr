using RapidCMS.Core.Abstractions.Mediators;

namespace Vaultr.Client.Components.EventArgs;

internal record HighlightEventArgs(Guid EditorId) : IMediatorEventArgs;
