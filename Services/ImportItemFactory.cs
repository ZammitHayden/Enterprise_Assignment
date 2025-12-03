using System;
using System.Collections.Generic;
using System.Text.Json;
using Domain.Interfaces;
using Enterprise_Assignment.Models;

namespace Enterprise_Assignment.Services
{
    public class ImportItemFactory
    {
        public List<IItemValidating> Create(string json)
        {
            var items = new List<IItemValidating>();

            try
            {
                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement element in root.EnumerateArray())
                    {
                        IItemValidating item = CreateItemFromJson(element);
                        if (item != null)
                            items.Add(item);
                    }
                }
                else
                {
                    IItemValidating item = CreateItemFromJson(root);
                    if (item != null)
                        items.Add(item);
                }
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Invalid JSON format", ex);
            }

            return items;
        }

        private IItemValidating CreateItemFromJson(JsonElement element)
        {
            if (!element.TryGetProperty("Type", out JsonElement typeElement))
            {
                throw new ArgumentException("JSON object must contain a 'Type' field");
            }

            string type = typeElement.GetString();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            IItemValidating item = type?.ToLower() switch
            {
                "restaurant" => JsonSerializer.Deserialize<Restaurant>(element.GetRawText(), options),
                "menuitem" => JsonSerializer.Deserialize<MenuItem>(element.GetRawText(), options),
                _ => throw new ArgumentException($"Unknown type: {type}")
            };

            if (item is MenuItem menuItem && menuItem.Id == Guid.Empty)
            {
                menuItem.Id = Guid.NewGuid();
            }

            return item;
        }
    }
}