HandbrakeCluster
================

A simple commandline tool to cluster handbrake file processing


Requirements
============

to use, you need [.NET Framework 4.5][1], [MSMQ][2] installed on your client and server machines, and [Handbrake][3] installed on each node. 

How to Use (work in prgress)
============================

* on your master node, create an private MSMQ named HandbrakeCluster. this should be marked as transactional.
* copy handbrakecluster to each of your nodes


change the config of the adder program to point at the correct MSMQ. for example: 
* FormatName:Direct=TCP:192.168.0.101\Private$\HandbrakeCluster
change the IP address to your machine name or IP. 


on your head node, run the "adder" program with the following params
* directory of source files (file share)
* type of files to read (eg *.avi, *.mkv)
* where you want the finished files places (file share)
* file extention (.m4v or .mp4 as example)

example: HandBrakeClusterAdder \\NAS\TVShows\CSI\ *.avi \\NAS\iTunesWatchFolder .m4v

If you have spaces in your folder names, you must wrap them with quotes (eg "\\NAS\TV Shows\CSI")

on each node, change config to point to correct path for HandBrake (x64 or x86 supported, point at HandbrakeCLI.exe) and also set the location of your MSMQ queue... 

run the program...

I have tested this on Domain Joined machines, so each machine knew each other and could talk without issues. Also, the EXE was run as Admin, but not sure if thats required.