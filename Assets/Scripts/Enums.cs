namespace SpriteController
{

/* Sprite-related Enums */

	public enum Facing
	{
		Up,
		UpLeft,
		Left,
		DownLeft,
		Down,
		DownRight,
		Right,
		UpRight
	}

/* Char Movement-related Enums */

	public enum JumpState
	{
		None,
		StandingJump,
		WallJump,
		NormalJump
	}

	public enum SlideState
	{
		None,
		Slide,
		WallSlide
	}

	public enum MoveState
	{
		Walk,
		Run,
		Locked
	}
}