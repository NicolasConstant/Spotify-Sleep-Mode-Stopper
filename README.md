# Spotify Sleep Mode Stopper

## Synopsis 

Spotify desktop app doesn't turn off Windows sleep mode, as a result when you're playing music on your PC and aren't working on it, the music will be stopped when the computers goes to sleep.

## Motivation

I wanted to stop this behavior as the Spotify devs doesn't handle this functionality  asked for years by users (even if multiple tickets are posted in their Ideas Community Website).

Here is a console and windows service apps that will analyze if Spotify is running and playing music, if so, it will prevent Windows to sleep.

## Installation

The best is to build and install the Windows Service. 

You can also find a Windows installer (x64 only for now) here:
https://github.com/NicolasConstant/Spotify-Sleep-Mode-Stopper/releases/download/1.0/SpotifySleepModeStopper.exe

## Libraries used

CSCore https://github.com/filoe/cscore

