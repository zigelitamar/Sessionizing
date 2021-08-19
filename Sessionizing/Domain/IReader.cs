using System.Collections.Generic;

namespace Sessionizing.Domain
{
    public interface IReader
    {
        public List<IEnumerable<PageView>> ReadData();
    }
}