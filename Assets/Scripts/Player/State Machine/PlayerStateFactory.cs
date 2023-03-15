using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFactory
{
   PlayerStateMachine _context;

   public Dictionary<string, PlayerBaseState> _playerStates = new Dictionary<string, PlayerBaseState>();

   public PlayerStateFactory(PlayerStateMachine currentContext)
   {
      _context = currentContext;

      _playerStates["grounded"] = new PlayerGroundedState(_context, this);
      _playerStates["idle"] = new PlayerIdleState(_context, this, _playerStates["grounded"]);
      _playerStates["walk"] = new PlayerWalkState(_context, this, _playerStates["grounded"]);
      _playerStates["run"] = new PlayerRunState(_context, this, _playerStates["grounded"]);
      _playerStates["slide"] = new PlayerSlideState(_context, this, _playerStates["grounded"]);
      _playerStates["interact"] = new PlayerInteractState(_context, this, _playerStates["grounded"]);
      _playerStates["landing"] = new PlayerLandingState(_context, this, _playerStates["grounded"]);
      _playerStates["hardlanding"] = new PlayerHardLandingState(_context, this, _playerStates["grounded"]);
      _playerStates["airborne"] = new PlayerAirborneState(_context, this);
      _playerStates["jump"] = new PlayerJumpState(_context, this, _playerStates["airborne"]);
      _playerStates["walljump"] = new PlayerWallJumpState(_context, this, _playerStates["airborne"]);
      _playerStates["fall"] = new PlayerFallState(_context, this, _playerStates["airborne"]);
      _playerStates["wall"] = new PlayerWallState(_context, this);
      _playerStates["ladder"] = new PlayerLadderState(_context, this, _playerStates["wall"]);
      _playerStates["wallslide"] = new PlayerWallSlideState(_context, this, _playerStates["wall"]);
      _playerStates["hang"] = new PlayerHangState(_context, this, _playerStates["wall"]);
      _playerStates["mantle"] = new PlayerMantleState(_context, this, _playerStates["wall"]);
   }

   #region Town-Based Player States

   public PlayerBaseState Grounded()
   {
      return _playerStates["grounded"]; 
   }

   #region Grounded Substates
   public PlayerBaseState Idle()
   {
      return _playerStates["idle"];
   }
   
   public PlayerBaseState Walk()
   {
      return _playerStates["walk"]; 
   }

   public PlayerBaseState Run()
   {
      return _playerStates["run"]; 
   }
  
   public  PlayerBaseState Slide()
   {
      return _playerStates["slide"]; 
   }

   public  PlayerBaseState Interact()
   {
      return _playerStates["interact"]; 
   }

   public  PlayerBaseState Landing()
   {
      return _playerStates["landing"]; 
   }

   public  PlayerBaseState HardLanding()
   {
      return _playerStates["hardlanding"]; 
   }
   #endregion
   
   public PlayerBaseState Airborne()
   {
      return _playerStates["airborne"];
   }
   #region Airborne Substates
   public PlayerBaseState Jump()
   {
      return _playerStates["jump"];
   }
   public PlayerBaseState WallJump()
   {
      return _playerStates["walljump"];
   }
   public PlayerBaseState Fall()
   {
      return _playerStates["fall"];
   }
   #endregion

   public PlayerBaseState Wall()
   {
      return _playerStates["wall"];
   }
   #region Wall Substates
   public PlayerBaseState Ladder()
   {
      return _playerStates["ladder"];
   }
   public PlayerBaseState WallSlide()
   {
      return _playerStates["wallslide"];
   }
   public PlayerBaseState Hang()
   {
      return _playerStates["hang"];
   }
   public PlayerBaseState Mantle()
   {
      return _playerStates["mantle"];
   }
   
   #endregion
   #endregion
}