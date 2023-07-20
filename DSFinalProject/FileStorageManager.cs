using System.Text.Json;

namespace DSFinalProject
{
    // Data saving and loading class
    public class FileStorageManager
    {
        // Loading the data via JSON
        public BoxInventoryManager? LoadData()
        {
            Dictionary<string, int>? boxInventory;
            Dictionary<string, DateTime>? boxSizeToLastPurchaseDate;
            int maxQuantity;
            int minQuantity;
            double maxOffsetPercentage;
            int maxSplits;

            string configurationsFilePath = "configurations.json";
            string boxInventoryFilePath = "boxInventory.json";
            string boxSizeToLastPurchaseDateFilePath = "boxSizeToLastPurchaseDate.json";

            if (!File.Exists(boxInventoryFilePath) || !File.Exists(boxSizeToLastPurchaseDateFilePath) || !File.Exists(configurationsFilePath))
                return null;

            // Loading boxInventory
            string boxInventoryJson = File.ReadAllText(boxInventoryFilePath);
            boxInventory = JsonSerializer.Deserialize<Dictionary<string, int>>(boxInventoryJson);

            // Loading boxSizeToLastPurchaseDate
            string boxSizeToLastPurchaseDateJson = File.ReadAllText(boxSizeToLastPurchaseDateFilePath);
            boxSizeToLastPurchaseDate = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(boxSizeToLastPurchaseDateJson);

            // Loading Configurations
            string configurationsJson = File.ReadAllText(configurationsFilePath);
            var configurationsData = JsonSerializer.Deserialize<dynamic>(configurationsJson);
            maxQuantity = configurationsData?.GetProperty("maxQuantity").GetInt32();
            minQuantity = configurationsData?.GetProperty("minQuantity").GetInt32();
            maxOffsetPercentage = configurationsData?.GetProperty("maxOffsetPercentage").GetDouble();
            maxSplits = configurationsData?.GetProperty("maxSplits").GetInt32();

            // Convert the string keys back to BoxSize
            var convertedBoxInventory = boxInventory?.ToDictionary(pair => BoxSize.Parse(pair.Key), pair => pair.Value);
            var convertedBoxSizeToLastPurchaseDate = boxSizeToLastPurchaseDate?.ToDictionary(pair => BoxSize.Parse(pair.Key), pair => pair.Value);

            if (convertedBoxInventory != null && convertedBoxSizeToLastPurchaseDate != null)
                return new BoxInventoryManager(convertedBoxInventory, convertedBoxSizeToLastPurchaseDate, maxQuantity, minQuantity, maxOffsetPercentage, maxSplits);

            return null;
        }

        // Saving the data via JSON
        public void SaveData(BoxInventoryManager boxInventoryManager)
        {
            // Convert BoxSize keys to string before saving (allows saving the BoxSize struct in JSON)
            var stringBoxInventory = boxInventoryManager.BoxInventory.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);
            var stringBoxSizeToLastPurchaseDate = boxInventoryManager.BoxSizeToLastPurchaseDate.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);

            // Saving boxInventory
            string boxInventoryJson = JsonSerializer.Serialize(stringBoxInventory);
            File.WriteAllText("boxInventory.json", boxInventoryJson);

            // Saving boxSizeToLastPurchaseDate
            string boxSizeToLastPurchaseDateJson = JsonSerializer.Serialize(stringBoxSizeToLastPurchaseDate);
            File.WriteAllText("boxSizeToLastPurchaseDate.json", boxSizeToLastPurchaseDateJson);

            // Saving Configurations
            var configurationsData = new
            {
                maxQuantity = boxInventoryManager.MaxQuantity,
                minQuantity = boxInventoryManager.MinQuantity,
                maxOffsetPercentage = boxInventoryManager.MaxOffsetPercentage,
                maxSplits = boxInventoryManager.MaxSplits
            };

            string configurationsJson = JsonSerializer.Serialize(configurationsData);
            File.WriteAllText("configurations.json", configurationsJson);
        }
    }
}
