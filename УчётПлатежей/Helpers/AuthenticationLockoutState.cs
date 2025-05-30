using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace УчётПлатежей.Helpers
{
    class AuthenticationLockoutState
    {
        public int FailedLoginAttempts { get; private set; } = 0;
        private DateTime _lockedOutUntil;

        private const string FILENAME = "authLockoutState.json";

        public bool IsLockedOut { get; private set; }
        public DateTime LockedOutUntil
        {
            get
            {
                if (IsLockedOut == false)
                    return DateTime.MinValue;

                return _lockedOutUntil;
            }
            private set => _lockedOutUntil = value;
        }

        public void IncrementFailedLoginAttempts() => FailedLoginAttempts++;

        public void LockOut(int lockoutSeconds)
        {
            IsLockedOut = true;
            _lockedOutUntil = DateTime.Now.AddSeconds(lockoutSeconds);
        }

        public void ResetLockout()
        {
            FailedLoginAttempts = 0;
            IsLockedOut = false;
            _lockedOutUntil = DateTime.MinValue;
        }

        public static void Save(AuthenticationLockoutState state)
        {
            var dto = new AuthenticationLockoutStateDTO()
            {
                FailedLoginAttempts = state.FailedLoginAttempts,
                IsLockedOut = state.IsLockedOut,
                LockedOutUntil = state.LockedOutUntil
            };
            
            string json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            string path = ApplicationDataManager.GetPathToSaveFile(FILENAME);

            File.WriteAllText(path, json);
        }

        public static AuthenticationLockoutState Load()
        {
            string path = ApplicationDataManager.GetPathToSaveFile(FILENAME);
            if (File.Exists(path) == false)
                return new AuthenticationLockoutState();

            string json = File.ReadAllText(ApplicationDataManager.GetPathToSaveFile(FILENAME));
            var dto = JsonConvert.DeserializeObject<AuthenticationLockoutStateDTO>(json);
            var state = new AuthenticationLockoutState
            {
                FailedLoginAttempts = dto.FailedLoginAttempts,
                IsLockedOut = dto.IsLockedOut,
                LockedOutUntil = dto.LockedOutUntil
            };

            return state;
        }

        public class AuthenticationLockoutStateDTO
        {
            public int FailedLoginAttempts { get; set; }
            public bool IsLockedOut { get; set; }
            public DateTime LockedOutUntil { get; set; }
        }
    }
}
