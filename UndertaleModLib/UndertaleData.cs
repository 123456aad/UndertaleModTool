﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UndertaleModLib.Models;

namespace UndertaleModLib
{
    public class UndertaleData
    {
        public UndertaleChunkFORM FORM;

        public UndertaleGeneralInfo GeneralInfo => FORM.GEN8.Object;
        public UndertaleOptions Options => FORM.OPTN.Object;
        public UndertaleLanguage Language => FORM.LANG.Object;
        public IList<UndertaleExtension> Extensions => FORM.EXTN.List;
        public IList<UndertaleSound> Sounds => FORM.SOND.List;
        [Obsolete("Unused")]
        public IList<UndertaleAudioGroup> AudioGroups => FORM.AGRP.List;
        public IList<UndertaleSprite> Sprites => FORM.SPRT.List;
        public IList<UndertaleBackground> Backgrounds => FORM.BGND.List;
        public IList<UndertalePath> Paths => FORM.PATH.List;
        public IList<UndertaleScript> Scripts => FORM.SCPT.List;
        public IList<UndertaleGlobalInit> GlobalInitScripts => FORM.GLOB.List;
        [Obsolete("Unused")]
        public IList<UndertaleShader> Shaders => FORM.SHDR.List;
        public IList<UndertaleFont> Fonts => FORM.FONT.List;
        [Obsolete("Unused")]
        public IList<UndertaleTimeline> Timelines => FORM.TMLN.List;
        public IList<UndertaleGameObject> GameObjects => FORM.OBJT.List;
        public IList<UndertaleRoom> Rooms => FORM.ROOM.List;
        //[Obsolete("Unused")]
        // DataFile
        public IList<UndertaleTexturePageItem> TexturePageItems => FORM.TPAG.List;
        public IList<UndertaleCode> Code => FORM.CODE.List;
        public IList<UndertaleVariable> Variables => FORM.VARI.List;
        public uint Variables_Unknown1 { get => FORM.VARI.Unknown1; set => FORM.VARI.Unknown1 = value; }
        public uint Variables_Unknown1Again { get => FORM.VARI.Unknown1Again; set => FORM.VARI.Unknown1Again = value; }
        public uint Variables_Unknown2 { get => FORM.VARI.Unknown2; set => FORM.VARI.Unknown2 = value; }
        public IList<UndertaleFunction> Functions => FORM.FUNC.Functions;
        public IList<UndertaleCodeLocals> CodeLocals => FORM.FUNC.CodeLocals;
        public IList<UndertaleString> Strings => FORM.STRG.List;
        public IList<UndertaleEmbeddedTexture> EmbeddedTextures => FORM.TXTR.List;
        public IList<UndertaleEmbeddedAudio> EmbeddedAudio => FORM.AUDO.List;

        public static UndertaleData CreateNew()
        {
            UndertaleData data = new UndertaleData();
            data.FORM = new UndertaleChunkFORM();
            data.FORM.Chunks["GEN8"] = new UndertaleChunkGEN8();
            data.FORM.Chunks["OPTN"] = new UndertaleChunkOPTN();
            data.FORM.Chunks["LANG"] = new UndertaleChunkLANG();
            data.FORM.Chunks["EXTN"] = new UndertaleChunkEXTN();
            data.FORM.Chunks["SOND"] = new UndertaleChunkSOND();
            data.FORM.Chunks["AGRP"] = new UndertaleChunkAGRP();
            data.FORM.Chunks["SPRT"] = new UndertaleChunkSPRT();
            data.FORM.Chunks["BGND"] = new UndertaleChunkBGND();
            data.FORM.Chunks["PATH"] = new UndertaleChunkPATH();
            data.FORM.Chunks["SCPT"] = new UndertaleChunkSCPT();
            data.FORM.Chunks["GLOB"] = new UndertaleChunkGLOB();
            data.FORM.Chunks["SHDR"] = new UndertaleChunkSHDR();
            data.FORM.Chunks["FONT"] = new UndertaleChunkFONT();
            data.FORM.Chunks["TMLN"] = new UndertaleChunkTMLN();
            data.FORM.Chunks["OBJT"] = new UndertaleChunkOBJT();
            data.FORM.Chunks["ROOM"] = new UndertaleChunkROOM();
            data.FORM.Chunks["DAFL"] = new UndertaleChunkDAFL();
            data.FORM.Chunks["TPAG"] = new UndertaleChunkTPAG();
            data.FORM.Chunks["CODE"] = new UndertaleChunkCODE();
            data.FORM.Chunks["VARI"] = new UndertaleChunkVARI();
            data.FORM.Chunks["FUNC"] = new UndertaleChunkFUNC();
            data.FORM.Chunks["STRG"] = new UndertaleChunkSTRG();
            data.FORM.Chunks["TXTR"] = new UndertaleChunkTXTR();
            data.FORM.Chunks["AUDO"] = new UndertaleChunkAUDO();
            data.FORM.GEN8.Object = new UndertaleGeneralInfo();
            data.FORM.OPTN.Object = new UndertaleOptions();
            data.FORM.LANG.Object = new UndertaleLanguage();
            data.GeneralInfo.Filename = data.Strings.MakeString("NewGame");
            data.GeneralInfo.Config = data.Strings.MakeString("Default");
            data.GeneralInfo.Name = data.Strings.MakeString("NewGame");
            data.GeneralInfo.DisplayName = data.Strings.MakeString("New UndertaleModTool Game");
            data.GeneralInfo.GameID = (uint)new Random().Next();
            data.GeneralInfo.Timestamp = (uint)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            data.Options.Constants.Add(new UndertaleOptions.Constant() { Name = data.Strings.MakeString("@@SleepMargin"), Value = data.Strings.MakeString(1.ToString()) });
            data.Options.Constants.Add(new UndertaleOptions.Constant() { Name = data.Strings.MakeString("@@DrawColour"), Value = data.Strings.MakeString(0xFFFFFFFF.ToString()) });
            data.Rooms.Add(new UndertaleRoom() { Name = data.Strings.MakeString("room0"), Caption = data.Strings.MakeString("") });
            return data;
        }
    }

    public static class UndertaleDataExtensionMethods
    {
        public static T ByName<T>(this IList<T> list, string name) where T : UndertaleNamedResource
        {
            foreach(var item in list)
            {
                if (item.Name.Content == name)
                    return item;
            }
            return default(T);
        }

        public static UndertaleString MakeString(this IList<UndertaleString> list, string content)
        {
            // TODO: without reference counting the strings, this may leave unused strings in the array
            foreach (UndertaleString str in list)
            {
                if (str.Content == content)
                    return str;
            }
            UndertaleString newString = new UndertaleString(content);
            list.Add(newString);
            return newString;
        }

        public static UndertaleFunction EnsureDefined(this IList<UndertaleFunction> list, string name, IList<UndertaleString> strg)
        {
            UndertaleFunction func = list.ByName(name);
            if (func == null)
            {
                func = new UndertaleFunction()
                {
                    Name = strg.MakeString(name),
                    UnknownChainEndingValue = 0 // TODO: seems to work...
                };
                list.Add(func);
            }
            return func;
        }
    }
}
