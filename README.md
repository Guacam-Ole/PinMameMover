# PinMameMover
This is a simple Utility to move the _VPinMame_ - Window to the expected position after VPinBall started.

## Config-Paramters (PinMameMover.Config)
_PlayerName_ The name of the Player (usually Virtual Pinball). This is the Prefix and not case-sensitive. So "vpin" would find "VPinBall922" and so on
_MameName_ The ProcessName from VPinBall. Should be always _MAME_
_X_, _Y_ Position to move the window to
_Width_, _Height_ Size of the VPinMameWindow
_Delay_  Pause to be used after the VPinBall-Window has been found before the VPInMame-Window is visible. 0=No Delay
_Interval_ Recheck every xx seconds. -1= No recheck

## Startup-Parameters
_none_ Detect newly created threads, search for the MAME-WIndow and resize it
_MANUAL_ Detect and resize **ONCE**. The MAME-Window must already be visible. For testing purpouses
_NOMOVE_ Only display the current position and size. Don't move

## Hints
### Finding the right position:
* Set the Config-Paramter _Interval_ to a 10 seconds. Then
* Start with "NOMOVE" - Parameter. 
* You should now open your game and wait until the MAME-Window is visible. 
* Move and resize the Mame-Window the the Position you want. 
* Note the Values for X,Y, Width and Height
* Stop PinMameMover
* Enter the Values for X,Y,Width and HEight into the Config

### Auto-Move/Resize 
#### Using Visual Pinball directly
If you use Visual Pinball without a launcher, the VPinBall-Process will always be in Memory. That is why the new process-detection will not work. Simply set a value for "Interval" to some seconds. So every xx seconds we will recheck if the MAME-Window is present. This should have nearly no impact on the performance.

Start PinMameMover without any parameters and the Window should resize automatically

#### Using a launcher
If you use a launcher like PinballX, you can use the "smarter" detection. Put _Interval_ to -1. Instead use a value for _Delay_. I had good experiences with 5 seconds. You might increase that if you computer is very slow.

Start PinMameMover without parameters and the MAME-Window should resize automatically after VPinball is started by your launcher.

## Reduce Output
You can reduce the Output (and logging) by changing the level unter _log4net/root/level_ Change the value to INFO, or WARN to get less output. If you want no output at all, remove the line _appender-ref ref="ConsoleAppender"_


