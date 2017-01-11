﻿using InternalLogic;
using InternalLogicWithFileIO;
using NUnit.Framework;
using OldInternalLogic;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class AnalysisEngineTest
    {
        [Test]
        public void TestParsimony()
        {
            // creates some test proteins and digests them (simulating a protein database)
            string sequence1 = "AKCKBK";
            string sequence2 = "DKCK";
            string sequence3 = "AAAAK";

            IEnumerable<string> sequencesInducingCleavage = new List<string>() { "K", "R" };
            IEnumerable<string> sequencesPreventingCleavage = new List<string>() { "KP", "RP" };
            Dictionary<int, List<MorpheusModification>> temp1 = new Dictionary<int, List<MorpheusModification>>();
            List<MorpheusModification> temp2 = new List<MorpheusModification>();
            int[] temp3 = new int[0];
            Protease protease = new Protease("Trypsin", sequencesInducingCleavage, sequencesPreventingCleavage, OldInternalLogic.Terminus.C, CleavageSpecificity.Full, null, null, null);
            HashSet<PeptideWithSetModifications> totalProteinList = new HashSet<PeptideWithSetModifications>();

            Protein p1 = new Protein(sequence1, "1", null, temp1, temp3, temp3, null, "Test1", "TestFullName1", 0, false);
            Protein p2 = new Protein(sequence2, "2", null, temp1, temp3, temp3, null, "Test2", "TestFullName2", 0, false);
            Protein p3 = new Protein(sequence3, "3", null, temp1, temp3, temp3, null, "Test3", "TestFullName3", 0, false);

            IEnumerable<PeptideWithPossibleModifications> digestedList1 = p1.Digest(protease, 2, InitiatorMethionineBehavior.Variable);
            IEnumerable<PeptideWithPossibleModifications> digestedList2 = p2.Digest(protease, 2, InitiatorMethionineBehavior.Variable);
            IEnumerable<PeptideWithPossibleModifications> digestedList3 = p3.Digest(protease, 2, InitiatorMethionineBehavior.Variable);

            foreach (var protein in digestedList1)
            {
                IEnumerable<PeptideWithSetModifications> peptides1 = protein.GetPeptideWithSetModifications(temp2, 4098, 3, temp2);

                foreach (var peptide in peptides1)
                    totalProteinList.Add(peptide);
            }

            foreach (var protein in digestedList2)
            {
                IEnumerable<PeptideWithSetModifications> peptides2 = protein.GetPeptideWithSetModifications(temp2, 4098, 3, temp2);

                foreach (var peptide in peptides2)
                    totalProteinList.Add(peptide);
            }

            foreach (var protein in digestedList3)
            {
                IEnumerable<PeptideWithSetModifications> peptides3 = protein.GetPeptideWithSetModifications(temp2, 4098, 3, temp2);

                foreach (var peptide in peptides3)
                    totalProteinList.Add(peptide);
            }

            // creates the initial dictionary of "peptide" and "protein" matches (protein must contain peptide sequence)
            Dictionary<CompactPeptide, HashSet<PeptideWithSetModifications>> initialDictionary = new Dictionary<CompactPeptide, HashSet<PeptideWithSetModifications>>();
            CompactPeptide[] peptides = new CompactPeptide[totalProteinList.Count()];
            HashSet<PeptideWithSetModifications>[] proteinSets = new HashSet<PeptideWithSetModifications>[totalProteinList.Count()];

            // creates peptide list
            for (int i = 0; i < totalProteinList.Count(); i++)
            {
                peptides[i] = new CompactPeptide(totalProteinList.ElementAt(i), temp2, temp2);
            }

            // creates protein list
            for (int i = 0; i < proteinSets.Length; i++)
            {
                proteinSets[i] = new HashSet<PeptideWithSetModifications>();

                foreach (var protein in totalProteinList)
                {
                    string peptideBaseSequence = string.Join("", peptides[i].BaseSequence.Select(b => char.ConvertFromUtf32(b)));

                    if (protein.BaseSequence.Contains(peptideBaseSequence))
                    {
                        proteinSets[i].Add(protein);
                        //proteinSets[i].Add(protein);
                    }
                }
            }

            // populates initial peptide-protein dictionary
            for (int i = 0; i < peptides.Length; i++)
            {
                if (!initialDictionary.ContainsKey(peptides[i]))
                {
                    initialDictionary.Add(peptides[i], proteinSets[i]);
                }
            }

            var p = new ParentSpectrumMatch[0][];
            var analysisEngine = new AnalysisEngine(p, null, null, null, null, null, null, null, null, null, null, null, true);

            // apply parsimony to initial dictionary
            var parsimonyTest = analysisEngine.ApplyProteinParsimony(initialDictionary);

            // apply the single pick version to parsimonious dictionary
            var singlePickTest = AnalysisEngine.GetSingleMatchDictionary(parsimonyTest);

            List<PeptideWithSetModifications> parsimonyProteinList = new List<PeptideWithSetModifications>();
            string[] parsimonyBaseSequences = new string[3];
            int j = 0;

            foreach (var kvp in parsimonyTest)
            {
                foreach (var protein in kvp.Value)
                {
                    if (!parsimonyProteinList.Contains(protein))
                    {
                        parsimonyProteinList.Add(protein);
                        parsimonyBaseSequences[j] = protein.BaseSequence;
                        j++;
                    }
                }
            }

            Assert.That(parsimonyProteinList.Count == 3);
            Assert.That(parsimonyBaseSequences.Contains(sequence1));
            Assert.That(parsimonyBaseSequences.Contains(sequence2));
            Assert.That(parsimonyBaseSequences.Contains(sequence3));

            /*
            // prints initial dictionary
            Console.WriteLine("----Initial Dictionary----");
            foreach (var kvp in initialDictionary)
            {
                Console.Write(string.Join("", kvp.Key.BaseSequence.Select(b => char.ConvertFromUtf32(b))) + "  \t\t\t  ");
                foreach (var peptide in kvp.Value)
                {
                    Console.Write(peptide.BaseSequence + " ;; ");
                }
                Console.WriteLine();
            }

            // prints parsimony dictionary
            Console.WriteLine("----Parsimonious Dictionary----");
            foreach (var kvp in parsimonyTest)
            {
                Console.Write(string.Join("", kvp.Key.BaseSequence.Select(b => char.ConvertFromUtf32(b))) + "  \t\t\t  ");
                foreach (var peptide in kvp.Value)
                {
                    Console.Write(peptide.BaseSequence + " ;; ");
                }
                Console.WriteLine();
            }
            */
        }
    }
}