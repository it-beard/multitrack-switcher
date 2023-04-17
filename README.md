# Premier Rpo Multitrack Switcher
Multitrack switcher for Premier Pro. Helps to change video tracks for podcasts and interviews on two persons filmed by two or three cameras.
Works with two or three traks (two or three cameras)

Used technologies: `Avalonia UI` & `.NET 7`

<img src="https://user-images.githubusercontent.com/45439635/232473909-b7f01408-d5b2-4672-b5be-2e3e24d88b36.png" width="500" />

### How it works
The programm analyzing your .prproj file and two audio tracks (primary and secondary speakers tracks). then based on this info programm generating result .prproj file with cutted multi-cam sequence. Programm shown camera 1 if primary speaker tolk and camera 2 if secondary speaker tolk. If speakers tolk together - programm shown camera 1 and camera 2 one by one (shown camera 3 if you select `three cameras mode`). Also programm diluting long frames (longer than 45 secs) by ohter camera plans.


## How to start
### Prerequisite
* Sequence - `25 frames per second`. It needed for correct sync process. Currently programm doess not support other framerates.

![image](https://user-images.githubusercontent.com/45439635/232479330-0fb90ad2-7ceb-4bcc-a91e-30b4e66f8cb6.png)

* Nested multi-cam sequence plased on Track `V1`. 

![image](https://user-images.githubusercontent.com/45439635/231892845-914c03f9-c2c0-4132-964a-94847b95c356.png)

* Inside nested multi-cam sequence:
  * Track `V1` - primary speaker video 
  * Track `V2` - secondary speaker video
  * Track `V3` - master-plan video (for `three cameras mode`)
  ![image](https://user-images.githubusercontent.com/45439635/232479131-2d1ba356-0edf-4c4d-862c-91f77ac34fc4.png)
* Render full audio track for primary speaker (`.wav`, `48000 Hz`)
* Render full audio track for secondary speaker (`.wav`, `48000 Hz`)
   * Both tracks should have same duration as nested multi-cam sequence
![image](https://user-images.githubusercontent.com/45439635/231892845-914c03f9-c2c0-4132-964a-94847b95c356.png)
   * You do not needed to include this tracks inside your .prprog like on image above - it is just for showing same duration of nested track and audio tracks.

### Workflow for two cameras (good for podcasters)
* Select your `.prproj` file, `.wav` tracks for primary and secondary speaker. Click to `"Let's CUT IT!"` button. 
* After a while programm propose you save result `.prproj` file. Save it in the folder of origianl `.prproj` file. Then open this new `.prproj` file in Premier Pro and have fun! 😎

Result multi-cam will be shown smtg like this:
![image](https://user-images.githubusercontent.com/45439635/231901602-985dc43a-13d4-4591-843f-42d0810d8580.png)

### Workflow for three cameras (good for interviews)
* Select your `.prproj` file, `.wav` tracks for primary and secondary speaker. Click to `"Let's CUT IT!"` button.
* Select `three cameras mode` by click on `Three (with master-plan)` radio-button

![image](https://user-images.githubusercontent.com/45439635/232481189-6257cb08-eb1c-4d32-8c13-8ab9ba90c504.png)
* Select preffered dilute mode
   * `TwoCameras` - using plans one of speackers for diluting
   * `ThreeCameras` - using master-plans for diluting
   * `Random` - using random plans
   
   ![image](https://user-images.githubusercontent.com/45439635/232481481-366c98e3-7c1a-4840-aa71-08c83086c934.png)
* After a while programm propose you save result `.prproj` file. Save it in the folder of origianl `.prproj` file. Then open this new `.prproj` file in Premier Pro and have fun! 😎


### Tune settings
* You can change `sensitivity of detection` for primary and secondary person. The value should be between 1 and 0. Higher number - higher sensitivity. Suggest start from default numbers. Can be float number (like "0.65")
* You can change `number of dilute iteration`. Dilute iteration needed to dilute long frames with other plans. A long frame is anything longer than 45 seconds. For each iteration, long frames are split in half. Accordingly, it may take several iterations to split a long monologue. We recommend that you start with 3.
   * `Zero` means that you disable diluting process.
* You can change `dilute frames duration` (from 1 up to 40 secs)



## How to run
### On Mac
* Go to `multitrack-switcher/Published/Mac/`folder and just run `PRProjMulticamCreator` - this is compiled mac version of programm.
  * Allow open apps not from App Store on your mac ([how](https://macpaw.com/how-to/unidentified-developer-mac)).
  * Can be required setup .NET7 runtime on your computer - https://dotnet.microsoft.com/en-us/download/dotnet/7.0
* Build & Compile - you just needed .NET7 environment

### On Windows or Linux
* Build & Compile - you just needed .NET7 environment


