// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace VFXEditor.Flatbuffer.Ephb
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct EphbZeta : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_24_3_25(); }
  public static EphbZeta GetRootAsEphbZeta(ByteBuffer _bb) { return GetRootAsEphbZeta(_bb, new EphbZeta()); }
  public static EphbZeta GetRootAsEphbZeta(ByteBuffer _bb, EphbZeta obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public EphbZeta __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public VFXEditor.Flatbuffer.Ephb.EphbUnknownT? Unknown1 { get { int o = __p.__offset(4); return o != 0 ? (VFXEditor.Flatbuffer.Ephb.EphbUnknownT?)(new VFXEditor.Flatbuffer.Ephb.EphbUnknownT()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
  public float Unknown2 { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; } }

  public static Offset<VFXEditor.Flatbuffer.Ephb.EphbZeta> CreateEphbZeta(FlatBufferBuilder builder,
      Offset<VFXEditor.Flatbuffer.Ephb.EphbUnknownT> unknown1Offset = default(Offset<VFXEditor.Flatbuffer.Ephb.EphbUnknownT>),
      float unknown2 = 0.0f) {
    builder.StartTable(2);
    EphbZeta.AddUnknown2(builder, unknown2);
    EphbZeta.AddUnknown1(builder, unknown1Offset);
    return EphbZeta.EndEphbZeta(builder);
  }

  public static void StartEphbZeta(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddUnknown1(FlatBufferBuilder builder, Offset<VFXEditor.Flatbuffer.Ephb.EphbUnknownT> unknown1Offset) { builder.AddOffset(0, unknown1Offset.Value, 0); }
  public static void AddUnknown2(FlatBufferBuilder builder, float unknown2) { builder.AddFloat(1, unknown2, 0.0f); }
  public static Offset<VFXEditor.Flatbuffer.Ephb.EphbZeta> EndEphbZeta(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<VFXEditor.Flatbuffer.Ephb.EphbZeta>(o);
  }
}


static public class EphbZetaVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyTable(tablePos, 4 /*Unknown1*/, VFXEditor.Flatbuffer.Ephb.EphbUnknownTVerify.Verify, false)
      && verifier.VerifyField(tablePos, 6 /*Unknown2*/, 4 /*float*/, 4, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}