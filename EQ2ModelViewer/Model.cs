using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;
using System.IO;
using System.Collections.Generic;
using System.Collections;

using Everquest2.Util;
using Everquest2.Visualization;

namespace EQ2ModelViewer
{
    public class Model
    {
        public Vector3 Position = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 Rotation = new Vector3(0.0f, 0.0f, 0.0f);
        public float Scale = 1.0f;
        public UInt32 WidgetID = 0;
        public UInt32 GridID = 0;
        public String modelName = "";
        List<MeshClass> m_meshes = new List<MeshClass>();
        LightShaderClass lightShader = new LightShaderClass();

        public bool Initialize(Device device, string modelFileName, string[] textureFileName)
        {
            if (!LoadModel(modelFileName))
            {
                Console.WriteLine("Model: Failed to load the model");
                return false;
            }

            int count = 0;
            foreach (MeshClass mesh in m_meshes)
            {
                if (!mesh.InitializeBuffers(device))
                {
                    Console.WriteLine("Model: Failed to initialize buffers.");
                    return false;
                }

                if (count >= textureFileName.Length)
                    mesh.LoadTexture(device, textureFileName[0]);
                else
                    mesh.LoadTexture(device, textureFileName[count]);

                count++;
            }

            lightShader.Initialize(device);
            return true;
        }

        public bool Initialize(Device device, VeMeshGeometryNode item, String baseDir)
        {
            if (item.collisionMeshName == null || item.collisionMeshName.Length < 1)
            {
                Console.WriteLine("No collision mesh for MeshGeometryNode");
                return false;
            }

            VeCollisionMesh collision = null;

            try
            {
                Eq2Reader reader2 = new Eq2Reader(new System.IO.FileStream(frmMain.DirName + item.collisionMeshName, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                collision = (VeCollisionMesh)reader2.ReadObject();
                reader2.Dispose();
            }
            catch (Exception ex)
            {

            }

            ArrayList textures = new ArrayList();

            string[][] meshes = ((VeMeshGeometryNode)item).renderMeshNames;
            for (int i = 0; i < meshes.Length; i++)
            {
                for (int j = 0; j < meshes[i].Length; j++)
                {
                    if (meshes[i][j] != null)
                    {
                        string path = meshes[i][j];

                        path = baseDir + path.Replace("/", "\\");
                        //                       if (path.Contains("\\flora\\"))
                        if (path.Contains("\\accessories\\"))
                        {
                            Console.WriteLine("Model: skipping loading of model (Accessories suck!)" + path);
                            return false;
                        }
                        else if (path.Contains("\\flora\\") && (path.Contains("leaves") || path.Contains("top_") || path.Contains("top0")))
                        {
                            Console.WriteLine("Model: skipping loading of model (Leaves and Tree Tops suck!)" + path);
                            return false;
                        }
                        string[] texture = frmMain.GetTextureFile(((VeMeshGeometryNode)item).shaderPaletteNames, baseDir);
                        string pickedTexture = "";
                        if (i < texture.Length && texture[i] != "goblin_ice.dds")
                        {
                            pickedTexture = texture[i];
                        }
                        else if (texture[0] != "goblin_ice.dds")
                        {
                            pickedTexture = texture[0];
                        }

                        if (pickedTexture.Length < 1)
                        {
                            Console.WriteLine("Model: missing texture " + path + " at i:" + i + " and j:" + j);
                        }
                        else
                        {
                            textures.Add(pickedTexture);

                            if (!LoadModel(path))
                            {
                                Console.WriteLine("Model: Failed to load the model " + path);
                                return false;
                            }
                        }
                    }
                }
            }

            int count = 0;
            ArrayList removeList = new ArrayList();
            foreach (MeshClass mesh in m_meshes)
            {
                if (mesh.GetVertices().Count < 1)
                {
                    removeList.Add(mesh);
                    continue;
                }

                if (!mesh.InitializeBuffers(device))
                {
                    Console.WriteLine("Model: Failed to initialize buffers.");
                    return false;
                }

                if (textures.Count > 0)
                {
                    if (count >= textures.Count)
                        mesh.LoadTexture(device, (string)textures[0]);
                    else
                        mesh.LoadTexture(device, (string)textures[count]);
                }

                count++;
            }

            // these are for meshclass files we couldn't load correctly, usually cause no primitivecount, only vertex count
            // moving on means trying to access a null texture and crashing in the render
            foreach (MeshClass mesh in removeList)
            {
                m_meshes.Remove(mesh);
            }

                lightShader.Initialize(device);
            return true;
        }

        public void Render(GraphicClass Graphics, CameraClass camera, bool highlight = false)
        {
            Matrix temp = Matrix.Multiply(Graphics.GetWorldMatrix(), Matrix.Scaling(Scale, Scale, Scale));
            temp = Matrix.Multiply(temp, Matrix.RotationYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z));
            temp = Matrix.Multiply(temp, Matrix.Translation(Position.X, Position.Y, Position.Z));

            Vector4 ambientColor;
            if (highlight)
                ambientColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            else
                ambientColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

            float increment = 0.05f;
            foreach (MeshClass mesh in m_meshes)
            {
                if (mesh.GetTexture() == null)
                    continue;

                mesh.RenderBuffers(Graphics.Context);
                lightShader.Render(Graphics.Context, mesh.GetIndexCount(), temp, camera.GetViewMatrix(), Graphics.GetProjectionMatrix(), mesh.GetTexture(), new Vector3(0.0f, 0.0f, 0.0f), ambientColor/*new Vector4(1.0f, 1.0f, 1.0f, 1.0f)*/, new Vector4(0.0f, 0.0f, 0.0f, 0.0f), camera.GetPosition(), new Vector4(0.0f, 0.0f, 0.0f, 0.0f), 0.0f);

                if (highlight)
                    ambientColor = new Vector4(increment, 1.0f, 0.0f, 1.0f - increment);

                increment += 0.05f;
                if (increment > .5f)
                    increment = 0.05f;
            }
        }

        public float TestIntersection(Ray ray, GraphicClass Graphics)
        {

            Matrix temp = Matrix.Multiply(Graphics.GetWorldMatrix(), Matrix.Scaling(Scale, Scale, Scale));
            temp = Matrix.Multiply(temp, Matrix.RotationYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z));
            temp = Matrix.Multiply(temp, Matrix.Translation(Position.X, Position.Y, Position.Z));

            float ret = 0.0f;
            foreach (MeshClass mesh in m_meshes)
            {
                int i = 0;
                while (i < mesh.m_model.Length)
                {
                    Vector3 vec1 = new Vector3(mesh.m_model[i].x, mesh.m_model[i].y, mesh.m_model[i].z);
                    Vector3 vec2 = new Vector3(mesh.m_model[i + 1].x, mesh.m_model[i + 1].y, mesh.m_model[i + 1].z);
                    Vector3 vec3 = new Vector3(mesh.m_model[i + 2].x, mesh.m_model[i + 2].y, mesh.m_model[i + 2].z);

                    Vector4 transformVec = Vector3.Transform(vec1, temp);
                    vec1 = new Vector3(transformVec.X, transformVec.Y, transformVec.Z);
                    transformVec = Vector3.Transform(vec2, temp);
                    vec2 = new Vector3(transformVec.X, transformVec.Y, transformVec.Z);
                    transformVec = Vector3.Transform(vec3, temp);
                    vec3 = new Vector3(transformVec.X, transformVec.Y, transformVec.Z);

                    if (Ray.Intersects(ray, vec1, vec2, vec3, out ret))
                    {
                        break;
                    }
                    i += 3;

                }
                if (ret != 0.0f)
                    break;
            }

            return ret;
        }

