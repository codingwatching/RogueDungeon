﻿using UnityEngine;

namespace Enemy.State
{
    public class Dying : EnemyState
    {
        private bool _hasStartedDeathAnim;
        private const string DeathAnimName = "Death";

        public Dying(EnemyStateCtx ctx) : base(ctx)
        {
            Name = STATE.DYING;
        }
        
        public override void Enter()
        {
            Ctx.Animator.SetTrigger(DeathAnimName);
            Ctx.MeshAgent.enabled = false;
            Ctx.Animator.applyRootMotion = true;
            base.Enter();
        }
        public override void Update()
        {
            AnimatorStateInfo currentAnimatorStateInfo = Ctx.Animator.GetCurrentAnimatorStateInfo(0);
            if (currentAnimatorStateInfo.IsName(DeathAnimName))
            {
                _hasStartedDeathAnim = true;
            } else if (_hasStartedDeathAnim && !currentAnimatorStateInfo.IsName(DeathAnimName))
            {
                Ctx.Despawner.Despawn();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}