﻿using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;
        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
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

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "1. szeminárium - háromszög";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);


            


            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;

            
            graphicWindow.Update += GraphicWindow_Update;

           
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            CheckError();


            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        public static void CheckError()
        {
            GLEnum error = Gl.GetError();
            if (error != GLEnum.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);


            CheckError();


            float[] vertexArray = new float[] {
               /* -0.5f, -0.5f, 0.0f,
                +0.5f, -0.5f, 0.0f,
                 0.0f, +0.5f, 0.0f,
                 1f, 1f, 0f,*/

                 //Felso oldal
                 0,0,0,//b
                 0.5f,0.2f,0,//f
                 0, 0.4f, 0,//g
                 -0.5f, 0.2f, 0,//d

                 //bal oldal
                 -0.5f, 0.2f, 0,//d 4
                 -0.5f, -0.4f, 0, //c 5 
                  0, -0.6f, 0,//a 6 
                  0,0,0,//b 7

                  //jobb oldal
                  0,0,0,//b 8
                  0, -0.6f, 0,//a 9
                  0.5f, -0.4f, 0,//e 10
                  0.5f,0.2f,0,//f 11

            };

            //rubik kocka kicsi kockaja
            /*for (int i=0; i<vertexArray.Length; i++) {
                vertexArray[i] /= 3;
            }*/


            float[] colorArray = new float[] {
               /* 1.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,*/

                //kocka szinei:
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,


                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,

                //vonalak
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,


            };


           
            uint[] indexArray = new uint[] {
                // b, f,d
                0,1,3,
                //d f g
                2,3,1,
                //d c b
                7,5,4,
                //c b a
                5, 7 , 6,
                //a f e
                9, 11, 10,
                //a b f
                9, 8, 11,

                11, 10 ,11
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);

           
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);
            
            CheckError();

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);


            CheckError();


            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);

            CheckError();




            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); 
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            CheckError();


            //rubik kocka vonalai
            float[] lineVertices = new float[] {
                -0.5f / 3, 0.2f / 3, 0.0f,//teteje harmadolo
                1f/3, 0.8f/3, 0.0f,

              -1f/3, 0.4f /3, 0.0f,
               0.5f/3,1f/3, 0.0f,


               -1f/3,0.8f/3,0f,
              0.5f/3,0.2f/3,0f,

                -0.5f/3,1f/3,0f,
              1/3f,0.4f/3,0f,//k

              //jobb oldal lefele
                0.5f/3,0.2f/3,0f,
                0.5f/3,-1.6f/3,0f,


              1/3f,0.4f/3,0f,//k
                1f/3,-1.4f/3,0f,
            //jobb oldal oldalra
                0f,-0.6f/3,0f,
               1.5f/3,0f,0f,


                0f, -1.2f/3,0f,
                1.5f/3,-0.6f/3,0f,

                //Bal oldal lefele
                 -0.5f / 3, 0.2f / 3, 0.0f,
                 -0.5f/3,-1.6f/3,0f,



                 -1f/3, 0.4f /3, 0.0f,
                 -1f/3, -1.4f/3, 0.0f,

                 //Bal oldal oldalra

                   0f,-0.6f/3,0f,
                   -1.5f/3,0f,0f,

                    0f, -1.2f/3,0f,
                    -1.5f/3,-0.6f/3,0f,
            };

            uint lineVao = Gl.GenVertexArray();
            Gl.BindVertexArray(lineVao);

            uint lineVbo = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, lineVbo);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)lineVertices.AsSpan(), GLEnum.StaticDraw);

            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            Gl.DrawArrays(GLEnum.Lines, 0, 24);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            Gl.BindVertexArray(0);


            CheckError();


            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);



            CheckError();

        }
    }
}
