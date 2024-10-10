# NodeLogger

**NodeLogger** is a mod designed for creating NodeMaps in **Crab Game**, primarily used for Pathfinding applications.

## Features
This mod allows you to easily log positions as nodes while walking around in Practice mode. You can also create large surfaces of nodes using corner-based input, remove unwanted nodes, and load previously saved NodeMaps for further editing.

## Usage
- **Mode**: Works only in **Practice Mode**.
- As you walk, nodes are created in the default logging mode.

## Controls

| Key | Action |
| --- | ------ |
| **T** | Start/Stop logging nodes. When logging is stopped, a file is created (or replaced if it already exists) in the `NodeLogger/NodeMap` folder. |
| **P** | Pause/Unpause node logging. |
| **C** | Corner mode (available only when logging is paused). Allows you to create large surfaces of nodes using two corners. |
| **R** | Removes the last added node or the last created surface of nodes (only available when logging is paused). |
| **F** | Removes the selected node (only available when logging is paused). |
| **L** | Load a previously saved NodeMap from a file for further editing. |

## How It Works
1. **Node Logging**: As you move around in Practice mode, nodes will be logged automatically.
2. **Surface Creation**: In corner mode (using `C`), you can define two corners to create a surface covered with nodes.
3. **Node Removal**: You can either remove individual nodes using `F` or remove entire surfaces using `R`.
4. **Load/Edit NodeMaps**: Load existing NodeMaps using `L`, and make further adjustments as needed.

## Folder Structure
- The generated NodeMaps are saved in the `NodeLogger/NodeMap` directory.
- If a file already exists when logging is stopped, it will be replaced.
