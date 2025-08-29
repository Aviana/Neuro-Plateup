using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Neuro_Plateup
{
    public class JsonSchema
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, JsonSchemaProperty> Properties { get; set; }

        [JsonProperty("required")]
        public List<string> Required { get; set; }
    }

    public class JsonSchemaProperty
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("minLength", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinLength { get; set; }

        [JsonProperty("maxLength", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxLength { get; set; }

        [JsonProperty("minimum", NullValueHandling = NullValueHandling.Ignore)]
        public int? Minimum { get; set; }

        [JsonProperty("maximum", NullValueHandling = NullValueHandling.Ignore)]
        public int? Maximum { get; set; }

        [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Enum { get; set; }
    }

    public static class SchemaFactory
    {
        public static List<string> Dishes = new List<string>();
        public static Dictionary<string, int> Players = new Dictionary<string, int>();

        public static JsonSchema GetSchema(string key)
        {
            switch (key)
            {
                case "rename_restaurant":
                    return new JsonSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, JsonSchemaProperty>
                        {
                            ["name"] = new JsonSchemaProperty
                            {
                                Type = "string",
                                MinLength = 3,
                                MaxLength = 24
                            }
                        },
                        Required = new List<string> { "name" }
                    };
                case "prepare_dish":
                    return new JsonSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, JsonSchemaProperty>
                        {
                            ["dish"] = new JsonSchemaProperty
                            {
                                Type = "string",
                                Enum = Dishes
                            }
                        },
                        Required = new List<string> { "dish" }
                    };
                case "go_to":
                    return new JsonSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, JsonSchemaProperty>
                        {
                            ["player"] = new JsonSchemaProperty
                            {
                                Type = "string",
                                Enum = Players.Keys.ToList()
                            }
                        },
                        Required = new List<string> { "player" }
                    };

                default:
                    return null;
            }
        }
        public static bool ValidateAgainstSchema(Dictionary<string, object> data, JsonSchema schema)
        {
            if (data == null)
                return false;

            // Check required fields
            foreach (var requiredKey in schema.Required)
            {
                if (!data.ContainsKey(requiredKey))
                {
                    Debug.LogWarning($"Missing required field: {requiredKey}");
                    return false;
                }
            }

            // Check types and constraints
            foreach (var property in schema.Properties)
            {
                if (!data.TryGetValue(property.Key, out var value))
                    continue;

                var expectedType = property.Value.Type;

                switch (expectedType)
                {
                    case "string":
                        if (!(value is string strValue))
                            return false;

                        if (property.Value.MinLength.HasValue && strValue.Length < property.Value.MinLength.Value)
                            return false;
                        if (property.Value.MaxLength.HasValue && strValue.Length > property.Value.MaxLength.Value)
                            return false;
                        break;

                    case "integer":
                        if (!(value is long || value is int))
                            return false;

                        var intValue = Convert.ToInt32(value);
                        if (property.Value.Minimum.HasValue && intValue < property.Value.Minimum.Value)
                            return false;
                        if (property.Value.Maximum.HasValue && intValue > property.Value.Maximum.Value)
                            return false;
                        break;

                    default:
                        Debug.LogWarning($"Unknown type '{expectedType}' in schema validation.");
                        break;
                }
            }

            return true;
        }
    }
}