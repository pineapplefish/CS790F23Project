# Assignment 5: Virtual Teleportation

In this assignment, you will implement a custom teleportation script and then add some advanced functionality.

## Submission Information

You should fill out this information before submitting your assignment.  Make sure to document the name and source of any third party assets such as 3D models, textures, or any other content used that was not solely written by you.  Include sufficient detail for the instructor or TA to easily find them, such as a download link. Also inlude directions for how the teleportation direction is controlled (details below).

Name: 

UWM Email:

Third Party Assets:

Instructions for controlling teleport directions:

## Getting Started

Clone the assignment using GitHub Classroom.  The project has been configured for the Oculus Quest, and the Oculus Integration package has already been imported. A basic empty scene (`Assignment5.unity`) with an Oculus Camera Rig is also included, and this should be the scene that you edit for the assignment.

## Rubric

Graded out of 20 points. 

1. Create a virtual scene that is a suitable testbed for virtual teleportation. It should be large enough that you can't walk through it without using virtual locomotion, and it should contain walkable surfaces at two or more elevations. For example, you might have a ground plane and a raised platform that the user will need to teleport to. Other than that, you are free to design the environment however you want, but make sure to sure low poly assets that can render smoothly on the Quest! (2)
1. Add a `LineRenderer` laser pointer to the controller. You can feel free to customize its appearance however you want. For the purposes of this assignment, you can chose either the left or right controller. You do not have to implement this functionality on both of them. (2)
1. The laser pointer should only appear when the thumbstick is pressed forward nearly all the way. When the thumbstick is in the rest position, it should be invisible. (2)
1. Designate at least 5 surfaces as teleportation targets. How you do this is up to you (e.g. you could add and check for a specific script, use tags, etc.). If the laser pointer intersects one of the teleportation targets, the `LineRenderer` color should change. If it does not intersect one of the teleportation targets, the color should be set to a default color (that you can choose). The teleportation targets should not allow users to teleport to non-flat surfaces (i.e., the user should not be able to teleport to the side of a surface). An easy way to accomplish this is to add invisible quad (or thin cube) primatives on top of the surfaces you want the user to teleport to, and designate those invisible primitives as the teleportation target. (2)
1. Now, implement the basic teleportation. When the user is pointing at a teleportation target and releases the thumbstick, move the camera rig so that the user is standing on point where the laser pointer intersected the teleportation target. Make sure to adjust the height correctly! (4)
1. Next, we are going to add the ability to control the direction after teleportation.  First, you should add an arrow or another visual indicator that will be displayed if the user is pointing at a valid teleportation point.  See Figure 2 in [Bozgeyikli et al.](https://dl.acm.org/doi/abs/10.1145/2967934.2968105) for an example.  You can decide how to best control the direction of the indicator, such as a thumbstick or the rotation of the user's hand.  Feel free to implement this however you want, but make sure that the user has a way to control the direction in all 360 degrees.  Make sure to describe the instructions in the submission of your readme file so we know how to use it properly. (4)
1. Finally, you can complete this assignment by modifying the camera rig's rotation so that the user's viewpoint is pointing in the specified direction after the teleport.  (4)

**Bonus Challenge:**  Make your teleportation script smarter!  When the user is pointing directly at a teleportation target , the teleport should work using the direct straight line raycast as described above.  If they are not pointing at a valid target, then the script should use a parabolic arc to attempt to find another target.  This will allow the user to point above the ground, and the laser pointer will adapt and curve downwards.  Note that this is actually quite complicated to implement, so you are free to use code from online or third party assets to implement the trajectory calculations and/or line drawing for the parabolic arc.  The teleportation functionality must be solely written by you.  Make sure to include comments on any external code and document the source in your readme file.  (2)

Make sure to document all third party assets in your readme file. ***Be aware that points will be deducted for using third party assets that are not properly documented.***

## Submission

You will need to check out and submit the project through GitHub classroom.  **Make sure your APK file is in the root folder.** Do not remove the `.gitignore` or `README.md` files.

Please test that your submission meets these requirements.  For example, after you check in your final version of the assignment to GitHub, check it out again to a new directory and make sure everything opens and runs correctly.  You can also test your APK file by installing it manually using [SideQuest](https://sidequestvr.com/).

## Acknowledgments

This assignment is a modified version of an assignment from a class taught by Professor Evan Suma Rosenberg at the University of Minnesota.   
