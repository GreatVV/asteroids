﻿using Asteroids.Components;
using DCFApixels.DragonECS;
using UnityEngine;

namespace Asteroids.Systems
{
    internal class ImmunitySystem : IEcsRun
    {
        [DI] private EcsWorld _world;

        class Aspect : EcsAspect
        {
            public EcsPool<Starship> Starships = Inc;
            public EcsPool<Immunity> Immunities = Inc;
        }
        public void Run()
        {
            foreach (var e in _world.Where(out Aspect a))
            {
                ref var immunity = ref a.Immunities.Get(e);
                immunity.TimeLeft -= Time.deltaTime;
                var starshipView = a.Starships.Get(e).View;
                
                if (immunity.TimeLeft <= 0)
                {
                    a.Immunities.Del(e);
                    starshipView.BlinkFromValue(0);
                }
                else
                {
                    starshipView.BlinkFromValue(immunity.TimeLeft);
                }
            }
        }
    }
}