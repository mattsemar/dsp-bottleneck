using System;
using System.Collections.Generic;
using System.Linq;

namespace Bottleneck
{
    public class ProductionKey : IComparable<ProductionKey>
    {
        private readonly int _productId;
        private readonly int _planetId;
        private readonly int _producerId;
        private static readonly Dictionary<int, Dictionary<int, Dictionary<int, ProductionKey>>> Pool = new();

        private ProductionKey(int productId, int planetId, int producerId)
        {
            _productId = productId;
            _planetId = planetId;
            _producerId = producerId;
        }

        public static ProductionKey From(int productId, int planId, int producerId)
        {
            if (!Pool.ContainsKey(productId))
            {
                Pool[productId] = new Dictionary<int, Dictionary<int, ProductionKey>>();
            }

            if (!Pool[productId].ContainsKey(planId))
            {
                Pool[productId][planId] = new Dictionary<int, ProductionKey>();
            }

            if (Pool[productId][planId].ContainsKey(producerId))
            {
                return Pool[productId][planId][producerId];
            }

            Pool[productId][planId][producerId] = new ProductionKey(productId, planId, producerId);
            return Pool[productId][planId][producerId];
        }

        public int CompareTo(ProductionKey other) => String.Compare(ToString(), other.ToString(), StringComparison.Ordinal);
        public override string ToString() => $"{_productId}_{_planetId}_{_producerId}";

        public override int GetHashCode() => ToString().GetHashCode();
    }

    public class PlanetaryProduction : IComparable<PlanetaryProduction>
    {
        public string PlanetName;
        public int Producers;

        public int CompareTo(PlanetaryProduction other)
        {
            if (PlanetName == other.PlanetName)
            {
                return 0;
            }

            return other.Producers.CompareTo(Producers);
        }
    }

    public class PlanetaryConsumption : IComparable<PlanetaryConsumption>
    {
        public string PlanetName;
        public int Consumers;

        public int CompareTo(PlanetaryConsumption other)
        {
            if (PlanetName == other.PlanetName)
            {
                return 0;
            }

            return other.Consumers.CompareTo(Consumers);
        }
    }

    public class PlanetaryProductionSummary
    {
        private readonly List<PlanetaryProduction> _productions = new();
        private readonly List<PlanetaryConsumption> _consumers = new();
        private readonly HashSet<string> _productionPlanets = new();
        private readonly HashSet<string> _consumptionPlanets = new();
        private bool _prodSummaryTextDirty = true;
        private string _prodSummary = "";

        private bool _consumerSummaryTextDirty = true;
        private string _consumerSummary = "";

        public void AddProduction(string planet, int producerCount)
        {
            if (_productionPlanets.Contains(planet))
            {
                var planetaryProduction = _productions.Find(existingPlanet => existingPlanet.PlanetName == planet);
                planetaryProduction.Producers += producerCount;
            }
            else
            {
                _productionPlanets.Add(planet);
                _productions.Add(new PlanetaryProduction
                {
                    Producers = producerCount,
                    PlanetName = planet
                });
            }

            _productions.Sort();
            _prodSummaryTextDirty = true;
        }

        public void AddConsumption(string planet, int consumerCount)
        {
            if (_consumptionPlanets.Contains(planet))
            {
                var planetaryConsumption = _consumers.Find(existingPlanet => existingPlanet.PlanetName == planet);
                planetaryConsumption.Consumers += consumerCount;
            }
            else
            {
                _consumptionPlanets.Add(planet);
                _consumers.Add(new PlanetaryConsumption
                {
                    Consumers = consumerCount,
                    PlanetName = planet
                });
            }

            _consumers.Sort();
            _consumerSummaryTextDirty = true;
        }

        public string GetProducerSummary()
        {
            if (!_prodSummaryTextDirty)
            {
                return _prodSummary;
            }

            var includedElements = _productions.GetRange(0, Math.Min(PluginConfig.productionPlanetCount.Value, _productions.Count))
                .Select(prod => $"{prod.PlanetName}: producers={prod.Producers}");
            _prodSummary = string.Join("\n", includedElements);
            _prodSummaryTextDirty = false;
            return _prodSummary;
        }

        public string GetConsumerSummary()
        {
            if (!_consumerSummaryTextDirty)
            {
                return _consumerSummary;
            }

            var includedElements = _consumers.GetRange(0, Math.Min(PluginConfig.productionPlanetCount.Value, _consumers.Count))
                .Select(cons => $"{cons.PlanetName}: consumers={cons.Consumers}");
            _consumerSummary = string.Join("\n", includedElements);
            _consumerSummaryTextDirty = false;
            return _consumerSummary;
        }

        public int PlanetCount()
        {
            return _productionPlanets.Count;
        }

        public int ConsumerPlanetCount()
        {
            return _consumptionPlanets.Count;
        }
    }
}