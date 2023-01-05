using System.Diagnostics;

namespace audiamus.aux
{
    public interface IProcessList
    {
        bool Add(Process process);
        bool Remove(Process process);
    }
}
