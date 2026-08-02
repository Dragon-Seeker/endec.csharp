using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using io.wispforest.endec.util;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace io.wispforest.endec.format.edm;

public class EdmIo {

    public static byte[] encode(EdmElement data) {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        encode(writer, data);
        return ms.ToArray();
    }

    public static void encode(BinaryWriter output, EdmElement data) {
        output.Write((byte) data.type);
        encodeElementData(output, data);

    }

    public static EdmElement decode(byte[] input){
        using var ms = new MemoryStream(input);
        using var reader = new BinaryReader(ms);
        return decodeElementData(reader, reader.ReadByte());
    }

    public static void encodeElementData(BinaryWriter output, EdmElement data) {
        switch (data.type) {
            case EdmElementType.I8: output.Write(data.cast<sbyte>()); break;
            case EdmElementType.U8: output.Write(data.cast<byte>()); break;
            case EdmElementType.I16: output.Write(data.cast<short>()); break;
            case EdmElementType.U16: output.Write(data.cast<ushort>()); break;
            case EdmElementType.I32: output.Write(data.cast<int>()); break;
            case EdmElementType.U32: output.Write(data.cast<uint>()); break;
            case EdmElementType.I64: output.Write(data.cast<long>()); break;
            case EdmElementType.U64: output.Write(data.cast<ulong>()); break;
            case EdmElementType.F32: output.Write(data.cast<float>()); break;
            case EdmElementType.F64: output.Write(data.cast<double>()); break;
            case EdmElementType.BOOLEAN: output.Write(data.cast<bool>()); break;
            case EdmElementType.STRING: output.Write(data.cast<string>()); break;
            case EdmElementType.BYTES:
                var bytes = data.cast<byte[]>();
                VarInts.writeInt(bytes.Length, output.Write);
                output.Write(bytes);
                break;

            case EdmElementType.OPTIONAL:
                var element = data.cast<EdmElement?>();

                output.Write(element != null);
                if (element != null) {
                    output.Write((byte) element.type);
                    encodeElementData(output, element);
                }

                break;
            case EdmElementType.SEQUENCE:
                var list = data.cast<IList<EdmElement>>();

                VarInts.writeInt(list.Count, output.Write);
                if (list.Count > 0) {
                    foreach (var entry in list) {
                        output.Write((byte) entry.type);
                        encodeElementData(output, entry);
                    }
                }

                break;
            case EdmElementType.MAP: {
                var map = data.cast<Dictionary<string, EdmElement>>();

                VarInts.writeInt(map.Count, output.Write);
                foreach (var entry in map.ToList()) {
                    output.Write(entry.Key);

                    output.Write((byte) entry.Value.type);
                    encodeElementData(output, entry.Value);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException($"{nameof(data.type)} type can not be handled in EdmIo.encodeElementData method");
        }
    }

    public static EdmElement decode(BinaryReader input){
        return decodeElementData(input, input.ReadByte());
    }

    private static EdmElement decodeElementData(BinaryReader input, byte type) {
        switch ((EdmElementType) type) {
            case EdmElementType.I8: return EdmElements.i8(input.ReadSByte());
            case EdmElementType.U8: return EdmElements.u8(input.ReadByte());
            case EdmElementType.I16: return EdmElements.i16(input.ReadInt16());
            case EdmElementType.U16: return EdmElements.u16(input.ReadUInt16());
            case EdmElementType.I32: return EdmElements.i32(input.ReadInt32());
            case EdmElementType.U32: return EdmElements.u32(input.ReadUInt32());
            case EdmElementType.I64: return EdmElements.i64(input.ReadInt64());
            case EdmElementType.U64: return EdmElements.u64(input.ReadUInt64());
            case EdmElementType.F32: return EdmElements.f32(input.ReadSingle());
            case EdmElementType.F64: return EdmElements.f64(input.ReadDouble());
            case EdmElementType.BOOLEAN: return EdmElements.@bool(input.ReadBoolean());
            case EdmElementType.STRING: return EdmElements.@string(input.ReadString());
            case EdmElementType.BYTES: return EdmElements.bytes(input.ReadBytes(VarInts.readInt(input.ReadByte)));
            case EdmElementType.OPTIONAL: return EdmElements.optional(input.ReadByte() != 0 ? decodeElementData(input, input.ReadByte()) : null);
            case EdmElementType.SEQUENCE: {
                var length = VarInts.readInt(input.ReadByte);
                var result = length <= 0 ? new List<EdmElement>(length) : new List<EdmElement>();

                if (length > 0) {
                    for (int i = 0; i < length; i++) {
                        result.Add(decodeElementData(input, input.ReadByte()));
                    }
                }

                return EdmElements.sequence(result);
            }
            case EdmElementType.MAP: {
                var length = VarInts.readInt(input.ReadByte);
                var result = new Dictionary<string, EdmElement>(length);

                for (var i = 0; i < length; i++) {
                    result.Add(
                            input.ReadString(),
                            decodeElementData(input, input.ReadByte())
                    );
                }

                return EdmElements.map(result);
            }
            default: throw new ArgumentOutOfRangeException(nameof(EdmElementType), type, "type can not be handled in EdmIo.decodeElementData method");
        }
    }
}