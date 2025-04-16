using System;
using System.Collections.Generic;
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

            // Загружаем плагины из директории "Plugins"
            string pluginsDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            ShapeFactory.LoadPlugins(pluginsDirectory);

            // Очищаем ShapeSelector перед добавлением новых элементов
            ShapeSelector.Items.Clear();

            // Заполняем ShapeSelector доступными фигурами
            foreach (var shapeName in ShapeFactory.GetRegisteredShapes())
            {
                ShapeSelector.Items.Add(new ComboBoxItem { Content = shapeName });
            }

            // Устанавливаем первую фигуру по умолчанию, если список не пуст
            if (ShapeSelector.Items.Count > 0)
            {
                ShapeSelector.SelectedIndex = 0;
            }

            DrawingCanvas.MouseDown += DrawingCanvas_MouseDown;
            DrawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            DrawingCanvas.MouseUp += DrawingCanvas_MouseUp;
            DrawingCanvas.MouseRightButtonDown += DrawingCanvas_MouseRightButtonDown;
            UndoButton.Click += UndoButton_Click;
            RedoButton.Click += RedoButton_Click;
            ShapeSelector.SelectionChanged += ShapeSelector_SelectionChanged;
        }

        private void ShapeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDrawing && currentShape != null)
            {
                // Отменяем текущее рисование при смене фигуры
                isDrawing = false;
                currentShape = null;
                DrawingCanvas.ReleaseMouseCapture(); // Освобождаем захват
                RedrawCanvas();
            }
        }


        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Игнорируем, если не левая кнопка
            if (e.LeftButton != MouseButtonState.Pressed) return;

            // Захватываем фокус, чтобы обработка KeyDown работала надежнее
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

                bool justStartedDrawing = false; // Флаг, чтобы захватить мышь один раз

                // Если начинаем новую фигуру ИЛИ добавляем точку к многоточечной
                if (currentShape == null || !currentShape.IsMultiPointShape)
                {
                    // Если не рисовали до этого, создаем новую фигуру
                    if (!isDrawing)
                    {
                        currentShape = ShapeFactory.CreateShape(selectedShape);
                        if (currentShape == null) // Проверка, если фабрика вернула null
                        {
                            isDrawing = false;
                            return;
                        }
                        currentShape.Initialize(startPoint);
                        isDrawing = true; // Начинаем рисование
                        justStartedDrawing = true; // Помечаем, что только что начали
                    }
                    else if (currentShape != null && currentShape.IsMultiPointShape)
                    {
                        // Это случай, когда была активна многоточечная фигура,
                        // но пользователь выбрал не-многоточечную.
                        // Надо завершить старую и начать новую (или просто начать новую).
                        currentShape = ShapeFactory.CreateShape(selectedShape);
                        if (currentShape == null) { isDrawing = false; return; }
                        currentShape.Initialize(startPoint);
                        isDrawing = true; // Начинаем рисование новой
                        justStartedDrawing = true; // Помечаем, что только что начали
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
                    // Добавляем точку к существующей многоточечной фигуре
                    if (!isDrawing) 
                    {
                        isDrawing = true;
                        justStartedDrawing = true;
                    }
                    (currentShape as dynamic).Points.Add(startPoint);
                }

                // Применяем свойства и перерисовываем
                if (isDrawing && currentShape != null)
                {
                    SetShapeProperties(currentShape);
                    // Захватываем мышь, если только что начали рисование (новой фигуры или нового сегмента многоточечной)
                    if (justStartedDrawing)
                    {
                        DrawingCanvas.CaptureMouse();
                    }
                    RedrawCanvas(); // Для многоточечных обновит добавленную точку
                }
            }
        }


        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
           
            if (isDrawing && currentShape != null) 
            {
                Point currentPoint = e.GetPosition(DrawingCanvas);
                currentShape.Update(currentPoint); // Обновляем геометрию фигуры (например, конечную точку для линии/прямоугольника)
                SetShapeProperties(currentShape); // Обновляем свойства на случай их изменения во время рисования

                if (currentShape.IsMultiPointShape)
                {
                    // Для многоточечных вызываем RedrawCanvasWithPreview,
                    // который нарисует и фигуру, и линию предпросмотра до курсора
                    RedrawCanvasWithPreview(currentPoint);
                }
                else
                {
                    // Для обычных фигур просто перерисовываем все, включая обновленную текущую фигуру
                    RedrawCanvas();
                }
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Реагируем только на отпускание ЛЕВОЙ кнопки и если мышь была захвачена ЭТИМ элементом
            if (e.ChangedButton == MouseButton.Left && DrawingCanvas.IsMouseCaptured)
            {
                DrawingCanvas.ReleaseMouseCapture();

                // Завершаем только НЕ многоточечные фигуры левой кнопкой
                if (isDrawing && currentShape != null && !currentShape.IsMultiPointShape)
                {
                    currentShape.Update(e.GetPosition(DrawingCanvas)); // Финальное обновление координат перед фиксацией
                    currentShape.FinalizeShape(); 
                    FinalizeShape();          
                    isDrawing = false;       
                }
                
            }
            else if (e.ChangedButton == MouseButton.Left && isDrawing && currentShape != null && !currentShape.IsMultiPointShape)
            {
                // Если мышь не была захвачена, но мы вроде как рисовали не-многоточечную фигуру,
                // то тоже завершаем ее
                currentShape.Update(e.GetPosition(DrawingCanvas));
                currentShape.FinalizeShape();
                FinalizeShape();
                isDrawing = false;
                       }
        }

        private void DrawingCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing && currentShape != null && currentShape.IsMultiPointShape)
            {
                bool canFinalize = true;
         
                if (currentShape is PolylineShape poly && poly.Points.Count < 2)
                {
                    canFinalize = false; // Не завершать ломаную линию из одной точки
                }
               
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
            
            // Устанавливаем свойства текущей фигуры
            shape.SetProperties(thickness, strokeColor, fillColor);
        }

        private void FinalizeShape()
        {
            if (currentShape == null) return;

            SetShapeProperties(currentShape);
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
    }
}