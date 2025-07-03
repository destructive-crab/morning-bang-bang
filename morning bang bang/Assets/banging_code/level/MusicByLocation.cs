using banging_code.level.entity_locating;
using MothDIed.DI;
using MothDIed.Scenes.SceneModules;

namespace banging_code.level
{
    public class MusicByLocation : SceneModule
    {
        private LocationManager locationManager;

        public MusicByLocation(LocationManager locationManager)
        {
            this.locationManager = locationManager;
        }
    }
}