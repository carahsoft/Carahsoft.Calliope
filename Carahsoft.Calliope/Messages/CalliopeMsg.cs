﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Messages
{
    public abstract class CalliopeMsg
    {
        public static CalliopeMsg Quit = new QuitMsg();
    }
}
