﻿using System;
using AbstractLibrary.Factory;

namespace AbstractLibrary.Character
{
    public abstract class AbstractCharacter
    {
        public string Name { get; private set; }
        public AbstractArea Position { get; set; }

        public AbstractCharacter(string name)
        {
            this.Name = name;
        }
    }
}
