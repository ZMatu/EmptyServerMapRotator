
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Plugin;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmptyServerMapRotator;

[MinimumApiVersion(60)]
public class EmptyServerMapRotator : BasePlugin
{
    public override string ModuleName => "EmptyServerMapRotator";
    public override string ModuleVersion => "1.0.4";
    public override string ModuleAuthor => "ZMatu";
    public override string ModuleDescription => "Cambia automáticamente de mapa cada X minutos si no hay jugadores.";

    private int currentMapIndex = 0;

   
    private int mapChangeIntervalMinutes = 5;
    private List<string> mapRotation = new() { "de_dust2", "de_inferno", "de_nuke", "de_mirage", "de_train", "de_overpass" };
    private string chatPrefix = "[MDQ]";
    
    public override void Load(bool hotReload)
    {
        ScheduleMapRotation();
        Server.PrintToConsole($"Plugin Cargado");
    }

    private void ScheduleMapRotation()
    {
        float totalTime = mapChangeIntervalMinutes * 60f;
        float interval = 60f;

        for (int i = 1; i <= mapChangeIntervalMinutes; i++)
        {
            AddTimer(i * interval, () =>
            {
                int remaining = mapChangeIntervalMinutes - i;
                if (remaining > 0)
                {
                    Server.PrintToConsole($"[DEBUG] Faltan {remaining} minutos para cambiar de mapa si no hay jugadores.");
                }
            });
        }

        AddTimer(totalTime, () =>
        {
            var players = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV).ToList();
            if (players.Count == 0)
            {
                currentMapIndex = (currentMapIndex + 1) % mapRotation.Count;
                var nextMap = mapRotation[currentMapIndex];
                string msg = $"{chatPrefix} No hay jugadores. Cambiando a {nextMap}...";
                Server.PrintToChatAll(msg);
                Server.PrintToConsole(msg);
                Server.ExecuteCommand($"changelevel {nextMap}");
            }
            else
            {
                Server.PrintToConsole($"[DEBUG] Hay jugadores. Reprogramando rotación de mapa.");
                ScheduleMapRotation();
            }
        });
    }
}
