using DSFinalProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    // Test Logic Class
    [TestClass]
    public class BoxInventoryTest
    {
        BoxInventoryManager boxInventoryManager = new BoxInventoryManager(20, 5, 150, 5);

        [TestMethod]
        public void TestMethod1() => Assert.AreEqual(1, 1);

        [TestMethod]
        public void AddBoxesToInventory_QuantityExceedsMaxLimit_ReturnsRemainingBoxes()
        {
            BoxSize box = new BoxSize { x = 10, y = 5 };
            int quantity = 15;
            int expectedRemainingBoxes = 5;

            // Act
            int actualRemainingBoxes = boxInventoryManager.AddBoxesToInventory(box, quantity);

            // Assert
            Assert.AreEqual(expectedRemainingBoxes, actualRemainingBoxes);
        }

        [TestMethod]
        public void AddBoxesToInventory_QuantityWithinLimit_ReturnsZeroRemainingBoxes()
        {
            // Arrange
            BoxSize box = new BoxSize { x = 10, y = 5 };
            int quantity = 5;
            int expectedRemainingBoxes = 0;

            // Act
            int actualRemainingBoxes = boxInventoryManager.AddBoxesToInventory(box, quantity);

            // Assert
            Assert.AreEqual(expectedRemainingBoxes, actualRemainingBoxes);
        }

        [TestMethod]
        public void AddBoxesToInventory_BoxSizeNotInInventory_CreatesNewInventoryEntry()
        {
            // Arrange
            BoxSize box = new BoxSize { x = 10, y = 5 };
            int quantity = 10;

            // Act
            int remainingBoxes = boxInventoryManager.AddBoxesToInventory(box, quantity);

            // Assert
            Assert.AreEqual(quantity, boxInventoryManager.GetBoxQuantity(box));
            Assert.AreEqual(0, remainingBoxes);
        }

        [TestMethod]
        public void GetBoxQuantity_ExistingBox_ReturnsQuantity()
        {
            // Arrange
            BoxSize box = new BoxSize { x = 10, y = 5 };
            int expectedQuantity = 10;
            boxInventoryManager.AddBoxesToInventory(box, expectedQuantity);

            // Act
            int actualQuantity = boxInventoryManager.GetBoxQuantity(box);

            // Assert
            Assert.AreEqual(expectedQuantity, actualQuantity);
        }

        [TestMethod]
        public void GetBoxQuantity_NonExistingBox_ReturnsNegativeOne()
        {
            // Arrange
            BoxSize box = new BoxSize { x = 10, y = 5 };

            // Act
            int actualQuantity = boxInventoryManager.GetBoxQuantity(box);

            // Assert
            Assert.AreEqual(-1, actualQuantity);
        }

        [TestMethod]
        public void SelectBoxesForPurchase_WhenBoxNotInInventory_ReturnsBestFitBoxes()
        {
            // Arrange
            var targetBox = new BoxSize { x = 10, y = 10 };
            var quantity = 5;
            boxInventoryManager.AddBoxesToInventory(targetBox, quantity);

            // Act
            var selectedBoxes = boxInventoryManager.SelectBoxesForPurchase(targetBox, quantity);

            // Assert
            Assert.IsNotNull(selectedBoxes);
            Assert.IsTrue(selectedBoxes.Count > 0);
            Assert.IsTrue(selectedBoxes.ContainsKey(targetBox));
            Assert.IsTrue(selectedBoxes[targetBox] >= quantity);
        }

        [TestMethod]
        public void PurchaseBoxes_WhenInventoryUpdated_StatusMessagesToUIContainsExpectedMessages()
        {
            // Arrange
            var selectedBoxes = new Dictionary<BoxSize, int>();
            boxInventoryManager.AddBoxesToInventory(new BoxSize { x = 10, y = 10 }, 1);
            boxInventoryManager.AddBoxesToInventory(new BoxSize { x = 20, y = 20 }, 2);

            selectedBoxes.Add(new BoxSize { x = 10, y = 10 }, 1);
            selectedBoxes.Add(new BoxSize { x = 20, y = 20 }, 1);

            // Act
            var statusMessages = boxInventoryManager.PurchaseBoxes(selectedBoxes);

            // Assert
            Assert.IsNotNull(statusMessages);
            Assert.IsTrue(statusMessages.Count > 0);
            Assert.IsTrue(statusMessages.Contains("Box of size (X=10, Y=10) got out of stock and removed"));
            Assert.IsTrue(statusMessages.Contains("Warning: Box of size (X=20, Y=20) has under 5 boxes left!"));
        }

        [TestMethod]
        public void GetUnpurchasedBoxesPastThreshold_NoUnpurchasedBoxes_ReturnsEmptyList()
        {
            // Arrange
            var days = 7;
            var expectedUnpurchasedBoxes = new List<BoxSize>();

            // Act
            var actualUnpurchasedBoxes = boxInventoryManager.GetUnpurchasedBoxesPastThreshold(TimeSpan.FromDays(days));

            // Assert
            CollectionAssert.AreEqual(expectedUnpurchasedBoxes, actualUnpurchasedBoxes);
        }

        [TestMethod]
        public void GetUnpurchasedBoxesPastThreshold_UnpurchasedBoxesExist_ReturnsUnpurchasedBoxes()
        {
            // Arrange
            var days = 7;
            var box1 = new BoxSize { x = 10, y = 10 };
            var box2 = new BoxSize { x = 20, y = 20 };
            boxInventoryManager.AddBoxesToInventory(box1, 1);
            boxInventoryManager.AddBoxesToInventory(box2, 2);

            // Simulate a purchase of box1
            boxInventoryManager.PurchaseBoxes(new Dictionary<BoxSize, int> { { box1, 1 } });

            // Act
            var actualUnpurchasedBoxes = boxInventoryManager.GetUnpurchasedBoxesPastThreshold(TimeSpan.FromDays(days));

            // Assert
            CollectionAssert.DoesNotContain(actualUnpurchasedBoxes, box2);
            CollectionAssert.DoesNotContain(actualUnpurchasedBoxes, box1);
        }

        [TestMethod]
        public void RemoveExpiredBoxesPastThreshold_ExpiredBoxesExist_ExpiredBoxesRemoved()
        {
            // Arrange
            var days = 7;
            var box1 = new BoxSize { x = 10, y = 10 };
            var box2 = new BoxSize { x = 20, y = 20 };
            boxInventoryManager.AddBoxesToInventory(box1, 1);
            boxInventoryManager.AddBoxesToInventory(box2, 2);

            // Simulate a purchase of box1
            boxInventoryManager.PurchaseBoxes(new Dictionary<BoxSize, int> { { box1, 1 } });

            // Act
            var unpurchased = boxInventoryManager.GetUnpurchasedBoxesPastThreshold(TimeSpan.FromDays(days));

            // Assert
            Assert.IsTrue(unpurchased.Count == 0);
        }

        [TestMethod]
        public void SplitPurchase()
        {
            var box1 = new BoxSize { x = 18, y = 17 };
            var box2 = new BoxSize { x = 19, y = 19 };
            var box3 = new BoxSize { x = 20, y = 20 };
            boxInventoryManager.AddBoxesToInventory(box1, 2);
            boxInventoryManager.AddBoxesToInventory(box2, 15);
            boxInventoryManager.AddBoxesToInventory(box3, 3);

            var ans1 = boxInventoryManager.SelectBoxesForPurchase(new BoxSize { x = 14, y = 14 }, 15);
            var ans2 = boxInventoryManager.SelectBoxesForPurchase(new BoxSize { x = 14, y = 14 }, 20);
            var ans3 = boxInventoryManager.SelectBoxesForPurchase(new BoxSize { x = 14, y = 14 }, 120);


            var expectedSelectedBoxes = new Dictionary<BoxSize, int>
            {
                {box1, 2 },
                {box2, 13}
            };

            Assert.IsTrue(expectedSelectedBoxes.ContainsKey(box1));
            Assert.IsTrue(expectedSelectedBoxes.ContainsKey(box2));
            CollectionAssert.AreEqual(expectedSelectedBoxes, ans1);
            Assert.IsTrue(ans3 == null);


        }
    }
}