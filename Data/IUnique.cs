﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ID = System.Int32;

namespace sturvey_app.Data
{
    public interface IUnique
    {
        ID Id(); // returns item ID
    }
}
