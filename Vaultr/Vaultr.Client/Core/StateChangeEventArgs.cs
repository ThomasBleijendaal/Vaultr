using RapidCMS.Core.Abstractions.Mediators;
using Vaultr.Client.Core.Models;

namespace Vaultr.Client.Core;

internal record StateChangedEventArgs(ConfigurationState State) : IMediatorEventArgs;
