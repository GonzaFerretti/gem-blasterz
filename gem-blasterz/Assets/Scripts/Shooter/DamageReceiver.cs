using System;

namespace Shooter
{
    [Serializable]
    public enum Team
    {
        Team1,
        Team2
    }
    
    public interface IDamageReceiver
    {
        public bool CanDamage(Team team);
        public void ReceiveDamage(float damage);
    }
}