﻿using System;
using System.Collections.Generic;
using System.Linq;
 using Thesis.Help_classes;

namespace Thesis.help_classes_hp
{
    public sealed class SimpleModelhp : Modelhp
    {
        //The six possible directions.
        public readonly Coord3D[] Directions = new Coord3D[6]
           {Coord3D.Left, Coord3D.Right, Coord3D.Up, Coord3D.Down, Coord3D.Forward, Coord3D.Back};


        public SimpleModelhp(InputModelhp inputModel, int patternSize, Coord3D outputSize, bool periodic, bool addNeighbours, bool probabilisticModel, List<double> inputPercentages, bool buildingProgram)
        {
            NeighboursMap = new Dictionary<int, Dictionary<Coord3D, List<int>>>();
            //defines if the input voxel model is periodic or not (i.e. if it can be "looped over" or not)
            Periodic = periodic;

            //defines if the code should use the pattern distribution in the input voxel model to create the output model
            //If it is set to false, the pattern is randomly chosen.
            ProbabilisticModel = probabilisticModel;
            InputPercentages = inputPercentages;
            PatternSize = patternSize;
            BuildingProgram = buildingProgram;

            //the number of times we "observe"
            NumGen = 0;

            //the x,y,z dimensions of the output model
            OutputSize = outputSize;

            Initialize(inputModel, patternSize, periodic, buildingProgram);

            //NeigboursMap is a data structure that stores the allowed neighboring patterns for every pattern and for every direction
            InitNeighboursMap();



            InitOutputMatrix(outputSize);

        }
 

        protected override void Initialize(InputModelhp inputModel, int patternSize, bool periodic, bool buildingProgram)
        {

            var inputMatrix = new int[inputModel.Size.X, inputModel.Size.Y, inputModel.Size.Z];
            var OptimizeMatrix = new double[inputModel.Size.X, inputModel.Size.Y, inputModel.Size.Z];
            patterns = new List<int[,,]>();

            patternMatrix = new int[(int)Math.Ceiling((double)(inputModel.Size.X / patternSize)),
                (int)Math.Ceiling((double)(inputModel.Size.Y / patternSize)),
                (int)Math.Ceiling((double)(inputModel.Size.Z / patternSize))];
            probabilites = new Dictionary<int, double>();

            //the matrix with the voxels identities (who am I as a tile?)
            inputModel.Voxels.ForEach(voxel => inputMatrix[voxel.X, voxel.Y, voxel.Z] = voxel.Identity);
            for (var x = 0; x < patternMatrix.GetLength(0); x++)
            {
                for (var y = 0; y < patternMatrix.GetLength(1); y++)
                {
                    for (var z = 0; z < patternMatrix.GetLength(2); z++)
                    {
                        var currentPattern = GetCurrentPattern(inputMatrix, x * patternSize, y * patternSize, z * patternSize, patternSize);


                        var index = patterns.ContainsPattern(currentPattern);
                        if (index < 0)
                        {
                            patterns.Add(currentPattern);



                            patternMatrix[x, y, z] = patterns.Count - 1;
                            if (ProbabilisticModel)
                                probabilites[patterns.Count - 1] = (double)1 / patternMatrix.Length;

                        }
                        else
                        {
                            patternMatrix[x, y, z] = index;
                            if (ProbabilisticModel)
                                probabilites[index] += (double)1 / patternMatrix.Length;
                        }
                    }
                }
            }



            if (buildingProgram)
                for (int i = 0; i < patterns.Count; i++)
                {
                    probabilites.Add(i, InputPercentages[i]);
                }


        }

