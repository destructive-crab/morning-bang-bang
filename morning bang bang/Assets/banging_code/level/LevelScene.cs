using banging_code.player_logic;
using MothDIed.Scenes;

namespace banging_code.level
{
    public abstract class LevelScene : Scene
    {
        public PlayerRoot PlayerInstance { get; protected set; }
    }
}