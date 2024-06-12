// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace VFXEditor.Flatbuffer.Ephb
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct EphbEta : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_24_3_25(); }
  public static EphbEta GetRootAsEphbEta(ByteBuffer _bb) { return GetRootAsEphbEta(_bb, new EphbEta()); }
  public static EphbEta GetRootAsEphbEta(ByteBuffer _bb, EphbEta obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public EphbEta __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public ushort Unknown1 { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetUshort(o + __p.bb_pos) : (ushort)0; } }
  public ushort Unknown2 { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetUshort(o + __p.bb_pos) : (ushort)0; } }
  public ushort Unknown3 { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetUshort(o + __p.bb_pos) : (ushort)0; } }
  public ushort Unknown4 { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetUshort(o + __p.bb_pos) : (ushort)0; } }

  public static Offset<VFXEditor.Flatbuffer.Ephb.EphbEta> CreateEphbEta(FlatBufferBuilder builder,
      ushort unknown1 = 0,
      ushort unknown2 = 0,
      ushort unknown3 = 0,
      ushort unknown4 = 0) {
    builder.StartTable(4);
    EphbEta.AddUnknown4(builder, unknown4);
    EphbEta.AddUnknown3(builder, unknown3);
    EphbEta.AddUnknown2(builder, unknown2);
    EphbEta.AddUnknown1(builder, unknown1);
    return EphbEta.EndEphbEta(builder);
  }

  public static void StartEphbEta(FlatBufferBuilder builder) { builder.StartTable(4); }
  public static void AddUnknown1(FlatBufferBuilder builder, ushort unknown1) { builder.AddUshort(0, unknown1, 0); }
  public static void AddUnknown2(FlatBufferBuilder builder, ushort unknown2) { builder.AddUshort(1, unknown2, 0); }
  public static void AddUnknown3(FlatBufferBuilder builder, ushort unknown3) { builder.AddUshort(2, unknown3, 0); }
  public static void AddUnknown4(FlatBufferBuilder builder, ushort unknown4) { builder.AddUshort(3, unknown4, 0); }
  public static Offset<VFXEditor.Flatbuffer.Ephb.EphbEta> EndEphbEta(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<VFXEditor.Flatbuffer.Ephb.EphbEta>(o);
  }
}


static public class EphbEtaVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Unknown1*/, 2 /*ushort*/, 2, false)
      && verifier.VerifyField(tablePos, 6 /*Unknown2*/, 2 /*ushort*/, 2, false)
      && verifier.VerifyField(tablePos, 8 /*Unknown3*/, 2 /*ushort*/, 2, false)
      && verifier.VerifyField(tablePos, 10 /*Unknown4*/, 2 /*ushort*/, 2, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}