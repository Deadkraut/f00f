﻿#region ::::Chancelog::::
/*
 *GameplayScreen.cs
 * 
 *
 * @author: Alexander Stoldt
 * @version: 1.0
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

        #endregion

        #region Fields

        //SpriteBatch mbatch; //Zeichnet die Lebenleist
        Effect effect;

        Model xwingModel;
        Model skyboxModel;
        Model targetModel;

        Texture2D[] skyboxTextures;


        int healthbarCon = 0; // Wird benötigt um die healthbar zu kontrolieren
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
        WeltraumSpiel game;
        GameTime gameti;    // Wird benötigt um an die Spielzeit für die Handelinput zu kommen

        SpriteBatch mBatch;

        Texture2D mHealthBar;

        #endregion  

        #region Initialization
        
        public GameplayScreen()
        {
            //game = new WeltraumSpiel(true);
        }
        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            effect = Content.Load<Effect>(@"Effects\effects");
            xwingModel = LoadModel(@"Models\xwing");
            targetModel = LoadModel(@"Models\target");

    
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);
 
            device = ScreenManager.GraphicsDevice;

            bulletTexture = Content.Load<Texture2D>(@"Textures\bullet1");
            skyboxModel = LoadModel(@"Models\Skybox\skybox", out skyboxTextures);



            gameFont = Content.Load<SpriteFont>(@"Fonts\gamefont");

            AddBoundaryBox();
            AddTargets();
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

            //Vector3[] boundaryPoints = new Vector3[2];
            //boundaryPoints[0] = new Vector3(1, 1, 1) * -dimension; // minValue
            //boundaryPoints[1] = new Vector3(1, 1, 1) * dimension; // maxValue

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
                    //healthbarDow();        //Noch debugen
                    return CollisionType.Target;
                }
            }

            // Es liegt keine Kollision vor
            return CollisionType.None;
        }

        //public void healthbarDow()
        //{
        //    if (game.Punktabzug >= 450)
        //    {
        //        Environment.Exit(0);
        //    }
        //    else if (healthbarCon == 1)
        //    {
        //        game.Punktabzug += 150; // Hier wird bei einer Kolision etwas von der Lebensleiste abgezogen
        //        System.Console.WriteLine("Der Punkestand ist" + game.Punktabzug); // Zum Testen
        //    }
        //    healthbarCon = 0;
        //}

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

                float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 500.0f * gameSpeed;
                MoveForward(ref xwingPosition, xwingRotation, moveSpeed);


                BoundingSphere xwingSpere = new BoundingSphere(xwingPosition, 0.04f);
                if (CheckCollision(xwingSpere) == CollisionType.Target)
                {
                    gameSpeed /= 1.1f;
                    healthbarCon = 1;       //Dient als Kontrolle für die Methode healthbarDow()
                    //healthbarDow();
                }
                if (CheckCollision(xwingSpere) == CollisionType.Boundary)
                {
                    xwingPosition = new Vector3(8, 1, -3);
                    xwingRotation = Quaternion.Identity;
                }

                UpdateCamera();
                UpdateBulletPositions(gameTime, moveSpeed);
            }

        }

        private void UpdateBulletPositions(GameTime gameTime, float moveSpeed)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            for (int i = 0; i < bulletList.Count; i++)
            {
                Bullet currentBullet = bulletList[i];
                if (currentTime - currentBullet.persistence > 10000)
                {
                    bulletList.RemoveAt(i);
                    i--;
                }
                else
                {
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
            DrawModel();
            DrawTargets();
            DrawBullets();

           gameti = gameTime;  //Wird benötigt um auf die Gametime in HandelInput zu zu greifen

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
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

                float turningSpeed = (float)gameti.ElapsedGameTime.TotalMilliseconds / 1000.0f; //Test
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

                Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRot) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot);
                xwingRotation *= additionalRot;
                MouseState state = Mouse.GetState();

                //if (keys.IsKeyDown(Keys.Space))
                if (state.LeftButton == ButtonState.Pressed)
                {
                    double currentTime = gameti.TotalGameTime.TotalMilliseconds;    //
                    if (currentTime - lastBulletTime > 100)
                    {
                        Bullet newBullet = new Bullet();
                        newBullet.position = xwingPosition;
                        newBullet.rotation = xwingRotation;
                        newBullet.persistence = currentTime; //Lebenszeit initialisieren
                        bulletList.Add(newBullet);

                        lastBulletTime = currentTime;
                    }
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