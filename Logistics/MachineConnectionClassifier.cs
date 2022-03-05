using System.Collections.Generic;
using System.Linq;

namespace Bottleneck.Logistics
{
    public enum ConnectionType
    {
        DIRECT,
        PLANETARY,
    }

    public readonly struct MachineIO
    {
        public readonly List<int> Items;
        public readonly List<ConnectionType> ConnectionTypes;

        public MachineIO(IEnumerable<int> items, List<ConnectionType> connectionTypes)
        {
            Items = items.ToList();
            ConnectionTypes = connectionTypes;
        }
    }

    public abstract class MachineConnectionClassifier<T>
    {
        protected readonly PlanetFactory PlanetFactory;
        private readonly Dictionary<int, List<StationComponent>> _cargoPathToStations = new();
        public abstract MachineIO ClassifyInputs(T machine);
        public abstract MachineIO ClassifyOutputs(T machine);

        public abstract EntityData GetEntity(T machine);

        public PlanetTransport GetTransport()
        {
            return PlanetFactory.transport;
        }
        public CargoTraffic GetCargoTraffic()
        {
            return PlanetFactory.cargoTraffic;
        }

        private List<StationComponent> GetUpstreamStations(BeltComponent beltComponent)
        {
            var traffic = PlanetFactory.cargoTraffic;
            List<StationComponent> result = new List<StationComponent>();
            var cargoPath = traffic.GetCargoPath(traffic.beltPool[beltComponent.id].segPathId);           
            if (cargoPath == null)
            {
                return result;
            }
            
            if (_cargoPathToStations.ContainsKey(cargoPath.id))
                return _cargoPathToStations[cargoPath.id];

            for (int index = 0; index < cargoPath.inputPaths.Count; ++index)
            {
                CargoPath inputPath = traffic.GetCargoPath(cargoPath.inputPaths[index]);
                if (inputPath != null)
                {
                    
                }
            }

            return result;
        }
    }

    public class AssemblerConnectionClassifier : MachineConnectionClassifier<AssemblerComponent>
    {
        public override MachineIO ClassifyInputs(AssemblerComponent machine)
        {
            var entityData = GetEntity(machine);
            // PlanetFactory.factorySystem.inserterPool;
            return new MachineIO();
        }

        public override MachineIO ClassifyOutputs(AssemblerComponent machine) => throw new System.NotImplementedException();

        public override EntityData GetEntity(AssemblerComponent machine)
        {
            return PlanetFactory.entityPool[machine.entityId];
        }
    }
}