﻿using System.Collections.Generic;
using System.Threading;
using ElectionLibrary.Environment;
using ElectionLibrary.Character;
using ElectionLibrary.Factory;
using ElectionLibrary.Parties;
using ElectionLibrary.Event;
using System.ComponentModel;
using AbstractLibrary.Pattern;
using System.Windows.Input;

namespace ElectionSimulator
{
    [System.Serializable]
    public class ElectionViewModel : BaseViewModel, IObservable<ElectionEvent>
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

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        public ICommand DetailsDelegate { get; internal set; }

        public AbstractArea areaSelected;

        public AbstractArea AreaSelected
        {
            get
            {
                return areaSelected;
            }
            set
            {
                System.Console.WriteLine("set");
                areaSelected = value;
                OnPropertyChanged("AreaSelected");
                DetailsDelegate.Execute(null);
            }
        }

        public bool Running { get; set; }

        public int DimensionX;

        public int DimensionY;

        public int RefreshRate { get; set; }

        public int NumberTurn { get; set; }

        public ElectionFactory factory = new ElectionFactory();

        public List<List<AbstractArea>> Areas { get; set; }

        public List<AbstractElectionCharacter> Characters { get; set; }

        public List<PoliticalParty> Parties { get; set; }

        public ElectionEvent Event { get; set; }

        public List<IObserver<ElectionEvent>> medias;

        public Media media;
        
        public ElectionViewModel()
        {
            ApplicationTitle = "Election Simulator";
            DimensionX = 20;
            DimensionY = 20;
            RefreshRate = 500;
            Areas = new List<List<AbstractArea>>();
            Characters = new List<AbstractElectionCharacter>();
            Parties = new List<PoliticalParty>();
            Event = null;
            medias = new List<IObserver<ElectionEvent>>();
        }

