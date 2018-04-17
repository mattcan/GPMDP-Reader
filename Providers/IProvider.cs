using System.Threading.Tasks;

namespace gpmdp_rdr
{
    interface IProvider
    {
        Task Start(string saveFileName);

        bool IsUseable();
    }
}
