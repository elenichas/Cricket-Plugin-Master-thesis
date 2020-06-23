using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino.Geometry;
 

namespace Thesis.Help_classes
{
    public abstract class Model
    {
        protected static readonly Random Rnd = new Random();
        public bool ProbabilisticModel { get; protected set; }
        public bool Periodic { get; protected set; }

        protected int[,,] patternMatrix;
        public List<int[,,]> patterns;
        protected int PatternSize { get; set; }
        public  Dictionary<int, double> probabilites;

        public Dictionary<int, Dictionary<Coord3D, List<int>>> NeighboursMap { get; protected set; }
        protected List<int>[,,] outputMatrix;

        protected Coord3D OutputSize { get; set; }

        public bool GenerationFinished { get; protected set; } = false;
        public bool Contradiction { get; protected set; } = false;
        public int NumGen { get; protected set; }

        //Abstract methods.

        protected abstract void Init(InputModel inputModel, int patternSize, bool periodic);

        public abstract void Observe();

        protected abstract void Propagate(Coord3D startPoint);

        public abstract int[,,] GetOutput();

        //Auxiliary methods.

        protected static int[,,] GetCurrentPattern(int[,,] matrix, int x, int y, int z, int patternSize)
        {
            var pattern = new int[patternSize, patternSize, patternSize];
            for (var i = x; i < x + patternSize; i++)
            {
                for (var j = y; j < y + patternSize; j++)
                {
                    for (var k = z; k < z + patternSize; k++)
                    {
                        pattern[i - x, j - y, k - z] = matrix[i % matrix.GetLength(0), j % matrix.GetLength(1), k % matrix.GetLength(2)];

                    }
                }
            }
 
            return pattern;
        }

        protected void InitOutputMatrix(Coord3D size)
        {
            outputMatrix = new List<int>[size.X, size.Y, size.Z];

            for (var x = 0; x < size.X; x++)
            {
                for (var y = 0; y < size.Y; y++)
                {
                    for (var z = 0; z < size.Z; z++)
                    {
                        outputMatrix[x, y, z] = new List<int>();

                        for (var i = 0; i < patterns.Count; i++)
                        {
                          
                            outputMatrix[x, y, z].Add(i);
                        }
                    }
                }
            }
        }

        protected List<Coord3D> GetCollapsableNodes()
        {
            var collapsableNodes = new List<Coord3D>();
            for (var x = 0; x < outputMatrix.GetLength(0); x++)
            {
                for (var y = 0; y < outputMatrix.GetLength(1); y++)
                {
                    for (var z = 0; z < outputMatrix.GetLength(2); z++)
                    {
                        if (outputMatrix[x, y, z].Count != 1 && outputMatrix[x, y, z].Count != 0)                            
                           collapsableNodes.Add(new Coord3D(x, y, z));                      
                    }
                }
            }

            return collapsableNodes;
        }


        protected Coord3D GetCollapsableNodeMiddle()
        {
            
           Coord3D collapsableNodeM = new Coord3D(outputMatrix.GetLength(0)/2, outputMatrix.GetLength(1)/2, outputMatrix.GetLength(2)/2);

            return collapsableNodeM;
        }
        protected bool CheckIfFinished()
        {
            //the output matrix includes all the nodes
            return outputMatrix.Cast<List<int>>().All(node => node.Count == 1);
        }

        public void Clear()
        {
            InitOutputMatrix(OutputSize);
            Contradiction = false;
            GenerationFinished = false;
            NumGen = 0;
        }

        public static int Mod(int n, int m)
        {

            return ((n % m) + m) % m;

        }


    }
}
