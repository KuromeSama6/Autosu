# Autosu!
Osu! autopilot script, made with CefSharp Winforms. A result of lots of modules and functions crammed into a single application.

**Logging out and disconnection from network is recommended.** Usage of this software in competitive gameplay is CLEARLY against the Osu! Rules and WILL lead to account restrictions.

This software is made with the intent of experimenting human movement simulation with the Osu! game. You are not required to disable/quit any antivirus software before beginning.

KuromeSama6, Standard Arc Hiktal, and all related developers may not be held liable for any Osu! account restrictions as a result of using this software.

This software is open source but private repository, until a stable release is produced.


## Video Showcase
-

## Downloadable Executables
-

## Clean Installation
1. Proceed to the "Releases" section of this repository and download a release.
2. Extract the files from the ZIP file, or install the software with the MSI installer, into any directory of your choice.
3. Open Autosu!.exe.
4. You will see the song selection screen. In the bottom there are two boxes, a *Beatmap Selection* box and a *Difficulty Selection*. Both boxes' contents are scrollable if its content overflows. Both boxes should be blank and 0 Beatmaps should be found.
5. Copy the absolute path of your Osu! installation directory (where the osu!.exe file is located), and paste it in the "Osu! Path" input box. Hit enter and beatmap should now load. This setting saves automatically.
6. Click on a beatmap and a difficulty next to it. The wide box below it should now show. Click "Initialize Autosu!" to confirm song selection and proceed to the autopilot screen.

## Autopilot Usage
The Osu! bot is referred to as "autopilot". 

In the autopilot screen, you will see five(5) boxes. They are:

1. Autopilot Master Control - The top wide box with the beatmap thumbnail, name, and difficulty. To the right of that box are toggle switches `A/P CMD`, `A/P DEBUG`, and a long press button `DISENGAGE`.
- `A/P CMD` - Enables autopilot at full control. Must use `DISENGAGE` to disable.
- `A/P DEBUG` - Enables autopilot in debug mode. 1) Will not hit keys, and 2) will play the beatmap audio, in order to test syncronization with osu!, and 3) starts autopilot procedure immediately without waiting for beatmap to start in osu!.
- `DISENGAGE` - Master disengage. Disables autopilot.

2. Autopilot Heads-Up Status Panel - The wide box below the Box 1 that has a bunch of status indicators.


A status indicator can be:
1. Gray/Unlit - Not Active
2. Orange/Amber - Active. Often indicates a warning.
3. Green - Active. Often indicates a module is armed or running.

- `A/P`
	- Amber: Autopilot is enabled but not at full capacity yet.
	- Green: Autopilot is at full capacity.
- `A/P ARM`
	- Amber: Autopilot is armed and is awaiting song start.
	- Green: Song cycle has already started..
- `A/P P/RST`
	- Amber Flash: Autopilot has been (soft) disengaged. Push this button to suppress warning.
- `OPMDE SWITCH`
	- Amber: Auto-switch is armed and is awating user input.
	- Green: Auto-switch is allowing human input and disables bot input.
- `ACCURACY`
	- Amber: Accuracy is too high or too low. If the accuracy is too low, or is too high while `DETENT STBY` is on, autopilot will disengage. 
	- Green: Accuracy is OK.
	
	
