using GrafikaSzeminarium;
using Silk.NET.Input;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Szeminarium
{
    internal class CubeArrangementModel
    {
        /// <summary>
        /// Gets or sets wheather the animation should run or it should be frozen.
        /// </summary>
        public bool AnimationEnabled { get; set; } = false;

        /// <summary>
        /// The time of the simulation. It helps to calculate time dependent values.
        /// </summary>
        private double Time { get; set; } = 0;

        /// <summary>
        /// The value by which the center cube is scaled. It varies between 0.8 and 1.2 with respect to the original size.
        /// </summary>
        public double CenterCubeScale { get; private set; } = 1;
        public double RubikCubeScale { get; private set; } = 0.3;

        /// <summary>
        /// The angle with which the diamond cube is rotated around the diagonal from bottom right front to top left back.
        /// </summary>
        public double DiamondCubeLocalAngle { get; private set; } = 0;

        /// <summary>
        /// The angle with which the diamond cube is rotated around the global Y axes.
        /// </summary>
        public double DiamondCubeGlobalYAngle { get; private set; } = 0;

        ///forgatas iranya
        public float direction { get;  set; } = 1;

        public static bool isRotating { get; private set; } = false;

        private float angleToRotate = (float)(Math.PI / 2f);//in radian
        private float angleRotated = 0.0f;
        // private float rotationTime = 0.5f;

        private char currentAxis;
        private int currentLayer;
        //cubeTransMatrix

        internal void AdvanceTime(double deltaTime)
        {
            if (isRotating)
            {
                angleRotated += 2*(float)deltaTime;

                if (angleRotated >= angleToRotate)
                {
                    isRotating = false;
                    angleRotated = 0.0f;

                    List<int> layerIndexes = new List<int>(GetLayerIndexes(currentAxis, currentLayer));
                    List<float[]> newRubicsCube = new List<float[]>(Program.RubicsCube);

                    foreach (int index in layerIndexes)
                    {
                        Program.RubicsCube[index] = UpdateCubePosition(newRubicsCube[index], currentAxis);
                    }
                }
                else
                {
                    RotateLayer(currentLayer, currentAxis,2* (float)deltaTime);

                }
            }
            
        }

        public void Rotate(int cubeLayer, char axis)
        {
            if(!isRotating)
            {
                isRotating = true;
                angleRotated = 0.0f;
                currentAxis = axis;
                currentLayer = cubeLayer;
                
            }
            if (angleRotated < angleToRotate || isRotating)
            {
                isRotating = true;
                
            }
            else
            {
                isRotating = false;
                angleRotated = 0.0f;


            }


        }
        public void RotateLayer(int cubeLayer, char axis, float angle)
        {
            //animating


            //Megcsinaljuk a megfelelo forgato matrixot
            Matrix4X4<float> rotation;
            switch (axis)
            {
                case 'X': rotation = Matrix4X4.CreateRotationX((angle) * direction); break;
                case 'Y': rotation = Matrix4X4.CreateRotationY((angle) * direction); break;
                case 'Z': rotation = Matrix4X4.CreateRotationZ((angle) * direction); break;
                default: return; // rossz tengely
            }

            //Console.WriteLine(cubeLayer); 

            //List<float[]> newRubicsCube = new List<float[]>(Program.RubicsCube);
            List<int> layerIndexes = new List<int>(GetLayerIndexes(axis, cubeLayer));

            foreach (int index in layerIndexes)
            {
                Program.cubeTransMatrix[index] = Program.cubeTransMatrix[index] * rotation;
                //Program.RubicsCube[index] = UpdateCubePosition(newRubicsCube[index], axis);
            }
           

        }
        private float[] UpdateCubePosition(float[] pos, char axis)
        {
            float[] newPos = new float[3];
            switch (axis)
            {
                case 'X':
                    newPos[0] = pos[0];
                    newPos[1] = -direction * pos[2];
                    newPos[2] = direction * pos[1];
                    break;
                case 'Y':
                    newPos[0] = direction * pos[2];
                    newPos[1] = pos[1];
                    newPos[2] = -direction * pos[0];
                    break;
                case 'Z':
                    newPos[0] = -direction * pos[1];
                    newPos[1] = direction * pos[0];
                    newPos[2] = pos[2];
                    break;
            }
            return newPos;
        }


        private static int[] GetLayerIndexes(char axis, int layer)
        {
            List<int> indexes = new List<int>();
            switch (axis)
            {
                case 'X':
                    for (int i = 0; i < Program.RubicsCube.Count; i++)
                    {
                        if (Program.RubicsCube[i][0] == layer)
                        {
                            indexes.Add(i);
                        }
                    }
                    break;
                case 'Y':
                    for (int i = 0; i < Program.RubicsCube.Count; i++)
                    {
                        if (Program.RubicsCube[i][1] == layer)
                        {
                            indexes.Add(i);
                        }
                    }
                    break;
                case 'Z':
                    for (int i = 0; i < Program.RubicsCube.Count; i++)
                    {
                        if (Program.RubicsCube[i][2] == layer)
                        {
                            indexes.Add(i);
                        }
                    }
                    break;
            }
            return indexes.ToArray();
          
        }

    }
}
