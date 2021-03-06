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
using System.Text;

namespace OpenSim.Physics.Manager
{
    public abstract class PhysicsActor
    {
        public static PhysicsActor Null
        {
            get
            {
                return new NullPhysicsActor();
            }
        }

        public abstract PhysicsVector Position
        {
            get;
            set;
        }

        public abstract PhysicsVector Velocity
        {
            get;
            set;
        }

        public abstract PhysicsVector Acceleration
        {
            get;
        }

        public abstract Axiom.MathLib.Quaternion Orientation
        {
            get;
            set;
        }

        public abstract bool Flying
        {
            get;
            set;
        }

        public abstract bool Kinematic
        {
            get;
            set;
        }

        public abstract void AddForce(PhysicsVector force);

        public abstract void SetMomentum(PhysicsVector momentum);
    }

    public class NullPhysicsActor : PhysicsActor
    {
        public override PhysicsVector Position
        {
            get
            {
                return PhysicsVector.Zero;
            }
            set
            {
                return;
            }
        }

        public override PhysicsVector Velocity
        {
            get
            {
                return PhysicsVector.Zero;
            }
            set
            {
                return;
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
            get { return PhysicsVector.Zero; }
        }

        public override bool Flying
        {
            get
            {
                return false;
            }
            set
            {
                return;
            }
        }

        public override bool Kinematic
        {
            get
            {
                return true;
            }
            set
            {
                return;
            }
        }

        public override void AddForce(PhysicsVector force)
        {
            return;
        }

        public override void SetMomentum(PhysicsVector momentum)
        {
            return;
        }
    }
}
