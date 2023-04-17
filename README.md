# Premier Rpo Multitrack Switcher
Multi-cam track switcher for Premier Pro. Helps you switch video tracks for podcasts and two-person interviews shot with two or three cameras.

Technologies used: `Avalonia UI` & `.NET 7`

<img src="https://user-images.githubusercontent.com/45439635/232473909-b7f01408-d5b2-4672-b5be-2e3e24d88b36.png" width="500" />

### How it works
The program analyzes your `.prproj` file and the two audio tracks (from the primary and secondary recording participant). Then, based on this information, the program generates the resulting `.prproj` file with multiple cameras switching in multi-cam sequence.

Basicaly the program switches on the camera from track `V1`, if the primary speaker speaks, and the camera from track `V2`, if the secondary speaker speaks. If the speakers speak together, the program switches on camera 1 and camera 2 in turn (if the `three cameras` mode is selected, the camera from track `V3` is switched on). Also the program knows how to split long frames (more than 45 seconds) with plans from other cameras.


## How to start
### Prerequisite
* The sequence is `25 frames per second`. This is necessary for proper synchronization. The program does not currently support other frame rates.

![image](https://user-images.githubusercontent.com/45439635/232479330-0fb90ad2-7ceb-4bcc-a91e-30b4e66f8cb6.png)

* The nested multi-camera sequence is located on track `V1`.

![image](https://user-images.githubusercontent.com/45439635/231892845-914c03f9-c2c0-4132-964a-94847b95c356.png)

* Inside the nested multi-cam sequence:
  * Track `V1` - primary speaker video 
  * Track `V2` - secondary speaker video
  * Track `V3` - master-plan video (for `three-camera mode`)
  ![image](https://user-images.githubusercontent.com/45439635/232479131-2d1ba356-0edf-4c4d-862c-91f77ac34fc4.png)
* Render the full sound track for the primary speaker (`.wav`, `48000 Hz`)
* Render the full sound track for the secondary speaker (`.wav`, `48000 Hz`)
   * Both tracks must have the same duration as the nested multi-camera sequence
![image](https://user-images.githubusercontent.com/45439635/231892845-914c03f9-c2c0-4132-964a-94847b95c356.png)
   * You do not need to include these tracks in your `.prprog` as in the picture above - the picture simply illustrates the same duration of the nested tracks and the audio tracks.

### Two-camera workflow (good for podcasters)
* Select your `.prproj` file, `.wav` tracks for primary and secondary speaker. Click to `"Let's CUT IT!"` button. 
* After a while (10 - 100 seconds) the program will prompt you to save the result to the `.prproj` file. Save it in the `.prproj` folder of the original file. Then open this new `.prproj` file in Premier Pro and have fun! 😎

Result multi-cam will be shown smtg like this:
![image](https://user-images.githubusercontent.com/45439635/231901602-985dc43a-13d4-4591-843f-42d0810d8580.png)

### Three-camera workflow (good for interviews)
* Select your `.prproj` file, `.wav` tracks for primary and secondary speaker. Click to `"Let's CUT IT!"` button.
* Set the `three-camera mode` by pressing the `Three (with master plan)` radio button.

![image](https://user-images.githubusercontent.com/45439635/232481189-6257cb08-eb1c-4d32-8c13-8ab9ba90c504.png)
* Select preffered dilute mode
   * `TwoCameras` - using the plans of one of the speakers to dilute
   * `ThreeCameras` - using master-plans to dilute
   * `Random` - using random plans to dilute
   
   ![image](https://user-images.githubusercontent.com/45439635/232481481-366c98e3-7c1a-4840-aa71-08c83086c934.png)
* After a while (10 - 100 seconds) the program will prompt you to save the result to the `.prproj` file. Save it in the `.prproj` folder of the original file. Then open this new `.prproj` file in Premier Pro and have fun! 😎


### Tune settings
* You can change the `detection sensitivity` for the primary and secondary speaker. The value should be between 1 and 0. A higher number means higher sensitivity. We suggest starting with the default value. The value can be fractional (e.g. `0.065`).
* You can change the `number of dilution iterations`. A dilution iteration is needed to dilute long frames with other plans. A long frame is any frame longer than 45 seconds. At each iteration, the long frames are split in half and diluted with a frame from another camera. Accordingly, it may take several iterations to divide a long monologue. We recommend that you start with value 3.
   * `Zero` means that you have disabled the dilution process.
* You can change the `duration` of the `dilution frame` (from 1 to 40 seconds).



## How to run
### On Mac
* Go to the `multitrack-switcher/Published/Mac/` folder and just run `PRProjMulticamCreator` - this is the compiled mac version of the program.
  * Allow apps not from the App Store to open on your mac ([how to](https://macpaw.com/how-to/unidentified-developer-mac)).
  * You may need to install [.NET 7 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0 ) on your computer.
* Compile final program from source files (you will need [.NET 7 environment](https://dotnet.microsoft.com/en-us/download/dotnet/7.0))

### On Windows or Linux
* Compile final program from source files (you will need [.NET 7 environment](https://dotnet.microsoft.com/en-us/download/dotnet/7.0))


