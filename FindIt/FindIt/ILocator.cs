using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FindIt.Models;

namespace FindIt
{
    public interface ILocator
    {
        Task<Local> GetLocationAsync();
    }
}
