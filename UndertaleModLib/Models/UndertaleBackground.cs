﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UndertaleModLib.Models
{
    public class UndertaleBackground : UndertaleNamedResource, INotifyPropertyChanged
    {
        private UndertaleString _Name;
        private uint _Unknown1;
        private uint _Unknown2;
        private uint _Unknown3;
        private UndertaleTexturePageItem _Texture;

        public UndertaleString Name { get => _Name; set { _Name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name")); } }
        public uint Unknown1 { get => _Unknown1; set { _Unknown1 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Unknown1")); } }
        public uint Unknown2 { get => _Unknown2; set { _Unknown2 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Unknown2")); } }
        public uint Unknown3 { get => _Unknown3; set { _Unknown3 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Unknown3")); } }
        public UndertaleTexturePageItem Texture { get => _Texture; set { _Texture = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Texture")); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Serialize(UndertaleWriter writer)
        {
            writer.WriteUndertaleString(Name);
            writer.Write(Unknown1);
            writer.Write(Unknown2);
            writer.Write(Unknown3);
            writer.WriteUndertaleObjectPointer(Texture);
            if (writer.undertaleData.GeneralInfo.Major >= 2)
            {
                throw new NotImplementedException();
            }
        }

        public void Unserialize(UndertaleReader reader)
        {
            Name = reader.ReadUndertaleString();
            Unknown1 = reader.ReadUInt32();
            Unknown2 = reader.ReadUInt32();
            Unknown3 = reader.ReadUInt32();
            Texture = reader.ReadUndertaleObjectPointer<UndertaleTexturePageItem>();
            if (reader.undertaleData.GeneralInfo.Major >= 2)
            {
                reader.ReadUInt32(); // 2/2
                reader.ReadUInt32(); // TileWidth = 32/64
                reader.ReadUInt32(); // TileHeight = 32/64
                reader.ReadUInt32(); // OutputBorderX = 2/2
                reader.ReadUInt32(); // OutputBorderY = 2/2
                reader.ReadUInt32(); // 32/23
                uint ItemsPerTileCount = reader.ReadUInt32(); // 1/32
                uint TileCount = reader.ReadUInt32(); // 1024/1024 = 32*32? = TileCount?
                reader.ReadUInt32(); // 0
                reader.ReadUInt32(); // 66666
                reader.ReadUInt32(); // 0
                for (int i = 0; i < TileCount*ItemsPerTileCount; i++)
                    reader.ReadUInt32(); // TileId?
            }
        }

        public override string ToString()
        {
            return Name.Content + " (" + GetType().Name + ")";
        }
    }
}
