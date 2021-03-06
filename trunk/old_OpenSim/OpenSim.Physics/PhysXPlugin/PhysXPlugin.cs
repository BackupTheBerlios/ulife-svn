/*
* Copyright (c) OpenSim project, http://sim.opensecondlife.org/
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the <organization> nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY <copyright holder> ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
* 
*/
/*
* Copyright (c) OpenSim project, http://sim.opensecondlife.org/
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the <organization> nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY <copyright holder> ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
* 
*/
using System;
using System.Collections.Generic;
using OpenSim.Physics.Manager;
using PhysXWrapper;

namespace OpenSim.Physics.PhysXPlugin
{
	/// <summary>
	/// Will be the PhysX plugin but for now will be a very basic physics engine
	/// </summary>
	public class PhysXPlugin : IPhysicsPlugin
	{
		private PhysXScene _mScene;
		
		public PhysXPlugin()
		{
			
		}
		
		public bool Init()
		{
			return true;
		}
		
		public PhysicsScene GetScene()
		{
			if(_mScene == null)
			{
				_mScene = new PhysXScene();
			}
			return(_mScene);
		}
		
		public string GetName()
		{
			return("RealPhysX");
		}
		
		public void Dispose()
		{
			
		}
	}
	
	public class PhysXScene :PhysicsScene
	{
		private List<PhysXCharacter> _characters = new List<PhysXCharacter>();
		private List<PhysXPrim> _prims = new List<PhysXPrim>();
		private float[] _heightMap = null;
		private NxPhysicsSDK mySdk;
		private NxScene scene;
		
		public PhysXScene()
		{
			mySdk = NxPhysicsSDK.CreateSDK();
			Console.WriteLine("Sdk created - now creating scene");
			scene = mySdk.CreateScene();
		
		}
		
		public override PhysicsActor AddAvatar(PhysicsVector position)
		{
			Vec3 pos = new Vec3();
			pos.X = position.X;
			pos.Y = position.Y;
			pos.Z = position.Z;
			PhysXCharacter act = new PhysXCharacter( scene.AddCharacter(pos));
			act.Position = position;
			_characters.Add(act);
			return act;
		}
		
		public override PhysicsActor AddPrim(PhysicsVector position, PhysicsVector size)
		{
			Vec3 pos = new Vec3();
			pos.X = position.X;
			pos.Y = position.Y;
			pos.Z = position.Z;
			Vec3 siz = new Vec3();
			siz.X = size.X;
			siz.Y = size.Y;
			siz.Z = size.Z;
			PhysXPrim act = new PhysXPrim( scene.AddNewBox(pos, siz));
			_prims.Add(act);
			return act;
		}
		public override void Simulate(float timeStep)
		{
            try
            {
                foreach (PhysXCharacter actor in _characters)
                {
                    actor.Move(timeStep);
                }
                scene.Simulate(timeStep);
                scene.FetchResults();
                scene.UpdateControllers();

                foreach (PhysXCharacter actor in _characters)
                {
                    actor.UpdatePosition();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
				
		}
		
		public override void GetResults()
		{
		
		}
		
		public override bool IsThreaded
		{
			get
			{
				return(false); // for now we won't be multithreaded
			}
		}
		
		public override void SetTerrain(float[] heightMap)
		{
            if (this._heightMap != null)
            {
                Console.WriteLine("PhysX - deleting old terrain");
                this.scene.DeleteTerrain();
            }
			this._heightMap = heightMap;
			this.scene.AddTerrain(heightMap);
		}

        public override void DeleteTerrain()
        {
            this.scene.DeleteTerrain();
        }	
    }
	
	public  class PhysXCharacter : PhysicsActor
	{
		private PhysicsVector _position;
		private PhysicsVector _velocity;
		private PhysicsVector _acceleration;
		private NxCharacter _character;
		private bool flying;
		private float gravityAccel;
		
		public PhysXCharacter(NxCharacter character)
		{
			_velocity = new PhysicsVector();
			_position = new PhysicsVector();
			_acceleration = new PhysicsVector();
			_character = character;
		}
		
		public override bool Flying
		{
			get
			{
				return flying;
			}
			set
			{
				flying = value;
			}
		}
		
		public override PhysicsVector Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
                Vec3 ps = new Vec3();
                ps.X = value.X;
                ps.Y = value.Y;
                ps.Z = value.Z;
                this._character.Position = ps;
			}
		}
		
