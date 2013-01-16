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

namespace Pixelated
{
    /// <summary>
    /// PixelArt, this is just for fun. Seizure warning!.
    /// </summary>
    public class PixelArt : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        /// <summary>
        /// An array containing color information for each pixel on the screen. A Pixel at location (i, j) has corresponding color information
        /// located at index = i + (RESOLUTION_WIDTH * j).
        /// </summary>
        Color[] colors;

        /// <summary>
        /// The location of all pixels that are static (Pixels that are unchanged).
        /// </summary>
        Dictionary<Point, Color> staticPixels;

        Random rand;

        /// <summary>
        /// The screens resolution width.
        /// </summary>
        public const int RES_WIDTH = 540;
        /// <summary>
        /// The screens resolution height.
        /// </summary>
        public const int RES_HEIGHT = 405;

        /// <summary>
        /// The amount of time that has occurred since program start.
        /// </summary>
        public float timeElapsed = 0;

        /// <summary>
        /// The amount of time (in seconds) that the climax is shown.
        /// </summary>
        public float holdClimax = 20f;
        public float holdCt = 0; //simple counter for the holdClimax
        public bool climaxed = false; //true iff the cycle has reached climax

        /// <summary>
        /// The amount of time it takes to cycle back to the original state.
        /// </summary>
        public const float MAX_CYCLE_TIME = 80f;
        /// <summary>
        /// Saturation controls how strong of an influence a static pixel has on its neighbouring pixels. Varying
        /// saturation causes very different results.
        /// </summary>
        double saturation = 2;
        /// <summary>
        /// Static pixels are pixels that have a non-black default color and are unchanged.
        /// </summary>
        double numStaticPixels = RES_WIDTH*RES_HEIGHT * 0.01;

        /// <summary>
        /// Round calculations.
        /// </summary>
        bool rounding = false;

        /// <summary>
        /// Forces static pixels to be skipped and never modified. If it is false, these pixels will be modified 
        /// like any other pixel.
        /// </summary>
        bool keepStaticPixels = true;
       
        public PixelArt()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = RES_WIDTH;
            graphics.PreferredBackBufferHeight = RES_HEIGHT;
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Initialize the color array.
        /// </summary>
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            colors = new Color[RES_WIDTH * RES_HEIGHT];
            staticPixels = new Dictionary<Point, Color>();
            rand = new Random();
            for (int i = 0; i < RES_WIDTH; i++)
            {
                for (int j = 0; j < RES_HEIGHT; j++)
                {
                    //default color is black for all pixels
                    SetPixelColor(i, j, Color.Black);
                    //colors[i, j] = new Color(r.Next(256), r.Next(256), r.Next(256));
                }
            }

            CreateStaticPixels(rand);
            base.Initialize();
        }

        /// <summary>
        /// Sets the color of the pixel at location (i, j).
        /// </summary>
        /// <param name="i">The x coordinate of a pixel.</param>
        /// <param name="j">The y coordinate of a pixel.</param>
        /// <param name="c">The Color.</param>
        public void SetPixelColor(int i, int j, Color c)
        {
            colors[i + j * RES_WIDTH] = c;
        }

        /// <summary>
        /// Adds the static pixels to the color array. These are pixels that default to non-black and are unchanged.
        /// </summary>
        /// <param name="random">The random object.</param>
        public void CreateStaticPixels(Random random)
        {
            for (int i = 0; i < numStaticPixels; i++)
            {
                int x = 0, y = 0;
                do
                {
                    x = random.Next(RES_WIDTH);
                    y = random.Next(RES_HEIGHT);
                } while (staticPixels.ContainsKey(new Point(x, y)));
                SetPixelColor(x, y, new Color(random.Next(256), random.Next(256), random.Next(256)));
                staticPixels.Add(new Point(x, y), colors[x + y * RES_WIDTH]);
            }
        }

        /// <summary>
        /// Update the art.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (holdCt <= 0)
            {
                timeElapsed += gameTime.ElapsedGameTime.Milliseconds / 1000f;
                //cycle saturation from [-1, 3]
                saturation = 1f + Math.Sin((timeElapsed / MAX_CYCLE_TIME) * 2 * Math.PI) * 2f;
                
                //if saturation is close to 2 begin climax
                if (2 - saturation < 0.05 && !climaxed)
                {
                    holdCt = holdClimax;
                    saturation = 2;
                    climaxed = true;
                }

                if (saturation < 0) climaxed = false;
            }
            holdCt -= gameTime.ElapsedGameTime.Milliseconds / 1000f;
            UpdatePixels();
            base.Update(gameTime);
        }

        /// <summary>
        /// Validates if i and j are valid indices for the color array.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="j">The j.</param>
        /// <returns>True iff i and j are valid indices for the color array</returns>
        public bool validate(int i, int j)
        {
            return i >= 0 && j >= 0 && i < RES_WIDTH && j < RES_HEIGHT;
        }


        /// <summary>
        /// Get the neighbouring colors of the pixel located at (i, j).
        /// </summary>
        /// <param name="i">The x coordinate of the pixel.</param>
        /// <param name="j">The y coordinate of the pixel.</param>
        /// <returns></returns>
        public List<Color> GetNeighbouringColors(int i, int j)
        {
            List<Color> adjColors = new List<Color>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    if (validate(i + x, j + y))
                    {
                        adjColors.Add(colors[(i + x)  + (j + y) * RES_WIDTH]);
                    }
                }
            }
            return adjColors;
        }

        /// <summary>
        /// Updates the pixels by calculating each pixels new RGB.
        /// </summary>
        public void UpdatePixels()
        {
            for (int i = 0; i < RES_WIDTH; i++)
            {
                for (int j = 0; j < RES_HEIGHT; j++)
                {
                    if (keepStaticPixels && staticPixels.ContainsKey(new Point(i, j))) continue;
                    List<Color> adjColors = GetNeighbouringColors(i, j);

                    double r = colors[i + j * RES_WIDTH].R;
                    double g = colors[i + j * RES_WIDTH].G;
                    double b = colors[i + j * RES_WIDTH].B;

                    double rTot = 0, gTot = 0, bTot = 0;
                    //calculate the total r,g,b values of the adjacent pixels
                    for (int k = 0; k < adjColors.Count; k++)
                    {
                        rTot += adjColors[k].R;
                        gTot += adjColors[k].G;
                        bTot += adjColors[k].B;
                    }

                    r = r + (1.0 / adjColors.Count) * (rTot + (saturation - 2) - adjColors.Count * r) * saturation;
                    g = g + (1.0 / adjColors.Count) * (gTot + (saturation - 2) - adjColors.Count * g) * saturation;
                    b = b + (1.0 / adjColors.Count) * (bTot + (saturation - 2) - adjColors.Count * b) * saturation;

                    //with rounding
                    if (rounding)
                    {
                        SetPixelColor(i, j, new Color((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b)));
                    }
                    else
                    {
                        //truncation
                        SetPixelColor(i, j, new Color((int)r, (int)g, (int)b));
                    }
                }
            }

        }
        /// <summary>
        /// Draw the art.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            Texture2D texture = new Texture2D(GraphicsDevice, RES_WIDTH, RES_HEIGHT);
            //set the colors for each pixel on our texture
            texture.SetData<Color>(colors, 0, RES_WIDTH * RES_HEIGHT);
            spriteBatch.Begin();
            //draw the texture to the screen
            spriteBatch.Draw(texture, new Rectangle(0, 0, texture.Width, texture.Height), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
