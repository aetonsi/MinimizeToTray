# MinimizeToTray
Windows tool to minimize/restore a terminal application to the system's tray.

# Usage
```powershell
.\MinimizeToTray.exe cmd /k echo hello
```
This starts `cmd /k echo hello` already minimized to the system's tray.

You can see the command output by opening its window by double clicking the tray icon:

![image](https://user-images.githubusercontent.com/18366087/200303475-ba8d1aac-80c3-4c6a-9cc2-5ab0c5446aa1.png)
![image](https://user-images.githubusercontent.com/18366087/200303530-bf3bbbee-2573-4f49-93e9-f03f76063481.png)

You have a few additional options if you right click the tray icon:

![image](https://user-images.githubusercontent.com/18366087/200303619-c2758245-905b-4fa5-ad0c-301cab37b77c.png)


# Technical specifications and limitations
- currently, it **cannot** handle user input! It can only show the command's output
- it makes use of `cmd.exe /c` to launch the requested application
- it redirects all stderr to stdout via `2>&1`
