using GrafikaSzeminarium;
using Silk.NET.Input;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Szeminarium
{
    internal class CubeArrangementModel
    {
        


        public double RubikCubeScale { get; private set; } = 0.3;
        public float RubikCubeRotation { get; private set; } = 0.0f;
        public bool isSolved { get; set; } = false;

        public static double Time = 0;

        public void RubicsCubeAnimation(double deltaTime)
        {
            if(!isSolved)
            {
                return;
            }

            Time += deltaTime;

            RubikCubeScale = 0.3 + 0.2 * Math.Sin(0.7 * Time);
            RubikCubeRotation =(float)Time;

            //5 masodpercig fog villogni
            if (Time >= 5)
            {
                Time = 0;
                isSolved = false;
                RubikCubeScale = 0.3;
                RubikCubeRotation = 0.0f;
            }
        }


        ///forgatas iranya
        public float direction { get;  set; } = 1;

        public static bool isRotating { get; private set; } = false;
        public static bool isRandom { get; private set; } = false;
        private static int randomCounter = 0;

        private float angleToRotate = (float)(Math.PI / 2f);//in radian
        private float angleRotated = 0.0f;
        // private float rotationTime = 0.5f;

        private char currentAxis;
        private int currentLayer;
        private Matrix4X4<float>[] currentTransMatrix;
        //cubeTransMatrix

        private Random rand = new Random();
        char randomAxis;
        float randomDirection;
        int randomLayer;


        public void startRandom()
        {
            isRandom = true;
            randomCounter = 0;
            angleRotated = 0;

            randomAxis = (char)('X' + rand.Next(3));
            randomDirection = rand.Next(2) * 2 - 1;//-1 vagy 1
            randomLayer = rand.Next(3) - 1;//-1, 0 vagy 1

            currentTransMatrix = CalculateFinalMatrixes(randomLayer, randomAxis, GetLayerIndexes(randomAxis, randomLayer), randomDirection);

        }
        internal void AdvanceTime(double deltaTime)
        {
            RubicsCubeAnimation(deltaTime);
            if (isRandom)
            {
                if (randomCounter < 30)
                {



                    angleRotated += 8 * (float)deltaTime;

                    if (angleRotated >= angleToRotate)
                    {
                        randomCounter++;
                        
                        angleRotated = 0.0f;

                        List<int> layerIndexes = new List<int>(GetLayerIndexes(randomAxis, randomLayer));
                        List<float[]> newRubicsCube = new List<float[]>(Program.RubicsCube);

                        foreach (int index in layerIndexes)
                        {
                            Program.RubicsCube[index] = UpdateCubePosition(newRubicsCube[index], randomAxis,randomDirection);
                        }
                        Program.cubeTransMatrix = currentTransMatrix;

                        //calculate the next rotation
                        randomAxis = (char)('X' + rand.Next(3));
                        randomDirection = rand.Next(2) * 2 - 1;//-1 vagy 1
                        randomLayer = rand.Next(3) - 1;//-1, 0 vagy 1
                        
                        currentTransMatrix = CalculateFinalMatrixes(randomLayer, randomAxis, GetLayerIndexes(randomAxis, randomLayer), randomDirection);
                    }
                    else
                    {
                        RotateLayer(randomLayer, randomAxis, 8 * (float)deltaTime,  randomDirection);
                    }
                } else
                {
                    isRandom = false;
                }
            }
            if (isRotating)
            {
                angleRotated += 3 * (float)deltaTime;

                if (angleRotated >= angleToRotate)
                {
                    isRotating = false;
                    angleRotated = 0.0f;

                    List<int> layerIndexes = new List<int>(GetLayerIndexes(currentAxis, currentLayer));
                    List<float[]> newRubicsCube = new List<float[]>(Program.RubicsCube);

                    foreach (int index in layerIndexes)
                    {
                        Program.RubicsCube[index] = UpdateCubePosition(newRubicsCube[index], currentAxis, direction);
                    }
                    Program.cubeTransMatrix = currentTransMatrix;

                    //Ellenorizzuk hogy sikerulte kirakjuk
                    if (Program.CheckIfSolved())
                    {
                        isSolved = true;
                    }


                }
                else
                {
                    RotateLayer(currentLayer, currentAxis, 3 * (float)deltaTime, direction);

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
                currentTransMatrix = CalculateFinalMatrixes(cubeLayer, axis, GetLayerIndexes(currentAxis, currentLayer), direction);
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

        //Kiszamitja a 90 fokkal elforgatott kocka trans matrixait, hogy ne legyenek a float szamitasok miatti elteresek
        private Matrix4X4<float>[] CalculateFinalMatrixes(int cubeLayer, char axis, int[] layerIndexes,float direction)
        {
            Matrix4X4<float>[] finalMatrixes = (Matrix4X4<float>[])Program.cubeTransMatrix.Clone();
            Matrix4X4<float> rotation;
            switch (axis)
            {
                case 'X': rotation = (Matrix4X4<float>)Matrix4X4.CreateRotationX((Math.PI / 2f) * direction); break;
                case 'Y': rotation = (Matrix4X4<float>)Matrix4X4.CreateRotationY((Math.PI / 2f) * direction); break;
                case 'Z': rotation = (Matrix4X4<float>)Matrix4X4.CreateRotationZ((Math.PI / 2f) * direction); break;
                default: throw new Exception("rossz tengely");
            }

            foreach (int index in layerIndexes)
            {
                finalMatrixes[index] = finalMatrixes[index] * rotation;
            }

            return finalMatrixes;
        }



        public void RotateLayer(int cubeLayer, char axis, float angle,float direction)
        {
         

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
        private float[] UpdateCubePosition(float[] pos, char axis, float direction)
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
