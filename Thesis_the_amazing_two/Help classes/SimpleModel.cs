using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
 

namespace Thesis.Help_classes
{
    public sealed class SimpleModel : Model
    {
        //The six possible directions.
        public readonly Coord3D[] Directions = new Coord3D[6]
           {Coord3D.Left, Coord3D.Right, Coord3D.Up, Coord3D.Down, Coord3D.Forward, Coord3D.Back};

        public SimpleModel(InputModel inputModel, int patternSize, Coord3D outputSize, bool periodic, bool addNeighbours, bool probabilisticModel)
        {
            NeighboursMap = new Dictionary<int, Dictionary<Coord3D, List<int>>>();
            //defines if the input voxel model is periodic or not (i.e. if it can be "looped over" or not)
            Periodic = periodic;
           
            //defines if the code should use the pattern distribution in the input voxel model to create the output model
            //If it is set to false, the pattern is randomly chosen.
            ProbabilisticModel = probabilisticModel;
            PatternSize = patternSize;
           
            //the number of times we "observe"
            NumGen = 0;

            //the x,y,z dimensions of the output model
            OutputSize = outputSize;

            Init(inputModel, patternSize, periodic);

            //NeigboursMap is a data structure that stores the allowed neighboring patterns for every pattern and for every direction
            InitNeighboursMap();

            if (addNeighbours)
            {
 
                //  causing problems, commented it out!!!!!!!!!!!!!!!!!!!!!!!!
               // DetectNeighbours();

            }
            //foreach (KeyValuePair<int, Dictionary<Coord3D, List<int>>> kvp in NeighboursMap)
            //{
            //    foreach (KeyValuePair<Coord3D, List<int>> hhh in kvp.Value)
            //        Rhino.RhinoApp.WriteLine("Key = {0}, keyin = {1},valuein ={2}", kvp.Key, hhh.Key.X.ToString() + "|"
            //            + hhh.Key.Y.ToString() + "|" + hhh.Key.Z.ToString(), String.Join(", ", hhh.Value));

            //}

            InitOutputMatrix(outputSize);

        }
        //public SimpleModel(InputModel inputModel, int patternSize, Coord3D outputSize, bool periodic, bool addNeighbours, bool probabilisticModel,bool Optimizedmodel, bool Maximizemodel, bool Minimizemodel)
        //{
        //    NeighboursMap = new Dictionary<int, Dictionary<Coord3D, List<int>>>();
        //    //defines if the input voxel model is periodic or not (i.e. if it can be "looped over" or not)
        //    Periodic = periodic;

        //    //defines if the code should use the pattern distribution in the input voxel model to create the output model
        //    //If it is set to false, the pattern is randomly chosen.
        //    ProbabilisticModel = probabilisticModel;
        //    OptimizedModel= Optimizedmodel;
        //    Maximize = Maximizemodel;
        //    Minimize = Minimizemodel;
        //    PatternSize = patternSize;

        //    //the number of times we "observe"
        //    NumGen = 0;

        //    //the x,y,z dimensions of the output model
        //    OutputSize = outputSize;

        //    Init(inputModel, patternSize, periodic);

        //    //NeigboursMap is a data structure that stores the allowed neighboring patterns for every pattern and for every direction
        //    InitNeighboursMap();

        //    if (addNeighbours)
        //    {

        //        //  causing problems, commented it out!!!!!!!!!!!!!!!!!!!!!!!!
        //        // DetectNeighbours();

        //    }
        //    //foreach (KeyValuePair<int, Dictionary<Coord3D, List<int>>> kvp in NeighboursMap)
        //    //{
        //    //    foreach (KeyValuePair<Coord3D, List<int>> hhh in kvp.Value)
        //    //        Rhino.RhinoApp.WriteLine("Key = {0}, keyin = {1},valuein ={2}", kvp.Key, hhh.Key.X.ToString() + "|"
        //    //            + hhh.Key.Y.ToString() + "|" + hhh.Key.Z.ToString(), String.Join(", ", hhh.Value));

        //    //}

        //    InitOutputMatrix(outputSize);

        //}
        
        private static int[,,] CreateEmptyPattern(int pSize)
        {
            var res = new int[pSize, pSize, pSize];

            for (var i = 0; i < res.GetLength(0); i++)
            {
                for (var j = 0; j < res.GetLength(1); j++)
                {
                    for (var k = 0; k < res.GetLength(2); k++)
                    {
                        res[i, j, k] = 0;
                    }
                }
            }
            return res;
        }

        protected override void Init(InputModel inputModel, int patternSize, bool periodic)
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

