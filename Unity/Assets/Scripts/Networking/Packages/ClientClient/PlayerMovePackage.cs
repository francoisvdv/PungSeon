using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovePackage : DataPackage
{
    public class PlayerMoveFactory : DataPackageFactory
    {
        public override int Id
        {
            get
            {
                return 1;
            }
        }

        public override DataPackage CreateFromBody(string b)
        {
            string[] split = b.Split('|');

            bool rotOnly = false;
            bool.TryParse(split[0], out rotOnly);

            if (!rotOnly)
            {
                Vector3 pos = Vector3.zero;
                float.TryParse(split[1], out pos.x);
                float.TryParse(split[2], out pos.y);
                float.TryParse(split[3], out pos.z);

                Vector3 rot = Vector3.zero;
                float.TryParse(split[4], out rot.x);
                float.TryParse(split[5], out rot.y);
                float.TryParse(split[6], out rot.z);

                int dirInt;
                int.TryParse(split[7], out dirInt);
                Direction dir = (Direction)dirInt;

                return new PlayerMovePackage(pos, rot, dir);
            }
            else
            {
                Vector3 rot = Vector3.zero;
                float.TryParse(split[1], out rot.x);
                float.TryParse(split[2], out rot.y);
                float.TryParse(split[3], out rot.z);
                return new PlayerMovePackage(rot);
            }
        }
    }
    public static PlayerMoveFactory factory = new PlayerMoveFactory();

    [Flags]
    public enum Direction { Stop, Up, Left, Back, Right }

    public override int Id
    {
        get { return factory.Id; }
    }
    public override string Body
    {
        get
        {
            string result = RotationOnly.ToString() + "|";

            if (!RotationOnly)
            {
                result += Position.x.ToString() + "|" + Position.y.ToString() + "|" + Position.z.ToString() + "|";
                result += Rotation.x.ToString() + "|" + Rotation.y.ToString() + "|" + Rotation.z.ToString() + "|";
                result += (int)Dir;
            }
            else
                result += Rotation.x.ToString() + "|" + Rotation.y.ToString() + "|" + Rotation.z.ToString();

            return result;
        }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public bool RotationOnly { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Direction Dir { get; set; }

    public PlayerMovePackage(Vector3 rot)
    {
        RotationOnly = true;
        Rotation = rot;
    }
    public PlayerMovePackage(Vector3 pos, Vector3 rot, Direction dir)
    {
        RotationOnly = false;
        Position = pos;
        Rotation = rot;
        Dir = dir;
    }
}