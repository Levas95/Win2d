// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SimpleSample
{
    /// <summary>
    /// Draws some graphics using Win2D
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int countX = 100;
        private const int countY = 100;
        public MainPage()
        {
            var listObj = new List<Tuple<int,int,string>>();
            for(int i = 0;i< countX;i++)
            {
                for (int j = 0; j < countY; j++)
                {
                    listObj.Add(new Tuple<int, int, string>(i, j, $"Object({i},{j})"));
                }
            }
            canvasHeight = countY * gridSize;
            canvasWidth = countX * gridSize;
            Edjes = new ObservableCollection<Tuple<int, int, string>>(listObj);
            this.InitializeComponent();
        }

        public ObservableCollection<Tuple<int, int, string>> Edjes;
        public int canvasHeight { get; set; }
        public int canvasWidth { get; set; }

        /// <summary>
        /// отрисовка протухла для регионов
        /// </summary>
        private void canvasControl_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            var invalidatedRegions = args.InvalidatedRegions;

            foreach (var region in invalidatedRegions)
            {
                DrawRegion(sender, region);
            }
        }
        const int gridSize = 70;

        #region resources
        CanvasTextFormat textFormat = new CanvasTextFormat()
        {
            HorizontalAlignment = CanvasHorizontalAlignment.Right,
            VerticalAlignment = CanvasVerticalAlignment.Bottom,
            FontFamily = "Comic Sans MS",
            FontSize = 48
        };

        CanvasTextFormat coordFormat = new CanvasTextFormat()
        {
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Center,
            FontSize = 10
        };

        private CanvasCachedGeometry cachedGeometry1 = null;

        private CanvasCachedGeometry getCashedValve15(ICanvasResourceCreator resourceCreator)
        {
            if(cachedGeometry1==null)
            {
                var radius = 15;
                cachedGeometry1 = CanvasCachedGeometry.CreateFill(CreateValve(resourceCreator, radius));
            }
            return cachedGeometry1;
        }

        private CanvasCachedGeometry cachedGeometry2 = null;
        private CanvasCachedGeometry getCashedValve16(ICanvasResourceCreator resourceCreator)
        {
            if (cachedGeometry2 == null)
            {
                var radius = 16;
                cachedGeometry2 = CanvasCachedGeometry.CreateFill(CreateValve(resourceCreator, radius));
            }
            return cachedGeometry2;
        }

        private CanvasGeometry CreateValve(ICanvasResourceCreator resourceCreator, float r)
        {
            var pathBuilder = new CanvasPathBuilder(resourceCreator);
            pathBuilder.BeginFigure(0, 0);
            pathBuilder.AddLine(0, 2 * r);
            pathBuilder.AddLine(2 * r, 1 * r);
            pathBuilder.EndFigure(CanvasFigureLoop.Closed);
            var geo0 = CanvasGeometry.CreatePath(pathBuilder);
            pathBuilder = new CanvasPathBuilder(resourceCreator);
            pathBuilder.BeginFigure(2 * r, 1 * r);
            pathBuilder.AddLine(4 * r, 0 * r);
            pathBuilder.AddLine(4 * r, 2 * r);
            pathBuilder.EndFigure(CanvasFigureLoop.Closed);
            var geo1 = CanvasGeometry.CreatePath(pathBuilder);
            var geo2 = CanvasGeometry.CreateRectangle(resourceCreator, 0, 0, 2 * r, r);
            var geometry = CanvasGeometry.CreateGroup(resourceCreator, new CanvasGeometry[] { geo0, geo1 });
            var res = geometry.CombineWith(geo2, Matrix3x2.CreateTranslation(1f * r, 0.5f * r), CanvasGeometryCombine.Union);
            return res.Transform(Matrix3x2.CreateTranslation(-2 * r, -1 * r));
        }
        #endregion
        /// <summary>
        /// отрисовка участка канваса
        /// </summary>
        private void DrawRegion(CanvasVirtualControl sender, Rect region)
        {
            var textt = new CanvasTextLayout(sender,
                "Everytime drawing text", textFormat, 1000, 0);

            using (var ds = sender.CreateDrawingSession(region))
            {
                ds.Clear(NextColor());

                var left = ((int)(region.X / gridSize) - 2) * gridSize;
                var top = ((int)(region.Y / gridSize) - 2) * gridSize;
                var right = ((int)((region.X + region.Width) / gridSize) + 2) * gridSize;
                var bottom = ((int)((region.Y + region.Height) / gridSize) + 2) * gridSize;

                for (var x = left; x <= right; x += gridSize)
                {
                    for (var y = top; y <= bottom; y += gridSize)
                    {
                        var curObj = Edjes.FirstOrDefault(xx => xx.Item1 == x/gridSize && xx.Item2 == y / gridSize);
                        if(curObj!=null)
                        {
                            var pos = new Vector2((float)x + 150, (float)y + 150);

                            ds.DrawCachedGeometry(getCashedValve16(sender), pos, Colors.Black);
                            ds.DrawCachedGeometry(getCashedValve15(sender), pos, Colors.LightGray);
                            ds.DrawText(curObj.Item3, pos, Colors.DarkBlue, coordFormat);
                        }
                    }
                }
            }
        }

        private void DrawTextWithBackground(CanvasDrawingSession ds, CanvasTextLayout layout, double x, double y)
        {
            var backgroundRect = layout.LayoutBounds;
            backgroundRect.X += x;
            backgroundRect.Y += y;

            backgroundRect.X -= 20;
            backgroundRect.Y -= 20;
            backgroundRect.Width += 40;
            backgroundRect.Height += 40;

            ds.FillRectangle(backgroundRect, NextColor());
            ds.DrawTextLayout(layout, (float)x, (float)y, Colors.Black);
        }

        static Color[] colors =
        {
            Colors.SandyBrown,
            Colors.Cornsilk,
            Colors.LightPink,
            Colors.Bisque,
            Colors.LightSalmon,
            Colors.LightGreen,
            Colors.BlanchedAlmond,
            Colors.Wheat,
            Colors.Honeydew
        };

        int lastColorIndex = 0;

        private Color NextColor()
        {
            var color = colors[lastColorIndex];

            lastColorIndex = (lastColorIndex + 1) % colors.Length;

            return color;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs args)
        {
            // Explicitly remove references to allow the Win2D controls to get garbage collected
            canvasControl.RemoveFromVisualTree();
            canvasControl = null;
        }
    }
}
