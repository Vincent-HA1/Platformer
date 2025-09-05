GUIDE ON HOW TO USE THE CODE

TESTSTAGE scene is the scene you should base all the scenes on.

It is organised in such a way that all the game objects there are meant to be used. Follow the guidelines (e.g. put enemies under enemies, collectibles under collectibles etc.)

The moving spikes are there automatically, add them to have a running stage. It does not move Y position, so keep the stage low. For running stages, need the super spring (moves player horizontally and vertically).

To create a new stage, create a scene, but in stage select, need to add a stage waypoint for it. Need to type the name of the scene, and the number of big coins there. This needs to match up with how many coins there are in the stage.

The game saves at the end of each stage.

Need to manually create gaps in the stage, and manually drag the camera Boundary Box gameobject to match the size of the stage. Also need to create stage checkpoints at points in the stage.

Remember enemies with the JumpEnemy script should not be put anywhere with ledges or platforms. Ideally put them in dips or flat terrain. Maybe it's fine if they jump off ledges though. Could add so that they die.

Fish should be put in easy water areas. They stop on touching a wall, so be careful.

Ideally use non jumping enemies as well.

