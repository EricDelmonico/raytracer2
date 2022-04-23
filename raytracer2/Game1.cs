using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monogame.Imgui.Renderer;
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

        private int threads = 24;
        private int currentRow = 0;

        private int samplesPerPass = 1;

        private bool enableMovement = true;
        private double movementSpeed = 1;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            windowWidth = 1024;
            windowHeight= 1024;
            _graphics.PreferredBackBufferWidth = windowWidth;
            _graphics.PreferredBackBufferHeight = windowHeight;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            imGuiRenderer = new ImGuiRenderer(this);
            imGuiRenderer.RebuildFontAtlas();

            renderTarget = new Camera(new Vec3(0, 0, -1), 256, 256, _graphics);

            allRenderers = new List<IRenderer>();
            allRenderers.Add(new RaytracingRenderer(samplesPerPass));
            allRenderers.Add(new RenderTargetTester());

            allRendererNames = allRenderers.Select((r) => r.GetType().Name).ToArray();

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

                    Vector2 diff = (prevMousePos.ToVector2() - mousePos.ToVector2());
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

            if (renderRealtime)
            {
                currentTask = currentTask.Where((t) => !t.IsCompleted).ToList();
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
                    ImGui.InputInt("Width", ref windowWidth);
                    ImGui.InputInt("Height", ref windowHeight);

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
                    }
                }

                ImGui.Combo("Current Renderer", ref currentRenderer, allRendererNames, allRendererNames.Length);

                if (ImGui.Button("Clear Target"))
                {
                    renderTarget.Clear();
                }

                ImGui.Checkbox("Render?", ref renderRealtime);

                ImGui.Checkbox("Enable Movement?", ref enableMovement);

                ImGui.DragInt("Threads", ref threads, 1, 1, 64);

                ImGui.InputInt("Samples Per Pass", ref samplesPerPass);
                allRenderers[currentRenderer].SamplesPerPass = samplesPerPass;

                ImGui.InputDouble("Movement Speed", ref movementSpeed);

                imGuiRenderer.AfterLayout();
            }

            base.Draw(gameTime);
        }
    }
}
