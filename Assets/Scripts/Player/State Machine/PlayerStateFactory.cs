public class PlayerStateFactory
{
   PlayerStateMachine _context;

   public PlayerStateFactory(PlayerStateMachine currentContext)
   {
      _context = currentContext;
   }

   #region Town-Based Player States

   public PlayerBaseState Grounded()
   {
      return new PlayerGroundedState(_context, this); 
   }

   #region Grounded Substates
   public PlayerBaseState Idle()
   {
      return new PlayerIdleState(_context, this);
   }

   public PlayerBaseState Walk()
   {
      return new PlayerWalkState(_context, this); 
   }

   public PlayerBaseState Run()
   {
      return new PlayerRunState(_context, this); 
   }

   public  PlayerBaseState Slide()
   {
      return new PlayerSlideState(_context, this); 
   }

   public  PlayerBaseState Interact()
   {
      return new PlayerInteractState(_context, this); 
   }

   public  PlayerBaseState Landing()
   {
      return new PlayerLandingState(_context, this); 
   }

   public  PlayerBaseState HardLanding()
   {
      return new PlayerHardLandingState(_context, this); 
   }
   #endregion
   
   public PlayerBaseState Airborne()
   {
      return new PlayerAirborneState(_context, this); 
   }
   #region Airborne Substates
   public PlayerBaseState Jump()
   {
      return new PlayerJumpState(_context, this); 
   }
   public PlayerBaseState WallJump()
   {
      return new PlayerWallJumpState(_context, this); 
   }
   public PlayerBaseState Fall()
   {
      return new PlayerFallState(_context, this); 
   }
   #endregion

   public PlayerBaseState Wall()
   {
      return new PlayerWallState(_context, this); 
   }
   #region Wall Substates
   public PlayerBaseState Ladder()
   {
      return new PlayerLadderState(_context, this); 
   }
   public PlayerBaseState WallSlide()
   {
      return new PlayerWallSlideState(_context, this); 
   }
   public PlayerBaseState Hang()
   {
      return new PlayerHangState(_context, this); 
   }
   public PlayerBaseState Mantle()
   {
      return new PlayerMantleState(_context, this); 
   }
   #endregion

   #endregion
}