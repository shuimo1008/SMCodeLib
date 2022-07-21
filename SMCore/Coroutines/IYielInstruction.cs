using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMCore.Coroutines
{
    public interface IYielInstruction
    {
        bool Await(float deltaTime);
    }
}
