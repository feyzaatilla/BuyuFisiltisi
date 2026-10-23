using System;
using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Interaction;

namespace BuyuFisiltisi.Companion.Brain
{
    public sealed class CompanionBrain
    {
        private readonly ICompanionBody body;
        private readonly float taskTimeout;

        private CompanionCommand activeTask;
        private float taskTime;

        public CompanionBrain(ICompanionBody body, float taskTimeout)
        {
            this.body = body ?? throw new ArgumentNullException(nameof(body));
            this.taskTimeout = taskTimeout;
        }

        public event Action<CompanionStateId, CompanionStateId> StateChanged;
        public event Action<string, CompanionTaskOutcome, float> TaskFinished;

        public CompanionStateId State { get; private set; } = CompanionStateId.Waiting;
        public bool HasTask => activeTask != null;

        public void Issue(CompanionCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (activeTask != null)
                FinishTask(CompanionTaskOutcome.Cancelled);

            switch (command.Type)
            {
                case CompanionCommandType.FollowPlayer:
                    body.FollowPlayer();
                    ChangeState(CompanionStateId.Following);
                    break;

                case CompanionCommandType.Wait:
                    body.Stop();
                    ChangeState(CompanionStateId.Waiting);
                    break;

                case CompanionCommandType.InteractWithTarget:
                case CompanionCommandType.FetchItem:
                    StartTask(command);
                    break;
            }
        }

        public void Tick(float deltaTime)
        {
            if (State != CompanionStateId.GoingToTarget || activeTask == null)
                return;

            taskTime += deltaTime;

            if (body.HasArrived)
            {
                CompleteTask();
                return;
            }

            if (body.IsStuck)
            {
                FailTask(CompanionTaskOutcome.Failed);
                return;
            }

            if (taskTime >= taskTimeout)
                FailTask(CompanionTaskOutcome.TimedOut);
        }

        private void StartTask(CompanionCommand command)
        {
            activeTask = command;
            taskTime = 0f;

            if (!command.Target.CanInteract(body.Self) || !body.MoveTo(command.Target.InteractionPoint.position))
            {
                FailTask(CompanionTaskOutcome.Failed);
                return;
            }

            ChangeState(CompanionStateId.GoingToTarget);
        }

        private void CompleteTask()
        {
            var target = activeTask.Target;
            var fetch = activeTask.Type == CompanionCommandType.FetchItem;

            body.Stop();
            ChangeState(CompanionStateId.Interacting);

            if (!target.CanInteract(body.Self))
            {
                FailTask(CompanionTaskOutcome.Failed);
                return;
            }

            target.Interact(body.Self);
            FinishTask(CompanionTaskOutcome.Succeeded);

            if (fetch)
            {
                body.FollowPlayer();
                ChangeState(CompanionStateId.Following);
            }
            else
            {
                ChangeState(CompanionStateId.Waiting);
            }
        }

        private void FailTask(CompanionTaskOutcome outcome)
        {
            body.Stop();
            FinishTask(outcome);
            ChangeState(CompanionStateId.Waiting);
        }

        private void FinishTask(CompanionTaskOutcome outcome)
        {
            var task = activeTask;
            activeTask = null;
            TaskFinished?.Invoke(TaskName(task), outcome, taskTime);
        }

        private void ChangeState(CompanionStateId next)
        {
            if (next == State)
                return;

            var previous = State;
            State = next;
            StateChanged?.Invoke(previous, next);
        }

        private static string TaskName(CompanionCommand command)
        {
            IInteractable target = command.Target;
            return target == null ? command.Type.ToString() : $"{command.Type}:{target.InteractionId}";
        }
    }
}
