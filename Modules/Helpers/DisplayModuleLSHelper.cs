// Decompiled with JetBrains decompiler
// Type: Hacknet.Modules.Helpers.DisplayModuleLSHelper
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Hacknet.Gui;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Modules.Helpers
{
    internal class DisplayModuleLSHelper
    {
        private const int BUTTON_HEIGHT = 20;
        private const int BUTTON_MARGIN = 2;
        private const int INDENT = 30;
        private ScrollableSectionedPanel panel;

        public DisplayModuleLSHelper()
        {
            panel = new ScrollableSectionedPanel(24, GuiData.spriteBatch.GraphicsDevice);
        }

        public void DrawUI(Rectangle dest, OS os)
        {
            var ButtonHeight = (int) (GuiData.ActiveFontConfig.tinyFontCharHeight + 10.0);
            if (panel.PanelHeight != ButtonHeight + 4)
                panel = new ScrollableSectionedPanel(ButtonHeight + 4, GuiData.spriteBatch.GraphicsDevice);
            var items =
                BuildDirectoryDrawList(
                    os.connectedComp == null ? os.thisComputer.files.root : os.connectedComp.files.root, 0, 0, os);
            panel.NumberOfPanels = items.Count;
            var width = dest.Width - 25;
            Action<int, Rectangle, SpriteBatch> DrawSection = (index, bounds, sb) =>
            {
                var lsItem = items[index];
                if (lsItem.IsEmtyDisplay)
                {
                    TextItem.doFontLabel(new Vector2(bounds.X + 5 + lsItem.indent, bounds.Y + 2), "-Empty-",
                        GuiData.tinyfont, new Color?(), width, ButtonHeight);
                }
                else
                {
                    if (
                        !Button.doButton(300000 + index, bounds.X + 5 + lsItem.indent, bounds.Y + 2,
                            width - lsItem.indent, ButtonHeight, lsItem.DisplayName, new Color?()))
                        return;
                    lsItem.Clicked();
                }
            };
            Button.DisableIfAnotherIsActive = true;
            panel.Draw(DrawSection, GuiData.spriteBatch, dest);
            Button.DisableIfAnotherIsActive = false;
        }

        private List<LSItem> BuildDirectoryDrawList(Folder f, int recItteration, int indentOffset, OS os)
        {
            var list = new List<LSItem>();
            var commandSeperationDelay = 0.019;
            for (var index1 = 0; index1 < f.folders.Count; ++index1)
            {
                var myIndex = index1;
                var lsItem = new LSItem
                {
                    DisplayName = "/" + f.folders[index1].name,
                    Clicked = () =>
                    {
                        var num = 0;
                        for (var index = 0; index < os.navigationPath.Count - recItteration; ++index)
                        {
                            Action action = () => os.runCommand("cd ..");
                            if (num > 0)
                                os.delayer.Post(ActionDelayer.Wait(num*commandSeperationDelay), action);
                            else
                                action();
                            ++num;
                        }
                        Action action1 = () => os.runCommand("cd " + f.folders[myIndex].name);
                        if (num > 0)
                            os.delayer.Post(ActionDelayer.Wait(num*commandSeperationDelay), action1);
                        else
                            action1();
                    },
                    indent = indentOffset
                };
                list.Add(lsItem);
                indentOffset += 30;
                if (os.navigationPath.Count - 1 >= recItteration && os.navigationPath[recItteration] == index1)
                    list.AddRange(BuildDirectoryDrawList(f.folders[index1], recItteration + 1, indentOffset, os));
                indentOffset -= 30;
            }
            for (var index1 = 0; index1 < f.files.Count; ++index1)
            {
                var myIndex = index1;
                var lsItem = new LSItem
                {
                    DisplayName = f.files[index1].name,
                    Clicked = () =>
                    {
                        var num = 0;
                        for (var index = 0; index < os.navigationPath.Count - recItteration; ++index)
                        {
                            Action action = () => os.runCommand("cd ..");
                            if (num > 0)
                                os.delayer.Post(ActionDelayer.Wait(num*commandSeperationDelay), action);
                            else
                                action();
                            ++num;
                        }
                        Action action1 = () => os.runCommand("cat " + f.files[myIndex].name);
                        if (num > 0)
                            os.delayer.Post(ActionDelayer.Wait(num*commandSeperationDelay), action1);
                        else
                            action1();
                    },
                    indent = indentOffset
                };
                list.Add(lsItem);
            }
            if (f.folders.Count == 0 && f.files.Count == 0)
            {
                var lsItem = new LSItem
                {
                    IsEmtyDisplay = true,
                    indent = indentOffset
                };
                list.Add(lsItem);
            }
            return list;
        }

        private struct LSItem
        {
            public Action Clicked;
            public int indent;
            public string DisplayName;
            public bool IsEmtyDisplay;
        }
    }
}