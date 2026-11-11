using System;

namespace Attest
{
    /// <summary>A published document a rule was written from.</summary>
    public sealed class Source
    {
        internal Source(string authority, string url, string title, DateTime accessed, SourceKind kind)
        {
            Authority = authority;
            Url = url;
            Title = title;
            Accessed = accessed;
            Kind = kind;
        }

        /// <summary>The body that published it.</summary>
        public string Authority { get; }

        /// <summary>Where it was read.</summary>
        public string Url { get; }

        /// <summary>The document title.</summary>
        public string Title { get; }

        /// <summary>The date it was last read.</summary>
        public DateTime Accessed { get; }

        /// <summary>What sort of document it is.</summary>
        public SourceKind Kind { get; }
    }
}
