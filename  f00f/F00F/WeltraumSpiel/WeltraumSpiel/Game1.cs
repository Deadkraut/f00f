using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace WeltraumSpiel
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Enumerations & Structs

        enum CollisionType { None, Building, Boundary, Target }

        struct Bullet
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        #endregion

        #region Screens

        //The screens and the current screen
        Menue.IntroScreen introScreen; // Greift auf IntroScreen im Ordner Menue zu
        Menue.MainScreen mainScreen;   // Greift auf MainScreen im Ordner Menue zu
        Menue.Screen currentScreen;    // Gibt den Aktuelen Screen an

        #endregion

        #region Constants

        const int screenWidth = 800;
        const int screenHeigth = 600;
        const int maxTargets = 500;

        #endregion

        #region Fields

        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch; //Ermöglicht das Zeichnen einer Gruppe von Sprites mithilfe derselben Einstellungen. 
        GraphicsDevice device;

        Healthbar healthbar;

        Effect effect;

        Model xwingModel;
        Model skyboxModel;
        Model targetModel;

        Texture2D[] skyboxTextures;
        
        int healthbarCon = 0; // Wird benötigt um die healthbar zu kontrolieren
        float gameSpeed = 1.0f;

        List<BoundingSphere> targetList = new List<BoundingSphere>(); Texture2D bulletTexture;

        List<Bullet> bulletList = new List<Bullet>(); double lastBulletTime = 0;

        Quaternion xwingRotation = Quaternion.Identity;
        Quaternion cameraRotation = Quaternion.Identity;

        Vector3 lightDirection = new Vector3(3, -2, 5);
        Vector3 xwingPosition = new Vector3(8, 1, -3);
        Vector3 cameraPosition;
        Vector3 cameraUpDirection;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        #endregion

        #region Initialization

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            healthbar = new Healthbar(this);
            Components.Add(healthbar);// Hier wird die Lebensanzeige als Gamecomponent hinzugefügt
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //this.IsMouseVisible = true;        //Macht die Maus sichtbar
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Fly in Space";
            lightDirection.Normalize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            device = graphics.GraphicsDevice;
            effect = Content.Load<Effect>(@"Effects\effects");
            xwingModel = LoadModel(@"Models\xwing");
            targetModel = LoadModel(@"Models\target");

            bulletTexture = Content.Load<Texture2D>(@"Textures\bullet1");
            skyboxModel = LoadModel(@"Models\Skybox\skybox", out skyboxTextures);

            //Initialize the various screens in the game
            introScreen = new Menue.IntroScreen(this.Content, new EventHandler(introScreenEvent));
            mainScreen = new Menue.MainScreen(this.Content, new EventHandler(mainScreenEvent));
            currentScreen = introScreen;

            AddTargets();
          
        }

        private Model LoadModel(string assetName)
        {

            Model newModel = Content.Load<Model>(assetName); foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
            return newModel;
        }

        private Model LoadModel(string assetName, out Texture2D[] textures)
        {

            Model newModel = Content.Load<Model>(assetName);
            textures = new Texture2D[newModel.Meshes.Count];
            int i = 0;
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    textures[i++] = currentEffect.Texture;

            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();

            return newModel;
        }

        private void AddTargets()
        {
            int cityWidth = screenWidth;
            int cityLength = screenHeigth;

            Random random = new Random();

            while (targetList.Count < maxTargets)
            {
                int x = random.Next(cityWidth);
                int z = -random.Next(cityLength);
                float y = (float)random.Next(2000) / 1000f + 1;
                float radius = (float)random.Next(1000) / 1000f * 0.2f + 0.01f;

                BoundingSphere newTarget = new BoundingSphere(new Vector3(x, y, z), radius);

                if (CheckCollision(newTarget) == CollisionType.None)
                    targetList.Add(newTarget);
            }
        }

        #endregion

        #region Unload Content

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #endregion

        #region Screen Management

        //This event fires when the Controller detect screen is returning control back to the main game class
        //Überstetzen
        public void introScreenEvent(object obj, EventArgs e)
        {
            currentScreen = mainScreen;
        }

        //This event is fired when the Title screen is returning control back to the main game class
        //Überstetzen
        public void mainScreenEvent(object obj, EventArgs e)
        {
            //Switch to the controller detect screen, the Title screen is finished being displayed
            currentScreen = introScreen;
        }

        #endregion

        public void healthbarDow()
        {
            if (healthbar.Punktabzug >= 450)
            {
                Environment.Exit(0);
            }
            else if (healthbarCon == 1)
            {
                healthbar.Punktabzug += 150; // Hier wird bei einer Kolision etwas von der Lebensleiste abgezogen
                System.Console.WriteLine("Der Punkestand ist" + healthbar.Punktabzug); // Zum Testen
            }
            healthbarCon = 0;
        }

        #region Collision

        private CollisionType CheckCollision(BoundingSphere sphere)
        {
            //for (int i = 0; i < buildingBoundingBoxes.Length; i++)
            //    if (buildingBoundingBoxes[i].Contains(sphere) != ContainmentType.Disjoint)
            //        return CollisionType.Building;

            //if (completeCityBox.Contains(sphere) != ContainmentType.Contains)
            //{
            //    return CollisionType.Boundary;
            //}

            for (int i = 0; i < targetList.Count; i++)
            {
                if (targetList[i].Contains(sphere) != ContainmentType.Disjoint)
                {
                    targetList.RemoveAt(i);
                    i--;
                    AddTargets();
                    healthbarDow();        //Noch debugen
                    return CollisionType.Target;
                }
            }

            return CollisionType.None;
        }

        #endregion

        #region Update

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            //By taking advantage of Polymorphism, we can call update on the current screen class, 
            //but the Update in the subclass is the one that will be executed.
            currentScreen.update(gameTime);

            ProcessKeyboard(gameTime);
            float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 500.0f * gameSpeed;
            MoveForward(ref xwingPosition, xwingRotation, moveSpeed);


            BoundingSphere xwingSpere = new BoundingSphere(xwingPosition, 0.04f);
            if (CheckCollision(xwingSpere) == CollisionType.Target)
            {
                xwingPosition = new Vector3(8, 1, -3);
                //xwingRotation = Quaternion.Identity;
                gameSpeed /= 1.1f;
                healthbarDow();
            }

            UpdateCamera();
            UpdateBulletPositions(moveSpeed);
            base.Update(gameTime);

        }

        private void UpdateBulletPositions(float moveSpeed)
        {
            for (int i = 0; i < bulletList.Count; i++)
            {
                Bullet currentBullet = bulletList[i];
                MoveForward(ref currentBullet.position, currentBullet.rotation, moveSpeed * 2.0f);
                bulletList[i] = currentBullet;

                BoundingSphere bulletSphere = new BoundingSphere(currentBullet.position, 0.05f);
                CollisionType colType = CheckCollision(bulletSphere);
                if (colType != CollisionType.None)
                {
                    bulletList.RemoveAt(i);
                    i--;

                    if (colType == CollisionType.Target)
                        gameSpeed *= 1.05f;
                }
            }
        }

        private void UpdateCamera()
        {

            cameraRotation = Quaternion.Lerp(cameraRotation, xwingRotation, 0.1f);

            Vector3 campos = new Vector3(0, 0.1f, 0.6f);
            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(cameraRotation));
            campos += xwingPosition;

            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(cameraRotation));

            viewMatrix = Matrix.CreateLookAt(campos, xwingPosition, camup);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.2f, 500.0f);

            cameraPosition = campos;
            cameraUpDirection = camup;
        }

        #endregion

        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);

            spriteBatch.Begin();
            //Again, using Polymorphism, we can call draw on teh current screen class
            //and the Draw in the subclass is the one that will be executed.
            currentScreen.Draw(spriteBatch);

            spriteBatch.End();

            DrawSkybox();
            DrawModel();
            DrawTargets();
            DrawBullets();

            base.Draw(gameTime);
        }

        private void DrawModel()
        {
            Matrix worldMatrix = Matrix.CreateScale(0.0005f, 0.0005f, 0.0005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(xwingRotation) * Matrix.CreateTranslation(xwingPosition);

            Matrix[] xwingTransforms = new Matrix[xwingModel.Bones.Count];
            xwingModel.CopyAbsoluteBoneTransformsTo(xwingTransforms);
            foreach (ModelMesh mesh in xwingModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Colored"];
                    currentEffect.Parameters["xWorld"].SetValue(xwingTransforms[mesh.ParentBone.Index] * worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xLightDirection"].SetValue(lightDirection);
                    currentEffect.Parameters["xAmbient"].SetValue(0.5f);
                }
                mesh.Draw();
            }
        }

        private void DrawTargets()
        {
            for (int i = 0; i < targetList.Count; i++)
            {
                Matrix worldMatrix = Matrix.CreateScale(targetList[i].Radius) * Matrix.CreateTranslation(targetList[i].Center);

                Matrix[] targetTransforms = new Matrix[targetModel.Bones.Count];
                targetModel.CopyAbsoluteBoneTransformsTo(targetTransforms);
                foreach (ModelMesh modmesh in targetModel.Meshes)
                {
                    foreach (Effect currentEffect in modmesh.Effects)
                    {
                        currentEffect.CurrentTechnique = currentEffect.Techniques["Colored"];
                        currentEffect.Parameters["xWorld"].SetValue(targetTransforms[modmesh.ParentBone.Index] * worldMatrix);
                        currentEffect.Parameters["xView"].SetValue(viewMatrix);
                        currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                        currentEffect.Parameters["xEnableLighting"].SetValue(true);
                        currentEffect.Parameters["xLightDirection"].SetValue(lightDirection);
                        currentEffect.Parameters["xAmbient"].SetValue(0.5f);
                    }
                    modmesh.Draw();
                }
            }
        }

        private void DrawBullets()
        {
            if (bulletList.Count > 0)
            {
                VertexPositionTexture[] bulletVertices = new VertexPositionTexture[bulletList.Count * 6];
                int i = 0;
                foreach (Bullet currentBullet in bulletList)
                {
                    Vector3 center = currentBullet.position;

                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 0));

                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                }

                effect.CurrentTechnique = effect.Techniques["PointSprites"];
                effect.Parameters["xWorld"].SetValue(Matrix.Identity);
                effect.Parameters["xProjection"].SetValue(projectionMatrix);
                effect.Parameters["xView"].SetValue(viewMatrix);
                effect.Parameters["xCamPos"].SetValue(cameraPosition);
                effect.Parameters["xTexture"].SetValue(bulletTexture);
                effect.Parameters["xCamUp"].SetValue(cameraUpDirection);
                effect.Parameters["xPointSpriteSize"].SetValue(0.1f);

                device.BlendState = BlendState.Additive;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, bulletVertices, 0, bulletList.Count * 2);
                }

                device.BlendState = BlendState.Opaque;
            }
        }

        private void DrawSkybox()
        {
            SamplerState ss = new SamplerState();
            ss.AddressU = TextureAddressMode.Clamp;
            ss.AddressV = TextureAddressMode.Clamp;
            device.SamplerStates[0] = ss;

            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = false;
            device.DepthStencilState = dss;

            Matrix[] skyboxTransforms = new Matrix[skyboxModel.Bones.Count];
            skyboxModel.CopyAbsoluteBoneTransformsTo(skyboxTransforms);
            int i = 0;
            foreach (ModelMesh mesh in skyboxModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = skyboxTransforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(xwingPosition);
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(viewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(skyboxTextures[i++]);
                }
                mesh.Draw();
            }

            dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            device.DepthStencilState = dss;
        }

        #endregion

        #region Handle Input

        private void ProcessKeyboard(GameTime gameTime)
        {
            float leftRightRot = 0;

            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            turningSpeed *= 1.6f * gameSpeed;
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right))
                leftRightRot += turningSpeed;
            if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left))
                leftRightRot -= turningSpeed;

            float upDownRot = 0;
            if (keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.Down))
                upDownRot += turningSpeed;
            if (keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up))
                upDownRot -= turningSpeed;
            if (keys.IsKeyDown(Keys.Escape))
            {
                Environment.Exit(0);// Beendet das Programm
            }

            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRot) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot);
            xwingRotation *= additionalRot;
            MouseState state = Mouse.GetState();

            //if (keys.IsKeyDown(Keys.Space))
            if (state.LeftButton == ButtonState.Pressed)
            {
                double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
                if (currentTime - lastBulletTime > 100)
                {
                    Bullet newBullet = new Bullet();
                    newBullet.position = xwingPosition;
                    newBullet.rotation = xwingRotation;
                    bulletList.Add(newBullet);

                    lastBulletTime = currentTime;
                }
            }
        }

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
            position += addVector * speed;
        }

        #endregion

        
    }
}
