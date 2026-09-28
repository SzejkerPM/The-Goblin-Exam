The Goblin Exam is a simple humorous 2D dungeon crawler about a goblin who needs to prove himself before the older goblins and escape the dungeon with valuable treasures.
The game was made as a final project for a C# course in a team of 3 people at SWPS University. As we wanted to increase the difficulty and learn a new engine - we decided to use Godot 4.6 and JetBrains Rider for development.

Main game features:
- Player movement with sprint feature restricted by Stamina system
- Interaction with objects and NPCs
- Inventory system with funny "stack on head" visualization
- Items have their own weight
- Player can pick up items and sell them to NPC
- NPC who sells the dungeon key that is spawned randomly
- Treasures that are spawned randomly
- Enemy that has line of sight, patrols the dungeon and chases the player when noticed
- Enemy rests sometimes and can be outrun if the player is not overweight with items
- Few dungeon levels with locked doors
- A minigame inspired by Gothic to open chests with treasures
- Player HUD (stamina bar, interaction prompt)
- UI (game over screen)

My contribution to this project:
- Complete Player node and controller (movement, stamina-based sprint system, animation state management)
- Dynamic Inventory System with weight mechanic impacting player speed (item pickup and weight calculation)
- Flexible interaction system (modular interaction with NPCs, doors, and pickable items)
- NPC Merchant System (item trading logic and key purchasing condition)
- Key system (key unlocks the door)
- Player HUD (stamina bar, interaction button)
- Shared with friends: bug fixing, code review and final polish

Architecture Approach:
- Component-Driven design
- Interfaces
- Resources
- Event-Driven communication
- Game State Management (scene switch, restart)
- Custom Nodes & C# Composition
- Fields and Methods visibility order with simple names
- Choosing the simple way of writing code over the complex, hard to understand shorter versions suggested by IDE
- Fail fast with error logging
- Git version control with regular code review

Credits & Acknowledgments:
- Developed in collaboration with my university team - thank you for the great teamwork, brainstorming, and joint effort during the development process
- Thanks to all the creators who created free assets that we could use in this project