		public override PhysicsVector Velocity
		{
			get
			{
				return _velocity;
			}
			set
			{
				_velocity = value;
			}
		}
		
		public override bool Kinematic
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}
		
		public override Axiom.MathLib.Quaternion Orientation
		{
			get
			{
				return Axiom.MathLib.Quaternion.Identity;
			}
			set
			{
				
			}
		}
		
		public override PhysicsVector Acceleration
		{
			get
			{
				return _acceleration;
			}
			
		}
		public void SetAcceleration (PhysicsVector accel)
		{
			this._acceleration = accel;
		}
		
		public override void AddForce(PhysicsVector force)
		{
			
		}
		
		public override void SetMomentum(PhysicsVector momentum)
		{
			
		}
		
		public void Move(float timeStep)
		{
			Vec3 vec = new Vec3();
			vec.X = this._velocity.X * timeStep;
			vec.Y = this._velocity.Y * timeStep;
			if(flying)
			{
				vec.Z = ( this._velocity.Z) * timeStep;
			}
			else
			{
				gravityAccel+= -9.8f;
				vec.Z = (gravityAccel + this._velocity.Z) * timeStep;
			}
			int res = this._character.Move(vec);
			if(res == 1)
			{
				gravityAccel = 0;
			}
		}
		
		public void UpdatePosition()
		{
			Vec3 vec = this._character.Position;
			this._position.X = vec.X;
			this._position.Y = vec.Y;
			this._position.Z = vec.Z;
		}
	}
	
	public  class PhysXPrim : PhysicsActor
	{
		private PhysicsVector _position;
		private PhysicsVector _velocity;
		private PhysicsVector _acceleration;
		private NxActor _prim;
		
		public PhysXPrim(NxActor prim)
		{
			_velocity = new PhysicsVector();
			_position = new PhysicsVector();
			_acceleration = new PhysicsVector();
			_prim = prim;
		}
		public override bool Flying
		{
			get
			{
				return false; //no flying prims for you
			}
			set
			{
				
			}
		}
		public override PhysicsVector Position
		{
			get
			{
				PhysicsVector pos = new PhysicsVector();
				Vec3 vec = this._prim.Position;
				pos.X = vec.X;
				pos.Y = vec.Y;
				pos.Z = vec.Z;
				return pos;
				
			}
			set
			{
				PhysicsVector vec = value;
				Vec3 pos = new Vec3();
				pos.X = vec.X;
				pos.Y = vec.Y;
				pos.Z = vec.Z;
				this._prim.Position = pos;
			}
		}
		
		public override PhysicsVector Velocity
		{
			get
			{
				return _velocity;
			}
			set
			{
				_velocity = value;
			}
		}
		
		public override bool Kinematic
		{
			get
			{
				return this._prim.Kinematic;
			}
			set
			{
				this._prim.Kinematic = value;
			}
		}
		
		public override Axiom.MathLib.Quaternion Orientation
		{
			get
			{
				Axiom.MathLib.Quaternion res = new Axiom.MathLib.Quaternion();
				PhysXWrapper.Quaternion quat = this._prim.GetOrientation();
				res.w = quat.W;
				res.x = quat.X;
				res.y = quat.Y;
				res.z = quat.Z;
				return res;
			}
			set
			{
				
			}
		}
		
		public override PhysicsVector Acceleration
		{
			get
			{
				return _acceleration;
			}
			
		}
		public void SetAcceleration (PhysicsVector accel)
		{
			this._acceleration = accel;
		}
		
		public override void AddForce(PhysicsVector force)
		{
			
		}
		
		public override void SetMomentum(PhysicsVector momentum)
		{
			
		}
		
		
	}

}
