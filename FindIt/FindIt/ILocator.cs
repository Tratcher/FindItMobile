﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FindIt
{
    public interface ILocator
    {
        Task<string> GetLocationAsync();
    }
}