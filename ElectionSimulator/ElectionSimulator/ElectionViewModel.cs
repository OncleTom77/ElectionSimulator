﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ElectionLibrary.Environment;

namespace ElectionSimulator
{
    public class ElectionViewModel : BaseViewModel
    {
        private string applicationTitle;

        public string ApplicationTitle
        {
            get
            {
                return applicationTitle;
            }
            set
            {
                applicationTitle = value;
                OnPropertyChanged("ApplicationTitle");
            }
        }

        public Boolean Running { get; set; }

        public int DimensionX;

        public int DimensionY;

        public int RefreshRate { get; set; }

        public List<List<AbstractArea>> Areas { get; set; }

        public ElectionViewModel()
        {
            DimensionX = 20;
            DimensionY = 20;
            Areas = new List<List<AbstractArea>>();
        }

        internal void NextTurn()
        {
            /*
            foreach (Character character in Characters)
            {
                character.NextTurn(DimensionX, DimensionY);
            }
            */
        }

        internal void Play()
        {
            Running = true;
            while (Running)
            {
                Thread.Sleep(RefreshRate);
                NextTurn();
            }
        }

        internal void Stop()
        {
            Running = false;
        }

        internal void AddStreet(int i)
        {
            
        }

        internal void AddBuilding(int i)
        {
         
        }

        internal void AddEmpty(int i)
        {
            
        }
    }
}
