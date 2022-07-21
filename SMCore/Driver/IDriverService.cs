﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Driver
{
    public interface IDriverService
    {
        void Update(float deltaTime);

        void Subscribe(Action<float> onDriving);

        void Unsubscribe(Action<float> onDriving);
    }
}
