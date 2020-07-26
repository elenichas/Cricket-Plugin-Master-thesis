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

            //Create a data structure for every pattern in patterns,for every direction(6 possible)-----> a list of possible patterns
            //(int = pattern) --->(coord(ex. 100))--->List<int> patterns
            //for every pattern for every coord(left,right,up...) what are the possible patterns
            InitNeighboursMap();

            if (addNeighbours)
            {
                //this method calls the CheckAddNeighbors method for 
                //every pattern and every other pattern in pattern matrix
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
            Rhino.RhinoApp.WriteLine($"Model size: {new Vector3d(inputModel.Size.X, inputModel.Size.Y, inputModel.Size.Z)}");
            Rhino.RhinoApp.WriteLine("Model Ready!");
        }
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
            patterns = new List<int[,,]>();
            patternMatrix = new int[(int)Math.Ceiling((double)(inputModel.Size.X / patternSize)),
                (int)Math.Ceiling((double)(inputModel.Size.Y / patternSize)),
                (int)Math.Ceiling((double)(inputModel.Size.Z / patternSize))];
            probabilites = new Dictionary<int, double>();


            inputModel.Voxels.ForEach(voxel => inputMatrix[voxel.X, voxel.Y, voxel.Z] = voxel.Color);

            ////Add "empty space" pattern.
            patterns.Add(CreateEmptyPattern(patternSize));
            probabilites[0] = 0;

            Rhino.RhinoApp.WriteLine(patternMatrix.GetLength(0) + " x__" + patternMatrix.GetLength(1) + " y__" + patternMatrix.GetLength(2) + " z");
            for (var x = 0; x < patternMatrix.GetLength(0); x++)
            {
                for (var y = 0; y < patternMatrix.GetLength(1); y++)
                {
                    for (var z = 0; z < patternMatrix.GetLength(2); z++)
                    {
                        var currentPattern = GetCurrentPattern(inputMatrix, x * patternSize,y *patternSize, z * patternSize, patternSize);
                        var index = patterns.ContainsPattern(currentPattern);
                        if (index < 0)
                        {
                            patterns.Add(currentPattern);
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
        //    string test = "";
        //    for (int i = 0; i < patternMatrix.GetLength(2); i++)
        //    {
        //        for (int j = 0; j < patternMatrix.GetLength(1); j++)
        //        {
        //            for (int k = 0; k < patternMatrix.GetLength(0); k++)
        //            {
        //                test += patternMatrix[k, j, i].ToString();
        //                test += " ";
        //            }
        //            test += Environment.NewLine;
        //        }
        //        test += "--------------";
        //        test += Environment.NewLine;
        //    }

        //    Rhino.RhinoApp.WriteLine(test);

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

        private void DetectNeighbours()
        {

            foreach (var pattern in patternMatrix)
            {
                foreach (var otherPattern in patternMatrix)
                {
                    CheckAddNeighbour(pattern, otherPattern);

                }
            }

        }

        private void CheckAddNeighbour(int pattern, int otherPattern)
        {
            var patternStruct = patterns[pattern];
            var otherPatternStruct = patterns[otherPattern];

            if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Left) && !NeighboursMap[pattern][Coord3D.Left].Contains(otherPattern))
                NeighboursMap[pattern][Coord3D.Left].Add(otherPattern);
            if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Right) && !NeighboursMap[pattern][Coord3D.Right].Contains(otherPattern))
                NeighboursMap[pattern][Coord3D.Right].Add(otherPattern);

            if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Down) && !NeighboursMap[pattern][Coord3D.Down].Contains(otherPattern))
                NeighboursMap[pattern][Coord3D.Down].Add(otherPattern);
            if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Up) && !NeighboursMap[pattern][Coord3D.Up].Contains(otherPattern))
                NeighboursMap[pattern][Coord3D.Up].Add(otherPattern);

            if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Back) && !NeighboursMap[pattern][Coord3D.Back].Contains(otherPattern))
                NeighboursMap[pattern][Coord3D.Back].Add(otherPattern);
            if (patternStruct.FitsPattern(otherPatternStruct, Coord3D.Forward) && !NeighboursMap[pattern][Coord3D.Forward].Contains(otherPattern))
                NeighboursMap[pattern][Coord3D.Forward].Add(otherPattern);




            //foreach (KeyValuePair<int, Dictionary<Coord3D, List<int>>> kvp in NeighboursMap)
            //{
            //    foreach (KeyValuePair<Coord3D, List<int>> hhh in kvp.Value)
            //        Rhino.RhinoApp.WriteLine("Key = {0}, keyin = {1},valuein ={2}", kvp.Key, hhh.Key.X.ToString() + "|"
            //            + hhh.Key.Y.ToString() + "|" + hhh.Key.Z.ToString(), String.Join(", ", hhh.Value));

            //}

        }


        public override void Observe()
        {
            Rhino.RhinoApp.WriteLine("I observed");
            //Build a list of nodes that have not been collapsed to a definite state.
            //A node is collapsabel when for the nodes x y z the output matrix is not 0 or 1
            // i think the output matrix are the posible x y z of the node that has not yet collapsed
            var collapsableNodes = GetCollapsableNodes();
           
            //Pick a random node from the collapsible nodes.
            if (collapsableNodes.Count == 0)
            {
                Rhino.RhinoApp.WriteLine("contradiction true: from Observe");
                Contradiction = true;
                return;
            }
            //var nodeCoords = collapsableNodes[Rnd.Next(collapsableNodes.Count)];
            var nodeCoords = collapsableNodes[0];

            //instead of start random start from the middle of the output grid
           // var nodeCoords = GetCollapsableNodeMiddle();
         
           // var availableNodeStates = outputMatrix[nodeCoords.X, nodeCoords.Y, nodeCoords.Z].Except(new[] { 0 }).ToList();
            var availableNodeStates = outputMatrix[nodeCoords.X, nodeCoords.Y, nodeCoords.Z].ToList();
            //Rhino.RhinoApp.WriteLine("the node at" + nodeCoords.X.ToString() + nodeCoords.Y.ToString() + nodeCoords.Z.ToString());
            //Rhino.RhinoApp.WriteLine("has these available states:");


            if (ProbabilisticModel)
            {
                //Rhino.RhinoApp.WriteLine("availableNodeStatesbefore");
                //foreach (int state in availableNodeStates)
                //{

                //    Rhino.RhinoApp.Write(state.ToString());
                //}

                //Eliminate all duplicates from the list of possible states.
                //why does he shuffle the list?
                //also there are no duplicates i think...
                availableNodeStates = availableNodeStates.Distinct().ToList().Shuffle().ToList();
                //availableNodeStates = availableNodeStates.Shuffle().ToList();
                //Rhino.RhinoApp.WriteLine("---------------------");

                //Rhino.RhinoApp.WriteLine("availableNodeStatesafter");
                //foreach (int state in availableNodeStates)
                //{
                //
                //    Rhino.RhinoApp.Write(state.ToString());
                //}


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
                //Collapse it to a random definite state.
                outputMatrix.SetValue(new List<int>() { availableNodeStates[Rnd.Next(availableNodeStates.Count)] }, nodeCoords.X, nodeCoords.Y, nodeCoords.Z);
            }
            //  foreach (KeyValuePair<int, double> hhh in probabilites)
            //    Rhino.RhinoApp.WriteLine(hhh.Key.ToString() + "-->" + hhh.Value.ToString());


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
                        if(!Periodic) continue;
                        else
                        {
                            //0->n 5->m 0=result, 1->n 5->m 1=result, 4->n 5->m 4=result,-1->n 5->m 4=result, 1->n 5->m 1=result, 5->n 5->m 0=result
                            nodeToBeChanged = new Coord3D(Mod(nodeToBeChanged.X, outputMatrix.GetLength(0)),
                                Mod(nodeToBeChanged.Y, outputMatrix.GetLength(1)),
                                Mod(nodeToBeChanged.Z, outputMatrix.GetLength(2)));
                        }

                    }

                    
                    
                    //Count the states before the propagation.
                    var statesBefore = outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z].Count;
                   // var restate = outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z];
                    //Rhino.RhinoApp.WriteLine(statesBefore.ToString()+"--> before");

                    //Eliminate neighbours that are not allowed from the output matrix
                    var allowedNghbrsInDirection = allowedNghbrs[direction].Distinct().ToList();
                    outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z]
                        .RemoveAll(neighbour => !allowedNghbrsInDirection.Contains(neighbour));

                    //Count the states after, if nbBefore != nbAfter queue it up.
                    var statesAfter = outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z].Count;
                    //Rhino.RhinoApp.WriteLine(statesAfter.ToString() + "--> after");
                    //Check for contradictions
                    // TODO Add a backtrack recovery system to remedy the contradictions.
                    if (outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z].Count == 0)
                    {
                        // restate.Clear();
                        Rhino.RhinoApp.WriteLine("contradiction true: from Propagate");

                        Contradiction = true;
                        return;
                       // outputMatrix[nodeToBeChanged.X, nodeToBeChanged.Y, nodeToBeChanged.Z] = restate;
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