        private void InitNeighboursMap()
        {
            //Init the data structure.
            for (var i = 0; i < patterns.Count; i++)
            {
                NeighboursMap[i] = new Dictionary<Coord3D, List<int>>();

                foreach (var direction in Directions)
                {
                    NeighboursMap[i][direction] = new List<int>();
                }
            }

            //Populate the data structure.
            for (var x = 0; x < patternMatrix.GetLength(0); x++)
            {
                for (var y = 0; y < patternMatrix.GetLength(1); y++)
                {
                    for (var z = 0; z < patternMatrix.GetLength(2); z++)
                    {
                        var currentPattern = patternMatrix[x, y, z];

                        if (x - 1 >= 0)
                        {
                            NeighboursMap[currentPattern][Coord3D.Left].Add(patternMatrix[x - 1, y, z]);
                        }
                        else
                        {
                            if (Periodic)
                            {
                                NeighboursMap[currentPattern][Coord3D.Left]
                                    .Add(patternMatrix[patternMatrix.GetLength(0) - 1, y, z]);
                            }
                        }
                        if (x + 1 < patternMatrix.GetLength(0))
                        {
                            NeighboursMap[currentPattern][Coord3D.Right].Add(patternMatrix[x + 1, y, z]);
                        }
                        else
                        {
                            if (Periodic)
                            {
                                NeighboursMap[currentPattern][Coord3D.Right].Add(patternMatrix[0, y, z]);
                            }
                        }

                        if (y - 1 >= 0)
                        {
                            NeighboursMap[currentPattern][Coord3D.Down].Add(patternMatrix[x, y - 1, z]);
                        }
                        else
                        {
                            if (Periodic)
                            {
                                NeighboursMap[currentPattern][Coord3D.Down]
                                    .Add(patternMatrix[x, patternMatrix.GetLength(1) - 1, z]);
                            }
                        }
                        if (y + 1 < patternMatrix.GetLength(1))
                        {
                            NeighboursMap[currentPattern][Coord3D.Up].Add(patternMatrix[x, y + 1, z]);
                        }
                        else
                        {
                            if (Periodic)
                            {
                                NeighboursMap[currentPattern][Coord3D.Up].Add(patternMatrix[x, 0, z]);
                            }
                        }

                        if (z - 1 >= 0)
                        {
                            NeighboursMap[currentPattern][Coord3D.Back].Add(patternMatrix[x, y, z - 1]);
                        }
                        else
                        {
                            if (Periodic)
                            {
                                NeighboursMap[currentPattern][Coord3D.Back]
                                    .Add(patternMatrix[x, y, patternMatrix.GetLength(2) - 1]);
                            }
                        }
                        if (z + 1 < patternMatrix.GetLength(2))
                        {
                            NeighboursMap[currentPattern][Coord3D.Forward].Add(patternMatrix[x, y, z + 1]);
                        }
                        else
                        {
                            if (Periodic)
                            {
                                NeighboursMap[currentPattern][Coord3D.Forward].Add(patternMatrix[x, y, 0]);
                            }
                        }
                    }
                }
            }


            //Eliminate duplicates in the neighbours map.
            for (var i = 0; i < patterns.Count; i++)
            {
                foreach (var direction in Directions)
                {
                    NeighboursMap[i][direction] = NeighboursMap[i][direction].Distinct().ToList();
                }
            }

            //// Add the empty space in case a pattern has no neighbour.  
            for (var i = 0; i < patterns.Count; i++)
            {
                foreach (var direction in Directions)
                {
                    if (NeighboursMap[i][direction].Count == 0)
                        NeighboursMap[i][direction].Add(0);
                }
            }

            //foreach (KeyValuePair<int, Dictionary<Coord3D, List<int>>> kvp in NeighboursMap)
            //{
            //    foreach (KeyValuePair<Coord3D, List<int>> hhh in kvp.Value)
            //        Debug.LogFormat("Key = {0}, keyin = {1},valuein ={2}", kvp.Key, hhh.Key.X.ToString() + "|"
            //            + hhh.Key.Y.ToString() + "|" + hhh.Key.Z.ToString(), String.Join(", ", hhh.Value));
            //}

        }




        public override void ObserveAndProgram()
        {
            //in the begining all nodes are collapsable and all patterns all possible for all nodes
            var collapsableNodes = GetCollapsableNodes();

            //if the output matrix is not instantiated and there are no collapsable nodes throw an error
            if (collapsableNodes.Count == 0)
            {
                Contradiction = true;
                return;
            }

            //Pick a random node to collapse
            var nodeCoords = collapsableNodes[Rnd.Next(collapsableNodes.Count)];
            var availableNodeStates = outputMatrix[nodeCoords.X, nodeCoords.Y, nodeCoords.Z].ToList();
            availableNodeStates = availableNodeStates.ToList().Shuffle().ToList();

            //Choose a state according to the probability distribution of the states in the input model.
            double runningTotal = 0;
            var totalProb = probabilites.Select(x => x)
           .Where(x => availableNodeStates.Contains(x.Key))
           .Sum(x => x.Value);
            var rndNumb = Rnd.NextDouble() * totalProb;

            foreach (var availableNodeState in availableNodeStates)
            {
                runningTotal += probabilites[availableNodeState];

                if (runningTotal > rndNumb)
                {
                    outputMatrix.SetValue(new List<int>() { availableNodeState }, nodeCoords.X, nodeCoords.Y,
                   nodeCoords.Z);
                    break;
                }
            }

            Propagate(nodeCoords);

            NumGen++;
        }

