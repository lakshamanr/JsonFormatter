using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace JsonFormatterApp.Services
{
    public class ConversionService
    {
        /// <summary>
        /// Convert JSON to XML
        /// </summary>
        public string JsonToXml(string jsonText, string rootElementName = "root")
        {
            try
            {
                var jToken = JToken.Parse(jsonText);
                var xmlDoc = new XDocument(new XElement(rootElementName));
                JsonToXmlElement(jToken, xmlDoc.Root!);
                return xmlDoc.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert JSON to XML: {ex.Message}", ex);
            }
        }

        private void JsonToXmlElement(JToken token, XElement parent)
        {
            if (token is JObject jObj)
            {
                foreach (var property in jObj.Properties())
                {
                    var element = new XElement(SanitizeXmlName(property.Name));
                    JsonToXmlElement(property.Value, element);
                    parent.Add(element);
                }
            }
            else if (token is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    var element = new XElement("item");
                    JsonToXmlElement(item, element);
                    parent.Add(element);
                }
            }
            else
            {
                parent.Value = token.ToString();
            }
        }

        /// <summary>
        /// Convert XML to JSON
        /// </summary>
        public string XmlToJson(string xmlText)
        {
            try
            {
                var xmlDoc = XDocument.Parse(xmlText);
                var json = XmlToJsonToken(xmlDoc.Root!);
                return JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert XML to JSON: {ex.Message}", ex);
            }
        }

        private JToken XmlToJsonToken(XElement element)
        {
            if (element.HasElements)
            {
                var obj = new JObject();
                foreach (var child in element.Elements())
                {
                    var childToken = XmlToJsonToken(child);
                    if (obj[child.Name.LocalName] != null)
                    {
                        if (obj[child.Name.LocalName] is JArray array)
                        {
                            array.Add(childToken);
                        }
                        else
                        {
                            var newArray = new JArray(obj[child.Name.LocalName], childToken);
                            obj[child.Name.LocalName] = newArray;
                        }
                    }
                    else
                    {
                        obj[child.Name.LocalName] = childToken;
                    }
                }
                return obj;
            }
            return new JValue(element.Value);
        }

        /// <summary>
        /// Convert JSON to C# classes
        /// </summary>
        public string JsonToCSharpClasses(string jsonText, string className = "RootObject", bool usePascalCase = true)
        {
            try
            {
                var jToken = JToken.Parse(jsonText);
                var sb = new StringBuilder();
                var processedTypes = new HashSet<string>();

                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendLine("using Newtonsoft.Json;");
                sb.AppendLine();

                GenerateClass(jToken, className, sb, processedTypes, usePascalCase);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert JSON to C# classes: {ex.Message}", ex);
            }
        }

        private void GenerateClass(JToken token, string className, StringBuilder sb, HashSet<string> processedTypes, bool usePascalCase)
        {
            if (token is not JObject jObj || processedTypes.Contains(className))
                return;

            processedTypes.Add(className);

            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            foreach (var property in jObj.Properties())
            {
                var propertyName = usePascalCase ? ToPascalCase(property.Name) : property.Name;
                var propertyType = GetCSharpType(property.Value, propertyName, sb, processedTypes, usePascalCase);

                if (usePascalCase && propertyName != property.Name)
                {
                    sb.AppendLine($"    [JsonProperty(\"{property.Name}\")]");
                }

                sb.AppendLine($"    public {propertyType} {propertyName} {{ get; set; }}");
                sb.AppendLine();
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        private string GetCSharpType(JToken token, string propertyName, StringBuilder sb, HashSet<string> processedTypes, bool usePascalCase)
        {
            return token.Type switch
            {
                JTokenType.String => "string",
                JTokenType.Integer => "int",
                JTokenType.Float => "double",
                JTokenType.Boolean => "bool",
                JTokenType.Date => "DateTime",
                JTokenType.Null => "object",
                JTokenType.Array => GetArrayType((JArray)token, propertyName, sb, processedTypes, usePascalCase),
                JTokenType.Object => GetObjectType((JObject)token, propertyName, sb, processedTypes, usePascalCase),
                _ => "object"
            };
        }

        private string GetArrayType(JArray array, string propertyName, StringBuilder sb, HashSet<string> processedTypes, bool usePascalCase)
        {
            if (array.Count == 0)
                return "List<object>";

            var firstItem = array[0];
            var itemType = GetCSharpType(firstItem, propertyName, sb, processedTypes, usePascalCase);
            return $"List<{itemType}>";
        }

        private string GetObjectType(JObject obj, string propertyName, StringBuilder sb, HashSet<string> processedTypes, bool usePascalCase)
        {
            var className = ToPascalCase(propertyName);
            GenerateClass(obj, className, sb, processedTypes, usePascalCase);
            return className;
        }

        /// <summary>
        /// Convert JSON to SQL INSERT statements
        /// </summary>
        public string JsonToSql(string jsonText, string tableName = "MyTable")
        {
            try
            {
                var jToken = JToken.Parse(jsonText);
                var sb = new StringBuilder();

                if (jToken is JArray jArray)
                {
                    foreach (var item in jArray)
                    {
                        if (item is JObject jObj)
                        {
                            sb.AppendLine(GenerateSqlInsert(jObj, tableName));
                        }
                    }
                }
                else if (jToken is JObject jObj)
                {
                    sb.AppendLine(GenerateSqlInsert(jObj, tableName));
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert JSON to SQL: {ex.Message}", ex);
            }
        }

        private string GenerateSqlInsert(JObject jObj, string tableName)
        {
            var columns = new List<string>();
            var values = new List<string>();

            foreach (var property in jObj.Properties())
            {
                columns.Add(property.Name);
                values.Add(FormatSqlValue(property.Value));
            }

            return $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)});";
        }

        private string FormatSqlValue(JToken token)
        {
            return token.Type switch
            {
                JTokenType.String => $"'{token.ToString().Replace("'", "''")}'",
                JTokenType.Null => "NULL",
                JTokenType.Boolean => token.ToString().ToUpper(),
                _ => token.ToString()
            };
        }

        /// <summary>
        /// Convert JSON to YAML
        /// </summary>
        public string JsonToYaml(string jsonText)
        {
            try
            {
                var jToken = JToken.Parse(jsonText);
                var obj = JsonConvert.DeserializeObject(jsonText);

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                return serializer.Serialize(obj!);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert JSON to YAML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convert YAML to JSON
        /// </summary>
        public string YamlToJson(string yamlText)
        {
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var obj = deserializer.Deserialize<object>(yamlText);
                return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert YAML to JSON: {ex.Message}", ex);
            }
        }

        private string ToPascalCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var words = str.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", words.Select(w => char.ToUpper(w[0]) + w.Substring(1)));
        }

        private string SanitizeXmlName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "element";

            var sb = new StringBuilder();
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else
                    sb.Append('_');
            }

            var result = sb.ToString();
            if (char.IsDigit(result[0]))
                result = "_" + result;

            return result;
        }
    }
}
