﻿using ElectionLibrary.Character;
using ElectionLibrary.Parties;
using System;
using System.Collections.Generic;

namespace ElectionLibrary.Environment
{
    public class Opinion
    {
        public Dictionary<PoliticalParty, double> opinionList;

        private static readonly Random random = new Random();

        public Opinion(List<PoliticalParty> parties)
        {
            opinionList = new Dictionary<PoliticalParty, double>();
            foreach (PoliticalParty party in parties)
            {
                opinionList.Add(party, 100 / parties.Count);
            }
        }

        public Dictionary<PoliticalParty, double> GetPartiesOpinions()
        {
            return opinionList;
        }

        public List<PoliticalParty> GetParties()
        {
            List<PoliticalParty> parties = new List<PoliticalParty>();

            foreach (PoliticalParty party in opinionList.Keys)
            {
                parties.Add(party);
            }

            return parties;
        }

        public double InfluenceOpinion(PoliticalParty party, int aura, int moral, int nbTurn)
        {
            if (!opinionList.ContainsKey(party))
            {
                throw new InvalidOperationException();
            }

            double calcul = 0;

            if (opinionList[party] < 100) {
                // Get the maximum percentage of opinion to add to the party : if current opinion of the building is 25%, max is 7.5%, if 50%, max is 5%, etc 
                double maxPercentage = (100 - opinionList[party]) / 10;

                // Get the percentage of moral the character has : if 20/25, then percentage is 80% (and moralPercentage is 80)
                double moralPercentage = ((double)moral / ElectionCharacter.INIT_MORAL) * 100;

                // Add a little random in the game. Pick a number betwen 0 and 100
                double pickedNumber = random.Next(0, 100);
                if (pickedNumber >= moralPercentage)
                {
                    // If the picked number is greater than the moralPercentage, the maximum percentage is multiplied by the moralPercentage divided by 100
                    // If moralPercentage is 80, then maxPercentage is multiplied by 0.8
                    calcul = maxPercentage * (moralPercentage / 100);
                }
                else
                {
                    calcul = maxPercentage;
                }

                // Finally, add the half of the aura to the maximum percentage, if it will not exceed 100%
                //double leftToReach100 = 100 - (opinionList[party] + maxPercentage);
                //calcul = maxPercentage + ((leftToReach100 < aura/2) ? leftToReach100 : aura/2);

                Console.WriteLine(maxPercentage + " : " + calcul);

                if (calcul < 0)
                {
                    throw new ArithmeticException();
                }

                UpdateNewOpinion(party, calcul);
            }
            else
            {
                Console.WriteLine(party + " => " + opinionList[party]);
            }

            return calcul;
        }

        private void UpdateNewOpinion(PoliticalParty party, double opinionToAdd)
        {
            // Compute new opinion for the party of the politician
            opinionList[party] += opinionToAdd;

            // Compute new opinion for the others parties
            List<PoliticalParty> concurrents = GetRepresentativeConcurrentsList(party);

            if(concurrents.Count == 0)
            {
                return;
            }
            
            double remainingOpinionToRemove = opinionToAdd;

            while (remainingOpinionToRemove > 0)
            {
                PoliticalParty concurrent = concurrents[random.Next(concurrents.Count)];
                double opinionToRemove = (remainingOpinionToRemove < 1) ? remainingOpinionToRemove : random.Next((int)remainingOpinionToRemove + 1);

                // Check current opinion for the concurrent. Do not remove more than it.
                if(opinionToRemove > opinionList[concurrent])
                {
                    opinionToRemove = opinionList[concurrent];
                }

                opinionList[concurrent]     -= opinionToRemove;
                remainingOpinionToRemove    -= opinionToRemove;
            }
        }

        private List<PoliticalParty> GetConcurrentParties(PoliticalParty party)
        {
            List<PoliticalParty> listConcurrent = new List<PoliticalParty>();
            foreach (KeyValuePair<PoliticalParty, double> entry in opinionList)
            {
                if (entry.Key != party)
                {
                    listConcurrent.Add(entry.Key);
                }
            }

            return listConcurrent;
        }

        private List<PoliticalParty> GetRepresentativeConcurrentsList(PoliticalParty party)
        {
            List<PoliticalParty> result = new List<PoliticalParty>();
            List<PoliticalParty> concurrents = GetConcurrentParties(party);

            foreach (PoliticalParty concurrent in concurrents)
            {
                for(int i=0; i<opinionList[concurrent]; i++)
                {
                    result.Add(concurrent);
                }
            }

            return result;
        }
    }
}