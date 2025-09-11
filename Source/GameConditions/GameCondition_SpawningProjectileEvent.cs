using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public abstract class GameCondition_SpawningProjectileEvent : GameCondition
    {
        protected int nextSpawnTick = 0;

        private const float BuildupPhaseRatio = 0.2f;
        private const float PeakPhaseRatio = 0.6f;

        public float EventProgress => (float)TicksPassed / Duration;

        public EventPhase CurrentPhase
        {
            get
            {
                if (EventProgress <= BuildupPhaseRatio) return EventPhase.Buildup;
                if (EventProgress <= BuildupPhaseRatio + PeakPhaseRatio) return EventPhase.Peak;
                return EventPhase.FadeOut;
            }
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            if (Find.TickManager.TicksGame >= nextSpawnTick)
            {
                SpawnProjectile();
                nextSpawnTick = Find.TickManager.TicksGame + GetNextSpawnInterval();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextSpawnTick, "nextSpawnTick", 0);
        }

        protected abstract int GetNextSpawnInterval();
        protected abstract void SpawnProjectile();

        public enum EventPhase
        {
            Buildup,
            Peak,
            FadeOut
        }
    }
}