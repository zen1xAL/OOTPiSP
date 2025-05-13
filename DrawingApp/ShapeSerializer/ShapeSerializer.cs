using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DrawingApp
{
    public class ShapeSerializer
    {
        public void SaveShapes(List<Shape> shapes, string filePath)
        {
            var serializedShapes = new List<Dictionary<string, object>>();
            foreach (var shape in shapes)
            {
                var shapeData = new Dictionary<string, object>
                {

                    // save the full name of the type include assemmbly name for load plugins correctly
                    { "Type", shape.GetType().AssemblyQualifiedName },
                    { "Data", shape.GetSerializationData() }
                };
                serializedShapes.Add(shapeData);
            }
            string json = JsonConvert.SerializeObject(serializedShapes, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public List<Shape> LoadShapes(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var serializedShapes = JsonConvert.DeserializeObject<List<JObject>>(json);
            var shapes = new List<Shape>();

            foreach (var shapeData in serializedShapes)
            {
                string typeName = (string)shapeData["Type"];
                var data = shapeData["Data"].ToObject<Dictionary<string, object>>();

                Type shapeType = Type.GetType(typeName);
                if (shapeType == null)
                {
                    Console.WriteLine($"Тип {typeName} не найден напрямую. Проверяем загруженные сборки...");
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        Console.WriteLine($"Проверяем сборку: {assembly.FullName}");

                        // take only type name with namespace
                        shapeType = assembly.GetType(typeName.Split(',')[0].Trim());
                        if (shapeType != null)
                        {
                            Console.WriteLine($"Тип найден в сборке: {assembly.FullName}");
                            break;
                        }
                    }
                }

                if (shapeType == null)
                {
                    throw new Exception($"Неизвестный тип фигуры: {typeName}");
                }

                Shape shape = (Shape)Activator.CreateInstance(shapeType);
                shape.SetSerializationData(data);
                shapes.Add(shape);
            }

            return shapes;
        }
    }
}