using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static ModelObjectDescriptor[] cube = new ModelObjectDescriptor[27];

        private static Matrix4X4<float>[] cubeTransMatrix = new Matrix4X4<float>[27];//MInden kockanak egy sajat transzformacio matrix

        private static List<int[]> rubicGroupIndexes = new List<int[]>(); //6 felekeppen lehet egy rubik kockat forgatni, itt a kicsi kockakat hozza rendeljuk a forgatasokhoz

        private static CameraDescriptor camera = new CameraDescriptor();

        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();

        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        private static uint program;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Grafika szeminárium";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;


            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            for(int i=0;i<27; i++)
            {
                cube[i].Dispose();
            }
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            for (int i = 0; i < 27; i++)
            {
                cube[i] = ModelObjectDescriptor.CreateCube(Gl, i);
                cubeTransMatrix[i] = Matrix4X4<float>.Identity;
            }

            for(int i = 0; i < 6; i++)
            {
                rubicGroupIndexes.Add(new int[9]);//1 oldalon 9 kocka van, 1 forgatassal 9 kocka forgatodik
            }


            SetUpRotations();

            Gl.ClearColor(System.Drawing.Color.White);
            
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(TriangleFace.Back);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {

            }

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    camera.DecreaseZYAngle();
                    break;
                case Key.Right:
                    camera.IncreaseZYAngle();
                    break;
                case Key.Down:
                    camera.IncreaseDistance();
                    break;
                case Key.Up:
                    camera.DecreaseDistance();
                    break;
                case Key.U:
                    camera.IncreaseZXAngle();
                    break;
                case Key.D:
                    camera.DecreaseZXAngle();
                    break;
                case Key.Space:
                    cubeArrangementModel.AnimationEnabled = !cubeArrangementModel.AnimationEnabled;
                    break;
                case Key.Number1:
                case Key.Number2:
                case Key.Number3:
                case Key.Number4:
                case Key.Number5:
                case Key.Number6:
                    cubeArrangementModel.RotateGroup(rubicGroupIndexes[key - Key.Number1], ref cubeTransMatrix);
                    graphicWindow.DoRender();
                    break;

            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe
            cubeArrangementModel.AdvanceTime(deltaTime);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 100f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);

            DrawRubicsCube();



        }

        private static unsafe void DrawRubicsCube()
        {
            Matrix4X4<float> rubicsScale = Matrix4X4.CreateScale((float)cubeArrangementModel.RubikCubeScale);

            float dist = 0.31f;

            int cubeIndex = 0;

            for (float i = -1; i <= 1; i++)
            {
                for (float j = -1; j <= 1; j++)
                {
                    for (float k = -1; k <= 1; k++)
                    {
                        Matrix4X4<float> trans = Matrix4X4.CreateTranslation(i * dist, j * dist, k * dist);
                        Matrix4X4<float> modelMatrixRubicsScube = rubicsScale * trans * cubeTransMatrix[cubeIndex];
                        SetMatrix(modelMatrixRubicsScube, ModelMatrixVariableName);
                        DrawModelObject(cube[cubeIndex]);
                        cubeIndex++;
                    }
                }
            }

        }


        private static unsafe void DrawModelObject(ModelObjectDescriptor modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }

        //6 felekeppen lehet egy rubik kockat forgatni,
        //itt a kicsi kockakat hozza rendeljuk a forgatasokhoz
        private static void SetUpRotations()
        {
            //az elso 3 a vizszintes forgatasok
            //masodik 3 a fuggoleges forgatasok
            rubicGroupIndexes[0] = (new int[] { 0, 1, 2, 9, 10, 11, 18, 19, 20 });
            rubicGroupIndexes[1] = (new int[] { 3, 4, 5, 12, 13, 14, 21, 22, 23 });
            rubicGroupIndexes[2] = (new int[] { 6, 7, 8, 15, 16, 17, 24, 25, 26 }); 

            for (int i = 3; i < 6; i++) {
                for(int j = 0; j < 9; j++) {
                    rubicGroupIndexes[i][j] = (i-3)*9 + j;
                }
            }

            /*for(int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(rubicGroupIndexes[i][j] + " ");
                }
                Console.WriteLine();
            }*/
        }


        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}