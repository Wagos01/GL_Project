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





        internal void AdvanceTime(double deltaTime)
        {
            // we do not advance the simulation when animation is stopped
            if (!AnimationEnabled)
                return;

            // set a simulation time
            Time += deltaTime;

            // lets produce an oscillating scale in time
            CenterCubeScale = 1 + 0.2 * Math.Sin(1.5 * Time);

            // the rotation angle is time x angular velocity;
            DiamondCubeLocalAngle = Time * 10;

            DiamondCubeGlobalYAngle = -Time;
        }

        internal void RotateHorizontalGroup(int index, ref Matrix4X4<float>[] cubeTransMatrix) { 
            Matrix4X4<float> rotation = Matrix4X4.CreateRotationY(MathF.PI / 2);
           
            index = 3 * index;

            int[] newOrder = new int[9];
         
            int move = 0;
            int index2 = index;

            int[] indexes = new int[9];
            int i = 0;

            while (i < 9) {
                indexes[i] = index;
                index++;
                move++;
                if (move == 3) {
                    index += 6;
                    move = 0;
                }
                i++;
                //Console.Write(indexes[i-1]+" ");
            }
            //Console.WriteLine();

            for (i = 0; i < 9; i++)
            {
                cubeTransMatrix[Program.globalStatus[indexes[i]]] = rotation * cubeTransMatrix[Program.globalStatus[indexes[i]]];
            }


            newOrder[0] = Program.globalStatus[indexes[2]];
            newOrder[1] = Program.globalStatus[indexes[5]];
            newOrder[2] = Program.globalStatus[indexes[8]];
            newOrder[3] = Program.globalStatus[indexes[1]];
            newOrder[4] = Program.globalStatus[indexes[4]];
            newOrder[5] = Program.globalStatus[indexes[7]];
            newOrder[6] = Program.globalStatus[indexes[0]];
            newOrder[7] = Program.globalStatus[indexes[3]];
            newOrder[8] = Program.globalStatus[indexes[6]];


            for (i = 0; i < 9; i++)
            {
                Program.globalStatus[indexes[i]] = newOrder[i];

                //Write out
               // Console.Write(Program.globalStatus[indexes[i]] + " ");
            }



          //  Console.WriteLine();

        }

        internal void RotateVerticalGroup(int index, ref Matrix4X4<float>[] cubeTransMatrix) {
            Matrix4X4<float> rotation = Matrix4X4.CreateRotationX( MathF.PI / 2);
            
            index = 9* index;
           
            int[] newOrder = new int[9];

            for (int i = index; i < index + 9; i++)
            {
                cubeTransMatrix[Program.globalStatus[i]]= rotation * cubeTransMatrix[Program.globalStatus[i]];
            }

            newOrder[0] = Program.globalStatus[index+2];
            newOrder[1] = Program.globalStatus[index+5];
            newOrder[2] = Program.globalStatus[index+8];
            newOrder[3] = Program.globalStatus[index + 1];
            newOrder[4] = Program.globalStatus[index + 4];
            newOrder[5] = Program.globalStatus[index + 7];
            newOrder[6] = Program.globalStatus[index + 0];
            newOrder[7] = Program.globalStatus[index + 3];
            newOrder[8] = Program.globalStatus[index + 6];


            for (int i = index; i < index + 9; i++)
            {
                Program.globalStatus[i] = newOrder[i - index];

                //Console.Write(Program.globalStatus[i] + " ");
            }

            //Console.WriteLine();

        }
       

       
    }
}
