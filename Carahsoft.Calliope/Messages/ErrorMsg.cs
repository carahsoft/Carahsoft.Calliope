﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope
{
    public class ErrorMsg : CalliopeMsg
    {
        public ErrorMsg(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception { get; }
    }
}