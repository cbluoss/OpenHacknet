// Decompiled with JetBrains decompiler
// Type: Hacknet.MissionFunctions
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Hacknet.Misc;
using Microsoft.Xna.Framework.Audio;

namespace Hacknet
{
    public static class MissionFunctions
    {
        private static OS os;

        private static void assertOS()
        {
            os = OS.currentInstance;
        }

        public static void runCommand(int value, string name)
        {
            assertOS();
            if (name.Equals("addRank"))
            {
                os.currentFaction.addValue(value, os);
                var mail = MailServer.generateEmail("Contract Successful",
                    "Congratulations,\nThe client of your recent contract has reported a success, and is pleased with your work.\n" +
                    "You are now free to accept further contracts from the contact database.\n" +
                    (object) "\nYour Current Ranking is " + os.currentFaction.getRank() + " of " +
                    os.currentFaction.getMaxRank() + ".\n" + "\nThankyou,\n -" + os.currentFaction.name,
                    os.currentFaction.name + " ReplyBot");
                ((MailServer) os.netMap.mailServer.getDaemon(typeof (MailServer))).addMail(mail, os.defaultUser.name);
            }
            else if (name.StartsWith("addFlags:"))
            {
                foreach (
                    var flag in
                        name.Substring("addFlags:".Length)
                            .Split(Utils.commaDelim, StringSplitOptions.RemoveEmptyEntries))
                    os.Flags.AddFlag(flag);
            }
            if (name.Equals("triggerThemeHackRevenge"))
                os.delayer.Post(ActionDelayer.Wait(5.0), () =>
                {
                    var mail = MailServer.generateEmail("Are you Kidding me?",
                        "Seriously?\n\n" +
                        "You think you can just fuck with my stuff and leave without consequence? Did you think I wouldn't notice?\n" +
                        "\nDid you think I wouldn't FIND you!?\n" +
                        "\nYou're a pathetic scrit kiddie, you couldn't hack a fucking honeypot without your precious buttons and scrollbars.\n" +
                        "\nSay goodbye to your x-server, idiot." + "\n\nNaix", "naix@jmail.com");
                    ((MailServer) os.netMap.mailServer.getDaemon(typeof (MailServer))).addMail(mail, os.defaultUser.name);
                    os.delayer.Post(ActionDelayer.Wait(24.0), () =>
                    {
                        try
                        {
                            HackerScriptExecuter.runScript("ThemeHack.txt", os);
                        }
                        catch (Exception ex)
                        {
                            if (!Settings.recoverFromErrorsSilently)
                                throw ex;
                            os.write("CAUTION: UNSYNDICATED OUTSIDE CONNECTION ATTEMPT");
                            os.write("RECOVERED FROM CONNECTION SUBTERFUGE SUCCESSFULLY");
                            Console.WriteLine("Critical error loading hacker script - aborting");
                        }
                    });
                });
            else if (name.Equals("changeSong"))
            {
                switch (value)
                {
                    case 2:
                        MusicManager.transitionToSong("Music\\The_Quickening");
                        break;
                    case 3:
                        MusicManager.transitionToSong("Music\\TheAlgorithm");
                        break;
                    case 4:
                        MusicManager.transitionToSong("Music\\Ryan3");
                        break;
                    case 5:
                        MusicManager.transitionToSong("Music\\Bit(Ending)");
                        break;
                    case 6:
                        MusicManager.transitionToSong("Music\\Rico_Puestel-Roja_Drifts_By");
                        break;
                    case 7:
                        MusicManager.transitionToSong("Music\\out_run_the_wolves");
                        break;
                    case 8:
                        MusicManager.transitionToSong("Music\\Irritations");
                        break;
                    case 9:
                        MusicManager.transitionToSong("Music\\Broken_Boy");
                        break;
                    case 10:
                        MusicManager.transitionToSong("Music\\Ryan10");
                        break;
                    case 11:
                        MusicManager.transitionToSong("Music\\tetrameth");
                        break;
                    default:
                        MusicManager.transitionToSong("Music\\Revolve");
                        break;
                }
            }
            else if (name.Equals("entropyEndMissionSetup"))
            {
                runCommand(3, "changeSong");
                var comp1 = findComp("corp0#IS");
                var comp2 = findComp("corp0#MF");
                var comp3 = findComp("corp0#BU");
                var fileEntry1 = new FileEntry(Computer.generateBinaryString(5000), "HacknetOS.rar");
                var fileEntry2 = new FileEntry(Computer.generateBinaryString(4000), "HacknetOS_Data.xnb");
                var fileEntry3 = new FileEntry(Computer.generateBinaryString(4000), "HacknetOS_Content.xnb");
                var folder1 = comp1.files.root.folders[2];
                folder1.files.Add(fileEntry1);
                folder1.files.Add(fileEntry2);
                folder1.files.Add(fileEntry3);
                var folder2 = comp2.files.root.folders[2];
                folder2.files.Add(fileEntry1);
                folder2.files.Add(fileEntry2);
                folder2.files.Add(fileEntry3);
                var fileEntry4 = new FileEntry(fileEntry1.data, fileEntry1.name + "_backup");
                var fileEntry5 = new FileEntry(fileEntry2.data, fileEntry2.name + "_backup");
                var fileEntry6 = new FileEntry(fileEntry3.data, fileEntry3.name + "_backup");
                var folder3 = comp3.files.root.folders[2];
                folder3.files.Add(fileEntry4);
                folder3.files.Add(fileEntry5);
                folder3.files.Add(fileEntry6);
                comp1.traceTime = Computer.BASE_TRACE_TIME*7.5f;
                comp3.traceTime = Computer.BASE_TRACE_TIME*7.5f;
                comp2.traceTime = Computer.BASE_TRACE_TIME*7.5f;
                comp2.portsNeededForCrack = 3;
                comp1.portsNeededForCrack = 2;
                comp3.portsNeededForCrack = 2;
                var folder4 = findComp("entropy01").files.root.folders[2];
                folder4.files.Add(new FileEntry(PortExploits.crackExeData[25], "SMTPoverflow.exe"));
                folder4.files.Add(new FileEntry(PortExploits.crackExeData[80], "WebServerWorm.exe"));
            }
            else if (name.Equals("entropyAddSMTPCrack"))
            {
                var f = findComp("entropy01").files.root.folders[2];
                f.files.Add(new FileEntry(PortExploits.crackExeData[25],
                    Utils.GetNonRepeatingFilename("SMTPoverflow", ".exe", f)));
                os.saveGame();
            }
            else if (name.Equals("transitionToBitMissions"))
            {
                if (Settings.isDemoMode)
                {
                    runCommand(6, "changeSong");
                    if (Settings.isPressBuildDemo)
                        ComputerLoader.loadMission("Content/Missions/Demo/PressBuild/DemoMission01.xml");
                    else
                        ComputerLoader.loadMission("Content/Missions/Demo/AvconDemo.xml");
                }
                else
                    ComputerLoader.loadMission("Content/Missions/BitMission0.xml");
            }
            else if (name.Equals("entropySendCSECInvite"))
                os.delayer.Post(ActionDelayer.Wait(6.0),
                    () => ComputerLoader.loadMission("Content/Missions/MainHub/Intro/Intro01.xml"));
            else if (name.Equals("hubBitSetComplete01"))
            {
                os.delayer.Post(ActionDelayer.Wait(4.0), () => runCommand(1, "addRank"));
                runCommand(3, "changeSong");
                os.Flags.AddFlag("csecBitSet01Complete");
            }
            else if (name.Equals("enTechEnableOfflineBackup"))
            {
                var computer = Programs.getComputer(os, "EnTechOfflineBackup");
                Programs.getComputer(os, "EnTechMainframe").links.Add(os.netMap.nodes.IndexOf(computer));
                os.Flags.AddFlag("VaporSequencerEnabled");
                var folder1 = findComp("mainHubAssets").files.root.searchForFolder("bin");
                var folder2 = folder1.searchForFolder("Sequencer");
                if (folder2 == null)
                {
                    folder2 = new Folder("Sequencer");
                    folder1.folders.Add(folder2);
                }
                if (folder2.searchForFile("Sequencer.exe") != null)
                    return;
                folder2.files.Add(new FileEntry(PortExploits.crackExeData[17], "Sequencer.exe"));
            }
            else if (name.Equals("rudeNaixResponse"))
                AchievementsManager.Unlock("rude_response", false);
            else if (name.Equals("assignPlayerToHubServerFaction"))
            {
                os.allFactions.setCurrentFaction("hub", os);
                var computer = Programs.getComputer(os, "mainHub");
                var missionHubServer = (MissionHubServer) computer.getDaemon(typeof (MissionHubServer));
                var userDetail = new UserDetail(os.defaultUser.name, "reptile", 3);
                computer.addNewUser(computer.ip, userDetail);
                missionHubServer.addUser(userDetail);
                os.homeNodeID = "mainHub";
                os.homeAssetServerID = "mainHubAssets";
                runCommand(3, "changeSong");
                os.Flags.AddFlag("CSEC_Member");
                AchievementsManager.Unlock("progress_csec", false);
            }
            else if (name.Equals("assignPlayerToEntropyFaction"))
            {
                runCommand(6, "changeSong");
                AchievementsManager.Unlock("progress_entropy", false);
            }
            else if (name.Equals("assignPlayerToLelzSec"))
            {
                os.homeNodeID = "lelzSecHub";
                os.homeAssetServerID = "lelzSecHub";
                os.Flags.AddFlag("LelzSec_Member");
                AchievementsManager.Unlock("progress_lelz", false);
            }
            else if (name.Equals("lelzSecVictory"))
                AchievementsManager.Unlock("secret_path_complete", false);
            else if (name.Equals("demoFinalMissionEnd"))
            {
                os.exes.Clear();
                PostProcessor.EndingSequenceFlashOutActive = true;
                PostProcessor.EndingSequenceFlashOutPercentageComplete = 1f;
                MusicManager.stop();
                os.delayer.Post(ActionDelayer.Wait(0.2),
                    () => os.content.Load<SoundEffect>("Music/Ambient/spiral_gauge_down").Play());
                os.delayer.Post(ActionDelayer.Wait(3.0), () =>
                {
                    PostProcessor.dangerModeEnabled = false;
                    PostProcessor.dangerModePercentComplete = 0.0f;
                    os.ExitScreen();
                    os.ScreenManager.AddScreen(new DemoEndScreen());
                });
            }
            else if (name.Equals("demoFinalMissionStart"))
            {
                os.Flags.AddFlag("DemoSequencerEnabled");
                MusicManager.transitionToSong("Music/Ambient/dark_drone_008");
            }
            else if (name.Equals("CSECTesterGameWorldSetup"))
            {
                for (var index = 0; index < PortExploits.services.Count && index < 4; ++index)
                    os.thisComputer.files.root.folders[2].files.Add(
                        new FileEntry(PortExploits.crackExeData[PortExploits.portNums[index]],
                            PortExploits.cracks[PortExploits.portNums[index]]));
                for (var index = 0; index < 4; ++index)
                {
                    var c = new Computer("DebugShell" + index, NetworkMap.generateRandomIP(),
                        os.netMap.getRandomPosition(), 0, 2, os);
                    c.adminIP = os.thisComputer.adminIP;
                    os.netMap.nodes.Add(c);
                    os.netMap.discoverNode(c);
                }
                os.delayer.Post(ActionDelayer.Wait(0.2), () =>
                {
                    os.allFactions.setCurrentFaction("entropy", os);
                    os.currentMission = null;
                    os.netMap.discoverNode(Programs.getComputer(os, "entropy00"));
                    os.netMap.discoverNode(Programs.getComputer(os, "entropy01"));
                });
            }
            else if (name.Equals("EntropyFastFowardSetup"))
            {
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[22],
                    PortExploits.cracks[22]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[21],
                    PortExploits.cracks[21]));
                for (var index = 0; index < 3; ++index)
                {
                    var c = new Computer("DebugShell" + index, NetworkMap.generateRandomIP(),
                        os.netMap.getRandomPosition(), 0, 2, os);
                    c.adminIP = os.thisComputer.adminIP;
                    os.netMap.nodes.Add(c);
                    os.netMap.discoverNode(c);
                }
                os.delayer.Post(ActionDelayer.Wait(0.2), () =>
                {
                    os.allFactions.setCurrentFaction("entropy", os);
                    os.currentMission = null;
                    os.netMap.discoverNode(Programs.getComputer(os, "entropy00"));
                    os.netMap.discoverNode(Programs.getComputer(os, "entropy01"));
                    var computer = Programs.getComputer(os, "entropy01");
                    var userDetail = computer.users[0];
                    userDetail.known = true;
                    computer.users[0] = userDetail;
                    os.allFactions.factions[os.allFactions.currentFaction].playerValue = 2;
                    os.delayer.Post(ActionDelayer.Wait(0.2), () =>
                    {
                        os.Flags.AddFlag("eosPathStarted");
                        ComputerLoader.loadMission(
                            "Content/Missions/Entropy/StartingSet/eosMissions/eosIntroDelayer.xml");
                    });
                });
            }
            else if (name.Equals("CSECFastFowardSetup"))
            {
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[22],
                    PortExploits.cracks[22]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[21],
                    PortExploits.cracks[21]));
                for (var index = 0; index < 3; ++index)
                {
                    var c = new Computer("DebugShell" + index, NetworkMap.generateRandomIP(),
                        os.netMap.getRandomPosition(), 0, 2, os);
                    c.adminIP = os.thisComputer.adminIP;
                    os.netMap.nodes.Add(c);
                    os.netMap.discoverNode(c);
                }
                os.delayer.Post(ActionDelayer.Wait(0.2), () =>
                {
                    runCommand(0, "assignPlayerToHubServerFaction");
                    os.currentMission = null;
                    os.netMap.discoverNode(Programs.getComputer(os, "mainHub"));
                    os.netMap.discoverNode(Programs.getComputer(os, "mainHubAssets"));
                    var computer = Programs.getComputer(os, "mainHubAssets");
                    var userDetail = computer.users[0];
                    userDetail.known = true;
                    computer.users[0] = userDetail;
                });
            }
            else if (name.Equals("csecAddTraceKill"))
            {
                var folder = findComp("mainHubAssets").files.root.searchForFolder("bin");
                var f = folder.searchForFolder("TK");
                if (f == null)
                {
                    f = new Folder("TK");
                    folder.folders.Add(f);
                }
                f.files.Add(
                    new FileEntry(
                        FileEncrypter.EncryptString(PortExploits.crackExeData[12], "Vapor Trick Enc.", "NULL", "dx122DX",
                            ".exe"), Utils.GetNonRepeatingFilename("TraceKill", ".dec", f)));
                os.Flags.AddFlag("bitPathStarted");
                runCommand(10, "changeSong");
            }
            else if (name.Equals("junebugComplete"))
            {
                var computer = Programs.getComputer(os, "pacemaker01");
                if (computer != null)
                {
                    var heartMonitorDaemon = (HeartMonitorDaemon) computer.getDaemon(typeof (HeartMonitorDaemon));
                    if (heartMonitorDaemon != null)
                        heartMonitorDaemon.ForceStopBeepSustainSound();
                }
                runCommand(1, "addRank");
            }
            else if (name.Equals("eosIntroMissionSetup"))
            {
                findComp("entropy01")
                    .files.root.searchForFolder("bin")
                    .files.Add(new FileEntry(PortExploits.crackExeData[13], "eosDeviceScan.exe"));
                os.delayer.Post(ActionDelayer.Wait(8.0), () =>
                {
                    var mail = MailServer.generateEmail("Fwd: eOS Stuff",
                        Utils.readEntireFile("Content/Post/eosScannerMail.txt"), "vtfx", new List<string>(new string[1]
                        {
                            "note#%#eOS Security Basics#%#" +
                            ("1: Get admin access to a computer that you suspect has an eOS device sync'd to it\n" +
                             "2: Run eosdevicescanner.exe\nto scan for paired devices and automatically open connection ports\n" +
                             "3: connect to the revealed device\n" +
                             "3: login with\nuser: \"admin\"\npassword: \"alpine\"\n\n" +
                             "The password is the same for all eOS devices!")
                        }));
                    ((MailServer) os.netMap.mailServer.getDaemon(typeof (MailServer))).addMail(mail, os.defaultUser.name);
                });
                runCommand(4, "changeSong");
                os.saveGame();
            }
            else
            {
                if (!name.Equals("eosIntroEndFunc"))
                    return;
                runCommand(1, "addRank");
                var missionListingServer =
                    (MissionListingServer) findComp("entropy00").getDaemon(typeof (MissionListingServer));
                var list = os.branchMissions;
                var m =
                    (ActiveMission)
                        ComputerLoader.readMission(
                            "Content/Missions/Entropy/StartingSet/eosMissions/eosAddedMission.xml");
                missionListingServer.addMisison(m);
                os.branchMissions = list;
            }
        }

        private static Computer findComp(string target)
        {
            for (var index = 0; index < os.netMap.nodes.Count; ++index)
            {
                if (os.netMap.nodes[index].idName.Equals(target))
                    return os.netMap.nodes[index];
            }
            return null;
        }
    }
}