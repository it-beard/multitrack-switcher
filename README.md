# Premier Rpo Multitrack Switcher
Multitrack switcher for Premier Pro. Helps to change video tracks for podcasts on two persons.
Works with two or three traks (two or three cameras)

Used `Avalonia UI` & `.NET 7`

End platform: Mac


## How to (for two cameras)
### Prerequisite
* Sequence 25 frames per second
* Nested multi-cam sequence on Track V1. Inside nested sequence
  * Primary person video on Track V1
  * Secondary person video on Track V2
  ![image](https://user-images.githubusercontent.com/45439635/231892960-96ca6e6e-03d7-484f-be1d-ce5666295086.png)

* Rendered audio track for primary person (.wav, 48000, 16kbs)
* Rendered audio track for primary person (.wav, 48000, 16kbs)
* Both tracks should have same duration as nested multi-cam sequence
![image](https://user-images.githubusercontent.com/45439635/231892845-914c03f9-c2c0-4132-964a-94847b95c356.png)

### How it works
Select your .prproj file, .wav tracks for primary and secondary persons. Click to "Let's CUT IT!" button. 

After that you gonna to found `Result.prproj` file right in folder with application. Just copy this file to you Premier Pro project folder, open it and have fun 😎

<img src="https://user-images.githubusercontent.com/45439635/231894195-2a2a6968-adae-4fa5-b4b4-799239ea07ec.png" width="500" />

### Result
![image](https://user-images.githubusercontent.com/45439635/231901602-985dc43a-13d4-4591-843f-42d0810d8580.png)

### Tune settings
* You can change sensitivity of detection for primary and secondary person. The value should be between 1 and 0. Higher number - higher sensitivity. Suggest start from default numbers. Can be float number (like "0.65")
* You can change number of dilute iteration. Dilute iteration needed to dilute long frames with other plans. A long frame is anything longer than 45 seconds. For each iteration, long frames are split in half. Accordingly, it may take several iterations to split a long monologue. We recommend that you start with 3.

### How to run
* Build & Compile - you just needed .NET7 environment
* Or go to `multitrack-switcher/Published/Mac/`folder and just run `PRProjMulticamCreator` - this is compiled mac version of programm



