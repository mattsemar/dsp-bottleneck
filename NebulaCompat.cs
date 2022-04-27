using System;
using System.Collections.Generic;
using System.Reflection;
using Bottleneck.Util;
using Bottleneck.Stats;
using HarmonyLib;
using NebulaAPI;

namespace Bottleneck.Nebula
{
    public static class NebulaCompat
    {
        public static bool IsMultiplayerActive { get; private set; }
        public static bool IsClient { get; private set; }
        public static int LastAstroFilter { get; private set; }

        public static void Init(Harmony harmony)
        {
            try
            {
                if (!NebulaModAPI.NebulaIsInstalled)
                    return;

                NebulaModAPI.RegisterPackets(Assembly.GetExecutingAssembly());
                NebulaModAPI.OnMultiplayerGameStarted += OnMultiplayerGameStarted;
                NebulaModAPI.OnMultiplayerGameEnded += OnMultiplayerGameEnded;
                OnMultiplayerGameStarted();

                Log.Info("Nebula compatibility is ready.");
            }
            catch (Exception e)
            {
                Log.Warn("Nebula compatibility failed!");
                Log.Warn(e.ToString());
            }

        }

        public static void OnDestroy()
        {
            if (NebulaModAPI.NebulaIsInstalled)
            {
                NebulaModAPI.OnMultiplayerGameStarted -= OnMultiplayerGameStarted;
                NebulaModAPI.OnMultiplayerGameEnded -= OnMultiplayerGameEnded;
            }
        }

        public static void OnMultiplayerGameStarted()
        {
            IsMultiplayerActive = NebulaModAPI.IsMultiplayerActive;
            IsClient = NebulaModAPI.IsMultiplayerActive && NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient;
        }

        public static void OnMultiplayerGameEnded()
        {
            IsMultiplayerActive = false;
            IsClient = false;
        }

        public static void SendRequest(ERequest request)
        {
            int astroFilter = UIRoot.instance.uiGame.statWindow.astroFilter;
            NebulaModAPI.MultiplayerSession.Network.SendPacket(new Bottleneck_Request(request, astroFilter));
            Log.Debug($"{request} {astroFilter}");
            LastAstroFilter = astroFilter;
        }

        public static void SendEntryRequest(int productId, bool isPrecursor)
        {
            NebulaModAPI.MultiplayerSession.Network.SendPacket(new Bottleneck_EntryRequest(productId, isPrecursor));
            Log.Debug($"{productId} {isPrecursor}");
        }
    }

    public enum ERequest
    {
        BetterStats,
        Bottleneck,
        Open
    }

    internal class Bottleneck_Request
    {
        public ERequest Reqest { get; set; }
        public int AstroFilter { get; set; }
        public int[] ProductIds { get; set; }
        public short[] Modes { get; set; }

        public Bottleneck_Request() { }
        public Bottleneck_Request(ERequest request, int astroFilter)
        {
            Reqest = request;
            AstroFilter = astroFilter;
            ItemCalculationRuntimeSetting.OutputModes(out int[] productIds, out short[] modes);
            ProductIds = productIds;
            Modes = modes;
        }
    }

    internal class Bottleneck_Respone1
    {
        public int AstroFilter { get; set; }
        public int[] Ids { get; set; }
        public float[] Productions { get; set; }
        public float[] Consumptions { get; set; }
        public int[] Producers { get; set; }
        public int[] Consumers { get; set; }

        public Bottleneck_Respone1() { }
        public Bottleneck_Respone1(int astroFilter, in Dictionary<int, BetterStats.ProductMetrics> counters)
        {
            AstroFilter = astroFilter;
            int i = 0, num = counters.Count;
            Ids = new int[num];
            Productions = new float[num];
            Consumptions = new float[num];
            Producers = new int[num];
            Consumers = new int[num];
            foreach (var pair in counters)
            {
                Ids[i] = pair.Key;
                Productions[i] = pair.Value.production;
                Consumptions[i] = pair.Value.consumption;
                Producers[i] = pair.Value.producers;
                Consumers[i] = pair.Value.consumers;
                i++;
            }
        }
    }

    [RegisterPacketProcessor]
    internal class Bottleneck_RequestProcessor : BasePacketProcessor<Bottleneck_Request>
    {
        public override void ProcessPacket(Bottleneck_Request packet, INebulaConnection conn)
        {
            if (IsClient) return;

            ItemCalculationRuntimeSetting.OutputModes(out int[] productIds, out short[] modes);
            ItemCalculationRuntimeSetting.InputModes(packet.ProductIds, packet.Modes);
            var tmp = BetterStats.counter;
            BetterStats.counter = new();

            if (packet.Reqest == ERequest.BetterStats)
            {
                ComputeDisplayEntries(BetterStats.AddPlanetFactoryData, packet.AstroFilter);
                if (BetterStats.counter.Count > 0)
                    conn.SendPacket(new Bottleneck_Respone1(packet.AstroFilter, BetterStats.counter));
            }
            else if (packet.Reqest == ERequest.Bottleneck)
            {
                ComputeDisplayEntries((x) => BottleneckPlugin.Instance.AddPlanetFactoryData(x, false), packet.AstroFilter);
                if (BetterStats.counter.Count > 0)
                    conn.SendPacket(new Bottleneck_Respone1(packet.AstroFilter, BetterStats.counter));
            }
            else if (packet.Reqest == ERequest.Open)
            {
                BottleneckPlugin.Instance.ProcessMadeOnTask();
            }
            BetterStats.counter = tmp;
            ItemCalculationRuntimeSetting.InputModes(productIds, modes);
        }