        protected override void Propagate(Coord3D startPoint)
        {

            //Queue the first element.
            var nodesToVisit = new Queue<Coord3D>();
            nodesToVisit.Enqueue(startPoint);

            //Perform a Breadth-First grid traversal.
            while (nodesToVisit.Any())
            {
                var current = nodesToVisit.Dequeue();

                //Get the list of the allowed neighbours of the current node
                var nghbrsMaps = outputMatrix[current.X, current.Y, current.Z].Select(possibleElement => NeighboursMap[possibleElement]).ToList();


                var allowedNghbrs = nghbrsMaps.SelectMany(dict => dict)
                    .ToLookup(pair => pair.Key, pair => pair.Value)
                    .ToDictionary(group => group.Key, group => group.SelectMany(list => list).ToList());


                //For every possible direction check if the node has already been affected by the propagation.
                //If it hasn't queue it up and mark it as visited, otherwise move on.
                foreach (var direction in Directions)
                {
                    var nodeToBeChanged = current.Add(direction.X, direction.Y, direction.Z);


                    if (outputMatrix.OutOfBounds(nodeToBeChanged))
                    {
                        if (!Periodic) continue;
                        else
                        {
                            nodeToBeChanged = new Coord3D(Mod(nodeToBeChanged.X, outputMatrix.GetLength(0)),
                            Mod(nodeToBeChanged.Y, outputMatrix.GetLength(1)),
                            Mod(nodeToBeChanged.Z, outputMatrix.GetLength(2)));
                        }
                    }


                    //Count the states before the propagation.
                    var statesBefore = outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z].Count;

                    //Eliminate neighbours that are not allowed from the output matrix
                    var allowedNghbrsInDirection = allowedNghbrs[direction].Distinct().ToList();
                    outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z]
                        .RemoveAll(neighbour => !allowedNghbrsInDirection.Contains(neighbour));

                    //Count the states after, if nbBefore != nbAfter queue it up.
                    var statesAfter = outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z].Count;

                    if (outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z].Count == 0)
                    {

                        Rhino.RhinoApp.WriteLine("contradiction true: from Propagate");

                        Contradiction = true;
                        return;

                    }

                    //Queue it up in order to spread the info to its neighbours and mark it as visited.
                    if (statesBefore != statesAfter)
                    {
                        if (!nodesToVisit.Contains(nodeToBeChanged))
                        {
                            nodesToVisit.Enqueue(nodeToBeChanged);
                        }
                    }
                }
            }

            GenerationFinished = CheckIfFinished();
        }

        public override int[,,] GetOutput()
        {
            var res = new int[outputMatrix.GetLength(0) * PatternSize, outputMatrix.GetLength(1) * PatternSize, outputMatrix.GetLength(2) * PatternSize];
            for (var x = 0; x < outputMatrix.GetLength(0); x++)
            {
                for (var y = 0; y < outputMatrix.GetLength(1); y++)
                {
                    for (var z = 0; z < outputMatrix.GetLength(2); z++)
                    {

                        // check if patterns assigned, if not return pattern zero (make sure that pattern zero is empty patter)
                        //var currentPattern = patterns[outputMatrix[x, y, z].Count>0? outputMatrix[x, y, z].First() : 0];

                        var currentPattern = patterns[outputMatrix[x, y, z].First()];
                        for (var i = 0; i < currentPattern.GetLength(0); i++)
                        {
                            for (var j = 0; j < currentPattern.GetLength(1); j++)
                            {
                                for (var k = 0; k < currentPattern.GetLength(2); k++)
                                {
                                    res[(x * currentPattern.GetLength(0)) + i, (y * currentPattern.GetLength(1)) + j,
                                        (z * currentPattern.GetLength(2)) + k] = currentPattern[i, j, k];
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
    }
}
