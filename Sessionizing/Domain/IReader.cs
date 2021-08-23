using System.Collections.Generic;

namespace Sessionizing.Domain
{
    public interface IReader
    {
        public List<IEnumerator<PageView>> ReadData();
    }
}