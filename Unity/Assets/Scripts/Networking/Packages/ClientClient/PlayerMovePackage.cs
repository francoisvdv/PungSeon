using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovePackage : DataPackage
{
    class PlayerMoveFactory : DataPackageFactory
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
            if (split.Length != 7)
                throw new Exception("Invalid package");

            Vector3 pos = Vector3.zero;
            float.TryParse(split[0], out pos.x);
            float.TryParse(split[1], out pos.y);
            float.TryParse(split[2], out pos.z);

            Vector3 rot = Vector3.zero;
            float.TryParse(split[3], out rot.x);
            float.TryParse(split[4], out rot.y);
            float.TryParse(split[5], out rot.z);

            int dirInt;
            int.TryParse(split[6], out dirInt);
            Direction dir = (Direction)dirInt;
            
            return new PlayerMovePackage(pos, rot, dir);
        }
    }
    static PlayerMoveFactory factory = new PlayerMoveFactory();
    public static void RegisterFactory()
    {
        DataPackageFactory.Factories.Add(factory);
    }

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
            string result = Position.x.ToString() + "|" + Position.y.ToString() + "|" + Position.z.ToString() + "|";
            result += Rotation.x.ToString() + "|" + Rotation.y.ToString() + "|" + Rotation.z.ToString() + "|";
            result += (int)Dir;
            return result;
        }
    }
    public override DataPackageFactory Factory
    {
        get { return factory; }
    }

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Direction Dir { get; set; }

    public PlayerMovePackage(Vector3 pos, Vector3 rot, Direction dir)
    {
        Position = pos;
        Rotation = rot;
        Dir = dir;
    }
}