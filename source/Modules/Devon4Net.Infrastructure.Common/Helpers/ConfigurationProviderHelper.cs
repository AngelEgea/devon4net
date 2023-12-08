using System.Text.Json;

namespace Devon4Net.Infrastructure.Common.Helpers
{
    public static class ConfigurationProviderHelper
    {
        public static Dictionary<string, string> JsonToStringDictionary(string json, string prefix)
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            return JsonToStringDictionary(jsonElement, prefix);
        }

        public static Dictionary<string, string> JsonToStringDictionary(JsonElement jsonElement, string prefix)
        {
            var jsonDictionary = JsonToStringDictionary(jsonElement);
            var resultValue = new Dictionary<string, string>();

            foreach (var keyValuePair in jsonDictionary)
            {
                resultValue[$"{prefix}:{keyValuePair.Key}"] = keyValuePair.Value;
            }

            return resultValue;
        }

        public static Dictionary<string, string> JsonToStringDictionary(string json)
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            return JsonToStringDictionary(jsonElement);
        }

        private static Dictionary<string, string> JsonToStringDictionary(JsonElement currentElement)
        {
            var returnValue = new Dictionary<string, string>();

            switch (currentElement.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var objectElement in currentElement.EnumerateObject())
                    {
                        foreach (var objectElementKeyPair in JsonToStringDictionary(objectElement.Value))
                        {
                            if (string.IsNullOrWhiteSpace(objectElementKeyPair.Key))
                            {
                                returnValue.Add($"{objectElement.Name}", objectElementKeyPair.Value);
                            }
                            else
                            {
                                returnValue.Add($"{objectElement.Name}:{objectElementKeyPair.Key}", objectElementKeyPair.Value);
                            }
                        }
                    }
                    break;
                case JsonValueKind.Array:
                    var currentIndex = 0;
                    foreach (var objectElement in currentElement.EnumerateArray())
                    {
                        foreach (var objectElementKeyPair in JsonToStringDictionary(objectElement))
                        {
                            if (string.IsNullOrWhiteSpace(objectElementKeyPair.Key))
                            {
                                returnValue.Add($"{currentIndex}", objectElementKeyPair.Value);
                            }
                            else
                            {
                                returnValue.Add($"{currentIndex}:{objectElementKeyPair.Key}", objectElementKeyPair.Value);
                            }
                        }
                        currentIndex++;
                    }
                    break;
                case JsonValueKind.String:
                case JsonValueKind.Number:
                    returnValue.Add(string.Empty, currentElement.ToString());
                    break;
                case JsonValueKind.True:
                    returnValue.Add(string.Empty, "true");
                    break;
                case JsonValueKind.False:
                    returnValue.Add(string.Empty, "false");
                    break;
                    // JsonValueKind.Null and JsonValueKind.Undefined are not processed
            }

            return returnValue;
        }
    }
}
