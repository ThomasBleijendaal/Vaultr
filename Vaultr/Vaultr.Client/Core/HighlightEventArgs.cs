﻿using RapidCMS.Core.Abstractions.Mediators;

namespace Vaultr.Client.Core;

internal record HighlightEventArgs(Guid EditorId) : IMediatorEventArgs;
