using Bottleneck.UI;
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
        private readonly Dictionary<string, PlanetaryConsumption> _consumptionPlanetToConsumer = new(); 
        private readonly Dictionary<string, PlanetaryProduction> _productionPlanetToProducer = new(); 
        private bool _prodSummaryTextDirty = true;
        private string _prodSummary = "";

        private bool _consumerSummaryTextDirty = true;
        private string _consumerSummary = "";

        public void AddProduction(string planet, int producerCount)
        {
            if (_productionPlanetToProducer.ContainsKey(planet))
            {
                var planetaryProduction = _productionPlanetToProducer[planet];
                planetaryProduction.Producers += producerCount;
            }
            else
            {
                var planetaryProduction = new PlanetaryProduction
                {
                    Producers = producerCount,
                    PlanetName = planet
                };
                _productionPlanetToProducer.Add(planet, planetaryProduction); 
                _productions.Add(planetaryProduction);
            }

            _productions.Sort();
            _prodSummaryTextDirty = true;
        }

        public void AddConsumption(string planet, int consumerCount)
        {
            if (_consumptionPlanetToConsumer.ContainsKey(planet))
            {
                var planetaryConsumption = _consumptionPlanetToConsumer[planet];
                planetaryConsumption.Consumers += consumerCount;
            }
            else
            {
                var planetaryConsumption = new PlanetaryConsumption
                {
                    Consumers = consumerCount,
                    PlanetName = planet
                };
                _consumptionPlanetToConsumer[planet] = planetaryConsumption;
                _consumers.Add(planetaryConsumption);
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

            var producersLabel = Strings.ProducersLabel;
            var includedElements = _productions.GetRange(0, Math.Min(PluginConfig.productionPlanetCount.Value, _productions.Count))
                .Select(prod => $"{prod.PlanetName}: {producersLabel}={prod.Producers}");
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
            var consLabel = Strings.ConsumersLabel;
            var includedElements = _consumers.GetRange(0, Math.Min(PluginConfig.productionPlanetCount.Value, _consumers.Count))
                .Select(cons => $"{cons.PlanetName}: {consLabel}={cons.Consumers}");
            _consumerSummary = string.Join("\n", includedElements);
            _consumerSummaryTextDirty = false;
            return _consumerSummary;
        }

        public int PlanetCount()
        {
            return _productionPlanetToProducer.Count;
        }

        public int ConsumerPlanetCount()
        {
            return _consumptionPlanetToConsumer.Count;
        }

        public bool IsConsumerPlanet(string planet)
        {
            return _consumptionPlanetToConsumer.ContainsKey(planet);
        }
        public bool IsProducerPlanet(string planet)
        {
            return _productionPlanetToProducer.ContainsKey(planet);
        }
    }
}