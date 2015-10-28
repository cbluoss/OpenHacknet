// Decompiled with JetBrains decompiler
// Type: Hacknet.MusicManager
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Hacknet
{
    public static class MusicManager
    {
        private const int PLAYING = 0;
        private const int FADING_OUT = 1;
        private const int FADING_IN = 2;
        private const int STOPPED = 3;
        private static readonly float DEFAULT_FADE_TIME = Settings.isConventionDemo ? 0.5f : 2f;
        public static float FADE_TIME = DEFAULT_FADE_TIME;
        private static float fadeTimer;
        private static bool initialized;
        public static bool dataLoadedFromOutsideFile = false;
        private static readonly Dictionary<string, Song> loadedSongs = new Dictionary<string, Song>();
        private static bool IsMediaPlayerCrashDisabled;
        public static Song curentSong;
        private static Song nextSong;
        private static string nextSongName;
        public static bool isPlaying;
        public static bool isMuted;
        public static string currentSongName;
        private static float destinationVolume;
        private static float fadeVolume;
        private static int state;
        private static ContentManager contentManager;

        public static void init(ContentManager content)
        {
            try
            {
                contentManager = content;
                currentSongName = "Music\\Revolve";
                curentSong = content.Load<Song>(currentSongName);
                isPlaying = false;
                if (!dataLoadedFromOutsideFile)
                {
                    isMuted = false;
                    MediaPlayer.Volume = 0.3f;
                }
                destinationVolume = MediaPlayer.Volume;
                state = 3;
                initialized = true;
            }
            catch
            {
            }
        }

        public static void playSong()
        {
            try
            {
                if (isPlaying || !initialized)
                    return;
                if (!Settings.soundDisabled)
                {
                    MediaPlayer.Play(curentSong);
                    MediaPlayer.IsRepeating = true;
                }
                isPlaying = true;
                state = 0;
            }
            catch
            {
            }
        }

        public static void toggleMute()
        {
            isMuted = !isMuted;
            MediaPlayer.IsMuted = isMuted;
        }

        public static void setIsMuted(bool muted)
        {
            isMuted = muted;
            MediaPlayer.IsMuted = isMuted;
        }

        public static void stop()
        {
            MediaPlayer.Stop();
            isPlaying = false;
            state = 3;
        }

        public static float getVolume()
        {
            return destinationVolume;
        }

        public static void setVolume(float volume)
        {
            MediaPlayer.Volume = volume;
            destinationVolume = volume;
        }

        public static void playSongImmediatley(string songname)
        {
            try
            {
                curentSong = contentManager.Load<Song>(songname);
            }
            catch (Exception ex)
            {
            }
            if (!(curentSong != null))
                return;
            isPlaying = false;
            currentSongName = songname;
            playSong();
            setVolume(destinationVolume);
        }

        public static void loadAsCurrentSong(string songname)
        {
            try
            {
                curentSong = contentManager.Load<Song>(songname);
            }
            catch (Exception ex)
            {
            }
            if (!(curentSong != null))
                return;
            isPlaying = false;
            currentSongName = songname;
        }

        public static void transitionToSong(string songName)
        {
            try
            {
                if (!(currentSongName != songName))
                    return;
                var thread = new Thread(loadSong);
                thread.IsBackground = true;
                nextSongName = songName;
                thread.Start();
                Console.WriteLine("Started song loader thread");
                state = 1;
                fadeTimer = FADE_TIME;
                currentSongName = songName;
            }
            catch
            {
                Console.WriteLine("Error transitioning to Song");
            }
        }

        private static void loadSong()
        {
            try
            {
                nextSong = contentManager.Load<Song>(nextSongName);
                loadedSongs.Add(nextSongName, nextSong);
            }
            catch (ArgumentException ex)
            {
                nextSong = loadedSongs[nextSongName];
            }
        }

        public static void Update(float t)
        {
            switch (state)
            {
                case 0:
                    fadeVolume = destinationVolume;
                    FADE_TIME = DEFAULT_FADE_TIME;
                    break;
                case 1:
                    fadeTimer -= t;
                    fadeVolume = destinationVolume*(fadeTimer/FADE_TIME);
                    if (fadeVolume <= 0.0)
                    {
                        if (nextSong != null)
                        {
                            state = 2;
                            MediaPlayer.Volume = 0.0f;
                            curentSong = nextSong;
                            nextSong = null;
                            fadeTimer = FADE_TIME;
                            if (Settings.soundDisabled)
                                break;
                            if (IsMediaPlayerCrashDisabled)
                                break;
                            try
                            {
                                MediaPlayer.Play(curentSong);
                                break;
                            }
                            catch (InvalidOperationException ex)
                            {
                                IsMediaPlayerCrashDisabled = true;
                                if (OS.currentInstance == null || OS.currentInstance.terminal == null)
                                    break;
                                OS.currentInstance.write("-------------------------------");
                                OS.currentInstance.write("-------------WARNING-----------");
                                OS.currentInstance.write("-------------------------------");
                                OS.currentInstance.write("HacknetOS VM Audio hook could not be established.");
                                OS.currentInstance.write(
                                    "Music Playback Disabled - Media Player (VM Hook:WindowsMediaPlayer)");
                                OS.currentInstance.write("Has been uninstalled or disabled.");
                                OS.currentInstance.write("-------------------------------");
                                OS.currentInstance.write("-------------WARNING-----------");
                                OS.currentInstance.write("-------------------------------");
                                break;
                            }
                        }
                        fadeVolume = 0.0f;
                        break;
                    }
                    MediaPlayer.Volume = fadeVolume;
                    break;
                case 2:
                    fadeTimer -= t;
                    fadeVolume = destinationVolume*(float) (1.0 - fadeTimer/(double) FADE_TIME);
                    if (fadeVolume >= (double) destinationVolume)
                    {
                        MediaPlayer.Volume = destinationVolume;
                        state = 0;
                        break;
                    }
                    MediaPlayer.Volume = fadeVolume;
                    break;
            }
        }
    }
}