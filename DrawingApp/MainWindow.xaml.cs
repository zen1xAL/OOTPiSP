using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingApp
{
    public partial class MainWindow : Window
    {
        private List<Shape> shapes = new List<Shape>();
        private Shape currentShape;
        private Point startPoint;
        private string currentMode = "";
        private bool isDrawing = false;
        private UndoRedoManager undoRedoManager;

        public MainWindow()
        {
            InitializeComponent();
            undoRedoManager = new UndoRedoManager(shapes);


            UpdateShapeSelector();

            ShapeFactory.ShapeRegistered += ShapeFactory_ShapeRegistered;

            DrawingCanvas.MouseDown += DrawingCanvas_MouseDown;
            DrawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            DrawingCanvas.MouseUp += DrawingCanvas_MouseUp;
            DrawingCanvas.MouseRightButtonDown += DrawingCanvas_MouseRightButtonDown;
            UndoButton.Click += UndoButton_Click;
            RedoButton.Click += RedoButton_Click;
            ShapeSelector.SelectionChanged += ShapeSelector_SelectionChanged;
            SaveButton.Click += SaveButton_Click;
            LoadButton.Click += LoadButton_Click;
            LoadPluginButton.Click += LoadPluginButton_Click;
        }

        private void ShapeFactory_ShapeRegistered(object sender, string shapeName)
        {
            // update selector when add a new figure
            Dispatcher.Invoke(() =>
            {
                if (!ShapeSelector.Items.Cast<ComboBoxItem>().Any(item => item.Content.ToString() == shapeName))
                {
                    ShapeSelector.Items.Add(new ComboBoxItem { Content = shapeName });
                }
            });
        }

        private void UpdateShapeSelector()
        {
            ShapeSelector.Items.Clear();
            foreach (var shapeName in ShapeFactory.GetRegisteredShapes())
            {
                ShapeSelector.Items.Add(new ComboBoxItem { Content = shapeName });
            }
            if (ShapeSelector.Items.Count > 0)
            {
                ShapeSelector.SelectedIndex = 0;
            }
        }

        private void LoadPluginButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "DLL files (*.dll)|*.dll";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ShapeFactory.LoadPluginFromFile(openFileDialog.FileName);
                    MessageBox.Show("Плагин успешно загружен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке плагина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShapeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDrawing && currentShape != null)
            {
                isDrawing = false;
                currentShape = null;
                DrawingCanvas.ReleaseMouseCapture();
                RedrawCanvas();
            }
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            DrawingCanvas.Focus();

            Point clickPoint = e.GetPosition(DrawingCanvas);

            if (!isDrawing || currentShape?.IsMultiPointShape == true)
            {
                startPoint = clickPoint;
                string selectedShape = (ShapeSelector.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrEmpty(selectedShape))
                {
                    return;
                }

                currentMode = selectedShape;

                bool justStartedDrawing = false;

                if (currentShape == null || !currentShape.IsMultiPointShape)
                {
                    if (!isDrawing)
                    {
                        currentShape = ShapeFactory.CreateShape(selectedShape);
                        if (currentShape == null)
                        {
                            isDrawing = false;
                            return;
                        }
                        currentShape.Initialize(startPoint);
                        isDrawing = true;
                        justStartedDrawing = true;
                    }
                    else if (currentShape != null && currentShape.IsMultiPointShape)
                    {
                        currentShape = ShapeFactory.CreateShape(selectedShape);
                        if (currentShape == null) { isDrawing = false; return; }
                        currentShape.Initialize(startPoint);
                        justStartedDrawing = true;
                    }
                    else
                    {
                        isDrawing = false;
                        currentShape = null;
                        return;
                    }
                }
                else if (currentShape.IsMultiPointShape)
                {
                    if (!isDrawing)
                    {
                        isDrawing = true;
                        justStartedDrawing = true;
                    }
                    (currentShape as dynamic).Points.Add(startPoint);
                }

                if (isDrawing && currentShape != null)
                {
                    SetShapeProperties(currentShape);

                    if (justStartedDrawing)
                    {
                        DrawingCanvas.CaptureMouse();
                    }
                    RedrawCanvas();
                }
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && currentShape != null)
            {
                Point currentPoint = e.GetPosition(DrawingCanvas);
                currentShape.Update(currentPoint);
                SetShapeProperties(currentShape);

                if (currentShape.IsMultiPointShape)
                {
                    RedrawCanvasWithPreview(currentPoint);
                }
                else
                {
                    RedrawCanvas();
                }
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && isDrawing && currentShape != null && !currentShape.IsMultiPointShape)
            {
                currentShape.Update(e.GetPosition(DrawingCanvas));
                currentShape.FinalizeShape();
                FinalizeShape();
                isDrawing = false;

                if (DrawingCanvas.IsMouseCaptured)
                {
                    DrawingCanvas.ReleaseMouseCapture();
                }
            }
        }

        private void DrawingCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing && currentShape != null && currentShape.IsMultiPointShape)
            {
                bool canFinalize = !(currentShape is PolylineShape poly && poly.Points.Count < 2);

                if (canFinalize)
                {
                    currentShape.FinalizeShape();
                    FinalizeShape();
                    isDrawing = false;

                    if (DrawingCanvas.IsMouseCaptured)
                    {
                        DrawingCanvas.ReleaseMouseCapture();
                    }
                }
            }
        }

        private void SetShapeProperties(Shape shape)
        {
            double thickness = ThicknessSlider.Value;
            string strokeColorName = (ColorSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            string fillColorName = (FillColorSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            Color strokeColor = ColorManager.GetColorFromName(strokeColorName);
            Color fillColor = ColorManager.GetColorFromName(fillColorName);

            shape.SetProperties(thickness, strokeColor, fillColor);
        }

        private void FinalizeShape()
        {
            if (currentShape == null) return;

            SetShapeProperties(currentShape);
            shapes.Add(currentShape);
            undoRedoManager.AddShape(currentShape);
            currentShape = null;
            currentMode = "";
            RedrawCanvas();
        }

        private void RedrawCanvas()
        {
            DrawingCanvas.Children.Clear();
            foreach (var shape in shapes)
            {
                DrawingCanvas.Children.Add(shape.Draw());
            }
            if (currentShape != null)
            {
                DrawingCanvas.Children.Add(currentShape.Draw());
            }
        }

        private void RedrawCanvasWithPreview(Point previewPoint)
        {
            DrawingCanvas.Children.Clear();

            foreach (var shape in shapes)
            {
                DrawingCanvas.Children.Add(shape.Draw());
            }

            if (currentShape != null)
            {
                UIElement currentElement = currentShape.Draw();
                DrawingCanvas.Children.Add(currentElement);

                if (currentShape.IsMultiPointShape && (currentShape as dynamic).Points.Count > 0)
                {
                    string strokeColorName = (ColorSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
                    Color strokeColor = ColorManager.GetColorFromName(strokeColorName);
                    double thickness = ThicknessSlider.Value;

                    var previewElements = currentShape.DrawPreview(previewPoint, thickness, strokeColor);
                    foreach (var element in previewElements)
                    {
                        DrawingCanvas.Children.Add(element);
                    }
                }
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (isDrawing)
            {
                currentShape = null;
                isDrawing = false;
                RedrawCanvas();
                return;
            }

            undoRedoManager.Undo();
            RedrawCanvas();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            undoRedoManager.Redo();
            RedrawCanvas();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == true)
            {
                ShapeSerializer serializer = new ShapeSerializer();
                serializer.SaveShapes(shapes, saveFileDialog.FileName);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                ShapeSerializer serializer = new ShapeSerializer();

                // clear old list
                shapes.Clear();

                // load and add figures from json
                var loadedShapes = serializer.LoadShapes(openFileDialog.FileName);
                shapes.AddRange(loadedShapes);

                undoRedoManager.Reset();
                foreach (var shape in loadedShapes)
                {
                    undoRedoManager.AddShape(shape);
                }

                RedrawCanvas();
            }
        }
    }
}