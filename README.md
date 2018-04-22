# CWB_AR_Spaces_Editor
Coders Without Borders Augmented Reality Object Editor

In Unity3D, select AR_Spaces as a project folder to open.
After doing so, open the scene "Editor.Unity" and run the program by pressing the play button.

This application does not work after building, because Vuforia will not support it.

Login credentials can be found on the Web Server under /Credentials.
Valid usernames are the titles of the text files in the Credentials directory (minus the leading underscores).
Valid passwords are in the text documents themselves.
I would list usernames and passwords here, but this is a public-viewable readme, so I'd rather not.

The Vuforia license and access/secret keys are on my (Kevin's) account.
It's a throwaway license and key set, but I'd still prefer if you swapped them out for your own.
I put up images on our Google Drive account (under Screenshots/Maintenance Manual/Setting Up a New Project) that
show pictures on how/where to place the 5 keys (license, client_access, clien_secret, server_access, and server_secret).

The other folder, ARMATestStuff, contains a couple of models and images to test the Editor with.
The editor will open a native file browser that works on both Mac and Windows; if you don't have any
.obj or image files laying around, you can use the stuff in ARMATestStuff.

Notes:
When saving large files, it may take a second for the program to respond (so if it looks like it froze, give it a second or two).

Likewise, when modifying an object you were working on, it has to download all of its assets from the Web Server, this could also take a second.

Any image file you save to the Web Server (Texture and Marker) are NOT viewable, (they are not .jpg and .png files like they say they are!).

If the GUI does not respond to your clicks, it is probably because it lost focus. Click on something else (like a different tab) in the Unity window,
and then try again.