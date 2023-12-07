using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azure.LaserScanner
{
    public interface IPageViewModel
    {
        string Name { get; }
        bool IsSelected { get; set; }
    }
}
