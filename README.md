# Warehouse Box Inventory Management System
This is a .NET project designed for managing a packing box warehouse efficiently and effectively. The warehouse keeps track of boxes of different sizes, each having a square bottom (with side X) and height Y, where X and Y can be any numbers.

# Key Features
**Inventory Management:** Allows filling the inventory based on the size of the box. New sizes are added as new records while existing sizes simply update the quantity. The system prevents overstocking beyond a maximum threshold defined in the configurations.

**Box Data Display:** Enables users to view the quantity and other details of boxes based on the given size.

**Gift Box Finder and Purchase:** This feature helps users to find the most suitable box for a gift and perform a purchase operation. It smartly identifies the most suitable size by looking for a box with the exact bottom size or a slightly larger one. It also considers the height while finding the most suitable box. Stock alerts and out-of-stock management is also handled.

**Bulk Purchase Support:** It provides the functionality to buy a certain quantity of boxes. It handles cases of partial fulfillment and offers alternatives for user approval.

**Stale Box Finder:** It displays all boxes that haven't been purchased for more than a specified period (T), where T is a configurable parameter.

**Expired Box Deletion:** Boxes that remain unpurchased for more than time T are considered expired and are deleted from the warehouse.

**Data Persistence (Bonus):** A background service manages and updates a database (or file) to keep the state persistent.

# Project Structure
The project is modular with a focus on code cleanliness, .NET naming conventions, indentations, and comments for maximum readability. It is optimized for high performance and allows the use of existing data structures or the implementation of custom ones. The project also includes a loader program for data ingestion.
