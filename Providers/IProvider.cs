using System.Threading.Tasks;

namespace gpmdp_rdr
{
    interface IProvider
    {
        void Start(string saveFileName);

        bool IsUseable();
    }
}
