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

        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, JsonSchemaProperty> Properties { get; set; }

        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Required { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public JsonSchema Items { get; set; }
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
                case "prepare_dishes":
                    return new JsonSchema
                    {
                        Type = "array",
                        Items = new JsonSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, JsonSchemaProperty>
                            {
                                ["dish"] = new JsonSchemaProperty
                                {
                                    Type = "string",
                                    Enum = Dishes
                                },
                                ["amount"] = new JsonSchemaProperty
                                {
                                    Type = "integer",
                                    Minimum = 1,
                                    Maximum = 4
                                }
                            },
                            Required = new List<string> { "dish", "amount" }
                        },
                    };

                default:
                    return null;
            }
        }

        public static bool ValidateAgainstSchema(List<Dictionary<string, object>> dataArray, JsonSchema schema, out string reason)
        {
            reason = "";

            if (dataArray == null || schema == null)
            {
                reason = "Parse error";
                return false;
            }

            var seenDishes = new HashSet<string>();

            foreach (var item in dataArray)
            {
                if (!(item is Dictionary<string, object> obj))
                {
                    reason = "Each item in the array must be an object.";
                    return false;
                }

                if (!ValidateAgainstSchema(obj, schema.Items, out reason))
                    return false;

                if (obj.TryGetValue("dish", out var dishObj) && dishObj is string dish)
                {
                    if (!seenDishes.Add(dish))
                    {
                        reason = $"Duplicate dish found: {dish}";
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool ValidateAgainstSchema(Dictionary<string, object> data, JsonSchema schema, out string reason)
        {
            reason = "";

            if (data == null || schema == null)
            {
                reason = "Parse error";
                return false;
            }

            foreach (var requiredKey in schema.Required)
            {
                if (!data.ContainsKey(requiredKey))
                {
                    reason = $"Missing required key: {requiredKey}";
                    return false;
                }
            }

            foreach (var property in schema.Properties)
            {
                if (!data.TryGetValue(property.Key, out var value))
                    continue;

                var expectedType = property.Value.Type;

                switch (expectedType)
                {
                    case "string":
                        if (!(value is string strValue))
                        {
                            reason = $"Key {property.Key} is wrong type";
                            return false;
                        }

                        if (property.Value.MinLength.HasValue && strValue.Length < property.Value.MinLength.Value)
                        {
                            reason = $"Key {property.Key} has wrong value";
                            return false;
                        }

                        if (property.Value.MaxLength.HasValue && strValue.Length > property.Value.MaxLength.Value)
                        {
                            reason = $"Key {property.Key} has wrong value";
                            return false;
                        }

                        if (property.Value.Enum != null && !property.Value.Enum.Contains(strValue))
                        {
                            reason = $"Key {property.Key} has wrong value";
                            return false;
                        }

                        break;

                    case "integer":
                        if (!(value is long || value is int))
                        {
                            reason = $"Key {property.Key} is wrong type";
                            return false;
                        }

                        var intValue = Convert.ToInt32(value);
                        if (property.Value.Minimum.HasValue && intValue < property.Value.Minimum.Value)
                        {
                            reason = $"Key {property.Key} has wrong value";
                            return false;
                        }

                        if (property.Value.Maximum.HasValue && intValue > property.Value.Maximum.Value)
                        {
                            reason = $"Key {property.Key} has wrong value";
                            return false;
                        }

                        break;

                    default:
                        reason = $"Key {property.Key} is of unsupported type";
                        return false;
                }
            }

            return true;
        }
    }
}