using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
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

        internal void RotateGroup(int[] group,ref Matrix4X4<float>[] cubeTransMatrix)
        {
            var rotation = CreateRotationMatrix(Vector3D<float>.UnitY, MathF.PI / 2);

            foreach (var i in group)
            {
                cubeTransMatrix[i] = rotation * cubeTransMatrix[i];
            }
        }

        private Matrix4X4<float> CreateRotationMatrix(Vector3D<float> axis, float angle)
        {
            var cos = MathF.Cos(angle);
            var sin = MathF.Sin(angle);
            var t = 1 - cos;

            return new Matrix4X4<float>(
                t * axis.X * axis.X + cos, t * axis.X * axis.Y - sin * axis.Z, t * axis.X * axis.Z + sin * axis.Y, 0,
                t * axis.X * axis.Y + sin * axis.Z, t * axis.Y * axis.Y + cos, t * axis.Y * axis.Z - sin * axis.X, 0,
                t * axis.X * axis.Z - sin * axis.Y, t * axis.Y * axis.Z + sin * axis.X, t * axis.Z * axis.Z + cos, 0,
                0, 0, 0, 1
            );
        }
    }
}
