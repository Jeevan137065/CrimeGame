using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrimeGame.Class_Code
{
    public class ActorManager //NOT IMPLEMENTED
    {
        private List<Actor> actors;
        public ActorManager()
        {
            actors = new List<Actor>();
        }
        public void AddActor(Actor actor)
        {
            actors.Add(actor);
            //create player or npc or Snpc and add to the list.
        }

        public void Update(GameTime gameTime)
        {
            foreach (var actor in actors)
            {
                actor.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var actor in actors)
            {
                actor.Draw();
            }
        }

        

    }
}