        private static void ComputeDisplayEntries(Action<PlanetFactory> action, int astroFilter)
        {
            if (astroFilter == -1)
            {
                for (int i = 0; i < GameMain.data.factoryCount; i++)
                    action(GameMain.data.factories[i]);
            }
            else if (astroFilter % 100 > 0)
            {
                PlanetData planetData = GameMain.data.galaxy.PlanetById(astroFilter);
                action(planetData.factory);
            }
            else if (astroFilter % 100 == 0)
            {
                int starId = astroFilter / 100;
                StarData starData = GameMain.data.galaxy.StarById(starId);
                for (int j = 0; j < starData.planetCount; j++)
                    if (starData.planets[j].factory != null)
                        action(starData.planets[j].factory);
            }
        }

    }

    [RegisterPacketProcessor]
    internal class Bottleneck_Respone1Processor : BasePacketProcessor<Bottleneck_Respone1>
    {
        public override void ProcessPacket(Bottleneck_Respone1 packet, INebulaConnection conn)
        {
            // If client has changed astroFilter before data arrive, ignore the packet
            if (packet.AstroFilter != UIRoot.instance.uiGame.statWindow.astroFilter)
                return;

            BetterStats.counter.Clear();
            for (int i = 0; i < packet.Ids.Length; i++)
            {
                var value = new BetterStats.ProductMetrics();
                value.producers = packet.Producers[i];
                value.consumers = packet.Consumers[i];
                value.production = packet.Productions[i];
                value.consumption = packet.Consumptions[i];
                BetterStats.counter.Add(packet.Ids[i], value);
            }
        }
    }

    internal class Bottleneck_EntryRequest
    {
        public int ItemId { get; set; }
        public bool IsPrecursor { get; set; }

        public Bottleneck_EntryRequest() { }
        public Bottleneck_EntryRequest(int itmeId, bool isPrecursor)
        {
            ItemId = itmeId;
            IsPrecursor = isPrecursor;
        }
    }

    internal class Bottleneck_EntryRespone
    {
        public int ItemId { get; set; }
        public bool IsPrecursor { get; set; }
        public string TipTitle { get; set; }
        public string TipText { get; set; }

        public Bottleneck_EntryRespone() { }
        public Bottleneck_EntryRespone(int itemId, bool isPrecursor, string tipTitle, string tipText)
        {
            ItemId = itemId;
            IsPrecursor = isPrecursor;
            TipTitle = tipTitle;
            TipText = tipText;
        }
    }

    [RegisterPacketProcessor]
    internal class Bottleneck_EntryRequestProcessor : BasePacketProcessor<Bottleneck_EntryRequest>
    {
        public override void ProcessPacket(Bottleneck_EntryRequest packet, INebulaConnection conn)
        {
            string tipTtile, tipText;
            if (packet.IsPrecursor)
                BottleneckPlugin.Instance.GetPrecursorButtonTip(packet.ItemId, out tipTtile, out tipText);
            else
                BottleneckPlugin.Instance.GetSuccessorButtonTip(packet.ItemId, out tipTtile, out tipText);

            conn.SendPacket(new Bottleneck_EntryRespone(packet.ItemId, packet.IsPrecursor, tipTtile, tipText));
        }
    }

    [RegisterPacketProcessor]
    internal class Bottleneck_EntryResponeProcessor : BasePacketProcessor<Bottleneck_EntryRespone>
    {
        public override void ProcessPacket(Bottleneck_EntryRespone packet, INebulaConnection conn)
        {
            for (int i = 0; i < UIRoot.instance.uiGame.statWindow.entriesLen; i++)
            {
                var entry = UIRoot.instance.uiGame.statWindow.entries[i];
                if (entry?.entryData != null && entry.entryData.itemId == packet.ItemId)
                {
                    var elt = BottleneckPlugin.Instance.GetEnhanceElement(entry);
                    if (packet.IsPrecursor)
                    {
                        elt.precursorButton.tips.tipTitle = packet.TipTitle;
                        elt.precursorButton.tips.tipText = packet.TipText;
                    }
                    else
                    {
                        elt.successorButton.tips.tipTitle = packet.TipTitle;
                        elt.successorButton.tips.tipText = packet.TipText;
                    }
                }
            }
        }
    }

}
