using System;

namespace ICSharpCode.ILSpy
{
    // Minimal shim to represent navigation request event args used by ILSpy.
    public sealed class RequestNavigateEventArgs : EventArgs
    {
        public Uri Uri { get; }
        public object? ExtraData { get; }

        public RequestNavigateEventArgs(Uri uri, object? extraData)
        {
            Uri = uri;
            ExtraData = extraData;
        }
    }
}