        internal void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Stop();
        }

        internal void GenerateCharacters()
        {
            List<Street> streets = GetStreets();

            foreach (PoliticalParty party in Parties)
            {
                HQ hq = FindHQ(party);
                
                // Add 4 activists for each Party
                for(int i = 0; i < 4; i++)
                {
                    Activist activist = (Activist)factory.CreateActivist(hq.Position, party);
                    Characters.Add(activist);
                    hq.AddCharacter(activist);
                }

                // Add one Leader for each Party
                Characters.Add(factory.CreateLeader(hq.Position, party));

                // Add one Journalist for each Party
                Journalist journalist = (Journalist)factory.CreateJournalist(GetRandomStreetPosition(streets));
                Characters.Add(journalist);
            }

            media = Media.GetInstance(Characters);
            Attach(media);
        }

        private Position GetRandomStreetPosition(List<Street> streets)
        {
            return streets[TextureLoader.random.Next(streets.Count)].Position;
        }

        private HQ FindHQ(PoliticalParty party)
        {
            foreach (List<AbstractArea> areaList in Areas)
            {
                foreach (AbstractArea area in areaList)
                {
                    if (area is HQ hq && hq.Party == party)
                    {
                       return hq;
                    }
                }
            }

            return null;
        }

        internal void GenerateAccessAndHQs()
        {
            GenerateHQs();
            GeneratePublicPlaces();
            GenerateAccess();
        }

        private void GenerateHQs()
        {
            List<Building> buildings = GetBuildings();

            foreach (PoliticalParty party in Parties)
            {
                Position HQPosition = buildings[TextureLoader.random.Next(buildings.Count)].Position;
                HQ hq = (HQ) factory.CreateHQ(HQPosition, party);

                // Generate 10 posters in the HQ
                for(int i=0; i<10; i++)
                {
                    hq.AddObject(factory.CreatePoster(HQPosition, party));
                }

                Areas[HQPosition.Y][HQPosition.X] = hq;
                party.HQ = hq;
            }
        }

        private void GeneratePublicPlaces()
        {
            List<Building> buildings = GetBuildings();

            int numberPublicPlaces = buildings.Count / 20;

            for(int i = 0; i < numberPublicPlaces; i++)
            {
                Position publicPlacePosition = buildings[TextureLoader.random.Next(buildings.Count)].Position;
                PublicPlace publicPlace = (PublicPlace)factory.CreatePublicPlace(new Opinion(Parties), publicPlacePosition);
                Areas[publicPlacePosition.Y][publicPlacePosition.X] = publicPlace;

                List<Building> neighbours = GetCloseBuilding(publicPlace);
                foreach(Building building in neighbours)
                {
                    publicPlace.Attach(building);
                }
            }
        }

        private List<Building> GetCloseBuilding(PublicPlace publicPlace)
        {
            List<Building> result = new List<Building>();
            int x = publicPlace.Position.X;
            int y = publicPlace.Position.Y;
            
            // Area up
            if (y > 0 && Areas[y - 1][x] is Building)
            {
                result.Add((Building)Areas[y - 1][x]);
            }

            // Area down
            if (y + 1 < Areas.Count && Areas[y + 1][x] is Building)
            {
                result.Add((Building)Areas[y + 1][x]);
            }

            if (x > 0)
            {
                // Area left up
                if (y > 0 && Areas[y - 1][x - 1] is Building)
                {
                    result.Add((Building) Areas[y - 1][x - 1]);
                }

                // Area left
                if (Areas[y][x - 1] is Building)
                {
                    result.Add((Building)Areas[y][x - 1]);
                }

                // Area left down
                if (y + 1 < Areas.Count && Areas[y + 1][x - 1] is Building)
                {
                    result.Add((Building)Areas[y + 1][x - 1]);
                }
            }

            if (x + 1 < Areas[y].Count)
            {
                // Area right up
                if (y > 0 && Areas[y - 1][x + 1] is Building)
                {
                    result.Add((Building)Areas[y - 1][x + 1]);
                }

                // Area right
                if (Areas[y][x + 1] is Building)
                {
                    result.Add((Building)Areas[y][x + 1]);
                }

                // Area right down
                if (y + 1 < Areas.Count && Areas[y + 1][x + 1] is Building)
                {
                    result.Add((Building)Areas[y + 1][x + 1]);
                }
            }

            return result;
        }

        internal void GenerateAccess()
        {
            for (int y = 0; y < Areas.Count; y++)
            {
                for (int x = 0; x < Areas[y].Count; x++)
                {
                    AbstractArea current = Areas[y][x];
                    ElectionAccess access;

                    // Check adding access to left
                    if (x > 0)
                    {
                        access = CheckAccess(current, Areas[y][x - 1]);
                        if (access != null)
                        {
                            current.AddAccess(access);
                        }
                    }

                    // Check adding access to right
                    if (x + 1 < Areas[y].Count)
                    {
                        access = CheckAccess(current, Areas[y][x + 1]);
                        if (access != null)
                        {
                            current.AddAccess(access);
                        }
                    }

                    // Check adding access to top
                    if (y > 0)
                    {
                        access = CheckAccess(current, Areas[y - 1][x]);
                        if (access != null)
                        {
                            current.AddAccess(access);
                        }
                    }

                    // Check adding access to bottom
                    if (y + 1 < Areas.Count)
                    {
                        access = CheckAccess(current, Areas[y + 1][x]);
                        if (access != null)
                        {
                            current.AddAccess(access);
                        }
                    }
                }
            }
        }

        internal ElectionAccess CheckAccess(AbstractArea a, AbstractArea b)
        {
            if ((a is AbstractElectionArea && b is Street) ||
                (a is Street && b is AbstractElectionArea) ||
                (a is Street && b is Street) ||
                (a is HQ && b is Street) ||
                (a is Street && b is HQ))
            {
                return new ElectionAccess(a, b);
            }
            return null;
        }

        internal void NextTurn()
        {
            Status = "Simulation en cours ! Tour "+NumberTurn;
            NumberTurn++;
            GenerateEvents();

            // Generate Posters in each HQ
            GenerateNewPosters();

            foreach (AbstractElectionCharacter character in Characters)
            {
                AbstractArea currentArea = Areas[character.Position.Y][character.Position.X];
                character.NextTurn(currentArea, Areas);
                AbstractArea newArea = Areas[character.Position.Y][character.Position.X];

                currentArea.RemoveCharacter(character);
                newArea.AddCharacter(character);
            }
        }

        private void GenerateNewPosters()
        {
            foreach(PoliticalParty party in Parties)
            {
                int pickedNumber = TextureLoader.random.Next(3);
                if(pickedNumber == 0)
                {
                    HQ hq = party.HQ;
                    hq.AddObject(factory.CreatePoster(hq.Position, party));
                }
            }
        }

        private List<Building> GetBuildings()
        {
            List<Building> buildings = new List<Building>();
            foreach (List<AbstractArea> areaList in Areas)
            {
                foreach (AbstractArea area in areaList)
                {
                    if (area is Building)
                        buildings.Add((Building)area);
                }
            }

            return buildings;
        }

        private List<Street> GetStreets()
        {
            List<Street> streets = new List<Street>();
            foreach (List<AbstractArea> areaList in Areas)
            {
                foreach (AbstractArea area in areaList)
                {
                    if (area is Street)
                        streets.Add((Street)area);
                }
            }

            return streets;
        }
       

        private void GenerateEvents()
        {
            if(NumberTurn == 1000)
            {
                Stop();
                List<Building> buildings = GetBuildings();
                List<Opinion> opinions = GetOpinions(buildings);
                Poll poll = new Poll(Poll.PollType.End);
                poll.GenerateResult(opinions);
                Event = poll;
            }

            int isThereANewPoll = TextureLoader.random.Next(0, 100);
            if(isThereANewPoll == 1)
            {
                List<Building> buildings = GetPollBuildings();
                List<Opinion> opinions = GetOpinions(buildings);
                Poll poll = new Poll(Poll.PollType.Poll);
                poll.GenerateResult(opinions);
                Event = poll;
            }

            // Notify Media
            Notify(Event);
        }

        private List<Building> GetPollBuildings()
        {
            List<Building> buildings = new List<Building>();
            foreach (List<AbstractArea> areaList in Areas)
            {
                foreach (AbstractArea area in areaList)
                {
                    if (area is Building)
                        buildings.Add((Building)area);
                }
            }

            List<Building> selectedBuildings = new List<Building>();
            for(int i = 0; i < buildings.Count / 20; i++)
            {
                int pickedBuilding = TextureLoader.random.Next(buildings.Count);
                selectedBuildings.Add(buildings[pickedBuilding]);
                buildings.RemoveAt(pickedBuilding);
            }

            return selectedBuildings;
        }

        private List<Opinion> GetOpinions(List<Building> buildings)
        {
            List<Opinion> opinions = new List<Opinion>();
            foreach(Building building in buildings)
            {
                opinions.Add(building.opinion);
            }
            return opinions;
        }

        internal void Play()
        {
            Running = true;
            Status = "Simulation en cours ! Tour " + NumberTurn;
            while (Running)
            {
                Thread.Sleep(RefreshRate);
                NextTurn();
            }
        }



        internal void Stop()
        {
            Running = false;
            Status = "Simulation en pause ... Tour " + NumberTurn;
        }

        internal void AddStreet(int i)
        {
            Areas[i].Add(factory.CreateStreet(new Position(Areas[i].Count, i)));
        }

        internal void AddBuilding(int i)
        {
            Areas[i].Add(factory.CreateBuilding(new Opinion(Parties), new Position(Areas[i].Count, i)));
        }

        internal void AddEmpty(int i)
        {
            Areas[i].Add(factory.CreateEmptyArea(new Position(Areas[i].Count, i)));
        }

        public void Attach(IObserver<ElectionEvent> observer)
        {
            medias.Add(observer);
        }

        public void Detach(IObserver<ElectionEvent> observer)
        {
            medias.Remove(observer);
        }

        public void Notify(ElectionEvent electionEvent)
        {
            foreach(IObserver<ElectionEvent> observer in medias)
            {
                observer.Update(electionEvent);
            }
        }
    }
}
