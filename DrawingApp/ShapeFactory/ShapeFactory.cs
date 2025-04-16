using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

namespace DrawingApp
{
    public static class ShapeFactory
    {
        private static readonly Dictionary<string, Type> shapeTypes = new Dictionary<string, Type>();

        public static void RegisterShape(string shapeName, Type shapeType)
        {
            if (string.IsNullOrEmpty(shapeName))
            {
                throw new ArgumentException("Название фигуры не может быть пустым или null.", nameof(shapeName));
            }
            if (shapeType == null)
            {
                throw new ArgumentNullException(nameof(shapeType));
            }
            if (!typeof(Shape).IsAssignableFrom(shapeType))
            {
                throw new ArgumentException($"Тип {shapeType.Name} должен быть производным от Shape.", nameof(shapeType));
            }

            shapeTypes[shapeName] = shapeType;
        }

        public static Shape CreateShape(string shapeName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(shapeName))
            {
                throw new ArgumentException("Название фигуры не может быть пустым или null.", nameof(shapeName));
            }

            if (shapeTypes.TryGetValue(shapeName, out Type shapeType))
            {
                try
                {
                    var constructor = shapeType.GetConstructors()
                        .FirstOrDefault(c => c.GetParameters().Length == parameters.Length);

                    if (constructor == null)
                    {
                        throw new InvalidOperationException($"Не найден подходящий конструктор для типа {shapeType.Name} с {parameters.Length} параметрами.");
                    }

                    return (Shape)constructor.Invoke(parameters);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Ошибка при создании фигуры {shapeName}: {ex.Message}", ex);
                }
            }

            throw new ArgumentException($"Неизвестный тип фигуры: {shapeName}");
        }

        public static Shape CreateShape(string shapeName)
        {
            return CreateShape(shapeName, Array.Empty<object>());
        }

        public static Type GetShapeType(string shapeName)
        {
            if (string.IsNullOrEmpty(shapeName))
            {
                throw new ArgumentException("Название фигуры не может быть пустым или null.", nameof(shapeName));
            }

            if (shapeTypes.TryGetValue(shapeName, out Type shapeType))
            {
                return shapeType;
            }

            throw new ArgumentException($"Неизвестный тип фигуры: {shapeName}");
        }

        public static List<string> GetRegisteredShapes()
        {
            return shapeTypes.Keys.ToList();
        }

        public static bool IsShapeRegistered(string shapeName)
        {
            return !string.IsNullOrEmpty(shapeName) && shapeTypes.ContainsKey(shapeName);
        }

        public static void LoadPlugins(string pluginsDirectory)
        {
            if (string.IsNullOrEmpty(pluginsDirectory))
            {
                throw new ArgumentException("Путь к директории плагинов не может быть пустым.", nameof(pluginsDirectory));
            }

            if (!Directory.Exists(pluginsDirectory))
            {
                Directory.CreateDirectory(pluginsDirectory);
                Console.WriteLine($"Директория {pluginsDirectory} создана, плагины отсутствуют.");
                return;
            }

            var dllFiles = Directory.GetFiles(pluginsDirectory, "*.dll");
            Console.WriteLine($"Найдено {dllFiles.Length} DLL-файлов в {pluginsDirectory}: {string.Join(", ", dllFiles)}");

            foreach (var dllPath in dllFiles)
            {
                try
                {
                    Console.WriteLine($"Попытка загрузить плагин: {dllPath}");
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    var shapeClasses = assembly.GetTypes()
                        .Where(t => typeof(Shape).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

                    var shapeClassList = shapeClasses.ToList();
                    Console.WriteLine($"Найдено {shapeClassList.Count} классов, унаследованных от Shape, в {dllPath}: {string.Join(", ", shapeClassList.Select(t => t.FullName))}");

                    foreach (var shapeType in shapeClassList)
                    {
                        string shapeName = shapeType.Name.Replace("Shape", "");
                        Console.WriteLine($"Регистрируем фигуру: {shapeName} (тип: {shapeType.FullName})");
                        RegisterShape(shapeName, shapeType);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке плагина {dllPath}: {ex.Message}");
                    if (ex is ReflectionTypeLoadException reflectionEx)
                    {
                        foreach (var loaderEx in reflectionEx.LoaderExceptions)
                        {
                            Console.WriteLine($"LoaderException: {loaderEx.Message}");
                        }
                    }
                }
            }
        }

        static ShapeFactory()
        {
            RegisterShape("Line", typeof(LineShape));
            RegisterShape("Rectangle", typeof(RectangleShape));
            RegisterShape("Ellipse", typeof(EllipseShape));
            RegisterShape("Polygon", typeof(PolygonShape));
            RegisterShape("Polyline", typeof(PolylineShape));
        }
    }
}