namespace Framework
{
	[System.Serializable]
	public class State<T> where T : class
	{
		public State()
		{

		}

		public State(int index)
		{
			mStateIndex = index;
		}

		private int mStateIndex;

		virtual public int index { get { return mStateIndex; } }
		protected FSM<T> fsm;
		protected T controller {  get { return fsm.Controller; } }

		public void SetIndex(int newIndex) { mStateIndex = newIndex; }
        public void SetController(FSM<T> newController) { fsm = newController; }
        public void SetState(int newState) { fsm.SetState(newState); }

		public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void FixedUpdate() { }
		public virtual void OnLeave() { }
		public virtual void OnEnter () { }
	}
}