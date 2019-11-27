using System;

namespace Zynapse.Common
{
    public sealed class DefaultIdentifierGenerator : IIdentifierGenerator
    {
        public static readonly DefaultIdentifierGenerator Instance = new DefaultIdentifierGenerator();

        private DefaultIdentifierGenerator() { }

        public string GenerateIdentifier()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