        private bool LoadModel(string modelFileName)
        {
            frmMain.AppendLoadFile("LoadModel: " + modelFileName);
            Eq2Reader reader = new Eq2Reader(File.OpenRead(modelFileName));
            VeRenderMesh model = (VeRenderMesh)reader.ReadObject();
            reader.Dispose();

            bool mod = false;
            if (modelFileName.Contains("ant_terrain_neroom_geo04_l"))
            {
                mod = true;
            }
            if (model.subMeshes.Length > 0)
            {
                for (int count = 0; count < model.subMeshes.Length; count++)
                {
                    MeshClass mesh = new MeshClass();
                    mesh.SetFaceCount(model.subMeshes[count].PrimitiveCount * 3);
                    int start = model.subMeshes[count].StartingIndex;
                    int end = model.subMeshes[count].PrimitiveCount * 3;

                    for (int i = 0; i < end; i++)
                    {
                        int index = i + start;
                        int indicesIdx = model.indices[index];
                        if (indicesIdx < 0 || indicesIdx >= model.vertices.Length)
                            break;
                        float x = model.vertices[model.indices[index]].X;
                        float y = model.vertices[model.indices[index]].Y;
                        float z = model.vertices[model.indices[index]].Z;

                        float nx = model.normals[model.indices[index]].X;
                        float ny = model.normals[model.indices[index]].Y;
                        float nz = model.normals[model.indices[index]].Z;

                        float tu = model.texCoords[0][model.indices[index]].U;
                        float tv = model.texCoords[0][model.indices[index]].V;

                        mesh.AddData(i, x, y, z, nx, ny, nz, tu, tv);
                    }

                    m_meshes.Add(mesh);
                }
            }
            else
            {
                MeshClass mesh = new MeshClass();

                mesh.SetFaceCount(model.indices.Length);

                for (int i = 0; i < model.indices.Length /*m_VertexCount*/; i++)
                {
                    if (model.indices[i] < 0 || model.indices[i] > model.vertices.Length)
                    {

                    }
                    else
                        mesh.AddData(i, model.vertices[model.indices[i]].X, model.vertices[model.indices[i]].Y, model.vertices[model.indices[i]].Z, model.normals[model.indices[i]].X, model.normals[model.indices[i]].Y, model.normals[model.indices[i]].Z, model.texCoords[0][model.indices[i]].U, model.texCoords[0][model.indices[i]].V);
                }

                m_meshes.Add(mesh);
            }

            return true;
        }

        public void ShutDown()
        {
            foreach (MeshClass mesh in m_meshes)
                mesh.ShutDown();
        }

        public List<Vector3> GetVertices()
        {
            List<Vector3> ret = new List<Vector3>();
            foreach (MeshClass m in m_meshes)
            {
                List<Vector3> newList = m.GetVertices();
                ret.AddRange(newList);
            }

            return ret;
        }
    }
}