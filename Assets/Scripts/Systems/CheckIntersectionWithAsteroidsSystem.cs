﻿using System.Collections.Generic;
using Asteroids.Components;
using Asteroids.Data;
using Asteroids.Utils;
using DCFApixels.DragonECS;

namespace Asteroids.Systems
{
    internal class CheckIntersectionWithAsteroidsSystem : IEcsRun
    {
        [DI] private EcsDefaultWorld _world;
        [DI] private RuntimeData _runtimeData;
        [DI] private StaticData _staticData;
        private readonly List<AreaHash2D<entlong>.Hit> _hits = new(64);
        private EcsPool<HitEvent> _hitEvents;

        private class Aspect : EcsAspect
        {
            public readonly EcsPool<RequestIntersectionEvent> RequestIntersectionEvents = Inc;
            public readonly EcsPool<MoveInfo> MoveInfos = Inc;
        }
    
        public void Run()
        {
            _hitEvents ??= _world.GetPool<HitEvent>();
            
            foreach (var e in _world.Where(out Aspect a))
            {
                ref var wantIntersection = ref a.RequestIntersectionEvents.Get(e);
                var position = a.MoveInfos.Get(e).Position;
                _runtimeData.AreaHash.FindAllInRadius(position.x, position.z, wantIntersection.CheckRadius, _hits);

                foreach (var hit in _hits)
                {
                    var hitEntity = hit.Id;
                    if (!hitEntity.TryUnpack(out var entity, out EcsWorld ecsWorld))
                    {
                        continue;
                    }
                    var currentAsteroidRadius = _world.GetPool<Asteroid>().Get(entity).Radius + wantIntersection.ObjectRadius;

                    if (hit.SqrDistance <= currentAsteroidRadius * currentAsteroidRadius)
                    {
                        _hitEvents.TryAddOrGet(entity).ByObject = _world.GetEntityLong(e);
                        if (!ecsWorld.GetPool<Immunity>().Has(e))
                        {
                            _hitEvents.TryAddOrGet(e).ByObject = hitEntity;
                        }
                        break;
                    }
                }
            }
        }
    }
}