using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace AllaganSync
{
    internal class Main : IDalamudPlugin
    {
        public string Name => "AllaganSync";

        private readonly DalamudPluginInterface _pluginInterface;
        private readonly ClientState _clientState;
        private readonly WindowSystem _windowSystem;
        private readonly CommandManager _command;

        public Main(DalamudPluginInterface pluginInterface, ClientState clientState, CommandManager command)
        {
            _pluginInterface = pluginInterface;
            _clientState = clientState;
            _command = command;
            clientState.Login += ClientStateOnLogin;
            clientState.Logout += ClientStateOnLogout;
            command.AddHandler("/allagansync", new CommandInfo(OnCommand)
            {
                HelpMessage = "Open the AllaganSync window."
            });
            if (clientState.IsLoggedIn)
                ClientStateOnLogin(null, null!);
            _windowSystem = LoadWindows();
        }

        private void OnCommand(string command, string arguments)
        {
            foreach (var friendListEntry in FriendList.List)
            {
                var contentId = friendListEntry.ContentId;
                var homeWorld = friendListEntry.HomeWorld;
                var name = friendListEntry.Name;
                PluginLog.Debug($"Friend: {contentId:X}@{homeWorld}:{name}");
            }
        }

        private void ClientStateOnLogout(object? sender, EventArgs e)
        {
            
        }

        private void ClientStateOnLogin(object? sender, EventArgs e)
        {
            PluginLog.Debug("Login");
            var user = _clientState.LocalPlayer;
            PluginLog.Debug($"User: {user.Name}");
            PluginLog.Debug($"LocalContentId: {_clientState.LocalContentId:X}");
            PluginLog.Debug($"HomeWorld: {user.HomeWorld.Id}");
            PluginLog.Debug($"Key: {_clientState.LocalContentId:X}@{user.HomeWorld.Id}:{user.Name}");
            foreach (var friendListEntry in FriendList.List)
            {
                var contentId = friendListEntry.ContentId;
                var homeWorld = friendListEntry.HomeWorld;
                var name = friendListEntry.Name;
                PluginLog.Debug($"Friend: {contentId:X}@{homeWorld}:{name}");
            }
        }

        public WindowSystem LoadWindows()
        {
            var sys = new WindowSystem(Name);
            return sys;
        }
        
        public void Dispose()
        {
            _clientState.Login -= ClientStateOnLogin;
            _clientState.Logout -= ClientStateOnLogout;
            _windowSystem.RemoveAllWindows();
        }

    }
}
