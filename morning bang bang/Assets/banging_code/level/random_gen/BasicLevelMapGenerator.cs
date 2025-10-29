using System;
using System.Collections.Generic;
using System.Linq;
using banging_code.common;
using banging_code.common.rooms;

namespace banging_code.level.random_gen
{
    public sealed class BasicLevelMapGenerator : LevelMapGenerator
    {
        private readonly BasicLevelConfig config;
        
        private int buisnessGenerated = 0;
        private int treasuresGenerated = 0;

        public BasicLevelMapGenerator(BasicLevelConfig config)
        {
            this.config = config;
        }

        public override BasicRoomTypes GetNext(BasicRoomTypes[] map, int current, int size)
        {
            List<BasicRoomTypes> candidates = new();

            for (int i = 1; i < 100; i++)
            {
                if (Enum.IsDefined(typeof(BasicRoomTypes), i))
                {
                    candidates.Add((BasicRoomTypes)i);
                }
                else
                {
                    candidates.Remove(candidates.Last());
                    break;
                }
            }
            
            if (current == size - 1)
            {
                return BasicRoomTypes.Final;
            }
            else
            {
                if (current <= size / 5)
                {
                    return BasicRoomTypes.Gang;
                }
                else
                {
                    if (buisnessGenerated >= config.BuisnessLimit)
                    {
                        candidates.Remove(BasicRoomTypes.Buisness);
                    }

                    if (treasuresGenerated >= config.TreasuresLimit)
                    {
                        candidates.Remove(BasicRoomTypes.Treasure);
                    }

                    var result = UTLS.RandomElement(candidates.ToArray());  

                    switch (result)
                    {
                        case BasicRoomTypes.Treasure:
                            treasuresGenerated++;
                            break;
                        case BasicRoomTypes.Buisness:
                            buisnessGenerated++;
                            break;
                    }

                    return result;
                }
            }
        }
    }
}