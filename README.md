HandbrakeCluster
================

A simple commandline tool to cluster handbrake file processing


How to Use (work in prgress)
----------------------------

on your master node, create an private MSMQ named HandbrakeCluster
copy handbrakecluster to each of your nodes

on your head node, run the "adder" program with the following params
* directory of source files (file share)
* type of files to read (eg *.avi, *.mkv)
* where you want the finished files places (file share)
* file extention (.m4v or .mp4 as example)

on each node, change config to point to correct path for HandBrake (x64 or x86 supported, point at HandbrakeCLI.exe)
run the program...

you will need MSMQ on each machine, and each machine needs to see the head node...

GUI Tool
--------
The GUI tool is a WPF app which takes a few minor paramaters...

* destionation is where you want the files to go (file share)
* drag and drop the files directly into the window and they will be added
* click the add button and they will be written to the queue.