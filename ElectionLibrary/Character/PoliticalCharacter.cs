﻿using ElectionLibrary.Character.Behavior;
using ElectionLibrary.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionLibrary.Character
{
    public abstract class PoliticalCharacter : ElectionCharacter
    {
        public PoliticalParty PoliticalParty { get; }

        protected PoliticalCharacter(string name, AbstractBehavior behavior, Position position, PoliticalParty politicalParty) : base(name, behavior, position)
        {
            this.PoliticalParty = politicalParty;
        }
    }
}
