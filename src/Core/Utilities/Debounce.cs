// ------------------------------------------------------------------------
// MIT License - Copyright (c) Microsoft Corporation. All rights reserved.
// ------------------------------------------------------------------------

namespace Blazor.Monaco.Utilities;

/// <summary>
/// The DebounceTask dispatcher delays the invocation of an action until a predetermined interval has elapsed since the last call.
/// This ensures that the action is only invoked once after the calls have stopped for the specified duration.
/// </summary>
public sealed class Debounce : InternalDebounce.DebounceAction
{
}