            //the matrixwith the values each voxel carries and we wont to optimize
            inputModel.Voxels.ForEach(voxel => OptimizeMatrix[voxel.X, voxel.Y, voxel.Z] = voxel.Value);

            //////Add "empty space" pattern.
            //patterns.Add(CreateEmptyPattern(patternSize));
            //probabilites[0] = 0;

            for (var x = 0; x < patternMatrix.GetLength(0); x++)
            {
                for (var y = 0; y < patternMatrix.GetLength(1); y++)
                {
                    for (var z = 0; z < patternMatrix.GetLength(2); z++)
                    {
                        var currentPattern = GetCurrentPattern(inputMatrix, x * patternSize,y *patternSize, z * patternSize, patternSize);

                        //the values fro each pattern that we want to optimize
                       // var PatternValue = GetCurrentPatternValue(OptimizeMatrix, x * patternSize, y * patternSize, z * patternSize, patternSize);

                        var index = patterns.ContainsPattern(currentPattern);
                        if (index < 0)
                        {
                            patterns.Add(currentPattern);
                           // OptimizableVals.Add(PatternValue);
                            
                            patternMatrix[x, y, z] = patterns.Count - 1;
                            probabilites[patterns.Count - 1] = (double)1 / patternMatrix.Length;
                        }
                        else
                        {
                            patternMatrix[x, y, z] = index;
                            probabilites[index] += (double)1 / patternMatrix.Length;
                        }
                    }
                }
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


        //This method was allowing  incorect adjacencies
        
            //private void DetectNeighbours()
        //{

        //    foreach (var pattern in patternMatrix)
        //    {
        //        foreach (var otherPattern in patternMatrix)
        //        {
        //            CheckAddNeighbour(pattern, otherPattern);

        //        }
        //    }

        //}

        //private void CheckAddNeighbour(int pattern, int otherPattern)
        //{
        //    var patternStruct = patterns[pattern];
        //    var otherPatternStruct = patterns[otherPattern];

        //    if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Left) && !NeighboursMap[pattern][Coord3D.Left].Contains(otherPattern))
        //        NeighboursMap[pattern][Coord3D.Left].Add(otherPattern);
        //    if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Right) && !NeighboursMap[pattern][Coord3D.Right].Contains(otherPattern))
        //        NeighboursMap[pattern][Coord3D.Right].Add(otherPattern);

        //    if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Down) && !NeighboursMap[pattern][Coord3D.Down].Contains(otherPattern))
        //        NeighboursMap[pattern][Coord3D.Down].Add(otherPattern);
        //    if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Up) && !NeighboursMap[pattern][Coord3D.Up].Contains(otherPattern))
        //        NeighboursMap[pattern][Coord3D.Up].Add(otherPattern);

        //    if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Back) && !NeighboursMap[pattern][Coord3D.Back].Contains(otherPattern))
        //        NeighboursMap[pattern][Coord3D.Back].Add(otherPattern);
        //    if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Forward) && !NeighboursMap[pattern][Coord3D.Forward].Contains(otherPattern))
        //        NeighboursMap[pattern][Coord3D.Forward].Add(otherPattern);

        //}


        public override void Observe()
        {
            //in the begining all nodes are collapsable and all patterns all possible for all nodes
            var collapsableNodes = GetCollapsableNodes();

           //This method collapses the first node to the first pattern and can be used in floorplans when you
           //want to ensure the floor plan will be closed with surrounding walls
           
            if ((NumGen == 0) && (ModelSynthesis.FixedCorners ==true))
            {
                
                var nodeCoordsfirst = collapsableNodes[0];
                var availableNodeStatesfirst = outputMatrix[nodeCoordsfirst.X, nodeCoordsfirst.Y, nodeCoordsfirst.Z].ToList();
                outputMatrix.SetValue(new List<int>() { availableNodeStatesfirst[1] }, nodeCoordsfirst.X, nodeCoordsfirst.Y, nodeCoordsfirst.Z);
                Propagate(nodeCoordsfirst);

                //We can also collapse the top right corner-last node to the last pattern to make sure it is also a corner
                //var nodeCoordslast = collapsableNodes.Last();
                //var availableNodeStateslast = outputMatrix[nodeCoordslast.X, nodeCoordslast.Y, nodeCoordslast.Z].ToList();
                //outputMatrix.SetValue(new List<int>() { availableNodeStateslast.Last()}, nodeCoordslast.X, nodeCoordslast.Y, nodeCoordslast.Z);
                //Propagate(nodeCoordslast);
            }

            //Pick a random node from the collapsible nodes.
            if (collapsableNodes.Count == 0)
            {
                Rhino.RhinoApp.WriteLine("contradiction true: from Observe");
                Contradiction = true;
                return;
            }
            var nodeCoords = collapsableNodes[Rnd.Next(collapsableNodes.Count)];
            //var availableNodeStates = outputMatrix[nodeCoords.X, nodeCoords.Y, nodeCoords.Z].Except(new[] { 0 }).ToList();
            var availableNodeStates = outputMatrix[nodeCoords.X, nodeCoords.Y, nodeCoords.Z].ToList();
          


            if (ProbabilisticModel)
            {
             
                //Eliminate all duplicates from the list of possible states.
                //availableNodeStates = availableNodeStates.Distinct().ToList().Shuffle().ToList();
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
            }
            else
            {            
                outputMatrix.SetValue(new List<int>() { availableNodeStates[Rnd.Next(availableNodeStates.Count)] }, nodeCoords.X, nodeCoords.Y, nodeCoords.Z);                   
            }
     
            Propagate(nodeCoords);

            NumGen++;
        }
        //public void ObserveAndOptimize()
        //{
        //    //in the begining all nodes are collapsable and all patterns all possible for all nodes
        //    var collapsableNodes = GetCollapsableNodes();
            
        //    int picked = 0;

        //    //if the output matrix is not instantiated and there are no collapsable nodes throw an error
        //    if (collapsableNodes.Count == 0)
        //    {
        //        Rhino.RhinoApp.WriteLine("contradiction true: from Observe");
        //        Contradiction = true;
        //        return;
        //    }
        //    //we pick a random node to collapse
        //    var nodeCoords = collapsableNodes[Rnd.Next(collapsableNodes.Count)];
        //   // var availableNodeStates = outputMatrix[nodeCoords.X, nodeCoords.Y, nodeCoords.Z].Except(new[] { 0 }).ToList();
        //    var availableNodeStates = outputMatrix[nodeCoords.X, nodeCoords.Y, nodeCoords.Z].ToList();



        //    if (ProbabilisticModel)
        //    {

        //        //Eliminate all duplicates from the list of possible states.
        //        //availableNodeStates = availableNodeStates.Distinct().ToList().Shuffle().ToList();
        //        availableNodeStates = availableNodeStates.ToList().Shuffle().ToList();
                

        //        //Choose a state according to the probability distribution of the states in the input model.
        //        double runningTotal = 0;
        //        var totalProb = probabilites.Select(x => x)
        //            .Where(x => availableNodeStates.Contains(x.Key))
        //            .Sum(x => x.Value);
        //        var rndNumb = Rnd.NextDouble() * totalProb;


        //        foreach (var availableNodeState in availableNodeStates)
        //        {
        //            runningTotal += probabilites[availableNodeState];

        //            if (runningTotal > rndNumb)
        //            {
        //                outputMatrix.SetValue(new List<int>() { availableNodeState }, nodeCoords.X, nodeCoords.Y,
        //               nodeCoords.Z);
        //                break;
        //            }
        //        }
        //    }
        //    else if (OptimizedModel)
        //    {
        //        var values_to_optimize = new Dictionary<int, double>();
                
               
        //        foreach (int state in availableNodeStates)
        //        {
        //            //The dictionary has the patterns and the values assosiated with them

        //            values_to_optimize.Add(state, OptimizableVals[state]);
                   
        //        }
        //        //pick the one with the minimum value
        //        if (Maximize)
        //        {
        //            //picked is the pattern that has the maximum value and we pick it when we want a maximum sum
        //            picked = values_to_optimize.Min().Key;
        //        }
        //        //pick the one with the maximum value
        //        if (Minimize)
        //        {
        //            //picked is the pattern that has the minimum value and we pick it when we want a maximum sum
        //            picked = values_to_optimize.Max().Key;
        //        }

        //        outputMatrix.SetValue(new List<int>() { availableNodeStates[picked] }, nodeCoords.X, nodeCoords.Y,
        //                              nodeCoords.Z);
               
        //    }
        //    else
        //    {
        //        outputMatrix.SetValue(new List<int>() { availableNodeStates[Rnd.Next(availableNodeStates.Count)] }, nodeCoords.X, nodeCoords.Y, nodeCoords.Z);
        //    }

        //    Propagate(nodeCoords);

        //    NumGen++;
        //}

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
                        if(!Periodic) continue;
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
                        var currentPattern = patterns[outputMatrix[x, y, z].Count>0? outputMatrix[x, y, z].First() : 0];
                       
                        //var currentPattern = patterns[outputMatrix[x, y, z].First()];
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
