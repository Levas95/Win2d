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
        private const int countX = 200;
        private const int countY = 50;
        public MainPage()
        {
            var listObj = new List<Tuple<int, int, string>>();
            for (int i = 0; i < countX; i++)
            {
                for (int j = 0; j < countY; j++)
                {
                    listObj.Add(new Tuple<int, int, string>(i, j, $"Object({i},{j})"));
                }
            }
            Edjes = new ObservableCollection<Tuple<int, int, string>>(listObj);
            this.InitializeComponent();
            canvasHeight = (int)canvasControl.Height;
            canvasWidth = (int)canvasControl.Width;
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
        const int Xoffset = 150;
        const int Yoffset = 150;
        #region resources

        CanvasTextFormat textFormat = new CanvasTextFormat()
        {
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Center,
            FontSize = 10
        };

        private CanvasCachedGeometry cachedGeometry1 = null;

        private CanvasCachedGeometry getCashedValve15(ICanvasResourceCreator resourceCreator)
        {
            if (cachedGeometry1 == null)
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
                        var curObj = Edjes.FirstOrDefault(xx => xx.Item1 == x / gridSize && xx.Item2 == y / gridSize);
                        if (curObj != null)
                        {
                            var pos = new Vector2(x + Xoffset, y + Yoffset);

                            ds.DrawCachedGeometry(getCashedValve16(sender), pos, Colors.Black);
                            ds.DrawCachedGeometry(getCashedValve15(sender), pos, Colors.LightGray);
                            ds.DrawText(curObj.Item3, pos, Colors.DarkBlue, textFormat);
                            DrawSelectedObject(ds);
                        }
                    }
                }
            }
        }

        private void DrawSelectedObject(CanvasDrawingSession ds, Tuple<int, int, string> obj = null)
        {
            if (selected != null)
            {
                if (obj == null)
                {
                    obj = Edjes.FirstOrDefault(xx => xx.Item1 == selected.Item1 && xx.Item2 == selected.Item2);
                }
                var pos = new Vector2(obj.Item1 * gridSize + Xoffset, obj.Item2 * gridSize + Yoffset);
                ds.FillRectangle(new Rect(pos.X, pos.Y, 100, 50),Colors.DarkGray);
                ds.DrawRectangle(new Rect(pos.X, pos.Y, 100, 50),Colors.LightSlateGray,1f);
                ds.DrawText(obj.Item3 + "\n#some text#\n#some text#", new Vector2(pos.X+50,pos.Y+ 25), Colors.Black, textFormat);
            }
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

        private Vector2 leftClickPos = new Vector2(int.MaxValue, int.MaxValue);
        private void canvasControl_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var pointerPoint = e.GetCurrentPoint(canvasControl);
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                leftClickPos = pointerPoint.Position.ToVector2();
            }
            SelectionCancel();
        }

        private Tuple<int, int> selected = null;

        const int owerwriteSize = gridSize;

        private void SelectObject(Vector2 pos)
        {
            var x = pos.X - Xoffset - gridSize / 2f;
            var y = pos.Y - Yoffset - gridSize / 2f;
            var xi = (int)(x / gridSize) + 1;
            var yi = (int)(y / gridSize) + 1;
            var curObj = Edjes.FirstOrDefault(xx => xx.Item1 == xi && xx.Item2 == yi );
            if (curObj != null)
            {
                selected = new Tuple<int, int>(curObj.Item1, curObj.Item2);
                DrawRegion(canvasControl, new Rect(Math.Max(0,pos.X - owerwriteSize), Math.Max(0,pos.Y - owerwriteSize),
                    Math.Min(pos.X + owerwriteSize,canvasWidth), Math.Min(pos.Y + owerwriteSize,canvasHeight)));
            }
        }

        private void SelectionCancel()
        {
            if (selected != null)
            {
                var x = selected.Item1 * gridSize + Xoffset;
                var y = selected.Item2 * gridSize + Yoffset;
                selected = null;
                DrawRegion(canvasControl, new Rect(Math.Max(0, x - owerwriteSize), Math.Max(0, y - owerwriteSize),
                     Math.Min(x + owerwriteSize, canvasWidth), Math.Min(y + owerwriteSize, canvasHeight)));
            }
        }

        private void canvasControl_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var pointerPoint = e.GetCurrentPoint(canvasControl);
            var pos = pointerPoint.Position.ToVector2();
            if (leftClickPos == pos)
            {
                SelectObject(pos);
            }
        }
    }
}
