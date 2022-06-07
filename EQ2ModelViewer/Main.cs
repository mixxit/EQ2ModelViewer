using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO.Compression;
using System.Xml;
using System.Text.RegularExpressions;

using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;

using Everquest2.Util;
using Everquest2.Visualization;
using System.IO;
using SlimDX.DirectInput;
using SlimDX.Direct2D;
using System.Security.Permissions;

namespace EQ2ModelViewer
{
    public partial class frmMain : Form
    {
        private System.Collections.Generic.List<Model> m_Models = new System.Collections.Generic.List<Model>();
        private System.Collections.Generic.List<VeRegion> m_Regions = new System.Collections.Generic.List<VeRegion>();
        private GraphicClass Graphics = new GraphicClass();
        public Model SelectedModel = null;
        private string ZoneFile;
        private string AppendFileStr = "";
        private bool Render3DAspect = true;
        private bool AutoExportOnLoad = false;
        private bool AutoExportRegionOnLoad = false;
        private String AutoLoadFileName = "";
        private bool IsLoaded = false;
        public frmMain()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CleanUp()
        {
            foreach (Model model in m_Models)
                model.ShutDown();

            if (Graphics != null)
                Graphics.ShutDown();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            CleanUp();
        }

        private Key hitKey = Key.NoConvert;
        double timestamp = 0;
        private CameraClass camera;
        int region_nodes = 0;
        private void frmMain_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            Graphics.Initialize(pGraphics);

            /*LightShaderClass lightShader = new LightShaderClass();
            lightShader.Initialize(Graphics.Device);*/

            camera = new CameraClass();
            // Initialize a base view matrix for 2D rendering
            camera.SetPosition(0.0f, 0.0f, -1.0f);
            camera.Render();
            Matrix baseViewMatrix = camera.GetViewMatrix();

            // now set the cameras starting position
            camera.SetPosition(0.0f, 1.50f, -3.0f);

            PositionClass position = new PositionClass();
            position.SetPosition(0.0f, 1.50f, -3.0f);

            InputClass input = new InputClass();
            input.Initialize(this);

            TimerClass timer = new TimerClass();
            timer.Initialize();

            FPSClass fps = new FPSClass();
            fps.Initialize();

            TextClass text = new TextClass();
            text.Initialize(Graphics.Device, Graphics.Context, pGraphics.ClientSize.Width, pGraphics.ClientSize.Height, baseViewMatrix);

