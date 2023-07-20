namespace DSFinalProject
{
    // User Interface Class
    public class BoxInventoryUI
    {
        public BoxInventoryManager boxInventoryManager;
        private FileStorageManager fileStorageManager;

        public BoxInventoryUI()
        {
            // Loading Data
            fileStorageManager = new FileStorageManager();
            BoxInventoryManager? loadedData = fileStorageManager.LoadData();

            if (loadedData != null)
            {
                Console.WriteLine("Data loaded successfully.");
                boxInventoryManager = loadedData;
            }
            else // First Initailization
            {
                Console.WriteLine("It seems that this is your first time using the app. Let's proceed with the initialization of the configurations:\n");
                int maxQuantity = ReadInt("Please enter the maximum quantity allowed for all boxes: ");
                int minQuantity = ReadInt("Please enter the alert threshold for minimum quantity for all boxes: ");
                double maxOffsetPercentage = ReadInt("Please enter global maximum offset limit (in percentages) for all boxes: ");
                int maxSplits = ReadInt("Please enter the maximum number of splits allowed for all boxes: ");

                boxInventoryManager = new BoxInventoryManager(maxQuantity, minQuantity, maxOffsetPercentage, maxSplits);
            }
        }

        public void Start()
        {
            while (true)
            {
                Console.WriteLine("\nWhat would you like to do? Choose a number or type 'exit' to quit:");
                Console.WriteLine("1. Add new box(es)");
                Console.WriteLine("2. Purchase box(es) for a gift");
                Console.WriteLine("3. Show box size information");
                Console.WriteLine("4. Show unpurchased boxes since a certain time");
                Console.WriteLine("5. Remove expired boxes since a certain time");
                Console.WriteLine("6. Edit configurations");
                Console.WriteLine("7. Show inventory");

                string? input = Console.ReadLine();
                if (input != null && input.ToLower().Equals("exit"))
                    break;

                if (int.TryParse(input, out int userChoice))
                {
                    switch (userChoice)
                    {
                        case 1:
                            AddBoxesToInventory();
                            break;
                        case 2:
                            HandlePurchaseProcess();
                            break;
                        case 3:
                            ShowBoxInformation();
                            break;
                        case 4:
                            ShowUnpurchasedBoxesPastThreshold();
                            break;
                        case 5:
                            ShowRemovedExpiredBoxesPastThreshold();
                            break;
                        case 6:
                            EditConfigurations();
                            break;
                        case 7:
                            PrintInventory();
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please enter a number between 1 and 7.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number or type 'exit' to quit.");
                    continue;
                }
            }
        }

        private void EditConfigurations()
        {
            int maxQuantity = ReadInt("Please enter the maximum quantity allowed for all boxes: ");
            int minQuantity = ReadInt("Please enter the alert threshold for minimum quantity for all boxes: ");
            double maxOffsetPercentage = ReadInt("Please enter global maximum offset limit (in percentages) for all boxes: ");
            int maxSplits = ReadInt("Please enter the maximum number of splits allowed for all boxes: ");

            boxInventoryManager.EditConfigurations(maxQuantity, minQuantity, maxOffsetPercentage, maxSplits);
        }

        private void ShowBoxInformation()
        {
            BoxSize box = ReadBoxSize();
            int quantity = boxInventoryManager.GetBoxQuantity(box);

            if (quantity != -1)
                Console.WriteLine($"There are {quantity} boxes of size (X={box.x}, Y={box.y}) remaining in inventory.");
            else
                Console.WriteLine("Box size doesn't exists in inventory");
        }

        private void AddBoxesToInventory()
        {
            BoxSize box = ReadBoxSize();
            int quantity = ReadInt($"Please enter the number of boxes of size (X={box.x}, Y={box.y}) you'd like to add: ");
            int remainingBoxes = boxInventoryManager.AddBoxesToInventory(box, quantity);

            if (remainingBoxes > 0)
            {
                Console.WriteLine($"The quantity of boxes exceeds the maximum limit defined in the configuration. " +
                    $"{remainingBoxes} boxes have been returned.");
            }

            Console.WriteLine($"Successfully added {quantity - remainingBoxes} box(es) of size (X={box.x}, Y={box.y}).");
        }

        private void HandlePurchaseProcess()
        {
            double x = ReadDouble("Please enter gift width (X): ");
            double y = ReadDouble("Please enter gift height (Y): ");
            BoxSize boxSizeToPurchase = new BoxSize { x = x, y = y };
            int quantity = ReadInt($"Please enter the number of boxes of size (X={boxSizeToPurchase.x}, Y={boxSizeToPurchase.y}) you'd like to purchase: ");

            Dictionary<BoxSize, int>? selectedBoxesForPurchase = boxInventoryManager.SelectBoxesForPurchase(boxSizeToPurchase, quantity);

            if (selectedBoxesForPurchase == null)
            {
                Console.WriteLine($"Did not found a {quantity} suited box(es) for gift of size (X={boxSizeToPurchase.x}, Y={boxSizeToPurchase.y}) in distance of {boxInventoryManager.MaxOffsetPercentage}%");
                Console.WriteLine("Action cancelled");
                return;
            }

            if (selectedBoxesForPurchase.Count > 0)
            {
                string selectedBoxesForPurchaseStr = string.Join(", ", selectedBoxesForPurchase.Select(boxAndUnitsBought =>
                $"({boxAndUnitsBought.Value} boxes of size X={boxAndUnitsBought.Key.x}, Y={boxAndUnitsBought.Key.y})"));
                Console.WriteLine($"The following box sizes will be purchased: {selectedBoxesForPurchaseStr}");

                if (UserAgreementPrompt())
                {
                    List<string> statusMessages = boxInventoryManager.PurchaseBoxes(selectedBoxesForPurchase);

                    foreach (string message in statusMessages)
                        Console.WriteLine(message);

                    Console.WriteLine($"Successfully bought the following boxes: {selectedBoxesForPurchaseStr}");
                }
                else
                {
                    Console.WriteLine($"Action cancelled, The boxes will not be purchased.");
                }
            }
            else
            {
                Console.WriteLine($"There are no valid boxes for purchase");
            }
        }

        private void ShowUnpurchasedBoxesPastThreshold()
        {
            int days = ReadInt("Please enter the number of days to look back for unpurchased boxes: ");
            TimeSpan T = TimeSpan.FromDays(days);
            List<BoxSize> unpurchasedBoxes = boxInventoryManager.GetUnpurchasedBoxesPastThreshold(T);

            if (unpurchasedBoxes.Count > 0)
            {
                string unpurchasedBoxesStr = string.Join(", ", unpurchasedBoxes.Select(box => $"(X={box.x}, Y={box.y})"));
                Console.WriteLine($"{unpurchasedBoxesStr} didn't get purchased for {days} days");
            }
            else
            {
                Console.WriteLine($"There are no boxes that didn't get purchased for {days} days");
            }
        }

        private void ShowRemovedExpiredBoxesPastThreshold()
        {
            int days = ReadInt("Please enter the number of days for unpurchased boxes to be removed: ");
            TimeSpan T = TimeSpan.FromDays(days);
            List<BoxSize> expiredBoxesToRemove = boxInventoryManager.GetUnpurchasedBoxesPastThreshold(T);

            if (expiredBoxesToRemove.Count > 0)
            {
                string expiredBoxesToRemoveStr = string.Join(", ", expiredBoxesToRemove.Select(box => $"(X={box.x}, Y={box.y})"));
                Console.WriteLine($"The following box sizes will be removed: {expiredBoxesToRemoveStr}");
                if (UserAgreementPrompt())
                {
                    boxInventoryManager.RemoveExpiredBoxesPastThreshold(expiredBoxesToRemove);
                    Console.WriteLine($"Successfully removed {expiredBoxesToRemoveStr} expired boxes.");
                }
                else
                {
                    Console.WriteLine("Action cancelled, The expired boxes will not be removed.");
                }
            }
            else
            {
                Console.WriteLine($"There are no expired boxes past {days} days");
            }
        }

        private void PrintInventory()
        {
            foreach (KeyValuePair<BoxSize, int> box in boxInventoryManager.BoxInventory)
                Console.WriteLine($"Box of Size: (X={box.Key.x}, Y={box.Key.y}), Quantity: {box.Value}");
        }

        /*
        private DateTime ReadDateTime(string prompt)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                string? input = Console.ReadLine();
                if (DateTime.TryParseExact(input, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime value))
                    return value;

                Console.WriteLine("Invalid input. Please enter a date in the format dd/MM/yyyy.");
            }
        }
        */

        private bool UserAgreementPrompt()
        {
            while (true)
            {
                Console.WriteLine($"Do you approve? (Y/N)");
                string? input = Console.ReadLine();

                if (input != null && input.ToLower() == "y")
                    return true;

                if (input != null && input.ToLower() == "n")
                    return false;

                Console.WriteLine("Invalid input. Please enter 'Y' for Yes or 'N' for No.");
            }
        }

        private BoxSize ReadBoxSize()
        {
            double x = ReadDouble("Please enter box width (X): ");
            double y = ReadDouble("Please enter box height (Y): ");

            return new BoxSize { x = x, y = y };
        }

        private int ReadInt(string prompt)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int value) && value >= 0)
                    return value;

                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }

        private double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                string? input = Console.ReadLine();
                if (double.TryParse(input, out double value) && value >= 0)
                    return value;

                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }
    }

}
