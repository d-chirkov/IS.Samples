using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IS.Models;

namespace IS.Repos
{
    public interface IClientRepo
    {
        Client GetClient(string id);
    }
}