            BitmapClass bmp = new BitmapClass();
            bmp.Initialize(Graphics.Device, pGraphics.ClientSize.Width, pGraphics.ClientSize.Height, "Background.bmp", 145, 220, baseViewMatrix);
            
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    string cmd = args[i].ToLower();
                    if (cmd.Equals("norender"))
                    {
                        Render3DAspect = false;
                    }
                    else if (cmd.Equals("export"))
                    {
                        AutoExportOnLoad = true;
                    }
                    else if (cmd.Equals("exportregion"))
                    {
                        AutoExportRegionOnLoad = true;
                    }
                    else if ( cmd.StartsWith("appendexportfile"))
                    {
                        int equalsSign = cmd.IndexOf("=");
                        if (equalsSign > 0 && (equalsSign+1) < cmd.Length)
                        {
                            string appendFileVal = cmd.Substring(equalsSign+1, cmd.Length - equalsSign - 1);
                            AppendFileStr = appendFileVal;
                        }
                    }
                    else
                    {
                        AutoLoadFileName = args[i];
                        break;
                    }
                }
            }

            if (AutoLoadFileName.Length > 0)
                LoadZoneFile(AutoLoadFileName);
            if (AutoExportOnLoad)
                exportToolStripMenuItem_Click(null, EventArgs.Empty);
            if (AutoExportRegionOnLoad)
                toolStripMenuItemExportWater_Click(null, EventArgs.Empty);

            if (!Render3DAspect)
            {
                Application.Exit();
                return;
            }
            //  FrustumClass frustum = new FrustumClass();
            try
            {
                MessagePump.Run(this, () =>
                {
                    if (!Graphics.SwapChain.Disposed) {
                        timer.Frame();
                        fps.Frame();

                        // Input code
                        input.Frame();

                        position.SetFrameTime(timer.GetTime());

                        if (this.Focused)
                        {
                            if (input.IsKeyPressed(SlimDX.DirectInput.Key.LeftShift) ||
                            input.IsKeyPressed(SlimDX.DirectInput.Key.RightShift))
                                position.m_ShiftDown = true;
                            else
                                position.m_ShiftDown = false;

                            position.TurnLeft(input.IsLeftPressed());
                            position.TurnRight(input.IsRightPressed());
                            position.MoveForward(input.IsUpPressed());
                            position.MoveBackward(input.IsDownPressed());
                            position.MoveUpward(input.IsAPressed());
                            position.MoveDownward(input.IsZPressed());
                            position.LookUpward(input.IsPgUpPressed());
                            position.LookDownward(input.IsPgDownPressed());

                            if (input.IsLeftMousePressed())
                            {
                                TestIntersection(input.GetMouseX(), input.GetMouseY());
                            }

                            if (SelectedModel != null)
                            {
                                if (input.IsKeyPressed(SlimDX.DirectInput.Key.Delete))
                                {
                                    m_Models.Remove(SelectedModel);
                                    SelectedModel = null;
                                }
                                else if (input.IsKeyPressed(SlimDX.DirectInput.Key.Escape))
                                {
                                    SelectedModel = null;
                                }

                                if (input.IsKeyPressed(SlimDX.DirectInput.Key.LeftControl))
                                {
                                    if (hitKey == Key.NoConvert)
                                    {
                                        if (input.IsKeyPressed(SlimDX.DirectInput.Key.R))
                                            hitKey = Key.R;
                                        else if (input.IsKeyPressed(SlimDX.DirectInput.Key.T))
                                            hitKey = Key.T;
                                        else if (input.IsKeyPressed(SlimDX.DirectInput.Key.Y))
                                            hitKey = Key.Y;
                                    }
                                    double curTime = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
                                    if ((curTime - timestamp) > 50)
                                    {
                                        if (hitKey == Key.R)
                                        {
                                            if (input.IsKeyReleased(SlimDX.DirectInput.Key.R))
                                                hitKey = Key.NoConvert;
                                            SelectedModel.Rotation += new Vector3(0.01f, 0.0f, 0.0f);
                                            timestamp = curTime;
                                        }
                                        else if (hitKey == Key.T)
                                        {
                                            if (input.IsKeyReleased(SlimDX.DirectInput.Key.T))
                                                hitKey = Key.NoConvert;
                                            SelectedModel.Rotation += new Vector3(0.0f, 0.01f, 0.0f);
                                            timestamp = curTime;
                                        }
                                        else if (hitKey == Key.Y)
                                        {
                                            if (input.IsKeyReleased(SlimDX.DirectInput.Key.Y))
                                                hitKey = Key.NoConvert;
                                            SelectedModel.Rotation += new Vector3(0.0f, 0.0f, 0.01f);
                                            timestamp = curTime;
                                        }

                                        if (SelectedModel.Rotation.X > 360.0f)
                                            SelectedModel.Rotation.X = 0.0f;
                                        if (SelectedModel.Rotation.Y > 360.0f)
                                            SelectedModel.Rotation.Y = 0.0f;
                                        if (SelectedModel.Rotation.Z > 360.0f)
                                            SelectedModel.Rotation.Z = 0.0f;
                                    }
                                }
                            }
                        }

                        camera.SetPosition(position.GetPosition());
                        camera.SetRotation(position.GetRotation());

                        // Render Code
                        Graphics.BeginScene();

                        // 3D
                        // View matrix
                        camera.Render();

                        //frustum.ConstructFrustum(1000.0f, Graphics.GetProjectionMatrix(), camera.GetViewMatrix());
                        foreach (Model model in m_Models) {
                            //if (frustum.CheckSphere(model.Position.X, model.Position.Y, model.Position.Z, 10.0f))
                            //{
                            /*Matrix temp = Matrix.Multiply(Graphics.GetWorldMatrix(), Matrix.Scaling(model.Scale, model.Scale, model.Scale));
                            temp = Matrix.Multiply(temp, Matrix.RotationYawPitchRoll(model.Rotation.X, model.Rotation.Y, model.Rotation.Z));
                            temp = Matrix.Multiply(temp, Matrix.Translation(model.Position.X, model.Position.Y, model.Position.Z));*/
                            model.Render(Graphics, camera, (model == SelectedModel)/*Graphics.Context*/);
                            //lightShader.Render(Graphics.Context, model.GetIndexCount(), temp, camera.GetViewMatrix(), Graphics.GetProjectionMatrix(), model.GetTexture(), new Vector3(0.0f, 0.0f, 0.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f), camera.GetPosition(), new Vector4(0.0f, 0.0f, 0.0f, 0.0f), 0.0f);
                            //}
                        }
                        // 2D
                        Graphics.TurnZBufferOff();
                        Graphics.TurnOnAlphaBlending();

                        bmp.Render(Graphics, 10, 10);

                        text.SetFPS(fps.GetFPS(), Graphics.Context);
                        text.SetPosition(position.GetPosition(), Graphics.Context);
                        text.SetSelectedModel(SelectedModel, Graphics.Context);
                        text.Render(Graphics.Context, Graphics.GetWorldMatrix(), Graphics.GetOrthoMatrix());

                        Graphics.TurnOffAlphaBlending();
                        Graphics.TurnZBufferOn();

                        Graphics.EndScene();
                    }
                });
            }
            catch (Exception exception) { }
        }

        public bool TestIntersection(int mouseX, int mouseY) {
            float pointX, pointY;
            Matrix projectionMatrix, viewMatrix, inverseViewMatrix;

            projectionMatrix = Graphics.GetProjectionMatrix();
            pointX = (2.0F * (float)mouseX / (float)pGraphics.ClientSize.Width - 1.0f) / projectionMatrix.M11;
            pointY = (-2.0f * (float)mouseY / (float)pGraphics.ClientSize.Height + 1.0f) / projectionMatrix.M22;
            Ray ray = new Ray(new Vector3(), new Vector3(pointX, pointY, 1.0f));

            viewMatrix = camera.GetViewMatrix();
            inverseViewMatrix = Matrix.Invert(viewMatrix);
            ray = new Ray(Vector3.TransformCoordinate(ray.Position, inverseViewMatrix), Vector3.TransformNormal(ray.Direction, inverseViewMatrix));
            ray.Direction.Normalize();

            float selectionDistance = 0.0f;
            foreach (Model model in m_Models) {
                float distance = model.TestIntersection(ray, Graphics);
                if (distance > 0.0f && (selectionDistance == 0.0f || distance < selectionDistance)) {
                    selectionDistance = distance;
                    SelectedModel = model;
                }
            }

            return false;
        }

        public static void AppendLoadFile(String txt)
        {
            StreamWriter sw = File.AppendText("loaded.txt");
            sw.WriteLine(txt);
            sw.Close();
        }

        private void loadZoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadZoneFile();
        }

        public static String DirName = "";
        private void LoadZoneFile(String filename="")
        {
            IsLoaded = false;

            bool isDrawFile = false;

            string fullName = "";
            DirName = "";
            if (filename.Length < 1)
            {
                OpenFileDialog fd = new OpenFileDialog();
                fd.Filter = "lut/draw files (*.lut;*.draw)|*.lut;*.draw|lut files (*.lut)|*.lut|draw files (*.draw)|*.draw";
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    AppendLoadFile("===================================================");
                    AppendLoadFile("Loading " + fd.FileName);

                    if (fd.FileName.EndsWith(".draw"))
                    {
                        isDrawFile = true;
                        string temp = fd.FileName.Substring(0, fd.FileName.LastIndexOf("\\"));
                        ZoneFile = fd.SafeFileName.Substring(0, fd.SafeFileName.IndexOf(".draw"));
                        fullName = ZoneFile;
                        DirName = temp;
                        filename = fd.FileName;
                    }
                    else
                    {
                        string temp = fd.FileName.Substring(0, fd.FileName.IndexOf("zones"));
                        ZoneFile = fd.SafeFileName.Substring(0, fd.SafeFileName.IndexOf(".lut"));
                        fullName = ZoneFile;
                        DirName = temp;
                        filename = fd.FileName;
                    }
                }
            }
            else
            {
                if (filename.EndsWith(".draw"))
                {
                    isDrawFile = true;
                    string temp = filename.Substring(0, filename.LastIndexOf("\\"));
                    ZoneFile = filename.Substring(0, filename.IndexOf(".draw"));
                    fullName = filename;
                    DirName = temp;
                }
                else
                {
                    string temp = filename.Substring(0, filename.IndexOf("zones"));
                    ZoneFile = filename.Substring(0, filename.IndexOf(".lut"));
                    fullName = filename;
                    DirName = temp;
                }
            }

            if (isDrawFile)
            {
                Model tmpModel = new Model();
                string[] textures = new string[1];
                textures[0] = "goblin_ice.dds";
                tmpModel.Initialize(Graphics.Device, filename, textures);
                tmpModel.Position.X = 0;
                tmpModel.Position.Y = 0;
                tmpModel.Position.Z = 0;
                tmpModel.Rotation.X = 0;
                tmpModel.Rotation.Y = 0;
                tmpModel.Rotation.Z = 0;
                tmpModel.Scale = 1;
                tmpModel.WidgetID = 1;
                tmpModel.GridID = 1;
                m_Models.Add(tmpModel);
                return;
            }

            if (fullName.Length < 1)
            {
                MessageBox.Show("No filename provided for loading a zonefile!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            region_nodes = 0;

            if (!File.Exists(filename))
                return;

            System.IO.BinaryReader reader2 = new System.IO.BinaryReader(new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                // Image(2020): Was ReadUint32, qey_harbor.lut however has 00 1F 00 7A, so that as an int32 is a very large number!
                reader2.ReadUInt32();
                do
                {
                    if (reader2.BaseStream.Position + 2 >= reader2.BaseStream.Length)
                    {
                        break;
                    }
                    UInt16 size = reader2.ReadUInt16();
                    string file = new string(reader2.ReadChars(size));

                    // was duplicating drive name
                    file = file.Replace("/", "\\");

                    file = DirName + file;
                    AppendLoadFile("VOC Loading: " + file);

                if ( file.Contains("qey_harbor_qey_terrain_harbor_geo05_rmob_0"))
                    {
                    int test = 0;
                    }
                    Eq2Reader reader = new Eq2Reader(new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                    VeNode venode = reader.ReadNodeObject();

                    CheckNode(DirName, venode, false, null, null, false);

                    //MessageBox.Show("Done!");

                    // 16 bytes between file names, grid id's maybe?
                    reader2.ReadBytes(16);
                } while (true);

            IsLoaded = true;
            }

        float x, y, z = 0;
        float yaw, pitch, roll = 0;
        float scale = 0;
        UInt32 widgetID;
        UInt32 regionMapVersion = 2;
        private void toolStripMenuItemExportWater_Click(object sender, EventArgs e)
        {
            if (!IsLoaded)
                return;

            StreamWriter swfile = new StreamWriter(ZoneFile + AppendFileStr + ".regionread");
            using (BinaryWriter file = new BinaryWriter(File.Open(ZoneFile + AppendFileStr + ".EQ2Region", FileMode.Create)))
            {
                file.Write(ZoneFile);
                file.Write(regionMapVersion);
                file.Write(m_Regions.Count);
                Int32 regionNum = 0;
                foreach (VeRegion region in m_Regions)
                {
                    file.Write(regionNum);
                    regionNum += 1;
                    Int32 node = 0;
                    file.Write(region.region_type);
                    file.Write(region.position[0]);
                    file.Write(region.position[1]);
                    file.Write(region.position[2]);
                    file.Write(region.splitdistance);
                    file.Write(region.envFileChosen);

                    String outFile = "";

                    Regex trimmer = new Regex(@"(?!.*\/)(\w|\s|-)+\.region");
                    Match out_ = trimmer.Match(region.parentNode.regionDefinitionFile);
                    if (out_.Success && out_.Groups.Count > 0)
                        outFile = out_.Value;

                    file.Write(outFile);
                    file.Write(region.GridID);
                    file.Write(region.vert_count);
                    swfile.WriteLine();
                    swfile.WriteLine("REGION: " + region.position[0] + " " + region.position[1] + " " + region.position[2] + " " + region.splitdistance + " - RegionType: " + region.region_type);
                    if (region.parentNode.regionDefinitionFile != null)
                        swfile.WriteLine("REGIONFILE: " + region.parentNode.regionDefinitionFile);
                    if (region.parentNode.environmentDefinitions != null)
                    {
                        foreach (string str in region.parentNode.environmentDefinitions)
                            swfile.WriteLine("EnvDefinition: " + str);
                    }
                    swfile.WriteLine("EnvData: " + region.unkcount + " / " + region.parentNode.unk1 + " / " + region.parentNode.unk2);

                    for (ushort i = 0; i < region.vert_count; ++i)
                    {
                        Int32 regiontype = 1;
                        Int32 special = region.special;
                        swfile.WriteLine(node + " " + region.m_normals[i, 0] + " " + region.m_normals[i, 1] + " " +
                            region.m_normals[i, 2] + " " + region.m_distance[i] + " " + regiontype + " " + special + " " +
                           region.m_childindex[i, 0] + " " + region.m_childindex[i, 1]);
                        file.Write(node);
                        node += 1;
                        file.Write(region.m_normals[i, 0]);
                        file.Write(region.m_normals[i, 1]);
                        file.Write(region.m_normals[i, 2]);
                        file.Write(region.m_distance[i]);
                        file.Write(regiontype);
                        file.Write(special);
                        file.Write((Int32)region.m_childindex[i, 0]);
                        file.Write((Int32)region.m_childindex[i, 1]);
                    }
                }
                file.Close();
            }
            swfile.Close();
        }

        UInt32 GridID;
        private void CheckNode(string temp, object item, bool parentXform, object next, object parentNode, bool selectNodeParent)
        {
            if (item is VeMeshGeometryNode)
            {
                widgetID = ((VeNode)item).WidgetID;

                // testing antonica spires which are not oriented correctly
                //if ( widgetID == 2990295910 )
                // testing tutorial_island02 boat
                //if (== 1253219127)

                // tutorial_island02 water
                if(widgetID == 337652899)
                {
                    int test = 0;
                }
                if(widgetID == 625647901)
                {
                    int test = 0;
                }
                Model model = new Model();
                model.Initialize(Graphics.Device, (VeMeshGeometryNode)item, temp);
                model.Position.X = x;
                model.Position.Y = y;
                model.Position.Z = z;
                model.Rotation.X = yaw;
                model.Rotation.Y = pitch;
                model.Rotation.Z = roll;
                model.Scale = scale;
                model.WidgetID = widgetID;
                model.GridID = GridID;
                m_Models.Add(model);
            }
            else
            {

                if (widgetID == 2720558016)
                {
                    int test = 0;
                }
                float x1 = 0.0f;
                float y1 = 0.0f;
                float z1 = 0.0f;

                if (item is VeEnvironmentNode)
                {
                    String envFile = "";
                    String writeFileName = "";
                    VeEnvironmentNode env = (VeEnvironmentNode)item;
                    bool noFly = false;
                    if (env.environmentDefinitions != null)
                    {
                        foreach (string str in env.environmentDefinitions)
                        {
                            if (str.Contains("no_fly.xml"))
                            {
                                /* <VdlFile xmlns="Vdl" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="Vdl BaseClasses.xsd">
                                      <Environment VDLTYPE="OBJECT">
                                        <iPriority VDLTYPE="INT">1</iPriority>
                                        <bOverrideZoneAllowFlying VDLTYPE="BOOL">true</bOverrideZoneAllowFlying>
                                        <bAllowFlying VDLTYPE="BOOL">false</bAllowFlying>
                                      </Environment>
                                   </VdlFile>
                                   */
                                noFly = true;
                                break;
                            }
                        }
                    }
                        if (noFly || env.regionDefinitionFile != null && env.regionDefinitionFile.Length > 0)
                    {
                        int waterType = 0;
                        if (!noFly && env.environmentDefinitions != null)
                        {
                            foreach (string str in env.environmentDefinitions)
                            {
                                writeFileName = str;

                                Regex trimmer = new Regex(@"(?!.*\/)(\w|\s|-)+\.xml");
                                Match out_ = trimmer.Match(writeFileName);
                                if (out_.Success && out_.Groups.Count > 0)
                                    writeFileName = out_.Value;

                                envFile = str;
                                envFile = envFile.Replace("/", "\\");

                                envFile = DirName + envFile;
                                waterType = LoadEnvXmlParseLiquid(envFile);
                                if (waterType != 0)
                                    break;
                            }
                        }

                        if (noFly)
                        {
                            /* no fly does not have normals in a env.regionDefinitionFile
                            ** perhaps they expect us to use the VolumeBox at the parent level?
                            */
                        }
                        else
                        {
                            bool watervol = env.regionDefinitionFile.Contains("watervol");
                            bool waterregion = env.regionDefinitionFile.Contains("waterregion");
                            bool waterregion2 = env.regionDefinitionFile.Contains("water_region");
                            bool iswater = env.regionDefinitionFile.Contains("water");
                            bool isocean = env.regionDefinitionFile.Contains("ocean");
                            bool isvolume = env.regionDefinitionFile.Contains("volume");
                            AppendLoadFile("Region established: " + waterType + ", " + envFile
                                + " WaterVol: " + watervol + " WaterRegion: " + waterregion +
                                " WaterRegion2: " + waterregion2 + " IsWater: " + iswater +
                                " IsOcean: " + isocean + " IsVolume: " + isvolume);
                            if (waterType == -2 || waterType == -3) // lava
                            {
                                AppendLoadFile("Lava region accepted: " + waterType + ", " + envFile);
                                Eq2Reader reader2 = new Eq2Reader(new System.IO.FileStream(DirName + env.regionDefinitionFile, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                                VeRegion region = (VeRegion)reader2.ReadObject();
                                region.parentNode = env;
                                region.region_type = 1; // default 'region' algorithm
                                region.special = -3;
                                region.envFileChosen = writeFileName;
                                region.GridID = GridID;
                                region_nodes += region.vert_count;
                                m_Regions.Add(region);
                            }
                            else if (waterType > 0)
                            {
                                AppendLoadFile("Region accepted: " + waterType + ", " + envFile
                                    + " WaterVol: " + watervol + " WaterRegion: " + waterregion +
                                    " WaterRegion2: " + waterregion2 + " IsWater: " + iswater +
                                    " IsOcean: " + isocean + " IsVolume: " + isvolume);
                                Eq2Reader reader2 = new Eq2Reader(new System.IO.FileStream(DirName + env.regionDefinitionFile, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                                VeRegion region = (VeRegion)reader2.ReadObject();
                                region.parentNode = env;
                                region.region_type = 0; // default water volume

                                if (waterregion) // 'sea'/ocean/waterregion in tutorial_island02 / qeynos_harbor
                                    region.region_type = 1;
                                else if (waterregion2)
                                    region.region_type = 0;
                                else if (isvolume && selectNodeParent)
                                    region.region_type = 4;
                                else if ((isocean && selectNodeParent)) // ocean in antonica/commonlands/tutorial
                                    region.region_type = 3;
                                else if (isocean && iswater) // caves in frostfang(halas)
                                    region.region_type = 4;
                                else if (isocean)
                                    region.region_type = 5;

                                region.special = waterType;
                                region_nodes += region.vert_count;
                                region.envFileChosen = writeFileName;
                                region.GridID = GridID;
                                m_Regions.Add(region);
                            }
                            else
                            {
                                if (env.regionDefinitionFile != null)
                                {
                                    AppendLoadFile("Region skipped: " + env.regionDefinitionFile);
                                }
                                else
                                    AppendLoadFile("Region skipped: ???");

                                if (env.environmentDefinitions != null)
                                {
                                    foreach (string str in env.environmentDefinitions)
                                        AppendLoadFile("EnvDefinition: " + str);
                                }
                            }
                        }
                    }
                }
                else if (item is VeRoomItemNode)
                {
                    yaw = ((VeRoomItemNode)item).orientation[0];
                    pitch = ((VeRoomItemNode)item).orientation[1];
                    roll = ((VeRoomItemNode)item).orientation[2];
                    GridID = ((VeRoomItemNode)item).myId_grid;
                }
                else if (item is VeXformNode)
                {
                    VeXformNode tmp = (VeXformNode)item;
                    x1 = ((VeXformNode)item).position[0];
                    y1 = ((VeXformNode)item).position[1];
                    z1 = ((VeXformNode)item).position[2];

                    if ( x1 < -16.39 && x1 > -16.41)
                    {
                        int test = 0;
                    }
                    if (parentXform)
                    {
                        yaw += (((VeXformNode)item).orientation[0]) * (3.141592654f / 180.0f);
                        pitch += (((VeXformNode)item).orientation[1]) * (3.141592654f / 180.0f);
                        roll += (((VeXformNode)item).orientation[2]) * (3.141592654f / 180.0f);
                    }
                    else
                    {
                        yaw = (((VeXformNode)item).orientation[0]) * (3.141592654f / 180.0f);
                        pitch = (((VeXformNode)item).orientation[1]) * (3.141592654f / 180.0f);
                        roll = (((VeXformNode)item).orientation[2]) * (3.141592654f / 180.0f);
                    }
                    scale = ((VeXformNode)item).scale;

                    x += x1;
                    y += y1;
                    z += z1;
                }

                if (item != null)
                {

                    float old_x = x, old_y = y, old_z = z;
                    float old_yaw = yaw, old_pitch = pitch, old_roll = roll;
                    float old_scale = scale;

                    System.Collections.IEnumerator enumerator = ((VeNode)item).EnumerateChildren();
                    bool parentBool = item is VeXformNode;
                    bool parentSelect = item is VeSelectNode;

                    if (enumerator.MoveNext())
                    {
                        object prevNode = null;
                        do
                        {
                            object curNode = enumerator.Current;

                            object nextNode = null;

                            object newParentNode = parentNode;
                            if (item is VeXformNode)
                                newParentNode = item;
                            else if ((item is VeSelectNode))
                                newParentNode = item;

                            if (enumerator.MoveNext())
                                nextNode = enumerator.Current;

                            if (prevNode != null && prevNode is VeXformNode)
                                parentBool = false;

                            CheckNode(temp, curNode, parentBool, nextNode, newParentNode, selectNodeParent ? true : parentSelect);

                            prevNode = curNode;

                            if (nextNode == null)
                                break;
                        } while (true);
                    }

                    if (parentNode is VeSelectNode && parentBool && !parentXform)
                    {
                        x = old_x;
                        y = old_y;
                        z = old_z;

                        yaw = old_yaw;
                        pitch = old_pitch;
                        roll = old_roll;

                        x -= x1;
                        y -= y1;
                        z -= z1;
                    }
                    else if(parentNode is VeSelectNode && next == null)
                    {
                        x = 0;
                        y = 0;
                        z = 0;

                        yaw = 0;
                        pitch = 0;
                        roll = 0;
                    }
                    else if(parentBool && next != null)
                    {
                        x = old_x;
                        y = old_y;
                        z = old_z;

                        if (((VeNode)next).WidgetID != ((VeNode)item).WidgetID)
                        {
                            yaw = 0;
                            pitch = 0;
                            roll = 0;
                        }
                        else
                        {
                            yaw = old_yaw;
                            pitch = old_pitch;
                            roll = old_roll;
                        }

                        x -= x1;
                        y -= y1;
                        z -= z1;
                    }
                    else
                    {
                        x = old_x;
                        y = old_y;
                        z = old_z;

                        yaw = old_yaw;
                        pitch = old_pitch;
                        roll = old_roll;

                        x -= x1;
                        y -= y1;
                        z -= z1;
                    }
                    
                }
            }
        }

        public static string[] GetTextureFile(string[] spPath, string basePath)
        {
            string ret = "goblin_ice.dds";
            System.Collections.Generic.List<string> strings = new System.Collections.Generic.List<string>();

            int i = 0;
            while (i < spPath.Length /*&& ret == "goblin_ice.dds"*/)
            {
                Eq2Reader reader = new Eq2Reader(new System.IO.FileStream(basePath + spPath[i], System.IO.FileMode.Open, System.IO.FileAccess.Read));
                VeBase sp = reader.ReadObject();
                reader.Close();

                if (sp is VeShaderPalette)
                {
                    bool found = false;
                    for (int s = 0; s < ((VeShaderPalette)sp).shaderNames.Length; s++)
                    {
                        String fileName = basePath + ((VeShaderPalette)sp).shaderNames[s];
                        fileName = fileName.Replace("/", "\\");
                        System.IO.StreamReader reader2 = new System.IO.StreamReader(fileName);
                        while (!reader2.EndOfStream)
                        {
                            string lineOrig = reader2.ReadLine();
                            if (lineOrig.Contains("name = \"@tex") && !lineOrig.Contains("Blend") && !lineOrig.Contains("UVSet"))
                            {
                                String line = reader2.ReadLine();
                                while (line.Length < 1)
                                    line = reader2.ReadLine();

                                line = line.Substring(line.IndexOf('"') + 1);
                                line = line.Substring(0, line.Length - 1);
                                ret = basePath + line;
                                strings.Add(ret);
                                found = true;
                                break;
                                //break;
                            }
                            if (found)
                                break;
                        }
                        reader2.Close();
                    }
                }
                i++;
            }

            if (strings.Count == 0)
                strings.Add(ret);


            return strings.ToArray();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsLoaded)
                return;

            //List<Vector3> MasterVertexList = new List<Vector3>();
            Dictionary<UInt32, List<Vector3>> MasterVertexList = new Dictionary<UInt32, List<Vector3>>();
            foreach (Model model in m_Models)
            {
                if  (model.WidgetID == 1253219127)
                {
                    int test = 0;
                }
                List<Vector3> VertexList = model.GetVertices();
                UInt32 grid = model.GridID;

                if (!MasterVertexList.ContainsKey(grid))
                    MasterVertexList[grid] = new List<Vector3>();

                List<Vector3> convertedVertices = new List<Vector3>();
                foreach(Vector3 vect in VertexList)
                {
                    Quaternion rotation = Quaternion.RotationYawPitchRoll(model.Rotation.X, model.Rotation.Y, model.Rotation.Z);
                    var matrix = Matrix.Identity;
                    Matrix.RotationQuaternion(ref rotation, out matrix);
                    Matrix scaled = Matrix.Multiply(matrix, Matrix.Scaling(model.Scale, model.Scale, model.Scale));

                    Vector3 result = Vector3.Add(Vector3.TransformNormal(vect, scaled), model.Position);
                    convertedVertices.Add(result);
                }
                MasterVertexList[grid].AddRange(convertedVertices);
            }

            float minX = float.NaN;
            float minZ = float.NaN;
            float maxX = float.NaN;
            float maxZ = float.NaN;
            foreach (KeyValuePair<UInt32, List<Vector3>> entry in MasterVertexList)
            {
                foreach (Vector3 v in entry.Value)
                {
                    if (float.IsNaN(minX))
                    {
                        minX = v.X;
                        maxX = v.X;
                        minZ = v.Z;
                        maxZ = v.Z;
                    }
                    else
                    {
                        if (v.X < minX)
                            minX = v.X;
                        if (v.X > maxX)
                            maxX = v.X;
                        if (v.Z < minZ)
                            minZ = v.Z;
                        if (v.Z > maxZ)
                            maxZ = v.Z;
                    }
                }
            }


            using (StreamWriter file = new StreamWriter(ZoneFile + AppendFileStr + ".obj"))
            {
                //   file.WriteLine(ZoneFile);
                //  file.WriteLine("Min");
                //   file.WriteLine(minX + " " + minZ);
                //  file.WriteLine("Max");
                //  file.WriteLine(maxX + " " + maxZ);

                //  file.WriteLine("Grid count");
                //  file.WriteLine(MasterVertexList.Count);
                //  file.WriteLine();

                List<string> indices = new List<string>();
                int count = 0;
                string buildStr = "";
                int curcount = 0;
                foreach (KeyValuePair<UInt32, List<Vector3>> entry in MasterVertexList)
                {
                    buildStr = "f ";
                   // file.WriteLine("Grid");
                   // file.WriteLine(entry.Key);

                   // file.WriteLine("Face count");
                   // file.WriteLine(entry.Value.Count);
                    foreach (Vector3 v in entry.Value)
                    {
                        if (curcount > 2)
                        {
                            buildStr += count;
                            indices.Add(buildStr);
                            buildStr = "f ";
                            curcount = 0;
                        }
                        else
                            buildStr += count + " ";

                        file.WriteLine("v " + v.X.ToString() + " " + v.Y.ToString()
                            + " " + v.Z.ToString());
                        count++;
                        curcount++;
                    }
                }
                foreach (string str in indices)
                {
                    file.WriteLine(str);
                }
                file.Close();
            }

            using (BinaryWriter file = new BinaryWriter(File.Open(ZoneFile + AppendFileStr + ".EQ2Map", FileMode.Create)))
            {
                file.Write(ZoneFile);
                file.Write(minX);
                file.Write(minZ);
                file.Write(maxX);
                file.Write(maxZ);
                file.Write(MasterVertexList.Count);
                foreach (KeyValuePair<UInt32, List<Vector3>> entry in MasterVertexList)
                {
                    file.Write(entry.Key);
                    file.Write(entry.Value.Count);
                    foreach (Vector3 v in entry.Value)
                    {
                        file.Write(v.X);
                        file.Write(v.Y);
                        file.Write(v.Z);
                    }
                }
                file.Close();
            }
            FileInfo fileToCompress = new FileInfo(ZoneFile + AppendFileStr + ".EQ2Map");

            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if ((File.GetAttributes(fileToCompress.FullName) &
                   FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                {
                    using (FileStream compressedFileStream = File.Create(ZoneFile + AppendFileStr + ".EQ2MapDeflated"))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                           CompressionMode.Compress))
                        {
                            originalFileStream.CopyTo(compressionStream);
                        }
                    }
                    FileInfo info = new FileInfo(ZoneFile + AppendFileStr + ".EQ2MapDeflated");
                    Console.WriteLine($"Compressed {fileToCompress.Name} from {fileToCompress.Length.ToString()} to {info.Length.ToString()} bytes.");
                }
            }

            if (sender != null)
               MessageBox.Show("Export Complete!");
        }


        private int LoadEnvXmlParseLiquid(string filename)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);

                var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("vdl", "Vdl");
                nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                XmlNode atmosphereNode = xmlDoc.SelectSingleNode("/vdl:VdlFile/vdl:Environment/vdl:iAtmosphere", nsmgr);
                if (atmosphereNode != null && Convert.ToInt32(atmosphereNode.InnerText) < 0)
                    return Convert.ToInt32(atmosphereNode.InnerText); // lava

                XmlNode liquidNode = xmlDoc.SelectSingleNode("/vdl:VdlFile/vdl:Environment/vdl:nLiquid", nsmgr);
                if (liquidNode != null)
                    return Convert.ToInt32(liquidNode.InnerText);
            }
            catch (Exception ex)
            {

            }
            return 0;
        }
    }
}
