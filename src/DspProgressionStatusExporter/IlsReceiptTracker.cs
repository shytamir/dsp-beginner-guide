using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal sealed class IlsReceiptEvidence
    {
        public string Status = "unavailable";
        public string ResetReason = "not-observed";
        public bool TitaniumReceived;
        public bool SiliconReceived;
        public long SampleTick;
        public long Epoch;
        public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object> {
                { "status", Status }, { "resetReason", ResetReason },
                { "titaniumReceived", TitaniumReceived }, { "siliconReceived", SiliconReceived },
                { "sampleGameTick", SampleTick }, { "evidenceEpoch", Epoch },
                { "scope", "Session-local birth-planet input after observed endpoint configuration; no exact sending-station attribution." }
            };
        }
    }

    internal sealed class IlsReceiptTracker
    {
        private object gameData;
        private IlsTransportEvidence previous;
        private string identity;
        private long epoch, tick, titanium, silicon;
        private bool receivedTitanium, receivedSilicon;

        public void ObserveSelected(object data, ObservedGameState state, string phaseId, int stage)
        {
            if (phaseId == "ils" && stage == 3)
                Observe(data, state);
            else
            {
                previous = null;
                identity = null;
                receivedTitanium = receivedSilicon = false;
            }
        }

        public void Observe(object data, ObservedGameState state)
        {
            bool newGame = !Object.ReferenceEquals(gameData, data);
            if (newGame) previous = null;
            gameData = data;
            IlsTransportEvidence pair = IlsTransportEvidence.Resolve(state, previous);
            previous = pair;
            state.IlsTransport = pair;
            var result = new IlsReceiptEvidence { SampleTick = state.TrafficSampleTick, Epoch = state.TrafficEvidenceEpoch };
            state.IlsReceipt = result;
            Dictionary<int, long> inputs;
            bool sourceKnown;
            bool sourceReady = SourceReady(state, pair.Source, out sourceKnown);
            bool configured = pair.DeploymentReady;
            bool available = sourceKnown && state.FinishedInputTotals.TryGetValue(state.StarterPlanetId, out inputs);
            // Establish the baseline only after configuration is observed. Rolling rates
            // can contain arrivals from before that point and cannot prove this receipt.
            string current = configured ? state.StarterPlanetId + ":" + pair.Home.StationId + ":" + pair.Source.PlanetId + ":" + pair.Source.StationId : null;
            inputs = null;
            state.FinishedInputTotals.TryGetValue(state.StarterPlanetId, out inputs);
            long nextTitanium = inputs != null && inputs.ContainsKey(1106) ? inputs[1106] : 0;
            long nextSilicon = inputs != null && inputs.ContainsKey(1105) ? inputs[1105] : 0;
            available = available && inputs.ContainsKey(1106) && inputs.ContainsKey(1105);
            string reset = newGame ? "game-data-replaced" : current != identity ? "configuration-changed"
                : state.TrafficEvidenceEpoch != epoch || state.TrafficSampleTick < tick || nextTitanium < titanium || nextSilicon < silicon ? "counter-reset"
                : !available ? "unavailable" : null;
            if (!configured || reset != null) receivedTitanium = receivedSilicon = false;
            if (configured && available && reset == null && state.TrafficSampleTick > tick)
            {
                receivedTitanium |= nextTitanium > titanium;
                receivedSilicon |= nextSilicon > silicon;
            }
            identity = configured && available ? current : null;
            epoch = state.TrafficEvidenceEpoch;
            tick = state.TrafficSampleTick;
            titanium = nextTitanium;
            silicon = nextSilicon;
            result.ResetReason = reset;
            result.TitaniumReceived = receivedTitanium;
            result.SiliconReceived = receivedSilicon;
            result.Status = !configured ? (pair.Available ? "configuration" : "unavailable") : !available ? "unavailable" : !sourceReady ? "source-empty"
                : receivedTitanium && receivedSilicon ? "confirmed" : "awaiting-receipt";
        }

        private static bool SourceReady(ObservedGameState state, ObservedStationState source, out bool known)
        {
            known = source != null && source.EvidenceAvailable;
            if (!known) return false;
            bool ready = true;
            foreach (int id in new[] { 1106, 1105 })
            {
                bool stock = false, production = false, productionKnown = false;
                foreach (ObservedStationSlot slot in source.Slots)
                    if (slot.ItemId == id && slot.Count > 0) stock = true;
                foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
                    if (flow.PlanetId == source.PlanetId && flow.ItemId == id && flow.OneMinuteAvailable)
                    { productionKnown = true; production |= flow.ProducedPerMinute > 0; }
                known &= stock || productionKnown;
                ready &= stock || production;
            }
            return ready;
        }
    }
}

