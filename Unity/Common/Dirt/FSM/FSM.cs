using Dirt.Log;
using System.Collections.Generic;

namespace Framework
{
    public class FSM<T> where T: class
	{
		public Dictionary<int, State<T> > mStates;
		public State<T> currentState {  get { return mCurrentState; } }
		private State<T> mCurrentState;
		private State<T> mNextState;
        private T mController;

        public T Controller {  get { return mController; } }

        public int stateIndex { get { return mCurrentState.index; } }

        public static FSM<T> Create(T controller) { return new FSM<T>(controller); }

		public FSM(T controller)
		{
            Console.Assert(controller != null, $"missing {typeof(T).Name} controller");
            mController = controller;
            mStates = new Dictionary<int, State<T> >();
			mCurrentState = null;
			mNextState = null;
        }


		public bool IsState(int state) { return mCurrentState.index == state; }

        public void AddState<StateClass>(int stateIndex) where StateClass : State<T>, new()
		{
			if (!mStates.ContainsKey (stateIndex))
			{
                StateClass state = new StateClass();
                state.SetController(this);
				state.SetIndex(stateIndex);
				mStates.Add (stateIndex, state);
			}
			else
			{
				Console.Warning($"State already existing at index {stateIndex} ({mStates[stateIndex].GetType().Name})");
			}
		}

        public  StateClass GetState<StateClass>(int stateIndex) where StateClass : State<T>
        {
            if ( mStates.ContainsKey(stateIndex) )
            {
                return mStates[stateIndex] as StateClass;
            }
            else
            {
                return null;
            }
        }

		public void Start(int entryState = 0)
		{
            if (mCurrentState == null)
			{
                SetState (entryState);
                ProcessChange();
                //ProcessChange();
			}
		}

        public void Stop()
        {
            if ( mCurrentState != null )
            {
                mCurrentState.OnLeave();
                mCurrentState = null;
            }
        }

		public void SetState(int state, bool overrideChange = false)
		{
			Console.Assert (mStates.ContainsKey (state), $"State not found {state}" );
			
            if ( mNextState != null && !overrideChange)
            {
                Console.Warning($"FSM already changing to new state {mNextState.GetType().Name}");
            }
            else
            { 
			    mNextState = mStates [state];
            }
		}

		public void Update()
		{
			if (mNextState != null)
            {
                ProcessChange();
			}
			else if (mCurrentState != null)
			{
				mCurrentState.Update ();
			}
		}

        public void LateUpdate()
        {
			if (mCurrentState != null)
            {
                mCurrentState.LateUpdate();
            }
        }

        public void FixedUpdate()
        {
            if ( mCurrentState != null )
            {
                mCurrentState.FixedUpdate();
            }
        }

        private void ProcessChange()
        {
            if (mCurrentState != null)
            {
                mCurrentState.OnLeave();
            }

            mCurrentState = mNextState;
            mNextState = null;

            mCurrentState.OnEnter();
        }
	}
}