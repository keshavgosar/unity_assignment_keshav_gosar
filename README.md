# Gravity Manipulation Puzzle (Unity Developer Test)

This repository contains the completed Unity Game Developer Task. The objective of this project is to create a 3D puzzle game where the player manipulates gravity to navigate the environment and collect all required cubes before the time limit expires.

## Standalone Builds

As per the assignment requirements, standalone executable builds for both **Windows** and **Mac** have been provided. To prevent repository bloat, these builds are not tracked in the source code history.

**To access the builds:**
*   Please navigate to the **[Releases](../../releases)** section on the right-hand side of this repository page to download the `.zip` files for Windows and Mac.

## Controls

The game utilizes the modern Unity Input System. The controls map directly to the assignment requirements:

*   **W, A, S, D:** Move character
*   **Space:** Jump
*   **Arrow Keys (Up, Down, Left, Right):** Select gravity manipulation direction (Displays Hologram)
*   **Enter / Return:** Apply gravity shift to the selected direction

## Features Implemented

*   **Character Movement & Physics:** Smooth, surface-aligned movement utilizing Rigidbody physics. The player correctly aligns to the new floor during gravity shifts without jittering or rotation conflicts.
*   **Gravity Manipulation System:** Arrow keys project a hologram of the character's target orientation. Pressing Enter seamlessly transitions the game's gravity and player rotation to the new plane.
*   **Dynamic Third-Person Camera:** A custom camera controller that dynamically adjusts its "Up" vector during gravity shifts to prevent disorienting screen-rolling. Includes a SphereCast collision system to prevent clipping through environmental geometry.
*   **Timer & Game State Conditions:** 
    *   2-minute countdown timer implemented.
    *   The player must collect 5 cubes to win.
    *   Falling into the abyss (detected via downward Raycasts) or running out of time results in a Game Over state.
*   **Clean Architecture:** 
    *   Utilizes C# Events/Actions to cleanly decouple the Game Logic (`GameManager`) from the User Interface (`InGameUI`, `GameOverUI`).
    *   Code is thoroughly documented using XML summary tags for maximum readability.
    *   Implements the New Input System (`UnityEngine.InputSystem`) for modern, scalable input handling.

## How to Run the Project (Editor)

1. Clone this repository to your local machine.
2. Open Unity Hub and click **Add -> Project from disk**.
3. Select the cloned repository folder.
4. Once the project loads, navigate to the `Assets/Scenes` folder and open the sample scene.
5. Press **Play** in the Unity Editor.

## Built With

*   **Unity Engine:** [6000.3.10f1]
*   **Language:** C#
*   Provided assets, characters, animations, and materials from the base project.
