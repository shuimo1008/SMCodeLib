﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZCSharpLib.Coroutines
{
    public interface IInterruptInstruction
    {
        bool Await(float deltaTime);
    }
}
