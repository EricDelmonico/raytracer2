using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monogame.Imgui.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace raytracer2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private ImGuiRenderer imGuiRenderer;

        private IRenderTarget renderTarget;

        private List<IRenderer> allRenderers;
        private string[] allRendererNames;
        private int currentRenderer;

        private int windowWidth;
        private int windowHeight;

        private bool renderRealtime = true;

        private List<Task> currentTask = new List<Task>();

        private Point prevMousePos;
        private Point mousePos;

        private Camera cam => renderTarget as Camera;

        private int threads = 1;
        private int currentRow = 0;

        private int samplesPerPass = 10;

        private bool savingImage = false;

        private bool enableMovement = true;
        private double movementSpeed = 1;

        string imageName = "render";

        private string[] allWorldNames;
        // Keep track of one camera per scene
        private Camera[] cameras;
        private int currentWorldIndex;
        private int prevWorldIndex;
        private bool changingWorlds = false;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            windowWidth = 1024;
            windowHeight = 1024;
            _graphics.PreferredBackBufferWidth = windowWidth;
            _graphics.PreferredBackBufferHeight = windowHeight;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            imGuiRenderer = new ImGuiRenderer(this);
            imGuiRenderer.RebuildFontAtlas();

            allRenderers = new List<IRenderer>();
            RaytracingRenderer rt = new RaytracingRenderer(samplesPerPass);
            allRenderers.Add(rt);
            allRenderers.Add(new RenderTargetTester());

            allRendererNames = allRenderers.Select((r) => r.GetType().Name).ToArray();

            allWorldNames = Enum.GetNames(typeof(Worlds));
            currentWorldIndex = 0;

            // Create a camera for each world.. unfortunately must be hard coded. camera at index 0 is the default camera
            cameras = new[]
            {
                new Camera(new Vec3(0, 0, -1), new Vec3(0, 0, -1), 512, 512, _graphics, 60),
                new Camera(new Vec3(0, 0, -1), new Vec3(278, 278, -800), 512, 512, _graphics, 40.0),
                new Camera(new Vec3(0, 0, -1), new Vec3(0, 0, -7.5), 512, 512, _graphics, 40.0)
            };
            renderTarget = cameras[currentWorldIndex + 1];

            rt.SetWorld(allWorldNames[currentWorldIndex]);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            MouseState mouse = Mouse.GetState();
            KeyboardState kbState = Keyboard.GetState();

            if (enableMovement)
            {
                if (mouse.RightButton == ButtonState.Pressed)
                {
                    mousePos = mouse.Position;

                    Vector2 diff = (mousePos.ToVector2() - prevMousePos.ToVector2());
                    diff.X /= windowWidth;
                    diff.Y /= windowHeight;
                    cam.ChangeForwardVec(diff);
                }
                else
                {
                    mousePos = mouse.Position;
                    prevMousePos = mouse.Position;
                }

                if (kbState.IsKeyDown(Keys.W))
                {
                    cam.Move(0.01 * movementSpeed, MoveAxis.ForwardBack);
                }
                if (kbState.IsKeyDown(Keys.A))
                {
                    cam.Move(-0.01 * movementSpeed, MoveAxis.LeftRight);
                }
                if (kbState.IsKeyDown(Keys.S))
                {
                    cam.Move(-0.01 * movementSpeed, MoveAxis.ForwardBack);
                }
                if (kbState.IsKeyDown(Keys.D))
                {
                    cam.Move(0.01 * movementSpeed, MoveAxis.LeftRight);
                }
                if (kbState.IsKeyDown(Keys.E))
                {
                    cam.Move(0.01 * movementSpeed, MoveAxis.UpDown);
                }
                if (kbState.IsKeyDown(Keys.Q))
                {
                    cam.Move(-0.01 * movementSpeed, MoveAxis.UpDown);
                }
            }

            currentTask = currentTask.Where((t) => !t.IsCompleted).ToList();
            if (renderRealtime && !savingImage && !changingWorlds)
            {
                if (currentTask.Count < threads)
                {
                    for (int i = 0; i < threads - currentTask.Count; i++)
                    {
                        int copy = currentRow;
                        currentTask.Add(Task.Run(() => allRenderers[currentRenderer].RenderTo(renderTarget, copy)));
                        currentRow++;
                        currentRow %= renderTarget.Height;
                    }
                }
            }

            prevMousePos = mousePos;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            renderTarget.Draw(_spriteBatch);

            // Draw UI
            {
                imGuiRenderer.BeforeLayout(gameTime);

                // Allow modification of resolution
                if (ImGui.CollapsingHeader("Resolution"))
                {
                    ImGui.InputInt("W_Width", ref windowWidth);
                    ImGui.InputInt("W_Height", ref windowHeight);

                    // If we changed window width or height to a reasonable number, make sure to resize
                    if ((windowWidth > 400 && windowWidth != _graphics.PreferredBackBufferWidth)
                        || (windowHeight > 300 && windowHeight != _graphics.PreferredBackBufferHeight))
                    {
                        _graphics.PreferredBackBufferWidth = windowWidth;
                        _graphics.PreferredBackBufferHeight = windowHeight;
                        _graphics.ApplyChanges();
                    }
                }

                if (ImGui.CollapsingHeader("Render Resolution"))
                {
                    int width = renderTarget.Width;
                    int height = renderTarget.Height;
                    ImGui.InputInt("Width", ref width);
                    ImGui.InputInt("Height", ref height);

                    if (width != renderTarget.Width || height != renderTarget.Height)
                    {
                        renderTarget.Resize(width, height);
                        // Restart rendering
                        currentRow = 0;
                    }
                }

                ImGui.Combo("Current Renderer", ref currentRenderer, allRendererNames, allRendererNames.Length);

                if (ImGui.Button("Clear Target"))
                {
                    renderTarget.Clear();
                    // Restart rendering
                    currentRow = 0;
                }

                ImGui.Checkbox("Render?", ref renderRealtime);

                // Allow saving of the image
                ImGui.InputText("Image Name", ref imageName, 20);
                if (ImGui.Button("Save"))
                {
                    Task.Run(() =>
                    {
                        // Wait here until rendering and any other saving stops
                        while (savingImage)
                        {
                            Task.Delay(100);
                        }
                        savingImage = true;
                        while (currentTask.Count > 0)
                        {
                            Task.Delay(100);
                        }

                        // Save the image
                        renderTarget.SaveImage(imageName + ".png");

                        // No longer saving image
                        savingImage = false;
                    });
                }

                ImGui.Checkbox("Enable Movement?", ref enableMovement);

                ImGui.DragInt("Threads", ref threads, 1, 1, 64);

                ImGui.InputInt("Samples Per Pass", ref samplesPerPass);
                allRenderers[currentRenderer].SamplesPerPass = samplesPerPass;

                ImGui.InputDouble("Movement Speed", ref movementSpeed);

                var cam = renderTarget as Camera;
                if (cam != null)
                {
                    double fov = cam.FOV;
                    ImGui.InputDouble("FOV", ref fov);
                    cam.FOV = Math.Clamp(fov, 10.0, 180.0);
                }

                var rt = allRenderers[currentRenderer] as RaytracingRenderer;
                if (rt != null)
                {
                    ImGui.Combo("World", ref currentWorldIndex, allWorldNames, allWorldNames.Length);
                    if (prevWorldIndex != currentWorldIndex)
                    {
                        // Save index so this thread doesn't use the possibly changing currentWorldIndex
                        int saveIndex = currentWorldIndex;
                        Task.Run(() =>
                        {
                            // Wait until any world changing has finished
                            while (changingWorlds)
                            {
                                Task.Delay(100);
                            }
                            changingWorlds = true;
                            // Wait until rendering has finished
                            while (currentTask.Count > 0)
                            {
                                Task.Delay(100);
                            }

                            rt.SetWorld(allWorldNames[saveIndex]);
                            if (saveIndex + 1 < cameras.Length && saveIndex + 1 > 0)
                                renderTarget = cameras[saveIndex + 1];
                            else
                                renderTarget = cameras[0];
                            changingWorlds = false;
                        });
                    }
                    prevWorldIndex = currentWorldIndex;

                    if (ImGui.CollapsingHeader("Scene Objects"))
                    {
                        var objects = rt.CurrentWorld.objects;
                        int sphereNum = 0;
                        int translateNum = 0;
                        foreach (var obj in objects)
                        {
                            switch (obj)
                            {
                                case Sphere sphere:
                                    sphereNum++;
                                    if (ImGui.CollapsingHeader($"Sphere {sphereNum}"))
                                    {
                                        System.Numerics.Vector3 pos = new System.Numerics.Vector3((float)sphere.Center.x, (float)sphere.Center.y, (float)sphere.Center.z);
                                        ImGui.InputFloat3($"Sphere {sphereNum} Position", ref pos);
                                        sphere.Center = new Vec3(pos.X, pos.Y, pos.Z);

                                        double rad = sphere.Radius;
                                        ImGui.InputDouble($"Sphere {sphereNum} Radius", ref rad);
                                        sphere.Radius = rad;
                                    }
                                    break;
                                case Translate translate:
                                    translateNum++;
                                    if (ImGui.CollapsingHeader($"Translate: {translateNum}"))
                                    {
                                        System.Numerics.Vector3 pos = new System.Numerics.Vector3((float)translate.Offset.x, (float)translate.Offset.y, (float)translate.Offset.z);
                                        ImGui.InputFloat3($"Translate: {translateNum} Position", ref pos);
                                        translate.Offset = new Vec3(pos.X, pos.Y, pos.Z);
                                    }
                                    break;
                            }
                        }
                    }
                }

                imGuiRenderer.AfterLayout();
            }

            base.Draw(gameTime);
        }
    }
}
