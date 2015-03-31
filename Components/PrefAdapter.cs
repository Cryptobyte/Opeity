using NDatabase;
using Opeity.Components.Classes;

namespace Opeity.Components {
    /// <summary>
    /// Adapter for reading and writing user specific preferences
    /// </summary>
    public class PrefAdapter {
        /// <summary>
        /// User preferences class, returns default class if none exist
        /// </summary>
        public Preferences UserPreferences { get; set; }

        /// <summary>
        /// Loads user preferences class from database or creates a new one
        /// </summary>
        /// <returns>User Preferences from Database or new default Preferences class</returns>
        public Preferences Load() {
            using (var _odb = OdbFactory.Open(Profiler.UserPreferences)) {
                var _obj = _odb.QueryAndExecute<Preferences>();

                if (_obj.Count > 0)
                    return _obj.GetFirst();

                return new Preferences();
            }
        }

        /// <summary>
        /// Saves user preferences from public UserPreferences variable
        /// </summary>
        public void Save() {
            using (var _odb = OdbFactory.Open(Profiler.UserPreferences)) {
                var _obj = _odb.QueryAndExecute<Preferences>();

                if (_obj.Count > 0)
                    _obj.Clear();

                _odb.Store(UserPreferences);
            }
        }

        /// <summary>
        /// Class main entry point
        /// </summary>
        public PrefAdapter() {
            UserPreferences = Load();
        }
    }
}
