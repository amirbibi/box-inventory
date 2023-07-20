namespace DSFinalProject
{
    // Logic Class
    public class BoxInventoryManager
    {
        private FileStorageManager fileStorageManager = new FileStorageManager();
        private Dictionary<BoxSize, int> boxInventory;
        private Dictionary<BoxSize, DateTime> boxSizeToLastPurchaseDate;
        private int maxQuantity;
        private int minQuantity;
        private double maxOffsetPercentage;
        private int maxSplits;

        public Dictionary<BoxSize, int> BoxInventory { get => boxInventory; }
        public Dictionary<BoxSize, DateTime> BoxSizeToLastPurchaseDate { get => boxSizeToLastPurchaseDate; }
        public int MaxQuantity { get => maxQuantity; }
        public int MinQuantity { get => minQuantity; }
        public double MaxOffsetPercentage { get => maxOffsetPercentage; }
        public int MaxSplits { get => MaxQuantity; }

        public BoxInventoryManager(Dictionary<BoxSize, int> boxInventory,
            Dictionary<BoxSize, DateTime> boxSizeToLastPurchaseDate,
            int maxQuantity, int minQuantity, double maxOffsetPercentage, int maxSplits)
        {
            this.boxInventory = boxInventory;
            this.boxSizeToLastPurchaseDate = boxSizeToLastPurchaseDate;
            EditConfigurations(maxQuantity, minQuantity, maxOffsetPercentage, maxSplits);
        }

        public BoxInventoryManager(int maxQuantity, int minQuantity, double maxOffsetPercentage, int maxSplits)
        {
            boxInventory = new Dictionary<BoxSize, int>();
            boxSizeToLastPurchaseDate = new Dictionary<BoxSize, DateTime>();
            EditConfigurations(maxQuantity, minQuantity, maxOffsetPercentage, maxSplits);
        }

        public int AddBoxesToInventory(BoxSize box, int quantity)
        {
            int remainingBoxes = 0;
            if (boxInventory.ContainsKey(box))
                boxInventory[box] += quantity;
            else
                boxInventory.Add(box, quantity);

            if (boxInventory[box] > maxQuantity)
            {
                remainingBoxes = boxInventory[box] - maxQuantity;
                boxInventory[box] = maxQuantity;
            }

            // Saving Data
            fileStorageManager.SaveData(this);

            return remainingBoxes;
        }

        // Using GetBestFitBoxesWithQuantity to find and select the best fit boxes for a gift (box)
        public Dictionary<BoxSize, int>? SelectBoxesForPurchase(BoxSize box, int quantity)
        {
            Dictionary<BoxSize, int>? selectedBoxesForPurchase = new Dictionary<BoxSize, int>();

            if (!boxInventory.ContainsKey(box))
                selectedBoxesForPurchase = GetBestFitBoxesWithQuantity(box, quantity);

            else if (boxInventory[box] < quantity)
                selectedBoxesForPurchase.Add(box, quantity);

            else
            {
                quantity -= boxInventory[box];
                selectedBoxesForPurchase = GetBestFitBoxesWithQuantity(box, quantity);

                if (selectedBoxesForPurchase == null)
                    return null;

                selectedBoxesForPurchase.Add(box, boxInventory[box]);
            }

            return selectedBoxesForPurchase;
        }

        public List<string> PurchaseBoxes(Dictionary<BoxSize, int> selectedBoxesForPurchase)
        {
            List<string> statusMessagesToUI = new List<string>();
            foreach (KeyValuePair<BoxSize, int> boxAndUnitsBought in selectedBoxesForPurchase)
            {
                BoxSize box = boxAndUnitsBought.Key;
                int unitsBought = boxAndUnitsBought.Value;

                boxInventory[box] -= unitsBought;

                UpdateBoxSizeToLastPurchaseDate(box);

                if (IsBoxSizeOutOfStock(box))
                {
                    boxInventory.Remove(box);
                    statusMessagesToUI.Add($"Box of size (X={box.x}, Y={box.y}) got out of stock and removed");
                }
                else if (boxInventory[box] < minQuantity)
                {
                    statusMessagesToUI.Add($"Warning: Box of size (X={box.x}, Y={box.y}) has under {minQuantity} boxes left!");
                }
            }

            fileStorageManager.SaveData(this);

            return statusMessagesToUI;
        }

        // This method returns a list of box sizes that haven't been purchased for a duration longer than the given threshold (T),
        // and are still available in the inventory.
        // (T = days) an input from the user.
        public List<BoxSize> GetUnpurchasedBoxesPastThreshold(TimeSpan T)
        {
            List<BoxSize> unpurchasedBoxes = new List<BoxSize>();
            DateTime currentTime = DateTime.Now;

            foreach (KeyValuePair<BoxSize, DateTime> boxSizeAndLastPurchaseDate in boxSizeToLastPurchaseDate)
            {
                TimeSpan boxUnpurchsedTime = currentTime - boxSizeAndLastPurchaseDate.Value;
                if (boxUnpurchsedTime > T && boxInventory.ContainsKey(boxSizeAndLastPurchaseDate.Key))
                    unpurchasedBoxes.Add(boxSizeAndLastPurchaseDate.Key);
            }

            return unpurchasedBoxes;
        }

        public void RemoveExpiredBoxesPastThreshold(List<BoxSize> expiredBoxesToRemove)
        {
            foreach (BoxSize box in expiredBoxesToRemove)
                boxInventory.Remove(box);

            fileStorageManager.SaveData(this);
        }

        public void EditConfigurations(int maxQuantity, int minQuantity, double maxOffsetPercentage, int maxSplits)
        {
            this.maxQuantity = maxQuantity;
            this.minQuantity = minQuantity;
            this.maxOffsetPercentage = maxOffsetPercentage;
            this.maxSplits = maxSplits;

            fileStorageManager.SaveData(this);
        }

        // This method attempts to find the best fitting box for the targetBox.
        // It filters and sorts the boxes based on their X, Y within the allowed offset percentage.
        // Boxes that have already reached their maximum quantity in CurrentBestFitBoxesWithQuantity are removed from consideration.
        // The method returns the first box from the filtered and sorted list, if any.
        public BoxSize? FindBestFitBox(BoxSize targetBox, Dictionary<BoxSize, int> CurrentBestFitBoxesWithQuantity)
        {
            double maxAllowedX = (targetBox.x * (100 + maxOffsetPercentage)) / 100;
            double maxAllowedY = (targetBox.y * (100 + maxOffsetPercentage)) / 100;

            // Filtering and Sorting the boxes to match our restrictions for X and Y
            List<BoxSize> filteredAndSortedBoxes = boxInventory.Keys
                .Where(box => box.x >= targetBox.x && box.y >= targetBox.y && box.x <= maxAllowedX && box.y <= maxAllowedY)
                .OrderBy(box => box.x)
                .ThenBy(box => box.y)
                .ToList();

            List<BoxSize> boxesToRemove = new List<BoxSize>();
            foreach (BoxSize box in filteredAndSortedBoxes)
            {
                CurrentBestFitBoxesWithQuantity.TryGetValue(box, out int currQuantityInBestFitBoxes);
                if (currQuantityInBestFitBoxes >= boxInventory[box])
                    boxesToRemove.Add(box);
            }

            foreach (BoxSize box in boxesToRemove)
                filteredAndSortedBoxes.Remove(box);

            if (filteredAndSortedBoxes.Count > 0)
                return filteredAndSortedBoxes[0];
            else
                return null;
        }

        // This method creates a dictionary of best-fitting boxes and their quantities for a given box size and quantity. 
        // It iteratively uses FindBestFitBox to fill the dictionary, increasing quantity or adding new entries as needed. 
        // If a suitable box isn't found or maxSplits is exceeded, it returns null.
        public Dictionary<BoxSize, int>? GetBestFitBoxesWithQuantity(BoxSize box, int quantity)
        {
            Dictionary<BoxSize, int> BestFitBoxesWithQuantity = new Dictionary<BoxSize, int>();

            for (int i = 0; i < quantity; i++)
            {
                if (BestFitBoxesWithQuantity.Count > maxSplits)
                    return null;

                BoxSize? bestFitBox = FindBestFitBox(box, BestFitBoxesWithQuantity);
                if (bestFitBox == null)
                    return null;

                else if (BestFitBoxesWithQuantity.ContainsKey((BoxSize)bestFitBox))
                    BestFitBoxesWithQuantity[((BoxSize)bestFitBox)] += 1;

                else
                    BestFitBoxesWithQuantity.Add((BoxSize)bestFitBox, 1);
            }

            return BestFitBoxesWithQuantity;
        }

        public int GetBoxQuantity(BoxSize box)
        {
            if (boxInventory.ContainsKey(box)) return boxInventory[box];
            else return -1;
        }

        public bool IsBoxSizeOutOfStock(BoxSize box)
        {
            return (boxInventory[box] == 0);
        }

        public void UpdateBoxSizeToLastPurchaseDate(BoxSize box)
        {
            if (boxSizeToLastPurchaseDate.ContainsKey(box))
                boxSizeToLastPurchaseDate[box] = DateTime.Now;
            else
                boxSizeToLastPurchaseDate.Add(box, DateTime.Now);
        }
    }

}
