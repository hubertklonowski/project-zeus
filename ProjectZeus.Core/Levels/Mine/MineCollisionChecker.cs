using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectZeus.Core.Entities;
using ProjectZeus.Core.Extensions;

namespace ProjectZeus.Core.Levels.Mine
{
    /// <summary>
    /// Checks collisions in the mine level
    /// </summary>
    public class MineCollisionChecker
    {
        public bool CheckPlayerDeath(Rectangle playerRect, List<MineCart> carts, List<Stalactite> stalactites, 
            List<MineBat> bats, GigaBat gigaBat, List<Guano> guanos)
        {
            // Check collision with carts
            foreach (var cart in carts)
            {
                if (playerRect.IntersectsWith(cart.Bounds))
                {
                    return true;
                }
            }
            
            // Check collision with stalactites
            foreach (var stalactite in stalactites)
            {
                if (playerRect.IntersectsWith(stalactite.Bounds))
                {
                    return true;
                }
            }
            
            // Check collision with bats
            foreach (var bat in bats)
            {
                if (playerRect.IntersectsWith(bat.Bounds))
                {
                    return true;
                }
            }
            
            // Check collision with GigaBat
            if (gigaBat != null && playerRect.IntersectsWith(gigaBat.Bounds))
            {
                return true;
            }
            
            // Check collision with guano
            foreach (var guano in guanos)
            {
                if (playerRect.IntersectsWith(guano.Bounds))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
