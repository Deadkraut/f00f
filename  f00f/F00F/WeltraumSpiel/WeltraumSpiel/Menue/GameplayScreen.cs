#region ::::Changelog::::
/*
 *GameplayScreen.cs
 * 
 *
 * @author: Alexander Stoldt
 * @version: 1.0
 * 
 * Diese Klasse wird benötigt um das Spiel auch spielen zu können
 * 
 * @author: Alexander Stoldt
 * @version: 1.1
 * Ich habe die Lebensleiste eingefügt und diese dann gebugfixt
 */
#endregion
#region Using Statements
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
#endregion

namespace WeltraumSpiel
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Enumerations & Structs

        enum CollisionType { None, Boundary, Target }

        struct Bullet
        {
            public Vector3 position;
            public Quaternion rotation;
            public double persistence;
        }

        #endregion

        #region Constants

        const int maxTargets = 500;
        const float dimension = 500; // Dimension der BoundaryBox
        private float turnMod = 0.75f;
        public int score = 0;

        #endregion

        #region Fields

        Effect effect;

        Model skyboxModel;
        Model targetModel;
        Model ship;


        Texture2D[] skyboxTextures;

        bool destroyed;

        int healthbarCon = 0; // Wird benötigt um die healthbar zu kontrolieren
        int punktabzug;
        float gameSpeed = 1.0f;

        List<BoundingSphere> targetList = new List<BoundingSphere>(); Texture2D bulletTexture;

        List<Bullet> bulletList = new List<Bullet>(); double lastBulletTime = 0;

        BoundingBox boundaryBox; // Box, die den Missionsbereich eingrenzt

        Quaternion xwingRotation = Quaternion.Identity;
        Quaternion cameraRotation = Quaternion.Identity;

        Vector3 lightDirection = new Vector3(3, -2, 5);
        Vector3 xwingPosition = new Vector3(8, 1, -3);
        Vector3 cameraPosition;
        Vector3 cameraUpDirection;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        ContentManager Content; //Lädt den Content
        SpriteFont gameFont;    //Schauen ob benötigt
        float pauseAlpha;



        SpriteBatch spriteBatch; //Ermöglicht das Zeichnen einer Gruppe von Sprites mithilfe derselben Einstellungen. 
        GraphicsDevice device;
        GameTime gameti;    // Wird benötigt um an die Spielzeit für die Handelinput zu kommen

        HUD.Crosshair crosshair;
        HUD.Score hudScore;

        Texture2D mHealthBar;
        Texture2D chTexture;

        SoundEffect weaponSound;
        SoundEffect targetExlposion;
        SoundEffect engineSound;
        SoundEffectInstance sefin;

        SoundEffect shipExplosion;
        Model[] destroyedShip;
        float leftWingRotation = 1.0f;
        float centerRotation = 1.0f;
        float rightWingRotation = 1.0f;
        double lastDestroyedTime = 0;
        Vector3 shipWingLeftPos;
        Vector3 shipCenterPos;
        Vector3 shipWingRightPos;
        Vector2 scorePosition = new Vector2(40,40);
        String scoreText = "Score";
        Color textColor = new Color(0, 0, 0, 127);


        #endregion

        #region Initialization

        public GameplayScreen()
        {
            destroyed = false;
        }
        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            effect = Content.Load<Effect>(@"Effects\effects");
            ship = Content.Load<Model>(@"Models\JaegerMK1");
            targetModel = Content.Load<Model>(@"Models\Asteroid");
            destroyedShip = new Model[] { Content.Load<Model>(@"Models\JaegerWingLeft"), Content.Load<Model>(@"Models\JaegerCenter"), Content.Load<Model>(@"Models\JaegerWingRight") };

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);
            mHealthBar = Content.Load<Texture2D>(@"Textures\healthBar") as Texture2D;

            device = ScreenManager.GraphicsDevice;

            bulletTexture = Content.Load<Texture2D>(@"Textures\bullet1");
            skyboxModel = LoadModel(@"Models\Skybox\skybox", out skyboxTextures);

            gameFont = Content.Load<SpriteFont>(@"Fonts\gamefont");

            chTexture = Content.Load<Texture2D>(@"Textures\crosshair");
            Rectangle newRectangle = new Rectangle(0, 0, 50, 50);
            crosshair = new HUD.Crosshair(chTexture, newRectangle, 4, ScreenManager.Game.Window.ClientBounds.Width, ScreenManager.Game.Window.ClientBounds.Height);
            hudScore = new HUD.Score(scoreText, scorePosition, spriteBatch, gameFont, device);

            AddBoundaryBox();
            AddTargets();

            engineSound = Content.Load<SoundEffect>(@"Sounds\EngineLoop");
            sefin = engineSound.CreateInstance();

            shipExplosion = Content.Load<SoundEffect>(@"Sounds\ShipExpl");

            targetExlposion = Content.Load<SoundEffect>(@"Sounds\TargetExpl");

            weaponSound = Content.Load<SoundEffect>(@"Sounds\Weapon");

            SpriteFont spriteFont = Content.Load<SpriteFont>(@"Fonts\gamefont");
            //spriteBatch.DrawString(spriteFont, scoreText + ": " + score, scorePosition, textColor);
            string scoreLabelText = "Score: ";
            hudScore = new HUD.Score(scoreLabelText, scorePosition, spriteBatch, gameFont, device);
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            Content.Unload();
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

        private void AddBoundaryBox()
        {
            Vector3[] boundaryPoints = new Vector3[8];
            boundaryPoints[0] = new Vector3(1, 1, 1) * dimension;
            boundaryPoints[1] = new Vector3(-1, 1, 1) * dimension;
            boundaryPoints[2] = new Vector3(-1, 1, -1) * dimension;
            boundaryPoints[3] = new Vector3(1, -1, -1) * dimension;
            boundaryPoints[4] = new Vector3(1, -1, 1) * dimension;
            boundaryPoints[5] = new Vector3(-1, -1, 1) * dimension;
            boundaryPoints[6] = new Vector3(1, 1, -1) * dimension;
            boundaryPoints[7] = new Vector3(-1, -1, -1) * dimension;

            boundaryBox = BoundingBox.CreateFromPoints(boundaryPoints);
        }

        private void AddTargets()
        {
            int boundaryWidth = (int)dimension;
            int boundaryLength = (int)dimension;

            Random random = new Random();

            while (targetList.Count < maxTargets)
            {
                int x = random.Next(boundaryWidth);
                int z = -random.Next(boundaryLength);
                float y = (float)random.Next(2000) / 1000f + 1;
                float radius = (float)random.Next(1000) / 1000f * 0.2f + 0.01f;

                BoundingSphere newTarget = new BoundingSphere(new Vector3(x, y, z), radius);

                if (CheckCollision(newTarget) == CollisionType.None)
                    targetList.Add(newTarget);
            }
        }
        #endregion

        #region Collision

        /// <summary>
        /// This function checks, if there's any collision with the
        /// passed BoundingSphere and the BoundaryBox or any target in
        /// the targetList and returns the accordingly CollisionType.
        /// </summary>
        /// <param name="sphere">Contains a BoundingSphere.</param>
        private CollisionType CheckCollision(BoundingSphere sphere)
        {
            // Überprüfe, ob sich die BoundingSphere außerhalb der BoundaryBox befindet
            if (boundaryBox.Contains(sphere) != ContainmentType.Contains)
            {
                return CollisionType.Boundary;
            }

            // Überprüfe für jedes Ziel in der targetList, ob eine Kollision mit der BoundingSphere vorliegt
            for (int i = 0; i < targetList.Count; i++)
            {
                if (targetList[i].Contains(sphere) != ContainmentType.Disjoint)
                {
                    targetList.RemoveAt(i);
                    i--;
                    AddTargets();
                    healthbarDow();


                    return CollisionType.Target;
                }
            }

            // Es liegt keine Kollision vor
            return CollisionType.None;
        }

        public void healthbarDow()
        {

            if (healthbarCon == 1)
            {
                punktabzug += 150; // Hier wird bei einer Kolision etwas von der Lebensleiste abgezogen
                System.Console.WriteLine("Der Punkestand ist " + punktabzug); // Zum Testen
                healthbarCon = 0;
                healthbarDow();
            }
            else if (punktabzug >= 600)
            {
                destroyed = true;
                sefin.Stop();
                ScreenManager.AddScreen(new GameOverMenuScreen(), ControllingPlayer);
            }

        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                        bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);



            if (IsActive)
            {
                if (!destroyed)
                {
                    if (sefin.State == SoundState.Stopped && sefin.IsLooped == false)
                    {
                        sefin.IsLooped = true;
                        sefin.Play();
                    }
                    float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 500.0f * gameSpeed;
                    MoveForward(ref xwingPosition, xwingRotation, moveSpeed);


                    BoundingSphere xwingSpere = new BoundingSphere(xwingPosition, 0.04f);
                    if (CheckCollision(xwingSpere) == CollisionType.Target)
                    {
                        if (gameSpeed > 1.0f)
                        {
                            gameSpeed /= 1.1f;
                            turnMod *= 1.1f;
                        }

                        healthbarCon = 1;       //Dient als Kontrolle für die Methode healthbarDow()
                        healthbarDow();
                        targetExlposion.Play();
                    }

                    if (CheckCollision(xwingSpere) == CollisionType.Boundary)
                    {
                        xwingPosition = new Vector3(8, 1, -3);
                        xwingRotation = Quaternion.Identity;
                    }
                    UpdateBulletPositions(gameTime, moveSpeed);
                    UpdateScore();
                }
            }
            if (destroyed)
            {
                double currentGameTime = gameTime.TotalGameTime.TotalMilliseconds;
                Random random = new Random();
                if (lastDestroyedTime == 0)
                {
                    shipWingLeftPos = xwingPosition;
                    shipCenterPos = xwingPosition;
                    shipWingRightPos = xwingPosition;
                    shipExplosion.Play();
                    lastDestroyedTime = currentGameTime;
                }
                if (currentGameTime - lastDestroyedTime > 10)
                {
                    float speed = 0.001f;

                    Vector3 addVector = Vector3.Transform(new Vector3(-1, 1, 1), xwingRotation);
                    shipWingLeftPos += addVector * speed;

                    addVector = Vector3.Transform(new Vector3(0, -1, 1), xwingRotation);
                    shipCenterPos += addVector * speed;


                    addVector = Vector3.Transform(new Vector3(1, 0, 1), xwingRotation);
                    shipWingRightPos += addVector * speed;
                }

                leftWingRotation += 0.1f;
                centerRotation += 0.1f;
                rightWingRotation += 0.1f;
            }
            UpdateCamera();
        }

        private void UpdateBulletPositions(GameTime gameTime, float moveSpeed)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            for (int i = 0; i < bulletList.Count; i++)
            {
                Bullet currentBullet = bulletList[i];
                if (currentTime - currentBullet.persistence > 2500)
                {
                    bulletList.RemoveAt(i);
                    i--;
                }
                else
                {
                    MoveForward(ref currentBullet.position, currentBullet.rotation, moveSpeed * 5.0f);
                    bulletList[i] = currentBullet;

                    BoundingSphere bulletSphere = new BoundingSphere(currentBullet.position, 0.05f);
                    CollisionType colType = CheckCollision(bulletSphere);
                    if (colType != CollisionType.None)
                    {
                        bulletList.RemoveAt(i);
                        i--;

                        if (colType == CollisionType.Target)
                            targetExlposion.Play();
                        score += 50;
                        if (gameSpeed < 5.0f)
                        {
                            turnMod /= 1.2f;
                            gameSpeed *= 1.2f;
                        }
                    }
                }
            }
        }

        private void UpdateScore()
        {
            Color color = Color.White;
            string scoreText = score.ToString();
            hudScore.Update(scoreText, color);
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
        public override void Draw(GameTime gameTime)
        {
            DrawSkybox();
            DrawTargets();
            hudScore.Draw();
            if (!destroyed)
            {
                DrawModel();
                DrawBullets();
                crosshair.Draw(spriteBatch);
                hudScore.Draw();


                //Hier wird die Lebensleiste erzeugt
                spriteBatch.Begin();
                //Draw the health for the health bar
                spriteBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.Window.ClientBounds.Width / 2 - mHealthBar.Width / 2,
                                                      500, mHealthBar.Width - punktabzug, 44), new Rectangle(0, 45, mHealthBar.Width, 44), Color.Red);
                //Draw the box around the health bar
                spriteBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.Window.ClientBounds.Width / 2 - mHealthBar.Width / 2,
                                                      500, mHealthBar.Width - punktabzug, 44), new Rectangle(0, 0, mHealthBar.Width, 44), Color.White);
                spriteBatch.End();
            }
            else
            {
                DrawDestroyedModel();
            }
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
            gameti = gameTime;  //Wird benötigt um auf die Gametime in HandelInput zu zu greifen
        }

        private void DrawModel()
        {
            Matrix worldMatrix = Matrix.CreateScale(0.02f, 0.02f, 0.02f) * Matrix.CreateRotationY(MathHelper.Pi / 2) * Matrix.CreateFromQuaternion(xwingRotation) * Matrix.CreateTranslation(xwingPosition);


            Matrix[] modelTransforms = new Matrix[ship.Bones.Count];
            ship.CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh mesh in ship.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();  // Beleuchtung aktivieren
                    effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();

            }
        }

        private void DrawTargets()
        {
            for (int i = 0; i < targetList.Count; i++)
            {
                Matrix worldMatrix = Matrix.CreateScale(targetList[i].Radius) * Matrix.CreateTranslation(targetList[i].Center);

                //Matrix[] targetTransforms = new Matrix[targetModel.Bones.Count];
                //targetModel.CopyAbsoluteBoneTransformsTo(targetTransforms);
                //foreach (ModelMesh modmesh in targetModel.Meshes)
                //{
                //    foreach (Effect currentEffect in modmesh.Effects)
                //    {
                //        currentEffect.CurrentTechnique = currentEffect.Techniques["Colored"];
                //        currentEffect.Parameters["xWorld"].SetValue(targetTransforms[modmesh.ParentBone.Index] * worldMatrix);
                //        currentEffect.Parameters["xView"].SetValue(viewMatrix);
                //        currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                //        currentEffect.Parameters["xEnableLighting"].SetValue(true);
                //        currentEffect.Parameters["xLightDirection"].SetValue(lightDirection);
                //        currentEffect.Parameters["xAmbient"].SetValue(0.5f);
                //    }
                //    modmesh.Draw();
                //}
                Matrix[] modelTransforms = new Matrix[targetModel.Bones.Count];
                targetModel.CopyAbsoluteBoneTransformsTo(modelTransforms);

                foreach (ModelMesh mesh in targetModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();  // Beleuchtung aktivieren
                        effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                        effect.View = viewMatrix;
                        effect.Projection = projectionMatrix;
                    }
                    mesh.Draw();

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

        private void DrawDestroyedModel()
        {
            // Linker Ship-Wing zeichnen
            int i = 0;
            Matrix worldMatrix = Matrix.CreateScale(0.02f, 0.02f, 0.02f) * Matrix.CreateRotationY(MathHelper.Pi / 2) * Matrix.CreateRotationX(leftWingRotation) * Matrix.CreateFromQuaternion(xwingRotation) * Matrix.CreateTranslation(shipWingLeftPos);

            Matrix[] modelTransforms = new Matrix[destroyedShip[i].Bones.Count];
            destroyedShip[i].CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh mesh in destroyedShip[i].Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();  // Beleuchtung aktivieren
                    effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();

            }
            // Ship-Center zeichnen
            i++;
            worldMatrix = Matrix.CreateScale(0.02f, 0.02f, 0.02f) * Matrix.CreateRotationY(MathHelper.Pi / 2) * Matrix.CreateRotationX(centerRotation) * Matrix.CreateFromQuaternion(xwingRotation) * Matrix.CreateTranslation(shipCenterPos);

            modelTransforms = new Matrix[destroyedShip[i].Bones.Count];
            destroyedShip[i].CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh mesh in destroyedShip[i].Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();  // Beleuchtung aktivieren
                    effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();

            }
            // Rechter Ship-Wing zeichnen
            i++;
            worldMatrix = Matrix.CreateScale(0.02f, 0.02f, 0.02f) * Matrix.CreateRotationY(MathHelper.Pi / 2) * Matrix.CreateRotationX(rightWingRotation) * Matrix.CreateFromQuaternion(xwingRotation) * Matrix.CreateTranslation(shipWingRightPos);

            modelTransforms = new Matrix[destroyedShip[i].Bones.Count];
            destroyedShip[i].CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh mesh in destroyedShip[i].Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();  // Beleuchtung aktivieren
                    effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();

            }
        }


        private void drawScore()
        {
 
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            if (input.IsPauseGame(ControllingPlayer))
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                float leftRightRot = 0;
                float leftRightRoll = 0;
                float upDownRot = 0;

                

                float turningSpeed = (float)gameti.ElapsedGameTime.TotalMilliseconds / 1000.0f; //Test
                turningSpeed *= 1.6f * gameSpeed;
                KeyboardState keys = Keyboard.GetState();
                if (keys.IsKeyDown(Keys.D) || keys.IsKeyDown(Keys.Right))
                    leftRightRot += turningSpeed * turnMod;
                if (keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.Left))
                    leftRightRot -= turningSpeed * turnMod;

                if (keys.IsKeyDown(Keys.Q))
                    leftRightRoll -= turningSpeed * turnMod * 1.5f;
                if (keys.IsKeyDown(Keys.E))
                    leftRightRoll += turningSpeed * turnMod * 1.5f;

                if (keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.Down))
                    upDownRot += turningSpeed * turnMod;
                if (keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Up))
                    upDownRot -= turningSpeed * turnMod;

                MouseState state = Mouse.GetState();
                int x = 0;
                int y = 0;
                Mouse.SetPosition(x, y);//Hier wird die Maus Position auf x=0 und y= 0 gesetzt
                int mausX = state.X;
                int mausY = state.Y;
                //Hier wird geprüft ob die Maus bewegt wurde
                if (mausX != x)
                {
                    mousePlay(mausX, mausY);
                }
                else if (mausY != y)
                {
                    mousePlay(mausX, mausY);
                }

                    

               
                Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRoll) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot) * Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), leftRightRot);
                xwingRotation *= additionalRot;
               
             if (state.LeftButton == ButtonState.Pressed || keys.IsKeyDown(Keys.Space))
                {
                    double currentTime = gameti.TotalGameTime.TotalMilliseconds;    //
                    if (currentTime - lastBulletTime > 150)
                    {
                        Bullet newBullet = new Bullet();
                        newBullet.position = xwingPosition;
                        newBullet.rotation = xwingRotation;
                        newBullet.persistence = currentTime; //Lebenszeit initialisieren
                        bulletList.Add(newBullet);

                        lastBulletTime = currentTime;
                        weaponSound.Play();
                    }
                }
            }
        }

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            KeyboardState keys = Keyboard.GetState();
            MouseState state = Mouse.GetState();
            if (keys.IsKeyDown(Keys.LeftShift) || state.RightButton == ButtonState.Pressed)
            {
                Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
                position += addVector * speed * 2;
                sefin.Volume = 1.0f;
            }
            if (keys.IsKeyUp(Keys.LeftShift))
            {
                Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
                position += addVector * speed;
                sefin.Volume = 0.5f;
            }
        }

        //Dies Methode wird benötigt um denn X-Wing mit der Maus zu steuern

        private void mousePlay(int xx, int yy)
        {
            float leftRightRot = 0;
            float leftRightRoll = 0;
            float upDownRot = 0;

            float turningSpeed = (float)gameti.ElapsedGameTime.TotalMilliseconds / 1000.0f; //Test
            turningSpeed *= 1.2f * gameSpeed;

            //Hier fliegt der Xwing nach Rechts
            if (xx > 0)
            {
                leftRightRot += turningSpeed * turnMod;
            }
            //Hier fliegt der Xwing nach Links
            if (xx < 0)
            {
                leftRightRot -= turningSpeed * turnMod;
            }

            //Hier fliegt der Xwing nach unten
            if (yy > 0)
            {
                upDownRot += turningSpeed * turnMod;
            }
            //Hier fliegt der Xwing nach Oben
            if (yy < 0)
            {
                upDownRot -= turningSpeed * turnMod;
            }


            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRoll) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot) * Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), leftRightRot);
            xwingRotation *= additionalRot;
        }


        #endregion
    }
}
